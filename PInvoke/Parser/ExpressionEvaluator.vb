' Copyright (c) Microsoft Corporation.  All rights reserved.

Imports System.Runtime.InteropServices

Namespace Parser

    ''' <summary>
    ''' Used to evaluate basic expressions encounter by the parser.  
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ExpressionEvaluator
        Private _parser As New ExpressionParser()
        Private _opts As ScannerOptions

        Public Sub New()
            _opts = New ScannerOptions()
            _opts.HideComments = True
            _opts.HideNewLines = True
            _opts.HideWhitespace = True
            _opts.ThrowOnEndOfStream = False
        End Sub

        Public Function TryEvaluate(ByVal expr As String, <Out> ByRef result As ExpressionValue) As Boolean
            Dim list As List(Of Token) = Scanner.TokenizeText(expr, _opts)
            Return TryEvaluate(list, result)
        End Function

        Public Function TryEvaluate(ByVal list As List(Of Token), <Out> ByRef result As ExpressionValue) As Boolean
            Dim node As ExpressionNode = Nothing
            If Not _parser.TryParse(list, node) Then
                result = Nothing
                Return False
            End If

            Return TryEvaluate(node, result)
        End Function

        Public Function TryEvaluate(ByVal node As ExpressionNode, <Out> ByRef result As ExpressionValue) As Boolean
            If Not TryEvaluateCore(node) Then
                Return False
            End If

            result = DirectCast(node.Tag, ExpressionValue)
            Return True
        End Function

        Private Function TryEvaluateCore(ByVal node As ExpressionNode) As Boolean
            If node Is Nothing Then
                Return True
            End If

            ' Make sure that the left and right are evaluated appropriately
            If Not TryEvaluateCore(node.LeftNode) OrElse Not TryEvaluateCore(node.RightNode) Then
                Return False
            End If

            Select Case node.Kind
                Case ExpressionKind.BinaryOperation
                    Return TryEvaluateBinaryOperation(node)
                Case ExpressionKind.Leaf
                    Return TryEvaluateLeaf(node)
                Case ExpressionKind.NegativeOperation
                    Return TryEvaluateNegative(node)
                Case ExpressionKind.Cast
                    Return TryEvaluateCast(node)
                Case ExpressionKind.FunctionCall
                    Return TryEvaluateFunctionCall(node)
                Case ExpressionKind.NegationOperation
                    Return TryEvaluateNegation(node)
                Case ExpressionKind.List
                    Return TryEvaluateList(node)
            End Select
        End Function

        Protected Overridable Function TryEvaluateCast(ByVal node As ExpressionNode) As Boolean
            Return False
        End Function

        Protected Overridable Function TryEvaluateFunctionCall(ByVal node As ExpressionNode) As Boolean
            Return False
        End Function

        Protected Overridable Function TryEvaluateNegation(ByVal node As ExpressionNode) As Boolean
            Return False
        End Function

        Protected Overridable Function TryEvaluateList(ByVal node As ExpressionNode) As Boolean
            Return True
        End Function

        Protected Overridable Function TryEvaluateNegative(ByVal node As ExpressionNode) As Boolean
            node.Tag = -(DirectCast(node.LeftNode.Tag, ExpressionValue))
            Return True
        End Function

        Protected Overridable Function TryEvaluateLeaf(ByVal node As ExpressionNode) As Boolean
            Dim token As Token = node.Token
            If token.IsNumber Then
                Dim value As Object = Nothing
                If Not TokenHelper.TryConvertToNumber(node.Token, value) Then
                    Return False
                End If
                node.Tag = New ExpressionValue(value)
                Return True
            ElseIf token.TokenType = TokenType.TrueKeyword Then
                node.Tag = New ExpressionValue(True)
                Return True
            ElseIf token.TokenType = TokenType.FalseKeyword Then
                node.Tag = New ExpressionValue(False)
                Return True
            ElseIf token.IsCharacter Then
                Dim cValue As Char = "0"c
                If Not TokenHelper.TryConvertToChar(node.Token, cValue) Then
                    Return False
                End If
                node.Tag = New ExpressionValue(cValue)
                Return True
            ElseIf token.IsQuotedString Then
                Dim sValue As String = Nothing
                If Not TokenHelper.TryConvertToString(token, sValue) Then
                    Return False
                End If
                node.Tag = New ExpressionValue(sValue)
                Return True
            Else
                Return False
            End If
        End Function

        Protected Overridable Function TryEvaluateBinaryOperation(ByVal node As ExpressionNode) As Boolean
            Dim left As ExpressionValue = DirectCast(node.LeftNode.Tag, ExpressionValue)
            Dim right As ExpressionValue = DirectCast(node.RightNode.Tag, ExpressionValue)
            Dim result As ExpressionValue = Nothing
            Select Case node.Token.TokenType
                Case TokenType.OpDivide
                    result = left / right
                Case TokenType.OpGreaterThan
                    result = New ExpressionValue(left > right)
                Case TokenType.OpGreaterThanOrEqual
                    result = New ExpressionValue(left >= right)
                Case TokenType.OpLessThan
                    result = New ExpressionValue(left < right)
                Case TokenType.OpLessThanOrEqual
                    result = New ExpressionValue(left <= right)
                Case TokenType.OpMinus
                    result = left - right
                Case TokenType.OpModulus
                    result = left - ((left \ right) * right)
                Case TokenType.OpShiftLeft
                    result = left << CInt(right.Value)
                Case TokenType.OpShiftRight
                    result = left >> CInt(right.Value)
                Case TokenType.OpPlus
                    result = left + right
                Case TokenType.OpBoolAnd
                    result = left AndAlso right
                Case TokenType.OpBoolOr
                    result = left OrElse right
                Case TokenType.OpEquals
                    result = New ExpressionValue(left.Value.Equals(right.Value))
                Case TokenType.OpNotEquals
                    result = New ExpressionValue(Not (left.Value.Equals(right.Value)))
                Case TokenType.OpAssign
                    result = right
                Case TokenType.Ampersand
                    result = left And right
                Case TokenType.Pipe
                    result = left Or right
                Case Else
                    Debug.Fail("Unrecognized binary operation")
                    Return False
            End Select

            node.Tag = result
            Return node.Tag IsNot Nothing
        End Function

    End Class

End Namespace
