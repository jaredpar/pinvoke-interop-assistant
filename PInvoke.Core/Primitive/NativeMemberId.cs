using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeMemberId : IEquatable<NativeMemberId>
    {
        public NativeTypeId ContainingTypeId {get;}
        public NativeTypeId MemberTypeId { get; }
        public string Name { get; }

        public NativeMemberId(string name, NativeTypeId memberTypeId, NativeTypeId containingTypeId)
        {
            ContainingTypeId = containingTypeId;
            MemberTypeId = memberTypeId;
            Name = name;
        }

        public static bool operator ==(NativeMemberId left, NativeMemberId right) => 
            left.Name == right.Name &&
            left.MemberTypeId == right.MemberTypeId &&
            left.ContainingTypeId == right.ContainingTypeId;

        public static bool operator !=(NativeMemberId left, NativeMemberId right) => !(left == right);
        public bool Equals(NativeMemberId other) => this == other;
        public override bool Equals(object obj) => obj is NativeMemberId && Equals((NativeMemberId)obj);
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
        public override string ToString() => $"{ContainingTypeId.Name}::{Name}";
    }
}
