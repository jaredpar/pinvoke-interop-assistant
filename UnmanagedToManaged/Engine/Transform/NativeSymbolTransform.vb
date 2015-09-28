' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Text.RegularExpressions

Namespace Transform

    ''' <summary>
    ''' Runs certain transformations on symbols
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NativeSymbolTransform

        Private m_it As New NativeSymbolIterator()

        Public Sub New()
        End Sub

        Public Sub CollapseNamedTypes(ByVal ns As NativeSymbol)
            For Each rel As NativeSymbolRelationship In m_it.FindAllNativeSymbolRelationships(ns)
                CollapseNamedTypesImpl(rel.Parent, rel.Symbol)
            Next
        End Sub

        Private Sub CollapseNamedTypesImpl(ByVal ns As NativeSymbol, ByVal child As NativeSymbol)
            If ns Is Nothing Then
                Return
            End If
            ThrowIfNull(child)

            If child.Kind = NativeSymbolKind.NamedType Then
                Dim namedNt As NativeNamedType = DirectCast(child, NativeNamedType)
                If namedNt.RealType IsNot Nothing Then
                    ns.ReplaceChild(child, namedNt.RealType)
                End If
            End If
        End Sub

        Public Sub CollapseTypedefs(ByVal ns As NativeSymbol)
            For Each rel As NativeSymbolRelationship In m_it.FindAllNativeSymbolRelationships(ns)
                CollapseTypedefsImpl(rel.Parent, rel.Symbol)
            Next
        End Sub

        Private Sub CollapseTypedefsImpl(ByVal ns As NativeSymbol, ByVal child As NativeSymbol)
            If ns Is Nothing Then
                Return
            End If
            ThrowIfNull(child)

            If child.Kind = NativeSymbolKind.TypedefType Then
                Dim typedef As NativeTypeDef = DirectCast(child, NativeTypeDef)
                If typedef.RealType IsNot Nothing Then
                    ns.ReplaceChild(child, typedef.RealType)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Renames matching defined types and named types to the new name
        ''' </summary>
        ''' <param name="ns"></param>
        ''' <param name="oldName"></param>
        ''' <param name="newName"></param>
        ''' <remarks></remarks>
        Public Sub RenameTypeSymbol(ByVal ns As NativeSymbol, ByVal oldName As String, ByVal newName As String)
            For Each sym As NativeSymbol In m_it.FindAllNativeSymbols(ns)
                If (sym.Category = NativeSymbolCategory.Defined OrElse sym.Kind = NativeSymbolKind.NamedType) _
                    AndAlso 0 = String.CompareOrdinal(sym.Name, oldName) Then
                    sym.Name = newName
                End If
            Next
        End Sub

        ''' <summary>
        ''' Inspect the type name and determine if there is a better name for it 
        ''' </summary>
        ''' <param name="definedNt"></param>
        ''' <remarks></remarks>
        Public Sub RunTypeNameHeuristics(ByVal definedNt As NativeDefinedType)
            ThrowIfNull(definedNt)


        End Sub

    End Class

End Namespace
