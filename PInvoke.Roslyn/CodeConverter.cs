using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Roslyn
{
    public sealed class CodeConverter
    {
        internal SyntaxTree Generate(NativeSymbolBag bag)
        {
            throw new Exception();
            /*
            ThrowIfNull(bag)
            ThrowIfNull(ep)

            ' Make sure than all of the referenced NativeDefinedType instances are in the correct
            ' portion of the bag
            ChaseReferencedDefinedTypes(bag)

            ' First step is to resolve the symbols
            bag.TryResolveSymbolsAndValues(ep)

            ' Create the codedom transform
            Dim transform As New CodeTransform(Me._type)
            Dim marshalUtil As New MarshalTransform(Me._type, _transformKind)
            Dim col As New CodeTypeDeclarationCollection()

            ' Only output the constants if there are actually any
            Dim list As New List(Of NativeConstant)(bag.FindResolvedConstants())
            If list.Count > 0 Then
                Dim constCtd As CodeTypeDeclaration = transform.GenerateConstants(list)
                If constCtd.Members.Count > 0 Then
                    col.Add(constCtd)
                End If
            End If

            For Each definedNt As NativeDefinedType In bag.FindResolvedDefinedTypes()
                Dim ctd As CodeTypeDeclaration = transform.GenerateDeclaration(definedNt)
                marshalUtil.Process(ctd)
                col.Add(ctd)
            Next

            Dim procList As New List(Of NativeProcedure)(bag.FindResolvedProcedures())
            If procList.Count > 0 Then
                Dim procType As CodeTypeDeclaration = transform.GenerateProcedures(procList)
                marshalUtil.Process(procType)
                col.Add(procType)
            End If

            ' Add the helper types that we need
            AddHelperTypes(col)

            ' Next step is to run the pretty lister on it
            Dim prettyLister As New CodeDomPrettyList(bag)
            prettyLister.PerformRename(col)

            Return col

            throw new Exception();
            */
        }

    }
}
