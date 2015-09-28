' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Globalization
Imports System.Text.RegularExpressions
Imports PInvoke.Contract

Namespace Parser

    ''' <summary>
    ''' Kind of expression
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum ExpressionKind

        ' Binary operation such as +,-,/ 
        ' Token: Operation
        BinaryOperation

        ' '-' operation.  Left is the value
        NegativeOperation

        ' ! operation, Left is the value
        NegationOperation

        ' Token is the name of the function.  
        ' Left: Value
        ' Right: , if there are more arguments
        FunctionCall

        List

        ' Token: Target Type
        ' Left: Source that is being cast
        Cast

        ' Token: Value of the expression
        Leaf
    End Enum

    ''' <summary>
    ''' Expression Node
    ''' </summary>
    ''' <remarks></remarks>
    <DebuggerDisplay("{DisplayString}")> _
    Public Class ExpressionNode
        Private m_kind As ExpressionKind
        Private m_left As ExpressionNode
        Private m_right As ExpressionNode
        Private m_token As Token
        Private m_parenthesized As Boolean
        Private m_tag As Object

        Public Property Kind() As ExpressionKind
            Get
                Return m_kind
            End Get
            Set(ByVal value As ExpressionKind)
                m_kind = value
            End Set
        End Property

        Public Property LeftNode() As ExpressionNode
            Get
                Return m_left
            End Get
            Set(ByVal value As ExpressionNode)
                m_left = value
            End Set
        End Property

        Public Property RightNode() As ExpressionNode
            Get
                Return m_right
            End Get
            Set(ByVal value As ExpressionNode)
                m_right = value
            End Set
        End Property

        Public Property Token() As Token
            Get
                Return m_token
            End Get
            Set(ByVal value As Token)
                m_token = value
            End Set
        End Property

        Public Property Parenthesized() As Boolean
            Get
                Return m_parenthesized
            End Get
            Set(ByVal value As Boolean)
                m_parenthesized = True
            End Set
        End Property

        Public Property Tag() As Object
            Get
                Return m_tag
            End Get
            Set(ByVal value As Object)
                m_tag = value
            End Set
        End Property

        Public ReadOnly Property DisplayString() As String
            Get
                Dim str As String = String.Empty
                If m_left IsNot Nothing Then
                    str &= "(Left: " & m_left.DisplayString & ")"
                End If

                If m_right IsNot Nothing Then
                    str &= "(Right: " & m_right.DisplayString & ")"
                End If

                If Not String.IsNullOrEmpty(str) Then
                    str = " " & str
                End If

                If m_token Is Nothing Then
                    Return "Nothing" & str
                Else
                    Return m_token.Value & str
                End If
            End Get
        End Property

        Public Sub New(ByVal kind As ExpressionKind, ByVal value As Token)
            m_kind = kind
            m_token = value
        End Sub

        Public Shared Function CreateLeaf(ByVal bValue As Boolean) As ExpressionNode
            Dim token As Token
            If bValue Then
                token = New Token(TokenType.TrueKeyword, "true")
            Else
                token = New Token(TokenType.TrueKeyword, "false")
            End If

            Return New ExpressionNode(ExpressionKind.Leaf, token)
        End Function

        Public Shared Function CreateLeaf(ByVal number As Integer) As ExpressionNode
            Return New ExpressionNode(ExpressionKind.Leaf, New Token(TokenType.Number, number.ToString()))
        End Function

    End Class

    ''' <summary>
    ''' Converts an expression into an expression tree
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ExpressionParser

        Public Function Parse(ByVal expression As String) As ExpressionNode
            Dim node As ExpressionNode = Nothing
            If Not Me.TryParse(expression, node) Then
                Throw New InvalidOperationException("Unable to parse the expression")
            End If

            Return node
        End Function

        Public Function IsParsable(ByVal expression As String) As Boolean
            Dim node As ExpressionNode = Nothing
            Return TryParse(expression, node)
        End Function

        Public Function TryParse(ByVal tokens As List(Of Token), ByRef node As ExpressionNode) As Boolean
            If tokens Is Nothing Then : Throw New ArgumentNullException("tokens") : End If

            Return TryParseComplete(tokens, node)
        End Function

        Public Function TryParse(ByVal expression As String, ByRef node As ExpressionNode) As Boolean
            If expression Is Nothing Then : Throw New ArgumentNullException("expression") : End If

            Using reader As New IO.StringReader(expression)
                Dim scanner As New Scanner(reader)
                scanner.Options.HideNewLines = True
                scanner.Options.HideComments = True
                scanner.Options.HideWhitespace = True
                scanner.Options.ThrowOnEndOfStream = False
                Return TryParseComplete(scanner.Tokenize(), node)
            End Using
        End Function

        Private Function TryParseComplete(ByVal tokens As List(Of Token), ByRef node As ExpressionNode) As Boolean
            Dim cur As ExpressionNode = Nothing
            Dim remaining As List(Of Token) = Nothing
            If Not TryParseCore(tokens, cur, remaining) Then
                Return False
            End If

            If remaining.Count = 0 Then
                node = cur
                Return True
            ElseIf Not remaining(0).IsBinaryOperation _
                AndAlso cur.Parenthesized _
                AndAlso cur.Kind = ExpressionKind.Leaf _
                AndAlso cur.Token.IsAnyWord Then

                ' This is a cast
                cur.Kind = ExpressionKind.Cast
                node = cur
                Return TryParseComplete(remaining, node.LeftNode)
            ElseIf remaining.Count = 1 OrElse Not remaining(0).IsBinaryOperation Then
                Return False
            Else
                Dim right As ExpressionNode = Nothing
                If Not TryParseComplete(remaining.GetRange(1, remaining.Count - 1), right) Then
                    Return False
                End If

                node = New ExpressionNode(ExpressionKind.BinaryOperation, remaining(0))
                node.LeftNode = cur
                node.RightNode = right
                Return True
            End If
        End Function

        Private Function TryParseCore(ByVal tokens As List(Of Token), ByRef node As ExpressionNode, ByRef remaining As List(Of Token)) As Boolean
            ThrowIfNull(tokens)

            If tokens.Count = 0 Then
                Return False
            End If

            ' Single tokens are the easiest
            If tokens.Count = 1 Then
                remaining = New List(Of Token)
                Return TryConvertTokenToExpressionLeafNode(tokens(0), node)
            End If

            Dim leftNode As ExpressionNode = Nothing
            Dim unaryNode As ExpressionNode = Nothing
            Dim nextIndex As Integer = -1

            If tokens.Count > 2 _
                AndAlso tokens(0).IsAnyWord _
                AndAlso tokens(1).TokenType = TokenType.ParenOpen Then

                ' Function call
                Return TryParseFunctionCall(tokens, node, remaining)
            ElseIf tokens(0).TokenType = TokenType.Bang Then
                node = New ExpressionNode(ExpressionKind.NegationOperation, tokens(0))
                Return TryParseCore(tokens.GetRange(1, tokens.Count - 1), node.LeftNode, remaining)
            ElseIf tokens(0).TokenType = TokenType.OpMinus Then
                node = New ExpressionNode(ExpressionKind.NegativeOperation, tokens(0))
                Return TryParseCore(tokens.GetRange(1, tokens.Count - 1), node.LeftNode, remaining)
            ElseIf tokens(0).TokenType = TokenType.ParenOpen Then
                Return TryParseParenExpression(tokens, node, remaining)
            ElseIf tokens.Count > 2 Then
                ' Has to be an operation so convert the left node to a leaf expression
                remaining = tokens.GetRange(1, tokens.Count - 1)
                Return TryConvertTokenToExpressionLeafNode(tokens(0), node)
            Else
                Return False
            End If
        End Function

        Private Function TryParseFunctionCall(ByVal tokens As List(Of Token), ByRef node As ExpressionNode, ByRef remaining As List(Of Token)) As Boolean
            ThrowIfTrue(tokens.Count < 3)

            node = New ExpressionNode(ExpressionKind.FunctionCall, tokens(0))

            ' Find the last index 
            Dim endIndex As Integer = FindMatchingParenIndex(tokens, 2)
            If endIndex = -1 Then
                Return False
            End If

            ' If there is more than just word() then there are arguments
            If tokens.Count > 3 Then
                Dim subList As List(Of Token) = tokens.GetRange(2, endIndex - 2)
                If Not TryParseFunctionCallArguments(subList, node) Then
                    Return False
                End If
            End If

            remaining = tokens.GetRange(endIndex + 1, tokens.Count - (endIndex + 1))
            Return True
        End Function

        Private Function TryParseFunctionCallArguments(ByVal tokens As List(Of Token), ByVal callNode As ExpressionNode) As Boolean
            ThrowIfNull(callNode)
            ThrowIfFalse(ExpressionKind.FunctionCall = callNode.Kind)

            ' Start the list
            Dim cur As ExpressionNode = callNode

            While tokens.Count > 0
                Dim index As Integer = FindNextCallArgumentSeparator(tokens)
                If index < 0 Then
                    ' No more separators so just parse out the rest of the tokens as an argument
                    If Not TryParseComplete(tokens, cur.LeftNode) Then
                        Return False
                    End If
                    tokens.Clear()
                Else
                    If Not TryParseComplete(tokens.GetRange(0, index), cur.LeftNode) Then
                        Return False
                    End If

                    tokens = tokens.GetRange(index + 1, tokens.Count - (index + 1))
                End If

                If tokens.Count > 0 Then
                    cur.RightNode = New ExpressionNode(ExpressionKind.List, New Token(TokenType.Comma, ","))
                    cur = cur.RightNode
                End If
            End While

            Return True
        End Function

        Private Function TryParseParenExpression(ByVal tokens As List(Of Token), ByRef node As ExpressionNode, ByRef remaining As List(Of Token)) As Boolean
            Dim endIndex As Integer = FindMatchingParenIndex(tokens, 1)
            If endIndex = -1 Then
                Return False
            End If

            remaining = tokens.GetRange(endIndex + 1, tokens.Count - (endIndex + 1))
            Dim success As Boolean = TryParseComplete(tokens.GetRange(1, endIndex - 1), node)
            If success Then
                node.Parenthesized = True
            End If

            Return success
        End Function

        Private Function FindNextCallArgumentSeparator(ByVal tokens As List(Of Token)) As Integer
            ThrowIfNull(tokens)

            For i As Integer = 0 To tokens.Count - 1
                If tokens(i).TokenType = TokenType.Comma Then
                    Return i
                End If
            Next

            Return -1
        End Function

        Private Function TryConvertTokenToExpressionBinaryOperation(ByVal token As Token, ByRef node As ExpressionNode) As Boolean
            ThrowIfNull(token)

            Dim isvalid As Boolean = token.IsBinaryOperation
            If isvalid Then
                node = New ExpressionNode(ExpressionKind.BinaryOperation, token)
                Return True
            Else
                node = Nothing
                Return False
            End If
        End Function

        Private Function TryConvertTokenToExpressionLeafNode(ByVal token As Token, ByRef node As ExpressionNode) As Boolean
            ThrowIfNull(token)

            If token.IsAnyWord Then
                node = New ExpressionNode(ExpressionKind.Leaf, token)
            ElseIf token.IsNumber Then
                node = New ExpressionNode(ExpressionKind.Leaf, token)
            ElseIf token.IsQuotedString Then
                node = New ExpressionNode(ExpressionKind.Leaf, token)
            ElseIf token.IsCharacter Then
                node = New ExpressionNode(ExpressionKind.Leaf, token)
            ElseIf token.TokenType = TokenType.TrueKeyword OrElse token.TokenType = TokenType.FalseKeyword Then
                node = New ExpressionNode(ExpressionKind.Leaf, token)
            Else
                node = Nothing
                Return False
            End If

            Return True
        End Function

        Private Function FindMatchingParenIndex(ByVal tokens As List(Of Token), ByVal start As Integer) As Integer

            Dim depth As Integer = 1
            For i As Integer = start To tokens.Count - 1
                Select Case tokens(i).TokenType
                    Case TokenType.ParenOpen
                        depth += 1
                    Case TokenType.ParenClose
                        depth -= 1
                        If 0 = depth Then
                            Return i
                        End If
                End Select
            Next

            Return -1
        End Function

    End Class

End Namespace
