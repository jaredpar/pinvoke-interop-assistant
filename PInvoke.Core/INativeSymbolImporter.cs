using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    internal interface INativeSymbolImporter
    {
        bool TryImportDefined(string name, out NativeDefinedType nt);
        bool TryImportTypedef(string name, out NativeTypeDef nt);
        bool TryImportProcedure(string name, out NativeProcedure proc);
        bool TryImportConstant(string name, out NativeConstant nConst);
    }
}
