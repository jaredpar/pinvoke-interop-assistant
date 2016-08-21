// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace PInvoke
{

    /// <summary>
    /// Parent Child symbol
    /// </summary>
    /// <remarks></remarks>
    public class NativeSymbolRelationship
    {
        public NativeSymbol Parent { get; }
        public NativeSymbol Symbol { get; }

        public NativeSymbolRelationship(NativeSymbol parent, NativeSymbol symbol)
        {
            Parent = parent;
            Symbol = symbol;
        }

        public override string ToString() => $"{Parent} -> {Symbol}";
    }

    /// <summary>
    /// Used to perform various iterations on NativeType
    /// </summary>
    /// <remarks></remarks>
    public class NativeSymbolIterator
    {

        /// <summary>
        /// Find all of the reachable NativeType instances from this one
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<NativeDefinedType> FindAllReachableDefinedTypes(NativeType nt)
        {

            List<NativeSymbol> list = FindAllNativeSymbols(nt);
            List<NativeDefinedType> retList = new List<NativeDefinedType>();
            foreach (NativeSymbol ns in list)
            {
                if (ns.Category == NativeSymbolCategory.Defined)
                {
                    retList.Add((NativeDefinedType)ns);
                }
            }

            return retList;
        }

        public List<NativeSymbol> FindAllNativeSymbols(IEnumerable<NativeSymbol> enumerable)
        {
            List<NativeSymbolRelationship> found = FindAllReachableChildrenImpl(enumerable);
            List<NativeSymbol> list = new List<NativeSymbol>();
            foreach (NativeSymbolRelationship rel in found)
            {
                list.Add(rel.Symbol);
            }

            return list;
        }

        public List<NativeSymbol> FindAllNativeSymbols(NativeSymbol ns)
        {
            if (ns == null)
            {
                throw new ArgumentNullException("ns");
            }

            List<NativeSymbol> list = new List<NativeSymbol>();
            list.Add(ns);
            return FindAllNativeSymbols(list);
        }

        public List<NativeSymbolRelationship> FindAllNativeSymbolRelationships(NativeSymbol ns)
        {
            List<NativeSymbol> list = new List<NativeSymbol>();
            list.Add(ns);
            return FindAllNativeSymbolRelationships(list);
        }

        public List<NativeSymbolRelationship> FindAllNativeSymbolRelationships(IEnumerable<NativeSymbol> enumerable)
        {
            return FindAllReachableChildrenImpl(enumerable);
        }

        /// <summary>
        /// Process all of the reachable children and return the list of found items
        /// </summary>
        /// <param name="originalToProcess"></param>
        /// <remarks></remarks>
        private List<NativeSymbolRelationship> FindAllReachableChildrenImpl(IEnumerable<NativeSymbol> originalToProcess)
        {

            List<NativeSymbolRelationship> found = new List<NativeSymbolRelationship>();
            Dictionary<NativeSymbol, bool> map = new Dictionary<NativeSymbol, bool>();
            Queue<NativeSymbol> toVisit = new Queue<NativeSymbol>(originalToProcess);

            // First add in all of the original symbols with no parents
            foreach (NativeSymbol orig in originalToProcess)
            {
                found.Add(new NativeSymbolRelationship(null, orig));
            }

            while (toVisit.Count > 0)
            {
                NativeSymbol cur = toVisit.Dequeue();
                if (map.ContainsKey(cur))
                {
                    continue;
                }

                map[cur] = true;
                foreach (NativeSymbol child in cur.GetChildren())
                {
                    found.Add(new NativeSymbolRelationship(cur, child));
                    toVisit.Enqueue(child);
                }
            }

            return found;
        }
    }
}
