// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.Enums;
using PInvoke.NativeTypes.Enums;
using System.Collections.Generic;
using System.Diagnostics;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// An enum value
    /// </summary>
    [DebuggerDisplay("{Name} = {Value}")]
    public class NativeEnumValue : NativeExtraSymbol
    {
        private NativeValueExpression _value;

        public string EnumName { get; }

        /// <summary>
        /// Value of the value
        /// </summary>
        public NativeValueExpression Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public NativeName NativeName => new NativeName(Name, NativeNameKind.EnumValue);

        public NativeEnumValue(string enumName, string valueName, string value = "")
        {
            EnumName = enumName;
            Name = valueName;
            Value = new NativeValueExpression(value);
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.EnumNameValue; }
        }

        public override IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(_value);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildSingle(oldChild, newChild, ref _value);
        }
    }
}
