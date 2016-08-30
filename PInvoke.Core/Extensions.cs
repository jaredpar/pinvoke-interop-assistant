using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public static class Extensions
    {
        /// <summary>
        /// Try and load a type by it's name
        /// </summary>
        public static bool TryFindByName(this INativeSymbolLookup lookup, string name, out NativeType nt)
        {
            NativeDefinedType definedNt = null;
            if (lookup.TryFindDefined(name, out definedNt))
            {
                nt = definedNt;
                return true;
            }

            NativeTypeDef typedef = null;
            if (lookup.TryFindTypedef(name, out typedef))
            {
                nt = typedef;
                return true;
            }

            // Lastly try and load the Builtin types
            NativeBuiltinType bt = null;
            if (NativeBuiltinType.TryConvertToBuiltinType(name, out bt))
            {
                nt = bt;
                return true;
            }

            nt = null;
            return false;
        }
    }
}
