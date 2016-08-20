// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using PInvoke.Parser;
using Helper = PInvoke.Parser.TokenHelper;
using Xunit;

namespace PInvoke.Test
{

    ///<summary>
    ///This is a test class for PInvoke.Parser.TokenHelper and is intended
    ///to contain all PInvoke.Parser.TokenHelper Unit Tests
    ///</summary>
    public class TokenHelperTest
    {

        public void VerifyParse(string str, object value)
        {
            Number converted;
            Assert.True(Helper.TryConvertToNumber(str, out converted));
            Assert.Equal(value, converted.Value);
        }

        public void VerifyString(string str)
        {
            Token token = new Token(TokenType.QuotedStringAnsi, "\"" + str + "\"");
            string converted = null;
            Assert.True(Helper.TryConvertToString(token, out converted));
            Assert.Equal(str, converted);

            token = new Token(TokenType.QuotedStringUnicode, "L\"" + str + "\"");
            Assert.True(Helper.TryConvertToString(token, out converted));
            Assert.Equal(str, converted);
        }

        public void VerifyChar(char c)
        {
            Token token = new Token(TokenType.CharacterAnsi, "'" + c + "'");
            char converted = '0';
            Assert.True(Helper.TryConvertToChar(token, out converted));
            Assert.Equal(c, converted);

            token = new Token(TokenType.CharacterUnicode, "L'" + c + "'");
            Assert.True(Helper.TryConvertToChar(token, out converted));
            Assert.Equal(c, converted);
        }

        /// <summary>
        /// Simple 32 bit numbers
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Signed1()
        {
            Number val;
            Assert.True(Helper.TryConvertToNumber("42", out val));
            Assert.Equal(42, val.Integer);
            Assert.True(Helper.TryConvertToNumber("400", out val));
            Assert.Equal(400, val.Integer);
            Assert.True(Helper.TryConvertToNumber("-1", out val));
            Assert.Equal(-1, val.Integer);
        }

        /// <summary>
        /// Simple 64 bit number.  Should auto expand into a 64 bit number
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Signed2()
        {
            Int64 @base = 6000000000L;
            VerifyParse(@base.ToString(), @base);
            VerifyParse("-" + @base.ToString(), -@base);
        }

        /// <summary>
        /// Long suffix does not force a 64 bit number
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Long1()
        {
            VerifyParse("6L", 6);
            VerifyParse("26L", 26);
            VerifyParse("-26L", -26);
        }

        /// <summary>
        /// UL should not force a 64 bit number
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Long2()
        {
            VerifyParse("6UL", 6);
            VerifyParse("26UL", 26);
        }

        [Fact()]
        public void Float1()
        {
            Number val;
            Assert.True(Helper.TryConvertToNumber("6.5F", out val));
            Assert.Equal(6.5f, val.Single);
        }

        /// <summary>
        /// Make sure that a prefix floating point does not get confused as an octal
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Float2()
        {
            VerifyParse("0.1F", 0.1f);
            VerifyParse("0.2F", 0.2f);
        }


        [Fact()]
        public void Exponent1()
        {
            Number val;
            Assert.True(Helper.TryConvertToNumber("6e2", out val));
            Assert.Equal(600f, val.Single);
        }

        [Fact()]
        public void Exponent2()
        {
            Number val;
            Assert.True(Helper.TryConvertToNumber("6.5e2", out val));
            Assert.Equal(650f, val.Single);
        }

        [Fact()]
        public void Exponent3()
        {
            Number val;
            Assert.True(Helper.TryConvertToNumber("6.5e2L", out val));
            Assert.Equal(650f, val.Single);
        }

        /// <summary>
        /// Simple unsigned 32 bit numbers
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Unsigned1()
        {
            Number val;
            Assert.True(Helper.TryConvertToNumber("42U", out val));
            Assert.Equal(42, val.Integer);
        }

        /// <summary>
        /// Too big to be a UInt32
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Unsigned2()
        {
            Number val;
            Assert.True(Helper.TryConvertToNumber("6000000000U", out val));
            Assert.Equal(6000000000L, val.Long);
        }

        /// <summary>
        /// Can't convert a 32 negative to a signed number
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Unsigned3()
        {
            Number val;
            Assert.True(Helper.TryConvertToNumber("-42", out val));
            Assert.Equal(-42, val.Integer);
        }

        /// <summary>
        /// Simple octal numbers
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Octal1()
        {
            VerifyParse("01", 1);
            VerifyParse("012", 10);
            VerifyParse("0101", 65);
        }

        /// <summary>
        /// Octal numbers with the unsigned suffix
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Octal2()
        {
            VerifyParse("012U", 10);
            VerifyParse("01u", 1);
        }

        [Fact()]
        public void Invalid1()
        {
            Number val;
            Assert.False(Helper.TryConvertToNumber("aoo", out val));
        }

        [Fact()]
        public void CallingConvention()
        {
            Assert.True(Helper.IsCallTypeModifier(TokenType.CDeclarationCallKeyword));
            Assert.True(Helper.IsCallTypeModifier(TokenType.StandardCallKeyword));
            Assert.True(Helper.IsCallTypeModifier(TokenType.ClrCallKeyword));
            Assert.True(Helper.IsCallTypeModifier(TokenType.InlineKeyword));
            Assert.True(Helper.IsCallTypeModifier(TokenType.PascalCallKeyword));
        }

        [Fact()]
        public void String1()
        {
            VerifyString("foo");
            VerifyString("baaoeuaoeu\"");
            VerifyString("aoeu13AEuaoeu'");
        }

        [Fact()]
        public void Char1()
        {
            VerifyChar('a');
            VerifyChar('b');
            VerifyChar(ControlChars.Lf);
            VerifyChar(Strings.ChrW(55));
        }

        [Fact()]
        public void Force64_1()
        {
            Number val;
            Assert.True(TokenHelper.TryConvertToNumber("4i64", out val));
            Assert.Equal(4L, val.Long);
            Assert.True(TokenHelper.TryConvertToNumber("999999999999999999i64", out val));
            Assert.Equal(999999999999999999L, val.Long);
        }
    }
}
