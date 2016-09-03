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
        private ExpressionKind _kind;
        private Token _token;
        private bool _parenthesized;

        public ExpressionKind Kind
        {
            get { return _kind; }
            set { _kind = value; }
        }

        public ExpressionNode LeftNode;

        public ExpressionNode RightNode;

        public Token Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public bool Parenthesized
        {
            get { return _parenthesized; }
            set { _parenthesized = value; }
        }

        public string DisplayString
        {
            get
            {
                string str = string.Empty;
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
            Token = value;
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
}
