using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeTypeDefData
    {
        public NativeSymbolId SourceTypeId { get; }
        public NativeTypeId TargetTypeId { get; }

        public NativeTypeDefData(NativeSymbolId sourceTypeId, NativeTypeId targetTypeId)
        {
            SourceTypeId = sourceTypeId;
            TargetTypeId = targetTypeId;
        }
    }
}
