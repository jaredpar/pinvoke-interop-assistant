' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.IO

Namespace Parser

    ''' <summary>
    ''' Module for the constants in the parser
    ''' </summary>
    ''' <remarks></remarks>
    Public Module Constants

        ''' <summary>
        ''' Used when a file name is needed but it's not known
        ''' </summary>
        ''' <remarks></remarks>
        Public UnknownFileName As String = "<unknown>"

    End Module

    ''' <summary>
    ''' Way of passing around a TextReader paired with it's name
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TextReaderBag
        Private _name As String
        Private _reader As TextReader

        ''' <summary>
        ''' Name of the stream
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name() As String
            Get
                Return _name
            End Get
        End Property

        ''' <summary>
        ''' The TextReader
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TextReader() As IO.TextReader
            Get
                Return _reader
            End Get
        End Property

        Public Sub New(ByVal reader As IO.TextReader)
            MyClass.New(Constants.UnknownFileName, reader)
        End Sub

        Public Sub New(ByVal name As String, ByVal reader As IO.TextReader)
            Me._name = name
            Me._reader = reader
        End Sub

    End Class

    Friend Class TriState(Of T)
        Public m_value As T
        Public m_hasValue As Boolean

        Public ReadOnly Property HasValue() As Boolean
            Get
                Return m_hasValue
            End Get
        End Property

        Public ReadOnly Property Value() As T
            Get
                ThrowIfFalse(HasValue)
                Return m_value
            End Get
        End Property

        Public Sub SetValue(ByVal value As T)
            m_hasValue = True
            m_value = value
        End Sub

        Public Sub Clear()
            m_hasValue = False
            m_value = Nothing
        End Sub
    End Class

End Namespace
