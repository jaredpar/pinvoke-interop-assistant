using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive.Bulk
{
    public static class Extensions
    {
        public static void WriteNameKind(this IBulkWriter writer, NativeNameKind kind) => writer.WriteInt32((int)kind);
        public static void WriteSymbolKind(this IBulkWriter writer, NativeSymbolKind kind) => writer.WriteInt32((int)kind);
        public static NativeNameKind ReadNameKind(this IBulkReader reader) => (NativeNameKind)reader.ReadInt32();
        public static NativeSymbolKind ReadSymbolKind(this IBulkReader reader) => (NativeSymbolKind)reader.ReadInt32();

    }
}
