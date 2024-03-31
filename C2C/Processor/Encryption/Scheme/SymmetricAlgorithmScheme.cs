using System;
using System.Security.Cryptography;

namespace C2C.Processor.Encryption.Scheme
{
    public abstract class SymmetricAlgorithmScheme : IEncryptScheme
    {
        // No need to dispose; it doesn't have actual key or iv
        private readonly SymmetricAlgorithm algorithm;

        private readonly byte[] keyDeriveSalt;
        private readonly int keyDeriveIteration;

        private byte[] encryptionKey;
        private bool disposedValue;

        protected SymmetricAlgorithmScheme(SymmetricAlgorithm algorithm, int keySize, byte[] keyDeriveSalt, int keyDeriveIteration)
        {
            this.algorithm = algorithm;
            this.keyDeriveSalt = keyDeriveSalt;
            this.keyDeriveIteration = keyDeriveIteration;

            this.algorithm.KeySize = keySize;
            this.algorithm.Mode = CipherMode.CBC;
            this.algorithm.Padding = PaddingMode.ISO10126;
        }

        /// <inheritdoc/>
        public byte[] Encrypt(byte[] data)
        {
            // Generate IV
            var iv = new byte[algorithm.BlockSize / 8];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);

            using (var encrypt = algorithm.CreateEncryptor(encryptionKey, iv))
            {
                // Do encrypt the data
                var cipherText = encrypt.TransformFinalBlock(data, 0, data.Length);

                // Append the IV header
                var outBuffer = new byte[iv.Length + cipherText.Length];
                Buffer.BlockCopy(iv, 0, outBuffer, 0, iv.Length);
                Buffer.BlockCopy(cipherText, 0, outBuffer, iv.Length, cipherText.Length);

                return outBuffer;
            }
        }

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] data)
        {
            // Read the IV header
            var iv = new byte[algorithm.BlockSize / 8];
            Buffer.BlockCopy(data, 0, iv, 0, iv.Length);

            using (var decrypt = algorithm.CreateDecryptor(encryptionKey, iv))
                return decrypt.TransformFinalBlock(data, iv.Length, data.Length - iv.Length);
        }

        /// <inheritdoc/>
        public void UpdateSecret(byte[] secret)
        {
            // Key stretching
            var pbkdf2 = new Rfc2898DeriveBytes(secret, keyDeriveSalt, keyDeriveIteration);
            encryptionKey = pbkdf2.GetBytes(algorithm.KeySize / 8);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    algorithm.Dispose();

                    Array.Clear(encryptionKey, 0, encryptionKey.Length);
                    encryptionKey = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
