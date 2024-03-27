using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C2C.DataProcessor
{
    internal interface IDataProcessor
    {
        byte[] ProcessOutgoingData(byte[] data);
        byte[] ProcessIncomingData(byte[] data);
    }
}
