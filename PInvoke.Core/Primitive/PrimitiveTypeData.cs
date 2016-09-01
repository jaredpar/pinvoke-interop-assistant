using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveTypeData
    {
        public PrimitiveSimpleId Id { get; }
        public NativeSymbolKind Kind { get; }
        public int ElementCount { get; }
        public PrimitiveTypeId ElementTypeId { get; }
        public BuiltinType BuiltinType { get; }
        public string Name { get; }
        public string Qualification { get; }
        public bool IsConst { get; }

        public PrimitiveTypeData(
            PrimitiveSimpleId id, 
            NativeSymbolKind kind, 
            int elementCount = 0, 
            PrimitiveTypeId? elementTypeId = null, 
            BuiltinType builtinType = default(BuiltinType), 
            string name = null,
            string qualification = null,
            bool isConst = false)
        {
            Id = id;
            Kind = kind;
            ElementCount = elementCount;
            ElementTypeId = elementTypeId ?? PrimitiveTypeId.Nil;
            BuiltinType = builtinType;
            Name = name;
            Qualification = qualification;
            IsConst = isConst;
        }
    }
}
