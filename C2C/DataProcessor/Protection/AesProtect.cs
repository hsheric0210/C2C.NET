using System.Security.Cryptography;

namespace C2C.DataProcessor.Protection
{
    public class AesProtect : SymmetricAlgorithmProtect
    {
        public AesProtect() : base(Aes.Create())
        {
        }
    }
}
