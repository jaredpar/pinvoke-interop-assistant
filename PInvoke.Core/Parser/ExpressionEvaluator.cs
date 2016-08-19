// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PInvoke.Parser
{

    /// <summary>
    /// Used to evaluate basic expressions encounter by the parser.  
    /// </summary>
    /// <remarks></remarks>
    public class ExpressionEvaluator
    {
        private ExpressionParser _parser = new ExpressionParser();

        private ScannerOptions _opts;
        public ExpressionEvaluator()
        {
            _opts = new ScannerOptions();
            _opts.HideComments = true;
            _opts.HideNewLines = true;
            _opts.HideWhitespace = true;
            _opts.ThrowOnEndOfStream = false;
        }

        public bool TryEvaluate(string expr, out ExpressionValue result)
        {
            List<Token> list = Scanner.TokenizeText(expr, _opts);
            return TryEvaluate(list, out result);
        }

        public bool TryEvaluate(List<Token> list, out ExpressionValue result)
        {
            ExpressionNode node = null;
            if (!_parser.TryParse(list, node))
            {
                result = null;
                return false;
            }

            return TryEvaluate(node, out result);
        }

        public bool TryEvaluate(ExpressionNode node, out ExpressionValue result)
        {
            if (!TryEvaluateCore(node))
            {
                result = null;
                return false;
            }

            result = (ExpressionValue)node.Tag;
            return true;
        }

        private bool TryEvaluateCore(ExpressionNode node)
        {
            if (node == null)
            {
                return true;
            }

            // Make sure that the left and right are evaluated appropriately
            if (!TryEvaluateCore(node.LeftNode) || !TryEvaluateCore(node.RightNode))
            {
                return false;
            }

            switch (node.Kind)
            {
                case ExpressionKind.BinaryOperation:
                    return TryEvaluateBinaryOperation(node);
                case ExpressionKind.Leaf:
                    return TryEvaluateLeaf(node);
                case ExpressionKind.NegativeOperation:
                    return TryEvaluateNegative(node);
                case ExpressionKind.Cast:
                    return TryEvaluateCast(node);
                case ExpressionKind.FunctionCall:
                    return TryEvaluateFunctionCall(node);
                case ExpressionKind.NegationOperation:
                    return TryEvaluateNegation(node);
                case ExpressionKind.List:
                    return TryEvaluateList(node);
                default:
                    Contract.InvalidEnumValue(node.Kind);
                    return false;
            }
        }

        protected virtual bool TryEvaluateCast(ExpressionNode node)
        {
            return false;
        }

        protected virtual bool TryEvaluateFunctionCall(ExpressionNode node)
        {
            return false;
        }

        protected virtual bool TryEvaluateNegation(ExpressionNode node)
        {
            return false;
        }

        protected virtual bool TryEvaluateList(ExpressionNode node)
        {
            return true;
        }

        protected virtual bool TryEvaluateNegative(ExpressionNode node)
        {
            node.Tag = -((ExpressionValue)node.LeftNode.Tag);
            return true;
        }

        protected virtual bool TryEvaluateLeaf(ExpressionNode node)
        {
            Token token = node.Token;
            if (token.IsNumber)
            {
                object value = null;
                if (!TokenHelper.TryConvertToNumber(node.Token, out value))
                {
                    return false;
                }
                node.Tag = new ExpressionValue(value);
                return true;
            }
            else if (token.TokenType == TokenType.TrueKeyword)
            {
                node.Tag = new ExpressionValue(true);
                return true;
            }
            else if (token.TokenType == TokenType.FalseKeyword)
            {
                node.Tag = new ExpressionValue(false);
                return true;
            }
            else if (token.IsCharacter)
            {
                char cValue = '0';
                if (!TokenHelper.TryConvertToChar(node.Token, out cValue))
                {
                    return false;
                }
                node.Tag = new ExpressionValue(cValue);
                return true;
            }
            else if (token.IsQuotedString)
            {
                string sValue = null;
                if (!TokenHelper.TryConvertToString(token, out sValue))
                {
                    return false;
                }
                node.Tag = new ExpressionValue(sValue);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool TryEvaluateBinaryOperation(ExpressionNode node)
        {
            ExpressionValue left = (ExpressionValue)node.LeftNode.Tag;
            ExpressionValue right = (ExpressionValue)node.RightNode.Tag;
            ExpressionValue result = null;
            switch (node.Token.TokenType)
            {
                case TokenType.OpDivide:
                    result = left / right;
                    break;
                case TokenType.OpGreaterThan:
                    result = new ExpressionValue(left > right);
                    break;
                case TokenType.OpGreaterThanOrEqual:
                    result = new ExpressionValue(left >= right);
                    break;
                case TokenType.OpLessThan:
                    result = new ExpressionValue(left < right);
                    break;
                case TokenType.OpLessThanOrEqual:
                    result = new ExpressionValue(left <= right);
                    break;
                case TokenType.OpMinus:
                    result = left - right;
                    break;
                case TokenType.OpModulus:
                    result = left - ((left / right) * right);
                    break;
                case TokenType.OpShiftLeft:
                    result = left << Convert.ToInt32(right.Value);
                    break;
                case TokenType.OpShiftRight:
                    result = left >> Convert.ToInt32(right.Value);
                    break;
                case TokenType.OpPlus:
                    result = left + right;
                    break;
                case TokenType.OpBoolAnd:
                    result = left && right;
                    break;
                case TokenType.OpBoolOr:
                    result = left || right;
                    break;
                case TokenType.OpEquals:
                    result = new ExpressionValue(left.Value.Equals(right.Value));
                    break;
                case TokenType.OpNotEquals:
                    result = new ExpressionValue(!(left.Value.Equals(right.Value)));
                    break;
                case TokenType.OpAssign:
                    result = right;
                    break;
                case TokenType.Ampersand:
                    result = left & right;
                    break;
                case TokenType.Pipe:
                    result = left | right;
                    break;
                default:
                    Debug.Fail("Unrecognized binary operation");
                    return false;
            }

            node.Tag = result;
            return node.Tag != null;
        }
    }
}
