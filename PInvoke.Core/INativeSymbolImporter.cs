using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    /// <summary>
    /// Responsible for importing symbol definitions with the given names.  The values returned
    /// from these can be freshly allocated on every call.
    /// </summary>
    public interface INativeSymbolImporter
    {
        IEnumerable<NativeName> Names { get; }

        bool TryImport(string name, out NativeGlobalSymbol symbol);
        bool TryImport(NativeName name, out NativeGlobalSymbol symbol);
    }
}
