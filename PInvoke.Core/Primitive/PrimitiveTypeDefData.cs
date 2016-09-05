using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveTypeDefData
    {
        public PrimitiveSymbolId SourceTypeId { get; }
        public PrimitiveTypeId TargetTypeId { get; }

        public PrimitiveTypeDefData(PrimitiveSymbolId sourceTypeId, PrimitiveTypeId targetTypeId)
        {
            SourceTypeId = sourceTypeId;
            TargetTypeId = targetTypeId;
        }
    }
}
