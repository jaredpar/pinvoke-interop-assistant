using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeTypeId : IEquatable<NativeTypeId>
    {
        public string Name { get; }
        public NativeSymbolKind Kind { get; }

        public NativeTypeId(string name, NativeSymbolKind kind)
        {
            Name = name;
            Kind = kind;
        }

        public NativeTypeId(NativeType nt)
        {
            Name = nt.Name;
            Kind = nt.Kind;
        }

        public static bool operator ==(NativeTypeId left, NativeTypeId right) => left.Name == right.Name && left.Kind == right.Kind;
        public static bool operator !=(NativeTypeId left, NativeTypeId right) => !(left == right);
        public bool Equals(NativeTypeId other) => this == other;
        public override bool Equals(object obj) => obj is NativeTypeId && Equals((NativeTypeId)obj);
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
        public override string ToString() => $"{Name} - {Kind}";
    }
}
