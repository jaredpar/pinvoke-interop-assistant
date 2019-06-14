using PInvoke.Enums;
using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PInvoke
{
    /// <summary>
    /// This is an implementation of <see cref="INativeSymbolStorage"/> which guarantees object
    /// identity.  Essentially calls to Add -> Lookup will produce an object of the same 
    /// instance.  It will not create new, identical objects, in lookup.
    /// </summary>
    public sealed class BasicSymbolStorage : INativeSymbolStorage
    {
        private readonly Dictionary<string, NativeConstant> _constMap = new Dictionary<string, NativeConstant>(StringComparer.Ordinal);
        private readonly Dictionary<string, NativeDefinedType> _definedMap = new Dictionary<string, NativeDefinedType>(StringComparer.Ordinal);
        private readonly Dictionary<string, NativeTypeDef> _typeDefMap = new Dictionary<string, NativeTypeDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, NativeProcedure> _procMap = new Dictionary<string, NativeProcedure>(StringComparer.Ordinal);
        private readonly Dictionary<string, NativeEnumValue> _enumValueMap = new Dictionary<string, NativeEnumValue>(StringComparer.Ordinal);

        public int Count => _constMap.Count + _definedMap.Count + _typeDefMap.Count + _procMap.Count + _enumValueMap.Count;
        public IEnumerable<NativeDefinedType> NativeDefinedTypes => _definedMap.Values;
        public IEnumerable<NativeTypeDef> NativeTypeDefs => _typeDefMap.Values;
        public IEnumerable<NativeProcedure> NativeProcedures => _procMap.Values;
        public IEnumerable<NativeConstant> NativeConstants => _constMap.Values;
        public IEnumerable<NativeEnum> NativeEnums => _definedMap.Values.Where(x => x.Kind == NativeSymbolKind.EnumType).Cast<NativeEnum>();
        public IEnumerable<NativeEnumValue> NativeEnumValues => _enumValueMap.Values;
        public IEnumerable<NativeName> NativeNames => NativeNameUtil.GetNames(_constMap.Values, _definedMap.Values, _typeDefMap.Values, _procMap.Values, _enumValueMap.Values);

        public void Add(NativeGlobalSymbol globalSymbol)
        {
            var symbol = globalSymbol.Symbol;
            var name = globalSymbol.Name;
            switch (globalSymbol.Kind)
            {
                case NativeNameKind.Struct:
                case NativeNameKind.Union:
                case NativeNameKind.FunctionPointer:
                    _definedMap.Add(name.Name, (NativeDefinedType)symbol);
                    break;
                case NativeNameKind.Enum:
                    {
                        // https://github.com/jaredpar/pinvoke/issues/16
                        var enumeration = (NativeEnum)symbol;
                        _definedMap.Add(enumeration.Name, enumeration);
                        foreach (var value in enumeration.Values)
                        {
                            _enumValueMap.Add(value.Name, value);
                        }
                    }
                    break;
                case NativeNameKind.Procedure:
                    _procMap.Add(name.Name, (NativeProcedure)symbol);
                    break;
                case NativeNameKind.TypeDef:
                    _typeDefMap.Add(name.Name, (NativeTypeDef)symbol);
                    break;
                case NativeNameKind.Constant:
                    _constMap.Add(name.Name, (NativeConstant)symbol);
                    break;
                case NativeNameKind.EnumValue:
                    throw new Exception("EnumValues are added automatically as a part of their containing enumeration");
                default:
                    throw Contract.CreateInvalidEnumValueException(globalSymbol.Kind);
            }
        }

        public bool TryGetGlobalSymbol(NativeName name, out NativeGlobalSymbol symbol)
        {
            switch (name.Kind)
            {
                case NativeNameKind.Struct:
                case NativeNameKind.Union:
                case NativeNameKind.FunctionPointer:
                case NativeNameKind.Enum:
                    {
                        if (_definedMap.TryGetValue(name.Name, out NativeDefinedType definedType))
                        {
                            symbol = new NativeGlobalSymbol(definedType);
                            return true;
                        }
                    }
                    break;
                case NativeNameKind.Procedure:
                    {
                        if (_procMap.TryGetValue(name.Name, out NativeProcedure proc))
                        {
                            symbol = new NativeGlobalSymbol(proc);
                            return true;
                        }
                    }
                    break;
                case NativeNameKind.TypeDef:
                    {
                        if (_typeDefMap.TryGetValue(name.Name, out NativeTypeDef typeDef))
                        {
                            symbol = new NativeGlobalSymbol(typeDef);
                            return true;
                        }
                    }
                    break;
                case NativeNameKind.Constant:
                    {
                        if (_constMap.TryGetValue(name.Name, out NativeConstant constant))
                        {
                            symbol = new NativeGlobalSymbol(constant);
                            return true;
                        }
                    }
                    break;
                case NativeNameKind.EnumValue:
                    {
                        if (_enumValueMap.TryGetValue(name.Name, out NativeEnumValue value))
                        {
                            symbol = new NativeGlobalSymbol(value);
                            return true;
                        }
                    }
                    break;
                default:
                    Contract.ThrowInvalidEnumValue(name.Kind);
                    break;
            }

            symbol = default(NativeGlobalSymbol);
            return false;
        }

        public bool TryGetGlobalSymbol(string name, out NativeGlobalSymbol symbol)
        {
            return this.TryGetGlobalSymbolExhaustive(name, out symbol);
        }
    }
}
