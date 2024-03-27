namespace C2C.DataProcessor.Protection
{
    public class NullProtect : IDataProtect
    {
        /// <inheritdoc/>
        public byte[] Encrypt(byte[] data) => data;

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] data) => data;

        /// <inheritdoc/>
        public void UpdateIV(byte[] iv) { }

        /// <inheritdoc/>
        public void UpdateKey(byte[] key) { }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
