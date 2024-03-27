using System;
using System.Security.Cryptography;

namespace C2C.DataProcessor.Protection
{
    public abstract class SymmetricAlgorithmProtect : IDataProtect
    {
        // No need to dispose; it doesn't have actual key or iv
        private readonly SymmetricAlgorithm algorithm;

        private byte[] key;
        private bool disposedValue;

        protected SymmetricAlgorithmProtect(SymmetricAlgorithm algorithm)
        {
            this.algorithm = algorithm;
            this.algorithm.Mode = CipherMode.CBC;
            this.algorithm.Padding = PaddingMode.ISO10126;
        }

        /// <inheritdoc/>
        public byte[] Encrypt(byte[] data)
        {
            // Generate IV
            var iv = new byte[algorithm.BlockSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);

            using (var encrypt = algorithm.CreateEncryptor(key, iv))
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
            var iv = new byte[algorithm.BlockSize];
            Buffer.BlockCopy(data, 0, iv, 0, iv.Length);

            using (var decrypt = algorithm.CreateDecryptor(key, iv))
                return decrypt.TransformFinalBlock(data, iv.Length, data.Length - iv.Length);
        }

        /// <inheritdoc/>
        public void UpdateKey(byte[] key) => this.key = key;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    algorithm.Dispose();

                    Array.Clear(key, 0, key.Length);
                    key = null;
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
