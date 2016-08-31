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
        public NativeTypeId ReturnTypeId { get; }
        public NativeSimpleId ReturnTypeSalId { get; }

        public NativeSignatureData(NativeSimpleId id, NativeTypeId returnTypeId, NativeSimpleId returnTypeSalId)
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
        public NativeTypeId TypeId { get; }

        public NativeParameterData(NativeSimpleId signatureId, int index, string name, NativeTypeId typeId)
        {
            SignatureId = signatureId;
            Index = index;
            Name = name;
            TypeId = typeId;
        }
    }
}
