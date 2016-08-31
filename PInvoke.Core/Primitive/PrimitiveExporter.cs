using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public sealed class PrimitiveExporter
    {
        private readonly IPrimitiveWriter _writer;

        public PrimitiveExporter(IPrimitiveWriter writer)
        {
            _writer = writer;
        }

        public void Export(NativeDefinedType nt)
        {
            throw new NotImplementedException();
        }
    }
}
