using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveMemberData
    {
        public PrimitiveSymbolId ContainingTypeId {get;}
        public PrimitiveTypeId MemberTypeId { get; }
        public string Name { get; }

        public PrimitiveMemberData(string name, PrimitiveTypeId memberTypeId, PrimitiveSymbolId containingTypeId)
        {
            ContainingTypeId = containingTypeId;
            MemberTypeId = memberTypeId;
            Name = name;
        }
    }
}
