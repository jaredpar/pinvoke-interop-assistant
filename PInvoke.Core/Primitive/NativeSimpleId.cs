using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeSimpleId : IEquatable<NativeSimpleId>
    {
        public static NativeSimpleId Nil => new NativeSimpleId(0);

        public int Id { get; }
        public bool IsNil => Nil == this;

        public NativeSimpleId(int id)
        {
            Id = id;
        }

        public static bool operator ==(NativeSimpleId left, NativeSimpleId right) => left.Id == right.Id;
        public static bool operator !=(NativeSimpleId left, NativeSimpleId right) => !(left == right);
        public bool Equals(NativeSimpleId other) => this == other;
        public override bool Equals(object obj) => obj is NativeSimpleId && Equals((NativeSimpleId)obj);
        public override int GetHashCode() => Id;
        public override string ToString() => $"{Id}";
    }
}
