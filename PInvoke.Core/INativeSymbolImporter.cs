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
        bool TryImport(string name, out NativeGlobalSymbol symbol);
        bool TryImport(NativeName name, out NativeGlobalSymbol symbol);

        // CTODO: move to extension methods
        bool TryImportDefined(string name, out NativeDefinedType nt);
        bool TryImportTypedef(string name, out NativeTypeDef nt);
        bool TryImportProcedure(string name, out NativeProcedure proc);
        bool TryImportConstant(string name, out NativeConstant constant);

        // CTODO: No way to guarantee enumeration has value as a reference in Values.  Perhaps just return
        // enumeration here and let the caller dig in for the value.
        bool TryImportEnumValue(string name, out NativeEnum enumeration, out NativeEnumValue value);
    }
}
