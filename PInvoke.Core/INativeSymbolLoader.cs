using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    // CTOD: Rename to INativeSymbolImporter
    internal interface INativeSymbolLoader
    {
        bool TryLoadDefined(string name, out NativeDefinedType nt);
        bool TryLoadTypedef(string name, out NativeTypeDef nt);
        bool TryLoadProcedure(string name, out NativeProcedure proc);
        bool TryLoadConstant(string name, out NativeConstant nConst);
    }
}
