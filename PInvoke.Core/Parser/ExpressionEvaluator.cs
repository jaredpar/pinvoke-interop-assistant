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
    public sealed class ExpressionEvaluator
    {
        private readonly ExpressionParser _parser = new ExpressionParser();
        private readonly Dictionary<string, Macro> _macroMap;
        private readonly ScannerOptions _opts;

        public ExpressionEvaluator(Dictionary<string, Macro> macroMap = null)
        {
            _macroMap = macroMap ?? new Dictionary<string, Macro>();
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
            if (!_parser.TryParse(list, out node))
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
                    Contract.ThrowInvalidEnumValue(node.Kind);
                    return false;
            }
        }

        /// <summary>
        /// For a cast just return the value of the left node
        /// </summary>
        private bool TryEvaluateCast(ExpressionNode node)
        {
            // CTODO: why left here?  Shouldn't it be right? 
            node.Tag = node.LeftNode.Tag;
            return true;
        }

        private bool TryEvaluateFunctionCall(ExpressionNode node)
        {
            bool value =
                node.Token.Value == "defined" &&
                node.LeftNode != null &&
                _macroMap.ContainsKey(node.LeftNode.Token.Value);

            node.Tag = ExpressionValue.Create(value);
            return true;
        }

        private bool TryEvaluateNegation(ExpressionNode node)
        {
            ExpressionValue value = (ExpressionValue)node.LeftNode.Tag;
            node.Tag = ExpressionValue.Create(!value.ConvertToBool());
            return true;
        }

        private bool TryEvaluateList(ExpressionNode node)
        {
            return true;
        }

        private bool TryEvaluateNegative(ExpressionNode node)
        {
            var exprValue = ((ExpressionValue)node.LeftNode.Tag);
            if (exprValue.IsFloatingPoint)
            {
                var value = exprValue.ConvertToDouble();
                node.Tag = ExpressionValue.Create(-value);
            }
            else
            {
                var value = exprValue.ConvertToInteger();
                node.Tag = ExpressionValue.Create(-value);
            }

            return true;
        }

        private bool TryEvaluateLeaf(ExpressionNode node)
        {
            Token token = node.Token;
            if (token.IsNumber)
            {
                Number value;
                if (!TokenHelper.TryConvertToNumber(node.Token, out value))
                {
                    return false;
                }
                node.Tag = ExpressionValue.Create(value);
                return true;
            }
            else if (token.TokenType == TokenType.TrueKeyword)
            {
                node.Tag = ExpressionValue.Create(true);
                return true;
            }
            else if (token.TokenType == TokenType.FalseKeyword)
            {
                node.Tag = ExpressionValue.Create(false);
                return true;
            }
            else if (token.TokenType == TokenType.Word)
            {
                return TryEvaluateMacro(node);
            }

            else if (token.IsCharacter)
            {
                char cValue = '0';
                if (!TokenHelper.TryConvertToChar(node.Token, out cValue))
                {
                    return false;
                }
                node.Tag = ExpressionValue.Create(cValue);
                return true;
            }
            else if (token.IsQuotedString)
            {
                string sValue = null;
                if (!TokenHelper.TryConvertToString(token, out sValue))
                {
                    return false;
                }
                node.Tag = ExpressionValue.Create(sValue);
                return true;
            }
            else if (TokenHelper.IsKeyword(node.Token.TokenType))
            {
                node.Tag = ExpressionValue.Create(1);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryEvaluateMacro(ExpressionNode node)
        {
            Contract.Requires(node.Kind == ExpressionKind.Leaf);
            Contract.Requires(node.Token.TokenType == TokenType.Word);

            ExpressionValue value = default(ExpressionValue);
            Macro m = null;
            if (_macroMap.TryGetValue(node.Token.Value, out m))
            {
                Number numValue;
                if (TokenHelper.TryConvertToNumber(m.Value, out numValue))
                {
                    value = ExpressionValue.Create(numValue);
                }
                else
                {
                    value = ExpressionValue.Create(1);
                }
            }
            else
            {
                value = ExpressionValue.Create(0);
            }

            node.Tag = value;
            return true;
        }

        private bool TryEvaluateBinaryOperation(ExpressionNode node)
        {
            BinaryOperator op;
            if (!TryConvertToBinaryOperator(node.Token.TokenType, out op))
            {
                return false;
            }

            ExpressionValue left = (ExpressionValue)node.LeftNode.Tag;
            ExpressionValue right = (ExpressionValue)node.RightNode.Tag;
            ExpressionValue result;
            var succeeded = TryEvaluateBinaryOperation(op, left, right, out result);
            node.Tag = result;
            return succeeded;
        }

        public static bool TryConvertToBinaryOperator(TokenType type, out BinaryOperator op)
        {
            switch (type)
            {
                case TokenType.OpDivide:
                    op = BinaryOperator.Divide;
                    break;
                case TokenType.OpGreaterThan:
                    op = BinaryOperator.GreaterThan;
                    break;
                case TokenType.OpGreaterThanOrEqual:
                    op = BinaryOperator.GreaterThanOrEqualTo;
                    break;
                case TokenType.OpLessThan:
                    op = BinaryOperator.LessThan;
                    break;
                case TokenType.OpLessThanOrEqual:
                    op = BinaryOperator.LessThanOrEqualTo;
                    break;
                case TokenType.OpMinus:
                    op = BinaryOperator.Subtract;
                    break;
                case TokenType.OpModulus:
                    op = BinaryOperator.Modulus;
                    break;
                case TokenType.OpShiftLeft:
                    op = BinaryOperator.ShiftLeft;
                    break;
                case TokenType.OpShiftRight:
                    op = BinaryOperator.ShiftRight;
                    break;
                case TokenType.OpPlus:
                    op = BinaryOperator.Add;
                    break;
                case TokenType.OpBoolAnd:
                    op = BinaryOperator.BooleanAnd;
                    break;
                case TokenType.OpBoolOr:
                    op = BinaryOperator.BooleanOr;
                    break;
                case TokenType.OpEquals:
                    op = BinaryOperator.BooleanEquals;
                    break;
                case TokenType.OpNotEquals:
                    op = BinaryOperator.BooleanNotEquals;
                    break;
                case TokenType.Ampersand:
                    op = BinaryOperator.BitwiseAnd;
                    break;
                case TokenType.Pipe:
                    op = BinaryOperator.BitwiseOr;
                    break;
                case TokenType.OpAssign:
                    op = BinaryOperator.Assign;
                    break;
                default:
                    op = BinaryOperator.Add;
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Reference used for this implementation
        /// 
        /// https://gcc.gnu.org/onlinedocs/cpp/If.html#If
        /// 
        /// In summary calculations done using the widest type known to compiler which is long for 
        /// our implementation.
        /// </summary>
        public static bool TryEvaluateBinaryOperation(BinaryOperator op, ExpressionValue left, ExpressionValue right, out ExpressionValue result)
        {
            long leftValue;
            bool leftBool;
            long rightValue;
            bool rightBool;

            try
            {
                leftValue = left.ConvertToLong();
                leftBool = left.ConvertToBool();
                rightValue = right.ConvertToLong();
                rightBool = right.ConvertToBool();
            }
            catch
            {
                result = null;
                return false;
            }

            switch (op)
            {
                case BinaryOperator.Divide:
                    result = ExpressionValue.Create(leftValue / rightValue);
                    break;
                case BinaryOperator.GreaterThan:
                    result = ExpressionValue.Create(leftValue > rightValue);
                    break;
                case BinaryOperator.GreaterThanOrEqualTo:
                    result = ExpressionValue.Create(leftValue >= rightValue);
                    break;
                case BinaryOperator.LessThan:
                    result = ExpressionValue.Create(leftValue < rightValue);
                    break;
                case BinaryOperator.LessThanOrEqualTo:
                    result = ExpressionValue.Create(leftValue <= rightValue);
                    break;
                case BinaryOperator.Subtract:
                    result = ExpressionValue.Create(leftValue - rightValue);
                    break;
                case BinaryOperator.Modulus:
                    result = ExpressionValue.Create(leftValue % rightValue);
                    break;
                case BinaryOperator.ShiftLeft:
                    result = ExpressionValue.Create(leftValue << (int)rightValue);
                    break;
                case BinaryOperator.ShiftRight:
                    result = ExpressionValue.Create(leftValue >> (int)rightValue);
                    break;
                case BinaryOperator.Add:
                    result = ExpressionValue.Create(leftValue + rightValue);
                    break;
                case BinaryOperator.Multiply:
                    result = ExpressionValue.Create(leftValue * rightValue);
                    break;
                case BinaryOperator.BooleanAnd:
                    result = ExpressionValue.Create(leftBool && rightBool);
                    break;
                case BinaryOperator.BooleanOr:
                    result = ExpressionValue.Create(leftBool || rightBool);
                    break;
                case BinaryOperator.BooleanEquals:
                    result = ExpressionValue.Create(leftValue == rightValue);
                    break;
                case BinaryOperator.BooleanNotEquals:
                    result = ExpressionValue.Create(leftValue != rightValue);
                    break;
                case BinaryOperator.BitwiseAnd:
                    result = ExpressionValue.Create(leftValue & rightValue);
                    break;
                case BinaryOperator.BitwiseOr:
                    result = ExpressionValue.Create(leftValue | rightValue);
                    break;
                case BinaryOperator.Assign:
                    result = right;
                    break;
                default:
                    Contract.ThrowInvalidEnumValue(op);
                    result = null;
                    return false;
            }

            return true;
        }
    }
}
