using C2C.Processor.Encryption.Scheme;
using System;
using System.Security.Cryptography;

namespace C2C.Processor.Encryption
{
    /// <summary>
    /// Encrypts the data or payload, making the MITM attack impossible.
    /// The encryption key is generated while handshaking with ECDH.
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/Ephemeral_key
    /// </remarks>
    public class EphemeralEncryptionProcessorCng : IProcessor
    {
        private ECDiffieHellman ecdh;
        private readonly IEncryptScheme scheme;

        public Guid ProcessorID => Guid.Parse("31B6CC1B-7BB2-4572-9328-A49F4C80FE95");

        public EphemeralEncryptionProcessorCng(IEncryptScheme scheme)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException("This feature is only available on Windows environment. (it uses CNG library)");

            this.scheme = scheme;
        }

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
            var secret = ecdh.DeriveKeyMaterial(otherPublicKey);
            scheme.UpdateSecret(secret);
        }

        /// <inheritdoc/>
        public byte[] ProcessIncomingData(byte[] data) => scheme.Decrypt(data);

        /// <inheritdoc/>
        public byte[] ProcessOutgoingData(byte[] data) => scheme.Encrypt(data);
    }
}
