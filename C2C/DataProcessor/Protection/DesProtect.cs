using System.Security.Cryptography;

namespace C2C.DataProcessor.Protection
{
    public class DesProtect : SymmetricAlgorithmProtect
    {
        public DesProtect() : base(DES.Create())
        {
        }
    }
}
