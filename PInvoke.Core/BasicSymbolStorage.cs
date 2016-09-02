using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly Dictionary<string, NativeSymbol> _valueMap = new Dictionary<string, NativeSymbol>(StringComparer.Ordinal);

        public int Count => _constMap.Count + _definedMap.Count + _typeDefMap.Count + _procMap.Count + _valueMap.Count;
        public IEnumerable<NativeDefinedType> NativeDefinedTypes => _definedMap.Values;
        public IEnumerable<NativeTypeDef> NativeTypeDefs => _typeDefMap.Values;
        public IEnumerable<NativeProcedure> NativeProcedures => _procMap.Values;
        public IEnumerable<NativeConstant> NativeConstants => _constMap.Values;
        public IEnumerable<NativeEnum> NativeEnums => _definedMap.Values.Where(x => x.Kind == NativeSymbolKind.EnumType).Cast<NativeEnum>();
        public IEnumerable<NativeName> NativeNames => NativeNameUtil.GetNames(_constMap.Values, _definedMap.Values, _typeDefMap.Values, _procMap.Values, _valueMap.Values);

        public void AddConstant(NativeConstant nConst)
        {
            _constMap.Add(nConst.Name, nConst);
        }

        public void AddDefinedType(NativeDefinedType definedNt)
        {
            _definedMap.Add(definedNt.Name, definedNt);

            // CTODO: is this the wrong layer.  Should every storage do this???? 
            var ntEnum = definedNt as NativeEnum;
            if (ntEnum != null)
            {
                foreach (NativeEnumValue pair in ntEnum.Values)
                {
                    AddValue(pair.Name, ntEnum);
                }
            }
        }

        public void AddTypeDef(NativeTypeDef typeDef)
        {
            _typeDefMap.Add(typeDef.Name, typeDef);
        }

        public void AddProcedure(NativeProcedure proc)
        {
            _procMap.Add(proc.Name, proc);
        }

        /// <summary>
        /// Add an expression into the bag
        /// </summary>
        private void AddValue(string name, NativeSymbol value)
        {
            _valueMap[name] = value;
        }

        public bool TryFindDefined(string name, out NativeDefinedType nt)
        {
            return _definedMap.TryGetValue(name, out nt);
        }

        public bool TryFindTypeDef(string name, out NativeTypeDef nt)
        {
            return _typeDefMap.TryGetValue(name, out nt);
        }

        public bool TryFindProcedure(string name, out NativeProcedure proc)
        {
            return _procMap.TryGetValue(name, out proc);
        }

        public bool TryFindConstant(string name, out NativeConstant nConst)
        {
            return _constMap.TryGetValue(name, out nConst);
        }

        public bool TryFindEnumValue(string name, out NativeEnum enumeration, out NativeEnumValue value)
        {
            foreach (var currentEnum in _definedMap.Values.Where(x => x.Kind == NativeSymbolKind.EnumType).Cast<NativeEnum>())
            {
                foreach (var currentValue in currentEnum.Values)
                {
                    if (currentValue.Name == name)
                    {
                        enumeration = currentEnum;
                        value = currentValue;
                        return true;
                    }
                }
            }

            enumeration = null;
            value = null;
            return false;
        }
    }
}
