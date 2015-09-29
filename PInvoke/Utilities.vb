' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic

#Region "Constants"

Public Module Constants

    Public Const ProductName As String = "PInvoke Interop Assistant"
    Public Const Version As String = "1.0.0.0"
    Public Const FriendlyVersion As String = "1.0"

End Module

#End Region

#Region "ErrorProvider"

''' <summary>
''' Provides an encapsulation for error messages and warnings
''' </summary>
''' <remarks></remarks>
Public Class ErrorProvider

    Private m_warningList As New List(Of String)
    Private m_errorList As New List(Of String)

    ''' <summary>
    ''' Errors
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Errors() As List(Of String)
        Get
            Return m_errorList
        End Get
    End Property

    ''' <summary>
    ''' Warnings
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Warnings() As List(Of String)
        Get
            Return m_warningList
        End Get
    End Property

    ''' <summary>
    ''' All messages 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property AllMessages() As IEnumerable(Of String)
        Get
            Dim list As New List(Of String)
            list.AddRange(m_warningList)
            list.AddRange(m_errorList)
            Return list
        End Get
    End Property

    Public Sub AddWarning(ByVal str As String)
        m_warningList.Add(str)
    End Sub

    Public Sub AddWarning(ByVal str As String, ByVal ParamArray args As Object())
        Dim msg As String = String.Format(str, args)
        m_warningList.Add(msg)
    End Sub

    Public Sub AddError(ByVal str As String)
        m_errorList.Add(str)
    End Sub

    Public Sub AddError(ByVal str As String, ByVal ParamArray args As Object())
        Dim msg As String = String.Format(str, args)
        m_errorList.Add(msg)
    End Sub

    ''' <summary>
    ''' Append the data in the passed in ErrorProvider into this instance
    ''' </summary>
    ''' <param name="ep"></param>
    ''' <remarks></remarks>
    Public Sub Append(ByVal ep As ErrorProvider)
        m_errorList.AddRange(ep.Errors)
        m_warningList.AddRange(ep.Warnings)
    End Sub

    Public Sub New()

    End Sub

    Public Sub New(ByVal ep As ErrorProvider)
        Append(ep)
    End Sub

    Public Function CreateDisplayString() As String
        Dim builder As New Text.StringBuilder()
        For Each msg As String In Errors
            builder.AppendFormat("Error: {0}", msg)
            builder.AppendLine()
        Next

        For Each msg As String In Warnings
            builder.AppendFormat("Warning: {0}", msg)
            builder.AppendLine()
        Next

        Return builder.ToString()
    End Function

End Class

#End Region

#Region "EnumerableShim"

Friend Class EnumerableShim(Of T)
    Implements IEnumerable(Of T)

    Private Class EnumeratorShim
        Implements IEnumerator(Of T)

        Private m_enumerator As IEnumerator

        Public Sub New(ByVal e As IEnumerator)
            m_enumerator = e
        End Sub

        Public ReadOnly Property Current() As T Implements System.Collections.Generic.IEnumerator(Of T).Current
            Get
                Return DirectCast(m_enumerator.Current, T)
            End Get
        End Property

        Public ReadOnly Property Current1() As Object Implements System.Collections.IEnumerator.Current
            Get
                Return m_enumerator.Current
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
            Return m_enumerator.MoveNext()
        End Function

        Public Sub Reset() Implements System.Collections.IEnumerator.Reset
            m_enumerator.Reset()
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            ' Nothing to dispose here
        End Sub
#End Region

    End Class

    Private m_enumerable As IEnumerable

    Public Sub New(ByVal e As IEnumerable)
        m_enumerable = e
    End Sub

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of T) Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator
        Return New EnumeratorShim(m_enumerable.GetEnumerator())
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return m_enumerable.GetEnumerator()
    End Function
End Class

#End Region

#Region "EnumUtil"

Public Module EnumUtil

    Public Function GetAllValues(Of T)() As List(Of T)
        Dim list As New List(Of T)
        For Each cur As T In System.Enum.GetValues(GetType(T))
            list.Add(cur)
        Next

        Return list
    End Function

    Public Function GetAllValuesObject(Of T)() As Object()
        Dim list As New List(Of Object)
        For Each cur As T In System.Enum.GetValues(GetType(T))
            list.Add(cur)
        Next

        Return list.ToArray()
    End Function

    Public Function GetAllValuesObjectExcept(Of T)(ByVal except As T) As Object()
        Dim comp As EqualityComparer(Of T) = EqualityComparer(Of T).Default
        Dim list As New List(Of Object)
        For Each cur As T In System.Enum.GetValues(GetType(T))
            If Not comp.Equals(cur, except) Then
                list.Add(cur)
            End If
        Next

        Return list.ToArray()
    End Function

End Module

#End Region
