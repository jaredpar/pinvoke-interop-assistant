// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static PInvoke.Contract;

namespace PInvoke.Transform
{
    /// <summary>
    /// Runs certain transformations on symbols
    /// </summary>
    /// <remarks></remarks>
    public class NativeSymbolTransform
    {
        private NativeSymbolIterator iterator = new NativeSymbolIterator();

        public NativeSymbolTransform()
        {
        }

        public void CollapseNamedTypes(NativeSymbol ns)
        {
            foreach (var relationship in iterator.FindAllNativeSymbolRelationships(ns))
            {
                CollapseNamedTypesImpl(relationship.Parent, relationship.Symbol);
            }
        }

        private void CollapseNamedTypesImpl(NativeSymbol ns, NativeSymbol child)
        {
            if (ns == null)
            {
                return;
            }
            ThrowIfNull(child);

            if (child.Kind == NativeSymbolKind.NamedType)
            {
                var namedNt = (NativeNamedType)child;
                if (namedNt.RealType != null)
                {
                    ns.ReplaceChild(child, namedNt.RealType);
                }
            }
        }

        public void CollapseTypedefs(NativeSymbol ns)
        {
            foreach (var relationship in iterator.FindAllNativeSymbolRelationships(ns))
            {
                CollapseTypedefsImpl(relationship.Parent, relationship.Symbol);
            }
        }

        private void CollapseTypedefsImpl(NativeSymbol ns, NativeSymbol child)
        {
            if (ns == null)
            {
                return;
            }
            ThrowIfNull(child);

            if (child.Kind == NativeSymbolKind.TypeDefType)
            {
                var typedef = (NativeTypeDef)child;
                if (typedef.RealType != null)
                {
                    ns.ReplaceChild(child, typedef.RealType);
                }
            }
        }

        /// <summary>
        /// Renames matching defined types and named types to the new name
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <remarks></remarks>
        public void RenameTypeSymbol(NativeSymbol ns, string oldName, string newName)
        {
            foreach (var sym in iterator.FindAllNativeSymbols(ns))
            {
                if ((sym.Category == NativeSymbolCategory.Defined || sym.Kind == NativeSymbolKind.NamedType) && 0 == string.CompareOrdinal(sym.Name, oldName))
                {
                    sym.Name = newName;
                }
            }
        }

        /// <summary>
        /// Inspect the type name and determine if there is a better name for it 
        /// </summary>
        /// <param name="definedNt"></param>
        /// <remarks></remarks>
        public void RunTypeNameHeuristics(NativeDefinedType definedNt)
        {
            ThrowIfNull(definedNt);
        }
    }
}
