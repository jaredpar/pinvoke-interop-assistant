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

        private int _nextSimpleId = NativeSimpleId.Nil.Id + 1;

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
                case NativeSymbolKind.FunctionPointer:
                    DoExportFunctionPointer((NativeFunctionPointer)symbol);
                    break;
                case NativeSymbolKind.Procedure:
                    DoExportProcedure((NativeProcedure)symbol);
                    break;
                case NativeSymbolKind.EnumNameValue:
                case NativeSymbolKind.SalAttribute:
                case NativeSymbolKind.SalEntry:
                case NativeSymbolKind.BuiltinType:
                case NativeSymbolKind.ProcedureSignature:
                    // Must be handled elsewhere
                    Contract.ThrowIfFalse(false);
                    break;
                default:
                    // Member missed by developer
                    Contract.ThrowInvalidEnumValue(symbol.Kind);
                    break;
            }
        }

        private void DoExportDefined(NativeDefinedType nt)
        {
            Contract.Requires(nt.Kind == NativeSymbolKind.StructType || nt.Kind == NativeSymbolKind.UnionType);
            var typeId = new NativeSymbolId(nt.Name, nt.Kind);
            _writer.Write(typeId);

            foreach (var member in nt.Members)
            {
                var data = new NativeMemberData(
                    member.Name,
                    DoExportType(member.NativeType),
                    typeId);
                _writer.Write(data);
                MaybeExport(member.NativeType);
            }
        }

        private void DoExportEnum(NativeEnum e)
        {
            var typeId = new NativeSymbolId(e.Name, e.Kind);
            _writer.Write(typeId);
            foreach (var value in e.Values)
            {
                var data = new NativeEnumValueData(value.Name, value.Value.Expression, typeId);
                _writer.Write(data);
            }
        }

        private NativeSymbolId DoExportType(NativeType nt)
        {
            if (nt.Kind == NativeSymbolKind.BuiltinType)
            {
                var b = (NativeBuiltinType)nt;
                return new NativeSymbolId(b.BuiltinType.ToString(), NativeSymbolKind.BuiltinType);
            }

            MaybeExport(nt);
            return new NativeSymbolId(nt.Name, nt.Kind);
        }

        private NativeSimpleId DoExportSal(NativeSalAttribute sal)
        {
            if (sal.IsEmpty)
            {
                return NativeSimpleId.Nil;
            }

            var id = GetNextSimpleId();
            var list = sal.SalEntryList;
            for (var i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                var data = new NativeSalEntryData(id, i, entry.SalEntryType, entry.Text);
                _writer.Write(data);
            }

            return id;
        }

        private NativeSimpleId DoExportSignature(NativeSignature sig)
        {
            var id = GetNextSimpleId();
            var returnTypeSalId = DoExportSal(sig.ReturnTypeSalAttribute);
            var sigData = new NativeSignatureData(
                id,
                DoExportType(sig.ReturnType),
                DoExportSal(sig.ReturnTypeSalAttribute));
            _writer.Write(sigData);

            var list = sig.Parameters;
            for (var i = 0; i < list.Count; i++)
            {
                var p = list[i];
                var data = new NativeParameterData(
                    id,
                    i,
                    p.Name,
                    DoExportType(p.NativeType));
                _writer.Write(data);
            }

            return id;
        }

        private void DoExportFunctionPointer(NativeFunctionPointer ptr)
        {
            var id = new NativeSymbolId(ptr.Name, ptr.Kind);
            _writer.Write(id);

            var sigId = DoExportSignature(ptr.Signature);
            var data = new NativeFunctionPointerData(
                id,
                ptr.CallingConvention,
                sigId);
            _writer.Write(data);
        }

        private void DoExportProcedure(NativeProcedure proc)
        {
            var id = new NativeSymbolId(proc.Name, proc.Kind);
            _writer.Write(id);

            var data = new NativeProcedureData(
                id,
                proc.CallingConvention,
                DoExportSignature(proc.Signature),
                proc.DllName);
            _writer.Write(data);
        }

        private NativeSimpleId GetNextSimpleId()
        {
            return new NativeSimpleId(_nextSimpleId++);
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
