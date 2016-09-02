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

        bool TryFindDefined(string name, out NativeDefinedType nt);
        bool TryFindTypeDef(string name, out NativeTypeDef nt);
        bool TryFindProcedure(string name, out NativeProcedure proc);
        bool TryFindConstant(string name, out NativeConstant constant);
        bool TryFindEnumValue(string name, out NativeEnum enumeration, out NativeEnumValue value);
    }
}
