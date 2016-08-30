using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public interface INativeSymbolBag
    {
        bool TryLoadDefined(string name, out NativeDefinedType nt);
        bool TryLoadTypedef(string name, out NativeTypeDef nt);
        bool TryLoadProcedure(string name, out NativeProcedure proc);
        bool TryLoadConstant(string name, out NativeConstant nConst);
        bool TryLoadByName(string name, out NativeType nt);
        bool TryLoadEnumByValueName(string enumValueName, out List<NativeDefinedType> enumTypes);
    }
}
