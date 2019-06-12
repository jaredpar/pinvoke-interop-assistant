// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Collections.Generic;
using PInvoke.NativeTypes.Enums;
using PInvoke.Parser;
using PInvoke.Parser.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents the value of an experession
    /// </summary>
    public class NativeValueExpression : NativeExtraSymbol
    {
        private string _expression;
        private List<NativeValue> _valueList;
        private ExpressionNode _node;

        private bool _errorParsingExpr = false;

        /// <summary>
        /// Value of the expression
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Expression
        {
            get { return _expression; }
            set
            {
                ResetValueList();
                _expression = value;
            }
        }

        public bool IsParsable
        {
            get
            {
                EnsureValueList();
                return !_errorParsingExpr;
            }
        }

        /// <summary>
        /// Is this an empty expression
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(_expression); }
        }

        /// <summary>
        /// Root expression node
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public ExpressionNode Node
        {
            get
            {
                EnsureValueList();
                return _node;
            }
        }

        /// <summary>
        /// List of values in the expression
        /// </summary>
        public List<NativeValue> Values
        {
            get
            {
                EnsureValueList();
                return _valueList;
            }
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.ValueExpression; }
        }

        public NativeValueExpression(string expr)
        {
            this.Name = "Value";
            _expression = expr;
        }

        private void ResetValueList()
        {
            _valueList = null;
            _node = null;
        }

        public void EnsureValueList()
        {
            if (_valueList != null)
            {
                return;
            }

            if (IsEmpty)
            {
                _valueList = new List<NativeValue>();
                _errorParsingExpr = false;
                return;
            }

            var parser = new ExpressionParser();
            _valueList = new List<NativeValue>();

            // It's valid no have an invalid expression :)
            if (!parser.TryParse(_expression, out _node))
            {
                _errorParsingExpr = true;
                _node = null;
            }
            else
            {
                _errorParsingExpr = false;
            }

            CalculateValueList(_node);
        }

        private void CalculateValueList(ExpressionNode cur)
        {
            if (cur == null)
            {
                return;
            }

            if (cur.Kind == ExpressionKind.Leaf)
            {
                var ntVal = NativeValue.TryCreateForLeaf(cur, bag: null);

                if (ntVal != null)
                {
                    _valueList.Add(ntVal);
                }
                else
                {
                    _errorParsingExpr = true;
                }
            }
            else if (cur.Kind == ExpressionKind.Cast)
            {
                // Create nodes for the cast expressions.  The target should be a symbol
                _valueList.Add(NativeValue.CreateSymbolType(cur.Token.Value));
            }

            CalculateValueList(cur.LeftNode);
            CalculateValueList(cur.RightNode);
        }

        /// <summary>
        /// A Native value expression is resolved.  It may output as an error string but it will output
        /// a value.  This is needed to support constants that are defined to non-valid code but we still
        /// have to output the string value
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool IsImmediateResolved
        {
            get { return true; }
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            EnsureValueList();
            return base.GetListChild(_valueList);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            EnsureValueList();
            base.ReplaceChildInList(oldChild, newChild, _valueList);
        }
    }
}
