using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C2C.DataProcessor.Protection
{
    internal class DataProtector : IDataProcessor
    {
        private readonly IDataProtect protector;

        public DataProtector(IDataProtect protector) => this.protector = protector;

        public byte[] ProcessIncomingData(byte[] data) => protector.Decrypt(data);
        public byte[] ProcessOutgoingData(byte[] data) => protector.Encrypt(data);
    }
}
