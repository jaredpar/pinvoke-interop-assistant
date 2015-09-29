' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Windows.Forms
Imports PInvoke

Namespace Controls
    Public Delegate Function Filter(ByVal arg As Object) As Boolean

    Public Class Result
        Public IncrementalFound As List(Of Object)
        Public AllFound As List(Of Object)
        Public Completed As Boolean
    End Class

    ''' <summary>
    ''' Provides a way of doing incremental searches 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class IncrementalSearch
        Private _isSearching As Boolean
        Private _delayTime As TimeSpan = TimeSpan.FromSeconds(0.2)
        Private _enumerator As IEnumerator
        Private _found As New List(Of Object)
        Private _filter As Filter

        ''' <summary>
        ''' Whether or not the search is completed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsCompleted() As Boolean
            Get
                Return _enumerator Is Nothing
            End Get
        End Property

        Public Property DelayTime() As TimeSpan
            Get
                Return _delayTime
            End Get
            Set(ByVal value As TimeSpan)
                _delayTime = value
            End Set
        End Property

        Public Sub New(ByVal enumerable As IEnumerable, ByVal cb As Filter)
            _enumerator = enumerable.GetEnumerator()
            _filter = cb
        End Sub

        Public Sub Cancel()
            If _enumerator IsNot Nothing Then
                _enumerator = Nothing
            End If
        End Sub

        Public Function Search() As Result
            If IsCompleted Then
                Dim res2 As New Result
                res2.Completed = True
                res2.AllFound = _found
                res2.IncrementalFound = New List(Of Object)()
                Return res2
            End If

            Dim start As DateTime = DateTime.Now
            Dim list As New List(Of Object)
            Dim completed As Boolean = False
            Do
                If Not _enumerator.MoveNext() Then
                    _enumerator = Nothing
                    completed = True
                    Exit Do
                End If

                Dim cur As Object = _enumerator.Current
                If _filter(cur) Then
                    list.Add(cur)
                End If

                If (DateTime.Now - start) > _delayTime Then
                    Exit Do
                End If
            Loop

            _found.AddRange(list)

            Dim res As New Result
            res.Completed = completed
            res.AllFound = _found
            res.IncrementalFound = list
            Return res
        End Function
    End Class

End Namespace
