// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static PInvoke.Contract;

namespace PInvoke.Parser
{

    /// <summary>
    /// Options for the Scanner
    /// </summary>
    /// <remarks></remarks>
    public class ScannerOptions
    {
        public bool ThrowOnEndOfStream;
        public bool HideComments;
        public bool HideWhitespace;

        public bool HideNewLines;

        public ScannerOptions()
        {
        }
    }

    /// <summary>
    /// Used to mark a point in the scanner to which a caller can move back to
    /// </summary>
    /// <remarks></remarks>
    public class ScannerMark
    {
        private int _index;
        private int _lineNumber;

        internal int Index
        {
            get { return _index; }
        }

        internal int LineNumber
        {
            get { return _lineNumber; }
        }

        internal ScannerMark(int index, int lineNumber)
        {
            _index = index;
            _lineNumber = lineNumber;
        }
    }


    /// <summary>
    /// Scans the Stream for tokens of interest
    /// </summary>
    /// <remarks></remarks>
    public class Scanner
    {

        #region "ScannerInternalException"
        private class ScannerInternalException : Exception
        {

            public ScannerInternalException(string msg) : base(msg)
            {
            }

            public ScannerInternalException(string msg, Exception inner) : base(msg, inner)
            {
            }
        }
        #endregion

        #region "ScannerBuffer"

        [DebuggerDisplay("{Display}")]
        private class ScannerBuffer
        {
            private string _text;
            private int _index;

            private int _lineNumber;
            /// <summary>
            /// Used as a debugger display property.  Gives a preview of the location in the stream
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public string Display
            {
                get
                {
                    int startIndex = _index - 10;
                    int endIndex = _index + 15;
                    if (startIndex <= 0)
                    {
                        startIndex = 0;
                    }

                    if (endIndex >= _text.Length)
                    {
                        endIndex = _text.Length - 1;
                    }

                    string value = _text.Substring(startIndex, _index - startIndex);
                    value += "->";
                    value += _text.Substring(_index, endIndex - _index);
                    return value;
                }
            }

            /// <summary>
            /// Line Number of the buffer
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public int LineNumber
            {
                get { return _lineNumber; }
            }

            /// <summary>
            /// True when we reach the end of the stream
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public bool EndOfStream
            {
                get { return _index >= _text.Length; }
            }

            public ScannerBuffer(TextReader reader)
            {
                _text = reader.ReadToEnd();
                _index = 0;
                _lineNumber = 1;
            }

            /// <summary>
            /// Mark the place in the stream so we can rollback to that spot
            /// </summary>
            /// <remarks></remarks>
            public ScannerMark Mark()
            {
                return new ScannerMark(_index, _lineNumber);
            }

            /// <summary>
            /// Rolling back unsets the mark
            /// </summary>
            /// <remarks></remarks>
            public void RollBack(ScannerMark mark)
            {
                Contract.ThrowIfNull(mark, "Must be passed a valid ScannerMark");
                _index = mark.Index;
                _lineNumber = mark.LineNumber;
            }

            /// <summary>
            /// Get a char from the stream
            /// </summary>
            /// <remarks></remarks>
            public char ReadChar()
            {
                EnsureNotEndOfStream();
                char ret = _text[_index];
                _index += 1;

                // Check for the end of the line
                if (ret == Convert.ToChar(PortConstants.CarriageReturn))
                {
                    if (_index == _text.Length)
                    {
                        _lineNumber += 1;
                    }
                    else if (PeekChar() == Convert.ToChar(PortConstants.LineFeed))
                    {
                        _lineNumber += 1;
                    }
                }

                return ret;
            }

            /// <summary>
            /// Peek the next char off of the stream
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            public char PeekChar()
            {
                EnsureNotEndOfStream();
                return _text[_index];
            }

            /// <summary>
            /// Moves past the next char in the stream.  Often used with
            /// PeekChar
            /// </summary>
            /// <remarks></remarks>
            public void EatChar()
            {
                EnsureNotEndOfStream();
                _index += 1;
            }

            public void MoveBack(int count)
            {
                _index -= count;
                if (_index < 0)
                {
                    throw new ScannerInternalException("Moved back before the start of the Stream");
                }
            }

            private void EnsureNotEndOfStream()
            {
                if (this.EndOfStream)
                {
                    throw new ScannerInternalException("EndOfStream encountered");
                }
            }
        }
        #endregion

        private ErrorProvider _errorProvider = new ErrorProvider();
        private ScannerOptions _options;

        private TextReaderBag _readerBag;
        /// <summary>
        /// Stream we are reading from
        /// </summary>
        /// <remarks></remarks>

        private ScannerBuffer _buffer;
        /// <summary>
        /// Options for the Scanner
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public ScannerOptions Options
        {
            get { return _options; }
        }

        /// <summary>
        /// What line number are we currently on
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int LineNumber
        {
            get { return _buffer.LineNumber; }
        }

        /// <summary>
        /// ErrorProvider for the instance
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public ErrorProvider ErrorProvider
        {
            get { return _errorProvider; }
            set { _errorProvider = value; }
        }

        /// <summary>
        /// Name of the file
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Name
        {
            get { return _readerBag.Name; }
        }

        /// <summary>
        /// Return whether or not we are at the end of the stream
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool EndOfStream
        {
            get
            {
                if (_options.ThrowOnEndOfStream)
                {
                    bool ret = false;
                    try
                    {
                        _options.ThrowOnEndOfStream = false;
                        ret = this.PeekNextToken().TokenType == TokenType.EndOfStream;
                    }
                    finally
                    {
                        _options.ThrowOnEndOfStream = true;
                    }
                    return ret;
                }
                else
                {
                    return this.PeekNextToken().TokenType == TokenType.EndOfStream;
                }
            }
        }

        public Scanner(TextReader reader) : this(new TextReaderBag(reader), new ScannerOptions())
        {
        }

        public Scanner(TextReaderBag bag, ScannerOptions options)
        {
            ThrowIfNull(bag);
            ThrowIfNull(options);
            _readerBag = bag;
            _buffer = new ScannerBuffer(bag.TextReader);
            _options = options;
        }



        /// <summary>
        /// Get the next token from the stream
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public Token GetNextToken()
        {

            Token ret = null;
            bool done = false;
            do
            {
                // Easy cases are out of the way.  Now we need to actually go ahead and 
                // parse out the next token.  Mark the stream so that if scanning fails
                // we can send back a token of the remainder of the line
                ScannerMark mark = _buffer.Mark();
                try
                {
                    ret = GetNextTokenImpl();
                }
                catch (ScannerInternalException ex)
                {
                    AddWarning(ex.Message);
                    _buffer.RollBack(mark);
                    ret = new Token(TokenType.Text, SafeReadTillEndOfLine());
                }

                done = true;
                // Done unless we find out otherwise
                switch (ret.TokenType)
                {
                    case TokenType.EndOfStream:
                        if (this.Options.ThrowOnEndOfStream)
                        {
                            throw new EndOfStreamException("Scanner reached the end of the stream");
                        }

                        break;
                    case TokenType.BlockComment:
                    case TokenType.LineComment:
                        if (this.Options.HideComments)
                        {
                            done = false;
                        }

                        break;
                    case TokenType.NewLine:
                        if (this.Options.HideNewLines)
                        {
                            done = false;
                        }

                        break;
                    case TokenType.WhiteSpace:
                        if (this.Options.HideWhitespace)
                        {
                            done = false;
                        }
                        break;
                }
            } while (!(done));

            return ret;
        }

        /// <summary>
        /// Peek the next token in the stream
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public Token PeekNextToken()
        {
            Token token = default(Token);
            ScannerMark mark = _buffer.Mark();
            try
            {
                token = GetNextToken();
            }
            finally
            {
                _buffer.RollBack(mark);
            }

            return token;
        }

        /// <summary>
        /// Peek a list of tokens from the stream.  Don't throw when doing an extended peek
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<Token> PeekTokenList(int count)
        {
            ScannerMark mark = this.Mark();
            bool oldThrow = _options.ThrowOnEndOfStream;
            _options.ThrowOnEndOfStream = false;
            try
            {
                List<Token> list = new List<Token>();
                for (int i = 0; i <= count - 1; i++)
                {
                    list.Add(this.GetNextToken());
                }

                return list;
            }
            finally
            {
                _options.ThrowOnEndOfStream = oldThrow;
                this.Rollback(mark);
            }
        }

        /// <summary>
        /// Get the next token that is not one of the specified types.  If EndOfStream
        /// is specified it will be ignored.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Token GetNextTokenNotOfType(params TokenType[] types)
        {
            Token token = GetNextToken();

            while (Array.IndexOf(types, token.TokenType) >= 0 && token.TokenType != TokenType.EndOfStream)
            {
                token = GetNextToken();
            }

            return token;
        }

        /// <summary>
        /// Peek the next token not of th specified type
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Token PeekNextTokenNotOfType(params TokenType[] types)
        {
            ScannerMark mark = _buffer.Mark();
            Token token = default(Token);
            try
            {
                token = GetNextTokenNotOfType(types);
            }
            finally
            {
                _buffer.RollBack(mark);
            }

            return token;
        }

        /// <summary>
        /// Get the next token and expect it to be of the specified type
        /// </summary>
        /// <param name="tt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Token GetNextToken(TokenType tt)
        {
            Token token = GetNextToken();
            if (token.TokenType != tt)
            {
                string msg = string.Format("Expected token of type {0} but found {1} instead.", tt, token.TokenType);
                throw new InvalidOperationException(msg);
            }

            return token;
        }

        /// <summary>
        /// Mark the point in the Scanner so we can jump back to it at a later
        /// time
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public ScannerMark Mark()
        {
            return _buffer.Mark();
        }

        /// <summary>
        /// Rollback to the specified mark
        /// </summary>
        /// <param name="mark"></param>
        /// <remarks></remarks>
        public void Rollback(ScannerMark mark)
        {
            _buffer.RollBack(mark);
        }

        /// <summary>
        /// Tokenize the remainder of the stream and return the result
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<Token> Tokenize()
        {
            List<Token> list = new List<Token>();
            while (!this.EndOfStream)
            {
                list.Add(this.GetNextToken());
            }

            return list;
        }

        /// <summary>
        /// Parse the next token out of the stream.  This does not consider any of the 
        /// Options and instead returns the next token period.  Callers must take 
        /// care to process the options correctly
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private Token GetNextTokenImpl()
        {

            // First check and see if we're at 'EndOfStream'.  
            if (_buffer.EndOfStream)
            {
                return new Token(TokenType.EndOfStream, string.Empty);
            }

            Token token = null;
            char c = _buffer.ReadChar();

            // First check for whitespace and return that

            if (char.IsWhiteSpace(c) && c != PortConstants.CarriageReturn && c != PortConstants.LineFeed)
            {
                _buffer.MoveBack(1);
                return ReadWhitespace();
            }

            // Use the first character to get the easy cases out of the way
            switch (c)
            {
                case '#':
                    token = ReadPoundToken();
                    break;
                case '{':
                    token = new Token(TokenType.BraceOpen, "{");
                    break;
                case '}':
                    token = new Token(TokenType.BraceClose, "}");
                    break;
                case '(':
                    token = new Token(TokenType.ParenOpen, "(");
                    break;
                case ')':
                    token = new Token(TokenType.ParenClose, ")");
                    break;
                case '[':
                    token = new Token(TokenType.BracketOpen, "[");
                    break;
                case ']':
                    token = new Token(TokenType.BracketClose, "]");
                    break;
                case ',':
                    token = new Token(TokenType.Comma, ",");
                    break;
                case '\\':
                    token = new Token(TokenType.BackSlash, "\\");

                    break;
                // Operators 
                case '+':
                    token = new Token(TokenType.OpPlus, "+");
                    break;
                case '-':
                    token = new Token(TokenType.OpMinus, "-");
                    break;
                case ';':
                    token = new Token(TokenType.Semicolon, ";");
                    break;
                case '*':
                    token = new Token(TokenType.Asterisk, "*");
                    break;
                case '.':
                    token = new Token(TokenType.Period, ".");
                    break;
                case ':':
                    token = new Token(TokenType.Colon, ":");
                    break;
                case '"':
                    token = ReadDoubleQuoteOrString();
                    break;
                case '\'':
                    token = ReadSingleQuoteOrCharacter();
                    break;
                case PortConstants.CarriageReturn:
                case PortConstants.LineFeed:
                    if (!_buffer.EndOfStream && _buffer.PeekChar() == PortConstants.LineFeed)
                    {
                        _buffer.EatChar();
                    }
                    token = new Token(TokenType.NewLine, PortConstants.NewLine);
                    break;
            }

            // If we found a token then return it
            if (token != null)
            {
                return token;
            }

            // We've gotten past the characters that can be determined by the first character.  Now 
            // we need to consider the second character as well.  Do an EndOfStream check here 
            // since there could just be a single character left in the Stream
            if (!_buffer.EndOfStream)
            {
                string c2 = _buffer.ReadChar().ToString();
                string both = c + c2;
                switch (both)
                {
                    case "//":
                        token = ReadLineComment();
                        break;
                    case "/*":
                        token = ReadBlockComment();
                        break;
                    case "&&":
                        token = new Token(TokenType.OpBoolAnd, "&&");
                        break;
                    case "||":
                        token = new Token(TokenType.OpBoolOr, "||");
                        break;
                    case "<=":
                        token = new Token(TokenType.OpLessThanOrEqual, "<=");
                        break;
                    case ">=":
                        token = new Token(TokenType.OpGreaterThanOrEqual, ">=");
                        break;
                    case "<<":
                        token = new Token(TokenType.OpShiftLeft, "<<");
                        break;
                    case ">>":
                        token = new Token(TokenType.OpShiftRight, ">>");
                        break;
                    case "==":
                        token = new Token(TokenType.OpEquals, "==");
                        break;
                    case "!=":
                        token = new Token(TokenType.OpNotEquals, "!=");
                        break;
                    case "L'":
                        token = ReadWideCharacterOrSingleL();
                        break;
                    case "L\"":
                        token = ReadWideStringOrSingleL();
                        break;
                }

                // If we found a token then return it
                if (token != null)
                {
                    return token;
                }

                // Move back the character since we didn't process it
                _buffer.MoveBack(1);
            }

            // There are several single character cases that are also a part of the double character 
            // case.  For ease of reading process those now as a simple select case
            switch (c)
            {
                case '/':
                    token = new Token(TokenType.OpDivide, "/");
                    break;
                case '|':
                    token = new Token(TokenType.Pipe, "|");
                    break;
                case '&':
                    token = new Token(TokenType.Ampersand, "&");
                    break;
                case '<':
                    token = new Token(TokenType.OpLessThan, "<");
                    break;
                case '>':
                    token = new Token(TokenType.OpGreaterThan, ">");
                    break;
                case '%':
                    token = new Token(TokenType.OpModulus, "%");
                    break;
                case '=':
                    token = new Token(TokenType.OpAssign, "=");
                    break;
                case '!':
                    token = new Token(TokenType.Bang, "!");
                    break;
            }

            // If we found a token then return it
            if (token != null)
            {
                return token;
            }

            // If this isn't a letter or digit then return this as junk
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                return new Token(TokenType.Text, Convert.ToString(c));
            }

            // This isn't a special token.  It's some type of word or number so move back to 
            // the start of this stream and read the word.
            _buffer.MoveBack(1);
            return ReadWordOrNumberToken();
        }

        /// <summary>
        /// Read the whitespace into a token
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private Token ReadWhitespace()
        {
            StringBuilder builder = new StringBuilder();
            bool done = false;
            do
            {
                char c = _buffer.ReadChar();
                if (!char.IsWhiteSpace(c))
                {
                    done = true;
                    _buffer.MoveBack(1);
                }
                else if (c == PortConstants.CarriageReturn || c == PortConstants.LineFeed)
                {
                    done = true;
                    _buffer.MoveBack(1);
                }
                else
                {
                    builder.Append(c);
                }
            } while (!(done || _buffer.EndOfStream));

            return new Token(TokenType.WhiteSpace, builder.ToString());
        }

        private Token ReadWordOrNumberToken()
        {
            string word = ReadWord();

            // First check and see if this is a keyword that we care about
            TokenType keywordType = default(TokenType);
            if (TokenHelper.KeywordMap.TryGetValue(word, out keywordType))
            {
                return new Token(keywordType, word);
            }

            TokenType numberType = TokenType.Ampersand;

            if (IsNumber(word, ref numberType))
            {
                // Loop for a floating point number literal
                if (!_buffer.EndOfStream && _buffer.PeekChar() == '.')
                {
                    ScannerMark mark = _buffer.Mark();

                    _buffer.ReadChar();
                    string fullWord = word + "." + ReadWord();
                    TokenType fullNumberType = TokenType.Ampersand;
                    if (IsNumber(fullWord, ref fullNumberType))
                    {
                        return new Token(fullNumberType, fullWord);
                    }
                    else
                    {
                        _buffer.RollBack(mark);
                    }
                }

                return new Token(numberType, word);
            }

            // Just a plain word
            return new Token(TokenType.Word, word);
        }

        private bool IsNumber(string word, ref TokenType tt)
        {
            // Now parse out pattern words
            if (Regex.IsMatch(word, "^[0-9.]+(e[0-9]+)?(([UFL]+)|(u?i64))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
            {
                tt = TokenType.Number;
                return true;
            }
            else if (Regex.IsMatch(word, "^0x[0-9a-f.]+(e[0-9]+)?(([UFL]+)|(u?i64))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
            {
                tt = TokenType.HexNumber;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads till the end of the line or stream.  Will not actuall consume the end
        /// of line token
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private string SafeReadTillEndOfLine()
        {
            StringBuilder builder = new StringBuilder();
            bool done = false;

            while (!done && !_buffer.EndOfStream)
            {
                char c = _buffer.PeekChar();
                if (c == PortConstants.CarriageReturn | c == PortConstants.LineFeed)
                {
                    done = true;
                }
                else
                {
                    builder.Append(c);
                    _buffer.ReadChar();
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Read a word from the stream
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private string ReadWord()
        {
            StringBuilder builder = new StringBuilder();
            bool done = false;

            while (!done && !_buffer.EndOfStream)
            {
                char c = _buffer.PeekChar();
                if (char.IsLetterOrDigit(c) || c == '_' || c == '$')
                {
                    builder.Append(c);
                    _buffer.EatChar();
                }
                else
                {
                    done = true;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// A # is already read, go ahead and read the text of the pound token
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private Token ReadPoundToken()
        {
            ScannerMark mark = this.Mark();
            string word = ReadWord();
            Token token = null;
            if (TokenHelper.TryConvertToPoundToken(word, out token))
            {
                return token;
            }

            // The word didn't match any of our pound tokens so just return the pound
            this.Rollback(mark);
            return new Token(TokenType.Pound, "#");
        }


        /// <summary>
        /// A '//' has already been read from the stream.  Read the rest of the comment
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private Token ReadLineComment()
        {
            string comment = SafeReadTillEndOfLine();
            return new Token(TokenType.LineComment, "//" + comment);
        }

        /// <summary>
        /// Read a block comment.  The '/*' has already been read
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private Token ReadBlockComment()
        {
            StringBuilder builder = new StringBuilder();
            bool done = false;
            builder.Append("/*");

            while (!done && !_buffer.EndOfStream)
            {
                char c = _buffer.ReadChar();
                if ((c == '*' && _buffer.PeekChar() == '/'))
                {
                    builder.Append("*/");
                    _buffer.EatChar();
                    // Eat the /
                    done = true;
                }
                else
                {
                    builder.Append(c);
                }
            }

            return new Token(TokenType.BlockComment, builder.ToString());
        }

        /// <summary>
        /// Read a quote or a string from the stream.  The initial quote has already been
        /// read
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private Token ReadDoubleQuoteOrString()
        {
            ScannerMark mark = _buffer.Mark();
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('"');

                bool done = false;
                while (!done)
                {
                    char c = _buffer.ReadChar();
                    switch (c)
                    {
                        case '"':
                            builder.Append('"');
                            done = true;
                            break;
                        case '\\':
                            builder.Append(c);
                            builder.Append(_buffer.ReadChar());
                            break;
                        default:
                            builder.Append(c);
                            break;
                    }
                }

                return new Token(TokenType.QuotedStringAnsi, builder.ToString());
            }
            catch (ScannerInternalException)
            {
                // If we get a scanner exception while trying to read the string then this
                // is just a simple quote.  Rollback the buffer and return the quote token
                _buffer.RollBack(mark);
                return new Token(TokenType.DoubleQuote, "\"");
            }
        }

        /// <summary>
        /// Read a single quote or a character.  The initial single quote has already been
        /// read
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private Token ReadSingleQuoteOrCharacter()
        {
            if (_buffer.EndOfStream)
            {
                return new Token(TokenType.SingleQuote, "'");
            }

            ScannerMark mark = _buffer.Mark();
            Token token = null;
            try
            {
                char data = _buffer.ReadChar();
                if (data != '\\')
                {
                    if (_buffer.ReadChar() == '\'')
                    {
                        token = new Token(TokenType.CharacterAnsi, '\'' + data.ToString() + '\'');
                    }
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(data);

                    do
                    {
                        data = _buffer.ReadChar();
                        if (data == '\'')
                        {
                            token = new Token(TokenType.CharacterAnsi, '\'' + builder.ToString() + '\'');
                            break; // TODO: might not be correct. Was : Exit Do
                        }
                        else if (_buffer.EndOfStream || builder.Length > 5)
                        {
                            break; // TODO: might not be correct. Was : Exit Do
                        }
                        else
                        {
                            builder.Append(data);
                        }
                    } while (true);
                }
            }
            catch (ScannerInternalException)
            {
                // Swallow the exception.  It will rollbakc when the token variable is not set
            }

            if (token == null)
            {
                _buffer.RollBack(mark);
                token = new Token(TokenType.SingleQuote, "'");
            }

            return token;
        }

        /// <summary>
        /// Called when we hit an L' in the stream.  The buffer is pointed after the text
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private Token ReadWideCharacterOrSingleL()
        {
            // If we read a single quote then there was no valid character after the L.  Rollback
            // the quote and return the word "L"
            Token token = ReadSingleQuoteOrCharacter();
            if (token.TokenType == TokenType.SingleQuote)
            {
                _buffer.MoveBack(1);
                return new Token(TokenType.Word, "L");
            }
            else
            {
                ThrowIfFalse(token.TokenType == TokenType.CharacterAnsi);
                return new Token(TokenType.CharacterUnicode, "L" + token.Value);
            }
        }

        /// <summary>
        /// Called when we hit an L" in the stream.  The buffer is pointed after the text 
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private Token ReadWideStringOrSingleL()
        {
            Token token = ReadDoubleQuoteOrString();
            if (token.TokenType == TokenType.DoubleQuote)
            {
                // Read a double quote which means there wasn't a valid string afterwards.  Move
                // back over the " and return the L
                _buffer.MoveBack(1);
                return new Token(TokenType.Word, "L");
            }
            else
            {
                ThrowIfFalse(token.TokenType == TokenType.QuotedStringAnsi);
                return new Token(TokenType.QuotedStringUnicode, "L" + token.Value);
            }
        }

        public static List<Token> TokenizeText(string text)
        {
            return TokenizeText(text, new ScannerOptions());
        }

        public static List<Token> TokenizeText(string text, ScannerOptions opts)
        {
            using (StringReader reader = new StringReader(text))
            {
                Scanner s = new Scanner(new TextReaderBag(reader), opts);
                return s.Tokenize();
            }
        }

        #region "Error Message Helpers"

        public void AddError(string msg)
        {
            _errorProvider.AddError(GetMessagePrefix() + msg);
        }

        public void AddError(string format, params object[] args)
        {
            AddError(string.Format(format, args));
        }

        public void AddWarning(string msg)
        {
            _errorProvider.AddWarning(GetMessagePrefix() + msg);
        }

        public void AddWarning(string format, params object[] args)
        {
            AddWarning(string.Format(format, args));
        }

        private string GetMessagePrefix()
        {
            return string.Format("{0} {1}: ", _readerBag.Name, _buffer.LineNumber);
        }

        #endregion

    }

}
