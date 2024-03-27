using System;

namespace C2C.DataProcessor.Protection
{
    internal interface IDataProtect : IDisposable
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
        /// Update the encryption key.
        /// </summary>
        /// <param name="key">The new encryption key.</param>
        void UpdateKey(byte[] key);
    }
}
