' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic
Imports PInvoke

Namespace Transform

    Public Enum SalEntryListType
        Pre
        Item
        Post
    End Enum

    Public Class SalEntry
        Public Type As SalEntryType
        Public Text As String

        Public Sub New(ByVal type As SalEntryType)
            Me.Type = type
            Me.Text = String.Empty
        End Sub

        Public Sub New(ByVal other As NativeSalEntry)
            Me.Type = other.SalEntryType
            Me.Text = other.Text.Trim(" "c)
        End Sub
    End Class

    ''' <summary>
    ''' Set of SAL annotation entries
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SalEntrySet
        Private _type As SalEntryListType
        Private _list As New List(Of SalEntry)

        Public Property Type() As SalEntryListType
            Get
                Return _type
            End Get
            Set(ByVal value As SalEntryListType)
                _type = value
            End Set
        End Property

        Public ReadOnly Property List() As List(Of SalEntry)
            Get
                Return _list
            End Get
        End Property

        Public Sub New(ByVal type As SalEntryListType)
            _type = type
        End Sub

        Public Function FindEntry(ByVal type As SalEntryType) As SalEntry
            For Each entry As SalEntry In List
                If entry.Type = type Then
                    Return entry
                End If
            Next

            Return Nothing
        End Function
    End Class

    ''' <summary>
    ''' Used to analyze SAL attributes
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SalAnalyzer
        Private _sal As NativeSalAttribute
        Private _preList As New List(Of SalEntrySet)
        Private _itemList As New List(Of SalEntrySet)
        Private _postList As New List(Of SalEntrySet)

        Public Sub New(ByVal sal As NativeSalAttribute)
            _sal = sal
            BuildLists()
        End Sub

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Return _preList.Count = 0 AndAlso _postList.Count = 0
            End Get
        End Property

#Region "Loose Directional Mappings"

        Public Function IsValidOut() As Boolean
            Return FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) IsNot Nothing _
                    OrElse FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly, SalEntryType.ExceptThat, SalEntryType.MaybeNull) IsNot Nothing
        End Function

        Public Function IsValidOutOnly() As Boolean
            Return IsValidOut() AndAlso Not IsValidIn()
        End Function

        Public Function IsValidIn() As Boolean
            Return FindPre(SalEntryType.Valid) IsNot Nothing
        End Function

        Public Function IsValidInOnly() As Boolean
            Return IsValidIn() AndAlso Not IsValidOut()
        End Function

        Public Function IsValidInOut() As Boolean
            Return IsValidIn() AndAlso IsValidOut()
        End Function

#End Region

#Region "Strict SAL mappings"

        ''' <summary>
        ''' Is this a single in pointer
        ''' 
        ''' __in
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsIn() As Boolean
            If _preList.Count <> 2 OrElse _itemList.Count <> 0 OrElse _postList.Count <> 0 Then
                Return False
            End If

            If FindPre(SalEntryType.Valid) Is Nothing _
                OrElse FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) Is Nothing Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Is this a single in pointer
        ''' 
        ''' __in_opt
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsInOptional() As Boolean
            If _preList.Count <> 2 OrElse _itemList.Count <> 0 OrElse _postList.Count <> 0 Then
                Return False
            End If

            If FindPre(SalEntryType.Valid) Is Nothing _
                OrElse FindPre(SalEntryType.Deref, SalEntryType.ReadOnly, SalEntryType.ExceptThat, SalEntryType.MaybeNull) Is Nothing Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Is this a single out pointer
        ''' 
        ''' __out
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsOut() As Boolean
            Dim size As String = Nothing
            If Not IsOutElementBuffer(size) Or 0 <> String.CompareOrdinal("1", size) Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Is this a single in/out pointer
        ''' 
        ''' __inout 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsInOut() As Boolean

            If _preList.Count <> 1 OrElse _itemList.Count <> 0 OrElse _postList.Count <> 1 Then
                Return False
            End If

            If FindPre(SalEntryType.Valid) Is Nothing _
                OrElse FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) Is Nothing Then

                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Is this a in element buffer
        ''' 
        ''' __in_ecount(size)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsInElementBuffer(ByRef sizeArg As String) As Boolean
            If _preList.Count <> 3 OrElse _itemList.Count <> 0 OrElse _postList.Count <> 0 Then
                Return False
            End If

            Dim bufSet As SalEntrySet = FindPre(SalEntryType.ElemReadableTo)
            If FindPre(SalEntryType.Valid) Is Nothing _
                OrElse FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) Is Nothing _
                OrElse bufSet Is Nothing Then
                Return False
            End If

            sizeArg = bufSet.FindEntry(SalEntryType.ElemReadableTo).Text
            Return True
        End Function

        ''' <summary>
        ''' Is this an optional in element buffer
        ''' 
        ''' __in_ecount_opt(size)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsInElementBufferOptional(ByRef sizeArg As String) As Boolean
            If _preList.Count <> 3 OrElse _itemList.Count <> 0 OrElse _postList.Count <> 0 Then
                Return False
            End If

            Dim bufSet As SalEntrySet = FindPre(SalEntryType.ElemReadableTo, SalEntryType.ExceptThat, SalEntryType.MaybeNull)
            If FindPre(SalEntryType.Valid) Is Nothing _
                OrElse FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) Is Nothing _
                OrElse bufSet Is Nothing Then
                Return False
            End If

            sizeArg = bufSet.FindEntry(SalEntryType.ElemReadableTo).Text
            Return True
        End Function

        ''' <summary>
        ''' Is this a in byte buffer
        ''' 
        ''' __in_bcount(size)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsInByteBuffer(ByRef sizeArg As String) As Boolean
            If _preList.Count <> 3 OrElse _itemList.Count <> 0 OrElse _postList.Count <> 0 Then
                Return False
            End If

            Dim bufSet As SalEntrySet = FindPre(SalEntryType.ByteReadableTo)
            If FindPre(SalEntryType.Valid) Is Nothing _
                OrElse FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) Is Nothing _
                OrElse bufSet Is Nothing Then
                Return False
            End If

            sizeArg = bufSet.FindEntry(SalEntryType.ByteReadableTo).Text
            Return True
        End Function

        ''' <summary>
        ''' Is this an optional in byte buffer
        ''' 
        ''' __in_bcount_opt(size)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsInByteBufferOptional(ByRef sizeArg As String) As Boolean
            If _preList.Count <> 3 OrElse _itemList.Count <> 0 OrElse _postList.Count <> 0 Then
                Return False
            End If

            Dim bufSet As SalEntrySet = FindPre(SalEntryType.ByteReadableTo, SalEntryType.ExceptThat, SalEntryType.MaybeNull)
            If FindPre(SalEntryType.Valid) Is Nothing _
                OrElse FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) Is Nothing _
                OrElse bufSet Is Nothing Then
                Return False
            End If

            sizeArg = bufSet.FindEntry(SalEntryType.ByteReadableTo).Text
            Return True
        End Function

        Public Function IsOutElementBuffer() As Boolean
            Dim sizeArg As String = Nothing
            Return IsOutElementBuffer(sizeArg)
        End Function

        ''' <summary>
        ''' Is this an out parameter that is a buffer of elements
        ''' 
        ''' __out_ecount(sizeArg)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsOutElementBuffer(ByRef sizeArg As String) As Boolean
            If _preList.Count <> 0 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 1 Then
                Return False
            End If

            Dim bufSet As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo)
            If FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) Is Nothing _
                OrElse bufSet Is Nothing Then
                Return False
            End If

            sizeArg = bufSet.FindEntry(SalEntryType.ElemWritableTo).Text
            Return True
        End Function


        Public Function IsOutElementBufferOptional() As Boolean
            Dim sizeArg As String = Nothing
            Return IsOutElementBufferOptional(sizeArg)
        End Function

        ''' <summary>
        ''' Is this an out parameter that is a buffer of elements
        ''' 
        ''' __out_ecount_opt(sizeArg)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsOutElementBufferOptional(ByRef sizeArg As String) As Boolean
            If _preList.Count <> 0 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 1 Then
                Return False
            End If

            Dim bufSet As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo)
            If FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly, SalEntryType.ExceptThat, SalEntryType.MaybeNull) Is Nothing _
                OrElse bufSet Is Nothing Then
                Return False
            End If

            sizeArg = bufSet.FindEntry(SalEntryType.ElemWritableTo).Text
            Return True
        End Function

        Public Function IsOutPartElementBuffer() As Boolean
            Dim sizeArg As String = Nothing
            Dim readableArg As String = Nothing

            Return IsOutPartElementBuffer(sizeArg, readableArg)
        End Function

        ''' <summary>
        ''' Is this a partially readable buffer
        ''' 
        ''' __out_ecount_part(sizeArg, readableArg)
        ''' </summary>
        ''' <param name="writableSize"></param>
        ''' <param name="readableSize"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsOutPartElementBuffer(ByRef writableSize As String, ByRef readableSize As String) As Boolean
            If _preList.Count <> 0 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 2 Then
                Return False
            End If

            Dim sizeBuf As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo)
            Dim readBuf As SalEntrySet = FindPost(SalEntryType.ElemReadableTo)
            If FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) Is Nothing _
                OrElse sizeBuf Is Nothing _
                OrElse readBuf Is Nothing Then
                Return False
            End If

            writableSize = sizeBuf.FindEntry(SalEntryType.ElemWritableTo).Text
            readableSize = readBuf.FindEntry(SalEntryType.ElemReadableTo).Text
            Return True
        End Function

        Public Function IsOutPartElementBufferOptional() As Boolean
            Dim notUsed1 As String = Nothing
            Dim notUsed2 As String = Nothing
            Return IsOutPartElementBufferOptional(notUsed1, notUsed2)
        End Function

        ''' <summary>
        ''' Is this an optional partially readable buffer
        ''' 
        ''' __out_ecount_part_opt(sizeArg, readableArg)
        ''' </summary>
        ''' <param name="writableSize"></param>
        ''' <param name="readableSize"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsOutPartElementBufferOptional(ByRef writableSize As String, ByRef readableSize As String) As Boolean
            If _preList.Count <> 0 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 2 Then
                Return False
            End If

            Dim sizeBuf As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo)
            Dim readBuf As SalEntrySet = FindPost(SalEntryType.ElemReadableTo, SalEntryType.ExceptThat, SalEntryType.MaybeNull)
            If FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) Is Nothing _
                OrElse sizeBuf Is Nothing _
                OrElse readBuf Is Nothing Then
                Return False
            End If

            writableSize = sizeBuf.FindEntry(SalEntryType.ElemWritableTo).Text
            readableSize = readBuf.FindEntry(SalEntryType.ElemReadableTo).Text
            Return True
        End Function

        ''' <summary>
        ''' Is this an out byte bufffer
        ''' 
        ''' __out_bcount(sizeArg)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsOutByteBuffer(ByRef sizeArg As String) As Boolean
            If _preList.Count <> 0 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 1 Then
                Return False
            End If

            Dim sizeBuf As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo)
            If FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) Is Nothing _
                OrElse sizeBuf Is Nothing Then
                Return False
            End If

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text
            Return True
        End Function

        ''' <summary>
        ''' Is this an optional out byte bufffer
        ''' 
        ''' __out_bcount_opt(sizeArg)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsOutByteBufferOptional(ByRef sizeArg As String) As Boolean
            If _preList.Count <> 0 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 1 Then
                Return False
            End If

            Dim sizeBuf As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo)
            If FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly, SalEntryType.ExceptThat, SalEntryType.MaybeNull) Is Nothing _
                OrElse sizeBuf Is Nothing Then
                Return False
            End If

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text
            Return True
        End Function

        ''' <summary>
        ''' Is this an out byte bufffer which is partiaally readable
        ''' 
        ''' __out_bcount_part(sizeArg, readableArg)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsOutPartByteBuffer(ByRef sizeArg As String, ByRef readableArg As String) As Boolean
            If _preList.Count <> 0 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 2 Then
                Return False
            End If

            Dim sizeBuf As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo)
            Dim readBuf As SalEntrySet = FindPost(SalEntryType.ByteReadableTo)
            If FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) Is Nothing _
                OrElse sizeBuf Is Nothing _
                OrElse readBuf Is Nothing Then
                Return False
            End If

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text
            readableArg = readBuf.FindEntry(SalEntryType.ByteReadableTo).Text
            Return True
        End Function

        ''' <summary>
        ''' Is this an out byte bufffer which is partiaally readable
        ''' 
        ''' __out_bcount_part_opt(sizeArg, readableArg)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsOutPartByteBufferOptional(ByRef sizeArg As String, ByRef readableArg As String) As Boolean
            If _preList.Count <> 0 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 2 Then
                Return False
            End If

            Dim sizeBuf As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo)
            Dim readBuf As SalEntrySet = FindPost(SalEntryType.ByteReadableTo, SalEntryType.ExceptThat, SalEntryType.MaybeNull)
            If FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) Is Nothing _
                OrElse sizeBuf Is Nothing _
                OrElse readBuf Is Nothing Then
                Return False
            End If

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text
            readableArg = readBuf.FindEntry(SalEntryType.ByteReadableTo).Text
            Return True
        End Function


        Public Function IsInOutElementBuffer() As Boolean
            Dim sizeArg As String = Nothing
            Return IsInOutElementBuffer(sizeArg)
        End Function

        ''' <summary>
        ''' Is this an in/out element buffer
        ''' 
        ''' __inout_ecount(size)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsInOutElementBuffer(ByRef sizeArg As String) As Boolean

            If _preList.Count <> 1 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 1 Then
                Return False
            End If

            Dim sizeBuf As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo)
            If FindPre(SalEntryType.Valid) Is Nothing _
                OrElse FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) Is Nothing _
                OrElse sizeBuf Is Nothing Then

                Return False
            End If

            sizeArg = sizeBuf.FindEntry(SalEntryType.ElemWritableTo).Text
            Return True
        End Function

        ''' <summary>
        ''' Is this an in/out byte buffer
        ''' 
        ''' __inout_bcount(size)
        ''' </summary>
        ''' <param name="sizeArg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsInOutByteBuffer(ByRef sizeArg As String) As Boolean

            If _preList.Count <> 1 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 1 Then
                Return False
            End If

            Dim sizeBuf As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo)
            If FindPre(SalEntryType.Valid) Is Nothing _
                OrElse FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) Is Nothing _
                OrElse sizeBuf Is Nothing Then

                Return False
            End If

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text
            Return True

        End Function

        ''' <summary>
        ''' Is this a __deref_out single element pointer
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsDerefOut() As Boolean
            If _preList.Count <> 0 OrElse _itemList.Count <> 1 OrElse _postList.Count <> 4 Then
                Return False
            End If

            ' Get the item part
            Dim found As SalEntrySet = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo)
            If found Is Nothing OrElse _
                Not StringComparer.Ordinal.Equals("1", found.FindEntry(SalEntryType.ElemWritableTo).Text) Then
                Return False
            End If

            ' Validate first level 
            found = FindPost(SalEntryType.ElemReadableTo)
            If found Is Nothing OrElse _
                Not StringComparer.Ordinal.Equals("1", found.FindEntry(SalEntryType.ElemReadableTo).Text) Then
                Return False
            End If

            ' Able to dereference the element
            If FindPost(SalEntryType.Deref, SalEntryType.NotNull) Is Nothing Then
                Return False
            End If

            found = FindPost(SalEntryType.Deref, SalEntryType.ElemWritableTo)
            If found Is Nothing OrElse _
                Not StringComparer.Ordinal.Equals("1", found.FindEntry(SalEntryType.ElemWritableTo).Text) Then
                Return False
            End If

            found = FindPost(SalEntryType.Deref, SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly)
            If found Is Nothing Then
                Return False
            End If

            Return True
        End Function

#End Region

        Public Function Find(ByVal type As SalEntryListType, ByVal ParamArray args As SalEntryType()) As SalEntrySet
            Select Case type
                Case SalEntryListType.Item
                    Return FindItem(args)
                Case SalEntryListType.Pre
                    Return FindPre(args)
                Case SalEntryListType.Post
                    Return FindPost(args)
                Case Else
                    InvalidEnumValue(type)
                    Return Nothing
            End Select
        End Function

        Public Function FindPost(ByVal ParamArray args As SalEntryType()) As SalEntrySet
            Return FindSet(_postList, args)
        End Function

        Public Function FindPre(ByVal ParamArray args As SalEntryType()) As SalEntrySet
            Return FindSet(_preList, args)
        End Function

        Public Function FindItem(ByVal ParamArray args As SalEntryType()) As SalEntrySet
            Return FindSet(_itemList, args)
        End Function

        ''' <summary>
        ''' Try and find a SalEntrySet with the specified entries
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function FindSet(ByVal list As List(Of SalEntrySet), ByVal args As SalEntryType()) As SalEntrySet
            For Each item As SalEntrySet In list
                If item.List.Count = args.Length Then
                    Dim match As Boolean = True
                    For i As Int32 = 0 To args.Length - 1
                        If item.List(i).Type <> args(i) Then
                            match = False
                            Exit For
                        End If
                    Next

                    If match Then
                        Return item
                    End If
                End If
            Next

            Return Nothing
        End Function

        Private Sub BuildLists()
            ThrowIfNull(_sal)
            If _sal.IsEmpty() Then
                Return
            End If

            Dim list As New List(Of NativeSalEntry)(_sal.SalEntryList)
            Dim dest As New List(Of SalEntrySet)
            Dim cur As SalEntrySet
            If list(0).SalEntryType = SalEntryType.Post Then
                cur = New SalEntrySet(SalEntryListType.Post)
                list.RemoveAt(0)
            ElseIf list(0).SalEntryType = SalEntryType.Pre Then
                cur = New SalEntrySet(SalEntryListType.Pre)
                list.RemoveAt(0)
            Else
                cur = New SalEntrySet(SalEntryListType.Item)
            End If

            For i As Int32 = 0 To list.Count - 1
                Dim entry As NativeSalEntry = list(i)
                If entry.SalEntryType = SalEntryType.Pre Then
                    dest.Add(cur)
                    cur = New SalEntrySet(SalEntryListType.Pre)
                ElseIf entry.SalEntryType = SalEntryType.Post Then
                    dest.Add(cur)
                    cur = New SalEntrySet(SalEntryListType.Post)
                Else
                    cur.List.Add(New SalEntry(entry))
                End If
            Next
            dest.Add(cur)

            For Each l As SalEntrySet In dest
                If l.List.Count = 0 Then
                    Continue For
                End If

                Select Case l.Type
                    Case SalEntryListType.Post
                        _postList.Add(l)
                    Case SalEntryListType.Pre
                        _preList.Add(l)
                    Case SalEntryListType.Item
                        _itemList.Add(l)
                    Case Else
                        InvalidEnumValue(l.Type)
                End Select
            Next

        End Sub

    End Class

End Namespace
