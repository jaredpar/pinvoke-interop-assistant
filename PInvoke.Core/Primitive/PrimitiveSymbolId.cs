using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveSymbolId : IEquatable<PrimitiveSymbolId>
    {
        public static PrimitiveSymbolId Nil => default(PrimitiveSymbolId);

        public string Name { get; }
        public NativeSymbolKind Kind { get; }
        public bool IsNil => this == Nil;

        public PrimitiveSymbolId(string name, NativeSymbolKind kind)
        {
            Name = name;
            Kind = kind;
        }

        public PrimitiveSymbolId(NativeType nt)
        {
            Name = nt.Name;
            Kind = nt.Kind;
        }

        public static bool operator ==(PrimitiveSymbolId left, PrimitiveSymbolId right) => left.Name == right.Name && left.Kind == right.Kind;
        public static bool operator !=(PrimitiveSymbolId left, PrimitiveSymbolId right) => !(left == right);
        public bool Equals(PrimitiveSymbolId other) => this == other;
        public override bool Equals(object obj) => obj is PrimitiveSymbolId && Equals((PrimitiveSymbolId)obj);
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
        public override string ToString() => $"{Name} - {Kind}";
    }
}
