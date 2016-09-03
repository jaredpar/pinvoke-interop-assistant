// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.IO;
using PInvoke.Parser;
using Xunit;

namespace PInvoke.Test
{
    public class ScannerTest
    {

        private ScannerOptions _defaultOpts;
        private ScannerOptions _lineOpts;
        private ScannerOptions _commentOpts;

        private ScannerOptions _rudeEndOpts;
        public ScannerTest()
        {
            _defaultOpts = new ScannerOptions();
            _defaultOpts.ThrowOnEndOfStream = false;
            _defaultOpts.HideComments = true;
            _defaultOpts.HideNewLines = true;
            _defaultOpts.HideWhitespace = true;

            _lineOpts = new ScannerOptions();
            _lineOpts.ThrowOnEndOfStream = false;
            _lineOpts.HideComments = true;
            _lineOpts.HideNewLines = false;
            _lineOpts.HideWhitespace = true;

            _commentOpts = new ScannerOptions();
            _commentOpts.HideComments = false;
            _commentOpts.ThrowOnEndOfStream = false;
            _commentOpts.HideNewLines = true;
            _commentOpts.HideWhitespace = true;

            _rudeEndOpts = new ScannerOptions();
            _rudeEndOpts.ThrowOnEndOfStream = true;
        }

        private void VerifyNext(Scanner scanner, TokenType tt)
        {
            Token token = scanner.GetNextToken();
            Assert.Equal(tt, token.TokenType);
        }

        private void VerifyNext(Scanner scanner, TokenType tt, string val)
        {
            Token token = scanner.GetNextToken();
            Assert.Equal(tt, token.TokenType);
            Assert.Equal(val, token.Value);
        }

        private void VerifyPeek(Scanner scanner, TokenType tt)
        {
            Token token = scanner.PeekNextToken();
            Assert.Equal(tt, token.TokenType);
        }

        private void VerifyPeek(Scanner scanner, TokenType tt, string val)
        {
            Token token = scanner.PeekNextToken();
            Assert.Equal(tt, token.TokenType);
            Assert.Equal(val, token.Value);
        }

        private Scanner CreateScanner(string data)
        {
            return new Scanner(new StringReader(data));
        }

        private Scanner CreateScanner(string data, ScannerOptions opts)
        {
            return new Scanner(new TextReaderBag(new StringReader(data)), opts);
        }

        [Fact()]
        public void BasicScan1()
        {
            Scanner scanner = CreateScanner("#define ", _defaultOpts);
            Token token = scanner.GetNextToken();
            Assert.Equal(TokenType.PoundDefine, token.TokenType);
        }

        [Fact()]
        public void BasicScan2()
        {
            Scanner scanner = CreateScanner("#if ", _defaultOpts);
            Token token = scanner.GetNextToken();
            Assert.Equal(TokenType.PoundIf, token.TokenType);
        }

        [Fact()]
        public void BasicScan3()
        {
            Scanner scanner = CreateScanner("{} ", _defaultOpts);
            VerifyNext(scanner, TokenType.BraceOpen);
            VerifyNext(scanner, TokenType.BraceClose);
        }

        [Fact()]
        public void BasicScan4()
        {
            Scanner scanner = CreateScanner("#define {", _defaultOpts);
            VerifyNext(scanner, TokenType.PoundDefine);
            VerifyNext(scanner, TokenType.BraceOpen);
        }

        [Fact()]
        public void BasicScan5()
        {
            Scanner scanner = CreateScanner("#define val {}", _defaultOpts);
            VerifyNext(scanner, TokenType.PoundDefine);
            VerifyNext(scanner, TokenType.Word, "val");
            VerifyNext(scanner, TokenType.BraceOpen);
            VerifyNext(scanner, TokenType.BraceClose);
        }

        [Fact()]
        public void BasicScan6()
        {
            Scanner scanner = CreateScanner("name[]");
            VerifyNext(scanner, TokenType.Word, "name");
            VerifyNext(scanner, TokenType.BracketOpen);
            VerifyNext(scanner, TokenType.BracketClose);
        }

        [Fact()]
        public void BasicScan7()
        {
            Scanner scanner = CreateScanner("foo[2]");
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.BracketOpen);
            VerifyNext(scanner, TokenType.Number, "2");
            VerifyNext(scanner, TokenType.BracketClose);
        }

        [Fact()]
        public void BasicScan8()
        {
            Scanner scanner = CreateScanner("typedef");
            VerifyNext(scanner, TokenType.TypeDefKeyword, "typedef");
        }

        [Fact()]
        public void BasicScan9()
        {
            Scanner scanner = CreateScanner("_hello");
            VerifyNext(scanner, TokenType.Word, "_hello");
        }

        [Fact()]
        public void BasicScan10()
        {
            Scanner scanner = CreateScanner("!foo!");
            VerifyNext(scanner, TokenType.Bang, "!");
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.Bang, "!");
        }

        [Fact()]
        public void BasicScan11()
        {
            Scanner scanner = CreateScanner("+-/%");
            VerifyNext(scanner, TokenType.OpPlus, "+");
            VerifyNext(scanner, TokenType.OpMinus, "-");
            VerifyNext(scanner, TokenType.OpDivide, "/");
            VerifyNext(scanner, TokenType.OpModulus, "%");
        }

        [Fact()]
        public void BasicScan12()
        {
            Scanner scanner = CreateScanner("<=>=");
            VerifyNext(scanner, TokenType.OpLessThanOrEqual, "<=");
            VerifyNext(scanner, TokenType.OpGreaterThanOrEqual, ">=");
        }

        [Fact()]
        public void BasicScan13()
        {
            Scanner scanner = CreateScanner("0x4 0x5 0xf");
            VerifyNext(scanner, TokenType.HexNumber, "0x4");
            VerifyNext(scanner, TokenType.WhiteSpace, " ");
            VerifyNext(scanner, TokenType.HexNumber, "0x5");
            VerifyNext(scanner, TokenType.WhiteSpace, " ");
            VerifyNext(scanner, TokenType.HexNumber, "0xf");
        }

        [Fact()]
        public void BasicScan14()
        {
            Scanner scanner = CreateScanner("__declspec(\"foo\")");
            VerifyNext(scanner, TokenType.DeclSpec);
            VerifyNext(scanner, TokenType.ParenOpen);
            VerifyNext(scanner, TokenType.QuotedStringAnsi);
            VerifyNext(scanner, TokenType.ParenClose);
        }

        [Fact()]
        public void BasicScan15()
        {
            Scanner scanner = CreateScanner("a>>1");
            VerifyNext(scanner, TokenType.Word);
            VerifyNext(scanner, TokenType.OpShiftRight);
            VerifyNext(scanner, TokenType.Number);
        }

        [Fact()]
        public void BasicScan16()
        {
            Scanner scanner = CreateScanner("a<<1");
            VerifyNext(scanner, TokenType.Word);
            VerifyNext(scanner, TokenType.OpShiftLeft);
            VerifyNext(scanner, TokenType.Number);
        }

        [Fact()]
        public void BasicScan17()
        {
            Scanner scanner = CreateScanner("$foo");
            VerifyNext(scanner, TokenType.Text);
            VerifyNext(scanner, TokenType.Word, "foo");
        }

        [Fact()]
        public void BasicScan18()
        {
            Scanner scanner = CreateScanner("__$foo");
            VerifyNext(scanner, TokenType.Word, "__$foo");
        }

        [Fact()]
        public void BasicScan19()
        {
            Scanner scanner = CreateScanner("__inline inline");
            VerifyNext(scanner, TokenType.InlineKeyword);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.InlineKeyword);
        }

        [Fact()]
        public void BasicScan20()
        {
            Scanner scanner = CreateScanner("volatile __clrcall 5");
            VerifyNext(scanner, TokenType.VolatileKeyword);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.ClrCallKeyword);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.Number, "5");
        }

        [Fact()]
        public void BasicScan21()
        {
            Scanner scanner = CreateScanner("__ptr32 __ptr64");
            VerifyNext(scanner, TokenType.Pointer32Keyword);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.Pointer64Keyword);
        }

        [Fact()]
        public void BasicScan22()
        {
            Scanner scanner = CreateScanner("-one");
            VerifyNext(scanner, TokenType.OpMinus, "-");
            VerifyNext(scanner, TokenType.Word, "one");
        }

        [Fact()]
        public void BasicScan23()
        {
            Scanner scanner = CreateScanner("-22one");
            VerifyNext(scanner, TokenType.OpMinus, "-");
            VerifyNext(scanner, TokenType.Word, "22one");
        }

        [Fact()]
        public void BasicScan24()
        {
            Scanner scanner = CreateScanner("'f' 'a'");
            VerifyNext(scanner, TokenType.CharacterAnsi, "'f'");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.CharacterAnsi, "'a'");
        }

        [Fact()]
        public void BasicScan25()
        {
            Scanner scanner = CreateScanner("'\\n' '\\10'");
            VerifyNext(scanner, TokenType.CharacterAnsi, "'\\n'");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.CharacterAnsi, "'\\10'");
        }

        [Fact()]
        public void BasicScan26()
        {
            Scanner scanner = CreateScanner("'\\foo");
            VerifyNext(scanner, TokenType.SingleQuote);
            VerifyNext(scanner, TokenType.BackSlash);
            VerifyNext(scanner, TokenType.Word, "foo");
        }

        [Fact()]
        public void BasicScan27()
        {
            Scanner scanner = CreateScanner("foo'");
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.SingleQuote);
        }

        [Fact()]
        public void BasicScan28()
        {
            Scanner scanner = CreateScanner("= ==");
            VerifyNext(scanner, TokenType.OpAssign, "=");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.OpEquals, "==");
        }

        [Fact()]
        public void BasicScan29()
        {
            Scanner scanner = CreateScanner("! !=");
            VerifyNext(scanner, TokenType.Bang, "!");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.OpNotEquals, "!=");
        }

        [Fact()]
        public void BasicScan30()
        {
            Scanner scanner = CreateScanner("_cdecl __cdecl __stdcall __pascal __winapi");
            VerifyNext(scanner, TokenType.CDeclarationCallKeyword);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.CDeclarationCallKeyword);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.StandardCallKeyword);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.PascalCallKeyword);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.WinApiCallKeyword);
        }

        [Fact()]
        public void BasciScan31()
        {
            Scanner scanner = CreateScanner("L'a' L1");
            VerifyNext(scanner, TokenType.CharacterUnicode, "L'a'");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.Word, "L1");
        }

        [Fact()]
        public void BasicScan32()
        {
            Scanner scanner = CreateScanner("L'    5");
            VerifyNext(scanner, TokenType.Word, "L");
            VerifyNext(scanner, TokenType.SingleQuote);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.Number, "5");
        }

        [Fact()]
        public void BasicScan33()
        {
            Scanner scanner = CreateScanner("public private protected class");
            VerifyNext(scanner, TokenType.PublicKeyword, "public");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.PrivateKeyword, "private");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.ProtectedKeyword, "protected");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.ClassKeyword, "class");
        }

        [Fact()]
        public void BasicScan34()
        {
            Scanner scanner = CreateScanner("signed unsigned");
            VerifyNext(scanner, TokenType.SignedKeyword, "signed");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.UnsignedKeyword, "unsigned");
        }

        [Fact()]
        public void BasicScan35()
        {
            Scanner scanner = CreateScanner("45i64 52ui64 -45i64");
            VerifyNext(scanner, TokenType.Number, "45i64");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.Number, "52ui64");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.OpMinus);
            VerifyNext(scanner, TokenType.Number, "45i64");
        }

        [Fact()]
        public void MultilineBasicScan1()
        {
            Scanner scanner = CreateScanner("foo" + PortConstants.NewLine + "bar", _lineOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.NewLine);
            VerifyNext(scanner, TokenType.Word, "bar");
        }

        [Fact()]
        public void MultilineBasicScan2()
        {
            Scanner scanner = CreateScanner("foo," + PortConstants.NewLine + "bar[]", _lineOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.Comma);
            VerifyNext(scanner, TokenType.NewLine);
            VerifyNext(scanner, TokenType.Word, "bar");
            VerifyNext(scanner, TokenType.BracketOpen);
            VerifyNext(scanner, TokenType.BracketClose);
        }

        [Fact()]
        public void MultilineBasicScan3()
        {
            Scanner scanner = CreateScanner("bar,   " + PortConstants.NewLine + "foo", _lineOpts);
            VerifyNext(scanner, TokenType.Word, "bar");
            VerifyNext(scanner, TokenType.Comma);
            VerifyNext(scanner, TokenType.NewLine);
            VerifyNext(scanner, TokenType.Word, "foo");
        }

        [Fact()]
        public void PeekToken1()
        {
            Scanner scanner = CreateScanner("bar");
            VerifyPeek(scanner, TokenType.Word, "bar");
            VerifyPeek(scanner, TokenType.Word, "bar");
            VerifyNext(scanner, TokenType.Word, "bar");
        }

        [Fact()]
        public void PeekToken2()
        {
            Scanner scanner = CreateScanner("bar[]");
            VerifyPeek(scanner, TokenType.Word, "bar");
            VerifyNext(scanner, TokenType.Word, "bar");
            VerifyPeek(scanner, TokenType.BracketOpen);
            VerifyNext(scanner, TokenType.BracketOpen);
            VerifyNext(scanner, TokenType.BracketClose);
        }

        [Fact()]
        public void MarkAndRollback1()
        {
            Scanner scanner = CreateScanner("bar()foo");
            VerifyNext(scanner, TokenType.Word, "bar");
            ScannerMark mark = scanner.Mark();
            VerifyNext(scanner, TokenType.ParenOpen);
            scanner.Rollback(mark);
            VerifyNext(scanner, TokenType.ParenOpen);
            VerifyNext(scanner, TokenType.ParenClose);
            VerifyNext(scanner, TokenType.Word, "foo");
        }

        [Fact()]
        public void MarkAndRollback2()
        {
            Scanner scanner = CreateScanner("bar[");
            ScannerMark mark = scanner.Mark();
            VerifyNext(scanner, TokenType.Word, "bar");
            VerifyNext(scanner, TokenType.BracketOpen);
            scanner.Rollback(mark);
            VerifyNext(scanner, TokenType.Word, "bar");
            VerifyNext(scanner, TokenType.BracketOpen);
        }

        [Fact()]
        public void QuotedString1()
        {
            Scanner scanner = CreateScanner("bar \"uu\"", _defaultOpts);
            VerifyNext(scanner, TokenType.Word, "bar");
            VerifyNext(scanner, TokenType.QuotedStringAnsi, "\"uu\"");
        }

        [Fact()]
        public void QuotedString2()
        {
            Scanner scanner = CreateScanner("\"a\"\"b\"", _defaultOpts);
            VerifyNext(scanner, TokenType.QuotedStringAnsi, "\"a\"");
            VerifyNext(scanner, TokenType.QuotedStringAnsi, "\"b\"");
        }

        [Fact()]
        public void QuotedString3()
        {
            Scanner scanner = CreateScanner("\"b\"hello", _defaultOpts);
            VerifyNext(scanner, TokenType.QuotedStringAnsi, "\"b\"");
            VerifyNext(scanner, TokenType.Word, "hello");
        }

        /// <summary>
        /// Test a string with a C++ escape sequence
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void QuotedString4()
        {
            Scanner scanner = CreateScanner("\"b\\n\"foo", _defaultOpts);
            VerifyNext(scanner, TokenType.QuotedStringAnsi, "\"b\\n\"");
            VerifyNext(scanner, TokenType.Word, "foo");
        }

        /// <summary>
        /// String with an escaped quote
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void QuotedString5()
        {
            Scanner scanner = CreateScanner("\"aaaa\\\"\"bar", _defaultOpts);
            VerifyNext(scanner, TokenType.QuotedStringAnsi, "\"aaaa\\\"\"");
            VerifyNext(scanner, TokenType.Word, "bar");
        }

        [Fact()]
        public void QuotedString6()
        {
            Scanner scanner = CreateScanner("L\"foo\" L\"");
            VerifyNext(scanner, TokenType.QuotedStringUnicode, "L\"foo\"");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.Word, "L");
            VerifyNext(scanner, TokenType.DoubleQuote);
        }

        [Fact()]
        public void LineComment1()
        {
            Scanner scanner = CreateScanner("hello // bar", _commentOpts);
            VerifyNext(scanner, TokenType.Word, "hello");
            VerifyNext(scanner, TokenType.LineComment, "// bar");
        }

        /// <summary>
        /// Read a line comment that nests line comment characters
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void LineComment2()
        {
            Scanner scanner = CreateScanner("foo // // bar", _commentOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.LineComment, "// // bar");
        }

        /// <summary>
        /// Line comment that embeds block comments which shouldn't count
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void LineComment3()
        {
            Scanner scanner = CreateScanner("foo // /* bar */", _commentOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.LineComment, "// /* bar */");
        }

        /// <summary>
        /// Line comment at the start of a line
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void LineComment4()
        {
            Scanner scanner = CreateScanner("// hello", _commentOpts);
            VerifyNext(scanner, TokenType.LineComment, "// hello");
        }

        /// <summary>
        /// Empty line comment
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void LineComment5()
        {
            Scanner scanner = CreateScanner("foo //", _commentOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.LineComment, "//");
        }

        [Fact()]
        public void LineComment6()
        {
            Scanner scanner = CreateScanner("foo //bar" + PortConstants.NewLine, new ScannerOptions());
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.WhiteSpace, " ");
            VerifyNext(scanner, TokenType.LineComment, "//bar");
            VerifyNext(scanner, TokenType.NewLine, PortConstants.NewLine);
        }

        [Fact()]
        public void BlockComment1()
        {
            Scanner scanner = CreateScanner("foo /* bar */", _commentOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.BlockComment, "/* bar */");
        }

        [Fact()]
        public void BlockComment2()
        {
            Scanner scanner = CreateScanner("foo /* bar */ a", _commentOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.BlockComment, "/* bar */");
            VerifyNext(scanner, TokenType.Word, "a");
        }

        /// <summary>
        /// Several block comments
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void BlockComment3()
        {
            Scanner scanner = CreateScanner("foo /* one */ /* two */", _commentOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.BlockComment, "/* one */");
            VerifyNext(scanner, TokenType.BlockComment, "/* two */");
        }

        /// <summary>
        /// Make sure that the block comment will process when unclosed
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void BlockComment4()
        {
            Scanner scanner = CreateScanner("/* foo", _commentOpts);
            VerifyNext(scanner, TokenType.BlockComment, "/* foo");
        }

        [Fact()]
        public void NextOfType()
        {
            Scanner scanner = CreateScanner("foo/* bar*/bar", _defaultOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.Word, "bar");
        }

        [Fact()]
        public void NextOfType2()
        {
            Scanner scanner = CreateScanner("/*bar*/foo", _defaultOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
        }

        [Fact()]
        public void NextOfType3()
        {
            Scanner scanner = CreateScanner("/*bar*/foo//hello", _defaultOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.EndOfStream, string.Empty);
        }

        [Fact()]
        public void PeekOfType()
        {
            Scanner scanner = CreateScanner("bar/*foo*/", _defaultOpts);
            VerifyPeek(scanner, TokenType.Word, "bar");
            VerifyNext(scanner, TokenType.Word, "bar");
            VerifyNext(scanner, TokenType.EndOfStream, string.Empty);
        }

        [Fact()]
        public void PeekOfType2()
        {
            Scanner scanner = CreateScanner("bar/*foo*/green", _defaultOpts);
            VerifyNext(scanner, TokenType.Word, "bar");
            VerifyPeek(scanner, TokenType.Word, "green");
            VerifyNext(scanner, TokenType.Word, "green");
        }

        [Fact()]
        public void GetNextRealToken()
        {
            Scanner scanner = CreateScanner("bar", _defaultOpts);
            scanner.GetNextToken(TokenType.Word);
        }

        [Fact()]
        public void GetNextRealToken2()
        {
            Scanner scanner = CreateScanner("bar", _defaultOpts);
            Assert.Throws<InvalidOperationException>(() => { scanner.GetNextToken(TokenType.Asterisk); });
        }

        [Fact()]
        public void NumberTest1()
        {
            Scanner scanner = CreateScanner("foo2");
            VerifyNext(scanner, TokenType.Word, "foo2");
        }

        [Fact()]
        public void NumberTest2()
        {
            Scanner scanner = CreateScanner("foo 22", _defaultOpts);
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.Number, "22");
        }

        [Fact()]
        public void NumberTest3()
        {
            Scanner scanner = CreateScanner("22foo 3.3", _defaultOpts);
            VerifyNext(scanner, TokenType.Word, "22foo");
            VerifyNext(scanner, TokenType.Number, "3.3");
        }

        [Fact()]
        public void NumberTest4()
        {
            Scanner scanner = CreateScanner("22L", _defaultOpts);
            VerifyNext(scanner, TokenType.Number, "22L");
        }

        [Fact()]
        public void NumberTest5()
        {
            Scanner scanner = CreateScanner("12u", _defaultOpts);
            VerifyNext(scanner, TokenType.Number, "12u");
        }

        [Fact()]
        public void NumberTest6()
        {
            Scanner scanner = CreateScanner("12U", _defaultOpts);
            VerifyNext(scanner, TokenType.Number, "12U");
        }

        [Fact()]
        public void NumberTest7()
        {
            Scanner scanner = CreateScanner("12UL", _defaultOpts);
            VerifyNext(scanner, TokenType.Number, "12UL");
        }

        /// <summary>
        /// Test the U option out
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void NumberTest8()
        {
            Scanner scanner = CreateScanner("12U 0x5U 6u", _defaultOpts);
            VerifyNext(scanner, TokenType.Number, "12U");
            VerifyNext(scanner, TokenType.HexNumber, "0x5U");
            VerifyNext(scanner, TokenType.Number, "6u");
        }

        /// <summary>
        /// Play with the L option
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void NumberTest9()
        {
            Scanner scanner = CreateScanner("12L 0x5L 6l", _defaultOpts);
            VerifyNext(scanner, TokenType.Number, "12L");
            VerifyNext(scanner, TokenType.HexNumber, "0x5L");
            VerifyNext(scanner, TokenType.Number, "6l");
        }

        /// <summary>
        /// Play with the F option
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void NumberTest10()
        {
            Scanner scanner = CreateScanner("12F 0x5F 6f", _defaultOpts);
            VerifyNext(scanner, TokenType.Number, "12F");
            VerifyNext(scanner, TokenType.HexNumber, "0x5F");
            VerifyNext(scanner, TokenType.Number, "6f");
        }

        /// <summary>
        /// Play with the exponnentials
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void NumberTest11()
        {
            Scanner scanner = CreateScanner("1e2 0x5e5 7e2", _defaultOpts);
            VerifyNext(scanner, TokenType.Number, "1e2");
            VerifyNext(scanner, TokenType.HexNumber, "0x5e5");
            VerifyNext(scanner, TokenType.Number, "7e2");
        }

        [Fact()]
        public void NumberTest12()
        {
            Scanner scanner = CreateScanner("0x15");
            VerifyNext(scanner, TokenType.HexNumber, "0x15");
        }

        [Fact()]
        public void NumberTest13()
        {
            Scanner scanner = CreateScanner("1.0 1.a");
            VerifyNext(scanner, TokenType.Number, "1.0");
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.Number, "1");
            VerifyNext(scanner, TokenType.Period);
            VerifyNext(scanner, TokenType.Word, "a");
        }

        [Fact()]
        public void NumberTestt14()
        {
            Scanner scanner = CreateScanner("-0.1F");
            VerifyNext(scanner, TokenType.OpMinus);
            VerifyNext(scanner, TokenType.Number, "0.1F");
        }

        [Fact()]
        public void BooleanTest1()
        {
            Scanner scanner = CreateScanner("true false");
            VerifyNext(scanner, TokenType.TrueKeyword);
            VerifyNext(scanner, TokenType.WhiteSpace);
            VerifyNext(scanner, TokenType.FalseKeyword);
        }


        /// <summary>
        /// Make sure it returns Whitespace
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void AllTest1()
        {
            Scanner scanner = CreateScanner("foo 22");
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.WhiteSpace, " ");
            VerifyNext(scanner, TokenType.Number, "22");
        }

        /// <summary>
        /// Make sure it returns all of teh whitespace
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void AllTest2()
        {
            Scanner scanner = CreateScanner("foo  22");
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.WhiteSpace, "  ");
            VerifyNext(scanner, TokenType.Number, "22");
        }

        /// <summary>
        /// More all whitespace tests
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void AllTest3()
        {
            Scanner scanner = CreateScanner("foo  2. 2");
            VerifyNext(scanner, TokenType.Word, "foo");
            VerifyNext(scanner, TokenType.WhiteSpace, "  ");
            VerifyNext(scanner, TokenType.Number, "2.");
            VerifyNext(scanner, TokenType.WhiteSpace, " ");
            VerifyNext(scanner, TokenType.Number, "2");
        }

        [Fact()]
        public void RudeEnd1()
        {
            Scanner scanner = CreateScanner("foo", _rudeEndOpts);
            scanner.GetNextToken();
            Assert.Throws<EndOfStreamException>(() => scanner.GetNextToken());
        }

        [Fact()]
        public void RudeEnd2()
        {
            Scanner scanner = CreateScanner("foo bar", _rudeEndOpts);
            scanner.GetNextToken();
            scanner.GetNextToken();
            scanner.GetNextToken();
            Assert.Throws<EndOfStreamException>(() => scanner.GetNextToken());
        }
    }
}