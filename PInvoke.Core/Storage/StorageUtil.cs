using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Storage
{
    public static partial class StorageUtil
    {
        public static BasicSymbolStorage ReadBinary(Stream stream)
        {
            var reader = new BinaryBulkReader(stream);
            return BulkImporter.Import(reader);
        }

        public static void WriteBinary(Stream stream, INativeSymbolLookup lookup)
        {
            var writer = new BinaryBulkWriter(stream);
            WriteCore(writer, lookup);
        }

        private static void WriteCore(IBulkWriter writer, INativeSymbolLookup lookup)
        {
            var exporter = new BulkExporter(writer);
            exporter.Write(lookup);
        }

        public static BasicSymbolStorage ReadCsv(Stream stream)
        {
            var reader = new CsvBulkReader(stream);
            return BulkImporter.Import(reader);
        }

        public static void WriteCsv(Stream stream, INativeSymbolLookup lookup)
        {
            var bulkWriter = new CsvBulkWriter(stream);
            WriteCore(bulkWriter, lookup);
            bulkWriter.WriteDone();
        }
    }
}
