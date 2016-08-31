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
        private void RoundTrip(NativeDefinedType nt)
        {
            var storage = new PrimitiveStorage();
            var exporter = new PrimitiveExporter(storage);
            exporter.Export(nt);
            var importer = new PrimitiveImporter(storage);
            NativeDefinedType other;
            Assert.True(importer.TryLoadDefined(nt.Name, out other));
            Assert.Equal(SymbolPrinter.Convert(nt), SymbolPrinter.Convert(other));
        }

        [Fact]
        public void StructSimple()
        {
            var ns = new NativeStruct("s1");
            RoundTrip(ns);
        }
    }
}
