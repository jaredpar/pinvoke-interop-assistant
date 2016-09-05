using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Storage
{
    public interface IBulkReader
    {
        bool IsDone();
        string ReadString();
        bool ReadBoolean();
        int ReadInt32();
        void ReadItemStart();
        void ReadItemEnd();
    }
}
