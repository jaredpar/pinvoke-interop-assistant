using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeConstantData
    {
        public NativeSymbolId Id { get; }
        public string Value { get; }
        public ConstantKind Kind { get; }

        public NativeConstantData(NativeSymbolId id, string value, ConstantKind kind)
        {
            Id = id;
            Value = value;
            Kind = kind;
        }
    }
}
