namespace C2C.Processor.Encryption.Scheme
{
    public class NullScheme : IEncryptScheme
    {
        /// <inheritdoc/>
        public byte[] Encrypt(byte[] data) => data;

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] data) => data;

        /// <inheritdoc/>
        public void UpdateIV(byte[] iv)
        { }

        /// <inheritdoc/>
        public void UpdateSecret(byte[] key)
        { }

        /// <inheritdoc/>
        public void Dispose()
        { }
    }
}
