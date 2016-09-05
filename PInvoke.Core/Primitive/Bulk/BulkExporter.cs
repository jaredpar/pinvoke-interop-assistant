using PInvoke.Primitive.Bulk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive.Bulk
{
    public sealed class BulkExporter
    {
        private readonly IBulkWriter _writer;

        /// <summary>
        /// Tracks the symbols we are in the process of exporting.  It's null then no work is done, false when 
        /// in the middle of exporting and true when simply done.
        /// </summary>
        private readonly Dictionary<NativeName, bool?> _writtenMap = new Dictionary<NativeName, bool?>();
        private readonly List<NativeGlobalSymbol> _foundTypes = new List<NativeGlobalSymbol>();

        public BulkExporter(IBulkWriter writer)
        {
            _writer = writer;

            // TODO: Need to emit a version identifier
        }

        public void Write(INativeSymbolLookup lookup)
        {
            foreach (var name in lookup.NativeNames)
            {
                if (name.Kind == NativeNameKind.EnumValue)
                {
                    continue;
                }

                Write(lookup.GetGlobalSymbol(name));
            }
        }

        public void Write(NativeGlobalSymbol symbol)
        {
            MaybeWrite(symbol);
        }

        private void MaybeWrite(NativeGlobalSymbol symbol)
        {
            if (IsAnyWrittenState(symbol.Name))
            {
                return;
            }

            try
            {
                _writtenMap[symbol.Name] = false;
                WriteCore(symbol);
                _writtenMap[symbol.Name] = true;
            }
            catch
            {
                _writtenMap[symbol.Name] = null;
                throw;
            }

            // Once a symbol is completely exported need to process any types it encountered
            // during processing. 
            WriteFoundTypes();
        }

        private void WriteFoundTypes()
        {
            while (_foundTypes.Count > 0)
            {
                MaybeWrite(_foundTypes[0]);
                _foundTypes.RemoveAt(1);
            }
        }

        private void WriteCore(NativeGlobalSymbol symbol)
        {
            _writer.WriteItemStart();
            switch (symbol.Kind)
            {
                case NativeNameKind.Struct:
                case NativeNameKind.Union:
                    WriteDefined((NativeDefinedType)symbol.Symbol);
                    break;
                case NativeNameKind.FunctionPointer:
                    WriteFunctionPointer((NativeFunctionPointer)symbol.Symbol);
                    break;
                case NativeNameKind.Procedure:
                    WriteProcedure((NativeProcedure)symbol.Symbol);
                    break;
                case NativeNameKind.TypeDef:
                    WriteTypeDef((NativeTypeDef)symbol.Symbol);
                    break;
                case NativeNameKind.Constant:
                    WriteConstant((NativeConstant)symbol.Symbol);
                    break;
                case NativeNameKind.Enum:
                    WriteEnum((NativeEnum)symbol.Symbol);
                    break;
                case NativeNameKind.EnumValue:
                    throw new Exception("Enum values are not written directly but as a part of the owning enum");
                default:
                    throw Contract.CreateInvalidEnumValueException(symbol.Kind);
            }
            _writer.WriteItemEnd();
        }

        private void WriteDefined(NativeDefinedType nt)
        {
            Contract.Requires(nt.Kind == NativeSymbolKind.StructType || nt.Kind == NativeSymbolKind.UnionType);
            _writer.WriteNameKind(nt.NameKind);
            _writer.WriteString(nt.Name);

            // Write out the members as a unit.
            _writer.WriteInt32(nt.Members.Count);

            foreach (var member in nt.Members)
            {
                _writer.WriteString(member.Name);
                WriteTypeReference(member.NativeType);
            }
        }

        private void WriteEnum(NativeEnum e)
        {
            _writer.WriteNameKind(NativeNameKind.Enum);
            _writer.WriteString(e.Name);

            _writer.WriteInt32(e.Values.Count);
            foreach (var value in e.Values)
            {
                _writer.WriteString(value.Name);
                _writer.WriteString(value.Value.Expression);
            }
        }

        private void WriteConstant(NativeConstant c)
        {
            _writer.WriteNameKind(NativeNameKind.Constant);
            _writer.WriteString(c.Name);
            _writer.WriteString(c.Value.Expression);
            _writer.WriteInt32((int)c.ConstantKind);
        }

        private void WriteTypeReference(NativeType nt)
        {
            switch (nt.Kind)
            {
                case NativeSymbolKind.BuiltinType:
                    {
                        var b = (NativeBuiltinType)nt;
                        _writer.WriteSymbolKind(NativeSymbolKind.BuiltinType);
                        _writer.WriteInt32((int)b.BuiltinType);
                        break;
                    }
                case NativeSymbolKind.ArrayType:
                    {
                        var array = (NativeArray)nt;
                        _writer.WriteSymbolKind(NativeSymbolKind.ArrayType);
                        _writer.WriteInt32(array.ElementCount);
                        WriteTypeReference(array.RealType);
                        break;
                    }
                case NativeSymbolKind.PointerType:
                    {
                        var pointer = (NativePointer)nt;
                        _writer.WriteSymbolKind(NativeSymbolKind.PointerType);
                        WriteTypeReference(pointer.RealType);
                        break;
                    }
                case NativeSymbolKind.BitVectorType:
                    {
                        var v = (NativeBitVector)nt;
                        _writer.WriteSymbolKind(NativeSymbolKind.BitVectorType);
                        _writer.WriteInt32(v.Size);
                        break;
                    }
                case NativeSymbolKind.NamedType:
                    {
                        var n = (NativeNamedType)nt;
                        _writer.WriteSymbolKind(NativeSymbolKind.NamedType);
                        _writer.WriteString(n.Qualification);
                        _writer.WriteString(n.Name);
                        _writer.WriteBoolean(n.IsConst);
                        break;
                    }
                case NativeSymbolKind.OpaqueType:
                    {
                        _writer.WriteSymbolKind(NativeSymbolKind.OpaqueType);
                        break;
                    }
                case NativeSymbolKind.StructType:
                case NativeSymbolKind.UnionType:
                case NativeSymbolKind.EnumType:
                case NativeSymbolKind.FunctionPointer:
                case NativeSymbolKind.TypeDefType:
                    {
                        var defined = (NativeDefinedType)nt;
                        WriteTypeReference(new NativeNamedType(defined.Name));
                        _foundTypes.Add(new NativeGlobalSymbol(defined));
                        break;
                    }
                default:
                    Contract.ThrowInvalidEnumValue(nt.Kind);
                    break;
            }
        }

        private void WriteSal(NativeSalAttribute sal)
        {
            var list = sal.SalEntryList;
            _writer.WriteInt32(list.Count);

            foreach (var entry in list)
            {
                _writer.WriteInt32((int)entry.SalEntryType);
                _writer.WriteString(entry.Text);
            }
        }

        private void WriteSignature(NativeSignature sig)
        {
            WriteSal(sig.ReturnTypeSalAttribute);
            WriteTypeReference(sig.ReturnType);
            _writer.WriteInt32(sig.Parameters.Count);
            foreach (var p in sig.Parameters)
            {
                _writer.WriteString(p.Name);
                WriteTypeReference(p.NativeType);
            }
        }

        private void WriteFunctionPointer(NativeFunctionPointer ptr)
        {
            _writer.WriteNameKind(NativeNameKind.FunctionPointer);
            _writer.WriteString(ptr.Name);
            _writer.WriteInt32((int)ptr.CallingConvention);
            WriteSignature(ptr.Signature);
        }

        private void WriteTypeDef(NativeTypeDef typeDef)
        {
            _writer.WriteNameKind(NativeNameKind.TypeDef);
            _writer.WriteString(typeDef.Name);
            WriteTypeReference(typeDef.RealType);
        }

        private void WriteProcedure(NativeProcedure proc)
        {
            _writer.WriteNameKind(NativeNameKind.Procedure);
            _writer.WriteString(proc.Name);
            _writer.WriteInt32((int)proc.CallingConvention);
            _writer.WriteString(proc.DllName);
            WriteSignature(proc.Signature);
        }

        private bool IsWriting(NativeName name)
        {
            bool? value;
            return _writtenMap.TryGetValue(name, out value) && value == false;
        }

        private bool IsWritten(NativeName name)
        {
            bool? value;
            return _writtenMap.TryGetValue(name, out value) && value == true;
        }

        private bool IsAnyWrittenState(NativeName name)
        {
            bool? value;
            return _writtenMap.TryGetValue(name, out value) && value != null;
        }
    }
}
