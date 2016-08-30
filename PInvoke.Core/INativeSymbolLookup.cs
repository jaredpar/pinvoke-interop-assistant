using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    // CTODO: should lookups be hierarchical? 
    public interface INativeSymbolLookup
    {
        bool TryFindDefined(string name, out NativeDefinedType nt);
        bool TryFindTypedef(string name, out NativeTypeDef nt);
        bool TryFindProcedure(string name, out NativeProcedure proc);
        bool TryFindConstant(string name, out NativeConstant nConst);

        // CTODO: Probably should be an extension method as well.  
        bool TryFindEnumByValueName(string enumValueName, out List<NativeDefinedType> enumTypes);
    }
}
