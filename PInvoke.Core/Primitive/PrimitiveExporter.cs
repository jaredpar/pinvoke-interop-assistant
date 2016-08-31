using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public sealed class PrimitiveExporter
    {
        private readonly IPrimitiveWriter _writer;

        /// <summary>
        /// Tracks the symbols we are in the process of exporting.  It's null then no work is done, false when 
        /// in the middle of exporting and true when simply done.
        /// </summary>
        private readonly Dictionary<NativeSymbol, bool?> _exportedMap = new Dictionary<NativeSymbol, bool?>();

        public PrimitiveExporter(IPrimitiveWriter writer)
        {
            _writer = writer;
        }

        public void Export(NativeSymbol symbol)
        {
            MaybeExport(symbol);
        }

        private void MaybeExport(NativeSymbol symbol)
        {
            if (IsAnyExport(symbol))
            {
                return;
            }

            DoExport(symbol);
        }

        private void DoExport(NativeSymbol symbol)
        {
            try
            {
                _exportedMap[symbol] = false;
                DoExportCore(symbol);
                _exportedMap[symbol] = true;
            }
            catch
            {
                _exportedMap[symbol] = null;
                throw;
            }
        }

        private void DoExportCore(NativeSymbol symbol)
        {
            switch (symbol.Kind)
            {
                case NativeSymbolKind.StructType:
                    DoExportDefined((NativeDefinedType)symbol);
                    break;
                default:
                    throw Contract.CreateInvalidEnumValueException(symbol.Kind);
            }

            foreach (var child in symbol.GetChildren())
            {
                MaybeExport(child);
            }
        }

        private void DoExportDefined(NativeDefinedType nt)
        {
            _writer.Write(new NativeTypeId(nt.Name, nt.Kind));
        }

        private bool IsExporting(NativeSymbol symbol)
        {
            bool? value;
            return _exportedMap.TryGetValue(symbol, out value) && value == false;
        }

        private bool IsExported(NativeSymbol symbol)
        {
            bool? value;
            return _exportedMap.TryGetValue(symbol, out value) && value == true;
        }

        private bool IsAnyExport(NativeSymbol symbol)
        {
            bool? value;
            return _exportedMap.TryGetValue(symbol, out value) && value != null;
        }
    }
}
