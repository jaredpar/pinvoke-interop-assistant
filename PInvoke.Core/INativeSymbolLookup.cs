using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    // CTODO: should lookups be hierarchical?  Yes very likely they should.  Let NativeSymbolBag 
    // do all of the resolution between the layers.
    public interface INativeSymbolLookup
    {
        // CTODO: need properties for rest of the types
        IEnumerable<NativeEnum> NativeEnums { get; }

        bool TryFindDefined(string name, out NativeDefinedType nt);
        bool TryFindTypedef(string name, out NativeTypeDef nt);
        bool TryFindProcedure(string name, out NativeProcedure proc);
        bool TryFindConstant(string name, out NativeConstant nConst);
    }
}
