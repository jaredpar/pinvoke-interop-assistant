using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Storage
{
    public sealed class BulkExporter
    {
        private readonly IBulkWriter writer;

        /// <summary>
        /// Tracks the symbols we are in the process of exporting.  It's null then no work is done, false when 
        /// in the middle of exporting and true when simply done.
        /// </summary>
        private readonly Dictionary<NativeName, bool?> writtenMap = new Dictionary<NativeName, bool?>();
        private readonly List<NativeGlobalSymbol> foundTypes = new List<NativeGlobalSymbol>();

        public BulkExporter(IBulkWriter writer)
        {
            this.writer = writer;

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
                writtenMap[symbol.Name] = false;
                WriteCore(symbol);
                writtenMap[symbol.Name] = true;
            }
            catch
            {
                writtenMap[symbol.Name] = null;
                throw;
            }

            // Once a symbol is completely exported need to process any types it encountered
            // during processing. 
            WriteFoundTypes();
        }

        private void WriteFoundTypes()
        {
            while (foundTypes.Count > 0)
            {
                MaybeWrite(foundTypes[0]);
                foundTypes.RemoveAt(1);
            }
        }

        private void WriteCore(NativeGlobalSymbol symbol)
        {
            writer.WriteItemStart();
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
            writer.WriteItemEnd();
        }

        private void WriteDefined(NativeDefinedType nt)
        {
            Contract.Requires(nt.Kind == NativeSymbolKind.StructType || nt.Kind == NativeSymbolKind.UnionType);
            writer.WriteNameKind(nt.NameKind);
            writer.WriteString(nt.Name);

            // Write out the members as a unit.
            writer.WriteInt32(nt.Members.Count);

            foreach (var member in nt.Members)
            {
                writer.WriteString(member.Name);
                WriteTypeReference(member.NativeType);
            }
        }

        private void WriteEnum(NativeEnum e)
        {
            writer.WriteNameKind(NativeNameKind.Enum);
            writer.WriteString(e.Name);

            writer.WriteInt32(e.Values.Count);
            foreach (var value in e.Values)
            {
                writer.WriteString(value.Name);
                writer.WriteString(value.Value.Expression);
            }
        }

        private void WriteConstant(NativeConstant c)
        {
            writer.WriteNameKind(NativeNameKind.Constant);
            writer.WriteString(c.Name);
            writer.WriteString(c.Value.Expression);
            writer.WriteInt32((int)c.ConstantKind);
        }

        private void WriteTypeReference(NativeType nt)
        {
            switch (nt.Kind)
            {
                case NativeSymbolKind.BuiltinType:
                    {
                        var b = (NativeBuiltinType)nt;
                        writer.WriteSymbolKind(NativeSymbolKind.BuiltinType);
                        writer.WriteInt32((int)b.BuiltinType);
                        break;
                    }
                case NativeSymbolKind.ArrayType:
                    {
                        var array = (NativeArray)nt;
                        writer.WriteSymbolKind(NativeSymbolKind.ArrayType);
                        writer.WriteInt32(array.ElementCount);
                        WriteTypeReference(array.RealType);
                        break;
                    }
                case NativeSymbolKind.PointerType:
                    {
                        var pointer = (NativePointer)nt;
                        writer.WriteSymbolKind(NativeSymbolKind.PointerType);
                        WriteTypeReference(pointer.RealType);
                        break;
                    }
                case NativeSymbolKind.BitVectorType:
                    {
                        var v = (NativeBitVector)nt;
                        writer.WriteSymbolKind(NativeSymbolKind.BitVectorType);
                        writer.WriteInt32(v.Size);
                        break;
                    }
                case NativeSymbolKind.NamedType:
                    {
                        var n = (NativeNamedType)nt;
                        writer.WriteSymbolKind(NativeSymbolKind.NamedType);
                        writer.WriteString(n.Qualification);
                        writer.WriteString(n.Name);
                        writer.WriteBoolean(n.IsConst);
                        break;
                    }
                case NativeSymbolKind.OpaqueType:
                    {
                        writer.WriteSymbolKind(NativeSymbolKind.OpaqueType);
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
                        foundTypes.Add(new NativeGlobalSymbol(defined));
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
            writer.WriteInt32(list.Count);

            foreach (var entry in list)
            {
                writer.WriteInt32((int)entry.SalEntryType);
                writer.WriteString(entry.Text);
            }
        }

        private void WriteSignature(NativeSignature sig)
        {
            WriteSal(sig.ReturnTypeSalAttribute);
            WriteTypeReference(sig.ReturnType);
            writer.WriteInt32(sig.Parameters.Count);
            foreach (var p in sig.Parameters)
            {
                writer.WriteString(p.Name);
                WriteTypeReference(p.NativeType);
            }
        }

        private void WriteFunctionPointer(NativeFunctionPointer ptr)
        {
            writer.WriteNameKind(NativeNameKind.FunctionPointer);
            writer.WriteString(ptr.Name);
            writer.WriteInt32((int)ptr.CallingConvention);
            WriteSignature(ptr.Signature);
        }

        private void WriteTypeDef(NativeTypeDef typeDef)
        {
            writer.WriteNameKind(NativeNameKind.TypeDef);
            writer.WriteString(typeDef.Name);
            WriteTypeReference(typeDef.RealType);
        }

        private void WriteProcedure(NativeProcedure proc)
        {
            writer.WriteNameKind(NativeNameKind.Procedure);
            writer.WriteString(proc.Name);
            writer.WriteInt32((int)proc.CallingConvention);
            writer.WriteString(proc.DllName);
            WriteSignature(proc.Signature);
        }

        private bool IsWriting(NativeName name)
        {
            return writtenMap.TryGetValue(name, out bool? value) && value == false;
        }

        private bool IsWritten(NativeName name)
        {
            return writtenMap.TryGetValue(name, out bool? value) && value == true;
        }

        private bool IsAnyWrittenState(NativeName name)
        {
            return writtenMap.TryGetValue(name, out bool? value) && value != null;
        }
    }
}
