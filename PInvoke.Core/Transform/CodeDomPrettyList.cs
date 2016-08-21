// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.Text.RegularExpressions;
using PInvoke;
using static PInvoke.Contract;

namespace PInvoke.Transform
{

    /// <summary>
    /// Used to pretty list entries in the code dom tree
    /// </summary>
    /// <remarks></remarks>
    public class CodeDomPrettyList
    {
        private NativeSymbolBag _bag;

        private List<NativeTypeDef> _resolvedTypeDefList;
        public CodeDomPrettyList(NativeSymbolBag bag)
        {
            _bag = bag;
        }

        public void PerformRename(CodeTypeDeclarationCollection col)
        {
            if (col == null)
            {
                throw new ArgumentNullException("col");
            }

            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.Ordinal);
            map.Add("tagPOINT", "Point");

            CodeDomIterator it = new CodeDomIterator();
            List<object> list = it.Iterate(col);

            // Use the iterator so we make sure to reach nested types
            foreach (CodeTypeDeclaration ctd in FindUnmodifiedTypes(list))
            {
                NativeDefinedType definedNt = GetDefined(ctd);
                if (IsBadName(ctd.Name))
                {
                    foreach (NativeTypeDef possible in FindTypedefsTargeting(definedNt))
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

            List<CodeTypeDeclaration> list = new List<CodeTypeDeclaration>();

            // Use the iterator so we make sure to reach nested types
            foreach (object obj in col)
            {
                CodeTypeDeclaration ctd = obj as CodeTypeDeclaration;
                if (ctd != null)
                {
                    NativeDefinedType definedNt = GetDefined(ctd);
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
                CodeTypeDeclaration ctd = obj as CodeTypeDeclaration;
                if (ctd != null)
                {
                    string newName = null;
                    if (map.TryGetValue(ctd.Name, out newName))
                    {
                        ctd.Name = newName;
                    }
                    continue;
                }

                CodeTypeReference typeRef = obj as CodeTypeReference;
                if (typeRef != null)
                {
                    string newName = null;
                    if (map.TryGetValue(typeRef.BaseType, out newName))
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
                CodeCustomExpression custom = obj as CodeCustomExpression;
                if (custom != null)
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
        private List<NativeTypeDef> FindTypedefsTargeting(NativeType target)
        {
            // Build the cache
            if (_resolvedTypeDefList == null)
            {
                _resolvedTypeDefList = new List<NativeTypeDef>(_bag.FindResolvedTypedefs());
            }

            List<NativeTypeDef> list = new List<NativeTypeDef>();

            // First look in the symbol bag
            foreach (NativeTypeDef td in _resolvedTypeDefList)
            {
                if (object.ReferenceEquals(td.RealTypeDigged, target))
                {
                    list.Add(td);
                }
            }

            // Next look in the native storage for more types  
            NativeStorage ns = _bag.NativeStorageLookup;
            NativeStorage.TypeReference typeRef = ns.CreateTypeReference(target);
            if (typeRef != null)
            {
                foreach (NativeStorage.TypedefTypeRow trow in ns.TypedefType.FindByTarget(typeRef))
                {
                    NativeTypeDef found = null;
                    if (_bag.TryFindOrLoadTypedef(trow.Name, out found))
                    {
                        list.Add(found);
                    }
                }
            }

            return list;
        }


    }

}
