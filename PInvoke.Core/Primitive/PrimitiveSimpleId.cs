using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveSimpleId : IEquatable<PrimitiveSimpleId>
    {
        public static PrimitiveSimpleId Nil => new PrimitiveSimpleId(0);

        public int Id { get; }
        public bool IsNil => Nil == this;

        public PrimitiveSimpleId(int id)
        {
            Id = id;
        }

        public static bool operator ==(PrimitiveSimpleId left, PrimitiveSimpleId right) => left.Id == right.Id;
        public static bool operator !=(PrimitiveSimpleId left, PrimitiveSimpleId right) => !(left == right);
        public bool Equals(PrimitiveSimpleId other) => this == other;
        public override bool Equals(object obj) => obj is PrimitiveSimpleId && Equals((PrimitiveSimpleId)obj);
        public override int GetHashCode() => Id;
        public override string ToString() => $"{Id}";
    }
}
