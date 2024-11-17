using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSeal.Core.Services
{
    /// <summary>
    /// Class used for data transformation using the AES algorithm.
    /// </summary>
    public interface IAesService
    {
        /// <summary>
        /// Encrypts given data.
        /// </summary>
        /// <param name="data">Byte array to encrypt.</param>
        /// <returns>Encrypted data.</returns>
        byte[] EncryptData(byte[] data);

        /// <summary>
        /// Decrypts given data.
        /// </summary>
        /// <param name="encryptedData">Byte array to decrypt.</param>
        /// <returns>Decrypted data.</returns>
        byte[] DecryptData(byte[] encryptedData);
    }
}
