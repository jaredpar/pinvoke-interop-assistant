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
        Private m_isSearching As Boolean
        Private m_delayTime As TimeSpan = TimeSpan.FromSeconds(0.2)
        Private m_enumerator As IEnumerator
        Private m_found As New List(Of Object)
        Private m_filter As Filter

        ''' <summary>
        ''' Whether or not the search is completed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsCompleted() As Boolean
            Get
                Return m_enumerator Is Nothing
            End Get
        End Property

        Public Property DelayTime() As TimeSpan
            Get
                Return m_delayTime
            End Get
            Set(ByVal value As TimeSpan)
                m_delayTime = value
            End Set
        End Property

        Public Sub New(ByVal enumerable As IEnumerable, ByVal cb As Filter)
            m_enumerator = enumerable.GetEnumerator()
            m_filter = cb
        End Sub

        Public Sub Cancel()
            If m_enumerator IsNot Nothing Then
                m_enumerator = Nothing
            End If
        End Sub

        Public Function Search() As Result
            If IsCompleted Then
                Dim res2 As New Result
                res2.Completed = True
                res2.AllFound = m_found
                res2.IncrementalFound = New List(Of Object)()
                Return res2
            End If

            Dim start As DateTime = DateTime.Now
            Dim list As New List(Of Object)
            Dim completed As Boolean = False
            Do
                If Not m_enumerator.MoveNext() Then
                    m_enumerator = Nothing
                    completed = True
                    Exit Do
                End If

                Dim cur As Object = m_enumerator.Current
                If m_filter(cur) Then
                    list.Add(cur)
                End If

                If (DateTime.Now - start) > m_delayTime Then
                    Exit Do
                End If
            Loop

            m_found.AddRange(list)

            Dim res As New Result
            res.Completed = completed
            res.AllFound = m_found
            res.IncrementalFound = list
            Return res
        End Function
    End Class

End Namespace
