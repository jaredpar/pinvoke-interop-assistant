' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Text.RegularExpressions

Namespace Parser


    ''' <summary>
    ''' Represents a macro in native code
    ''' </summary>
    ''' <remarks></remarks>
    <DebuggerDisplay("{Name} -> {Value}")> _
    Public Class Macro
        Private m_name As String
        Private m_value As String
        Private m_isMethod As Boolean
        Private m_isPermanent As Boolean
        Private m_isFromParse As Boolean = True

        ''' <summary>
        ''' Name of the Macro
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                m_name = value
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
                Return m_value
            End Get
            Set(ByVal val As String)
                m_value = val
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
                Return m_isMethod
            End Get
            Set(ByVal value As Boolean)
                m_isMethod = value
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
                Return m_isPermanent
            End Get
            Set(ByVal value As Boolean)
                m_isPermanent = value
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
                Return m_isFromParse
            End Get
            Set(ByVal value As Boolean)
                m_isFromParse = value
            End Set
        End Property

        Public Sub New(ByVal name As String)
            m_name = name
        End Sub

        Public Sub New(ByVal name As String, ByVal val As String)
            MyClass.New(name, val, False)
        End Sub

        Public Sub New(ByVal name As String, ByVal val As String, ByVal permanent As Boolean)
            m_name = name
            m_value = val
            m_isPermanent = permanent
        End Sub

    End Class

    ''' <summary>
    ''' Macros that are methods
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MethodMacro
        Inherits Macro

        Private m_paramList As List(Of String)
        Private m_bodyList As List(Of Token)
        Private m_fullBodyList As List(Of Token)

        ''' <summary>
        ''' Text parameters of the macro
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Parameters() As List(Of String)
            Get
                Return m_paramList
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
                Return m_bodyList
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
                Return m_fullBodyList
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
                For i As Integer = 0 To m_paramList.Count - 1
                    b.Append(m_paramList(i))
                    If i + 1 < m_paramList.Count Then
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

            m_paramList = paramList
            m_bodyList = body
            m_fullBodyList = fullBody
        End Sub

        Public Function Replace(ByVal argList As List(Of Token)) As List(Of Token)
            If argList.Count <> m_paramList.Count Then
                Return New List(Of Token)
            End If

            ' Replace is done in 2 passes.  The first puts the arguments into the token stream.
            Dim retList As New List(Of Token)
            For Each item As Token In m_bodyList
                If item.TokenType <> TokenType.Word Then
                    retList.Add(item)
                Else
                    Dim index As Int32 = m_paramList.IndexOf(item.Value)
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
