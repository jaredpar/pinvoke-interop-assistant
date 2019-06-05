// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System;
using System.Diagnostics;
using static PInvoke.Contract;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Built-in types (int, boolean, etc ...)
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayName}")]
    public class NativeBuiltinType : NativeSpecializedType
    {
        private BuiltinType _builtinType;
        private bool _isUnsigned;
        private Type _managedType;

        private System.Runtime.InteropServices.UnmanagedType _unmanagedType;
        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.BuiltinType; }
        }

        public override string DisplayName
        {
            get
            {
                if (_builtinType == PInvoke.BuiltinType.NativeUnknown)
                {
                    return Name;
                }

                string str = Name;
                if (IsUnsigned)
                {
                    str = "unsigned " + str;
                }

                return str;
            }
        }

        /// <summary>
        /// Bulitin Type
        /// </summary>
        public BuiltinType BuiltinType
        {
            get { return _builtinType; }
            set { _builtinType = value; }
        }

        public bool IsUnsigned
        {
            get { return _isUnsigned; }
            set
            {
                _isUnsigned = value;
                Init();
            }
        }

        public Type ManagedType
        {
            get { return _managedType; }
        }

        public System.Runtime.InteropServices.UnmanagedType UnmanagedType
        {
            get { return _unmanagedType; }
        }

        public NativeBuiltinType(BuiltinType bt) : base("")
        {
            BuiltinType = bt;
            Init();
        }

        public NativeBuiltinType(BuiltinType bt, bool isUnsigned) : this(bt)
        {
            this.IsUnsigned = isUnsigned;
            Init();
        }

        public NativeBuiltinType(string name) : base(name)
        {
            BuiltinType = PInvoke.BuiltinType.NativeUnknown;
            Init();
        }

        private void Init()
        {
            switch (this.BuiltinType)
            {
                case PInvoke.BuiltinType.NativeBoolean:
                    Name = "boolean";
                    _managedType = typeof(bool);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.Bool;
                    break;
                case PInvoke.BuiltinType.NativeByte:
                    Name = "byte";
                    _managedType = typeof(byte);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I1;
                    break;
                case PInvoke.BuiltinType.NativeInt16:
                    Name = "short";
                    if (IsUnsigned)
                    {
                        _managedType = typeof(UInt16);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.U2;
                    }
                    else
                    {
                        _managedType = typeof(Int16);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I2;
                    }
                    break;
                case PInvoke.BuiltinType.NativeInt32:
                    Name = "int";
                    if (IsUnsigned)
                    {
                        _managedType = typeof(UInt32);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.U4;
                    }
                    else
                    {
                        _managedType = typeof(Int32);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I4;
                    }
                    break;
                case PInvoke.BuiltinType.NativeInt64:
                    Name = "__int64";
                    if (IsUnsigned)
                    {
                        _managedType = typeof(UInt64);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.U8;
                    }
                    else
                    {
                        _managedType = typeof(Int64);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I8;
                    }
                    break;
                case PInvoke.BuiltinType.NativeChar:
                    Name = "char";
                    _managedType = typeof(byte);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I1;
                    break;
                case PInvoke.BuiltinType.NativeWChar:
                    Name = "wchar";
                    _managedType = typeof(char);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I2;
                    break;
                case PInvoke.BuiltinType.NativeFloat:
                    Name = "float";
                    _managedType = typeof(float);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.R4;
                    break;
                case PInvoke.BuiltinType.NativeDouble:
                    Name = "double";
                    _managedType = typeof(double);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.R8;
                    break;
                case PInvoke.BuiltinType.NativeVoid:
                    Name = "void";
                    _managedType = typeof(void);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.AsAny;
                    break;
                case PInvoke.BuiltinType.NativeUnknown:
                    Name = "unknown";
                    _managedType = typeof(object);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.AsAny;
                    break;
                default:
                    ThrowInvalidEnumValue(BuiltinType);
                    break;
            }

        }

        public static bool TryConvertToBuiltinType(string name, out NativeBuiltinType nativeBt)
        {
            Parser.TokenType tt = default(Parser.TokenType);
            if (Parser.TokenHelper.KeywordMap.TryGetValue(name, out tt))
            {
                return TryConvertToBuiltinType(tt, out nativeBt);
            }

            nativeBt = null;
            return false;
        }

        public static bool TryConvertToBuiltinType(Parser.TokenType tt, out NativeBuiltinType nativeBt)
        {
            if (!Parser.TokenHelper.IsTypeKeyword(tt))
            {
                nativeBt = null;
                return false;
            }

            BuiltinType bt = default(BuiltinType);
            bool isUnsigned = false;
            switch (tt)
            {
                case Parser.TokenType.BooleanKeyword:
                    bt = PInvoke.BuiltinType.NativeBoolean;
                    break;
                case Parser.TokenType.ByteKeyword:
                    bt = PInvoke.BuiltinType.NativeByte;
                    break;
                case Parser.TokenType.ShortKeyword:
                case Parser.TokenType.Int16Keyword:
                    bt = PInvoke.BuiltinType.NativeInt16;
                    break;
                case Parser.TokenType.IntKeyword:
                case Parser.TokenType.LongKeyword:
                case Parser.TokenType.SignedKeyword:
                    bt = PInvoke.BuiltinType.NativeInt32;
                    break;
                case Parser.TokenType.UnsignedKeyword:
                    bt = PInvoke.BuiltinType.NativeInt32;
                    isUnsigned = true;
                    break;
                case Parser.TokenType.Int64Keyword:
                    bt = PInvoke.BuiltinType.NativeInt64;
                    break;
                case Parser.TokenType.CharKeyword:
                    bt = PInvoke.BuiltinType.NativeChar;
                    break;
                case Parser.TokenType.WCharKeyword:
                    bt = PInvoke.BuiltinType.NativeWChar;
                    break;
                case Parser.TokenType.FloatKeyword:
                    bt = PInvoke.BuiltinType.NativeFloat;
                    break;
                case Parser.TokenType.DoubleKeyword:
                    bt = PInvoke.BuiltinType.NativeDouble;
                    break;
                case Parser.TokenType.VoidKeyword:
                    bt = PInvoke.BuiltinType.NativeVoid;
                    break;
                default:
                    bt = PInvoke.BuiltinType.NativeUnknown;
                    ThrowInvalidEnumValue(tt);
                    break;
            }

            nativeBt = new NativeBuiltinType(bt, isUnsigned);
            return true;
        }

        public static string BuiltinTypeToName(BuiltinType bt)
        {
            NativeBuiltinType nativeBt = new NativeBuiltinType(bt);
            return nativeBt.Name;
        }

        public static bool IsNumberType(BuiltinType bt)
        {

            if (bt == PInvoke.BuiltinType.NativeInt16 || bt == PInvoke.BuiltinType.NativeInt32 || bt == PInvoke.BuiltinType.NativeInt64 || bt == PInvoke.BuiltinType.NativeFloat || bt == PInvoke.BuiltinType.NativeDouble || bt == PInvoke.BuiltinType.NativeByte)
            {
                return true;
            }

            return false;
        }

    }
}
