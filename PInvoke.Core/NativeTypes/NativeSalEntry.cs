// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System.Diagnostics;
using static PInvoke.Contract;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a SAL attribute in code
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayName}")]
    public sealed class NativeSalEntry : NativeExtraSymbol
    {
        private SalEntryType _type;

        private string _text;

        /// <summary>
        /// Type of attribute
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public SalEntryType SalEntryType
        {
            get { return _type; }
            set
            {
                _type = value;
                this.Name = value.ToString();
            }
        }

        /// <summary>
        /// Text of the attribute
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public override string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return Name;
                }
                else
                {
                    return string.Format("{0}({1})", Name, Text);
                }
            }
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.SalEntry; }
        }

        public NativeSalEntry() : this(SalEntryType.Null, string.Empty)
        {

        }

        public NativeSalEntry(SalEntryType type) : this(type, string.Empty)
        {
        }

        public NativeSalEntry(SalEntryType type, string text)
        {
            this.SalEntryType = type;
            _type = type;
            _text = text;
        }

        public static string GetDirectiveForEntry(SalEntryType entry)
        {
            switch (entry)
            {
                case SalEntryType.Null:
                    return "SAL_null";
                case SalEntryType.NotNull:
                    return "SAL_notnull";
                case SalEntryType.MaybeNull:
                    return "SAL_maybenull";
                case SalEntryType.ReadOnly:
                    return "SAL_readonly";
                case SalEntryType.NotReadOnly:
                    return "SAL_notreadonly";
                case SalEntryType.MaybeReadOnly:
                    return "SAL_maybereadonly";
                case SalEntryType.Valid:
                    return "SAL_valid";
                case SalEntryType.NotValid:
                    return "SAL_notvalid";
                case SalEntryType.MaybeValid:
                    return "SAL_maybevalid";
                case SalEntryType.ReadableTo:
                    return "SAL_readableTo()";
                case SalEntryType.ElemReadableTo:
                    return "SAL_readableTo(elementCount())";
                case SalEntryType.ByteReadableTo:
                    return "SAL_readableTo(byteCount())";
                case SalEntryType.WritableTo:
                    return "SAL_writableTo()";
                case SalEntryType.ElemWritableTo:
                    return "SAL_writableTo(elementCount())";
                case SalEntryType.ByteWritableTo:
                    return "SAL_writableTo(byteCount())";
                case SalEntryType.Deref:
                    return "SAL_deref";
                case SalEntryType.Pre:
                    return "SAL_pre";
                case SalEntryType.Post:
                    return "SAL_post";
                case SalEntryType.ExceptThat:
                    return "SAL_except";
                case SalEntryType.InnerControlEntryPoint:
                    return "SAL_entrypoint(controlEntry, )";
                case SalEntryType.InnerDataEntryPoint:
                    return "SAL_entrypoint(dataEntry, )";
                case SalEntryType.InnerSucces:
                    return "SAL_success()";
                case SalEntryType.InnerCheckReturn:
                    return "SAL_checkReturn";
                case SalEntryType.InnerTypefix:
                    return "SAL_typefix";
                case SalEntryType.InnerOverride:
                    return "__override";
                case SalEntryType.InnerCallBack:
                    return "__callback";
                case SalEntryType.InnerBlocksOn:
                    return "SAL_blocksOn()";
                default:
                    ThrowInvalidEnumValue(entry);
                    return string.Empty;
            }
        }

    }
}
