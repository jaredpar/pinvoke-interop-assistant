// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Collections.Generic;
using System.Diagnostics;
using static PInvoke.Contract;
using PInvoke.Parser;
using PInvoke.NativeTypes.Enums;
using PInvoke.Parser.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a value inside of an expression
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{Value} ({ValueKind})")]
    public class NativeValue : NativeExtraSymbol
    {
        /// <summary>
        /// The actual value
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public object Value { get; set; }

        public NativeSymbol SymbolValue
        {
            get
            {
                if ((ValueKind == NativeValueKind.SymbolValue))
                {
                    return (NativeSymbol)Value;
                }

                return null;
            }
        }

        public NativeSymbol SymbolType
        {
            get
            {
                if ((ValueKind == NativeValueKind.SymbolType))
                {
                    return (NativeSymbol)Value;
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
        public NativeValueKind ValueKind { get; }

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
                        return this.Value != null;
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
                switch (ValueKind)
                {
                    case NativeValueKind.Number:
                        return Value.ToString();
                    case NativeValueKind.String:
                        return Value.ToString();
                    case NativeValueKind.Character:
                        return Value.ToString();
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
                        ThrowInvalidEnumValue(ValueKind);
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
            ValueKind = kind;
            Value = value;
        }

        public override IEnumerable<NativeSymbol> GetChildren()
        {
            if (ValueKind == NativeValueKind.SymbolType)
            {
                return GetSingleChild(SymbolType);
            }
            else if (ValueKind == NativeValueKind.SymbolValue)
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
            if (ValueKind == NativeValueKind.SymbolType)
            {
                NativeSymbol x = null;
                ReplaceChildSingle(SymbolType, newChild, ref x);
                Value = x;
            }
            else if (ValueKind == NativeValueKind.SymbolValue)
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

            var token = cur.Token;
            var ntVal = default(NativeValue);
            if (token.IsQuotedString)
            {
                if (TokenHelper.TryConvertToString(token, out string strValue))
                {
                    ntVal = NativeValue.CreateString(strValue);
                }
            }
            else if (token.IsNumber)
            {
                if (TokenHelper.TryConvertToNumber(token, out Number value))
                {
                    ntVal = NativeValue.CreateNumber(value);
                }
            }
            else if (token.IsCharacter)
            {
                var cValue = 'c';
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
                if (bag != null && bag.TryGetGlobalSymbol(token.Value, out NativeConstant constant))
                {
                    ntVal = NativeValue.CreateSymbolValue(token.Value, constant);
                }
                else if (bag != null && bag.TryGetEnumByValueName(token.Value, out NativeEnum enumeration, out NativeEnumValue value))
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
