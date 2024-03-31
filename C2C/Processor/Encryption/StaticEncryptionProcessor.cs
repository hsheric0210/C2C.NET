using C2C.Processor.Encryption.Scheme;
using System;

namespace C2C.Processor.Encryption
{
    /// <summary>
    /// Encrypts the data or payload, making the MITM attack impossible.
    /// You should manually specify the static encryption key.
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/Static_key
    /// </remarks>
    public class StaticEncryptionProcessor : IProcessor
    {
        private readonly IEncryptScheme scheme;

        public Guid ProcessorID => Guid.Parse("6C33A39A-AC2C-44B9-8F38-CC3F7F4C94F7");

        public StaticEncryptionProcessor(IEncryptScheme protector) => this.scheme = protector;

        public void UpdateKey(byte[] key) => scheme.UpdateSecret(key);

        /// <inheritdoc/>
        public byte[] GetNegotationData() => Array.Empty<byte>();

        /// <inheritdoc/>
        public void FinishNegotiate(byte[] negotiationData)
        { }

        /// <inheritdoc/>
        public byte[] ProcessIncomingData(byte[] data) => scheme.Decrypt(data);

        /// <inheritdoc/>
        public byte[] ProcessOutgoingData(byte[] data) => scheme.Encrypt(data);
    }
}
