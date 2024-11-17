using DigitalSeal.Core.Models.Config;
using DigitalSeal.Core.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace DigitalSeal.Tests.Services
{
    public class AesServiceTests
    {
        private const string AesKey = "thisisaverysecurekey123456789012";
        private const string AesIV = "thisisaninitvect";
        public AesServiceTests()
        {
        }

        [Fact]
        public void EncryptData_ShouldReturnEncryptedData()
        {
            // Arrange
            IAesService aesService = CreateAesService(AesKey, AesIV);
            byte[] data = Encoding.UTF8.GetBytes("Hello world!");

            // Act
            byte[] encryptedData = aesService.EncryptData(data);

            // Assert
            encryptedData.Should().BeOfType<byte[]>();
            encryptedData.Should().NotBeNullOrEmpty();
            encryptedData.Should().NotBeEquivalentTo(data);
        }

        [Fact]
        public void DecryptData_ShouldReturnDecryptedData()
        {
            // Arrange
            IAesService aesService = CreateAesService(AesKey, AesIV);
            byte[] toEncrypt = Encoding.UTF8.GetBytes("Hello world!");
            byte[] encryptedData = aesService.EncryptData(toEncrypt);

            // Act
            byte[] decryptedData = aesService.DecryptData(encryptedData);

            // Assert
            decryptedData.Should().BeOfType<byte[]>();
            decryptedData.Should().NotBeNullOrEmpty();
            decryptedData.Should().BeEquivalentTo(toEncrypt);
        }

        [Theory]
        [InlineData("InvalidKey", "InvalidIV")]
        public void AesOptions_ShouldHaveValidAesKeys(string aesKey, string aesIV)
        {
            // Arrange
            IAesService aesService = CreateAesService(aesKey, aesIV);
            byte[] toEncrypt = Encoding.UTF8.GetBytes("Hello world!");

            // Act
            Action encryptAction = () => aesService.EncryptData(toEncrypt);

            // Assert
            encryptAction.Should().Throw<CryptographicException>();
        }

        private static IAesService CreateAesService(string aesKey, string aesIV)
        {
            IOptions<AesOptions> options = Options.Create(new AesOptions
            {
                Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(aesKey)),
                Vector = Convert.ToBase64String(Encoding.UTF8.GetBytes(aesIV))
            });

            return new AesService(options);
        }
    }
}
