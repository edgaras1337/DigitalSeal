using DigitalSeal.Core.Models.Config;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace DigitalSeal.Core.Services
{
    public class AesService : IAesService
    {
        private readonly AesOptions _aesOptions;
        public AesService(IOptions<AesOptions> aesOptions)
            => _aesOptions = aesOptions.Value;

        public byte[] EncryptData(byte[] data)
            => TransformData(data, aes => aes.CreateEncryptor());

        public byte[] DecryptData(byte[] encryptedData)
            => TransformData(encryptedData, aes => aes.CreateDecryptor());

        /// <summary>
        /// Transforms data using the given transformation algorithm.
        /// </summary>
        /// <param name="data">Data to transform.</param>
        /// <param name="createTransformAlg">Delegate that creates a transformation algorithm.</param>
        /// <returns>Transformed data.</returns>
        private byte[] TransformData(byte[] data, Func<Aes, ICryptoTransform> createTransformAlg)
        {
            var aesAlg = Aes.Create();
            aesAlg.Key = Convert.FromBase64String(_aesOptions.Key);
            aesAlg.IV = Convert.FromBase64String(_aesOptions.Vector);

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, createTransformAlg(aesAlg), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();

            return ms.ToArray();
        }
    }
}
