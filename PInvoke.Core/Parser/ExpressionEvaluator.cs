// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.Parser.Enums;
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
        private readonly ExpressionParser parser = new ExpressionParser();
        private readonly Dictionary<string, Macro> macroMap;
        private readonly ScannerOptions opts;

        public ExpressionEvaluator(Dictionary<string, Macro> macroMap = null)
        {
            this.macroMap = macroMap ?? new Dictionary<string, Macro>();
            opts = new ScannerOptions
            {
                HideComments = true,
                HideNewLines = true,
                HideWhitespace = true,
                ThrowOnEndOfStream = false
            };
        }

        public bool TryEvaluate(string expr, out ExpressionValue result)
        {
            var list = Scanner.TokenizeText(expr, opts);
            return TryEvaluate(list, out result);
        }

        public bool TryEvaluate(List<Token> list, out ExpressionValue result)
        {
            if (!parser.TryParse(list, out ExpressionNode node))
            {
                result = null;
                return false;
            }

            return TryEvaluate(node, out result);
        }

        public bool TryEvaluate(ExpressionNode node, out ExpressionValue result)
        {
            try
            {
                result = EvaluateCore(node);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public ExpressionValue Evalaute(ExpressionNode node)
        {
            return EvaluateCore(node);
        }

        private ExpressionValue EvaluateCore(ExpressionNode node)
        {
            Contract.ThrowIfNull(node);

            switch (node.Kind)
            {
                case ExpressionKind.BinaryOperation:
                    return EvaluateBinaryOperation(node);
                case ExpressionKind.Leaf:
                    return EvaluateLeaf(node);
                case ExpressionKind.NegativeOperation:
                    return EvaluateNegative(node);
                case ExpressionKind.Cast:
                    return EvaluateCast(node);
                case ExpressionKind.FunctionCall:
                    return EvaluateFunctionCall(node);
                case ExpressionKind.NegationOperation:
                    return EvaluateNegation(node);
                case ExpressionKind.List:
                    return TryEvaluateList(node);
                default:
                    throw Contract.CreateInvalidEnumValueException(node.Kind);
            }
        }

        /// <summary>
        /// For a cast just return the value of the left node
        /// </summary>
        private ExpressionValue EvaluateCast(ExpressionNode node)
        {
            // TODO: why left here?  Shouldn't it be right? 
            return EvaluateCore(node.LeftNode);
        }

        private ExpressionValue EvaluateFunctionCall(ExpressionNode node)
        {
            bool value =
                node.Token.Value == "defined" &&
                node.LeftNode != null &&
                macroMap.ContainsKey(node.LeftNode.Token.Value);

            return ExpressionValue.Create(value);
        }

        private ExpressionValue EvaluateNegation(ExpressionNode node)
        {
            var value = EvaluateCore(node.LeftNode);
            return ExpressionValue.Create(!value.ConvertToBool());
        }

        private ExpressionValue TryEvaluateList(ExpressionNode node)
        {
            throw new Exception($"Cannot evaluate list");
        }

        private ExpressionValue EvaluateNegative(ExpressionNode node)
        {
            var exprValue = EvaluateCore(node.LeftNode);
            if (exprValue.IsFloatingPoint)
            {
                var value = exprValue.ConvertToDouble();
                return ExpressionValue.Create(-value);
            }
            else
            {
                var value = exprValue.ConvertToInteger();
                return ExpressionValue.Create(-value);
            }
        }

        private ExpressionValue EvaluateLeaf(ExpressionNode node)
        {
            var token = node.Token;
            if (token.IsNumber)
            {
                if (!TokenHelper.TryConvertToNumber(node.Token, out Number value))
                {
                    throw new Exception($"Can't convert token to number {node.Token}");
                }

                return ExpressionValue.Create(value);
            }
            else if (token.TokenType == TokenType.TrueKeyword)
            {
                return ExpressionValue.Create(true);
            }
            else if (token.TokenType == TokenType.FalseKeyword)
            {
                return ExpressionValue.Create(false);
            }
            else if (token.TokenType == TokenType.Word)
            {
                return EvaluateMacro(node);
            }
            else if (token.IsCharacter)
            {
                var cValue = '0';
                if (!TokenHelper.TryConvertToChar(node.Token, out cValue))
                {
                    throw new Exception($"Can't convert token to char {node.Token}");
                }
                return ExpressionValue.Create(cValue);
            }
            else if (token.IsQuotedString)
            {
                if (!TokenHelper.TryConvertToString(token, out string sValue))
                {
                    throw new Exception($"Can't convert token to string {node.Token}");
                }
                return ExpressionValue.Create(sValue);
            }
            else if (TokenHelper.IsKeyword(node.Token.TokenType))
            {
                return ExpressionValue.Create(1);
            }
            else
            {
                throw new Exception($"Unexpected leaf token {node.Token}");
            }
        }

        private ExpressionValue EvaluateMacro(ExpressionNode node)
        {
            Contract.Requires(node.Kind == ExpressionKind.Leaf);
            Contract.Requires(node.Token.TokenType == TokenType.Word);

            var value = default(ExpressionValue);
            if (macroMap.TryGetValue(node.Token.Value, out Macro m))
            {
                if (TokenHelper.TryConvertToNumber(m.Value, out Number numValue))
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

            return value;
        }

        private ExpressionValue EvaluateBinaryOperation(ExpressionNode node)
        {
            if (!TryConvertToBinaryOperator(node.Token.TokenType, out BinaryOperator op))
            {
                throw new Exception($"Invalid binary node {node.Token.TokenType}");
            }

            var left = EvaluateCore(node.LeftNode);
            var right = EvaluateCore(node.RightNode);
            return EvaluateBinaryOperation(op, left, right);
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
        public static ExpressionValue EvaluateBinaryOperation(BinaryOperator op, ExpressionValue left, ExpressionValue right)
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
            catch (Exception ex)
            {
                throw new Exception($"Unable to convert binary operands: {ex.Message}", ex);
            }

            ExpressionValue result;
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
                    throw Contract.CreateInvalidEnumValueException(op);
            }

            return result;
        }
    }
}
