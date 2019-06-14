// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.Enums;
using PInvoke.NativeTypes.Enums;
using System;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Constant in Native code
    /// </summary>
    public class NativeConstant : NativeExtraSymbol
    {
        private NativeValueExpression _value;

        private ConstantKind _constantKind;
        /// <summary>
        /// What type of constant is this
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public ConstantKind ConstantKind
        {
            get { return _constantKind; }
            set { _constantKind = value; }
        }

        /// <summary>
        /// Value for the constant
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeValueExpression Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string RawValue
        {
            get
            {
                if (_value == null)
                {
                    return string.Empty;
                }

                return _value.Expression;
            }
        }

        public override NativeSymbolKind Kind => NativeSymbolKind.Constant;

        public NativeName NativeName => new NativeName(Name, NativeNameKind.Constant);

        private NativeConstant()
        {
        }

        public NativeConstant(string name) : this(name, null)
        {
        }

        public NativeConstant(string name, string value) : this(name, value, ConstantKind.Macro)
        {
        }

        public NativeConstant(string name, string value, ConstantKind kind)
        {
            this.Name = name ?? throw new ArgumentNullException("name");

            ConstantKind = kind;

            // We don't support macro methods at this point.  Instead we will just generate out the 
            // method signature for the method and print the string out into the code
            if (ConstantKind == ConstantKind.MacroMethod)
            {
                value = "\"" + value + "\"";
            }

            Value = new NativeValueExpression(value);
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(_value);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildSingle(oldChild, newChild, ref _value);
        }

    }
}
