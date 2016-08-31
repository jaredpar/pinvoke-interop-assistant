using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeMemberData
    {
        public NativeSymbolId ContainingTypeId {get;}
        public NativeTypeId MemberTypeId { get; }
        public string Name { get; }

        public NativeMemberData(string name, NativeTypeId memberTypeId, NativeSymbolId containingTypeId)
        {
            ContainingTypeId = containingTypeId;
            MemberTypeId = memberTypeId;
            Name = name;
        }
    }
}
