using System.Security.Cryptography;
using System.Text;

namespace C2C.Processor.Encryption.Scheme
{
    public class AesScheme : SymmetricAlgorithmScheme
    {
        private static readonly byte[] ExampleKeyDeriveSalt = Encoding.UTF8.GetBytes("AES-256 Encryption @ C2C.NET");
        private const int ExampleKeyDeriveIteration = 2001;

        public AesScheme(byte[] keyDeriveSalt, int keyDeriveIteration) : base(Aes.Create(), 256, keyDeriveSalt, keyDeriveIteration)
        {
        }

        public AesScheme() : this(ExampleKeyDeriveSalt, ExampleKeyDeriveIteration)
        {
        }
    }
}
