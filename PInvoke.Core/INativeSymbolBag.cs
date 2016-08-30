using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public interface INativeSymbolBag : INativeSymbolStorage
    {
        bool TryResolveSymbolsAndValues(ErrorProvider ep = null);
    }
}
