using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive.Bulk
{
    public static partial class BulkUtil
    {
        private sealed class Reader
        {
            private readonly IBulkReader _reader;

            internal Reader(IBulkReader reader)
            {
                _reader = reader;
            }

            internal BasicPrimitiveStorage Read()
            {
                var storage = new BasicPrimitiveStorage();
                while (!_reader.IsDone())
                {
                    _reader.ReadItemStart();
                    var kind = (ItemKind)_reader.ReadInt32();
                    switch (kind)
                    {
                        case ItemKind.EnumValueData:
                            storage.Write(ReadEnumValueData());
                            break;
                        case ItemKind.SignatureData:
                            storage.Write(ReadSignatureData());
                            break;
                        case ItemKind.FunctionPointerData:
                            storage.Write(ReadFunctionPointerData());
                            break;
                        case ItemKind.TypeData:
                            storage.Write(ReadTypeData());
                            break;
                        case ItemKind.ConstantData:
                            storage.Write(ReadConstantData());
                            break;
                        case ItemKind.TypeDefData:
                            storage.Write(ReadTypeDefData());
                            break;
                        case ItemKind.ProcedureData:
                            storage.Write(ReadProcedureData());
                            break;
                        case ItemKind.ParameterData:
                            storage.Write(ReadParameterData());
                            break;
                        case ItemKind.SalEntryData:
                            storage.Write(ReadSalEntryData());
                            break;
                        case ItemKind.MemberData:
                            storage.Write(ReadMemberData());
                            break;
                        case ItemKind.SymbolId:
                            storage.Write(ReadSymbolId());
                            break;
                        default:
                            Contract.ThrowInvalidEnumValue(kind);
                            break;
                    }

                    _reader.ReadItemEnd();
                }

                return storage;
            }

            private PrimitiveSymbolId ReadSymbolIdCore()
            {
                return new PrimitiveSymbolId(
                    _reader.ReadString(),
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

            private PrimitiveSalEntryData ReadSalEntryData()
            {
                var data = new PrimitiveSalEntryData(
                    ReadSimpleIdCore(),
                    _reader.ReadInt32(),
                    (SalEntryType)_reader.ReadInt32(),
                    _reader.ReadString());
                return data;
            }

            private PrimitiveParameterData ReadParameterData()
            {
                var data = new PrimitiveParameterData(
                    ReadSimpleIdCore(),
                    _reader.ReadInt32(),
                    _reader.ReadString(),
                    ReadTypeIdCore());
                return data;
            }

            private PrimitiveProcedureData ReadProcedureData()
            {
                var data = new PrimitiveProcedureData(
                    ReadSymbolIdCore(),
                    (NativeCallingConvention)_reader.ReadInt32(),
                    ReadSimpleIdCore(),
                    _reader.ReadString());
                return data;
            }

            private PrimitiveTypeDefData ReadTypeDefData()
            {
                var data = new PrimitiveTypeDefData(
                    ReadSymbolIdCore(),
                    ReadTypeIdCore());
                return data;
            }

            private PrimitiveConstantData ReadConstantData()
            {
                var data = new PrimitiveConstantData(
                    ReadSymbolIdCore(),
                    _reader.ReadString(),
                    (ConstantKind)_reader.ReadInt32());
                return data;
            }

            private PrimitiveTypeData ReadTypeData()
            {
                var data = new PrimitiveTypeData(
                    ReadSimpleIdCore(),
                    (NativeSymbolKind)_reader.ReadInt32(),
                    _reader.ReadInt32(),
                    ReadTypeIdCore(),
                    (BuiltinType)_reader.ReadInt32(),
                    _reader.ReadString(),
                    _reader.ReadString(),
                    _reader.ReadBoolean());
                return data;
            }

            private PrimitiveFunctionPointerData ReadFunctionPointerData()
            {
                var data = new PrimitiveFunctionPointerData(
                    ReadSymbolIdCore(),
                    (NativeCallingConvention)_reader.ReadInt32(),
                    ReadSimpleIdCore());
                return data;
            }

            private PrimitiveSignatureData ReadSignatureData()
            {
                var data = new PrimitiveSignatureData(
                    ReadSimpleIdCore(),
                    ReadTypeIdCore(),
                    ReadSimpleIdCore());
                return data;
            }

            private PrimitiveEnumValueData ReadEnumValueData()
            {
                var data = new PrimitiveEnumValueData(
                    _reader.ReadString(),
                    _reader.ReadString(),
                    ReadSymbolIdCore());
                return data;
            }

            private PrimitiveMemberData ReadMemberData()
            {
                var data = new PrimitiveMemberData(
                    _reader.ReadString(),
                    ReadTypeIdCore(),
                    ReadSymbolIdCore());
                return data;
            }

            private PrimitiveSymbolId ReadSymbolId()
            {
                var data = ReadSymbolIdCore();
                return data;
            }
        }
    }
}
