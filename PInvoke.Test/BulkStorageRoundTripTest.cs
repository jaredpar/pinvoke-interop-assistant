using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using PInvoke.Storage;

namespace PInvoke.Test
{
    public sealed class BulkStorageRoundTripTest
    {
        #region RoundTripUtil 

        private abstract class RoundTripUtil
        {
            internal abstract void Write(INativeSymbolLookup lookup);

            internal abstract BasicSymbolStorage Read();
        }

        private sealed class BasicSymbolStorageRoundTripUtil : RoundTripUtil
        {
            private readonly BasicSymbolStorage _storage = new BasicSymbolStorage();

            internal override BasicSymbolStorage Read() => _storage;

            internal override void Write(INativeSymbolLookup lookup)
            {
                foreach (var name in lookup.NativeNames)
                {
                    if (name.Kind == NativeNameKind.EnumValue)
                    {
                        continue;
                    }

                    _storage.Add(lookup.GetGlobalSymbol(name));
                }
            }
        }

        private sealed class BinaryRoundTripUtil : RoundTripUtil
        {
            private readonly MemoryStream _stream = new MemoryStream();

            internal override BasicSymbolStorage Read()
            {
                _stream.Position = 0;
                return StorageUtil.ReadBinary(_stream);
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

            internal override BasicSymbolStorage Read()
            {
                _stream.Position = 0;
                return StorageUtil.ReadCsv(_stream);
            }

            internal override void Write(INativeSymbolLookup lookup)
            {
                _stream.Position = 0;
                StorageUtil.WriteCsv(_stream, lookup);
            }
        }

        #endregion

        private readonly List<RoundTripUtil> _utilList = new List<RoundTripUtil>();

        public BulkStorageRoundTripTest()
        {
            _utilList.Add(new CsvRoundTripUtil());
            _utilList.Add(new BinaryRoundTripUtil());
            _utilList.Add(new BasicSymbolStorageRoundTripUtil());
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
                var storage = util.Read();
                foreach (var name in lookup.NativeNames)
                {
                    if (name.Kind == NativeNameKind.EnumValue)
                    {
                        continue;
                    }

                    var symbol = lookup.GetGlobalSymbol(name);
                    NativeGlobalSymbol other;

                    if (!storage.TryGetGlobalSymbol(name, out other))
                    {

                    }
                    Assert.True(storage.TryGetGlobalSymbol(name, out other));
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
