using PInvoke.Parser.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Parser
{
    /// <summary>
    /// Expression Node
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayString}")]
    public sealed class ExpressionNode
    {
        public ExpressionKind Kind { get; set; }

        public ExpressionNode LeftNode;

        public ExpressionNode RightNode;

        public Token Token { get; set; }

        public bool Parenthesized { get; set; }

        public string DisplayString
        {
            get
            {
                var str = string.Empty;
                if (LeftNode != null)
                {
                    str += "(Left: " + LeftNode.DisplayString + ")";
                }

                if (RightNode != null)
                {
                    str += "(Right: " + RightNode.DisplayString + ")";
                }

                if (!string.IsNullOrEmpty(str))
                {
                    str = " " + str;
                }

                if (Token == null)
                {
                    return "Nothing" + str;
                }
                else
                {
                    return Token.Value + str;
                }
            }
        }

        public ExpressionNode(ExpressionKind kind, Token value)
        {
            Kind = kind;
            Token = value;
        }

        public static ExpressionNode CreateLeaf(bool bValue)
        {
            var token = default(Token);
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
}
