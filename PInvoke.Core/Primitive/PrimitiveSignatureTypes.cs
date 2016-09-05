using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveSignatureData
    {
        public PrimitiveSimpleId SignatureId { get; }
        public PrimitiveTypeId ReturnTypeId { get; }
        public PrimitiveSimpleId ReturnTypeSalId { get; }

        public PrimitiveSignatureData(PrimitiveSimpleId id, PrimitiveTypeId returnTypeId, PrimitiveSimpleId returnTypeSalId)
        {
            SignatureId = id;
            ReturnTypeId = returnTypeId;
            ReturnTypeSalId = returnTypeSalId;
        }
    }

    public struct PrimitiveParameterData
    {
        public PrimitiveSimpleId SignatureId { get; }
        public int Index { get; }
        public string Name { get; }
        public PrimitiveTypeId TypeId { get; }

        public PrimitiveParameterData(PrimitiveSimpleId signatureId, int index, string name, PrimitiveTypeId typeId)
        {
            SignatureId = signatureId;
            Index = index;
            Name = name;
            TypeId = typeId;
        }
    }
}
