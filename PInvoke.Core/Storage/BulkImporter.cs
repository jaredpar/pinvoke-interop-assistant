using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Storage
{
    public sealed class BulkImporter
    {
        private readonly IBulkReader _reader;

        private BulkImporter(IBulkReader reader)
        {
            _reader = reader;
        }

        public static BasicSymbolStorage Import(IBulkReader reader)
        {
            var importer = new BulkImporter(reader);
            return importer.Go();
        }

        private BasicSymbolStorage Go()
        {
            var storage = new BasicSymbolStorage();
            while (!_reader.IsDone())
            {
                var symbol = Import();
                storage.Add(symbol);
            }

            return storage;
        }

        private NativeGlobalSymbol Import()
        {
            _reader.ReadItemStart();
            try
            {
                var kind = _reader.ReadNameKind();
                switch (kind)
                {
                    case NativeNameKind.Struct:
                    case NativeNameKind.Union:
                        return ImportStructOrUnion(kind);
                    case NativeNameKind.Enum:
                        return ImportEnum();
                    case NativeNameKind.FunctionPointer:
                        return ImportFunctionPointer();
                    case NativeNameKind.Procedure:
                        return ImportProcedure();
                    case NativeNameKind.TypeDef:
                        return ImportTypeDef();
                    case NativeNameKind.Constant:
                        return ImportConstant();
                    case NativeNameKind.EnumValue:
                        throw new Exception("Enum values are imported as a part of the enclosing enum");
                    default:
                        throw Contract.CreateInvalidEnumValueException(kind);
                }
            }
            finally
            {
                _reader.ReadItemEnd();
            }
        }

        private NativeType ImportTypeReference()
        {
            var kind = _reader.ReadSymbolKind();
            switch (kind)
            {
                case NativeSymbolKind.ArrayType:
                    {
                        var elementCount = _reader.ReadInt32();
                        var type = ImportTypeReference();
                        return new NativeArray(type, elementCount);
                    }
                case NativeSymbolKind.PointerType:
                    {
                        return new NativePointer(ImportTypeReference());
                    }
                case NativeSymbolKind.BuiltinType:
                    {
                        var bt = (BuiltinType)_reader.ReadInt32();
                        return new NativeBuiltinType(bt);
                    }
                case NativeSymbolKind.BitVectorType:
                    {
                        var count = _reader.ReadInt32();
                        return new NativeBitVector(count);
                    }
                case NativeSymbolKind.NamedType:
                    {
                        var qualification = _reader.ReadString();
                        var name = _reader.ReadString();
                        var isConst = _reader.ReadBoolean();
                        return new NativeNamedType(qualification: qualification, name: name, isConst: isConst);
                    }
                case NativeSymbolKind.OpaqueType:
                    {
                        return new NativeOpaqueType();
                    }
                default:
                    throw Contract.CreateInvalidEnumValueException(kind);
            }
        }

        private NativeGlobalSymbol ImportTypeDef()
        {
            var name = _reader.ReadString();
            var type = ImportTypeReference();
            var typeDef = new NativeTypeDef(name, type);
            return new NativeGlobalSymbol(typeDef);
        }

        private NativeGlobalSymbol ImportConstant()
        {
            var name = _reader.ReadString();
            var value = _reader.ReadString();
            var kind = (ConstantKind)_reader.ReadInt32();
            var constant = new NativeConstant(name, value, kind);
            return new NativeGlobalSymbol(constant);
        }

        private NativeGlobalSymbol ImportStructOrUnion(NativeNameKind kind)
        {
            Contract.Requires(kind == NativeNameKind.Struct || kind == NativeNameKind.Union);
            var name = _reader.ReadString();
            var nt = kind == NativeNameKind.Struct
                ? (NativeDefinedType)new NativeStruct(name)
                : new NativeUnion(name);

            var count = _reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var memberName = _reader.ReadString();
                var memberType = ImportTypeReference();
                nt.Members.Add(new NativeMember(memberName, memberType));
            }

            return new NativeGlobalSymbol(nt);
        }

        private NativeGlobalSymbol ImportEnum()
        {
            var e = new NativeEnum(_reader.ReadString());
            var count = _reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var name = _reader.ReadString();
                var value = _reader.ReadString();
                e.AddValue(name, value);
            }

            return new NativeGlobalSymbol(e);
        }

        private NativeSalAttribute ImportSalAttribute()
        {
            var sal = new NativeSalAttribute();
            var count = _reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var type = (SalEntryType)_reader.ReadInt32();
                var text = _reader.ReadString();
                var entry = new NativeSalEntry(type, text);
                sal.SalEntryList.Add(entry);
            }
            return sal;
        }

        private NativeSignature ImportSignature()
        {
            var sig = new NativeSignature();
            sig.ReturnTypeSalAttribute = ImportSalAttribute();
            sig.ReturnType = ImportTypeReference();

            var count = _reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var name = _reader.ReadString();
                var type = ImportTypeReference();
                sig.Parameters.Add(new NativeParameter(name, type));
            }

            return sig;
        }

        private NativeGlobalSymbol ImportFunctionPointer()
        {
            var ptr = new NativeFunctionPointer(_reader.ReadString());
            ptr.CallingConvention = (NativeCallingConvention)_reader.ReadInt32();
            ptr.Signature = ImportSignature();
            return new NativeGlobalSymbol(ptr);
        }

        private NativeGlobalSymbol ImportProcedure()
        {
            var proc = new NativeProcedure(_reader.ReadString());
            proc.CallingConvention = (NativeCallingConvention)_reader.ReadInt32();
            proc.DllName = _reader.ReadString();
            proc.Signature = ImportSignature();
            return new NativeGlobalSymbol(proc);
        }
    }
}
