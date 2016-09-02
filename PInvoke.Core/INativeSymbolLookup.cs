using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    /// <summary>
    /// Interface for looking up symbols. 
    ///
    /// Successive calls to TryFindDefined must produce objects which are reference equals.  If an 
    /// implementation cannot guarantee that it must implement <see cref="INativeSymbolImporter"/>.
    /// </summary>
    public interface INativeSymbolLookup
    {
        /// <summary>
        /// The set of all global names this lookup can provide.
        /// </summary>
        IEnumerable<NativeName> NativeNames { get; }

        /// <summary>
        /// Try and get a global symbol with the specified name and kind
        /// </summary>
        bool TryGetGlobalSymbol(NativeName name, out NativeGlobalSymbol symbol);

        /// <summary>
        /// Try and get a global symbol with a matching name
        /// </summary>
        bool TryGetGlobalSymbol(string name, out NativeGlobalSymbol symbol);
    }
}
