// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using static PInvoke.Contract;

namespace PInvoke.Parser
{

    /// <summary>
    /// Kind of expression
    /// </summary>
    /// <remarks></remarks>
    public enum ExpressionKind
    {

        // Binary operation such as +,-,/ 
        // Token: Operation
        BinaryOperation,

        // '-' operation.  Left is the value
        NegativeOperation,

        // ! operation, Left is the value
        NegationOperation,

        // Token is the name of the function.  
        // Left: Value
        // Right: , if there are more arguments
        FunctionCall,

        List,

        // Token: Target Type
        // Left: Source that is being cast
        Cast,

        // Token: Value of the expression
        Leaf
    }

    /// <summary>
    /// Expression Node
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayString}")]
    public class ExpressionNode
    {
        private ExpressionKind _kind;
        private ExpressionNode _left;
        private ExpressionNode _right;
        private Token _token;
        private bool _parenthesized;

        private object _tag;
        public ExpressionKind Kind
        {
            get { return _kind; }
            set { _kind = value; }
        }

        public ExpressionNode LeftNode
        {
            get { return _left; }
            set { _left = value; }
        }

        public ExpressionNode RightNode
        {
            get { return _right; }
            set { _right = value; }
        }

        public Token Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public bool Parenthesized
        {
            get { return _parenthesized; }
            set { _parenthesized = true; }
        }

        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public string DisplayString
        {
            get
            {
                string str = string.Empty;
                if (_left != null)
                {
                    str += "(Left: " + _left.DisplayString + ")";
                }

                if (_right != null)
                {
                    str += "(Right: " + _right.DisplayString + ")";
                }

                if (!string.IsNullOrEmpty(str))
                {
                    str = " " + str;
                }

                if (_token == null)
                {
                    return "Nothing" + str;
                }
                else
                {
                    return _token.Value + str;
                }
            }
        }

        public ExpressionNode(ExpressionKind kind, Token value)
        {
            _kind = kind;
            _token = value;
        }

        public static ExpressionNode CreateLeaf(bool bValue)
        {
            Token token = default(Token);
            if (bValue)
            {
                token = new Token(TokenType.TrueKeyword, "true");
            }
            else
            {
                token = new Token(TokenType.TrueKeyword, "false");
            }

            return new ExpressionNode(ExpressionKind.Leaf, token);
        }

        public static ExpressionNode CreateLeaf(int number)
        {
            return new ExpressionNode(ExpressionKind.Leaf, new Token(TokenType.Number, number.ToString()));
        }

    }

    /// <summary>
    /// Converts an expression into an expression tree
    /// </summary>
    /// <remarks></remarks>
    public class ExpressionParser
    {

        public ExpressionNode Parse(string expression)
        {
            ExpressionNode node = null;
            if (!this.TryParse(expression, ref node))
            {
                throw new InvalidOperationException("Unable to parse the expression");
            }

            return node;
        }

        public bool IsParsable(string expression)
        {
            ExpressionNode node = null;
            return TryParse(expression, ref node);
        }

        public bool TryParse(List<Token> tokens, ref ExpressionNode node)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            return TryParseComplete(tokens, ref node);
        }

        public bool TryParse(string expression, ref ExpressionNode node)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            using (IO.StringReader reader = new IO.StringReader(expression))
            {
                Scanner scanner = new Scanner(reader);
                scanner.Options.HideNewLines = true;
                scanner.Options.HideComments = true;
                scanner.Options.HideWhitespace = true;
                scanner.Options.ThrowOnEndOfStream = false;
                return TryParseComplete(scanner.Tokenize(), ref node);
            }
        }

        private bool TryParseComplete(List<Token> tokens, ref ExpressionNode node)
        {
            ExpressionNode cur = null;
            List<Token> remaining = null;
            if (!TryParseCore(tokens, ref cur, ref remaining))
            {
                return false;
            }

            if (remaining.Count == 0)
            {
                node = cur;
                return true;

            }
            else if (!remaining(0).IsBinaryOperation && cur.Parenthesized && cur.Kind == ExpressionKind.Leaf && cur.Token.IsAnyWord)
            {
                // This is a cast
                cur.Kind = ExpressionKind.Cast;
                node = cur;
                return TryParseComplete(remaining, ref node.LeftNode);
            }
            else if (remaining.Count == 1 || !remaining(0).IsBinaryOperation)
            {
                return false;
            }
            else
            {
                ExpressionNode right = null;
                if (!TryParseComplete(remaining.GetRange(1, remaining.Count - 1), ref right))
                {
                    return false;
                }

                node = new ExpressionNode(ExpressionKind.BinaryOperation, remaining(0));
                node.LeftNode = cur;
                node.RightNode = right;
                return true;
            }
        }

        private bool TryParseCore(List<Token> tokens, ref ExpressionNode node, ref List<Token> remaining)
        {
            ThrowIfNull(tokens);

            if (tokens.Count == 0)
            {
                return false;
            }

            // Single tokens are the easiest
            if (tokens.Count == 1)
            {
                remaining = new List<Token>();
                return TryConvertTokenToExpressionLeafNode(tokens(0), ref node);
            }

            ExpressionNode leftNode = null;
            ExpressionNode unaryNode = null;
            int nextIndex = -1;


            if (tokens.Count > 2 && tokens(0).IsAnyWord && tokens(1).TokenType == TokenType.ParenOpen)
            {
                // Function call
                return TryParseFunctionCall(tokens, ref node, ref remaining);
            }
            else if (tokens(0).TokenType == TokenType.Bang)
            {
                node = new ExpressionNode(ExpressionKind.NegationOperation, tokens(0));
                return TryParseCore(tokens.GetRange(1, tokens.Count - 1), ref node.LeftNode, ref remaining);
            }
            else if (tokens(0).TokenType == TokenType.OpMinus)
            {
                node = new ExpressionNode(ExpressionKind.NegativeOperation, tokens(0));
                return TryParseCore(tokens.GetRange(1, tokens.Count - 1), ref node.LeftNode, ref remaining);
            }
            else if (tokens(0).TokenType == TokenType.ParenOpen)
            {
                return TryParseParenExpression(tokens, ref node, ref remaining);
            }
            else if (tokens.Count > 2)
            {
                // Has to be an operation so convert the left node to a leaf expression
                remaining = tokens.GetRange(1, tokens.Count - 1);
                return TryConvertTokenToExpressionLeafNode(tokens(0), ref node);
            }
            else
            {
                return false;
            }
        }

        private bool TryParseFunctionCall(List<Token> tokens, ref ExpressionNode node, ref List<Token> remaining)
        {
            ThrowIfTrue(tokens.Count < 3);

            node = new ExpressionNode(ExpressionKind.FunctionCall, tokens(0));

            // Find the last index 
            int endIndex = FindMatchingParenIndex(tokens, 2);
            if (endIndex == -1)
            {
                return false;
            }

            // If there is more than just word() then there are arguments
            if (tokens.Count > 3)
            {
                List<Token> subList = tokens.GetRange(2, endIndex - 2);
                if (!TryParseFunctionCallArguments(subList, node))
                {
                    return false;
                }
            }

            remaining = tokens.GetRange(endIndex + 1, tokens.Count - (endIndex + 1));
            return true;
        }

        private bool TryParseFunctionCallArguments(List<Token> tokens, ExpressionNode callNode)
        {
            ThrowIfNull(callNode);
            ThrowIfFalse(ExpressionKind.FunctionCall == callNode.Kind);

            // Start the list
            ExpressionNode cur = callNode;

            while (tokens.Count > 0)
            {
                int index = FindNextCallArgumentSeparator(tokens);
                if (index < 0)
                {
                    // No more separators so just parse out the rest of the tokens as an argument
                    if (!TryParseComplete(tokens, ref cur.LeftNode))
                    {
                        return false;
                    }
                    tokens.Clear();
                }
                else
                {
                    if (!TryParseComplete(tokens.GetRange(0, index), ref cur.LeftNode))
                    {
                        return false;
                    }

                    tokens = tokens.GetRange(index + 1, tokens.Count - (index + 1));
                }

                if (tokens.Count > 0)
                {
                    cur.RightNode = new ExpressionNode(ExpressionKind.List, new Token(TokenType.Comma, ","));
                    cur = cur.RightNode;
                }
            }

            return true;
        }

        private bool TryParseParenExpression(List<Token> tokens, ref ExpressionNode node, ref List<Token> remaining)
        {
            int endIndex = FindMatchingParenIndex(tokens, 1);
            if (endIndex == -1)
            {
                return false;
            }

            remaining = tokens.GetRange(endIndex + 1, tokens.Count - (endIndex + 1));
            bool success = TryParseComplete(tokens.GetRange(1, endIndex - 1), ref node);
            if (success)
            {
                node.Parenthesized = true;
            }

            return success;
        }

        private int FindNextCallArgumentSeparator(List<Token> tokens)
        {
            ThrowIfNull(tokens);

            for (int i = 0; i <= tokens.Count - 1; i++)
            {
                if (tokens(i).TokenType == TokenType.Comma)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool TryConvertTokenToExpressionBinaryOperation(Token token, ref ExpressionNode node)
        {
            ThrowIfNull(token);

            bool isvalid = token.IsBinaryOperation;
            if (isvalid)
            {
                node = new ExpressionNode(ExpressionKind.BinaryOperation, token);
                return true;
            }
            else
            {
                node = null;
                return false;
            }
        }

        private bool TryConvertTokenToExpressionLeafNode(Token token, ref ExpressionNode node)
        {
            ThrowIfNull(token);

            if (token.IsAnyWord)
            {
                node = new ExpressionNode(ExpressionKind.Leaf, token);
            }
            else if (token.IsNumber)
            {
                node = new ExpressionNode(ExpressionKind.Leaf, token);
            }
            else if (token.IsQuotedString)
            {
                node = new ExpressionNode(ExpressionKind.Leaf, token);
            }
            else if (token.IsCharacter)
            {
                node = new ExpressionNode(ExpressionKind.Leaf, token);
            }
            else if (token.TokenType == TokenType.TrueKeyword || token.TokenType == TokenType.FalseKeyword)
            {
                node = new ExpressionNode(ExpressionKind.Leaf, token);
            }
            else
            {
                node = null;
                return false;
            }

            return true;
        }

        private int FindMatchingParenIndex(List<Token> tokens, int start)
        {

            int depth = 1;
            for (int i = start; i <= tokens.Count - 1; i++)
            {
                switch (tokens(i).TokenType)
                {
                    case TokenType.ParenOpen:
                        depth += 1;
                        break;
                    case TokenType.ParenClose:
                        depth -= 1;
                        if (0 == depth)
                        {
                            return i;
                        }
                        break;
                }
            }

            return -1;
        }

    }

}
