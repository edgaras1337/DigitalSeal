using DigitalSeal.Core.Exceptions;
using DigitalSeal.Core.Extensions;
using DigitalSeal.Core.Models.Config;
using DigitalSeal.Core.Models.Config.Email;
using DigitalSeal.Core.Models.Document;
using DigitalSeal.Core.Models.Notifications;
using DigitalSeal.Core.Utilities;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace DigitalSeal.Core.Services
{
    internal class DocService : IDocService
    {
        private readonly IDocRepository _docRepository;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IEmailService _emailService;
        private readonly IMessageCreator _notificationService;
        private readonly ICurrentUserProvider _currUserProvider;
        private readonly ISignatureService _signatureService;
        private readonly IStringLocalizer<DocService> _localizer;
        private readonly DocumentOptions _docOptions;

        public DocService(
            IDocRepository docRepository,
            IUserNotificationRepository userNotificationRepository,
            IEmailService emailService, 
            IMessageCreator notificationService, 
            ICurrentUserProvider currUserProvider, 
            ISignatureService signatureService, 
            IStringLocalizer<DocService> localizer,
            IOptions<DocumentOptions> docOptions)
        {
            _docRepository = docRepository;
            _userNotificationRepository = userNotificationRepository;
            _emailService = emailService;
            _notificationService = notificationService;
            _currUserProvider = currUserProvider;
            _signatureService = signatureService;
            _localizer = localizer;
            _docOptions = docOptions.Value;
        }

        public async Task<Result<bool>> CreateDocumentAsync(CreateDocumentModel model)
        {
            IFormFile formFile = model.FormFile;

            byte[] content = ReadFileContent(formFile);
            if (!PdfHelper.IsPdfFile(content))
            {
                return new(ValidationException.Warning(_localizer["OnlyPdf"]));
            }

            int sizeMB = _docOptions.MaxFileSizeMB;
            if (content.Length > _docOptions.MaxFileSizeMB * 1024 * 1024)
            {
                return new(ValidationException.Warning(_localizer["SizeExceeded", sizeMB]));
            }

            Document docEntity = await CreateDocumentEntityAsync(formFile, content);
            await _docRepository.AddAsync(docEntity);
            await _docRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Result<bool>> DeleteAsync(int[] docIds)
        {
            List<Document> documents = await _docRepository.GetByIdsAsync(docIds, includeRelatedData: true);
            if (!await IsAuthorAsync(documents))
            {
                return new(ValidationException.Error(_localizer["DeleteNonPersonal"]));
            }

            _docRepository.RemoveRange(documents);
            await _docRepository.SaveChangesAsync();

            return true;
        }

        public Task<Document?> GetByIdAsync(int docId, bool includeRelatedData = false, bool includeFileContent = false)
        {
            var ownerId = new UserId(_currUserProvider.CurrentUserId);
            return _docRepository.GetAsync(docId, ownerId, includeRelatedData, includeFileContent);
        }

        public async Task<byte[]?> GetContentAsync(int docId)
        {
            Document? doc = await GetByIdAsync(docId, includeFileContent: true);
            return doc?.FileContent.Content;
        }

        public async Task<Result<string>> GetStatusAsync(int docId)
        {
            Document? document = await GetByIdAsync(docId, includeFileContent: true);
            if (document == null)
            {
                return new(ValidationException.Error(_localizer["DocNotFound"]));
            }

            SignStatus status = _signatureService.GetDocumentStatus(document);
            return status.ToString();
        }

        public async Task<Result<UpdateDocumentResult>> UpdateAsync(UpdateDocumentModel model)
        {
            Document? document = await GetByIdAsync(model.DocId, includeRelatedData: true);
            if (document == null)
            {
                return new(ValidationException.Error(_localizer["DocNotFound"]));
            }

            //DocumentParty author = document.DocumentParties.Single(party => party.IsAuthor);
            //if (author.Party.UserId != _currUserProvider.CurrentUserId)
            //{
            //    return new(ValidationException.Error("You do not have permission to edit this document"));
            //}

            //if (model.Title != null)
            //{
            //    string? title = model.Title?.Trim();
            //    if (string.IsNullOrEmpty(title))
            //    {
            //        return new(ValidationException.Error("Title is required"));
            //    }

            //    document.Name = title;
            //}

            string? name = model.Name;
            if (name != null)
            {
                if (name.Trim() == string.Empty)
                {
                    return new(ValidationException.Error(_localizer["NameNotNull"]));
                }

                document.Name = name;
            }

            if (model.Deadline is DateTime deadline)
            {
                DateTime newDateTime = deadline.ToUniversalTime();
                if (newDateTime != document.Deadline)
                {
                    if (newDateTime <= DateTime.UtcNow)
                    {
                        return new(ValidationException.Error(_localizer["OnlyFutureDates"]));
                    }

                    document.Deadline = newDateTime;

                    await InformDocUpdatedAsync(document);
                }
            }

            await _docRepository.SaveChangesAsync();

            SignStatus status = _signatureService.GetDocumentStatus(document);

            var updateResponse = new UpdateDocumentResult
            {
                Status = status.ToString(),
                IsDeadlinePassed = DateTime.UtcNow >= document.Deadline,
                Name = document.Name,
                Deadline = document.Deadline,
            };
            return updateResponse;
        }


        private static byte[] ReadFileContent(IFormFile file)
        {
            using Stream? fileStream = file.OpenReadStream();
            byte[] content = new byte[file.Length];
            fileStream.Read(content, 0, (int)file.Length);
            return content;
        }

        private async Task<Document> CreateDocumentEntityAsync(IFormFile file, byte[] fileContent)
        {
            string[] nameSplit = file.FileName.Split('.');
            string title = nameSplit[0];
            string extension = nameSplit[^1];

            var docEntity = new Document
            {
                Name = title,
                Extension = extension,
                //CreatedDate = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(5),
            };

            docEntity.FileContent = new FileContent
            {
                DocId = docEntity.Id,
                Content = fileContent
            };

            docEntity.DocumentParties.Add(new DocumentParty
            {
                DocId = docEntity.Id,
                PartyId = await _currUserProvider.GetCurrentPartyIdAsync(),
                Permission = (int)DocPermission.Owner
            });

            return docEntity;
        }

        private async Task InformDocUpdatedAsync(Document document)
        {
            HtmlMessage message = _notificationService.DeadlineChanged(document);
            var partiesEmails = new List<string>(document.DocumentParties.Count);

            Party currParty = await _currUserProvider.GetCurrentPartyAsync();
            foreach (DocumentParty docParty in document.DocumentParties)
            {
                if (docParty.Has(DocPermission.Owner) || docParty.Has(DocPermission.Update))
                {
                    continue;
                }

                Party party = docParty.Party;
                partiesEmails.Add(party.User.Email);

                UserNotification notification = message.AsTextNotification(
                    currParty.OrganizationId, currParty.UserId, party.UserId);

                _userNotificationRepository.Add(notification);
            }

            if (partiesEmails.Count != 0)
            {
                _ = _emailService.SendEmailAsync(new EmailMessage(message.Title, message.Content, true, [.. partiesEmails]));
            }
        }

        private async Task<bool> IsAuthorAsync(IEnumerable<Document> documents)
        {
            int partyId = await _currUserProvider.GetCurrentPartyIdAsync();
            return !documents.Any(doc => doc.DocumentParties.Any(docParty => 
                docParty.PartyId == partyId && !docParty.IsAuthor));
        }
    }
}
