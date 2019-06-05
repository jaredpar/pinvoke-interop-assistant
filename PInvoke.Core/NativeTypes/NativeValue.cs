// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Collections.Generic;
using System.Diagnostics;
using static PInvoke.Contract;
using PInvoke.Parser;
using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a value inside of an expression
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{Value} ({ValueKind})")]
    public class NativeValue : NativeExtraSymbol
    {
        private NativeValueKind _valueKind;

        private object _value;
        /// <summary>
        /// The actual value
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public NativeSymbol SymbolValue
        {
            get
            {
                if ((_valueKind == NativeValueKind.SymbolValue))
                {
                    return (NativeSymbol)_value;
                }

                return null;
            }
        }

        public NativeSymbol SymbolType
        {
            get
            {
                if ((_valueKind == NativeValueKind.SymbolType))
                {
                    return (NativeSymbol)_value;
                }

                return null;
            }
        }

        /// <summary>
        /// What kind of value is this
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeValueKind ValueKind
        {
            get { return _valueKind; }
        }

        /// <summary>
        /// Is the value resolvable
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsValueResolved
        {
            get
            {
                switch (this.ValueKind)
                {
                    case NativeValueKind.Number:
                    case NativeValueKind.String:
                    case NativeValueKind.Character:
                    case NativeValueKind.Boolean:
                        return this._value != null;
                    case NativeValueKind.SymbolType:
                        return SymbolType != null;
                    case NativeValueKind.SymbolValue:
                        return SymbolValue != null;
                    default:
                        ThrowInvalidEnumValue(this.ValueKind);
                        return false;
                }
            }
        }

        /// <summary>
        /// Get the value as a display string
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string DisplayValue
        {
            get
            {
                switch (_valueKind)
                {
                    case NativeValueKind.Number:
                        return _value.ToString();
                    case NativeValueKind.String:
                        return _value.ToString();
                    case NativeValueKind.Character:
                        return _value.ToString();
                    case NativeValueKind.SymbolType:
                        if (SymbolType != null)
                        {
                            return SymbolType.DisplayName;
                        }

                        return Name;
                    case NativeValueKind.SymbolValue:
                        if (SymbolValue != null)
                        {
                            return SymbolValue.DisplayName;
                        }

                        return Name;
                    default:
                        ThrowInvalidEnumValue(_valueKind);
                        return string.Empty;
                }
            }
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.Value; }
        }

        private NativeValue(object value, NativeValueKind kind) : this(kind.ToString(), value, kind)
        {
        }

        private NativeValue(string name, object value, NativeValueKind kind)
        {
            this.Name = name;
            _valueKind = kind;
            _value = value;
        }

        public override IEnumerable<NativeSymbol> GetChildren()
        {
            if (_valueKind == NativeValueKind.SymbolType)
            {
                return GetSingleChild(SymbolType);
            }
            else if (_valueKind == NativeValueKind.SymbolValue)
            {
                return GetSingleChild(SymbolValue);
            }
            else
            {
                return GetSingleChild<NativeSymbol>(null);
            }
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            if (_valueKind == NativeValueKind.SymbolType)
            {
                NativeSymbol x = null;
                ReplaceChildSingle(SymbolType, newChild, ref x);
                Value = x;
            }
            else if (_valueKind == NativeValueKind.SymbolValue)
            {
                NativeSymbol x = null;
                ReplaceChildSingle(SymbolValue, newChild, ref x);
                Value = x;
            }
        }

        public static NativeValue CreateNumber(int i)
        {
            return CreateNumber(new Number(i));
        }

        public static NativeValue CreateNumber(Number n)
        {
            // TODO: Consider passing Number through here.
            return new NativeValue(n.Value, NativeValueKind.Number);
        }

        public static NativeValue CreateBoolean(bool b)
        {
            return new NativeValue(b, NativeValueKind.Boolean);
        }

        public static NativeValue CreateString(string s)
        {
            return new NativeValue(s, NativeValueKind.String);
        }

        public static NativeValue CreateCharacter(char c)
        {
            return new NativeValue(c, NativeValueKind.Character);
        }

        public static NativeValue CreateSymbolValue(string name)
        {
            return new NativeValue(name, null, NativeValueKind.SymbolValue);
        }

        public static NativeValue CreateSymbolValue(string name, NativeSymbol ns)
        {
            return new NativeValue(name, ns, NativeValueKind.SymbolValue);
        }

        public static NativeValue CreateSymbolType(string name)
        {
            return new NativeValue(name, null, NativeValueKind.SymbolType);
        }

        public static NativeValue CreateSymbolType(string name, NativeSymbol ns)
        {
            return new NativeValue(name, ns, NativeValueKind.SymbolType);
        }

        public static NativeValue TryCreateForLeaf(ExpressionNode cur, NativeSymbolBag bag)
        {
            ThrowIfNull(cur);
            ThrowIfFalse(cur.Kind == ExpressionKind.Leaf);

            Token token = cur.Token;
            NativeValue ntVal = null;
            if (token.IsQuotedString)
            {
                string strValue = null;
                if (TokenHelper.TryConvertToString(token, out strValue))
                {
                    ntVal = NativeValue.CreateString(strValue);
                }
            }
            else if (token.IsNumber)
            {
                Number value;
                if (TokenHelper.TryConvertToNumber(token, out value))
                {
                    ntVal = NativeValue.CreateNumber(value);
                }
            }
            else if (token.IsCharacter)
            {
                char cValue = 'c';
                if (TokenHelper.TryConvertToChar(token, out cValue))
                {
                    ntVal = NativeValue.CreateCharacter(cValue);
                }
                else
                {
                    ntVal = NativeValue.CreateString(token.Value);
                }
            }
            else if (token.TokenType == TokenType.TrueKeyword)
            {
                ntVal = NativeValue.CreateBoolean(true);
            }
            else if (token.TokenType == Parser.TokenType.FalseKeyword)
            {
                ntVal = NativeValue.CreateBoolean(false);
            }
            else if (token.IsAnyWord)
            {
                NativeConstant constant;
                NativeEnum enumeration;
                NativeEnumValue value;
                if (bag != null && bag.TryGetGlobalSymbol(token.Value, out constant))
                {
                    ntVal = NativeValue.CreateSymbolValue(token.Value, constant);
                }
                else if (bag != null && bag.TryGetEnumByValueName(token.Value, out enumeration, out value))
                {
                    ntVal = NativeValue.CreateSymbolValue(token.Value, enumeration);
                }
                else
                {
                    ntVal = NativeValue.CreateSymbolValue(token.Value);
                }
            }

            return ntVal;
        }
    }
}
