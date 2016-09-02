using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    internal sealed class EmptyNativeSymbolBag : INativeSymbolLookup
    {
        internal static readonly EmptyNativeSymbolBag Instance = new EmptyNativeSymbolBag();

        public IEnumerable<NativeName> NativeNames => new NativeName[] { };

        public bool TryGetGlobalSymbol(string name, out NativeGlobalSymbol symbol)
        {
            symbol = default(NativeGlobalSymbol);
            return false;
        }

        public bool TryGetGlobalSymbol(NativeName name, out NativeGlobalSymbol symbol)
        {
            symbol = default(NativeGlobalSymbol);
            return false;
        }
    }
}
