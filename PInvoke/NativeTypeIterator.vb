' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic

''' <summary>
''' Parent Child symbol
''' </summary>
''' <remarks></remarks>
Public Class NativeSymbolRelationship
    Private _parent As NativeSymbol
    Private _symbol As NativeSymbol

    Public ReadOnly Property Parent() As NativeSymbol
        Get
            Return _parent
        End Get
    End Property

    Public ReadOnly Property Symbol() As NativeSymbol
        Get
            Return _symbol
        End Get
    End Property

    Public Sub New(ByVal parent As NativeSymbol, ByVal symbol As NativeSymbol)
        _parent = parent
        _symbol = symbol
    End Sub
End Class

''' <summary>
''' Used to perform various iterations on NativeType
''' </summary>
''' <remarks></remarks>
Public Class NativeSymbolIterator

    ''' <summary>
    ''' Find all of the reachable NativeType instances from this one
    ''' </summary>
    ''' <param name="nt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindAllReachableDefinedTypes(ByVal nt As NativeType) As IEnumerable(Of NativeDefinedType)

        Dim list As List(Of NativeSymbol) = FindAllNativeSymbols(nt)
        Dim retList As New List(Of NativeDefinedType)
        For Each ns As NativeSymbol In list
            If ns.Category = NativeSymbolCategory.Defined Then
                retList.Add(DirectCast(ns, NativeDefinedType))
            End If
        Next

        Return retList
    End Function

    Public Function FindAllNativeSymbols(ByVal enumerable As IEnumerable(Of NativeSymbol)) As List(Of NativeSymbol)
        Dim found As List(Of NativeSymbolRelationship) = FindAllReachableChildrenImpl(enumerable)
        Dim list As New List(Of NativeSymbol)
        For Each rel As NativeSymbolRelationship In found
            list.Add(rel.Symbol)
        Next

        Return list
    End Function

    Public Function FindAllNativeSymbols(ByVal ns As NativeSymbol) As List(Of NativeSymbol)
        If ns Is Nothing Then : Throw New ArgumentNullException("ns") : End If

        Dim list As New List(Of NativeSymbol)
        list.Add(ns)
        Return FindAllNativeSymbols(list)
    End Function

    Public Function FindAllNativeSymbolRelationships(ByVal ns As NativeSymbol) As List(Of NativeSymbolRelationship)
        Dim list As New List(Of NativeSymbol)
        list.Add(ns)
        Return FindAllNativeSymbolRelationships(list)
    End Function

    Public Function FindAllNativeSymbolRelationships(ByVal enumerable As IEnumerable(Of NativeSymbol)) As List(Of NativeSymbolRelationship)
        Return FindAllReachableChildrenImpl(enumerable)
    End Function

    ''' <summary>
    ''' Process all of the reachable children and return the list of found items
    ''' </summary>
    ''' <param name="originalToProcess"></param>
    ''' <remarks></remarks>
    Private Function FindAllReachableChildrenImpl(ByVal originalToProcess As IEnumerable(Of NativeSymbol)) As List(Of NativeSymbolRelationship)

        Dim found As New List(Of NativeSymbolRelationship)
        Dim map As New Dictionary(Of NativeSymbol, Boolean)
        Dim toVisit As New Queue(Of NativeSymbol)(originalToProcess)

        ' First add in all of the original symbols with no parents
        For Each orig As NativeSymbol In originalToProcess
            found.Add(New NativeSymbolRelationship(Nothing, orig))
        Next

        While toVisit.Count > 0
            Dim cur As NativeSymbol = toVisit.Dequeue()
            If map.ContainsKey(cur) Then
                Continue While
            End If

            map(cur) = True
            For Each child As NativeSymbol In cur.GetChildren()
                found.Add(New NativeSymbolRelationship(cur, child))
                toVisit.Enqueue(child)
            Next
        End While

        Return found
    End Function


End Class
