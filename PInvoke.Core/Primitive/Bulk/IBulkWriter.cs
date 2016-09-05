using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive.Bulk
{
    public interface IBulkWriter
    {
        void WriteString(string str);
        void WriteBoolean(bool b);
        void WriteInt32(int i);
        void WriteItemStart();
        void WriteItemEnd();
    }
}
