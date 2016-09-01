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
    public abstract class PrimitiveRoundTripTests
    {
        protected abstract IPrimitiveWriter CreateWriter();

        protected abstract IPrimitiveReader CreateReader();

        private void TestRoundTrip(NativeSymbol symbol)
        {
            var exporter = new PrimitiveExporter(CreateWriter());
            exporter.Export(symbol);
            var importer = new PrimitiveImporter(CreateReader());
            NativeSymbol other;
            Assert.True(importer.TryImport(symbol.Name, out other));
            Assert.Equal(SymbolPrinter.Convert(symbol), SymbolPrinter.Convert(other));
        }

        private void TestRoundTrip(NativeProcedure p)
        {
            var exporter = new PrimitiveExporter(CreateWriter());
            exporter.Export(p);
            var importer = new PrimitiveImporter(CreateReader());
            NativeProcedure other;
            Assert.True(importer.TryImportProcedure(p.Name, out other));
            Assert.Equal(SymbolPrinter.Convert(p), SymbolPrinter.Convert(other));
        }

        public sealed class BasicPrimitiveStorageTests : PrimitiveRoundTripTests
        {
            private readonly BasicPrimitiveStorage _storage = new BasicPrimitiveStorage();

            protected override IPrimitiveReader CreateReader() => _storage;

            protected override IPrimitiveWriter CreateWriter() => _storage;
        }

        public sealed class BinaryPrimitiveStorageTests : PrimitiveRoundTripTests
        {
            private readonly MemoryStream _stream = new MemoryStream();

            protected override IPrimitiveReader CreateReader()
            {
                _stream.Position = 0;
                return BinaryPrimitiveStorage.CreateReader(_stream);
            }

            protected override IPrimitiveWriter CreateWriter()
            {
                return BinaryPrimitiveStorage.CreateWriter(_stream);
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
            e.Values.Add(new NativeEnumValue("v1"));
            e.Values.Add(new NativeEnumValue("v2"));
            TestRoundTrip(e);
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
