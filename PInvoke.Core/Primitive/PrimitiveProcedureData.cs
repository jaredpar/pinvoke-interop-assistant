using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveProcedureData
    {
        public PrimitiveSymbolId ProcedureId {get;}
        public NativeCallingConvention CallingConvention { get; }
        public PrimitiveSimpleId SignatureId { get; }
        public string DllName { get; }

        public PrimitiveProcedureData(PrimitiveSymbolId procedureId, NativeCallingConvention convention, PrimitiveSimpleId signatureId, string dllName)
        {
            ProcedureId = procedureId;
            CallingConvention = convention;
            SignatureId = signatureId;
            DllName = dllName;
        }
    }
}
