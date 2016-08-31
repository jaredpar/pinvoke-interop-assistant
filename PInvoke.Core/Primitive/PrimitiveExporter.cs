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
                case NativeSymbolKind.UnionType:
                    DoExportDefined((NativeDefinedType)symbol);
                    break;
                case NativeSymbolKind.EnumType:
                    DoExportEnum((NativeEnum)symbol);
                    break;
                case NativeSymbolKind.EnumNameValue:
                    // Must be handled elsewhere
                    Contract.ThrowIfFalse(false);
                    break;
                default:
                    Contract.ThrowInvalidEnumValue(symbol.Kind);
                    break;
            }
        }

        private void DoExportDefined(NativeDefinedType nt)
        {
            Contract.Requires(nt.Kind == NativeSymbolKind.StructType || nt.Kind == NativeSymbolKind.UnionType);
            var typeId = new NativeTypeId(nt.Name, nt.Kind);
            _writer.Write(typeId);

            foreach (var member in nt.Members)
            {
                var data = new NativeMemberData(
                    member.Name,
                    new NativeTypeId(member.NativeType.Name, member.NativeType.Kind),
                    typeId);
                _writer.Write(data);
                MaybeExport(member.NativeType);
            }
        }

        private void DoExportEnum(NativeEnum e)
        {
            var typeId = new NativeTypeId(e.Name, e.Kind);
            _writer.Write(typeId);
            foreach (var value in e.Values)
            {
                var data = new NativeEnumValueData(value.Name, value.Value.Expression, typeId);
                _writer.Write(data);
            }
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
