// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.Text.RegularExpressions;
using PInvoke;
using static PInvoke.Contract;
using PInvoke.NativeTypes;
using PInvoke.Enums;

namespace PInvoke.Transform
{

    /// <summary>
    /// Used to pretty list entries in the code dom tree
    /// </summary>
    /// <remarks></remarks>
    public class CodeDomPrettyList
    {
        private NativeSymbolBag bag;

        private List<NativeTypeDef> resolvedTypeDefList;
        public CodeDomPrettyList(NativeSymbolBag bag)
        {
            this.bag = bag;
        }

        public void PerformRename(CodeTypeDeclarationCollection col)
        {
            if (col == null)
            {
                throw new ArgumentNullException("col");
            }

            var map = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "tagPOINT", "Point" }
            };

            var it = new CodeDomIterator();
            var list = it.Iterate(col);

            // Use the iterator so we make sure to reach nested types
            foreach (var ctd in FindUnmodifiedTypes(list))
            {
                var definedNt = GetDefined(ctd);
                if (IsBadName(ctd.Name))
                {
                    foreach (var possible in FindTypeDefsTargeting(definedNt))
                    {
                        if (!IsBadName(possible.Name))
                        {
                            map[ctd.Name] = possible.Name;
                        }
                    }
                }
            }

            SmartTypeRename(map, list);
            ResetCustomExpressions(list);
        }

        private List<CodeTypeDeclaration> FindUnmodifiedTypes(List<object> col)
        {

            var list = new List<CodeTypeDeclaration>();

            // Use the iterator so we make sure to reach nested types
            foreach (var obj in col)
            {
                if (obj is CodeTypeDeclaration ctd)
                {
                    var definedNt = GetDefined(ctd);
                    if (definedNt != null && 0 == string.CompareOrdinal(definedNt.Name, ctd.Name))
                    {
                        list.Add(ctd);
                    }
                }
            }

            return list;
        }

        private bool IsBadName(string name)
        {
            if (Regex.IsMatch(name, "^_\\w+$"))
            {
                return true;
            }

            if (NativeSymbolBag.IsAnonymousName(name))
            {
                return true;
            }

            return false;
        }

        private NativeDefinedType GetDefined(CodeTypeDeclaration ctd)
        {
            ThrowIfNull(ctd);

            if (ctd.UserData.Contains(TransformConstants.Type))
            {
                return (NativeDefinedType)ctd.UserData[TransformConstants.Type];
            }

            return null;
        }

        private void SmartTypeRename(Dictionary<string, string> map, List<object> col)
        {
            ThrowIfNull(map);
            ThrowIfNull(col);

            foreach (object obj in col)
            {
                if (obj is CodeTypeDeclaration ctd)
                {
                    if (map.TryGetValue(ctd.Name, out string newName))
                    {
                        ctd.Name = newName;
                    }
                    continue;
                }

                if (obj is CodeTypeReference typeRef)
                {
                    if (map.TryGetValue(typeRef.BaseType, out string newName))
                    {
                        typeRef.BaseType = newName;
                    }
                    continue;
                }
            }

        }

        /// <summary>
        /// Instances of CodeCustomExpression are represented as CodeSnippetExpression instances in the tree.  Because
        /// their is no good way to virtually update the Value property we are forced to recalculate it whenever there 
        /// is a change to one of the expressions contained within the custom node. 
        /// 
        /// This method will force all of the custom expression instances in the tree to be updated 
        /// </summary>
        /// <param name="col"></param>
        /// <remarks></remarks>
        private void ResetCustomExpressions(List<object> col)
        {
            ThrowIfNull(col);

            foreach (object obj in col)
            {
                if (obj is CodeCustomExpression custom)
                {
                    custom.ResetValue();
                }
            }
        }

        /// <summary>
        /// Find any typedegs where the passed in value is the real type
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private List<NativeTypeDef> FindTypeDefsTargeting(NativeType target)
        {
            // Build the cache
            if (resolvedTypeDefList == null)
            {
                resolvedTypeDefList = new List<NativeTypeDef>(bag.FindResolvedTypeDefs());
            }

            var list = new List<NativeTypeDef>();

            // First look in the symbol bag
            foreach (var td in resolvedTypeDefList)
            {
                if (ReferenceEquals(td.RealTypeDigged, target))
                {
                    list.Add(td);
                }
            }

            // Next look in the native storage for more types  
            // TODO: this cast is bad.
            var lookup = bag.NextSymbolLookup;
            foreach (var name in lookup.NativeNames.Where(x => x.Kind == NativeNameKind.TypeDef))
            {
                var typeDef = lookup.GetGlobalSymbol<NativeTypeDef>(name);
                if (typeDef.RealType == target)
                { 
                    list.Add(typeDef);
                }
            }

            return list;
        }
    }
}
