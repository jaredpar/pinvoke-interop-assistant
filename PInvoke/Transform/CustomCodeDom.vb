' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.Diagnostics.CodeAnalysis
Imports System.Text

Namespace Transform

    Friend MustInherit Class CodeCustomExpression
        Inherits CodeSnippetExpression

        Private _lang As LanguageType

        Protected Property LanguageType() As LanguageType
            Get
                Return _lang
            End Get
            Set(ByVal value As LanguageType)
                _lang = value
                UpdateValue()
            End Set
        End Property

        Protected Sub New(ByVal lang As LanguageType)
            _lang = lang
        End Sub

        Public Sub ResetValue()
            UpdateValue()
        End Sub

        Protected MustOverride Sub UpdateValue()

        Protected Function GetProvider() As CodeDomProvider
            Select Case _lang
                Case Transform.LanguageType.CSharp
                    Return New Microsoft.CSharp.CSharpCodeProvider()
                Case Transform.LanguageType.VisualBasic
                    Return New Microsoft.VisualBasic.VBCodeProvider()
                Case Else
                    InvalidEnumValue(_lang)
                    Return Nothing
            End Select
        End Function

    End Class


    ''' <summary>
    ''' Used to perform a Not/! operation in a particular language
    ''' </summary>C
    ''' <remarks></remarks>
    Friend Class CodeNotExpression
        Inherits CodeCustomExpression

        Private _expr As CodeExpression

        Friend Property Expression() As CodeExpression
            Get
                Return _expr
            End Get
            Set(ByVal value As CodeExpression)
                _expr = value
                UpdateValue()
            End Set
        End Property


        Friend Sub New(ByVal lang As LanguageType, ByVal expr As CodeExpression)
            MyBase.New(lang)
            _expr = expr
            UpdateValue()
        End Sub

        <SuppressMessage("Microsoft.Security", "CA2122")> _
        Protected Overrides Sub UpdateValue()
            Dim provider As CodeDomProvider = GetProvider()
            Dim prefix As String
            Select Case Me.LanguageType
                Case Transform.LanguageType.CSharp
                    prefix = "! ("
                Case Transform.LanguageType.VisualBasic
                    prefix = "Not ("
                Case Else
                    InvalidEnumValue(Me.LanguageType)
                    Return
            End Select

            Using writer As New IO.StringWriter()
                provider.GenerateCodeFromExpression(_expr, writer, New CodeGeneratorOptions())
                Value = prefix & writer.ToString() & ")"
            End Using
        End Sub

    End Class

    ''' <summary>
    ''' Implements a bitshift expression
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class CodeShiftExpression
        Inherits CodeCustomExpression

        Private _leftExpr As CodeExpression
        Private _rightExpr As CodeExpression
        Private _shiftLeft As Boolean

        Public ReadOnly Property Left() As CodeExpression
            Get
                Return _leftExpr
            End Get
        End Property

        Public ReadOnly Property Right() As CodeExpression
            Get
                Return _rightExpr
            End Get
        End Property

        Friend Sub New(ByVal lang As LanguageType, ByVal shiftLeft As Boolean, ByVal left As CodeExpression, ByVal right As CodeExpression)
            MyBase.New(lang)
            _leftExpr = left
            _rightExpr = right
            _shiftLeft = shiftLeft
            UpdateValue()
        End Sub

        <SuppressMessage("Microsoft.Security", "CA2122")> _
        Protected Overrides Sub UpdateValue()
            Dim provider As CodeDomProvider = MyBase.GetProvider()
            Dim expr As String = "("

            Using writer As New IO.StringWriter
                provider.GenerateCodeFromExpression(_leftExpr, writer, New CodeGeneratorOptions())
                expr += writer.ToString()
                expr += ")"
            End Using

            If _shiftLeft Then
                expr += " << "
            Else
                expr += " >> "
            End If

            Using writer As New IO.StringWriter
                provider.GenerateCodeFromExpression(_rightExpr, writer, New CodeGeneratorOptions())
                expr += String.Format("({0})", writer.ToString())
            End Using

            Value = expr
        End Sub

    End Class


    ''' <summary>
    ''' Used to perform a - operation in a particular language
    ''' </summary>C
    ''' <remarks></remarks>
    Friend Class CodeNegativeExpression
        Inherits CodeCustomExpression

        Private _expr As CodeExpression

        Friend Property Expression() As CodeExpression
            Get
                Return _expr
            End Get
            Set(ByVal value As CodeExpression)
                _expr = value
                UpdateValue()
            End Set
        End Property


        Friend Sub New(ByVal lang As LanguageType, ByVal expr As CodeExpression)
            MyBase.New(lang)
            _expr = expr
            UpdateValue()
        End Sub

        <SuppressMessage("Microsoft.Security", "CA2122")> _
        Protected Overrides Sub UpdateValue()
            Dim provider As CodeDomProvider = GetProvider()
            Using writer As New IO.StringWriter()
                provider.GenerateCodeFromExpression(_expr, writer, New CodeGeneratorOptions())
                Value = "-" & writer.ToString()
            End Using
        End Sub

    End Class

    ''' <summary>
    ''' Used in C# when we need to pass a parameter in a specific direction (ref or out)
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class CodeDirectionalSymbolExpression
        Inherits CodeCustomExpression

        Private _symbolExpr As CodeExpression
        Private _direction As FieldDirection

        Public ReadOnly Property Expression() As CodeExpression
            Get
                Return _symbolExpr
            End Get
        End Property

        Private Sub New(ByVal lang As LanguageType, ByVal symbolExpr As CodeExpression, ByVal direction As FieldDirection)
            MyBase.New(lang)
            _symbolExpr = symbolExpr
            _direction = direction
        End Sub

        <SuppressMessage("Microsoft.Security", "CA2122")> _
        Protected Overrides Sub UpdateValue()
            Dim provider As CodeDomProvider = MyBase.GetProvider()
            Dim expr As String
            Select Case _direction
                Case FieldDirection.Out
                    expr = "out "
                Case FieldDirection.Ref
                    expr = "ref "
                Case Else
                    expr = String.Empty
            End Select

            Using writer As New IO.StringWriter
                provider.GenerateCodeFromExpression(_symbolExpr, writer, New CodeGeneratorOptions())
                expr += writer.ToString()
            End Using

            Value = expr
        End Sub

        Public Shared Function Create(ByVal lang As LanguageType, ByVal symbolExpr As CodeExpression, ByVal dir As FieldDirection) As CodeExpression
            If lang = Transform.LanguageType.VisualBasic Then
                Return symbolExpr
            End If

            Return New CodeDirectionalSymbolExpression(lang, symbolExpr, dir)
        End Function
    End Class

End Namespace
