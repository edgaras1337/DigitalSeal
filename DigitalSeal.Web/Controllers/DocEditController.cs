using Microsoft.AspNetCore.Mvc;
using AspNetCoreHero.ToastNotification.Abstractions;
using DigitalSeal.Web.Models.ViewModels.DocEdit;
using LanguageExt.Common;
using DigitalSeal.Core.Models.DocParty;
using DigitalSeal.Core.ListProviders.DocPartyList;
using DigitalSeal.Core.Services;
using DigitalSeal.Core.ListProviders.DocPartyPossibList;
using DigitalSeal.Core.Models.Document;
using DigitalSeal.Core.Utilities;
using DigitalSeal.Data.Models;
using DigitalSeal.Core.ListProviders.Signatures;
using System.Globalization;
using Microsoft.Extensions.Localization;
using DigitalSeal.Web.Extensions;
using DigitalSeal.Web.Models.ViewModels.DocParty;

namespace DigitalSeal.Web.Controllers
{
    [Route("documents/edit")]
    public class DocEditController : BaseDSController
    {
        private readonly ISignatureService _signatureService;
        private readonly IDocService _docService;
        private readonly IDocPartyService _docPartyService;
        private readonly IDocPartyListProvider _docPartyListProvider;
        private readonly IDocPartyPossibListProvider _docPartyPossibListProvider;
        private readonly ISignaturesListProvider _signaturesListProvider;
        private readonly IStringLocalizer<DocEditController> _localizer;
        private readonly ICurrentUserProvider _currentUserProvider;

        public DocEditController(
            INotyfService notyf, 
            ISignatureService signatureService, 
            IDocService docService,
            IDocPartyService docPartyService, 
            IDocPartyListProvider docPartyListProvider, 
            IDocPartyPossibListProvider docPartyPossibListProvider, 
            ISignaturesListProvider signaturesListProvider,
            IStringLocalizer<DocEditController> localizer,
            ICurrentUserProvider currentUserProvider)
            : base(notyf)
        {
            _signatureService = signatureService;
            _docService = docService;
            _docPartyService = docPartyService;
            _docPartyListProvider = docPartyListProvider;
            _docPartyPossibListProvider = docPartyPossibListProvider;
            _signaturesListProvider = signaturesListProvider;
            _localizer = localizer;
            _currentUserProvider = currentUserProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DocEditPageModel model)
        {
            Document? document = await _docService.GetByIdAsync(model.DocId, includeRelatedData: true);
            if (document == null)
            {
                return ErrorResult(RedirectHome(), _localizer["DocNotFound"]);
            }

            DocumentViewModel viewModel = await CreateViewModelAsync(document, model.ReturnUrl);
            return View(viewModel);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateDocument(UpdateDocumentRequest request)
        {
            Result<UpdateDocumentResult> result = await _docService.UpdateAsync(request.ToModel());
            return await MatchResultAsync(result, async successResult =>
            {
                Notyf.Success(_localizer["DocUpdated"]);
                User currUser = await _currentUserProvider.GetCurrentUserAsync();
                var response = new UpdateDocumentResponse
                {
                    Name = successResult.Name,
                    Status = successResult.Status,
                    IsDeadlinePassed = successResult.IsDeadlinePassed,
                    Deadline = DateHelper.ConvertAndFormat(successResult.Deadline, currUser.LastTimeZone),
                };
                return Ok(response);
            }, error => BadRequest());
        }

        [HttpGet("view/{docId}")]
        public async Task<IActionResult> GetDocument(GetDocumentRequest request)
        {
            byte[]? content = await _docService.GetContentAsync(request.DocId);
            return content != null ?
                File(content, HttpClientHelper.MimeTypes.Pdf) :
                ErrorResult(BadRequest(), _localizer["DocNotFound"]);
        }

        [HttpDelete("delete/{docId}")]
        public async Task<IActionResult> DeleteDocument(int docId)
        {
            Result<bool> result = await _docService.DeleteAsync([docId]);
            return MatchResult(result, _localizer["DocDeleted"]);
        }

        [HttpGet("sign-status/{docID}")]
        public async Task<IActionResult> GetSigningStatus(int docId)
            => MatchResult(await _docService.GetStatusAsync(docId));


        [HttpGet("parties/{docId}")]
        public async Task<IActionResult> GetParties(DocPartyListRequest request)
            => Ok(await _docPartyListProvider.CreateListAsync(request));

        [HttpPost("parties/add")]
        public async Task<IActionResult> AddParties(AddDocPartyRequest request)
        {
            Result<bool> result = await _docPartyService.AddAsync(request.ToModel());
            return MatchResult(result, _localizer["DocPartiesAdded"]);
        }

        [HttpDelete("parties/delete")]
        public async Task<IActionResult> RemoveParties(RemoveDocPartyRequest request)
        {
            Result<bool> result = await _docPartyService.RemoveAsync(request.ToModel());
            return MatchResult(result, _localizer["DocPartiesRemoved"]);
        }

        [HttpGet("parties/possible/{docId}")]
        public async Task<IActionResult> GetPossibleParties(DocPartyPossibListRequest request)
            => Ok(await _docPartyPossibListProvider.CreateListAsync(request));

        [HttpGet("signatures/{docId}")]
        public async Task<IActionResult> GetSignatures(SignaturesListRequest request)
            => Ok(await _signaturesListProvider.CreateListAsync(request));

        private async Task<DocumentViewModel> CreateViewModelAsync(Document document, string returnUrl)
        {
            User author = document.DocumentParties.Single(party => party.IsAuthor).Party.User;

            User currUser = await _currentUserProvider.GetCurrentUserAsync();

            SignStatus docStatus = _signatureService.GetDocumentStatus(document);
            var viewModel = new DocumentViewModel
            {
                DocId = document.Id,
                Name = document.Name,
                CreatedDate = DateHelper.ConvertAndFormat(document.Created, currUser.LastTimeZone),
                Deadline = DateHelper.ConvertAndFormat(document.Deadline, currUser.LastTimeZone), //document.Deadline.ToString(),
                Deadline2 = document.Deadline,
                Author = UserHelper.FormatUserName(author),
                IsAuthor = author.Id == CurrentUserId,
                ReturnUrl = returnUrl,
                SigningStatus = docStatus,
                IsDeadlinePassed = DateTime.UtcNow >= document.Deadline,
                PartyListColumnDefs = _docPartyListProvider.CreateColumnDefs(),
                PossiblePartyListColumnDefs = _docPartyPossibListProvider.CreateColumnDefs(),
                SignaturesListColumnDefs = _signaturesListProvider.CreateColumnDefs(),
                FileUrl = Url.Action(
                    nameof(GetDocument), 
                    CurrentControllerName, 
                    new GetDocumentRequest(document.Id)) ?? string.Empty,
            };

            return viewModel;
        }



        //    private Expression<Func<Document, bool>> AnyDocPartyExpression(int docID)
        //        => doc => doc.Id == docID && doc.DocumentParties.Any(docParty => docParty.UserId == CurrentUserID);

        //    private async Task<IActionResult?> EnsureModifyRightAsync(int docID, bool checkDeadline = true)
        //        => EnsureModifyRight(await GetDocumentAsync(docID), checkDeadline);

        //    private IActionResult? EnsureModifyRight(Document? document, bool checkDeadline = true)
        //    {
        //        if (document == null)
        //            return ErrorResult(BadRequest(), "Document not found");

        //        if (!IsAuthor(document))
        //            return ErrorResult(BadRequest(), "Permission denied");

        //        if (checkDeadline && document.Deadline <= DateTime.Now)
        //            return ErrorResult(BadRequest(), "Document deadline was missed");

        //        return null;
        //    }

        //    private SignStatus GetPartySigningStatus(Document document, DocumentParty party)
        //    {
        //        byte[]? content = document.FileContent?.Content;
        //        if (content == null)
        //            return SignStatus.NotSigned;

        //        SignatureVerificationResult? firstValid = GetFirstValidSignature(content, party.UserId);

        //        if (firstValid == null)
        //            if (DateTime.Now < document.Deadline)
        //                return SignStatus.InProgress;
        //            else
        //                return SignStatus.NotSigned;

        //        return IsSignLate(firstValid, document) ? SignStatus.SignedLate : SignStatus.Signed;
        //    }

        //    private SignatureVerificationResult? GetFirstValidSignature(byte[] docContent, int userID)
        //    {
        //        IList<SignatureVerificationResult> signatureResults = _signingService.VerifySignatures(docContent, userID);

        //        SignatureVerificationResult? firstValid = signatureResults
        //            .OrderBy(s => s.SignTime)
        //            .FirstOrDefault(sRes => sRes.Status == SignatureVerificationStatus.VerificationPassed);

        //        return firstValid;
        //    }

        //    private static bool IsSignLate(SignatureVerificationResult signResult, Document document)
        //        => signResult.SignTime > document.Deadline;

        //    //private async Task<Document?> GetDocumentAsync(int docID, bool includeContent = false)
        //    //{
        //    //    var docQuery = _unitOfWork.DocumentRepository.AsQueryable();
        //    //    if (includeContent)
        //    //        docQuery = docQuery.Include(doc => doc.FileContent);

        //    //    return await docQuery.Include(doc => doc.FileContent)
        //    //        .Include(doc => doc.DocumentParties)
        //    //            .ThenInclude(docParty => docParty.User)
        //    //        .Include(doc => doc.DocumentParties)
        //    //            .ThenInclude(docParty => docParty.SignatureInfos)
        //    //        .FirstOrDefaultAsync(AnyDocPartyExpression(docID));
        //    //}

        //    private bool IsAuthor(Document document) =>
        //        document.DocumentParties.Any(party => party.IsAuthor && party.UserId == CurrentUserID);

        //    private Task<DocPartyModel> ParsePartyModel(Document document, DocumentParty party)
        //    {
        //        return Task.FromResult(new DocPartyModel
        //        {
        //            UserID = party.UserId,
        //            Email = party.User!.Email,
        //            FirstName = party.User.FirstName,
        //            LastName = party.User.LastName,
        //            SignatureStatus = _stringLocalizer[GetPartySigningStatus(document, party).ToString()],
        //        });
        //    }

        //    private bool IsSigned(Document doc, int userID)
        //    {
        //        try
        //        {
        //            byte[]? content = doc.FileContent?.Content;
        //            if (content == null)
        //                return false;

        //            IList<SignatureVerificationResult> userSignatures = _signingService.VerifySignatures(content, userID);
        //            return userSignatures.Any(s => s.Status == SignatureVerificationStatus.VerificationPassed);
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }

        //    private async Task<List<DocumentParty>> GetDocumentPartiesAsync(int docID)
        //    {
        //        List<DocumentParty> parties = await _unitOfWork.DocumentPartyRepository
        //            .AsQueryable()
        //            .Include(party => party.User)
        //            //.ThenInclude(user => user!.UserRoles)
        //            //    .ThenInclude(userRole => userRole.Role)
        //            .Include(party => party.SignatureInfos)
        //            .Where(party => party.DocId == docID)
        //            .OrderBy(entity => entity.Id != CurrentUserID)
        //            .ThenBy(entity => entity.Id)
        //            .ToListAsync();

        //        return parties;
        //    }

        //    //private async Task SendNotificationsForUpdateAsync(Document document)
        //    //{
        //    //    HtmlMessage message = _messageGenerator.DeadlineChangedMessage(document);
        //    //    var partiesEmails = new List<string>(document.DocumentParties.Count);
        //    //    foreach (DocumentParty docParty in document.DocumentParties)
        //    //    {
        //    //        if (docParty.IsAuthor)
        //    //            continue;

        //    //        partiesEmails.Add(docParty.Party.User.Email);

        //    //        UserNotification notif = _notificationCreator.CreatePlainTextNotification(CurrentUserID, docParty.Party.User.Id,
        //    //            message.Title, message.Content);
        //    //        await _unitOfWork.UserNotificationRepository.AddAsync(notif);
        //    //    }

        //    //    if (partiesEmails.Count != 0)
        //    //        await _emailService.SendEmailAsync(new EmailMessage(message.Title, message.Content, true, partiesEmails.ToArray()));
        //    //}

        //    private async Task<List<User>> GetInvitedUsersAsync(AddDocPartyRequest model) =>
        //        await _unitOfWork.UserManager.Users.AsQueryable()
        //            .Where(user => model.UserIDs.Contains(user.Id))
        //            .ToListAsync();

        //    private async Task SentPartyInvitedNotifAsync(HtmlMessage invitedMessage, User user)
        //    {
        //        UserNotification notif = _notificationCreator.CreatePlainTextNotification(CurrentUserID, user.Id,
        //            invitedMessage.Title, invitedMessage.Content);
        //        await _unitOfWork.UserNotificationRepository.AddAsync(notif);
        //    }

        //    private async Task AddDocumentPartyAsync(AddDocPartyRequest model, User user)
        //    {
        //        await _unitOfWork.DocumentPartyRepository.AddAsync(new DocumentParty
        //        {
        //            DocId = model.DocID,
        //            PartyId = user.Id,
        //        });
        //    }

        //    private async Task SendPartyRemovedNotifAsync(DocumentParty party)
        //    {
        //        UserNotification? notif = _notificationCreator.CreatePlainTextNotification(CurrentUserID, party.UserId, "Removed from document parties",
        //            $"You have been removed from document #{party.DocId} {party.Document?.Name} and no longer need to sign it");
        //        await _unitOfWork.UserNotificationRepository.AddAsync(notif);
        //    }
    }
}
