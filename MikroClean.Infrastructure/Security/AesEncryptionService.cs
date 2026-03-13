using MikroClean.Domain.Interfaces.Security;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace MikroClean.Infrastructure.Security
{
    /// <summary>
    /// Implementación de encriptación AES-256 para passwords de routers
    /// </summary>
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private const int KeySize = 32; // 256 bits
        private const int IvSize = 16; // 128 bits

        public AesEncryptionService(IConfiguration configuration)
        {
            var secretKey = configuration["Encryption:SecretKey"] 
                ?? throw new InvalidOperationException("Encryption:SecretKey no configurada en appsettings");

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("Encryption:SecretKey no puede estar vacía");

            // Derivar key e IV desde el secret key usando SHA256
            using var sha256 = SHA256.Create();
            var keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
            
            _key = new byte[KeySize];
            Array.Copy(keyBytes, _key, KeySize);

            // IV se deriva de la segunda mitad del hash + padding
            _iv = new byte[IvSize];
            using var sha256_iv = SHA256.Create();
            var ivBytes = sha256_iv.ComputeHash(Encoding.UTF8.GetBytes(secretKey + "_iv"));
            Array.Copy(ivBytes, _iv, IvSize);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Retornar como Base64 para almacenamiento en DB
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error encriptando datos", ex);
            }
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentNullException(nameof(encryptedText));

            if (!IsValidEncryptedText(encryptedText))
                throw new ArgumentException("El texto no está en formato encriptado válido", nameof(encryptedText));

            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error desencriptando datos. Verifique la clave secreta.", ex);
            }
        }

        public bool IsValidEncryptedText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            try
            {
                // Validar que es Base64 válido
                var buffer = Convert.FromBase64String(text);
                return buffer.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
