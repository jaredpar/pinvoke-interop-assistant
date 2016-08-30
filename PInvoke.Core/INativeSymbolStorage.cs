using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public interface INativeSymbolStorage
    {
        void AddConstant(NativeConstant nConst);
        void AddDefinedType(NativeDefinedType definedNt);
        void AddTypedef(NativeTypeDef typeDef);
        void AddProcedure(NativeProcedure proc);
    }
}
