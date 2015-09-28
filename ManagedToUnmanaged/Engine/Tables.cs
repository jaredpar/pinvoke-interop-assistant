/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SignatureGenerator
{
    /// <summary>
    /// Lists allowed unmanaged marshal types for primitive managed types.
    /// </summary>
    /// <remarks>
    /// The first item of the field denotes the default unmanaged type which is used when there is no
    /// attribute <see cref="MarshalAs"/>. The &quot;P&quot; suffix indicates that the array is valid
    /// for P/Invoke use, whereas the &quot;C&quot; suffix means that it is valid for COM Interop use.
    /// </remarks>
    internal static class MarshalType
    {
        public static readonly UnmanagedType[] Int16     = { UnmanagedType.I2, UnmanagedType.U2 };
        public static readonly UnmanagedType[] Int32     = { UnmanagedType.I4, UnmanagedType.U4, UnmanagedType.Error };
        public static readonly UnmanagedType[] Int64     = { UnmanagedType.I8, UnmanagedType.U8 };
        public static readonly UnmanagedType[] UInt16    = { UnmanagedType.U2, UnmanagedType.I2 };
        public static readonly UnmanagedType[] UInt32    = { UnmanagedType.U4, UnmanagedType.I4, UnmanagedType.Error };
        public static readonly UnmanagedType[] UInt64    = { UnmanagedType.U8, UnmanagedType.I8 };

        public static readonly UnmanagedType[] BooleanP  = { UnmanagedType.Bool, UnmanagedType.VariantBool, UnmanagedType.I1, UnmanagedType.U1 };
        public static readonly UnmanagedType[] BooleanC  = { UnmanagedType.VariantBool, UnmanagedType.Bool, UnmanagedType.I1, UnmanagedType.U1 };
        public static readonly UnmanagedType[] Char      = { UnmanagedType.I1, UnmanagedType.U1, UnmanagedType.U2, UnmanagedType.I2 };

        public static readonly UnmanagedType[] DecimalParam = { UnmanagedType.Struct, UnmanagedType.LPStruct, UnmanagedType.Currency };
        public static readonly UnmanagedType[] DecimalField = { UnmanagedType.Struct, UnmanagedType.Currency };

        public static readonly UnmanagedType[] Class     = { UnmanagedType.LPStruct };
        public static readonly UnmanagedType[] Struct    = { UnmanagedType.Struct };
        public static readonly UnmanagedType[] Empty     = { };

        public static readonly UnmanagedType[] Byte      = { UnmanagedType.U1, UnmanagedType.I1 };
        public static readonly UnmanagedType[] SByte     = { UnmanagedType.I1, UnmanagedType.U1 };
        
        public static readonly UnmanagedType[] Single    = { UnmanagedType.R4 };
        public static readonly UnmanagedType[] Double    = { UnmanagedType.R8 };
        
        // the default unmanaged type depends on ANSI/Unicode/auto setting, more restrictions apply
        public static readonly UnmanagedType[] StringP   = { UnmanagedType.LPTStr, UnmanagedType.LPWStr, UnmanagedType.LPStr, UnmanagedType.BStr,
            UnmanagedType.TBStr, UnmanagedType.VBByRefStr, UnmanagedType.AnsiBStr };

        // COM disallows LPTStr
        public static readonly UnmanagedType[] StringC   = { UnmanagedType.BStr, UnmanagedType.LPWStr, UnmanagedType.LPStr, UnmanagedType.TBStr,
            UnmanagedType.VBByRefStr, UnmanagedType.AnsiBStr };

        // the default unmanaged type depends on ANSI/Unicode/auto setting, more restrictions apply
        public static readonly UnmanagedType[] SBuilderP = { UnmanagedType.LPTStr, UnmanagedType.LPWStr, UnmanagedType.LPStr };

        // COM disallows LPTStr
        public static readonly UnmanagedType[] SBuilderC = { UnmanagedType.LPWStr, UnmanagedType.LPStr };

        public static readonly UnmanagedType[] IntPtr     = { UnmanagedType.SysInt, UnmanagedType.SysUInt };
        public static readonly UnmanagedType[] Pointer    = { }; // non-default unmanaged type disallowed
        public static readonly UnmanagedType[] DelegP     = { UnmanagedType.FunctionPtr, UnmanagedType.Interface };
        public static readonly UnmanagedType[] DelegC     = { UnmanagedType.Interface, UnmanagedType.FunctionPtr };
        public static readonly UnmanagedType[] DelegField = { UnmanagedType.FunctionPtr };
        public static readonly UnmanagedType[] Guid       = { UnmanagedType.Struct, UnmanagedType.LPStruct };

        public static readonly UnmanagedType[] ArrayClass = { UnmanagedType.Interface, UnmanagedType.SafeArray };
        public static readonly UnmanagedType[] ArrayClassField = { UnmanagedType.Interface, UnmanagedType.SafeArray, UnmanagedType.ByValArray };

        public static readonly UnmanagedType[] ArrayP     = { UnmanagedType.LPArray, UnmanagedType.SafeArray };
        public static readonly UnmanagedType[] ArrayC     = { UnmanagedType.SafeArray, UnmanagedType.LPArray };
        public static readonly UnmanagedType[] ArrayField = { UnmanagedType.SafeArray, UnmanagedType.ByValArray };

        public static readonly UnmanagedType[] Interface  = { UnmanagedType.Interface };

        public static readonly UnmanagedType[] ObjectParam = { UnmanagedType.Struct, UnmanagedType.Interface, UnmanagedType.IUnknown,
            UnmanagedType.IDispatch, UnmanagedType.AsAny };

        // field marshaling
        public static readonly UnmanagedType[] StringField = { UnmanagedType.LPTStr, UnmanagedType.LPWStr, UnmanagedType.LPStr, UnmanagedType.BStr,
            UnmanagedType.ByValTStr };

        public static readonly UnmanagedType[] ObjectField = { UnmanagedType.Interface, UnmanagedType.IUnknown, UnmanagedType.IDispatch,
            UnmanagedType.Struct };
        public static readonly UnmanagedType[] ObjectElement = { UnmanagedType.Struct, UnmanagedType.IUnknown, UnmanagedType.IDispatch,
            UnmanagedType.Interface };
    }

    /// <summary>
    /// Defines names of primitive unmanaged types.
    /// </summary>
    [Serializable]
    public struct TypeName
    {
        public enum PtrPrefix
        {
            None, P_Prefix, LP_Prefix
        }

        public const int Platform32PointerSize = 4;
        public const int Platform64PointerSize = 8;

        public readonly int size; // -1 means platform pointer size (4 or 8 bytes)
        public readonly string PlainC;
        public readonly string WinApi;
        public readonly PtrPrefix PointerPrefix;

        private TypeName(string plainC, string winApi, int size, PtrPrefix pointerPrefix)
        {
            this.PlainC = plainC;
            this.WinApi = winApi;
            this.size = size;
            this.PointerPrefix = pointerPrefix;
        }

        private TypeName(string plainC, string winApi, int size)
            : this(plainC, winApi, size, PtrPrefix.LP_Prefix)
        { }

        static TypeName()
        {
            primitiveUnmanagedToNameMap = new TypeName[Utility.GetMaxEnumValue<int>(typeof(UnmanagedType)) + 1];

            primitiveUnmanagedToNameMap[(int)UnmanagedType.I1] = TypeName.I1;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.I2] = TypeName.I2;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.I4] = TypeName.I4;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.I8] = TypeName.I8;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.U1] = TypeName.U1;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.U2] = TypeName.U2;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.U4] = TypeName.U4;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.U8] = TypeName.U8;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.R4] = TypeName.R4;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.R8] = TypeName.R8;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.SysInt]      = TypeName.Void;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.SysUInt]     = TypeName.Void;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.Error]       = TypeName.Error;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.Bool]        = TypeName.Bool;
            primitiveUnmanagedToNameMap[(int)UnmanagedType.VariantBool] = TypeName.VariantBool;
        }

        public static TypeName MakeTypeName(string name, int size)
        {
            return new TypeName(name, name, size, PtrPrefix.None);
        }

        public bool IsEmpty
        {
            get
            { return (PlainC == null || WinApi == null); }
        }

        public int GetSize(bool platform64bit)
        {
            // should never ask for the size of void
            Debug.Assert(size != 0);

            if (size >= 0) return size;

            int neg_size = -size;
            return ((neg_size & 0xff) * GetPointerSize(platform64bit) + (neg_size >> 8));
        }

        public static int GetPointerSize(bool platform64bit)
        {
            return (platform64bit ? Platform64PointerSize : Platform32PointerSize);
        }

        #region PrintTo

        public static void PrintTo(ICodePrinter printer, string typeNameStr)
        {
            // - words starting with upper-case letter are identifiers
            // - words starting with lower-case letter or underscore are keywords
            // - * is an operator
            // - space is 'other'

            StringBuilder sb = new StringBuilder(typeNameStr.Length);
            OutputType type = OutputType.Other;

            for (int i = 0; i < typeNameStr.Length; i++)
            {
                char ch = typeNameStr[i];

                OutputType new_type = type;
                switch (ch)
                {
                    case ' ': new_type = OutputType.Other; break;
                    case '*': new_type = OutputType.Operator; break;
                    case '_': break;

                    default:
                    {
                        if (Char.IsLower(ch) && type != OutputType.TypeName) new_type = OutputType.Keyword;
                        else if (Char.IsUpper(ch)) new_type = OutputType.TypeName;
                        break;
                    }
                }

                if (new_type != type)
                {
                    if (sb.Length > 0)
                    {
                        printer.Print(type, sb.ToString());
                        sb.Length = 0;
                    }
                    type = new_type;
                }

                sb.Append(ch);
            }

            if (sb.Length > 0) printer.Print(type, sb.ToString());
        }

        #endregion

        // indexed by UnmanagedType values
        private static readonly TypeName[] primitiveUnmanagedToNameMap;

        public static TypeName GetTypeNameForUnmanagedType(UnmanagedType unmngType)
        {
            return primitiveUnmanagedToNameMap[(int)unmngType];
        }

        // first item is the plain C name, second item is the usual COM/Windows API name
        public static readonly TypeName Empty = new TypeName(null, null, 0, PtrPrefix.None);

        public static readonly TypeName I1           = new TypeName("char",             "CHAR",    1, PtrPrefix.P_Prefix);
        public static readonly TypeName U1           = new TypeName("unsigned char",    "BYTE",    1);
        public static readonly TypeName I1_Bool      = new TypeName("bool",             "CHAR",    1, PtrPrefix.P_Prefix);
        public static readonly TypeName U1_Bool      = new TypeName("bool",             "BYTE",    1);
        public static readonly TypeName I2           = new TypeName("short",            "SHORT",   2);
        public static readonly TypeName U2           = new TypeName("unsigned short",   "WORD",    2);
        public static readonly TypeName I4           = new TypeName("int",              "INT",     4);
        public static readonly TypeName U4           = new TypeName("unsigned int",     "UINT",    4);
        public static readonly TypeName I8           = new TypeName("__int64",          "LONG64",  8);
        public static readonly TypeName U8           = new TypeName("unsigned __int64", "ULONG64", 8);

        public static readonly TypeName R4           = new TypeName("float",            "FLOAT",   4);
        public static readonly TypeName R8           = new TypeName("double",           "DOUBLE",  8, PtrPrefix.None); // there's no (L)PDOUBLE

        public static readonly TypeName Void         = new TypeName("void",             "VOID",    0);    
        public static readonly TypeName UChar        = new TypeName("unsigned char",    "UCHAR",   1);
        public static readonly TypeName WChar        = new TypeName("wchar_t",          "WCHAR",   2);
        public static readonly TypeName TCharA       = new TypeName("char",             "TCHAR",   1);
        public static readonly TypeName TCharW       = new TypeName("wchar_t",          "TCHAR",   2);

        public static readonly TypeName BStr         = new TypeName("wchar_t *",        "BSTR",    -1, PtrPrefix.None);
        public static readonly TypeName AnsiBStr     = new TypeName("char *",           "PCHAR",   -1, PtrPrefix.None);
        public static readonly TypeName TBStrA       = new TypeName("char *",           "PTCHAR",  -1, PtrPrefix.None);
        public static readonly TypeName TBStrW       = new TypeName("wchar_t *",        "PTCHAR",  -1, PtrPrefix.None);

        public static readonly TypeName LPStr        = new TypeName("char *",           "LPSTR",   -1, PtrPrefix.None);
        public static readonly TypeName LPCStr       = new TypeName("const char *",     "LPCSTR",  -1, PtrPrefix.None);
        public static readonly TypeName LPWStr       = new TypeName("wchar_t *",        "LPWSTR",  -1, PtrPrefix.None);
        public static readonly TypeName LPCWStr      = new TypeName("const wchar_t *",  "LPCWSTR", -1, PtrPrefix.None);
        public static readonly TypeName LPTStrA      = new TypeName("char *",           "LPTSTR",  -1, PtrPrefix.None);
        public static readonly TypeName LPTStrW      = new TypeName("wchar_t *",        "LPTSTR",  -1, PtrPrefix.None);
        public static readonly TypeName LPCTStrA     = new TypeName("const char *",     "LPCTSTR", -1, PtrPrefix.None);
        public static readonly TypeName LPCTStrW     = new TypeName("const wchar_t *",  "LPCTSTR", -1, PtrPrefix.None);

        public static readonly TypeName Bool         = new TypeName("int",              "BOOL",         4);
        public static readonly TypeName VariantBool  = new TypeName("short",            "VARIANT_BOOL", 2,  PtrPrefix.None);
        public static readonly TypeName Currency     = new TypeName("__int64",          "CURRENCY",     8,  PtrPrefix.None);
        public static readonly TypeName Decimal      = new TypeName("DECIMAL",          "DECIMAL",      16, PtrPrefix.None);
        public static readonly TypeName Error        = new TypeName("long",             "HRESULT",      4,  PtrPrefix.None);
        public static readonly TypeName OleColor     = new TypeName("unsigned long",    "OLE_COLOR",    4);
        public static readonly TypeName SafeArray    = new TypeName("SAFEARRAY",        "SAFEARRAY",    -(2 + (16 << 8)), PtrPrefix.LP_Prefix); // 16 + 2 * pointer size
        public static readonly TypeName Variant      = new TypeName("VARIANT" ,         "VARIANT",      -(2 + (8 << 8)),  PtrPrefix.LP_Prefix); // 8 + 2 * pointer size
        public static readonly TypeName Date         = new TypeName("double",           "DATE",         8);
        public static readonly TypeName Guid         = new TypeName("GUID",             "GUID",         16);
        public static readonly TypeName Handle       = new TypeName("void *",           "HANDLE",       -1);
        public static readonly TypeName VaList       = new TypeName("va_list",          "va_list",      -1);

        public static readonly TypeName IDispatch    = new TypeName("IDispatch *",      "IDispatch *",    -1, PtrPrefix.None);
        public static readonly TypeName IUnknown     = new TypeName("IUnknown *",       "IUnknown *",     -1, PtrPrefix.None);
        public static readonly TypeName IEnumVARIANT = new TypeName("IEnumVARIANT *",   "IEnumVARIANT *", -1, PtrPrefix.None);
        public static readonly TypeName Array        = new TypeName("_Array *",         "_Array *",       -1, PtrPrefix.None);
    }

    static class CppKeywords
    {
        private static Set<string> table;

        static CppKeywords()
        {
            table = new Set<string>(new String[]
            {
                "auto",     "break",   "case",      "char",    "const",    "continue", "default",  "do",
                "double",   "else",    "enum",      "extern",  "float",    "for",      "goto",     "if",
                "int",      "long",    "register",  "return",  "short",    "signed",   "sizeof",   "static",
                "struct",   "switch",  "typedef",   "union",   "unsigned", "void",     "volatile", "while",
                "bool",     "catch",   "class",     "delete",  "friend",   "inline",   "new",      "namespace",
                "operator", "private", "protected", "public",  "tempate",  "this",     "throw",    "try",
                "template",
                "__int8",  "__int16",   "__int32", "__int64",  "_int8",    "_int16",   "_int32",   "_int64"
            });
        }

        public static bool IsKeyword(string str)
        {
            Debug.Assert(!String.IsNullOrEmpty(str));

            if (Char.IsUpper(str[0])) return false;

            return table.Contains(str);
        }
    }
}
