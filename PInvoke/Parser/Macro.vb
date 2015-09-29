' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Text.RegularExpressions

Namespace Parser


    ''' <summary>
    ''' Represents a macro in native code
    ''' </summary>
    ''' <remarks></remarks>
    <DebuggerDisplay("{Name} -> {Value}")> _
    Public Class Macro
        Private _name As String
        Private _value As String
        Private _isMethod As Boolean
        Private _isPermanent As Boolean
        Private _isFromParse As Boolean = True

        ''' <summary>
        ''' Name of the Macro
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                _name = value
            End Set
        End Property

        ''' <summary>
        ''' Value of the macro
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Value() As String
            Get
                Return _value
            End Get
            Set(ByVal val As String)
                _value = val
            End Set
        End Property

        ''' <summary>
        ''' Whether or not this is a method style macro
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsMethod() As Boolean
            Get
                Return _isMethod
            End Get
            Set(ByVal value As Boolean)
                _isMethod = value
            End Set
        End Property

        ''' <summary>
        ''' Represents a macro that cannot be overriden by user code.  
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsPermanent() As Boolean
            Get
                Return _isPermanent
            End Get
            Set(ByVal value As Boolean)
                _isPermanent = value
            End Set
        End Property

        ''' <summary>
        ''' Is this macro created from actually parsing code?  The alternate is that the 
        ''' macro is added to the initial set of macros.  This allows the parser to determine
        ''' what is actually a part of the parsed code as opposed to the setup code
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Property IsFromParse() As Boolean
            Get
                Return _isFromParse
            End Get
            Set(ByVal value As Boolean)
                _isFromParse = value
            End Set
        End Property

        Public Sub New(ByVal name As String)
            _name = name
        End Sub

        Public Sub New(ByVal name As String, ByVal val As String)
            MyClass.New(name, val, False)
        End Sub

        Public Sub New(ByVal name As String, ByVal val As String, ByVal permanent As Boolean)
            _name = name
            _value = val
            _isPermanent = permanent
        End Sub

    End Class

    ''' <summary>
    ''' Macros that are methods
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MethodMacro
        Inherits Macro

        Private _paramList As List(Of String)
        Private _bodyList As List(Of Token)
        Private _fullBodyList As List(Of Token)

        ''' <summary>
        ''' Text parameters of the macro
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Parameters() As List(Of String)
            Get
                Return _paramList
            End Get
        End Property

        ''' <summary>
        ''' Tokens inside the macro body minus any whitespace characters
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Body() As List(Of Token)
            Get
                Return _bodyList
            End Get
        End Property

        ''' <summary>
        ''' Tokens inside the macro body including anywhitespace characters
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FullBody() As List(Of Token)
            Get
                Return _fullBodyList
            End Get
        End Property

        ''' <summary>
        ''' Get the text of the method signature
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MethodSignature() As String
            Get
                Dim b As New Text.StringBuilder()
                b.Append("(")
                For i As Integer = 0 To _paramList.Count - 1
                    b.Append(_paramList(i))
                    If i + 1 < _paramList.Count Then
                        b.Append(",")
                    End If
                Next
                b.Append(") ")
                For Each cur As Token In FullBody
                    b.Append(cur.Value)
                Next

                Return b.ToString()
            End Get
        End Property

        Public Sub New(ByVal name As String, ByVal paramList As List(Of String), ByVal body As List(Of Token), ByVal fullBody As List(Of Token))
            MyBase.New(name)
            MyBase.Value = name & "()"
            MyBase.IsMethod = True

            _paramList = paramList
            _bodyList = body
            _fullBodyList = fullBody
        End Sub

        Public Function Replace(ByVal argList As List(Of Token)) As List(Of Token)
            If argList.Count <> _paramList.Count Then
                Return New List(Of Token)
            End If

            ' Replace is done in 2 passes.  The first puts the arguments into the token stream.
            Dim retList As New List(Of Token)
            For Each item As Token In _bodyList
                If item.TokenType <> TokenType.Word Then
                    retList.Add(item)
                Else
                    Dim index As Int32 = _paramList.IndexOf(item.Value)
                    If index >= 0 Then
                        retList.Add(argList(index))
                    Else
                        retList.Add(item)
                    End If
                End If
            Next

            ' Second phase, process all of the # entries 
            Dim i As Int32 = 0
            While i < retList.Count - 1
                Dim curToken As Token = retList(i)
                Dim nextToken As Token = retList(i + 1)
                If curToken.TokenType = TokenType.Pound Then

                    If nextToken.TokenType = TokenType.Pound Then
                        ' Don't accidentally process a ## as a # token
                        i += 1
                    ElseIf argList.IndexOf(nextToken) >= 0 Then
                        If nextToken.IsQuotedString Then
                            ' Already quoted so it doesn't need to be quoted again
                            retList.RemoveAt(i)
                            i += 1
                        Else
                            ' Quote me macro
                            retList(i) = New Token(TokenType.QuotedStringAnsi, """" & nextToken.Value & """")
                            retList.RemoveAt(i + 1)
                        End If
                    End If
                End If

                i += 1
            End While

            Return retList
        End Function

        Public Shared Function TryCreateFromDeclaration(ByVal name As String, ByVal body As String, ByRef method As MethodMacro) As Boolean
            Try
                Dim engine As New PreProcessorEngine(New PreProcessorOptions)
                Using reader As New IO.StringReader("#define " & name & body)
                    engine.Process(New TextReaderBag(reader))
                    Dim created As Macro = Nothing
                    If engine.MacroMap.TryGetValue(name, created) AndAlso created.IsMethod Then
                        method = DirectCast(created, MethodMacro)
                        Return True
                    End If
                End Using
            Catch
                Return False
            End Try

            Return False
        End Function

    End Class
End Namespace
