using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PInvoke.Roslyn.UnitTests
{
    public class CSharpNativeFactoryTest
    {
        private readonly CSharpNativeFactory _factory = new CSharpNativeFactory();

        public sealed class StructTest : CSharpNativeFactoryTest
        {
            [Fact]
            public void Empty()
            {
                var ns = new NativeStruct("test");
                var code = _factory.GenerateStruct(ns);
                Assert.Equal("", code.ToFullString());
            }
        }
    }
}
