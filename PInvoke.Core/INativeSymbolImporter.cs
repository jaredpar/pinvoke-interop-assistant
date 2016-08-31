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
        bool TryImportDefined(string name, out NativeDefinedType nt);
        bool TryImportTypedef(string name, out NativeTypeDef nt);
        bool TryImportProcedure(string name, out NativeProcedure proc);
        bool TryImportConstant(string name, out NativeConstant nConst);
    }
}
