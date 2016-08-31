using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeSymbolId : IEquatable<NativeSymbolId>
    {
        public string Name { get; }
        public NativeSymbolKind Kind { get; }

        public NativeSymbolId(string name, NativeSymbolKind kind)
        {
            Name = name;
            Kind = kind;
        }

        public NativeSymbolId(NativeType nt)
        {
            Name = nt.Name;
            Kind = nt.Kind;
        }

        public static bool operator ==(NativeSymbolId left, NativeSymbolId right) => left.Name == right.Name && left.Kind == right.Kind;
        public static bool operator !=(NativeSymbolId left, NativeSymbolId right) => !(left == right);
        public bool Equals(NativeSymbolId other) => this == other;
        public override bool Equals(object obj) => obj is NativeSymbolId && Equals((NativeSymbolId)obj);
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
        public override string ToString() => $"{Name} - {Kind}";
    }
}
