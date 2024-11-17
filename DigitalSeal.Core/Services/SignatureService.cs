using iTextSharp.text.pdf.security;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Pkcs;
using System.Security.Cryptography;
using System.Security;
using iTextSharp.text;
using DigitalSeal.Core.Models.Signature;
using DigitalSeal.Data.Models;
using LanguageExt.Common;
using DigitalSeal.Core.Exceptions;
using DigitalSeal.Core.Models.Config.Email;
using DigitalSeal.Core.Models.Notifications;
using DocEntity = DigitalSeal.Data.Models.Document;
using DigitalSeal.Core.Utilities;
using DigitalSeal.Core.Extensions;
using DigitalSeal.Data.Repositories;
using Microsoft.Extensions.Localization;

namespace DigitalSeal.Core.Services
{
    public enum SignStatus
    {
        InProgress,
        Signed,
        SignedLate,
        NotSigned,
    };

    internal class SignatureService : ISignatureService
    {
        //private readonly AppDbContext _context = null;
        private readonly IDocRepository _docRepository;
        private readonly ISignatureInfoRepository _signatureInfoRepository;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IUserCertificateProvider _userCertificateProvider;
        private readonly ICurrentUserProvider _currUserProvider;
        private readonly IEmailService _emailService;
        private readonly IMessageCreator _messageCreator;
        private readonly IStringLocalizer<SignatureService> _localizer;
        public SignatureService(
            IDocRepository docRepository,
            ISignatureInfoRepository signatureInfoRepository,
            IUserNotificationRepository userNotificationRepository, 
            IUserCertificateProvider userCertificateProvider, 
            ICurrentUserProvider currUserProvider,
            IEmailService emailService, 
            IMessageCreator messageCreator,
            IStringLocalizer<SignatureService> localizer)
        {
            _docRepository = docRepository;
            _signatureInfoRepository = signatureInfoRepository;
            _userNotificationRepository = userNotificationRepository;
            _userCertificateProvider = userCertificateProvider;
            _currUserProvider = currUserProvider;
            _emailService = emailService;
            _messageCreator = messageCreator;
            _localizer = localizer;

            PdfReader.unethicalreading = true;
        }

        public async Task<Result<List<DocEntity>>> GetSigneableDocumentsAsync(IEnumerable<int> docIds)
        {
            //int partyId = await _currUserProvider.GetCurrentPartyIdAsync();
            int userId = _currUserProvider.CurrentUserId;
            List<DocEntity> docs = await _docRepository.GetByIdsAsync(docIds, new UserId(userId), includeRelatedData: true);

            foreach (DocEntity doc in docs)
            {
                if (doc.Extension != FileExtensions.PDF)
                {
                    return new(ValidationException.Error(_localizer["OnlyPdf"]));
                }
            }

            return docs;
        }

        public async Task<Result<bool>> SignDocumentsAsync(SignModel request)
        {
            Party party = await _currUserProvider.GetCurrentPartyAsync();
            int partyId = party.Id;

            //List<DocEntity> docs = await _context.Documents
            //    .Include(doc => doc.FileContent)
            //    .Include(doc => doc.DocumentParties)
            //        .ThenInclude(dp => dp.Party)
            //            .ThenInclude(p => p.User)
            //    .Where(doc => request.DocIds.Contains(doc.Id) && doc.DocumentParties.Any(dp => dp.PartyId == party.Id))
            //    .ToListAsync();

            int userId = party.UserId;
            List<DocEntity> docs = await _docRepository.GetByIdsAsync(request.DocIds, new PartyId(partyId), true);

            //if (docs.Count == 0)
            //{
            //    return new(ValidationException.Error("No documents were selected"));
            //}

            CryptoInfo cryptoInfo = await GetCryptoInfo(party.UserId);

            if (docs.Count > 1)
            {
                request.Position = SignaturePosition.Hidden;
            }

            var messageByEmail = new Dictionary<string, HtmlMessage>();
            foreach (var doc in docs)
            {
                DocumentParty docParty = doc.DocumentParties.First(x => x.PartyId == partyId);
                DocumentParty authorDocParty = doc.DocumentParties.First(x => x.IsAuthor);

                // Sign document.
                byte[] updatedContent = Sign(doc, cryptoInfo, request, partyId);

                VerifySignaturesResult lastSignature = VerifySignatures(updatedContent, partyId)
                    .OrderByDescending(x => x.SignTime)
                    .First();

                // Create signature info.
                _signatureInfoRepository.Add(new SignatureInfo
                {
                    SignDate = lastSignature.SignTime,
                    SignDisplayLocation = (int)request.Position,
                    SignDisplayPage = (int)request.Page,
                    DocumentPartyId = docParty.Id,
                });

                // Only send message to other users.
                SignStatus currentStatus = GetDocumentStatus(doc, updatedContent);
                if (docParty != authorDocParty)
                {
                    User authorUser = authorDocParty.Party.User;
                    HtmlMessage message;
                    SignStatus previousStatus = GetDocumentStatus(doc);
                    if (IsFullySigned(previousStatus) || currentStatus == SignStatus.InProgress)
                    {
                        message = _messageCreator.DocumentSigned(doc, UserHelper.FormatUserName(party.User));
                    }
                    else
                    {
                        message = _messageCreator.DocumentSigningCompleted(doc, currentStatus);
                        messageByEmail.Add(authorUser.Email, message);
                    }

                    UserNotification notification = message.AsTextNotification(party.OrganizationId, party.UserId, authorUser.Id);
                    _userNotificationRepository.Add(notification);
                }

                // Update content.
                doc.FileContent.Content = updatedContent;
            }

            await _signatureInfoRepository.SaveChangesAsync();

            // Send emails only when all is done.
            var sendTasks = new List<Task>(messageByEmail.Count);
            foreach (var item in messageByEmail)
            {
                string senderEmail = item.Key;
                EmailMessage message = item.Value.AsEmailMessage(senderEmail);
                sendTasks.Add(_emailService.SendEmailAsync(message));
            }
            await Task.WhenAll(sendTasks);

            return true;
        }

        public SignStatus GetDocumentStatus(DocEntity doc) => GetDocumentStatus(doc, null);

        private SignStatus GetDocumentStatus(DocEntity doc, byte[]? newContent = null)
        {
            byte[] content = newContent ?? doc.FileContent.Content;
            List<VerifySignaturesResult> signatures = [.. VerifySignatures(content).OrderBy(x => x.SignTime)];

            int signCount = 0;
            bool isLate = false;
            foreach (DocumentParty party in doc.DocumentParties)
            {
                VerifySignaturesResult? firstValid = signatures
                    .FirstOrDefault(x => x.SignerPartyId == party.PartyId &&
                        x.Status == SignatureVerificationStatus.VerificationPassed);
                //.OrderBy(x => x.SignTime)
                //.FirstOrDefault();

                if (firstValid == null)
                {
                    continue;
                }

                if (!isLate)
                {
                    isLate = firstValid.SignTime > doc.Deadline;
                }

                signCount++;
            }

            if (signCount == doc.DocumentParties.Count)
            {
                return isLate ? SignStatus.SignedLate : SignStatus.Signed;
            }
            else if (DateTime.UtcNow <= doc.Deadline)
            {
                return SignStatus.InProgress;
            }

            return SignStatus.NotSigned;


            //int signCount = 0;
            //bool isLate = false;

            //byte[] content = newContent ?? document.FileContent.Content;
            //foreach (DocumentParty party in document.DocumentParties)
            //{
            //    SignatureVerificationResult? firstValid = GetFirstValidSignature(content, party.Party.UserId);
            //    if (firstValid == null)
            //        continue;

            //    if (!isLate)
            //        isLate = firstValid.SignTime > document.Deadline;

            //    signCount++;
            //}

            //if (signCount == document.DocumentParties.Count)
            //    return isLate ? SignStatus.SignedLate : SignStatus.Signed;
            //else if (DateTime.Now <= document.Deadline)
            //    return SignStatus.InProgress;
            //return SignStatus.InProgress;
        }

        public List<VerifySignaturesResult> VerifySignatures(byte[] pdfContent, int partyId = 0)
        {
            var signatureList = new List<VerifySignaturesResult>();
            using var reader = new PdfReader(pdfContent);
            AcroFields acroFields = reader.AcroFields;
            List<string> names = acroFields.GetSignatureNames();

            if (names.Count == 0)
            {
                return signatureList;
            }

            foreach (string signatureName in names)
            {
                VerifySignaturesResult? signature = VerifySignature(acroFields, signatureName, partyId);
                if (signature != null)
                {
                    signatureList.Add(signature);
                }
            }

            return signatureList;
        }

        public bool HasValidSignatures(DocEntity doc, int partyId)
        {
            byte[] content = doc.FileContent.Content;
            List<VerifySignaturesResult> userSignatures = VerifySignatures(content, partyId);
            return userSignatures.Any(s => s.Status == SignatureVerificationStatus.VerificationPassed);
        }

        public Dictionary<SignaturePage, List<SignaturePosition>> GetAvailableSignaturePositions(List<DocEntity> docs)
        {
            var hiddenPositionsByPage = new Dictionary<SignaturePage, List<SignaturePosition>>();

            foreach (DocEntity doc in docs)
            {
                IEnumerable<SignatureInfo> signInfos = doc.DocumentParties.SelectMany(dp => dp.SignatureInfos);
                foreach (SignatureInfo info in signInfos)
                {
                    var page = (SignaturePage)info.SignDisplayPage;
                    var location = (SignaturePosition)info.SignDisplayLocation;

                    if (location == SignaturePosition.Hidden)
                    {
                        continue;
                    }

                    if (!hiddenPositionsByPage.TryGetValue(page, out List<SignaturePosition>? positions))
                    {
                        hiddenPositionsByPage[page] = positions = [];
                    }

                    positions.Add(location);
                }

            }

            return hiddenPositionsByPage;
        }


        private static bool IsFullySigned(SignStatus status)
            => status == SignStatus.Signed || status == SignStatus.SignedLate;

        private async Task<CryptoInfo> GetCryptoInfo(int userId)
        {
            byte[] cert = await _userCertificateProvider.LoadUserCertificate(userId);

            using var certMs = new MemoryStream(cert);
            var pkcs12 = new Pkcs12Store(certMs, []);
            string? privateKeyAlias = pkcs12.Aliases.OfType<string>().FirstOrDefault(pkcs12.IsKeyEntry);

            AsymmetricKeyEntry keyEntry = pkcs12.GetKey(privateKeyAlias);
            X509CertificateEntry[] certEntries = pkcs12.GetCertificateChain(privateKeyAlias);
            if (certEntries == null || certEntries.Length == 0)
            {
                throw new SecurityException("Faulty certificate chain");
            }

            Org.BouncyCastle.X509.X509Certificate endEntityCert = certEntries[0].Certificate;

            ICollection<Org.BouncyCastle.X509.X509Certificate> certChain = certEntries.Select(entry => entry.Certificate).ToList();

            return new(keyEntry, certChain);
        }

        private record CryptoInfo(AsymmetricKeyEntry KeyEntry, ICollection<Org.BouncyCastle.X509.X509Certificate> CertChain);

        private static byte[] Sign(DocEntity doc, CryptoInfo crypto, SignModel request, int partyId)
        {
            byte[] content = doc.FileContent.Content;

            using var reader = new PdfReader(content);

            using var modifiedMs = new MemoryStream();
            using var stamper = PdfStamper.CreateSignature(reader, modifiedMs, '\0', Path.GetTempFileName(), true);

            // Set visible signature
            PdfSignatureAppearance appearance = GetSignatureAppearance(request, partyId, reader, stamper);

            // Create a digital signature
            IExternalSignature pks = new PrivateKeySignature(crypto.KeyEntry.Key, HashAlgorithmName.SHA512.Name);

            // Use MakeSignature.SignDetached to sign the document
            MakeSignature.SignDetached(appearance, pks, crypto.CertChain, null, null, null, 0, CryptoStandard.CMS);

            return modifiedMs.ToArray();
        }

        private static PdfSignatureAppearance GetSignatureAppearance(SignModel request, int partyId, PdfReader reader, PdfStamper stamper)
        {
            int signaturePage = request.Page == SignaturePage.First ? 1 : reader.NumberOfPages;
            Rectangle signatureRect = GetSignatureRectangle(reader, signaturePage, request.Position);

            // Set visible signature
            PdfSignatureAppearance appearance = stamper.SignatureAppearance;
            appearance.Reason = request.Reason;
            appearance.Location = request.Location;
            appearance.Contact = request.Contact;
            appearance.SetVisibleSignature(signatureRect, signaturePage, GetSignatureName(reader, partyId).ToString());

            return appearance;
        }

        private static SignatureFieldName GetSignatureName(PdfReader reader, int partyId)
        {
            int signaturesCount = reader.AcroFields.GetSignatureNames().Count;
            return new SignatureFieldName(partyId, signaturesCount);
        }

        private static VerifySignaturesResult? VerifySignature(AcroFields acroFields, string signatureName, int partyId = 0)
        {
            var signature = new VerifySignaturesResult();

            try
            {
                var fieldName = SignatureFieldName.FromString(signatureName);
                if (partyId > 0 && fieldName?.SignerId != partyId)
                {
                    return null;
                }

                PdfPKCS7 pkcs7 = acroFields.VerifySignature(signatureName);
                bool isVerified = pkcs7.Verify();

                signature.SignerPartyId = fieldName?.SignerId ?? 0;

                signature.Status = isVerified ? SignatureVerificationStatus.VerificationPassed :
                    SignatureVerificationStatus.VerificationFailed;

                Org.BouncyCastle.X509.X509Certificate signingCert = pkcs7.SigningCertificate;
                signature.SignedBy = CertificateInfo.GetSubjectFields(signingCert).GetField("CN");

                PdfDictionary signDict = acroFields.GetSignatureDictionary(signatureName);
                signature.Reason = pkcs7.Reason;
                signature.Location = pkcs7.Location;
                signature.Contact = GetStringFromPdfDictionary(signDict, "ContactInfo");
                signature.SignTime = pkcs7.SignDate.ToUniversalTime();
            }
            catch
            {
                signature.Status = SignatureVerificationStatus.VerificationFailed;
            }

            return signature;
        }

        private static Rectangle GetSignatureRectangle(PdfReader reader, int page, SignaturePosition position)
        {
            Rectangle pageSize = reader.GetPageSize(page);

            float padding = 5f;

            float leftX = padding;
            float rightX = pageSize.Width - padding;
            float topY = pageSize.Height - padding;
            float bottomY = padding;
            float middleX = pageSize.Width / 2;

            float maxWidth = 100f;

            return position switch
            {
                SignaturePosition.TopLeft => new Rectangle(leftX, topY, leftX + maxWidth, topY - 50),
                SignaturePosition.TopRight => new Rectangle(rightX - maxWidth, topY, rightX, topY - 50),
                SignaturePosition.BottomLeft => new Rectangle(leftX, bottomY, leftX + maxWidth, bottomY + 50),
                SignaturePosition.BottomRight => new Rectangle(rightX - maxWidth, bottomY, rightX, bottomY + 50),
                SignaturePosition.TopMiddle => new Rectangle(middleX - maxWidth / 2, topY, middleX + maxWidth / 2, topY - 50),
                SignaturePosition.BottomMiddle => new Rectangle(middleX - maxWidth / 2, bottomY, middleX + maxWidth / 2, bottomY + 50),
                _ => new Rectangle(0, 0, 0, 0),
            };
        }

        private static string? GetStringFromPdfDictionary(PdfDictionary dict, string key) =>
            dict.GetAsString(new PdfName(key))?.ToString();

        public class SignatureFieldName(int signerId, int signatureCount)
        {
            public int SignerId { get; set; } = signerId;
            public int SignatureCount { get; set; } = signatureCount;
            public const string Prefix = "S_";

            public override string ToString() => Prefix + SignerId + "_" + SignatureCount;
            public static SignatureFieldName? FromString(string fieldName)
            {
                string[] splits = fieldName.Split("_");
                return splits.Length != 3 ? null : new SignatureFieldName(int.Parse(splits[1]), int.Parse(splits[2]));
            }
        }
    }



    //public class SignatureService : ISignatureService
    //{
    //    private readonly IUserCertificateProvider _userCertificateProvider;
    //    public SignatureService(IUserCertificateProvider userCertificateProvider)
    //    {
    //        _userCertificateProvider = userCertificateProvider;
    //        PdfReader.unethicalreading = true;
    //    }

    //    public async Task<SignResponse> SignDocumentsAsync(SignRequest request)
    //    {
    //        //if (args.Documents.Count == 0)
    //        //    throw new ArgumentException("No documents to sign");

    //        byte[] cert = await _userCertificateProvider.LoadUserCertificate(request.UserId);

    //        using var certMs = new MemoryStream(cert);
    //        var pkcs12 = new Pkcs12Store(certMs, []);
    //        string? privateKeyAlias = pkcs12.Aliases.OfType<string>().FirstOrDefault(pkcs12.IsKeyEntry);

    //        AsymmetricKeyEntry keyEntry = pkcs12.GetKey(privateKeyAlias);
    //        X509CertificateEntry[] certEntries = pkcs12.GetCertificateChain(privateKeyAlias);
    //        if (certEntries == null || certEntries.Length == 0)
    //            throw new SecurityException("Faulty certificate chain");

    //        Org.BouncyCastle.X509.X509Certificate endEntityCert = certEntries[0].Certificate;

    //        ICollection<Org.BouncyCastle.X509.X509Certificate> certChain = certEntries.Select(entry => entry.Certificate).ToList();

    //        if (request.Documents.Count > 1)
    //            request.Position = SignaturePosition.Hidden;

    //        var result = new SignResponse();
    //        foreach (SignRequest.Document document in request.Documents)
    //        {
    //            byte[] content = document.Content;

    //            using var reader = new PdfReader(content);

    //            using var modifiedMs = new MemoryStream();
    //            using var stamper = PdfStamper.CreateSignature(reader, modifiedMs, '\0', Path.GetTempFileName(), true);

    //            // Set visible signature
    //            PdfSignatureAppearance appearance = GetSignatureAppearance(request, reader, stamper);

    //            // Create a digital signature
    //            IExternalSignature pks = new PrivateKeySignature(keyEntry.Key, HashAlgorithmName.SHA512.Name);

    //            // Use MakeSignature.SignDetached to sign the document
    //            MakeSignature.SignDetached(appearance, pks, certChain, null, null, null, 0, CryptoStandard.CMS);

    //            result.SignedDocuments.Add(new SignResponse.Document(document.Id, modifiedMs.ToArray()));
    //        }

    //        return result;
    //    }

    //    public IList<SignatureVerificationResult> VerifySignatures(byte[] pdfContent, int userID = 0)
    //    {
    //        var signatureList = new List<SignatureVerificationResult>();
    //        using var reader = new PdfReader(pdfContent);
    //        AcroFields acroFields = reader.AcroFields;
    //        List<string> names = acroFields.GetSignatureNames();

    //        if (names.Count == 0)
    //            return signatureList;

    //        foreach (string signatureName in names)
    //        {
    //            SignatureVerificationResult? signature = VerifySignature(acroFields, signatureName, userID);
    //            if (signature != null)
    //                signatureList.Add(signature);
    //        }

    //        return signatureList;
    //    }

    //    private static PdfSignatureAppearance GetSignatureAppearance(SignRequest args, PdfReader reader, PdfStamper stamper)
    //    {
    //        int signaturePage = args.Page == SignaturePage.First ? 1 : reader.NumberOfPages;
    //        Rectangle signatureRect = GetSignatureRectangle(reader, signaturePage, args.Position);

    //        // Set visible signature
    //        PdfSignatureAppearance appearance = stamper.SignatureAppearance;
    //        appearance.Reason = args.Reason;
    //        appearance.Location = args.Location;
    //        appearance.Contact = args.Contact;
    //        appearance.SetVisibleSignature(signatureRect, signaturePage, GetSignatureName(reader, args.UserId).ToString());

    //        return appearance;
    //    }

    //    private static SignatureFieldName GetSignatureName(PdfReader reader, int userID)
    //    {
    //        int signaturesCount = reader.AcroFields.GetSignatureNames().Count;
    //        return new SignatureFieldName(userID, signaturesCount);
    //    }

    //    private static SignatureVerificationResult? VerifySignature(AcroFields acroFields, string signatureName, int userID = 0)
    //    {
    //        var signature = new SignatureVerificationResult();

    //        try
    //        {
    //            var fieldName = SignatureFieldName.FromString(signatureName);
    //            if (userID > 0 && fieldName?.SignerId != userID)
    //                return null;

    //            PdfPKCS7 pkcs7 = acroFields.VerifySignature(signatureName);
    //            bool isVerified = pkcs7.Verify();

    //            signature.Status = isVerified ? SignatureVerificationStatus.VerificationPassed :
    //                SignatureVerificationStatus.VerificationFailed;

    //            SetSignatureMetadata(signature, acroFields, signatureName, pkcs7);
    //        }
    //        catch
    //        {
    //            signature.Status = SignatureVerificationStatus.VerificationFailed;
    //        }

    //        return signature;
    //    }

    //    private static void SetSignatureMetadata(SignatureVerificationResult signature, AcroFields acroFields, string signatureName, PdfPKCS7 pkcs7)
    //    {
    //        Org.BouncyCastle.X509.X509Certificate signingCert = pkcs7.SigningCertificate;
    //        signature.SignedBy = CertificateInfo.GetSubjectFields(signingCert).GetField("CN");

    //        PdfDictionary signDict = acroFields.GetSignatureDictionary(signatureName);
    //        signature.Reason = pkcs7.Reason;
    //        signature.Location = pkcs7.Location;
    //        signature.Contact = GetStringFromPdfDictionary(signDict, "ContactInfo");
    //        signature.SignTime = pkcs7.SignDate;
    //    }

    //    private static Rectangle GetSignatureRectangle(PdfReader reader, int page, SignaturePosition position)
    //    {
    //        Rectangle pageSize = reader.GetPageSize(page);

    //        float padding = 5f;

    //        float leftX = padding;
    //        float rightX = pageSize.Width - padding;
    //        float topY = pageSize.Height - padding;
    //        float bottomY = padding;
    //        float middleX = pageSize.Width / 2;

    //        float maxWidth = 100f;

    //        return position switch
    //        {
    //            SignaturePosition.TopLeft => new Rectangle(leftX, topY, leftX + maxWidth, topY - 50),
    //            SignaturePosition.TopRight => new Rectangle(rightX - maxWidth, topY, rightX, topY - 50),
    //            SignaturePosition.BottomLeft => new Rectangle(leftX, bottomY, leftX + maxWidth, bottomY + 50),
    //            SignaturePosition.BottomRight => new Rectangle(rightX - maxWidth, bottomY, rightX, bottomY + 50),
    //            SignaturePosition.TopMiddle => new Rectangle(middleX - maxWidth / 2, topY, middleX + maxWidth / 2, topY - 50),
    //            SignaturePosition.BottomMiddle => new Rectangle(middleX - maxWidth / 2, bottomY, middleX + maxWidth / 2, bottomY + 50),
    //            _ => new Rectangle(0, 0, 0, 0),
    //        };
    //    }

    //    private static string? GetStringFromPdfDictionary(PdfDictionary dict, string key) =>
    //        dict.GetAsString(new PdfName(key))?.ToString();

    //    public class SignatureFieldName(int signerId, int signatureCount)
    //    {
    //        public int SignerId { get; set; } = signerId;
    //        public int SignatureCount { get; set; } = signatureCount;
    //        public const string Prefix = "S_";

    //        public override string ToString() => Prefix + SignerId + "_" + SignatureCount;
    //        public static SignatureFieldName? FromString(string fieldName)
    //        {
    //            string[] splits = fieldName.Split("_");
    //            if (splits.Length != 3)
    //                return null;

    //            return new SignatureFieldName(int.Parse(splits[1]), int.Parse(splits[2]));
    //        }
    //    }
    //}
}
