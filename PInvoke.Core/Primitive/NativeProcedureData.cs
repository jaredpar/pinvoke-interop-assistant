using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeProcedureData
    {
        public NativeSymbolId ProcedureId {get;}
        public NativeCallingConvention CallingConvention { get; }
        public NativeSimpleId SignatureId { get; }
        public string DllName { get; }

        public NativeProcedureData(NativeSymbolId procedureId, NativeCallingConvention convention, NativeSimpleId signatureId, string dllName)
        {
            ProcedureId = procedureId;
            CallingConvention = convention;
            SignatureId = signatureId;
            DllName = dllName;
        }
    }
}
