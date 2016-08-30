using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    internal sealed class EmptyNativeSymbolBag : INativeSymbolLookup
    {
        internal static readonly EmptyNativeSymbolBag Instance = new EmptyNativeSymbolBag();

        public IEnumerable<NativeEnum> NativeEnums => new NativeEnum[] { };

        public bool TryFindByName(string name, out NativeType nt)
        {
            nt = null;
            return false;
        }

        public bool TryFindConstant(string name, out NativeConstant nConst)
        {
            nConst = null;
            return false;
        }

        public bool TryFindDefined(string name, out NativeDefinedType nt)
        {
            nt = null;
            return false;
        }

        public bool TryFindEnumByValueName(string enumValueName, out List<NativeDefinedType> enumTypes)
        {
            enumTypes = null;
            return false;
        }

        public bool TryFindProcedure(string name, out NativeProcedure proc)
        {
            proc = null;
            return false;
        }

        public bool TryFindTypedef(string name, out NativeTypeDef nt)
        {
            nt = null;
            return false;
        }
    }
}
