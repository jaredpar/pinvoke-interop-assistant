using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PInvoke.Transform;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Roslyn
{
    public sealed class CodeConverter
    {
        private readonly LanguageType _langaugeType = LanguageType.CSharp;

        internal SyntaxTree Generate(NativeSymbolBag bag, ErrorProvider ep)
        {
            // Make sure than all of the referenced NativeDefinedType instances are in the correct
            // portion of the bag
            ChaseReferencedDefinedTypes(bag);

            // First step is to resolve the symbols
            bag.TryResolveSymbolsAndValues(ep);

            // Create the codedom transform
            var transform = new CodeTransform(_langaugeType, bag);
            var marshalUtil = new MarshalTransform(_langaugeType, bag, TransformKindFlags.All);
            var col = new CodeTypeDeclarationCollection();

            // Only output the constants if there are actually any
            var constList = bag.FindResolvedConstants().ToList();
            if (constList.Count > 0)
            {
                var constCtd = transform.GenerateConstants(constList);
                if (constCtd.Members.Count > 0)
                {
                    col.Add(constCtd);
                }
            }

            foreach (var definedNt in bag.FindResolvedDefinedTypes())
            {
                var ctd = transform.GenerateDeclaration(definedNt);
                marshalUtil.Process(ctd);
                col.Add(ctd);
            }

            var procList = bag.FindResolvedProcedures().ToList();
            if (procList.Count > 0)
            {
                var procType = transform.GenerateProcedures(procList);
                marshalUtil.Process(procType);
                col.Add(procType);
            }

            // Add the helper types that we need
            AddHelperTypes(col);

            // Next step is to run the pretty lister on it
            var prettyLister = new CodeDomPrettyList(bag);
            prettyLister.PerformRename(col);

            var code = BasicConverter.ConvertCodeDomToPInvokeCodeImpl(_langaugeType, col, ep);
            return CSharpSyntaxTree.ParseText(code);
        }

        /// <summary>
        /// Add any of the helper types that we need
        /// </summary>
        private void AddHelperTypes(CodeTypeDeclarationCollection col)
        {
            var needed = false;
            var it = new CodeDomIterator();
            foreach (var ctdRef in it.Iterate(col).OfType<CodeTypeReference>())
            {
                if (String.Equals(ctdRef.BaseType, MarshalTypeFactory.PInvokePointerTypeName))
                {
                    needed = true;
                    break;
                }
            }

            if (needed)
            {
                col.Add(MarshalTypeFactory.CreatePInvokePointerType());
            }
        }
        /*

        Private Sub AddHelperTypes(ByVal col As CodeTypeDeclarationCollection)
            Dim addPInvokePointer As Boolean = False
            Dim it As New CodeDomIterator()
            Dim list As List(Of Object) = it.Iterate(col)
            For Each obj As Object In list
                Dim ctdRef As CodeTypeReference = TryCast(obj, CodeTypeReference)
                If ctdRef IsNot Nothing AndAlso 0 = String.CompareOrdinal(ctdRef.BaseType, MarshalTypeFactory.PInvokePointerTypeName) Then
                    addPInvokePointer = True
                End If
            Next

            If addPInvokePointer Then
                col.Add(MarshalTypeFactory.CreatePInvokePointerType())
            End If
        End Sub
        */

        /// <summary>
        /// Make sure that any NativeDefinedType referenced is in the bag.  That way if we 
        /// have structures which point to other NativeDefinedType instances, they are automagically
        /// put into the bag 
        /// </summary>
        private void ChaseReferencedDefinedTypes(NativeSymbolBag bag)
        {
            bag.TryResolveSymbolsAndValues();
            
            foreach (var sym in bag.FindAllReachableNativeSymbols())
            {
                if (sym.Category == NativeSymbolCategory.Defined)
                {
                    NativeDefinedType defined = null;
                    if (!bag.TryFindDefinedType(sym.Name, out defined))
                    {
                        bag.AddDefinedType((NativeDefinedType)sym);
                    }
                }
            }
        }
    }
}
