using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PInvoke.Primitive;
using PInvoke.Primitive.Bulk;

namespace PInvoke
{
    public static partial class StorageUtil
    {
        public static BasicPrimitiveStorage ReadBinaryPrimitive(Stream stream)
        {
            var reader = new BinaryBulkReader(stream);
            return BulkUtil.Read(reader);
        }

        public static INativeSymbolImporter ReadBinary(Stream stream)
        {
            return ReadBinaryPrimitive(stream).ToSymbolImporter();
        }

        public static void WriteBinary(Stream stream, INativeSymbolLookup lookup)
        {
            var writer = BulkUtil.CreateWriter(new BinaryBulkWriter(stream));
            WriteCore(writer, lookup);
        }

        private static void WriteCore(IPrimitiveWriter writer, INativeSymbolLookup lookup)
        {
            var exporter = new PrimitiveExporter(writer);
            foreach (var name in lookup.NativeNames)
            {
                if (name.Kind == NativeNameKind.EnumValue)
                {
                    continue;
                }

                var symbol = lookup.GetGlobalSymbol(name);
                exporter.Export(symbol.Symbol);
            }
        }

        public static BasicPrimitiveStorage ReadCsvPrimitive(Stream stream)
        {
            var reader = new CsvBulkReader(stream);
            return BulkUtil.Read(reader);
        }

        public static INativeSymbolImporter ReadCsv(Stream stream)
        {
            return ReadCsvPrimitive(stream).ToSymbolImporter();
        }

        public static void WriteCsv(Stream stream, INativeSymbolLookup lookup)
        {
            var bulkWriter = new CsvBulkWriter(stream);
            var writer = BulkUtil.CreateWriter(bulkWriter);
            WriteCore(writer, lookup);
            bulkWriter.WriteDone();
        }
    }
}
