using System;

namespace C2C.Processor.Encryption.Scheme
{
    public interface IEncryptScheme : IDisposable
    {
        /// <summary>
        /// Encrypts the specified data BLOB.
        /// </summary>
        /// <param name="data">the data the encrypt.</param>
        /// <returns>Encrypted data BLOB.</returns>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Decrypts the specified data BLOB.
        /// </summary>
        /// <param name="data">The data to decrypt.</param>
        /// <returns>Decrypted data BLOB.</returns>
        byte[] Decrypt(byte[] data);

        /// <summary>
        /// Update the encryption secret.
        /// </summary>
        /// <param name="secret">The new encryption secret.</param>
        void UpdateSecret(byte[] secret);
    }
}
