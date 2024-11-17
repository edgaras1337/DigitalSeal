using System.Security.Cryptography.X509Certificates;

namespace DigitalSeal.Core.Services
{
    /// <summary>
    /// Helper class that provides users cryptographic certificates used for document signing.
    /// </summary>
    public interface IUserCertificateProvider
    {
        /// <summary>
        /// Gets a valid user certificate.
        /// </summary>
        /// <param name="userID">User ID.</param>
        /// <returns>Byte array of the certificate.</returns>
        Task<byte[]> LoadUserCertificate(int userID);

        /// <summary>
        /// Adds a new certificate into the database.
        /// </summary>
        /// <param name="userID">User ID.</param>
        /// <returns>The newly created certificate.</returns>
        Task<X509Certificate2> AddNewCertificateAsync(int userID);

        /// <summary>
        /// Generates and adds RSA key pair for the user.
        /// </summary>
        /// <param name="userID">User ID.</param>
        Task AddRSAKeyAsync(int userID);
    }
}
