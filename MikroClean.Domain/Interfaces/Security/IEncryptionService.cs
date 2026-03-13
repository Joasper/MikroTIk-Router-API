namespace MikroClean.Domain.Interfaces.Security
{
    /// <summary>
    /// Servicio para encriptar/desencriptar información sensible como contraseñas de routers
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encripta texto usando AES-256
        /// </summary>
        string Encrypt(string plainText);

        /// <summary>
        /// Desencripta texto previamente encriptado
        /// </summary>
        string Decrypt(string encryptedText);

        /// <summary>
        /// Valida si un texto está encriptado correctamente
        /// </summary>
        bool IsValidEncryptedText(string text);
    }
}
