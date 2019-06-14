using PInvoke.Enums;
using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;
using PInvoke.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public static class Extensions
    {
        #region INativeSymbolLookup 

        public static bool Contains(this INativeSymbolLookup lookup, NativeName name)
        {
            NativeGlobalSymbol symbol;
            return lookup.TryGetGlobalSymbol(name, out symbol);
        }

        public static bool Contains(this INativeSymbolLookup lookup, string name)
        {
            NativeGlobalSymbol symbol;
            return lookup.TryGetGlobalSymbol(name, out symbol);
        }

        /// <summary>
        /// Do a lookup for a symbol with a specific name of the specified type.
        /// </summary>
        public static bool TryGetGlobalSymbol<T>(this INativeSymbolLookup lookup, NativeName name, out T symbol)
            where T : NativeSymbol
        {
            NativeGlobalSymbol globalSymbol;
            if (!lookup.TryGetGlobalSymbol(name, out globalSymbol))
            {
                symbol = null;
                return false;
            }

            symbol = globalSymbol.Symbol as T;
            return symbol != null;
        }

        /// <summary>
        /// Do a lookup for a symbol with a specific name of the specified type.
        /// </summary>
        public static bool TryGetGlobalSymbol<T>(this INativeSymbolLookup lookup, string name, out T symbol)
            where T : NativeSymbol
        {
            NativeGlobalSymbol globalSymbol;
            if (!lookup.TryGetGlobalSymbol(name, out globalSymbol))
            {
                symbol = null;
                return false;
            }

            symbol = globalSymbol.Symbol as T;
            return symbol != null;
        }

        /// <summary>
        /// Do a lookup for a symbol with a specific name of the specified type.
        /// </summary>
        internal static bool TryGetGlobalSymbolExhaustive(this INativeSymbolLookup lookup, string name, out NativeGlobalSymbol symbol)
        {
            foreach (var kind in Enum.GetValues(typeof(NativeNameKind)).Cast<NativeNameKind>())
            {
                var nativeName = new NativeName(name, kind);
                if (lookup.TryGetGlobalSymbol(nativeName, out symbol))
                {
                    return true;
                }
            }

            symbol = default(NativeGlobalSymbol);
            return false;
        }

        /// <summary>
        /// Try and find any global symbol with the specified name.
        /// </summary>
        public static bool TryGetType(this INativeSymbolLookup lookup, string name, out NativeType nt)
        {
            return lookup.TryGetGlobalSymbol(name, out nt);
        }

        public static bool TryGetValue(this INativeSymbolLookup lookup, string name, out NativeSymbol symbol) =>
            lookup.TryGetGlobalSymbol(new NativeName(name, NativeNameKind.Constant), out symbol) ||
            lookup.TryGetGlobalSymbol(new NativeName(name, NativeNameKind.EnumValue), out symbol);

        /// <summary>
        /// Find all NativeEnum which have a value of this name in this lookup.
        /// </summary>
        public static bool TryGetEnumByValueName(this INativeSymbolLookup lookup, string enumValueName, out NativeEnum enumeration)
        {
            NativeEnumValue value;
            return TryGetEnumByValueName(lookup, enumValueName, out enumeration, out value);
        }

        public static bool TryGetEnumByValueName(this INativeSymbolLookup lookup, string enumValueName, out NativeEnum enumeration, out NativeEnumValue value)
        {
            if (!lookup.TryGetGlobalSymbol(new NativeName(enumValueName, NativeNameKind.EnumValue), out value))
            {
                enumeration = null;
                return false;
            }

            return lookup.TryGetGlobalSymbol(
                new NativeName(value.EnumName, NativeNameKind.Enum),
                out enumeration);
        }

        public static NativeGlobalSymbol GetGlobalSymbol(this INativeSymbolLookup lookup, string name)
        {
            NativeGlobalSymbol symbol;
            if (!lookup.TryGetGlobalSymbol(name, out symbol))
            {
                throw new Exception($"Unable to get symbol {name}");
            }

            return symbol;
        }

        public static NativeGlobalSymbol GetGlobalSymbol(this INativeSymbolLookup lookup, NativeName name)
        {
            NativeGlobalSymbol symbol;
            if (!lookup.TryGetGlobalSymbol(name, out symbol))
            {
                throw new Exception($"Unable to get symbol {name}");
            }

            return symbol;
        }

        public static T GetGlobalSymbol<T>(this INativeSymbolLookup lookup, string name)
            where T : NativeSymbol
        {
            var symbol = GetGlobalSymbol(lookup, name);
            return (T)symbol.Symbol;
        }

        public static T GetGlobalSymbol<T>(this INativeSymbolLookup lookup, NativeName name)
            where T : NativeSymbol
        {
            var symbol = GetGlobalSymbol(lookup, name);
            return (T)symbol.Symbol;
        }

        public static BasicSymbolStorage ToBasicSymbolStorage(this INativeSymbolLookup lookup)
        {
            var storage = new BasicSymbolStorage();
            foreach (var name in lookup.NativeNames)
            {
                var symbol = lookup.GetGlobalSymbol(name);
                storage.Add(symbol);
            }
            return storage;
        }

        #endregion

        #region INativeSymbolStorage

        public static void AddConstant(this INativeSymbolStorage storage, NativeConstant constant) => storage.Add(new NativeGlobalSymbol(constant));

        public static void AddDefinedType(this INativeSymbolStorage storage, NativeDefinedType definedType) => storage.Add(new NativeGlobalSymbol(definedType));

        public static void AddTypeDef(this INativeSymbolStorage storage, NativeTypeDef typeDef) => storage.Add(new NativeGlobalSymbol(typeDef));

        public static void AddProcedure(this INativeSymbolStorage storage, NativeProcedure procedure) => storage.Add(new NativeGlobalSymbol(procedure));

        public static void AddEnumAndValues(this INativeSymbolStorage storage, NativeEnum enumeration)
        {
            // Enumerations are already special cased hence this behavior is automatic. 
            // https://github.com/jaredpar/pinvoke/issues/16
            storage.Add(new NativeGlobalSymbol(enumeration));
        }

        public static IEnumerable<Macro> GetAllMacros(this INativeSymbolStorage storage)
        {
            var list = new List<Macro>();
            foreach (var name in storage.NativeNames.Where(x => x.Kind == NativeNameKind.Constant))
            {
                NativeConstant constant;
                if (!storage.TryGetGlobalSymbol(name, out constant))
                {
                    continue;
                }

                switch (constant.ConstantKind)
                { 
                    case ConstantKind.MacroMethod:
                        {
                            var body = constant.Value.Expression;
                            if (body.Length > 1 && body[0] == '"' && body[body.Length - 1] == '"')
                            {
                                body = body.Substring(1, body.Length - 2);
                            }

                            MethodMacro method = null;
                            if (MethodMacro.TryCreateFromDeclaration(name.Name, body, out method))
                            {
                                list.Add(method);
                            }
                        }
                        break;
                    case ConstantKind.Macro:
                        list.Add(new Macro(name.Name, constant.Value.Expression));
                        break;
                    default:
                        Contract.ThrowInvalidEnumValue(constant.ConstantKind);
                        break;
                }
            }

            return list;
        }

        #endregion
    }
}
