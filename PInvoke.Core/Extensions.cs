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
        /// Try and load a type by it's name in this lookup.
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
            if (lookup.TryFindTypeDef(name, out typedef))
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

        /// <summary>
        /// Find all NativeEnum which have a value of this name in this lookup.
        /// </summary>
        public static bool TryFindEnumByValueName(this INativeSymbolLookup lookup, string enumValueName, out List<NativeDefinedType> enumTypes)
        {
            enumTypes = new List<NativeDefinedType>();
            foreach (var name in lookup.NativeNames.Where(x => x.Kind == NativeNameKind.Enum))
            {
                NativeDefinedType nt;
                if (!lookup.TryFindDefined(name.Name, out nt))
                {
                    continue;
                }

                var e = (NativeEnum)nt;
                if (e.Values.Any(x => x.Name == enumValueName))
                {
                    enumTypes.Add(nt);
                }
            }

            return enumTypes.Count > 0;
        }

        public static bool TryFind<T>(this INativeSymbolLookup lookup, string name, out T symbol)
            where T: NativeSymbol
        {
            NativeDefinedType nt;
            if (lookup.TryFindDefined(name, out nt))
            {
                symbol = nt as T;
                return symbol != null;
            }

            NativeTypeDef typeDef;
            if (lookup.TryFindTypeDef(name, out typeDef))
            {
                symbol = typeDef as T;
                return symbol != null;
            }

            NativeProcedure proc;
            if (lookup.TryFindProcedure(name, out proc))
            {
                symbol = proc as T;
                return symbol != null;
            }

            NativeConstant constant;
            if (lookup.TryFindConstant(name, out constant))
            {
                symbol = constant as T;
                return symbol != null;
            }

            symbol = null;
            return false;
        }

        public static bool TryFindType(this INativeSymbolLookup lookup, string name, out NativeType type)
        {
            NativeDefinedType nt;
            if (lookup.TryFindDefined(name, out nt))
            {
                type = nt;
                return true;
            }

            NativeTypeDef typeDef;
            if (lookup.TryFindTypeDef(name, out typeDef))
            {
                type = typeDef;
                return true;
            }

            type = null;
            return false;
        }

        public static bool TryFindEnumValue(this INativeSymbolLookup lookup, string name, out NativeEnumValue value)
        {
            NativeEnum enumeration;
            return lookup.TryFindEnumValue(name, out enumeration, out value);
        }

        public static bool TryFindValue(this INativeSymbolLookup lookup, string name, out NativeSymbol symbol)
        {
            NativeConstant constant;
            if (lookup.TryFindConstant(name, out constant))
            {
                symbol = constant;
                return true;
            }

            NativeEnumValue value;
            if (lookup.TryFindEnumValue(name, out value))
            {
                symbol = value;
                return true;
            }

            symbol = null;
            return false;
        }
    }
}
