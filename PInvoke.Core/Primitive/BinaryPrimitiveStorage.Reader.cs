using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public static partial class BinaryPrimitiveStorage
    {
        private sealed class Reader
        {
            private readonly BinaryReader _reader;
            private readonly BasicPrimitiveStorage _storage;

            internal Reader(BinaryReader reader, BasicPrimitiveStorage storage)
            {
                _reader = reader;
                _storage = storage;
            }

            internal void Go()
            {
                while (_reader.PeekChar() != -1)
                {
                    var kind = (ItemKind)_reader.ReadInt32();
                    switch (kind)
                    {
                        case ItemKind.EnumValueData:
                            ReadEnumValueData();
                            break;
                        case ItemKind.SignatureData:
                            ReadSignatureData();
                            break;
                        case ItemKind.FunctionPointerData:
                            ReadFunctionPointerData();
                            break;
                        case ItemKind.TypeData:
                            ReadTypeData();
                            break;
                        case ItemKind.ConstantData:
                            ReadConstantData();
                            break;
                        case ItemKind.TypeDefData:
                            ReadTypeDefData();
                            break;
                        case ItemKind.ProcedureData:
                            ReadProcedureData();
                            break;
                        case ItemKind.ParameterData:
                            ReadParameterData();
                            break;
                        case ItemKind.SalEntryData:
                            ReadSalEntryData();
                            break;
                        case ItemKind.MemberData:
                            ReadMemberData();
                            break;
                        case ItemKind.SymbolId:
                            ReadSymbolId();
                            break;
                        default:
                            Contract.ThrowInvalidEnumValue(kind);
                            break;
                    }
                }
            }

            private PrimitiveSymbolId ReadSymbolIdCore()
            {
                return new PrimitiveSymbolId(
                    ReadStringCore(),
                    (NativeSymbolKind)_reader.ReadInt32());
            }

            private PrimitiveSimpleId ReadSimpleIdCore()
            {
                return new PrimitiveSimpleId(_reader.ReadInt32());
            }

            private PrimitiveTypeId ReadTypeIdCore()
            {
                return new PrimitiveTypeId(ReadSymbolIdCore(), ReadSimpleIdCore());
            }

            private string ReadStringCore()
            {
                var hasString = _reader.ReadBoolean();
                return hasString ? _reader.ReadString() : null;
            }

            private void ReadSalEntryData()
            {
                var data = new PrimitiveSalEntryData(
                    ReadSimpleIdCore(),
                    _reader.ReadInt32(),
                    (SalEntryType)_reader.ReadInt32(),
                    ReadStringCore());
                _storage.Write(data);
            }

            private void ReadParameterData()
            {
                var data = new PrimitiveParameterData(
                    ReadSimpleIdCore(),
                    _reader.ReadInt32(),
                    ReadStringCore(),
                    ReadTypeIdCore());
                _storage.Write(data);
            }

            private void ReadProcedureData()
            {
                var data = new PrimitiveProcedureData(
                    ReadSymbolIdCore(),
                    (NativeCallingConvention)_reader.ReadInt32(),
                    ReadSimpleIdCore(),
                    ReadStringCore());
                _storage.Write(data);
            }

            private void ReadTypeDefData()
            {
                var data = new PrimitiveTypeDefData(
                    ReadSymbolIdCore(),
                    ReadTypeIdCore());
                _storage.Write(data);
            }

            private void ReadConstantData()
            {
                var data = new PrimitiveConstantData(
                    ReadSymbolIdCore(),
                    ReadStringCore(),
                    (ConstantKind)_reader.ReadInt32());
                _storage.Write(data);
            }

            private void ReadTypeData()
            {
                var data = new PrimitiveTypeData(
                    ReadSimpleIdCore(),
                    (NativeSymbolKind)_reader.ReadInt32(),
                    _reader.ReadInt32(),
                    ReadTypeIdCore(),
                    (BuiltinType)_reader.ReadInt32(),
                    ReadStringCore(),
                    ReadStringCore(),
                    _reader.ReadBoolean());
                _storage.Write(data);
            }

            private void ReadFunctionPointerData()
            {
                var data = new PrimitiveFunctionPointerData(
                    ReadSymbolIdCore(),
                    (NativeCallingConvention)_reader.ReadInt32(),
                    ReadSimpleIdCore());
                _storage.Write(data);
            }

            private void ReadSignatureData()
            {
                var data = new PrimitiveSignatureData(
                    ReadSimpleIdCore(),
                    ReadTypeIdCore(),
                    ReadSimpleIdCore());
                _storage.Write(data);
            }

            private void ReadEnumValueData()
            {
                var data = new PrimitiveEnumValueData(
                    ReadStringCore(),
                    ReadStringCore(),
                    ReadSymbolIdCore());
                _storage.Write(data);
            }

            private void ReadMemberData()
            {
                var data = new PrimitiveMemberData(
                    ReadStringCore(),
                    ReadTypeIdCore(),
                    ReadSymbolIdCore());
                _storage.Write(data);
            }

            private void ReadSymbolId()
            {
                var data = ReadSymbolIdCore();
                _storage.Write(data);
            }
        }
    }
}
