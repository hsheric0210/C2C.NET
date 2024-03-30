using C2C.Processor.Encryption.Scheme;
using System;
using System.Security.Cryptography;

namespace C2C.Processor.Encryption
{
    public class EphemeralEncryptionProcessor : IProcessor
    {
        private ECDiffieHellman ecdh;
        private readonly IEncryptScheme scheme;

        public Guid ProcessorID => Guid.Parse("31B6CC1B-7BB2-4572-9328-A49F4C80FE95");

        public EphemeralEncryptionProcessor(IEncryptScheme protector) => this.scheme = protector;

        /// <inheritdoc/>
        public byte[] GetNegotationData()
        {
            ecdh = ECDiffieHellman.Create();
            ecdh.KeySize = 521; // secp512r1

            return ecdh.PublicKey.ToByteArray();
        }

        /// <inheritdoc/>
        public void FinishNegotiate(byte[] negotiationData)
        {
            if (ecdh == null)
                throw new InvalidOperationException("ECDH is not initialized. (wrong method call order? Enforce order: 'GetNegotiationData -> FinishNegotiate')");

            var otherPublicKey = ECDiffieHellmanCngPublicKey.FromByteArray(negotiationData, CngKeyBlobFormat.EccPublicBlob);
            var key = ecdh.DeriveKeyFromHash(otherPublicKey, HashAlgorithmName.SHA256);
            scheme.UpdateSecret(key);
        }

        /// <inheritdoc/>
        public byte[] ProcessIncomingData(byte[] data) => scheme.Decrypt(data);

        /// <inheritdoc/>
        public byte[] ProcessOutgoingData(byte[] data) => scheme.Encrypt(data);
    }
}
