// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using static PInvoke.Contract;

namespace PInvoke.Parser
{

    #region "PreProcessorOptions"

    /// <summary>
    /// Options for the preprocessor
    /// </summary>
    /// <remarks></remarks>
    public class PreProcessorOptions
    {
        private List<Macro> _macroList = new List<Macro>();
        private bool _followIncludes;
        private List<string> _includePathList = new List<string>();

        private bool _trace;
        /// <summary>
        /// Options to start the preprocessor with
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<Macro> InitialMacroList
        {
            get { return _macroList; }
        }

        /// <summary>
        /// Whether or not the pre-processor should follow #include's
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool FollowIncludes
        {
            get { return _followIncludes; }
            set { _followIncludes = value; }
        }

        /// <summary>
        /// List of paths to search for header file that is included
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> IncludePathList
        {
            get { return _includePathList; }
        }

        /// <summary>
        /// When true, the preprocessor will output comments detailing the conditional evalution into
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        /// </summary>
        public bool Trace
        {
            get { return _trace; }
            set { _trace = value; }
        }

        public PreProcessorOptions()
        {
        }

    }

    #endregion

    /// <summary>
    /// Runs the preprocessor on a stream of data and returns the result without the macros
    /// or preprocessor junk
    /// </summary>
    /// <remarks></remarks>
    public partial class PreProcessorEngine
    {
        [DebuggerDisplay("{DisplayLine}")]
        private class PreprocessorLine
        {
            public List<Token> TokenList;
            public Token FirstValidToken;

            public bool IsPreProcessorDirectiveLine;
            /// <summary>
            /// Useful for debugging 
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public string DisplayLine
            {
                get
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (Token token in this.GetValidTokens())
                    {
                        builder.AppendFormat("[{0}] ", token.Value);
                    }

                    return builder.ToString();
                }
            }

            public List<Token> GetValidTokens()
            {
                List<Token> list = new List<Token>();
                foreach (Token token in TokenList)
                {
                    switch (token.TokenType)
                    {
                        case TokenType.WhiteSpace:
                        case TokenType.NewLine:
                        case TokenType.EndOfStream:
                            break;
                        // Don't add these types
                        default:
                            list.Add(token);
                            break;
                    }
                }

                return list;
            }

            public override string ToString()
            {
                var b = new StringBuilder();
                foreach (Token cur in TokenList)
                {
                    b.Append(cur.Value);
                }

                return b.ToString();
            }
        }

        private class PreProcessorException : Exception
        {

            private bool _isError = true;
            internal bool IsError
            {
                get { return _isError; }
                set { _isError = value; }
            }

            public PreProcessorException(string msg) : base(msg)
            {
            }

            public PreProcessorException(string msg, bool isError) : base(msg)
            {
                _isError = isError;
            }

            public PreProcessorException(string msg, Exception inner) : base(msg, inner)
            {
            }

        }

        private readonly PreProcessorOptions _options;
        private readonly Dictionary<string, Macro> _macroMap = new Dictionary<string, Macro>();
        private bool _processing;
        private Scanner _scanner;
        private TextWriter _outputStream;
        private ErrorProvider _errorProvider = new ErrorProvider();
        private ExpressionEvaluator _eval;
        private Dictionary<string, string> _metadataMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Options of the NativePreProcessor
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public PreProcessorOptions Options
        {
            get { return _options; }
        }

        /// <summary>
        /// List of macros encountered by the NativePreProcessor
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public Dictionary<string, Macro> MacroMap
        {
            get { return _macroMap; }
        }

        public ErrorProvider ErrorProvider
        {
            get { return _errorProvider; }
            set { _errorProvider = value; }
        }

        public PreProcessorEngine(PreProcessorOptions options)
        {
            _eval = new ExpressionEvaluator(_macroMap);
            _options = options;
        }

        /// <summary>
        /// Process the given stream and return the result of removing the 
        /// preprocessor definitions
        /// </summary>
        /// <param name="readerBag"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Process(TextReaderBag readerBag)
        {
            ThrowIfTrue(_processing, "Recursive parsing not supported in this manner.");

            StringBuilder builder = new StringBuilder();
            try
            {
                // Setup the macro map
                _macroMap.Clear();
                foreach (Macro m in _options.InitialMacroList)
                {
                    _macroMap[m.Name] = m;
                }

                _outputStream = new StringWriter(builder);
                using (_outputStream)
                {
                    _processing = true;
                    ProcessCore(readerBag);

                    if (_options.Trace)
                    {
                        TraceMacroMap();
                    }
                }
            }
            finally
            {
                _processing = false;
                _outputStream = null;
            }

            return builder.ToString();

        }

        /// <summary>
        /// Called to process a particular stream of text.  Can be called recursively
        /// </summary>
        /// <param name="readerBag"></param>
        /// <remarks></remarks>
        private void ProcessCore(TextReaderBag readerBag)
        {
            ThrowIfFalse(_processing);
            Scanner oldScanner = _scanner;
            try
            {
                // Create the scanner
                _scanner = new Scanner(readerBag, CreateScannerOptions());
                _scanner.ErrorProvider = this.ErrorProvider;

                ProcessLoop();

            }
            finally
            {
                _scanner = oldScanner;
            }

        }

        private ScannerOptions CreateScannerOptions()
        {
            ScannerOptions opts = new ScannerOptions();
            opts.HideComments = true;
            opts.HideNewLines = false;
            opts.HideWhitespace = false;
            opts.ThrowOnEndOfStream = false;
            return opts;
        }

        /// <summary>
        /// Core processing loop.  Processes blocks of text.
        /// </summary>
        /// <remarks></remarks>

        private void ProcessLoop()
        {
            bool done = false;

            while (!done)
            {
                ScannerMark mark = _scanner.Mark();

                try
                {
                    PreprocessorLine line = this.GetNextLine();
                    ThrowIfFalse(line.TokenList.Count > 0);

                    Token token = line.FirstValidToken;
                    if (token == null)
                    {
                        WriteToStream(line);
                        continue;
                    }

                    switch (token.TokenType)
                    {
                        case TokenType.PoundIf:
                            ProcessPoundIf(line);
                            break;
                        case TokenType.PoundIfndef:
                            ProcessPoundIfndef(line);
                            break;
                        case TokenType.PoundElse:
                        case TokenType.PoundElseIf:
                            // stop on a conditional branch end
                            ChewThroughConditionalEnd();
                            done = true;
                            break;
                        case TokenType.EndOfStream:
                        case TokenType.PoundEndIf:
                            done = true;
                            break;
                        case TokenType.PoundPragma:
                            ProcessPoundPragma(line);
                            break;
                        case TokenType.PoundDefine:
                            ProcessPoundDefine(line);
                            break;
                        case TokenType.PoundUnDef:
                            ProcessPoundUndefine(line);
                            break;
                        case TokenType.PoundInclude:
                            ProcessPoundInclude(line);
                            break;
                        default:
                            WriteToStream(line);
                            break;
                    }

                }
                catch (PreProcessorException ex)
                {
                    if (ex.IsError)
                    {
                        _errorProvider.AddError(ex.Message);
                    }
                    else
                    {
                        _errorProvider.AddWarning(ex.Message);
                    }
                    _scanner.Rollback(mark);
                    GetNextLine();
                    // Chew through the line
                }
            }
        }

        /// <summary>
        /// Called when a define token is hit
        /// </summary>
        /// <remarks></remarks>

        private void ProcessPoundDefine(PreprocessorLine line)
        {
            // Get the non whitespace tokens
            List<Token> list = line.GetValidTokens();
            ThrowIfFalse(list[0].TokenType == TokenType.PoundDefine);

            Macro macro = null;
            if (list.Count == 3 && list[1].TokenType == TokenType.Word)
            {
                string name = list[1].Value;
                macro = new Macro(name, list[2].Value);
            }
            else if (list.Count == 2 && list[1].TokenType == TokenType.Word)
            {
                string name = list[1].Value;
                macro = new Macro(name, string.Empty);
            }
            else if (list.Count == 1)
            {
                _scanner.AddWarning("Encountered an empty #define");

            }
            else if (list.Count > 3 && list[1].TokenType == TokenType.Word && list[2].TokenType == TokenType.ParenOpen)
            {
                macro = ProcessPoundDefineMethod(line);
            }
            else
            {
                macro = ProcessPoundDefineComplexMacro(line);
            }

            if (macro != null)
            {
                Macro oldMacro = null;
                if (_macroMap.TryGetValue(macro.Name, out oldMacro) && oldMacro.IsPermanent)
                {
                    TraceToStream("Kept: {0} -> {1} Attempted Value {2}", oldMacro.Name, oldMacro.Value, macro.Value);
                }
                else
                {
                    _macroMap[macro.Name] = macro;
                    if (macro.IsMethod)
                    {
                        MethodMacro method = (MethodMacro)macro;
                        TraceToStream("Defined: {0} -> {1}", macro.Name, method.MethodSignature);
                    }
                    else
                    {
                        TraceToStream("Defined: {0} -> {1}", macro.Name, macro.Value);
                    }
                }
            }
        }

        private Macro ProcessPoundDefineComplexMacro(PreprocessorLine line)
        {
            // It's a complex macro.  Go ahead and get the line information
            List<Token> list = new List<Token>(line.TokenList);
            int i = 0;

            // Strip the newlines
            while (i < list.Count)
            {
                if (list[i].TokenType == TokenType.NewLine)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i += 1;
                }
            }
            i = 0;

            // Get the #define token
            Token defineToken = null;
            while (i < list.Count)
            {
                if (list[i].TokenType == TokenType.PoundDefine)
                {
                    defineToken = list[i];
                    break; // TODO: might not be correct. Was : Exit While
                }
                i += 1;
            }

            // Get the name token
            Token nameToken = null;
            while (i < list.Count)
            {
                if (list[i].TokenType == TokenType.Word)
                {
                    nameToken = list[i];
                    break; // TODO: might not be correct. Was : Exit While
                }

                i += 1;
            }

            if (defineToken == null || nameToken == null)
            {
                _errorProvider.AddWarning("Error processing line: {0}", line.ToString());
                return new Macro(NativeSymbolBag.GenerateAnonymousName(), string.Empty);
            }

            // i now points to the name token.  Remove the range of tokens up until this point.  Now remove the
            // whitespace on either end of the list
            list.RemoveRange(0, i + 1);
            while (list.Count > 0 && (list[0].TokenType == TokenType.WhiteSpace || list[0].TokenType == TokenType.NewLine))
            {
                list.RemoveAt(0);
            }

            while (list.Count > 0 && (list[list.Count - 1].TokenType == TokenType.WhiteSpace || list[list.Count - 1].TokenType == TokenType.NewLine))
            {
                list.RemoveAt(list.Count - 1);
            }

            // Create a string for all of the tokens
            var b = new StringBuilder();
            foreach (Token cur in list)
            {
                b.Append(cur.Value);
            }

            return new Macro(nameToken.Value, b.ToString());
        }

        /// <summary>
        /// Process a #define that is actually a function
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private Macro ProcessPoundDefineMethod(PreprocessorLine line)
        {
            // First step is to parse out the name and parameters
            List<Token> list = line.GetValidTokens();
            string name = list[1].Value;
            list.RemoveRange(0, 3);

            List<string> paramList = new List<string>();
            while ((list[0].TokenType != TokenType.ParenClose))
            {
                if (list[0].TokenType == TokenType.Word)
                {
                    paramList.Add(list[0].Value);
                }
                else if (list[0].TokenType == TokenType.ParenOpen)
                {
                    // ( is not legal inside a parameter list.  This is a simple macro
                    return ProcessPoundDefineComplexMacro(line);
                }
                list.RemoveAt(0);
            }

            // Now get the fullBody.  We need the actual text for the fullBody so search through the true token list
            Int32 index = 0;
            while ((line.TokenList[index].TokenType != TokenType.ParenClose))
            {
                index += 1;
            }

            index += 1;
            List<Token> fullBody = line.TokenList.GetRange(index, line.TokenList.Count - index);

            // Strip the trailing and ending whitespace on the fullBody
            while (fullBody.Count > 0 && (fullBody[0].TokenType == TokenType.WhiteSpace || fullBody[0].TokenType == TokenType.NewLine))
            {
                fullBody.RemoveAt(0);
            }

            // Don't be fooled by a simple #define that simply wraps the entire fullBody inside a
            // set of ().  
            if ((fullBody.Count == 0))
            {
                return ProcessPoundDefineComplexMacro(line);
            }

            while (fullBody.Count > 0 && (fullBody[fullBody.Count - 1].TokenType == TokenType.WhiteSpace || fullBody[fullBody.Count - 1].TokenType == TokenType.NewLine))
            {
                fullBody.RemoveAt(fullBody.Count - 1);
            }

            // Coy the body token list since we are about to change the data
            List<Token> body = new List<Token>(fullBody);

            // Collapse the whitespace around ## entries
            int i = 0;
            while (i + 1 < body.Count)
            {
                Token left = body[i];
                Token right = body[i + 1];

                if (left.TokenType == TokenType.Pound && right.TokenType == TokenType.Pound)
                {
                    // First look at the right
                    if (i + 2 < body.Count && body[i + 2].TokenType == TokenType.WhiteSpace)
                    {
                        body.RemoveAt(i + 2);
                    }

                    // Now look at the left
                    if (i > 0 && body[i - 1].TokenType == TokenType.WhiteSpace)
                    {
                        body.RemoveAt(i - 1);
                    }
                }

                i += 1;
            }

            index += 1;
            return new MethodMacro(name, paramList, body, fullBody);
        }

        /// <summary>
        /// Called for a #undef line
        /// </summary>
        /// <param name="line"></param>
        /// <remarks></remarks>
        private void ProcessPoundUndefine(PreprocessorLine line)
        {
            // Get the none whitespace tokens
            List<Token> list = line.GetValidTokens();
            ThrowIfFalse(list[0].TokenType == TokenType.PoundUnDef);

            if (list.Count != 2 || list[1].TokenType != TokenType.Word)
            {
                _scanner.AddWarning("Error processing #undef");
            }
            else
            {
                string name = list[1].Value;
                if (_macroMap.ContainsKey(name))
                {
                    _macroMap.Remove(name);
                    TraceToStream("Undefined: {0}", name);
                }
            }
        }

        /// <summary>
        /// Process a #include line.  These take typically two forms 
        ///   #include "foo.h"
        ///   #include &gt;foo.h&gt;
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <remarks></remarks>

        private void ProcessPoundInclude(PreprocessorLine line)
        {
            if (!_options.FollowIncludes)
            {
                return;
            }

            // if the user did a <> include then there won't be any quotes around the string
            // so go ahead and redo the include to look like a "filename.h" include
            List<Token> list = new List<Token>(line.GetValidTokens());

            // Get rid of the #include
            ThrowIfFalse(list[1].TokenType == TokenType.PoundInclude);
            list.RemoveAt(0);

            string name = null;
            if (list[1].TokenType == TokenType.OpLessThan)
            {
                name = string.Empty;
                list.RemoveAt(0);
                while (list[1].TokenType != TokenType.OpGreaterThan)
                {
                    name += list[1].Value;
                    list.RemoveAt(0);
                }
                list.RemoveAt(0);
            }
            else if (list[1].IsQuotedString)
            {
                name = TokenHelper.ConvertToString(list[1]);
            }
            else
            {
                name = null;
            }

            if (name == null)
            {
                _scanner.AddWarning("Invalid #include statement");
                return;
            }

            // Now actually try and find the file.  First check the custom list
            bool found = false;
            if (File.Exists(name))
            {
                found = true;
                TraceToStream("include {0} followed -> {0}", name);
                TraceToStream("include {0} start", name);
                using (StreamReader reader = new StreamReader(name))
                {
                    ProcessCore(new TextReaderBag(name, reader));
                }
                TraceToStream("include {0} end", name);
            }
            else if (_options.IncludePathList.Count > 0)
            {
                // Search through the path list
                found = false;
                foreach (string prefix in _options.IncludePathList)
                {
                    string fullPath = Path.Combine(prefix, name);
                    if (File.Exists(fullPath))
                    {
                        found = true;
                        TraceToStream("include {0} followed -> {1}", name, fullPath);
                        TraceToStream("include {0} start", name);
                        using (StreamReader reader = new StreamReader(fullPath))
                        {
                            ProcessCore(new TextReaderBag(fullPath, reader));
                        }
                        TraceToStream("include {0} end", name);
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
            }
            else
            {
                found = false;
            }

            if (!found)
            {
                _scanner.AddWarning("Could not locate include file {0}", name);
                TraceToStream("include {0} not followed", name);
            }

        }

        /// <summary>
        /// Process a #pragma statement.
        /// </summary>
        /// <param name="line"></param>
        /// <remarks></remarks>
        private void ProcessPoundPragma(PreprocessorLine line)
        {
            // We don't support #pragma at this point
        }

        /// <summary>
        /// Called when a #if is encountered.  If the condition is true,
        /// it will eat the #if line and let parsing continue.  Otherwise
        /// it will chew until it hits the branch that should be processed 
        /// or it hits the #endif 
        /// </summary>
        /// <remarks></remarks>

        private void ProcessPoundIf(PreprocessorLine line)
        {
            // The object here is to find the branch of the conditional that should
            // be processed
            bool isCondTrue = EvalauteConditional(line);
            TraceToStream("{0}: {1}", isCondTrue, line.DisplayLine);
            if (isCondTrue)
            {
                // Start another processing loop
                this.ProcessLoop();
            }
            else
            {
                ProcessConditionalRemainder();
            }
        }

        /// <summary>
        /// Called when an #ifndef is encountered
        /// </summary>
        /// <param name="line"></param>
        /// <remarks></remarks>
        private void ProcessPoundIfndef(PreprocessorLine line)
        {
            bool isCondTrue = EvalauteConditional(line);
            TraceToStream("{0}: {1}", isCondTrue, line.DisplayLine);
            if (!isCondTrue)
            {
                // Start a processing loop
                this.ProcessLoop();
            }
            else
            {
                ProcessConditionalRemainder();
            }
        }

        /// <summary>
        /// Called when the #if branch of a conditional is not true.  Processes the branch
        /// </summary>
        /// <remarks></remarks>

        private void ProcessConditionalRemainder()
        {
            bool done = false;

            while (!done)
            {
                // It's possible to have unmatched #if blocks.  If we hit the end of the stream this means
                // it is unbalanced so throw an exception
                if (_scanner.EndOfStream)
                {
                    throw new PreProcessorException("Found unbalanced conditional preprocessor branch");
                }

                // Look at the next branch
                ChewThroughConditionalBranch();

                PreprocessorLine cur = this.GetNextLine();
                switch (cur.FirstValidToken.TokenType)
                {
                    case TokenType.PoundElse:
                        // Start another processing loop
                        this.ProcessLoop();
                        done = true;
                        break;
                    case TokenType.PoundElseIf:
                        if (EvalauteConditional(cur))
                        {
                            this.ProcessLoop();
                            done = true;
                        }
                        break;
                    case TokenType.PoundEndIf:
                        done = true;
                        break;
                }
            }
        }


        /// <summary>
        /// Get the next line of tokens
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private PreprocessorLine GetNextLine()
        {
            PreprocessorLine line = new PreprocessorLine();
            line.TokenList = new List<Token>();

            Token lastValidToken = null;
            bool done = false;
            while (!done)
            {
                Token token = _scanner.GetNextToken();
                line.TokenList.Add(token);

                bool isValid = false;

                if (token.TokenType == TokenType.NewLine)
                {
                    // Check and see if this is a preprocessor directive token that ends with a 
                    // backslash.  If so then remove the backslash from the stream and continue processing
                    // the line
                    if (lastValidToken != null && lastValidToken.TokenType == TokenType.BackSlash)
                    {
                        isValid = false;
                        line.TokenList.Remove(lastValidToken);
                        lastValidToken = null;
                    }
                    else
                    {
                        done = true;
                        isValid = true;
                    }
                }
                else if (token.TokenType == TokenType.EndOfStream)
                {
                    done = true;
                    isValid = true;

                    // simulate a newline token
                    line.TokenList.RemoveAt(line.TokenList.Count - 1);
                    line.TokenList.Add(new Token(TokenType.NewLine, Environment.NewLine));
                }
                else if (token.TokenType != TokenType.WhiteSpace)
                {
                    isValid = true;
                }
                else
                {
                    isValid = false;
                }

                if (isValid)
                {
                    lastValidToken = token;
                    if (line.FirstValidToken == null)
                    {
                        line.FirstValidToken = token;

                        // See if this is a preprocessor line
                        if (token.IsPreProcessorDirective)
                        {
                            line.IsPreProcessorDirectiveLine = true;
                        }
                    }
                }
            }

            // This should always have at least one valid token
            ThrowIfNull(line.FirstValidToken);

            // Check and see if the line looks like the following.  If so convert it to a valid pre-processor line
            // #    define foo
            CollapseExpandedPreprocessorLines(ref line);

            // If this is not a preprocessor directive line then we need to substitute all of the
            // #define'd tokens in the stream
            if (!line.IsPreProcessorDirectiveLine || (line.FirstValidToken != null && line.FirstValidToken.TokenType == TokenType.PoundInclude))
            {
                ReplaceDefinedTokens(line);
            }

            // Collapse quoted strings that are adjacent to each other
            CollapseAdjacentQuoteStrings(line);


            return line;
        }

        private void CollapseExpandedPreprocessorLines(ref PreprocessorLine line)
        {
            if (line.FirstValidToken != null && line.FirstValidToken.TokenType == TokenType.Pound)
            {
                List<Token> list = line.GetValidTokens();
                Token possibleToken = list[1];
                Token poundToken = null;

                if (list.Count >= 2 && TokenHelper.TryConvertToPoundToken(possibleToken.Value, out poundToken))
                {
                    // Strip out everything # -> define
                    List<Token> newList = new List<Token>(line.TokenList);
                    bool done = false;
                    while (!done)
                    {
                        if (newList.Count == 0)
                        {
                            Debug.Fail("Non-crititcal error reducing the preprocessor line");
                            return;
                        }
                        else if (object.ReferenceEquals(newList[0], possibleToken))
                        {
                            newList.RemoveAt(0);
                            newList.Insert(0, poundToken);
                            done = true;
                        }
                        else
                        {
                            newList.RemoveAt(0);
                        }
                    }

                    PreprocessorLine formattedLine = new PreprocessorLine();
                    formattedLine.FirstValidToken = poundToken;
                    formattedLine.IsPreProcessorDirectiveLine = true;
                    formattedLine.TokenList = newList;
                    line = formattedLine;
                }
            }
        }

        /// <summary>
        /// Peek at the next line
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private PreprocessorLine PeekNextLine()
        {
            ScannerMark mark = _scanner.Mark();
            PreprocessorLine line = null;
            try
            {
                line = GetNextLine();
            }
            finally
            {
                _scanner.Rollback(mark);
            }

            return line;
        }

        private void ReplaceDefinedTokens(PreprocessorLine line)
        {
            ThrowIfNull(line);

            int i = 0;
            List<Token> list = line.TokenList;
            while ((i < list.Count))
            {
                Token token = list[i];
                if (token.TokenType != TokenType.Word)
                {
                    i += 1;
                    continue;
                }

                Macro macro = null;
                if (_macroMap.TryGetValue(token.Value, out macro))
                {
                    // Remove the original token
                    list.RemoveAt(i);

                    List<Token> replaceList = null;
                    if (macro.IsMethod)
                    {
                        MethodMacro method = (MethodMacro)macro;
                        List<Token> args = ParseAndRemoveMacroMethodArguments(list, i);
                        if (args == null)
                        {
                            // Parse did not succeed, move to the next token
                            i += 1;
                        }
                        else
                        {
                            // Insert the tokens
                            replaceList = ReplaceMethodMacro(method, args);
                        }
                    }
                    else
                    {
                        // Use the scanner to create the replacement tokens
                        replaceList = Scanner.TokenizeText(macro.Value, CreateScannerOptions());
                    }

                    if (replaceList != null)
                    {
                        CollapseDoublePounds(replaceList);
                        list.InsertRange(i, replaceList);
                    }
                }
                else
                {
                    i += 1;
                }

            }

            // Do one more pass to check and see if we need a recursive replace
            bool needAnotherPass = false;
            foreach (Token cur in line.TokenList)
            {
                if (cur.TokenType == TokenType.Word && _macroMap.ContainsKey(cur.Value))
                {
                    needAnotherPass = true;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }

            if (needAnotherPass)
            {
                ReplaceDefinedTokens(line);
            }

        }

        private List<Token> ReplaceMethodMacro(MethodMacro method, List<Token> args)
        {
            // First run the replacement 
            List<Token> retList = method.Replace(args);

            // When creating the arguments for a macro, non-trivial arguments (1+2) come accross
            // as text macros.  For those items we need to reparse them here and put them back into the stream.
            // Have to do this after the above loop so that ## and # are processed correctly
            int i = 0;
            while (i < retList.Count)
            {
                Token cur = retList[i];
                if (cur.TokenType == TokenType.Text && args.IndexOf(cur) >= 0)
                {
                    retList.RemoveAt(i);
                    retList.InsertRange(i, Scanner.TokenizeText(cur.Value, _scanner.Options));
                }

                i += 1;
            }

            return retList;
        }

        private List<Token> ParseAndRemoveMacroMethodArguments(List<Token> list, Int32 start)
        {
            List<Token> args = new List<Token>();
            Int32 i = start;

            // Search for the start paren
            while (i < list.Count && list[i].TokenType == TokenType.WhiteSpace)
            {
                i += 1;
            }

            if (list[i].TokenType != TokenType.ParenOpen)
            {
                return null;
            }
            i += 1;
            // Move past the '('

            var depth = 0;
            var curArg = new Token(TokenType.Text, string.Empty);
            var done = false;
            while (i < list.Count && !done)
            {
                Token cur = list[i];
                bool append = false;
                switch (cur.TokenType)
                {
                    case TokenType.Comma:
                        if (depth == 0)
                        {
                            args.Add(curArg);
                            curArg = new Token(TokenType.Text, string.Empty);
                        }
                        break;
                    case TokenType.ParenOpen:
                        depth += 1;
                        append = true;
                        break;
                    case TokenType.ParenClose:
                        if (depth == 0)
                        {
                            args.Add(curArg);
                            done = true;
                        }
                        else
                        {
                            depth -= 1;
                            append = true;
                        }
                        break;
                    default:
                        append = true;
                        break;
                }

                if (done)
                {
                    break;
                }

                if (append)
                {
                    if (curArg.TokenType == TokenType.Text && string.IsNullOrEmpty(curArg.Value))
                    {
                        curArg = cur;
                    }
                    else
                    {
                        curArg = new Token(TokenType.Text, curArg.Value + cur.Value);
                    }
                }

                i += 1;
            }

            if (i == list.Count)
            {
                return null;
            }

            // Success so remove the list.  'i' currently points at )
            list.RemoveRange(start, (i - start) + 1);
            return args;
        }

        /// <summary>
        /// When two quoted strings appear directly next to each other then make them one 
        /// quoted string
        /// </summary>
        /// <param name="line"></param>
        /// <remarks></remarks>

        private void CollapseAdjacentQuoteStrings(PreprocessorLine line)
        {
            List<Token> list = line.TokenList;

            // Loop for more
            Int32 index = 0;

            while (index < list.Count)
            {
                if (!list[index].IsQuotedString)
                {
                    index += 1;
                    continue;
                }

                // Found a quoted string, search for a partner
                Int32 nextIndex = index + 1;
                Token nextToken = null;
                bool done = false;
                while (nextIndex < list.Count && !done)
                {
                    switch (list[nextIndex].TokenType)
                    {
                        case TokenType.WhiteSpace:
                        case TokenType.NewLine:
                            nextIndex += 1;
                            break;
                        case TokenType.QuotedStringAnsi:
                        case TokenType.QuotedStringUnicode:
                            nextToken = list[nextIndex];
                            done = true;
                            break;
                        default:
                            done = true;
                            break;
                    }
                }

                if (nextToken != null)
                {
                    // Create the new token
                    string first = list[index].Value;
                    string second = nextToken.Value;
                    string str = "\"" + first.Substring(1, first.Length - 2) + second.Substring(1, second.Length - 2) + "\"";

                    // Remove all of the tokens between these two and the second string
                    list.RemoveRange(index, (nextIndex - index) + 1);
                    list.Insert(index, new Token(TokenType.QuotedStringAnsi, str));
                }
                else
                {
                    index += 1;
                }
            }
        }

        private void CollapseDoublePounds(List<Token> list)
        {
            int i = 0;
            while ((i + 3) < list.Count)
            {
                Token t1 = list[i];
                Token t2 = list[i + 1];
                Token t3 = list[i + 2];
                Token t4 = list[i + 3];
                if (t2.TokenType == TokenType.Pound && t3.TokenType == TokenType.Pound)
                {
                    list.RemoveRange(i, 4);
                    list.Insert(i, new Token(TokenType.Text, t1.Value + t4.Value));
                }

                i += 1;
            }
        }

        #region "Trace"

        private void Trace(string msg)
        {
            if (_options.Trace)
            {
                _outputStream.Write("// ");
                _outputStream.WriteLine(msg);
            }
        }

        private void TraceToStream(string format, params object[] args)
        {
            if (_options.Trace)
            {
                Trace(string.Format(format, args));
            }
        }

        private void TraceSkippedLine(PreprocessorLine line)
        {
            if (_options.Trace)
            {
                Trace(string.Format("Skipped: {0}", line.DisplayLine));
            }
        }

        private void TraceMacroMap()
        {
            if (_options.Trace)
            {
                List<Macro> list = new List<Macro>(_macroMap.Values);
                list.Sort(TraceCompareMacros);
                Trace("Macro Map Dump");
                foreach (Macro cur in list)
                {
                    if (cur.IsMethod)
                    {
                        TraceToStream("{0} -> {1}", cur.Name, ((MethodMacro)cur).MethodSignature);
                    }
                    else
                    {
                        TraceToStream("{0} -> {1}", cur.Name, cur.Value);
                    }
                }
            }
        }

        private static int TraceCompareMacros(Macro x, Macro y)
        {
            return string.CompareOrdinal(x.Name, y.Name);
        }

        #endregion

        private void WriteToStream(PreprocessorLine line)
        {
            foreach (Token token in line.TokenList)
            {
                _outputStream.Write(token.Value);
            }
        }


        /// <summary>
        /// Chew through the current conditional branch.  Stop when the next valid
        /// preprocessor branch is encountered.  This will chew through any nestede branches
        /// with no regard for their content
        /// 
        /// When this method is finished, a valid pre-processor line will be the next available
        /// line
        /// </summary>
        /// <remarks></remarks>
        private void ChewThroughConditionalBranch()
        {
            bool done = false;
            int nestedIfCount = 0;
            while (!done)
            {
                PreprocessorLine line = this.PeekNextLine();
                if (line.FirstValidToken.TokenType == TokenType.EndOfStream)
                {
                    return;
                }

                TokenType type = line.FirstValidToken.TokenType;
                if (nestedIfCount == 0)
                {
                    // Not in a nested if, just look for the next valid preprocessor token
                    switch (type)
                    {
                        case TokenType.PoundElse:
                        case TokenType.PoundElseIf:
                        case TokenType.PoundEndIf:
                            done = true;
                            break;
                        case TokenType.PoundIf:
                        case TokenType.PoundIfndef:
                            nestedIfCount = +1;
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case TokenType.PoundIf:
                        case TokenType.PoundIfndef:
                            nestedIfCount += 1;
                            break;
                        case TokenType.PoundEndIf:
                            nestedIfCount -= 1;
                            break;
                    }
                }

                // If we're not done yet then chew through the line
                if (!done)
                {
                    TraceSkippedLine(line);
                    this.GetNextLine();
                }
            }
        }

        /// <summary>
        /// Chew completely through the remainder of the conditional.  Basically consume 
        /// the #endif line to match the #if/#elsif we've already processed
        /// </summary>
        /// <remarks></remarks>
        private void ChewThroughConditionalEnd()
        {
            bool done = false;
            while (!done)
            {
                ChewThroughConditionalBranch();

                PreprocessorLine line = this.GetNextLine();
                TraceSkippedLine(line);
                if (line.FirstValidToken.TokenType == TokenType.PoundEndIf)
                {
                    done = true;
                }
            }
        }

        private string ValidTokenListToString(IEnumerable<Token> enumerable)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Token token in enumerable)
            {
                builder.Append(token.Value);
                builder.Append(" ");
            }

            // Remove the last space
            builder.Length -= 1;
            return builder.ToString();
        }

        /// <summary>
        /// Evaluate a preprocessor conditional statement and return whether or not it is true
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool EvalauteConditional(PreprocessorLine line)
        {
            List<Token> list = line.GetValidTokens();

            // Remove the #pound token.  We don't care what type of conditional this is, this just serves
            // to evaluate it and let the caller interpret the result
            list.RemoveAt(0);

            // Make sure that all "defined" expressions wrap the next value in ()
            Int32 i = 0;
            while (i + 1 < list.Count)
            {
                Token cur = list[i];

                if (cur.TokenType == TokenType.Word && 0 == string.CompareOrdinal("defined", cur.Value) && list[i + 1].TokenType == TokenType.Word)
                {
                    list.Insert(i + 1, new Token(TokenType.ParenOpen, "("));
                    list.Insert(i + 3, new Token(TokenType.ParenClose, ")"));

                    i += 3;
                }

                i += 1;
            }

            ExpressionValue value = null;
            if (!_eval.TryEvaluate(list, out value))
            {
                _errorProvider.AddError("Could not evaluate expression {0}", line.ToString());
                return false;
            }

            return value.ConvertToBool();
        }
    }

}
