using PInvoke.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PInvoke.Test
{
    public class PrimitiveTests
    {
        public sealed class RoundTrip : PrimitiveTests
        {
            private void TestRoundTrip(NativeDefinedType nt)
            {
                var storage = new PrimitiveStorage();
                var exporter = new PrimitiveExporter(storage);
                exporter.Export(nt);
                var importer = new PrimitiveImporter(storage);
                NativeDefinedType other;
                Assert.True(importer.TryLoadDefined(nt.Name, out other));
                Assert.Equal(SymbolPrinter.Convert(nt), SymbolPrinter.Convert(other));
            }

            private void TestRoundTrip(NativeProcedure p)
            {
                var storage = new PrimitiveStorage();
                var exporter = new PrimitiveExporter(storage);
                exporter.Export(p);
                var importer = new PrimitiveImporter(storage);
                NativeProcedure other;
                Assert.True(importer.TryLoadProcedure(p.Name, out other));
                Assert.Equal(SymbolPrinter.Convert(p), SymbolPrinter.Convert(other));
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
        }
    }
}
