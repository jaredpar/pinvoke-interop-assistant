using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeSignatureData
    {
        public NativeSimpleId SignatureId { get; }
        public NativeSymbolId ReturnTypeId { get; }
        public NativeSimpleId ReturnTypeSalId { get; }

        public NativeSignatureData(NativeSimpleId id, NativeSymbolId returnTypeId, NativeSimpleId returnTypeSalId)
        {
            SignatureId = id;
            ReturnTypeId = returnTypeId;
            ReturnTypeSalId = returnTypeSalId;
        }
    }

    public struct NativeParameterData
    {
        public NativeSimpleId SignatureId { get; }
        public int Index { get; }
        public string Name { get; }
        public NativeSymbolId TypeId { get; }

        public NativeParameterData(NativeSimpleId signatureId, int index, string name, NativeSymbolId typeId)
        {
            SignatureId = signatureId;
            Index = index;
            Name = name;
            TypeId = typeId;
        }
    }
}
