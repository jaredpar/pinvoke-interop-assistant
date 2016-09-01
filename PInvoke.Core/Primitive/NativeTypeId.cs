using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public struct NativeTypeId : IEquatable<NativeTypeId>
    {
        public static NativeTypeId Nil => new NativeTypeId();
        public NativeSymbolId SymbolId { get; }
        public NativeSimpleId SimpleId { get; }
        public bool IsSymbolId => !SymbolId.IsNil;
        public bool IsSimpleId => !SimpleId.IsNil;
        public bool IsNil => this == Nil;

        public NativeTypeId(NativeSymbolId symbolId)
        {
            SymbolId = symbolId;
            SimpleId = NativeSimpleId.Nil;
        }

        public NativeTypeId(NativeSimpleId simpleId)
        {
            SymbolId = NativeSymbolId.Nil;
            SimpleId = simpleId;
        }

        public NativeTypeId(NativeSymbolId symbolId, NativeSimpleId simpleId)
        {
            SymbolId = symbolId;
            SimpleId = simpleId;
        }

        public static bool operator ==(NativeTypeId left, NativeTypeId right) => left.SymbolId == right.SymbolId && left.SimpleId == right.SimpleId;
        public static bool operator !=(NativeTypeId left, NativeTypeId right) => !(left == right);
        public bool Equals(NativeTypeId other) => this == other;
        public override bool Equals(object obj) => obj is NativeTypeId && Equals((NativeTypeId)obj);
        public override int GetHashCode() => IsSymbolId ? SymbolId.GetHashCode() : SimpleId.GetHashCode();
        public override string ToString() => IsSymbolId ? SymbolId.ToString() : SimpleId.ToString();
    }
}
