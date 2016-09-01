using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveConstantData
    {
        public PrimitiveSymbolId Id { get; }
        public string Value { get; }
        public ConstantKind Kind { get; }

        public PrimitiveConstantData(PrimitiveSymbolId id, string value, ConstantKind kind)
        {
            Id = id;
            Value = value;
            Kind = kind;
        }
    }
}
