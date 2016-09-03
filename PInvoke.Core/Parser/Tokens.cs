// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using static PInvoke.Contract;

namespace PInvoke.Parser
{

    #region "TokenType"
    public enum TokenType
    {

        // Pre-processor macros
        PoundDefine,
        PoundIf,
        PoundIfndef,
        PoundUnDef,
        PoundElse,
        PoundElseIf,
        PoundEndIf,
        PoundInclude,
        PoundPragma,
        PoundError,

        // Delimeter tokens
        BraceOpen,
        BraceClose,
        ParenOpen,
        ParenClose,
        BracketOpen,
        BracketClose,
        Comma,
        Semicolon,
        Colon,
        DoubleQuote,
        SingleQuote,
        Asterisk,
        Period,
        Bang,
        Ampersand,
        Pipe,
        BackSlash,
        Pound,

        // Operator tokens
        OpAssign,
        OpEquals,
        OpNotEquals,
        OpGreaterThan,
        OpLessThan,
        OpGreaterThanOrEqual,
        OpLessThanOrEqual,
        OpBoolAnd,
        OpBoolOr,
        OpPlus,
        OpMinus,
        OpDivide,
        OpModulus,
        OpShiftLeft,
        OpShiftRight,

        WhiteSpace,
        NewLine,

        // Reads through the comments
        LineComment,
        BlockComment,

        // Different words
        Word,
        Text,
        QuotedStringAnsi,
        QuotedStringUnicode,
        CharacterAnsi,
        CharacterUnicode,
        Number,
        HexNumber,

        // Keywords 
        StructKeyword,
        UnionKeyword,
        EnumKeyword,
        ClassKeyword,
        TypedefKeyword,
        InlineKeyword,
        VolatileKeyword,
        ClrCallKeyword,
        CDeclarationCallKeyword,
        StandardCallKeyword,
        PascalCallKeyword,
        WinApiCallKeyword,
        Pointer32Keyword,
        Pointer64Keyword,
        ConstKeyword,
        TrueKeyword,
        FalseKeyword,
        PublicKeyword,
        PrivateKeyword,
        ProtectedKeyword,
        SignedKeyword,
        UnsignedKeyword,

        // Type Keywords
        BooleanKeyword,
        ByteKeyword,
        ShortKeyword,
        Int16Keyword,
        IntKeyword,
        Int64Keyword,
        LongKeyword,
        CharKeyword,
        WCharKeyword,
        FloatKeyword,
        DoubleKeyword,
        VoidKeyword,

        // SAL
        DeclSpec,

        /// <summary>
        /// End of the Stream we are scanning
        /// </summary>
        /// <remarks></remarks>
        EndOfStream

    }

    #endregion

    #region "Token"

    [DebuggerDisplay("{TokenType} - {Value}")]
    public class Token
    {
        private TokenType _tokenType;

        private string _value;
        /// <summary>
        /// Type of the token
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TokenType TokenType
        {
            get { return _tokenType; }
        }

        public string Value
        {
            get { return _value; }
        }

        public bool IsKeyword
        {
            get { return TokenHelper.IsKeyword(_tokenType); }
        }

        public bool IsAnyWord
        {
            get { return _tokenType == Parser.TokenType.Word || IsKeyword; }
        }

        public bool IsCallTypeModifier
        {
            get { return TokenHelper.IsCallTypeModifier(_tokenType); }
        }

        public bool IsBinaryOperation
        {
            get { return TokenHelper.IsBinaryOperation(_tokenType); }
        }

        public bool IsPreProcessorDirective
        {
            get { return TokenHelper.IsPreprocessorToken(_tokenType); }
        }

        public bool IsCharacter
        {
            get { return TokenHelper.IsCharacter(_tokenType); }
        }

        public bool IsQuotedString
        {
            get { return TokenHelper.IsQuotedString(_tokenType); }
        }

        public bool IsNumber
        {
            get { return TokenHelper.IsNumber(_tokenType); }
        }

        public bool IsAccessModifier
        {
            get { return TokenHelper.IsAccessModifier(_tokenType); }
        }

        public bool IsTypeKeyword
        {
            get { return TokenHelper.IsTypeKeyword(_tokenType); }
        }

        public Token(TokenType tType, string val)
        {
            _tokenType = tType;
            _value = val;
        }
    }

    #endregion

    #region "TokenHelper"

    /// <summary>
    /// Helper methods for Tokens
    /// </summary>
    /// <remarks></remarks>
    public static class TokenHelper
    {


        private static Dictionary<string, TokenType> s_keywordMap;
        public static Dictionary<string, TokenType> KeywordMap
        {
            get
            {
                if (s_keywordMap == null)
                {
                    s_keywordMap = BuildKeywordMap();
                }

                return s_keywordMap;
            }
        }

        public static bool IsKeyword(string word)
        {
            return KeywordMap.ContainsKey(word);
        }

        public static bool IsKeyword(TokenType tt)
        {
            return KeywordMap.ContainsValue(tt);
        }

        public static bool IsCallTypeModifier(TokenType tt)
        {
            switch (tt)
            {
                case TokenType.ClrCallKeyword:
                    return true;
                case TokenType.InlineKeyword:
                    return true;
                case TokenType.CDeclarationCallKeyword:
                    return true;
                case TokenType.StandardCallKeyword:
                    return true;
                case TokenType.PascalCallKeyword:
                    return true;
                case TokenType.WinApiCallKeyword:
                    return true;
                default:
                    return false;
            }
        }

        private static Dictionary<string, TokenType> BuildKeywordMap()
        {
            Dictionary<string, TokenType> keywordMap = new Dictionary<string, TokenType>(StringComparer.Ordinal);
            keywordMap["struct"] = TokenType.StructKeyword;
            keywordMap["union"] = TokenType.UnionKeyword;
            keywordMap["typedef"] = TokenType.TypedefKeyword;
            keywordMap["enum"] = TokenType.EnumKeyword;
            keywordMap["class"] = TokenType.ClassKeyword;
            keywordMap["__declspec"] = TokenType.DeclSpec;
            keywordMap["volatile"] = TokenType.VolatileKeyword;
            keywordMap["__inline"] = TokenType.InlineKeyword;
            keywordMap["__forceinline"] = TokenType.InlineKeyword;
            keywordMap["inline"] = TokenType.InlineKeyword;
            keywordMap["__clrcall"] = TokenType.ClrCallKeyword;
            keywordMap["__ptr32"] = TokenType.Pointer32Keyword;
            keywordMap["__ptr64"] = TokenType.Pointer64Keyword;
            keywordMap["const"] = TokenType.ConstKeyword;
            keywordMap["false"] = TokenType.FalseKeyword;
            keywordMap["true"] = TokenType.TrueKeyword;
            keywordMap["_cdecl"] = TokenType.CDeclarationCallKeyword;
            keywordMap["__cdecl"] = TokenType.CDeclarationCallKeyword;
            keywordMap["__stdcall"] = TokenType.StandardCallKeyword;
            keywordMap["__pascal"] = TokenType.PascalCallKeyword;
            keywordMap["__winapi"] = TokenType.WinApiCallKeyword;
            keywordMap["public"] = TokenType.PublicKeyword;
            keywordMap["private"] = TokenType.PrivateKeyword;
            keywordMap["protected"] = TokenType.ProtectedKeyword;

            // type information
            keywordMap["signed"] = TokenType.SignedKeyword;
            keywordMap["unsigned"] = TokenType.UnsignedKeyword;

            // Update builtin type map
            keywordMap.Add("boolean", TokenType.BooleanKeyword);
            keywordMap.Add("bool", TokenType.BooleanKeyword);
            keywordMap.Add("byte", TokenType.ByteKeyword);
            keywordMap.Add("short", TokenType.ShortKeyword);
            keywordMap.Add("__int16", TokenType.Int16Keyword);
            keywordMap.Add("int", TokenType.IntKeyword);
            keywordMap.Add("long", TokenType.LongKeyword);
            keywordMap.Add("__int32", TokenType.IntKeyword);
            keywordMap.Add("__int64", TokenType.Int64Keyword);
            keywordMap.Add("char", TokenType.CharKeyword);
            keywordMap.Add("wchar", TokenType.WCharKeyword);
            keywordMap.Add("float", TokenType.FloatKeyword);
            keywordMap.Add("double", TokenType.DoubleKeyword);
            keywordMap.Add("void", TokenType.VoidKeyword);

            // Make sure to update iscalltypemodifier as well
            return keywordMap;
        }

        public static bool IsAnyWord(TokenType tt)
        {
            return tt == TokenType.Word || IsKeyword(tt);
        }

        private struct NumberInfo
        {
            public NumberStyles Style;
            public bool IsUnsigned;
            public bool IsLong;
            public bool IsFloatingPoint;
            public bool IsOctal;
            public bool IsForced64;
            public Int32 Exponent;
        }

        public static bool IsAccessModifier(TokenType tt)
        {
            switch (tt)
            {
                case TokenType.PublicKeyword:
                    return true;
                case TokenType.PrivateKeyword:
                    return true;
                case TokenType.ProtectedKeyword:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsTypeKeyword(TokenType tt)
        {
            switch (tt)
            {
                case TokenType.BooleanKeyword:
                    return true;
                case TokenType.ByteKeyword:
                    return true;
                case TokenType.ShortKeyword:
                    return true;
                case TokenType.Int16Keyword:
                    return true;
                case TokenType.IntKeyword:
                    return true;
                case TokenType.Int64Keyword:
                    return true;
                case TokenType.LongKeyword:
                    return true;
                case TokenType.CharKeyword:
                    return true;
                case TokenType.WCharKeyword:
                    return true;
                case TokenType.FloatKeyword:
                    return true;
                case TokenType.DoubleKeyword:
                    return true;
                case TokenType.VoidKeyword:
                    return true;
                case TokenType.UnsignedKeyword:
                    return true;
                case TokenType.SignedKeyword:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNumber(TokenType tt)
        {
            return tt == TokenType.Number || tt == TokenType.HexNumber;
        }

        public static bool IsCharacter(TokenType tt)
        {
            return tt == TokenType.CharacterAnsi || tt == TokenType.CharacterUnicode;
        }

        public static bool IsQuotedString(TokenType tt)
        {
            return tt == TokenType.QuotedStringAnsi || tt == TokenType.QuotedStringUnicode;
        }

        /// <summary>
        /// Is this a type of binary operation.  For exampl +,-,/, etc ...
        /// </summary>
        /// <param name="tt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsBinaryOperation(TokenType tt)
        {
            bool isvalid = false;
            switch (tt)
            {
                case TokenType.Ampersand:
                    isvalid = true;
                    break;
                case TokenType.Pipe:
                    isvalid = true;
                    break;
                case TokenType.Asterisk:
                    isvalid = true;
                    break;
                case TokenType.OpBoolAnd:
                    isvalid = true;
                    break;
                case TokenType.OpBoolOr:
                    isvalid = true;
                    break;
                case TokenType.OpDivide:
                    isvalid = true;
                    break;
                case TokenType.OpEquals:
                    isvalid = true;
                    break;
                case TokenType.OpNotEquals:
                    isvalid = true;
                    break;
                case TokenType.OpAssign:
                    isvalid = true;
                    break;
                case TokenType.OpGreaterThan:
                    isvalid = true;
                    break;
                case TokenType.OpLessThan:
                    isvalid = true;
                    break;
                case TokenType.OpGreaterThanOrEqual:
                    isvalid = true;
                    break;
                case TokenType.OpLessThanOrEqual:
                    isvalid = true;
                    break;
                case TokenType.OpMinus:
                    isvalid = true;
                    break;
                case TokenType.OpModulus:
                    isvalid = true;
                    break;
                case TokenType.OpPlus:
                    isvalid = true;
                    break;
                case TokenType.OpShiftLeft:
                    isvalid = true;
                    break;
                case TokenType.OpShiftRight:
                    isvalid = true;
                    break;
            }

            return isvalid;
        }

        /// <summary>
        /// Is this a preprocessor token
        /// </summary>
        /// <param name="tt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsPreprocessorToken(TokenType tt)
        {
            bool isValid = false;
            switch (tt)
            {
                case TokenType.PoundDefine:
                    isValid = true;
                    break;
                case TokenType.PoundElse:
                    isValid = true;
                    break;
                case TokenType.PoundElseIf:
                    isValid = true;
                    break;
                case TokenType.PoundEndIf:
                    isValid = true;
                    break;
                case TokenType.PoundError:
                    isValid = true;
                    break;
                case TokenType.PoundIf:
                    isValid = true;
                    break;
                case TokenType.PoundIfndef:
                    isValid = true;
                    break;
                case TokenType.PoundInclude:
                    isValid = true;
                    break;
                case TokenType.PoundPragma:
                    isValid = true;
                    break;
                case TokenType.PoundUnDef:
                    isValid = true;
                    break;
            }

            return isValid;
        }

        public static string ConvertToString(Token token)
        {
            string sValue = null;
            if (!TryConvertToString(token, out sValue))
            {
                throw new InvalidOperationException("Unable to convert to string: " + token.Value);
            }

            return sValue;
        }

        public static bool TryConvertToString(Token token, out string str)
        {
            str = null;
            if (!token.IsQuotedString)
            {
                return false;
            }

            str = token.Value;
            if (token.TokenType == TokenType.QuotedStringUnicode)
            {
                ThrowIfFalse(str[0] == 'L');
                str = str.Substring(1);
            }

            if (str.Length < 2 || str[0] != '\"' || str[str.Length - 1] != '\"')
            {
                return false;
            }

            str = str.Substring(1, str.Length - 2);
            return true;
        }

        public static bool TryConvertToChar(Token token, out char retChar)
        {
            retChar = ' ';

            if (!token.IsCharacter)
            {
                return false;
            }
            string val = token.Value;

            // Strip out the L
            if (token.TokenType == TokenType.CharacterUnicode)
            {
                val = val.Substring(1);
            }

            if (string.IsNullOrEmpty(val) || val.Length < 3 || val[0] != '\'' || val[val.Length - 1] != '\'')
            {
                return false;
            }

            val = val.Substring(1, val.Length - 2);
            // Strip the quotes
            if (val[0] != '\\')
            {
                return char.TryParse(val, out retChar);
            }

            // Look for the simple escape codes
            bool found = false;
            if (val.Length == 2)
            {
                found = true;
                switch (val[1])
                {
                    case '\\':
                        retChar = '\\';
                        break;
                    case '\'':
                        retChar = '\'';
                        break;
                    case '"':
                        retChar = '"';
                        break;
                    case '?':
                        retChar = '?';
                        break;
                    case '0':
                        retChar = '\0';
                        break;
                    case 'a':
                        retChar = Convert.ToChar(7);
                        break;
                    case 'b':
                        retChar = '\b';
                        break;
                    case 'f':
                        retChar = '\f';
                        break;
                    case 'n':
                        retChar = Convert.ToChar(10);
                        break;
                    case 'r':
                        retChar = '\r';
                        break;
                    case 't':
                        retChar = '\t';
                        break;
                    case 'v':
                        retChar = '\v';
                        break;
                    default:
                        found = false;
                        break;
                }
            }

            if (found)
            {
                return true;
            }

            // It's an escape sequence
            val = val.Substring(1);
            if (string.IsNullOrEmpty(val))
            {
                return false;
            }

            Number number;
            if (char.ToLower(val[0]) == 'x')
            {
                if (!TryConvertToNumber("0x" + val.Substring(1), out number))
                {
                    return false;
                }
            }
            else if (char.ToLower(val[0]) == 'u')
            {
                if (!TryConvertToNumber(val.Substring(1), out number))
                {
                    return false;
                }
            }
            else
            {
                if (!TryConvertToNumber(val, out number))
                {
                    return false;
                }
            }

            try
            {
                // TODO: Possible make this a function of Number
                retChar = Convert.ToChar(Convert.ToInt32(number.Value));
                return true;
            }
            catch (Exception)
            {
                Debug.Fail("Error converting to integer");
                return false;
            }
        }


        /// <summary>
        /// Try and convert the token into a number
        /// </summary>
        /// <param name="t"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool TryConvertToNumber(Token t, out Number number)
        {
            return TryConvertToNumber(t.Value, out number);
        }

        /// <summary>
        /// Try convert the value to a number
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool TryConvertToNumber(string str, out Number number)
        {
            number = default(Number);

            NumberInfo info = default(NumberInfo);
            if (!ProcessNumberInfo(ref str, ref info))
            {
                return false;
            }

            // If this is an octal value then we need to convert the octal value into an int32 value and get 
            // the string back as a base 10 number
            if (info.IsOctal)
            {
                string base10Value = string.Empty;
                if (!TryParseOctalNumber(str, info.IsUnsigned, ref base10Value))
                {
                    return false;
                }

                str = base10Value;
            }

            bool ret = true;
            if (info.IsFloatingPoint)
            {
                // Mulitiplier is only valid for floating point numbers
                long mult = 1;
                if (info.Exponent != 0)
                {
                    mult = Convert.ToInt64(Math.Pow(10, info.Exponent));
                }

                float floatVal = 0;
                double doubleVal = 0;
                if (float.TryParse(str, info.Style, CultureInfo.CurrentCulture, out floatVal))
                {
                    number = new Number(Convert.ToSingle(floatVal * mult));
                }
                else if (double.TryParse(str, info.Style, CultureInfo.CurrentCulture, out doubleVal))
                {
                    number = new Number(Convert.ToDouble(doubleVal * mult));
                }
                else
                {
                    ret = false;
                }
            }
            else if (info.IsUnsigned)
            {
                uint uint32Value = 0;
                ulong uint64Value = 0;
                if (!info.IsForced64 && UInt32.TryParse(str, info.Style, CultureInfo.CurrentCulture, out uint32Value))
                {
                    number = uint32Value <= int.MaxValue
                        ? new Number((int)uint32Value)
                        : new Number(uint32Value);
                }
                else if (UInt64.TryParse(str, info.Style, CultureInfo.CurrentCulture, out uint64Value))
                {
                    number = uint64Value <= long.MaxValue
                        ? new Number((long)uint64Value)
                        : new Number(uint64Value);
                }
                else
                {
                    ret = false;
                }
            }
            else
            {
                int int32Value = 0;
                long int64Value = 0;
                if (!info.IsForced64 && Int32.TryParse(str, info.Style, CultureInfo.CurrentCulture, out int32Value))
                {
                    number = new Number(int32Value);
                }
                else if (Int64.TryParse(str, info.Style, CultureInfo.CurrentCulture, out int64Value))
                {
                    number = new Number(int64Value);
                }
                else
                {
                    ret = false;
                }
            }

            return ret;
        }

        private static bool ProcessNumberInfo(ref string str, ref NumberInfo info)
        {
            // Get the hex out of the number
            if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                str = str.Substring(2);
                info.Style = NumberStyles.HexNumber;
            }
            else
            {
                info.Style = NumberStyles.Number;
            }

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            // if the number ends with u?i64 then it is a 64 bit type.  Just process the i64 here
            // and the next loop will grab the U suffix
            if (str.Length > 3 && str.EndsWith("i64", StringComparison.OrdinalIgnoreCase))
            {
                info.IsForced64 = true;
                str = str.Substring(0, str.Length - 3);
            }


            // If it ends with an LUF then we need to process that suffix 
            do
            {
                char last = char.ToLower(str[str.Length - 1]);
                if (last == 'u')
                {
                    info.IsUnsigned = true;
                }
                else if (last == 'f' && info.Style != NumberStyles.HexNumber)
                {
                    // F is a valid number value in a hex number
                    info.IsFloatingPoint = true;
                }
                else if (last == 'l')
                {
                    info.IsLong = true;
                }
                else
                {
                    break;
                }

                str = str.Substring(0, str.Length - 1);
                if (string.IsNullOrEmpty(str))
                {
                    break;
                }
            } while (true);

            // Exponent is 0 unless there is an exponent.  Can't have an exponent with
            // a hex number
            if (info.Style != NumberStyles.HexNumber)
            {
                for (int i = 0; i <= str.Length - 1; i++)
                {
                    char cur = char.ToLower(str[i]);
                    if (cur == 'e')
                    {
                        if (!Int32.TryParse(str.Substring(i + 1), out info.Exponent))
                        {
                            return false;
                        }

                        info.IsFloatingPoint = true;
                        str = str.Substring(0, i);
                        break;
                    }
                    else if (cur == '.')
                    {
                        info.IsFloatingPoint = true;
                    }
                }

                // Check for octal
                if (str.Length > 0 && '0' == str[0] && !info.IsFloatingPoint)
                {
                    info.IsOctal = true;
                }
            }


            return true;
        }

        public static bool TryParseOctalNumber(string number, bool isUnsigned, ref string base10Value)
        {
            if (string.IsNullOrEmpty(number))
            {
                base10Value = "0";
                return true;
            }

            int exponent = 0;
            int index = number.Length - 1;
            UInt64 unsignedValue = 0;
            UInt64 signedValue = 0;
            while (index >= 0)
            {
                int mult = Convert.ToInt32(Math.Pow(8, exponent));
                int digit = 0;
                if (!Int32.TryParse(number[index].ToString(), out digit))
                {
                    return false;
                }

                if (isUnsigned)
                {
                    unsignedValue += Convert.ToUInt64(digit * mult);
                }
                else
                {
                    signedValue += Convert.ToUInt64(digit * mult);
                }

                index -= 1;
                exponent += 1;
            }

            if (isUnsigned)
            {
                base10Value = unsignedValue.ToString();
            }
            else
            {
                base10Value = signedValue.ToString();
            }

            return true;
        }


        public static bool TryConvertToPoundToken(string word, out Token token)
        {

            token = null;
            switch (word.ToLower())
            {
                case "define":
                    token = new Token(TokenType.PoundDefine, "define");
                    break;
                case "include":
                    token = new Token(TokenType.PoundInclude, "include");
                    break;
                case "pragma":
                    token = new Token(TokenType.PoundPragma, "pragma");
                    break;
                case "if":
                case "ifdef":
                    token = new Token(TokenType.PoundIf, "if");
                    break;
                case "ifndef":
                    token = new Token(TokenType.PoundIfndef, "ifndef");
                    break;
                case "else":
                    token = new Token(TokenType.PoundElse, "else");
                    break;
                case "elseif":
                case "elif":
                    token = new Token(TokenType.PoundElseIf, "elseif");
                    break;
                case "endif":
                    token = new Token(TokenType.PoundEndIf, "endif");
                    break;
                case "undefine":
                case "undef":
                    token = new Token(TokenType.PoundUnDef, "undef");
                    break;
                case "error":
                    token = new Token(TokenType.PoundError, "error");
                    break;
            }

            return token != null;
        }

        public static string TokenListToString(IEnumerable<Token> enumerable)
        {
            var builder = new StringBuilder();
            foreach (Token cur in enumerable)
            {
                builder.Append(cur.Value);
            }

            return builder.ToString();
        }
    }

    #endregion

}
