using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public interface INativeSymbolStorage : INativeSymbolLookup
    {
        void Add(NativeGlobalSymbol symbol);
    }
}
