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

        public IEnumerable<NativeEnum> NativeEnums
        {
            get { return _definedMap.Values.Where(x => x.Kind == NativeSymbolKind.EnumType).Cast<NativeEnum>(); }
        }

        public void AddConstant(NativeConstant nConst)
        {
            _constMap.Add(nConst.Name, nConst);
        }

        public void AddDefinedType(NativeDefinedType definedNt)
        {
            _definedMap.Add(definedNt.Name, definedNt);
        }

        public void AddTypedef(NativeTypeDef typeDef)
        {
            _typeDefMap.Add(typeDef.Name, typeDef);
        }

        public void AddProcedure(NativeProcedure proc)
        {
            _procMap.Add(proc.Name, proc);
        }

        public bool TryFindDefined(string name, out NativeDefinedType nt)
        {
            return _definedMap.TryGetValue(name, out nt);
        }

        public bool TryFindTypedef(string name, out NativeTypeDef nt)
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
    }
}
