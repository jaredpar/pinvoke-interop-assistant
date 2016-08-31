using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeTypeData
    {
        public NativeSimpleId Id { get; }
        public NativeSymbolKind Kind { get; }
        public int ElementCount { get; }
        public NativeTypeId ElementTypeId { get; }
        public BuiltinType BuiltinType { get; }

        public NativeTypeData(NativeSimpleId id, NativeSymbolKind kind, int elementCount = 0, NativeTypeId? elementTypeId = null, BuiltinType builtinType = default(BuiltinType))
        {
            Id = id;
            Kind = kind;
            ElementCount = elementCount;
            ElementTypeId = elementTypeId ?? NativeTypeId.Nil;
            BuiltinType = builtinType;
        }
    }
}
