using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PInvoke.Test
{
    public class BasicSymbolStorageTest
    {
        private readonly BasicSymbolStorage _storage = new BasicSymbolStorage();

        /// <summary>
        /// Presently enumerations are treated specially and storage will manually add 
        /// in all of their values automatically.
        /// 
        /// It's probably better if this wasn't the design but for now need to validate
        /// that it functions.
        /// https://github.com/jaredpar/pinvoke/issues/16
        /// </summary>
        [Fact]
        public void EnumAdd()
        {
            var enumeration = new NativeEnum("e");
            enumeration.AddValue("v1", "1");
            enumeration.AddValue("v2", "2");

            _storage.AddDefinedType(enumeration);
            Assert.True(_storage.Contains("v1"));
            Assert.True(_storage.Contains("v2"));
        }
    }
}
