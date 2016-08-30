// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static PInvoke.Contract;

namespace PInvoke
{
    /// <summary>
    /// Bag for NativeType instances which is used for querying and type resolution
    /// </summary>
    public class NativeSymbolBag : INativeSymbolBag
    {
        // CTODO: Remove all the storage and replace with INativeSymbolStorage or INativeSymbolBag
        private readonly Dictionary<string, NativeConstant> _constMap = new Dictionary<string, NativeConstant>(StringComparer.Ordinal);
        private readonly Dictionary<string, NativeDefinedType> _definedMap = new Dictionary<string, NativeDefinedType>(StringComparer.Ordinal);
        private readonly Dictionary<string, NativeTypeDef> _typeDefMap = new Dictionary<string, NativeTypeDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, NativeProcedure> _procMap = new Dictionary<string, NativeProcedure>(StringComparer.Ordinal);
        private readonly Dictionary<string, NativeSymbol> _valueMap = new Dictionary<string, NativeSymbol>(StringComparer.Ordinal);

        // CTODO: make this readonly
        private INativeSymbolLookup _nextSymbolLookup;

        public static INativeSymbolLookup EmptyLookup => EmptyNativeSymbolBag.Instance;

        public int Count
        {
            get { return _constMap.Count + _definedMap.Count + _typeDefMap.Count + _procMap.Count + _valueMap.Count; }
        }

        /// <summary>
        /// List of NativeDefinedType instances in the map
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<NativeDefinedType> NativeDefinedTypes
        {
            get { return _definedMap.Values; }
        }

        /// <summary>
        /// List of NativeTypedef instances in the map
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<NativeTypeDef> NativeTypedefs
        {
            get { return _typeDefMap.Values; }
        }

        /// <summary>
        /// Procedures in the bag
        /// </summary>
        public IEnumerable<NativeProcedure> NativeProcedures
        {
            get { return _procMap.Values; }
        }

        /// <summary>
        /// List of NativeConstant instances
        /// </summary>
        public IEnumerable<NativeConstant> NativeConstants
        {
            get { return _constMap.Values; }
        }

        public IEnumerable<NativeEnum> NativeEnums
        {
            get { return _definedMap.Values.Where(x => x.Kind == NativeSymbolKind.EnumType).Cast<NativeEnum>(); }
        }

        /// <summary>
        /// Backing INativeSymbolBag for this bag.  Used to resolve NativeNamedType instances
        /// </summary>
        public INativeSymbolLookup NextSymbolBag
        {
            get { return _nextSymbolLookup; }
            set { _nextSymbolLookup = value; }
        }

        public NativeSymbolBag(INativeSymbolLookup nextSymbolBag = null)
        {
            _nextSymbolLookup = nextSymbolBag ?? EmptyLookup;
        }

        /// <summary>
        /// Add the defined type into the bag
        /// </summary>
        /// <param name="nt"></param>
        /// <remarks></remarks>
        public void AddDefinedType(NativeDefinedType nt)
        {
            if (nt == null)
            {
                throw new ArgumentNullException("nt");
            }

            if (nt.IsAnonymous)
            {
                nt.Name = GenerateAnonymousName();
            }

            _definedMap.Add(nt.Name, nt);

            NativeEnum ntEnum = nt as NativeEnum;
            if (ntEnum != null)
            {
                foreach (NativeEnumValue pair in ntEnum.Values)
                {
                    AddValue(pair.Name, ntEnum);
                }
            }
        }

        /// <summary>
        /// Try and find a NativeDefinedType instance by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool TryFindDefinedType(string name, out NativeDefinedType nt)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return _definedMap.TryGetValue(name, out nt);
        }

        public bool TryFindDefined(string name, out NativeDefinedType nt)
        {
            return TryFindOrLoadDefinedType(name, out nt);
        }

        public bool TryFindOrLoadDefinedType(string name, out NativeDefinedType nt)
        {
            bool notUsed = false;
            return TryFindOrLoadDefinedType(name, out nt, out notUsed);
        }

        public bool TryFindOrLoadDefinedType(string name, out NativeDefinedType nt, out bool fromStorage)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (TryFindDefinedType(name, out nt))
            {
                fromStorage = false;
                return true;
            }

            if (_nextSymbolLookup.TryFindDefined(name, out nt))
            {
                AddDefinedType(nt);
                fromStorage = true;
                return true;
            }

            nt = null;
            fromStorage = false;
            return false;
        }

        /// <summary>
        /// Add a typedef to the bag
        /// </summary>
        /// <param name="nt"></param>
        /// <remarks></remarks>
        public void AddTypedef(NativeTypeDef nt)
        {
            if (nt == null)
            {
                throw new ArgumentNullException("nt");
            }

            _typeDefMap.Add(nt.Name, nt);
        }


        /// <summary>
        /// Try and find a NativeTypeDef instance by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool TryFindTypedef(string name, out NativeTypeDef nt)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return _typeDefMap.TryGetValue(name, out nt);
        }

        public bool TryFindOrLoadTypedef(string name, out NativeTypeDef nt)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (TryFindTypedef(name, out nt))
            {
                return true;
            }

            if (_nextSymbolLookup.TryFindTypedef(name, out nt))
            {
                AddTypedef(nt);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a procedure to the bag
        /// </summary>
        /// <param name="proc"></param>
        /// <remarks></remarks>
        public void AddProcedure(NativeProcedure proc)
        {
            if (proc == null)
            {
                throw new ArgumentNullException("proc");
            }

            _procMap.Add(proc.Name, proc);
        }

        /// <summary>
        /// Try and find a NativeProcedure instance by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="proc"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool TryFindProcedure(string name, out NativeProcedure proc)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return _procMap.TryGetValue(name, out proc);
        }

        public bool TryFindOrLoadProcedure(string name, out NativeProcedure proc)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (TryFindProcedure(name, out proc))
            {
                return true;
            }

            if (_nextSymbolLookup.TryFindProcedure(name, out proc))
            {
                AddProcedure(proc);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a constant to the bag
        /// </summary>
        /// <param name="nConst"></param>
        /// <remarks></remarks>
        public void AddConstant(NativeConstant nConst)
        {
            if (nConst == null)
            {
                throw new ArgumentNullException("nConst");
            }

            _constMap.Add(nConst.Name, nConst);
            AddValue(nConst.Name, nConst);
        }

        /// <summary>
        /// Add an expression into the bag
        /// </summary>
        /// <param name="value"></param>
        /// <remarks></remarks>
        private void AddValue(string name, NativeSymbol value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            _valueMap[name] = value;
        }

        /// <summary>
        /// Try find a NativeConstant by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nConst"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool TryFindConstant(string name, out NativeConstant nConst)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return _constMap.TryGetValue(name, out nConst);
        }

        public bool TryFindOrLoadConstant(string name, out NativeConstant nConst)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (TryFindConstant(name, out nConst))
            {
                return true;
            }

            if (_nextSymbolLookup.TryFindConstant(name, out nConst))
            {
                AddConstant(nConst);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Find the resolved symbols
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<NativeSymbol> FindResolvedNativeSymbols()
        {
            List<NativeSymbol> list = new List<NativeSymbol>();

            foreach (NativeDefinedType definedNt in FindResolvedDefinedTypes())
            {
                list.Add(definedNt);
            }

            foreach (NativeTypeDef typedef in FindResolvedTypedefs())
            {
                list.Add(typedef);
            }

            foreach (NativeConstant c in this.FindResolvedConstants())
            {
                list.Add(c);
            }

            foreach (NativeProcedure proc in this.FindResolvedProcedures())
            {
                list.Add(proc);
            }

            return list;
        }

        /// <summary>
        /// find all of the reachable NativeSymbol instances in this bag
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<NativeSymbol> FindAllReachableNativeSymbols()
        {

            List<NativeSymbol> list = new List<NativeSymbol>();
            foreach (NativeSymbolRelationship cur in FindAllReachableNativeSymbolRelationships())
            {
                list.Add(cur.Symbol);
            }

            return list;
        }

        public List<NativeSymbolRelationship> FindAllReachableNativeSymbolRelationships()
        {
            // Build up the list of types
            List<NativeSymbol> list = new List<NativeSymbol>();
            foreach (NativeDefinedType definedNt in _definedMap.Values)
            {
                list.Add(definedNt);
            }

            foreach (NativeTypeDef typedefNt in _typeDefMap.Values)
            {
                list.Add(typedefNt);
            }

            foreach (NativeProcedure proc in _procMap.Values)
            {
                list.Add(proc);
            }

            foreach (NativeConstant c in _constMap.Values)
            {
                list.Add(c);
            }

            NativeSymbolIterator iter = new NativeSymbolIterator();
            return iter.FindAllNativeSymbolRelationships(list);
        }

        /// <summary>
        /// Find all of the NativeNamedType instances for which a type could not be found
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<NativeSymbolRelationship> FindUnresolvedNativeSymbolRelationships()
        {
            List<NativeSymbolRelationship> list = new List<NativeSymbolRelationship>();
            foreach (NativeSymbolRelationship rel in this.FindAllReachableNativeSymbolRelationships())
            {
                if (!rel.Symbol.IsImmediateResolved)
                {
                    list.Add(rel);
                }
            }

            return list;
        }

        public List<NativeValue> FindUnresolvedNativeValues()
        {
            List<NativeValue> list = new List<NativeValue>();
            foreach (NativeSymbol ns in this.FindAllReachableNativeSymbols())
            {
                NativeValue nValue = ns as NativeValue;
                if (nValue != null && !nValue.IsValueResolved)
                {
                    list.Add(nValue);
                }
            }

            return list;
        }

        public bool TryFindOrLoadNativeType(NativeNamedType namedType, ref NativeType nt)
        {
            bool notUsed = false;
            return TryFindOrLoadNativeType(namedType, out nt, out notUsed);
        }

        /// <summary>
        /// Try and load the named type
        /// </summary>
        /// <param name="namedType"></param>
        /// <param name="nt"></param>
        /// <param name="loadFromStorage"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool TryFindOrLoadNativeType(NativeNamedType namedType, out NativeType nt, out bool loadFromStorage)
        {
            if (string.IsNullOrEmpty(namedType.Qualification))
            {
                // If there is no qualification then just load the type by it's name
                return TryFindOrLoadNativeType(namedType.Name, out nt, out loadFromStorage);
            }

            // When there is a qualification it is either struct, union or enum.  Try and load the defined type
            // for the name and then make sure that it is the correct type 
            NativeDefinedType definedNt = null;
            if (!this.TryFindOrLoadDefinedType(namedType.Name, out definedNt, out loadFromStorage))
            {
                nt = null;
                return false;
            }

            string test = null;
            switch (definedNt.Kind)
            {
                case NativeSymbolKind.StructType:
                    test = "struct";
                    break;
                case NativeSymbolKind.UnionType:
                    test = "union";
                    break;
                case NativeSymbolKind.EnumType:
                    test = "enum";
                    break;
                default:
                    nt = null;
                    return false;
            }

            string qual = namedType.Qualification;
            if (string.Equals("class", qual, StringComparison.OrdinalIgnoreCase))
            {
                qual = "struct";
            }

            if (0 != string.CompareOrdinal(test, qual))
            {
                nt = null;
                return false;
            }

            nt = definedNt;
            return true;
        }

        /// <summary>
        /// Try and get a NativeType from the bag with the specified name.  Prefer types in
        /// the following order
        ///   NativeDefinedType
        ///   NativeTypeDef
        ///   NativeStorage
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool TryFindOrLoadNativeType(string name, out NativeType nt)
        {
            bool notUsed = false;
            return TryFindOrLoadNativeType(name, out nt, out notUsed);
        }

        public bool TryFindOrLoadNativeType(string name, out NativeType nt, out bool loadFromStorage)
        {

            // First check the defined types
            loadFromStorage = false;
            NativeDefinedType definedNt = null;
            if (TryFindDefinedType(name, out definedNt))
            {
                nt = definedNt;
                return true;
            }

            // Second, check the typedefs
            NativeTypeDef typeDefNt = null;
            if (TryFindTypedef(name, out typeDefNt))
            {
                nt = typeDefNt;
                return true;
            }

            // Lastly try and find it in the stored file
            if (_nextSymbolLookup.TryFindByName(name, out nt))
            {
                ThrowIfNull(nt);
                loadFromStorage = true;

                // If this is a stored symbol we need to add it to the bag.  Otherwise we can 
                // hit an infinite loop.  Assume we have a structure like so

                // struct s1
                // { 
                //   struct s1 *p;
                // }
                //
                // This contains a recursive reference to itself.  We need to store the looked
                // up type to prevent an infinite loop
                if (nt.Category == NativeSymbolCategory.Defined)
                {
                    AddDefinedType((NativeDefinedType)nt);
                }
                else if (nt.Kind == NativeSymbolKind.TypedefType)
                {
                    AddTypedef((NativeTypeDef)nt);
                }

                return true;
            }

            nt = null;
            return false;
        }

        public bool TryFindValue(string valueName, out NativeSymbol ns)
        {
            return _valueMap.TryGetValue(valueName, out ns);
        }

        public bool TryFindOrLoadValue(string valueName, out NativeSymbol ns)
        {
            bool notUsed = false;
            return TryFindOrLoadValue(valueName, out ns, out notUsed);
        }

        public bool TryFindOrLoadValue(string valueName, out NativeSymbol ns, out bool loaded)
        {
            loaded = false;
            if (TryFindValue(valueName, out ns))
            {
                return true;
            }

            // First look for a constant by this name
            NativeConstant nConst = null;
            if (_nextSymbolLookup.TryFindConstant(valueName, out nConst))
            {
                AddConstant(nConst);
                loaded = true;
                ns = nConst;
                return true;
            }

            // Lastly look for enums by value 
            List<NativeDefinedType> enumTypes;
            if (_nextSymbolLookup.TryFindEnumByValueName(valueName, out enumTypes))
            {
                loaded = true;
                ns = enumTypes[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Save all of the information into a NativeStorage database that is completely resolved
        /// </summary>
        public void SaveToNativeStorage(INativeSymbolStorage nativeStorage)
        {
            foreach (NativeConstant nConst in this.FindResolvedConstants())
            {
                nativeStorage.AddConstant(nConst);
            }

            foreach (NativeDefinedType definedNt in this.FindResolvedDefinedTypes())
            {
                nativeStorage.AddDefinedType(definedNt);
            }

            foreach (NativeTypeDef typeDef in this.FindResolvedTypedefs())
            {
                nativeStorage.AddTypedef(typeDef);
            }

            foreach (NativeProcedure proc in this.FindResolvedProcedures())
            {
                nativeStorage.AddProcedure(proc);
            }
        }

        /// <summary>
        /// Find all of the resolved defined types.  
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<NativeDefinedType> FindResolvedDefinedTypes()
        {
            Dictionary<NativeSymbol, Nullable<bool>> map = new Dictionary<NativeSymbol, Nullable<bool>>();
            List<NativeDefinedType> list = new List<NativeDefinedType>();

            foreach (NativeDefinedType definedNt in _definedMap.Values)
            {
                if (IsResolved(definedNt, map))
                {
                    list.Add(definedNt);
                }
            }

            return list;
        }

        /// <summary>
        /// Find all of the resolved typedefs
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<NativeTypeDef> FindResolvedTypedefs()
        {
            Dictionary<NativeSymbol, Nullable<bool>> map = new Dictionary<NativeSymbol, Nullable<bool>>();
            List<NativeTypeDef> list = new List<NativeTypeDef>();

            foreach (NativeTypeDef typedefNt in _typeDefMap.Values)
            {
                if (IsResolved(typedefNt, map))
                {
                    list.Add(typedefNt);
                }
            }

            return list;
        }

        /// <summary>
        /// Find all of the resolved NativeProcedure instances
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<NativeProcedure> FindResolvedProcedures()
        {
            Dictionary<NativeSymbol, Nullable<bool>> map = new Dictionary<NativeSymbol, Nullable<bool>>();
            List<NativeProcedure> list = new List<NativeProcedure>();

            foreach (NativeProcedure proc in _procMap.Values)
            {
                if (IsResolved(proc, map))
                {
                    list.Add(proc);
                }
            }

            return list;
        }

        public IEnumerable<NativeConstant> FindResolvedConstants()
        {
            Dictionary<NativeSymbol, Nullable<bool>> map = new Dictionary<NativeSymbol, Nullable<bool>>();
            List<NativeConstant> list = new List<NativeConstant>();

            foreach (NativeConstant c in _constMap.Values)
            {
                if (IsResolved(c, map))
                {
                    list.Add(c);
                }
            }

            return list;
        }

        #region "Resolution Functions"

        public bool TryResolveSymbolsAndValues()
        {
            using (ProcedureFinder finder = new ProcedureFinder())
            {
                return TryResolveSymbolsAndValues(finder, new ErrorProvider());
            }
        }

        public bool TryResolveSymbolsAndValues(ErrorProvider ep)
        {
            using (ProcedureFinder finder = new ProcedureFinder())
            {
                return TryResolveSymbolsAndValues(finder, ep);
            }
        }

        /// <summary>
        /// Try and resolve all of the unresolved types in the bag.  Return false if the types
        /// couldn't all be resolved
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool TryResolveSymbolsAndValues(ProcedureFinder finder, ErrorProvider ep)
        {
            if (ep == null)
            {
                throw new ArgumentNullException("ep");
            }

            // Try and resolve the proc name
            foreach (NativeProcedure proc in _procMap.Values)
            {
                if (string.IsNullOrEmpty(proc.DllName))
                {
                    finder.TryFindDllNameExact(proc.Name, out proc.DllName);
                }
            }

            return ResolveCore(ep);
        }

        private bool ResolveCore(ErrorProvider ep)
        {
            bool allResolved = false;
            do
            {
                bool loadedSymbolFromStorage = false;
                bool allSymbolResolved = ResolveCoreSymbols(ep, ref loadedSymbolFromStorage);

                bool loadedValueFromStorage = false;
                bool allValuesResolved = ResolveCoreValues(ep, ref loadedValueFromStorage);

                // When an object is loaded from storage it is done a at a single level.  So we
                // now need to walk that type and resolve any named types from it
                if (!loadedSymbolFromStorage && !loadedValueFromStorage)
                {
                    allResolved = (allValuesResolved && allSymbolResolved);
                    break; // TODO: might not be correct. Was : Exit Do
                }
            } while (true);

            return allResolved;
        }

        /// <summary>
        /// Try and resolve the unresolved symbols in the system
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="loadedSomethingFromStorage"></param>
        /// <remarks></remarks>
        private bool ResolveCoreSymbols(ErrorProvider ep, ref bool loadedSomethingFromStorage)
        {
            bool allResolved = true;

            foreach (NativeSymbolRelationship rel in this.FindUnresolvedNativeSymbolRelationships())
            {
                // Values and value expressions are resolved below
                if (rel.Symbol.Kind == NativeSymbolKind.Value || rel.Symbol.Kind == NativeSymbolKind.ValueExpression)
                {
                    continue;
                }

                // All we can resolve here are NativeNamedType instances
                NativeNamedType namedType = rel.Symbol as NativeNamedType;
                if (namedType == null)
                {
                    ep.AddError("Failed to resolve {0} -> '{1}'", rel.Symbol.Kind, rel.Symbol.DisplayName);
                    continue;
                }

                NativeType nt = null;
                bool fromStorage = false;
                if (this.TryFindOrLoadNativeType(namedType, out nt, out fromStorage))
                {
                    if (fromStorage)
                    {
                        loadedSomethingFromStorage = true;
                    }
                    namedType.RealType = nt;

                }
                else if (rel.Parent != null && rel.Parent.Kind == NativeSymbolKind.PointerType && !string.IsNullOrEmpty(namedType.Qualification))
                {
                    // When we have a pointer to an unresolved type, treat this as an opaque type
                    ep.AddWarning("Treating '{0}' as pointer to opaque type", namedType.DisplayName);
                    namedType.RealType = new NativeOpaqueType();
                }
                else
                {
                    ep.AddError("Failed to resolve name '{0}'", namedType.DisplayName);
                    allResolved = false;
                }
            }

            return allResolved;
        }

        /// <summary>
        /// Try and resolve the unresolved values in the system.  
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="loadedSomethingFromStorage"></param>
        /// <remarks></remarks>
        private bool ResolveCoreValues(ErrorProvider ep, ref bool loadedSomethingFromStorage)
        {
            bool allResolved = true;
            foreach (NativeValue nValue in this.FindUnresolvedNativeValues())
            {
                bool fromStorage = false;

                switch (nValue.ValueKind)
                {
                    case NativeValueKind.SymbolValue:
                        NativeSymbol ns = null;
                        if (this.TryFindOrLoadValue(nValue.Name, out ns, out fromStorage))
                        {
                            nValue.Value = ns;
                        }
                        break;
                    case NativeValueKind.SymbolType:
                        NativeType nt = null;
                        if (this.TryFindOrLoadNativeType(nValue.Name, out nt, out fromStorage))
                        {
                            nValue.Value = nt;
                        }
                        break;
                }

                if (!nValue.IsImmediateResolved)
                {
                    ep.AddError("Failed to resolve value '{0}'", nValue.Name);
                    allResolved = false;
                }

                if (fromStorage)
                {
                    loadedSomethingFromStorage = true;
                }
            }

            return allResolved;
        }

        private bool IsResolved(NativeSymbol ns, Dictionary<NativeSymbol, bool?> map)
        {
            ThrowIfNull(ns);
            ThrowIfNull(map);

            // See if this has already been calculated
            bool? ret = false;
            if (map.TryGetValue(ns, out ret))
            {
                if (ret.HasValue)
                {
                    return ret.Value;
                }
                else
                {
                    // We're in a recursive call to the same type.  Return true here because if another type is
                    // not resolved then this will fall out
                    return true;
                }
            }

            // If there are no immediate children then the type is most definately resolved
            NativeSymbolIterator it = new NativeSymbolIterator();
            List<NativeSymbol> children = new List<NativeSymbol>(ns.GetChildren());
            if (children.Count == 0)
            {
                return true;
            }

            // Add an entry into the map to indicate that we are exploring this type
            map.Add(ns, null);

            ret = true;
            foreach (NativeSymbol child in children)
            {
                if (!child.IsImmediateResolved || !IsResolved(child, map))
                {
                    ret = false;
                    break;
                }
            }

            // Save the success
            map[ns] = ret;
            return ret.Value;
        }

        #endregion

        #region "Shared Helpers"

        public static string GenerateAnonymousName()
        {
            Guid g = Guid.NewGuid();
            string name = g.ToString();
            name = name.Replace("-", "_");
            return "Anonymous_" + name;
        }

        public static bool IsAnonymousName(string name)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(name, "^Anonymous_((\\w+_){4})(\\w+)$");
        }

        public static NativeSymbolBag CreateFrom(Parser.NativeCodeAnalyzerResult result)
        {
            return CreateFrom(result, NativeStorage.DefaultInstance);
        }

        public static NativeSymbolBag CreateFrom(Parser.NativeCodeAnalyzerResult result, NativeStorage ns)
        {
            return CreateFrom(result, ns, new ErrorProvider());
        }

        /// <summary>
        /// Create a NativeTypeBag from the result of a code analysis
        /// </summary>
        public static NativeSymbolBag CreateFrom(Parser.NativeCodeAnalyzerResult result, INativeSymbolLookup nextSymbolBag, ErrorProvider ep)
        {
            if (ep == null)
            {
                throw new ArgumentNullException("ep");
            }

            NativeSymbolBag bag = new NativeSymbolBag(nextSymbolBag);
            foreach (NativeConstant nConst in result.AllNativeConstants)
            {
                try
                {
                    bag.AddConstant(nConst);
                }
                catch
                {
                    ep.AddError("Duplicate NativeConstant Name: {0}", nConst.Name);
                }
            }

            foreach (NativeDefinedType definedNt in result.NativeDefinedTypes)
            {
                try
                {
                    bag.AddDefinedType(definedNt);
                }
                catch
                {
                    ep.AddError("Duplicate NativeDefinedType Name: {0}", definedNt.Name);
                }
            }

            foreach (NativeTypeDef typedefNt in result.NativeTypeDefs)
            {
                try
                {
                    bag.AddTypedef(typedefNt);
                }
                catch
                {
                    ep.AddError("Duplicate NativeTypeDef Name: {0}", typedefNt.Name);
                }
            }

            foreach (NativeProcedure proc in result.NativeProcedures)
            {
                try
                {
                    bag.AddProcedure(proc);
                }
                catch (Exception)
                {
                    ep.AddError("Duplicate NativeProcedure Name: {0}", proc.Name);
                }
            }

            return bag;
        }

        #endregion

    }
}
