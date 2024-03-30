using System.Security.Cryptography;
using System.Text;

namespace C2C.Processor.Encryption.Scheme
{
    public class DesScheme : SymmetricAlgorithmScheme
    {
        private static readonly byte[] ExampleKeyDeriveSalt = Encoding.UTF8.GetBytes("DES Encryption @ C2C.NET");
        private const int ExampleKeyDeriveIteration = 1978;

        public DesScheme(byte[] keyDeriveSalt, int keyDeriveIteration) : base(DES.Create(), 56, keyDeriveSalt, keyDeriveIteration)
        {
        }

        public DesScheme() : this(ExampleKeyDeriveSalt, ExampleKeyDeriveIteration)
        {
        }
    }
}
