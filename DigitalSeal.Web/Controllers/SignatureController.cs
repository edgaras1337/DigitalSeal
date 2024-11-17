using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using DigitalSeal.Web.Models.ViewModels.Signature;
using DigitalSeal.Core.Services;
using DigitalSeal.Data.Models;
using LanguageExt.Common;
using LanguageExt;
using DigitalSeal.Web.Extensions;

namespace DigitalSeal.Web.Controllers
{
    [Route("signing")]
    public class SignatureController : BaseDSController
    {
        private readonly ISignatureService _signatureService;
        private readonly IStringLocalizer<SignatureController> _localizer;
        public SignatureController(
            INotyfService notyf, 
            ISignatureService signatureService,
            IStringLocalizer<SignatureController> localizer)
            : base(notyf)
        {
            _signatureService = signatureService;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] SignaturePageModel model)
        {
            Result<List<Document>> result = await _signatureService.GetSigneableDocumentsAsync(model.DocIds);
            return MatchResult(result, 
                docs => View(CreateViewModel(model, docs)), 
                _ => Redirect(model.ReturnUrl));
        }

        [HttpPost]
        public async Task<IActionResult> SignDocument(SignRequest request)
        {
            Result<bool> result = await _signatureService.SignDocumentsAsync(request.ToModel());
            IActionResult redirectBack = Redirect(request.ReturnUrl);
            return MatchResult(result, 
                _ => SuccessResult(redirectBack, _localizer["Signed"]), 
                _ => redirectBack);
        }

        private SignatureViewModel CreateViewModel(SignaturePageModel model, List<Document> docs)
        {
            Array.Sort(model.DocIds);
            return new SignatureViewModel
            {
                DocIds = model.DocIds,
                ReturnUrl = model.ReturnUrl,
                DocNames = docs.Select(doc => doc.Name).ToArray(),
                HiddenPositionsByPage = _signatureService.GetAvailableSignaturePositions(docs),
            };
        }

        //private static Core.Models.Signature.SignModel MapSignRequest(Models.ViewModels.Signature.SignRequest model)
        //{
        //    return new Core.Models.Signature.SignModel
        //    {
        //        Reason = model.Reason,
        //        Contact = model.Contact,
        //        Location = model.Location,
        //        Page = model.Page,
        //        Position = model.Position,
        //        DocIds = model.DocIds
        //    };
        //}


        //[HttpGet("verify/{docID}")]
        //public async Task<IActionResult> VerifySignatures(int docID)
        //{
        //    try
        //    {
        //        Document? document = await _unitOfWork.DocumentRepository
        //            .AsQueryable()
        //            .Include(doc => doc.FileContent)
        //            .Include(doc => doc.DocumentParties)
        //            .FirstOrDefaultAsync(doc => doc.Id == docID &&
        //                doc.DocumentParties.Any(docParty => docParty.UserId == CurrentUserID));

        //        if (document == null)
        //            return Json(new GridResponse<DocSignatureInfoModel>());

        //        IList<SignatureVerificationResult> result = _signatureService.VerifySignatures(document.FileContent!.Content);

        //        GridResponse<DocSignatureInfoModel> response = await CreateGridResponseAsync(result, ParseSignVerifResult);
        //        response.DisableAllSelection = true;

        //        return Json(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ErrorResult(BadRequest(), ex.Message);
        //    }
        //}

        //private Task<DocSignatureInfoModel> ParseSignVerifResult(SignatureVerificationResult signature)
        //{
        //    return Task.FromResult(new DocSignatureInfoModel
        //    {
        //        Status = _localizer[signature.Status.ToString()],
        //        SignedBy = signature.SignedBy,
        //        SignTime = DateHelper.FormatDate(signature.SignTime),
        //        Reason = signature.Reason,
        //        Location = signature.Location,
        //        Contact = signature.Contact,
        //    });
        //}

        //private record DocStatusModel(Document Document, SignStatus Status);

        //private async Task UpdateSignedDocumentsAsync(SigningArgs signArgs, SigningResult signResult, HashSet<DocStatusModel> docStatusSet)
        //{
        //    foreach (SigningResult.Document signedDoc in signResult.SignedDocuments)
        //    {
        //        DocStatusModel? docStatusModel = docStatusSet.FirstOrDefault(ds => ds.Document.Id == signedDoc.Id);
        //        if (docStatusModel == null)
        //            continue;

        //        SignStatus statusBeforeSign = docStatusModel.Status;
        //        Document document = docStatusModel.Document;

        //        FileContent? fileContent = docStatusModel.Document.FileContent;
        //        if (fileContent == null)
        //            return;

        //        fileContent.Content = signedDoc.Content;
        //        _unitOfWork.FileContentRepository.Update(fileContent);

        //        DocumentParty signerParty = document.DocumentParties.Single(p => p.UserId == signArgs.UserID);

        //        await AddSignatureInfo(signArgs, signerParty);

        //        if (signerParty.IsAuthor || statusBeforeSign == SignStatus.Signed || statusBeforeSign == SignStatus.SignedLate)
        //            continue;

        //        await SendMessagesBasedOnStatusAsync(document);
        //    }

        //    await _unitOfWork.SaveChangesAsync();
        //}

        //private async Task SendMessagesBasedOnStatusAsync(Document document)
        //{
        //    DocumentParty ownerParty = document.DocumentParties.Single(p => p.IsAuthor);

        //    HtmlMessage signedMsg;
        //    SignStatus statusAfterSign = _documentHelper.GetDocumentStatus(document);
        //    if (statusAfterSign == SignStatus.Signed || statusAfterSign == SignStatus.SignedLate)
        //    {
        //        signedMsg = _messageGenerator.DocumentSigningCompleted(document, statusAfterSign);
        //        await _emailService.SendEmailAsync(new EmailMessage(signedMsg.Title, signedMsg.Content, true, ownerParty.User!.Email!));
        //    }
        //    else
        //        signedMsg = _messageGenerator.DocumentSigned(document);

        //    UserNotification signNotif = _notificationCreator.CreatePlainTextNotification(CurrentUserID, ownerParty.UserId,
        //        signedMsg.Title, signedMsg.Content);
        //    await _unitOfWork.UserNotificationRepository.AddAsync(signNotif);
        //}

        //private async Task AddSignatureInfo(SigningArgs signArgs, DocumentParty signerParty)
        //{
        //    await _unitOfWork.SignatureInfoRepository.AddAsync(new SignatureInfo
        //    {
        //        SignDate = DateTime.Now,
        //        SignDisplayLocation = (int)signArgs.Position,
        //        SignDisplayPage = (int)signArgs.Page,
        //        DocumentPartyId = signerParty.Id,
        //    });
        //}



        //private async Task<List<Document>> GetDocumentsAsync(IEnumerable<int> fileIDs)
        //{
        //    return await _context.Documents
        //        .Include(doc => doc.FileContent)
        //        .Include(doc => doc.DocumentParties)
        //            .ThenInclude(docParty => docParty.SignatureInfos)
        //        .OrderBy(e => e.Id)
        //        .Where(doc => fileIDs.Contains(doc.Id) &&
        //            doc.DocumentParties.Any(docParty => docParty.UserId == CurrentUserID))
        //        .ToListAsync();
        //}

        //protected static bool IsPdf(Document doc) => doc.Extension == "pdf";

        //private async Task<SignatureViewModel?> CreateViewModelAsync(SignatureModel model)
        //{
        //    var result = _signatureService.GetSigneableDocumentsAsync(model.DocIDs);
        //    return result.Match()

        //    List<Document> docs = await GetDocumentsAsync(model.DocIDs!);
        //    if (!IsDocsValid(docs))
        //        return null;

        //    var viewModel = new SignatureViewModel
        //    {
        //        DocIds = model.DocIDs!,
        //        ReturnUrl = model.ReturnUrl,
        //        DocNames = docs.Select(doc => doc.Name).ToList(),
        //        HiddenPositionsByPage = _signatureService.GetAvailableSignaturePositions(docs),
        //    };

        //    return viewModel;
        //}

        //private bool IsDocsValid(IEnumerable<Document> docs)
        //{
        //    foreach (var doc in docs)
        //    {
        //        if (!IsPdf(doc))
        //        {
        //            Notyf.Error("Only PDF documents can be signed.");
        //            return false;
        //        }

        //        //if (doc?.FileContent?.Content == null)
        //        //{
        //        //    Notyf.Error("Failed to retrieve documents.");
        //        //    return false;
        //        //}
        //    }
        //    return true;
        //}

        //private HashSet<DocStatusModel> GetDocStatusSet(IList<Document> documents)
        //{
        //    var docStatusSet = new HashSet<DocStatusModel>();
        //    foreach (Document document in documents)
        //        docStatusSet.Add(new DocStatusModel(document, _documentHelper.GetDocumentStatus(document)));
        //    return docStatusSet;
        //}

        //private async Task<List<Document>> GetDocumentsForSigningAsync(SignRequestModel model) =>
        //    await _context.DocumentRepository
        //        .AsQueryable()
        //        .Include(doc => doc.FileContent)
        //        .Include(doc => doc.DocumentParties)
        //            .ThenInclude(docParty => docParty.SignatureInfos)
        //        .Include(doc => doc.DocumentParties)
        //            .ThenInclude(docParty => docParty.User)
        //        .Where(doc => model.DocIds.Contains(doc.Id) &&
        //            doc.DocumentParties.Any(docParty => docParty.UserId == CurrentUserID))
        //        .ToListAsync();

        //private SigningArgs CreateSigningArgs(SignRequestModel model, List<Document> documents) => new()
        //{
        //    UserID = CurrentUserID,
        //    Reason = model.Reason,
        //    Contact = model.Contact,
        //    Location = model.Location,
        //    Page = model.Page,
        //    Position = model.Position,
        //    Documents = documents.Select(doc => new SigningArgs.Document(doc)).ToHashSet(),
        //};
    }
}
