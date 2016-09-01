using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveFunctionPointerData
    {
        public PrimitiveSymbolId ContainingTypeId {get;}
        public NativeCallingConvention CallingConvention { get; }
        public PrimitiveSimpleId SignatureId { get; }

        public PrimitiveFunctionPointerData(PrimitiveSymbolId containingTypeId, NativeCallingConvention convention, PrimitiveSimpleId signatureId)
        {
            ContainingTypeId = containingTypeId;
            CallingConvention = convention;
            SignatureId = signatureId;
        }
    }
}
