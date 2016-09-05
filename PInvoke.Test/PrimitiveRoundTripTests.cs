using PInvoke.Primitive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PInvoke.Test
{
    public sealed class PrimitiveRoundTripTests
    {
        #region RoundTripUtil 

        private abstract class RoundTripUtil
        {
            internal abstract void Write(INativeSymbolLookup lookup);

            internal abstract IPrimitiveReader CreateReader();
        }

        private sealed class BasicPrimitiveStorageRoundTripUtil : RoundTripUtil
        {
            private readonly BasicPrimitiveStorage _storage = new BasicPrimitiveStorage();

            internal override IPrimitiveReader CreateReader() => _storage;

            internal override void Write(INativeSymbolLookup lookup)
            {
                var exporter = new PrimitiveExporter(_storage);
                foreach (var name in lookup.NativeNames)
                {
                    if (name.Kind == NativeNameKind.EnumValue)
                    {
                        continue;
                    }

                    var symbol = lookup.GetGlobalSymbol(name);
                    exporter.Export(symbol.Symbol);
                }
            }
        }

        private sealed class BinaryRoundTripUtil : RoundTripUtil
        {
            private readonly MemoryStream _stream = new MemoryStream();

            internal override IPrimitiveReader CreateReader()
            {
                _stream.Position = 0;
                return StorageUtil.ReadBinaryPrimitive(_stream);
            }

            internal override void Write(INativeSymbolLookup lookup)
            {
                _stream.Position = 0;
                StorageUtil.WriteBinary(_stream, lookup);
            }
        }

        private sealed class CsvRoundTripUtil : RoundTripUtil
        {
            private readonly MemoryStream _stream = new MemoryStream();

            internal override IPrimitiveReader CreateReader()
            {
                _stream.Position = 0;
                return StorageUtil.ReadCsvPrimitive(_stream);
            }

            internal override void Write(INativeSymbolLookup lookup)
            {
                _stream.Position = 0;
                StorageUtil.WriteCsv(_stream, lookup);
            }
        }

        #endregion

        private readonly List<RoundTripUtil> _utilList = new List<RoundTripUtil>();

        public PrimitiveRoundTripTests()
        {
            _utilList.Add(new CsvRoundTripUtil());
            _utilList.Add(new BasicPrimitiveStorageRoundTripUtil());
            _utilList.Add(new BinaryRoundTripUtil());
        }

        private void TestRoundTrip(NativeSymbol symbol)
        {
            var storage = new BasicSymbolStorage();
            var name = NativeNameUtil.GetName(symbol);
            var globalSymbol = new NativeGlobalSymbol(name, symbol);
            storage.Add(globalSymbol);
            TestRoundTrip(storage);
        }

        private void TestRoundTrip(INativeSymbolLookup lookup)
        {
            foreach (var util in _utilList)
            {
                util.Write(lookup);
                var importer = new PrimitiveImporter(util.CreateReader());
                foreach (var name in lookup.NativeNames)
                {
                    if (name.Kind == NativeNameKind.EnumValue)
                    {
                        continue;
                    }

                    var symbol = lookup.GetGlobalSymbol(name);
                    NativeGlobalSymbol other;

                    if (!importer.TryImport(name, out other))
                    {

                    }

                    Assert.True(importer.TryImport(name, out other));
                    Assert.Equal(SymbolPrinter.Convert(symbol.Symbol), SymbolPrinter.Convert(other.Symbol));
                }
            }
        }

        [Fact]
        public void StructSimple()
        {
            var ns = new NativeStruct("s1");
            TestRoundTrip(ns);
        }

        [Fact]
        public void EnumSimple()
        {
            var ns = new NativeEnum("e1");
            TestRoundTrip(ns);
        }

        [Fact]
        public void EnumWithValues()
        {
            var e = new NativeEnum("e1");
            e.Values.Add(new NativeEnumValue("e1", "v1"));
            e.Values.Add(new NativeEnumValue("e1", "v2"));
            var storage = new BasicSymbolStorage();
            storage.AddEnumAndValues(e);
            TestRoundTrip(storage);
        }

        [Fact]
        public void UnionSimple()
        {
            var u = new NativeUnion("u1");
            TestRoundTrip(u);
        }

        [Fact]
        public void FunctionPointerSimple()
        {
            var p = new NativeFunctionPointer("ptr");
            p.CallingConvention = NativeCallingConvention.CDeclaration;
            p.Signature = new NativeSignature();
            p.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeInt32);
            TestRoundTrip(p);
        }

        [Fact]
        public void FunctionPointerReturnSal()
        {
            var p = new NativeFunctionPointer("ptr");
            p.CallingConvention = NativeCallingConvention.CDeclaration;
            p.Signature = new NativeSignature();
            p.Signature.ReturnTypeSalAttribute = new NativeSalAttribute();
            p.Signature.ReturnTypeSalAttribute.SalEntryList.Add(new NativeSalEntry(SalEntryType.NotNull));
            p.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeInt32);
            TestRoundTrip(p);
        }

        [Fact]
        public void ProcedureSimple()
        {
            var p = new NativeProcedure("ptr");
            p.CallingConvention = NativeCallingConvention.CDeclaration;
            p.Signature = new NativeSignature();
            p.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeInt32);
            TestRoundTrip(p);
        }

        [Fact]
        public void TypeDefSimple()
        {
            var td = new NativeTypeDef("a", BuiltinType.NativeByte);
            TestRoundTrip(td);
        }
    }
}
