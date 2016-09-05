using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveEnumValueData
    {
        public PrimitiveSymbolId ContainingTypeId {get;}
        public string Name { get; }
        public string Value { get; }

        public PrimitiveEnumValueData(string name, string value, PrimitiveSymbolId containingTypeId)
        {
            Name = name;
            Value = value;
            ContainingTypeId = containingTypeId;
        }
    }
}
