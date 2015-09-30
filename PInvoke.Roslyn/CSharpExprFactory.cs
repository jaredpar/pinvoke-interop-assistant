using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PInvoke.Parser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PInvoke.Roslyn
{
    internal enum ExpressionType
    {
        Number,
        String,
        Char,
        Boolean,
    }

    internal sealed class CSharpExprFactory
    {
        private readonly ExpressionEvaluator _evaluator = new ExpressionEvaluator();

        internal ExpressionSyntax GenerateValue(ExpressionNode nativeNode)
        {
            ExpressionType type;
            return GenerateValue(nativeNode, out type);
        }

        internal ExpressionSyntax GenerateValue(ExpressionNode nativeNode, out ExpressionType type)
        {
            ExpressionSyntax exprSyntax;
            if (!TryGenerateValue(nativeNode, out exprSyntax, out type))
            {
                throw new Exception("Cannot generate value");
            }

            return exprSyntax;
        }

        internal bool TryGenerateValue(ExpressionNode nativeNode, out ExpressionSyntax exprSyntax, out ExpressionType type)
        {
            throw new Exception();
            /*
            ExpressionValue nativeValue;
            if (!_evaluator.TryEvaluate(nativeNode, out nativeValue))
            {
                exprSyntax = null;
                type = ExpressionType.Number;
                return false;
            }

            switch (nativeNode.Kind)
            {
                case ExpressionKind.Leaf:
                    return TryGenerateValueLeaf((NativeValue)nativeNode.Tag, out exprSyntax, out type);
                default:
                    throw Contract.InvalidEnum(nativeNode.Kind);
            }
            */
        }

        internal bool TryGenerateValueLeaf(NativeValue nativeValue, out ExpressionSyntax exprSyntax, out ExpressionType type)
        {
            switch (nativeValue.ValueKind)
            {
                case NativeValueKind.Number:
                    type = ExpressionType.Number;
                    exprSyntax = SyntaxFactory.LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        SyntaxFactory.Literal((int)nativeValue.Value));
                    return true;
                    /*
                case NativeValueKind.Boolean:
                    type = ExpressionType.Boolean;
                    exprNode = _generator.Literal((bool)nativeValue.Value);
                    break;
                case NativeValueKind.Character:
                    type = ExpressionType.Char;
                    exprNode = _generator.Literal((char)nativeValue.Value);
                    break;
                case NativeValueKind.String:
                    type = ExpressionType.String;
                    exprNode = _generator.Literal((string)nativeValue.Value);
                    break;
                    */
                default:
                    // TODO: implement
                    throw new NotSupportedException();
            }
        }
    }
}
