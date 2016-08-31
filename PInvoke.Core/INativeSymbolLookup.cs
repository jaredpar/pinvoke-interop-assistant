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
        // CTODO: Need to move this to a different layer.  Not all storage can efficiently return
        // blocks of APIs.  Could be hugely allocating.
        // CTODO: need properties for rest of the types
        // CTODO: probably return an enumeration of SymbolId or such.
        IEnumerable<NativeEnum> NativeEnums { get; }

        bool TryFindDefined(string name, out NativeDefinedType nt);
        bool TryFindTypedef(string name, out NativeTypeDef nt);
        bool TryFindProcedure(string name, out NativeProcedure proc);
        bool TryFindConstant(string name, out NativeConstant nConst);
    }
}
