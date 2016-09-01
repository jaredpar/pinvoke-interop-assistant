using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct PrimitiveTypeId : IEquatable<PrimitiveTypeId>
    {
        public static PrimitiveTypeId Nil => new PrimitiveTypeId();
        public PrimitiveSymbolId SymbolId { get; }
        public PrimitiveSimpleId SimpleId { get; }
        public bool IsSymbolId => !SymbolId.IsNil;
        public bool IsSimpleId => !SimpleId.IsNil;
        public bool IsNil => this == Nil;

        public PrimitiveTypeId(PrimitiveSymbolId symbolId)
        {
            SymbolId = symbolId;
            SimpleId = PrimitiveSimpleId.Nil;
        }

        public PrimitiveTypeId(PrimitiveSimpleId simpleId)
        {
            SymbolId = PrimitiveSymbolId.Nil;
            SimpleId = simpleId;
        }

        public PrimitiveTypeId(PrimitiveSymbolId symbolId, PrimitiveSimpleId simpleId)
        {
            SymbolId = symbolId;
            SimpleId = simpleId;
        }

        public static bool operator ==(PrimitiveTypeId left, PrimitiveTypeId right) => left.SymbolId == right.SymbolId && left.SimpleId == right.SimpleId;
        public static bool operator !=(PrimitiveTypeId left, PrimitiveTypeId right) => !(left == right);
        public bool Equals(PrimitiveTypeId other) => this == other;
        public override bool Equals(object obj) => obj is PrimitiveTypeId && Equals((PrimitiveTypeId)obj);
        public override int GetHashCode() => IsSymbolId ? SymbolId.GetHashCode() : SimpleId.GetHashCode();
        public override string ToString() => IsSymbolId ? SymbolId.ToString() : SimpleId.ToString();
    }
}
