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

        private int _nextSimpleId = PrimitiveSimpleId.Nil.Id + 1;

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
                case NativeSymbolKind.TypedefType:
                    DoExportTypeDef((NativeTypeDef)symbol);
                    break;
                case NativeSymbolKind.Procedure:
                    DoExportProcedure((NativeProcedure)symbol);
                    break;
                case NativeSymbolKind.Constant:
                    DoExportConstant((NativeConstant)symbol);
                    break;
                case NativeSymbolKind.EnumNameValue:
                case NativeSymbolKind.SalAttribute:
                case NativeSymbolKind.SalEntry:
                case NativeSymbolKind.BuiltinType:
                case NativeSymbolKind.ProcedureSignature:
                case NativeSymbolKind.BitVectorType:
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
            var typeId = new PrimitiveSymbolId(nt.Name, nt.Kind);
            _writer.Write(typeId);

            foreach (var member in nt.Members)
            {
                var data = new PrimitiveMemberData(
                    member.Name,
                    DoExportType(member.NativeType),
                    typeId);
                _writer.Write(data);
                MaybeExport(member.NativeType);
            }
        }

        private void DoExportEnum(NativeEnum e)
        {
            var typeId = new PrimitiveSymbolId(e.Name, e.Kind);
            _writer.Write(typeId);
            foreach (var value in e.Values)
            {
                var data = new PrimitiveEnumValueData(value.Name, value.Value.Expression, typeId);
                _writer.Write(data);
            }
        }

        private void DoExportConstant(NativeConstant c)
        {
            var id = new PrimitiveSymbolId(c.Name, c.Kind);
            _writer.Write(id);
            _writer.Write(new PrimitiveConstantData(id, c.Value.Expression, c.ConstantKind));
        }

        private PrimitiveTypeId DoExportType(NativeType nt)
        {
            switch (nt.Kind)
            {
                case NativeSymbolKind.BuiltinType:
                    {
                        // CTODO: shoud cache builtins
                        var id = GetNextSimpleId();
                        var b = (NativeBuiltinType)nt;
                        _writer.Write(new PrimitiveTypeData(id, nt.Kind, builtinType: b.BuiltinType));
                        return new PrimitiveTypeId(id);
                    }
                case NativeSymbolKind.ArrayType:
                    {
                        var id = GetNextSimpleId();
                        var array = (NativeArray)nt;
                        _writer.Write(new PrimitiveTypeData(id, nt.Kind, elementCount: array.ElementCount, elementTypeId: DoExportType(array.RealType)));
                        return new PrimitiveTypeId(id);
                    }
                case NativeSymbolKind.PointerType:
                    {
                        var id = GetNextSimpleId();
                        var pointer = (NativePointer)nt;
                        _writer.Write(new PrimitiveTypeData(id, nt.Kind, elementTypeId: DoExportType(pointer.RealType)));
                        return new PrimitiveTypeId(id);
                    }
                case NativeSymbolKind.BitVectorType:
                    {
                        var id = GetNextSimpleId();
                        var v = (NativeBitVector)nt;
                        _writer.Write(new PrimitiveTypeData(id, nt.Kind, elementCount: v.Size));
                        return new PrimitiveTypeId(id);
                    }
                case NativeSymbolKind.NamedType:
                    {
                        var id = GetNextSimpleId();
                        var n = (NativeNamedType)nt;
                        _writer.Write(new PrimitiveTypeData(id, nt.Kind, name: n.Name, qualification: n.Qualification, isConst: n.IsConst));
                        return new PrimitiveTypeId(id);
                    }
                case NativeSymbolKind.OpaqueType:
                    {
                        var id = GetNextSimpleId();
                        _writer.Write(new PrimitiveTypeData(id, nt.Kind));
                        return new PrimitiveTypeId(id);
                    }
                case NativeSymbolKind.StructType:
                case NativeSymbolKind.UnionType:
                case NativeSymbolKind.EnumType:
                case NativeSymbolKind.FunctionPointer:
                    MaybeExport(nt);
                    return new PrimitiveTypeId(new PrimitiveSymbolId(nt.Name, nt.Kind));
                default:
                    Contract.ThrowInvalidEnumValue(nt.Kind);
                    return PrimitiveTypeId.Nil;
            }
        }

        private PrimitiveSimpleId DoExportSal(NativeSalAttribute sal)
        {
            if (sal.IsEmpty)
            {
                return PrimitiveSimpleId.Nil;
            }

            var id = GetNextSimpleId();
            var list = sal.SalEntryList;
            for (var i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                var data = new PrimitiveSalEntryData(id, i, entry.SalEntryType, entry.Text);
                _writer.Write(data);
            }

            return id;
        }

        private PrimitiveSimpleId DoExportSignature(NativeSignature sig)
        {
            var id = GetNextSimpleId();
            var returnTypeSalId = DoExportSal(sig.ReturnTypeSalAttribute);
            var sigData = new PrimitiveSignatureData(
                id,
                DoExportType(sig.ReturnType),
                DoExportSal(sig.ReturnTypeSalAttribute));
            _writer.Write(sigData);

            var list = sig.Parameters;
            for (var i = 0; i < list.Count; i++)
            {
                var p = list[i];
                var data = new PrimitiveParameterData(
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
            var id = new PrimitiveSymbolId(ptr.Name, ptr.Kind);
            _writer.Write(id);

            var sigId = DoExportSignature(ptr.Signature);
            var data = new PrimitiveFunctionPointerData(
                id,
                ptr.CallingConvention,
                sigId);
            _writer.Write(data);
        }

        private void DoExportTypeDef(NativeTypeDef typeDef)
        {
            var id = new PrimitiveSymbolId(typeDef.Name, typeDef.Kind);
            _writer.Write(id);

            var data = new PrimitiveTypeDefData(id, DoExportType(typeDef.RealType));
            _writer.Write(data);
        }

        private void DoExportProcedure(NativeProcedure proc)
        {
            var id = new PrimitiveSymbolId(proc.Name, proc.Kind);
            _writer.Write(id);

            var data = new PrimitiveProcedureData(
                id,
                proc.CallingConvention,
                DoExportSignature(proc.Signature),
                proc.DllName);
            _writer.Write(data);
        }

        private PrimitiveSimpleId GetNextSimpleId()
        {
            return new PrimitiveSimpleId(_nextSimpleId++);
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
