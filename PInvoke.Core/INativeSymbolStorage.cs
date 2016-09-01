using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public interface INativeSymbolStorage : INativeSymbolLookup
    {
        void AddConstant(NativeConstant nConst);
        void AddDefinedType(NativeDefinedType definedNt);
        void AddTypeDef(NativeTypeDef typeDef);
        void AddProcedure(NativeProcedure proc);
    }
}
