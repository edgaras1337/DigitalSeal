using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using DigitalSeal.Data.Models;
using Microsoft.AspNetCore.Identity;
using DigitalSeal.Data.Repositories;

namespace DigitalSeal.Core.Services
{
    /// <summary>
    /// <inheritdoc cref="IUserCertificateProvider"/>
    /// </summary>
    internal class UserCertificateProvider : IUserCertificateProvider
    {
        private readonly IX509CertificateRepository _x509CertificateRepository;
        private readonly IRSAKeyRepository _rsaKeyRepository;
        private readonly IAesService _aesService;
        private readonly UserManager<User> _userManager;
        public UserCertificateProvider(
            IX509CertificateRepository x509CertificateRepository,
            IRSAKeyRepository rsaKeyRepository,
            IAesService aesService, 
            UserManager<User> userManager)
        {
            _x509CertificateRepository = x509CertificateRepository;
            _rsaKeyRepository = rsaKeyRepository;
            _aesService = aesService;
            _userManager = userManager;
        }

        // TODO: Should move this value into configuration.
        /// <summary>
        /// Amount of years until the certificate expires.
        /// </summary>
        public const int X509CertExpirationYears = 1;

        /// <summary>
        /// <inheritdoc cref="IUserCertificateProvider.LoadUserCertificate"/>
        /// </summary>
        public async Task<byte[]> LoadUserCertificate(int userId)
        {
            Data.Models.X509Certificate? encryptedCert = await _x509CertificateRepository.GetLastForUserAsync(userId);

            X509Certificate2 certificate;
            if (encryptedCert == null || DateTime.UtcNow >= encryptedCert.ExpirationDate)
            {
                certificate = await AddNewCertificateAsync(userId);
            }
            else
            {
                byte[] decryptedPfx = _aesService.DecryptData(encryptedCert.EncryptedCertificateBlob);
                certificate = new X509Certificate2(decryptedPfx);
                if (!certificate.Verify())
                    certificate = await AddNewCertificateAsync(userId);
            }

            return certificate.Export(X509ContentType.Pfx);
        }

        /// <summary>
        /// <inheritdoc cref="IUserCertificateProvider.AddNewCertificateAsync"/>
        /// </summary>
        public async Task<X509Certificate2> AddNewCertificateAsync(int userId)
        {
            GenerateX509CertResult genResult = await GenerateX509Cert(userId, await GetUserCNAsync(userId));
            X509Certificate2 certificate = genResult.Certificate;
            byte[] encryptedPfx = _aesService.EncryptData(certificate.Export(X509ContentType.Pfx));
            var dbObject = new Data.Models.X509Certificate
            {
                EncryptedCertificateBlob = encryptedPfx,
                ExpirationDate = genResult.ExpirationDate,
                UserId = userId
            };
            await _x509CertificateRepository.AddAsync(dbObject);
            await _x509CertificateRepository.SaveChangesAsync();
            return certificate;
        }

        /// <summary>
        /// <inheritdoc cref="IUserCertificateProvider.AddRSAKeyAsync"/>
        /// </summary>
        public async Task AddRSAKeyAsync(int userId) =>
            await AddRsaKeyAsync(userId, GenerateRSAXmlKey());

        private async Task AddRsaKeyAsync(int userId, string xmlKey)
        {
            byte[] encryptedKey = _aesService.EncryptData(Encoding.UTF8.GetBytes(xmlKey));
            var dbObject = new RSAKey
            {
                EncryptedXmlString = encryptedKey,
                UserId = userId
            };
            await _rsaKeyRepository.AddAsync(dbObject);
            await _rsaKeyRepository.SaveChangesAsync();
        }

        private static string GenerateRSAXmlKey()
        {
            using var rsa = new RSACryptoServiceProvider();
            return rsa.ToXmlString(true);
        }

        private async Task<string> GetUserCNAsync(int userId)
        {
            User user = (await _userManager.FindByIdAsync(userId.ToString()))!;
            return $"CN={user.FirstName} {user.LastName}";
        }

        private async Task<GenerateX509CertResult> GenerateX509Cert(int userId, string certOwnerName)
        {
            using RSACryptoServiceProvider rsa = await LoadUserRSCP(userId);

            // Create a certificate request
            var request = new CertificateRequest(certOwnerName, rsa, HashAlgorithmName.SHA512,
                RSASignaturePadding.Pkcs1);

            var expirationDate = DateTime.UtcNow.AddYears(X509CertExpirationYears);

            // Create a self-signed X.509 certificate
            X509Certificate2 certificate = request.CreateSelfSigned(DateTime.UtcNow, expirationDate);

            return new GenerateX509CertResult(certificate, expirationDate);
        }

        public async Task<RSACryptoServiceProvider> LoadUserRSCP(int userId)
        {
            RSAKey? key = await _rsaKeyRepository.GetByUserId(userId);

            if (key == null)
            {
                key = GenerateUserRSAKey(userId);
                await _rsaKeyRepository.AddAsync(key);
                await _rsaKeyRepository.SaveChangesAsync();
            }

            var rsa = new RSACryptoServiceProvider();
            byte[] decryptedKeyData = _aesService.DecryptData(key.EncryptedXmlString);
            rsa.FromXmlString(Encoding.UTF8.GetString(decryptedKeyData));
            return rsa;
        }

        private RSAKey GenerateUserRSAKey(int userId)
        {
            using var rsa = new RSACryptoServiceProvider();
            string xml = rsa.ToXmlString(true);
            byte[] encryptedData = _aesService.EncryptData(Encoding.UTF8.GetBytes(xml));
            return new RSAKey
            {
                EncryptedXmlString = encryptedData,
                UserId = userId
            };
        }

        private record GenerateX509CertResult(X509Certificate2 Certificate, DateTime ExpirationDate);
    }
}
