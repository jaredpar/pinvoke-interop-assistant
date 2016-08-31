using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeEnumValueData
    {
        public NativeSymbolId ContainingTypeId {get;}
        public string Name { get; }
        public string Value { get; }

        public NativeEnumValueData(string name, string value, NativeSymbolId containingTypeId)
        {
            Name = name;
            Value = value;
            ContainingTypeId = containingTypeId;
        }
    }
}
