// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;
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
    /// Result of a parse operation
    /// </summary>
    /// <remarks></remarks>
    public sealed class ParseResult
    {
        /// <summary>
        /// Contains error and warning information from the Parse
        /// </summary>
        public ErrorProvider ErrorProvider { get; } = new ErrorProvider();

        public List<NativeDefinedType> NativeDefinedTypes { get; } = new List<NativeDefinedType>();

        public List<NativeProcedure> NativeProcedures { get; } = new List<NativeProcedure>();

        public List<NativeTypeDef> NativeTypeDefs { get; } = new List<NativeTypeDef>();

        public List<NativeEnumValue> NativeEnumValues { get; } = new List<NativeEnumValue>();

        public List<NativeType> ParsedTypes { get; } = new List<NativeType>();

        public ParseResult()
        {

        }
    }

    public class ParseException : Exception
    {
        private bool _isError = true;

        private bool _isStreamOk;
        internal bool IsError
        {
            get { return _isError; }
        }

        internal bool IsStreamOk
        {
            get { return _isStreamOk; }
        }

        private ParseException(string msg, bool isError) : base(msg)
        {
            _isError = isError;
        }

        private ParseException(string msg, Exception inner) : base(msg, inner)
        {
        }

        public static ParseException CreateError(string msg)
        {
            return new ParseException(msg, true);
        }

        public static ParseException CreateError(string format, params object[] args)
        {
            return CreateError(string.Format(format, args));
        }

        public static ParseException CreateError(string msg, Exception inner)
        {
            return new ParseException(msg, inner);
        }

        /// <summary>
        /// Warning where the stream is in a bad positition
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ParseException CreateWarning(string msg)
        {
            return new ParseException(msg, false);
        }

        public static ParseException CreateWarning(string format, params object[] args)
        {
            return CreateWarning(string.Format(format, args));
        }

        /// <summary>
        /// Warning where the stream is properly set past the problem 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ParseException CreateWarningStreamOk(string msg)
        {
            ParseException ex = CreateWarning(msg);
            ex._isStreamOk = true;
            return ex;
        }

        public static ParseException CreateWarningStreamOk(string format, params object[] args)
        {
            return CreateWarningStreamOk(string.Format(format, args));
        }

        public static ParseException FoundEndOfStream()
        {
            return ParseException.CreateError("Unexpected end of stream encountered");
        }
    }

    /// <summary>
    /// Parses out Native Code to find the types, macros, typedefs and functions we are
    /// interested in.  It does not do any type resolution nor does it attempt to do 
    /// any sort of macro processing 
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayString}")]
    public class ParseEngine
    {

        #region "ParseEngineException"
        private class ParseEngineException : Exception
        {

            public ParseEngineException(string msg) : base(msg)
            {
            }

            public ParseEngineException(string msg, Exception inner) : base(msg, inner)
            {
            }
        }

        #endregion

        private bool parsing;
        private Scanner scanner;
        private ParseResult resultant;
        private ErrorProvider errorProvider = new ErrorProvider();

        private Dictionary<string, SalEntryType> _salTable = new Dictionary<string, SalEntryType>(StringComparer.OrdinalIgnoreCase);
        private string DisplayString
        {
            get { return "ParseEngine: " + this.PeekLineInformation(20); }
        }

        /// <summary>
        /// Create a new Parser
        /// </summary>
        /// <remarks></remarks>
        public ParseEngine()
        {
            BuildLookupTables();
        }

        private void BuildLookupTables()
        {
            // Build the SAL table 
            foreach (SalEntryType e in System.Enum.GetValues(typeof(SalEntryType)))
            {
                _salTable.Add(NativeSalEntry.GetDirectiveForEntry(e), e);
            }
        }

        public ParseResult Parse(TextReader reader)
        {
            return ParseCore(new TextReaderBag(reader));
        }

        public ParseResult Parse(TextReaderBag readerbag)
        {
            return ParseCore(readerbag);
        }

        public ParseResult Parse(string text)
        {
            dynamic bytes = Encoding.UTF8.GetBytes(text);
            var stream = new MemoryStream(bytes); // TODO : using
            return Parse(new StreamReader(stream)); // TODO : using
        }

        private ParseResult ParseCore(TextReaderBag readerBag)
        {
            ThrowIfNull(readerBag);
            ThrowIfTrue(parsing, "Recursive parsing is not supported.  Instead create a new Parser");
            var toReturn = default(ParseResult);

            try
            {
                // Build the options
                var opts = new ScannerOptions
                {
                    ThrowOnEndOfStream = true,
                    HideWhitespace = true,
                    HideNewLines = true,
                    HideComments = true // TODO : if we set this to true the prser barfs on the comments
                };

                parsing = true;
                resultant = new ParseResult();
                scanner = new Scanner(readerBag, opts)
                {
                    ErrorProvider = resultant.ErrorProvider
                };

                // Actually do the parsing
                ParseCoreRoutine();

                resultant.ErrorProvider.Append(errorProvider);
                toReturn = resultant;
            }
            finally
            {
                scanner = null;
                parsing = false;
                resultant = null;
            }

            return toReturn;
        }

        /// <summary>
        /// Core Parsing loop
        /// </summary>
        /// <remarks></remarks>

        private void ParseCoreRoutine()
        {
            // Since the parser will constantly retry the same operation after moving the scanner 
            // just a bit we will often get the same error message multiple times.  To prevent giving 
            // the user this error message a ton of times will keep a table to ensure it doesn't happen
            Dictionary<string, object> parseErrorTable = new Dictionary<string, object>();

            bool done = false;

            while (!done)
            {
                // Check for the end of the stream
                if (scanner.EndOfStream)
                {
                    done = true;
                    continue;
                }

                // Setup a mark.  If the routine fails to parse we want to rollback the scanner
                // and read past the troublesome line
                ScannerMark mark = scanner.Mark();

                Token token = scanner.PeekNextToken();
                try
                {
                    NativeSalAttribute ntSal = new NativeSalAttribute();
                    if (token.TokenType == TokenType.DeclSpec)
                    {
                        ntSal = ProcessSalAttribute();
                        token = scanner.PeekNextToken();
                    }

                    if (token.TokenType == TokenType.TypeDefKeyword)
                    {
                        ProcessTypeDef();
                    }
                    else if (TokenHelper.IsCallTypeModifier(token.TokenType))
                    {
                        ProcessProcedure();

                    }
                    else if (token.IsAnyWord)
                    {
                        // Next try and process a type
                        NativeType parsedType = ProcessTypeNameOrType();
                        Token nextToken = scanner.PeekNextToken();

                        if (parsedType.Category == NativeSymbolCategory.Defined)
                        {
                            // If the next token is a semicolon we are done with this type 
                            if (nextToken.TokenType == TokenType.Semicolon)
                            {
                                continue;
                            }
                        }

                        if (nextToken.TokenType == TokenType.Word || TokenHelper.IsCallTypeModifier(nextToken.TokenType))
                        {
                            ProcessProcedure(parsedType, ntSal, new TriState<NativeCallingConvention>());
                        }
                        else if (nextToken.TokenType == TokenType.ParenOpen)
                        {
                            ProcessFunctionPointer(string.Empty, parsedType, ntSal);
                        }
                    }
                    else
                    {
                        ProcessGlobalTokenForUnsupportedScenario();
                        scanner.GetNextToken();
                    }
                }
                catch (ParseException ex)
                {
                    if (!parseErrorTable.ContainsKey(ex.Message))
                    {
                        parseErrorTable.Add(ex.Message, null);
                        if (ex.IsError)
                        {
                            errorProvider.AddError(ex.Message);
                        }
                        else
                        {
                            errorProvider.AddWarning(ex.Message);
                        }

                        // If the thrower did not put the stream in a good place chew
                        // through this line
                        if (!ex.IsStreamOk)
                        {
                            ChewThroughEndOfLine();
                        }
                    }

                }
                catch (EndOfStreamException)
                {
                    // Rollback the scanner and process the next line
                    errorProvider.AddError("Unexpectedly hit the end of the stream");
                    scanner.Rollback(mark);
                    ChewThroughEndOfLine();


                }
                catch (Exception ex)
                {
                    // Rollback the scanner.  The process through this line
                    errorProvider.AddError(ex.Message);
                    scanner.Rollback(mark);
                    ChewThroughEndOfLine();
                }
            }
        }

        /// <summary>
        /// Process a type definition from code
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private List<NativeTypeDef> ProcessTypeDef()
        {

            // Chew through the typedef token if it hasn't been consumed
            Token token = scanner.PeekNextToken();
            if (token.TokenType == TokenType.TypeDefKeyword)
            {
                scanner.GetNextToken();
            }

            NativeSalAttribute sal = default(NativeSalAttribute);
            NativeType source = default(NativeType);
            ScannerMark typeMark = scanner.Mark();
            try
            {
                // Get the type name which is the source of the typedef.  This can only 
                // be a defined type or a type name.  Also since this could still be 
                // a function pointer, read the possible return type sal attribute
                sal = this.ProcessSalAttribute();
                NativeDefinedType definedNt = ProcessDefinedTypeNoFunctionPointers(string.Empty);
                if (definedNt != null)
                {
                    source = definedNt;
                }
                else
                {
                    source = ProcessShortTypeName();
                }
            }
            catch (ParseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                scanner.Rollback(typeMark);
                string msg = string.Format("Error processing typedef \"{0}\": {1}", PeekLineInformation(4), ex.Message);
                throw ParseException.CreateError(msg, ex);
            }

            // Now just process the post members
            return this.ProcessTypePostTypeDefs(source);
        }

        private NativeStruct ProcessClass()
        {
            return ProcessClass(string.Empty);
        }

        /// <summary>
        /// Called when we encounter a class token.  Treat this just like a struct 
        /// </summary>
        /// <param name="nameprefix"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeStruct ProcessClass(string nameprefix)
        {
            Contract.ThrowIfNull(nameprefix);

            // If this called with a struct token still in the stream, remove it
            if (scanner.PeekNextToken().TokenType == TokenType.ClassKeyword)
            {
                scanner.GetNextToken();
            }

            return ProcessStruct(nameprefix);
        }

        private NativeStruct ProcessStruct()
        {
            return ProcessStruct(string.Empty);
        }

        /// <summary>
        /// Called when we encounter a struct token.  
        /// 
        /// struct foo 
        /// {
        ///   int i;
        ///   boolean j
        /// };
        /// </summary>
        /// <remarks></remarks>
        private NativeStruct ProcessStruct(string namePrefix)
        {
            Contract.ThrowIfNull(namePrefix);

            // If this called with a struct token still in the stream, remove it
            if (scanner.PeekNextToken().TokenType == TokenType.StructKeyword)
            {
                scanner.GetNextToken();
            }

            // Remove any SAL attribute
            // TODO: It may be worthwhile in a future version to add support for the __declspec attribute
            if (scanner.PeekNextToken().TokenType == TokenType.DeclSpec)
            {
                ProcessSalAttribute();
            }

            // Check and see if the next token is a word.  If so then it's a named 
            // struct and otherwise it's inline
            string name = null;
            bool isInline = false;
            Token nameToken = scanner.PeekNextToken();
            if (nameToken.TokenType == TokenType.Word)
            {
                scanner.GetNextToken();
                name = namePrefix + nameToken.Value;
                isInline = false;
            }
            else
            {
                name = string.Empty;
                isInline = true;
            }

            // For forward declaration structs the next token will be a ';'.  There is nothing 
            // to add for structures of this type
            if (scanner.PeekNextToken().TokenType == TokenType.Semicolon)
            {
                return null;
            }

            // Check through the open brace structure
            scanner.GetNextToken(TokenType.BraceOpen);

            // Get the members
            List<NativeMember> list = ProcessTypeMemberList(name);

            // Move through the close brace
            scanner.GetNextToken(TokenType.BraceClose);

            // Create the struct type
            NativeStruct ntStruct = new NativeStruct();
            ntStruct.Name = name;
            ntStruct.Members.AddRange(list);

            // If this is an inline type, make sure to mark it as anonymous
            if (isInline)
            {
                ntStruct.IsAnonymous = true;
            }

            // Process the type
            ProcessParsedDefinedType(ntStruct);

            // If this is not an inline definition then it's possible to add typedefs immediately
            // after the struct definition
            if (!isInline)
            {
                ProcessTypePostTypeDefs(ntStruct);
            }

            return ntStruct;
        }

        private NativeUnion ProcessUnion()
        {
            return ProcessUnion(string.Empty);
        }

        /// <summary>
        /// Process a union type member
        /// </summary>
        /// <remarks></remarks>
        private NativeUnion ProcessUnion(string namePrefix)
        {
            Contract.ThrowIfNull(namePrefix);

            // Check through the union token if it hasn't been consumed
            Token token = scanner.PeekNextToken();
            if (token.TokenType == TokenType.UnionKeyword)
            {
                scanner.GetNextToken();
            }

            // See if this is an inline union or a named one
            bool isInline = false;
            string name = string.Empty;
            token = scanner.PeekNextToken();
            if (token.TokenType == TokenType.Word)
            {
                name = namePrefix + token.Value;
                scanner.GetNextToken();
            }
            else
            {
                isInline = true;
            }

            // Get the open brace
            scanner.GetNextToken(TokenType.BraceOpen);

            List<NativeMember> list = ProcessTypeMemberList(name);

            // Get the close brace
            scanner.GetNextToken(TokenType.BraceClose);

            // Create the union
            NativeUnion ntUnion = new NativeUnion();
            ntUnion.Name = name;
            ntUnion.IsAnonymous = isInline;
            ntUnion.Members.AddRange(list);
            ProcessParsedDefinedType(ntUnion);

            // If this is not an inline type then process the post type defs
            if (!isInline)
            {
                ProcessTypePostTypeDefs(ntUnion);
            }

            return ntUnion;
        }

        private NativeEnum ProcessEnum()
        {
            return ProcessEnum(string.Empty);
        }

        /// <summary>
        /// Process out the enumeration
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeEnum ProcessEnum(string namePrefix)
        {
            Contract.ThrowIfNull(namePrefix);

            // Move past the enum token if it's still in the stream
            Token token = scanner.PeekNextToken();
            if (token.TokenType == TokenType.EnumKeyword)
            {
                scanner.GetNextToken();
            }

            // Check to see if this is an inline enum
            bool isInline = false;
            string name = string.Empty;
            token = scanner.PeekNextToken();
            if (token.TokenType == TokenType.Word)
            {
                scanner.GetNextToken();
                isInline = false;
                name = namePrefix + token.Value;
            }
            else
            {
                isInline = true;
            }

            // Get the open brace
            scanner.GetNextToken(TokenType.BraceOpen);

            var ntEnum = new NativeEnum(name);
            ntEnum.IsAnonymous = isInline;
            ProcessParsedDefinedType(ntEnum);

            List<NativeEnumValue> list = ProcessEnumValues(ntEnum.Name);
            foreach (var item in list)
            {
                resultant.NativeEnumValues.Add(item);
                ntEnum.Values.Add(item);
            }

            // Get the close brace
            scanner.GetNextToken(TokenType.BraceClose);

            // If this isnot' an inline type then process the post type defs
            if (!isInline)
            {
                ProcessTypePostTypeDefs(ntEnum);
            }

            return ntEnum;
        }

        /// <summary>
        /// Read the list of enum values for an enum definition
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private List<NativeEnumValue> ProcessEnumValues(string enumName)
        {
            List<NativeEnumValue> list = new List<NativeEnumValue>();

            // Allow for an empty enum list
            if (scanner.PeekNextToken().TokenType == TokenType.BraceClose)
            {
                return list;
            }

            bool done = false;
            while (!done)
            {
                Token nameToken = scanner.GetNextToken(TokenType.Word);

                Token token = scanner.PeekNextToken();
                switch (token.TokenType)
                {
                    case TokenType.Comma:
                        scanner.GetNextToken();
                        list.Add(new NativeEnumValue(enumName, nameToken.Value));
                        break;
                    case TokenType.BraceClose:
                        list.Add(new NativeEnumValue(enumName, nameToken.Value));
                        break;
                    case TokenType.OpAssign:
                        scanner.GetNextToken();
                        string value = ProcessConstantValue();
                        if (scanner.PeekNextToken().TokenType == TokenType.Comma)
                        {
                            scanner.GetNextToken();
                        }
                        list.Add(new NativeEnumValue(enumName: enumName, valueName: nameToken.Value, value: value));
                        break;
                    default:
                        scanner.AddWarning("Unexpected token while processing enum values: {0}", token.TokenType);
                        done = true;
                        break;
                }

                token = scanner.PeekNextToken();
                if (token.TokenType == TokenType.BraceClose)
                {
                    done = true;
                }
            }

            return list;
        }

        private NativeProcedure ProcessProcedure()
        {
            ScannerMark mark = scanner.Mark();
            TriState<NativeCallingConvention> callmod = new TriState<NativeCallingConvention>();
            ProcessCalltypeModifier(ref callmod);

            // Process the return type sal attribute
            NativeSalAttribute retTypeSal = ProcessSalAttribute();
            ProcessCalltypeModifier(ref callmod);
            NativeType retType = ProcessTypeNameOrType();
            if (retType == null)
            {
                scanner.Rollback(mark);
                return null;
            }

            return ProcessProcedure(retType, retTypeSal, callmod);
        }

        /// <summary>
        /// Try and Parse out a procedure from the code.  Unlike enum, struct and union, there is 
        /// no keyword before to say that we are about to be at one so we have to do a bit of guessing here 
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeProcedure ProcessProcedure(NativeType retType, NativeSalAttribute retTypeSal, TriState<NativeCallingConvention> callmod)
        {
            ThrowIfNull(callmod);
            ScannerMark mark = scanner.Mark();

            ProcessCalltypeModifier(ref callmod);
            Token nameToken = scanner.PeekNextToken();
            if (nameToken.TokenType != TokenType.Word)
            {
                scanner.Rollback(mark);
                return null;
            }
            scanner.GetNextToken();

            try
            {
                List<NativeParameter> list = ProcessParameterList(nameToken.Value);
                if (list == null)
                {
                    scanner.Rollback(mark);
                    return null;
                }

                // Create the signature
                NativeSignature sig = new NativeSignature();
                sig.ReturnType = retType;
                sig.ReturnTypeSalAttribute = retTypeSal;
                sig.Parameters.AddRange(list);

                // Create the procedure
                NativeProcedure proc = new NativeProcedure();
                proc.Name = nameToken.Value;
                proc.Signature = sig;

                // Check to see if the procedure has an inline block declared after it.  If so then process
                // the block away
                if (!scanner.EndOfStream && scanner.PeekNextToken().TokenType == TokenType.BraceOpen)
                {
                    ProcessBlock(TokenType.BraceOpen, TokenType.BraceClose);
                    throw ParseException.CreateWarningStreamOk("Ignoring Procedure {0} because it is defined inline.", proc.Name);
                }

                // If we found a calling convention for the procedure add it to the definition
                if (callmod.HasValue)
                {
                    proc.CallingConvention = callmod.Value;
                }

                // Add the procedure to the parsed list
                ProcessParsedProcedure(proc);
                return proc;
            }
            catch (ParseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error processing procedure {0}: {1}", nameToken.Value, ex.Message);
                throw ParseException.CreateError(msg, ex);
            }

        }

        private void ProcessCalltypeModifier(ref TriState<NativeCallingConvention> value)
        {
            while (TokenHelper.IsCallTypeModifier(scanner.PeekNextToken().TokenType))
            {
                Token token = scanner.GetNextToken();
                NativeCallingConvention callmod = default(NativeCallingConvention);
                switch (token.TokenType)
                {
                    case TokenType.WinApiCallKeyword:
                        callmod = NativeCallingConvention.WinApi;
                        break;
                    case TokenType.StandardCallKeyword:
                        callmod = NativeCallingConvention.Standard;
                        break;
                    case TokenType.CDeclarationCallKeyword:
                        callmod = NativeCallingConvention.CDeclaration;
                        break;
                    case TokenType.PascalCallKeyword:
                        callmod = NativeCallingConvention.Pascal;
                        break;
                    case TokenType.ClrCallKeyword:
                        callmod = NativeCallingConvention.Clr;
                        break;
                    case TokenType.InlineKeyword:
                        callmod = NativeCallingConvention.Inline;
                        break;
                    default:
                        callmod = NativeCallingConvention.WinApi;
                        ThrowInvalidEnumValue(token.TokenType);
                        break;
                }

                value.SetValue(callmod);
            }
        }

        /// <summary>
        /// Process the access modifier lines that can be added to native structures.  We don't need
        /// these for our generation story but we do need to parse them out 
        /// </summary>
        /// <remarks></remarks>
        private void ProcessAccessModifiers()
        {
            Token token = scanner.PeekNextToken();
            while (token.IsAccessModifier)
            {
                scanner.GetNextToken();
                token = scanner.PeekNextToken();
                if (token.TokenType == TokenType.Colon)
                {
                    scanner.GetNextToken();
                    token = scanner.PeekNextToken();
                }
            }
        }

        private NativeFunctionPointer ProcessFunctionPointer(string namePrefix, NativeType retType)
        {
            return ProcessFunctionPointer(namePrefix, retType, new NativeSalAttribute());
        }

        /// <summary>
        /// Process a function pointer in code
        /// 
        /// This function is called when the parser is immediately after the return type of the 
        /// function pointer in the scanner
        /// </summary>
        /// <param name="retType"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeFunctionPointer ProcessFunctionPointer(string namePrefix, NativeType retType, NativeSalAttribute retTypeSal)
        {
            ThrowIfNull(namePrefix);
            ThrowIfNull(retType);

            // It's fine for this method to be called with the scanner either immediately before or 
            // after the opening paren
            if (scanner.PeekNextToken().TokenType == TokenType.ParenOpen)
            {
                scanner.GetNextToken();
            }

            // Remove the calling convention
            TriState<NativeCallingConvention> callmod = new TriState<NativeCallingConvention>();
            ProcessCalltypeModifier(ref callmod);

            // If there is a * in the name then parse that as well
            if (scanner.PeekNextToken().TokenType == TokenType.Asterisk)
            {
                scanner.GetNextToken();
            }

            // Get the acutal name from code.  Make sure to handle the anonymous function pointer case
            string name = null;
            if (scanner.PeekNextToken().TokenType == TokenType.Word)
            {
                name = namePrefix + scanner.GetNextToken().Value;
            }
            else
            {
                name = NativeSymbolBag.GenerateAnonymousName();
            }

            scanner.GetNextToken(TokenType.ParenClose);

            return ProcessFunctionPointerParameters(name, retType, retTypeSal, callmod);
        }

        private NativeFunctionPointer ProcessFunctionPointerParameters(string name, NativeType retType, NativeSalAttribute retTypeSal, TriState<NativeCallingConvention> callmod)
        {
            // Now get the parameter list
            List<NativeParameter> list = this.ProcessParameterList(name);
            if (list == null)
            {
                throw ParseException.CreateError("Error parsing parameters for function pointer {0}", name);
            }

            NativeFunctionPointer ptr = new NativeFunctionPointer(name);
            ptr.Signature.ReturnType = retType;
            ptr.Signature.ReturnTypeSalAttribute = retTypeSal;
            ptr.Signature.Parameters.AddRange(list);
            ptr.IsAnonymous = NativeSymbolBag.IsAnonymousName(name);

            if (callmod.HasValue)
            {
                ptr.CallingConvention = callmod.Value;
            }

            ProcessParsedDefinedType(ptr);
            return ptr;
        }

        /// <summary>
        /// Process the list of parameters.  This can be for either a function pointer or a normal
        /// procedure.  
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private List<NativeParameter> ProcessParameterList(string procName)
        {
            List<NativeParameter> list = new List<NativeParameter>();

            Token token = scanner.GetNextToken();
            if (token.TokenType != TokenType.ParenOpen)
            {
                return null;
            }

            // Check for the (void) signature
            List<Token> voidList = scanner.PeekTokenList(2);

            if (voidList[1].TokenType == TokenType.ParenClose && voidList[0].TokenType == TokenType.VoidKeyword)
            {
                // Get the tokens for the signature off of the stream
                scanner.GetNextToken(TokenType.VoidKeyword);
                scanner.GetNextToken(TokenType.ParenClose);
                return list;
            }

            do
            {
                token = scanner.PeekNextToken();
                if (token.TokenType == TokenType.ParenClose)
                {
                    scanner.GetNextToken();
                    break;
                }
                else if (token.TokenType == TokenType.Period)
                {
                    // Check for variable arguments signature
                    List<Token> varList = scanner.PeekTokenList(3);
                    if (varList[1].TokenType == TokenType.Period && varList[2].TokenType == TokenType.Period)
                    {
                        ProcessBlockRemainder(TokenType.ParenOpen, TokenType.ParenClose);

                        // Make sure to remove the { if it is both variable and inline
                        if (!scanner.EndOfStream && scanner.PeekNextToken().TokenType == TokenType.BraceOpen)
                        {
                            ProcessBlock(TokenType.BraceOpen, TokenType.BraceClose);
                        }

                        throw ParseException.CreateWarningStreamOk("Procedure {0} has a variable argument signature which is unsupported.", procName);
                    }
                }

                // Process the actual parameter
                list.Add(ProcessParameter());

                if (scanner.PeekNextToken().TokenType == TokenType.Comma)
                {
                    scanner.GetNextToken();
                }
            } while (true);

            return list;
        }

        private NativeParameter ProcessParameter()
        {

            // Process any sal attributes
            NativeSalAttribute sal = ProcessSalAttribute();

            NativeParameter param = new NativeParameter();
            param.NativeType = ProcessTypeName();

            if (scanner.PeekNextToken().TokenType == TokenType.Word)
            {
                // Match the name if it's present
                param.Name = scanner.GetNextToken().Value;

            }
            else if (scanner.PeekNextToken().TokenType == TokenType.ParenOpen)
            {
                // It's legal to have an inline function pointer as a parameter type.  In that
                // case though the parameter will have no name and will instead take the name of 
                // the function pointer (if it's no anonymous)
                NativeFunctionPointer fptr = ProcessFunctionPointer(string.Empty, param.NativeType);
                param.NativeType = fptr;
                if (!fptr.IsAnonymous)
                {
                    param.Name = fptr.Name;
                    fptr.IsAnonymous = true;
                    fptr.Name = NativeSymbolBag.GenerateAnonymousName();
                }

            }

            // It's valid for the trailing [] to come after the parameter name and we
            // need to process them here
            while (scanner.PeekNextToken().TokenType == TokenType.BracketOpen)
            {
                scanner.GetNextToken();
                scanner.GetNextToken(TokenType.BracketClose);

                param.NativeType = new NativePointer(param.NativeType);
            }

            param.SalAttribute = sal;
            return param;
        }

        /// <summary>
        /// Read a constant R-Value from the token stream
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private string ProcessConstantValue()
        {

            // First check and see if this is a simple single token value
            string value = string.Empty;
            bool done = false;
            do
            {
                Token token = scanner.PeekNextToken();
                if (token.TokenType == TokenType.Comma || token.TokenType == TokenType.BraceClose)
                {
                    done = true;
                }
                else
                {
                    scanner.GetNextToken();
                    value += token.Value;
                }
            } while (!(done));

            return value;
        }


        /// <summary>
        /// At the end of a C struct is a set of words that are type defs of the 
        /// struct name
        /// 
        /// struct s1
        /// {
        ///   int i;
        /// } foo;
        ///  
        /// This will create a struct named "s1" and a typedef "foo" to that 
        /// struct
        /// </summary>
        /// <remarks></remarks>
        private List<NativeTypeDef> ProcessTypePostTypeDefs(NativeType originalNt)
        {
            bool done = false;
            List<NativeTypeDef> list = new List<NativeTypeDef>();

            try
            {
                do
                {
                    if (scanner.EndOfStream)
                    {
                        break;
                    }

                    Token token = scanner.PeekNextToken();
                    switch (token.TokenType)
                    {
                        case TokenType.Semicolon:
                        case TokenType.NewLine:
                            // Terminating conditions
                            done = true;
                            break;
                        case TokenType.Comma:
                            // Delimiter between the type names.  Ignore it
                            scanner.GetNextToken();
                            break;
                        default:
                            NativeTypeDef ntDef = ProcessTypePostTypedefSingle(originalNt);
                            if (ntDef != null)
                            {
                                list.Add(ntDef);
                            }
                            break;
                    }

                } while (!(done));
            }
            catch (ParseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error processing post typedef types for {0}: {1}", originalNt.Name, ex.Message);
                throw ParseException.CreateError(msg, ex);
            }

            return list;
        }

        private NativeTypeDef ProcessTypePostTypedefSingle(NativeType nt)
        {

            // Get the modifiers.  After this we will have a complete type name
            nt = ProcessTypeNameModifiers(nt);
            string name = null;

            // Strip any call modifiers
            TriState<NativeCallingConvention> callmod = new TriState<NativeCallingConvention>();
            ProcessCalltypeModifier(ref callmod);

            Token peekToken = scanner.PeekNextToken();

            if (peekToken.TokenType == TokenType.ParenOpen)
            {
                // Syntax for a function pointer typedef
                nt = ProcessFunctionPointer(string.Empty, nt);
                name = nt.Name;

            }
            else if (peekToken.TokenType == TokenType.Word)
            {
                // Standard typedef
                name = scanner.GetNextToken(TokenType.Word).Value;

                // The newer function pointer syntax allows you to forgo the parens and *
                if (scanner.PeekNextToken().TokenType == TokenType.ParenOpen)
                {
                    nt = ProcessFunctionPointerParameters(name, nt, new NativeSalAttribute(), callmod);
                }
            }
            else if (peekToken.IsTypeKeyword)
            {
                // Ignore this typedef.  Some parts of the windows header files attempt to typedef out
                // certain items we consider kewords.
                scanner.GetNextToken();
                return null;

            }
            else
            {
                // Unknown
                throw ParseException.CreateError("Error processing typedef list.  Expected word or paren open but found '{0}'.", scanner.PeekNextToken().Value);
            }

            // Now that we've processed out the type, we need to once again process modifiers because
            // it could be followed by an array suffix of sorts
            nt = ProcessTypeNameModifiers(nt);
            NativeTypeDef ntDef = new NativeTypeDef(name, nt);
            ProcessParsedTypeDef(ntDef);
            return ntDef;
        }

        /// <summary>
        /// Process a list of members for a type (structs, unions, etc).  Essentially
        /// any list of member's that are separated by semicolons
        /// </summary>
        /// <remarks></remarks>
        private List<NativeMember> ProcessTypeMemberList(string parentTypeName)
        {
            List<NativeMember> list = new List<NativeMember>();
            Token token = scanner.PeekNextToken();
            if (token.TokenType == TokenType.BraceClose)
            {
                // Empty struct
                return list;
            }

            bool done = false;
            do
            {
                ProcessAccessModifiers();

                NativeMember member = ProcessTypeMember(parentTypeName, list.Count);
                list.Add(member);

                // Get the end token.  Process any comma seperated list of members
                Token endToken = scanner.GetNextToken();
                while (endToken.TokenType == TokenType.Comma)
                {
                    list.Add(ProcessNativeMemberWithType(parentTypeName, list.Count, member.NativeType));
                    endToken = scanner.GetNextToken();
                }

                if (endToken.TokenType == TokenType.ParenOpen)
                {
                    // Member function.  Consume the remainder of the function and report an error
                    list.Remove(member);
                    ProcessBlockRemainder(TokenType.ParenOpen, TokenType.ParenClose);

                    // Remove the const qualifier if present
                    if (!scanner.EndOfStream && scanner.PeekNextToken().TokenType == TokenType.ConstKeyword)
                    {
                        scanner.GetNextToken();
                    }

                    // Remave an inline definition
                    if (!scanner.EndOfStream && scanner.PeekNextToken().TokenType == TokenType.BraceOpen)
                    {
                        ProcessBlock(TokenType.BraceOpen, TokenType.BraceClose);
                    }

                    if (!scanner.EndOfStream && scanner.PeekNextToken().TokenType == TokenType.Semicolon)
                    {
                        scanner.GetNextToken();
                    }

                    // This is not a fatal parse problem.  Simply add a warning and continue with 
                    // the rest of the members
                    errorProvider.AddWarning("Type member procedures are not supported: {0}.{1}", parentTypeName, member.Name);
                }
                else
                {
                    if (endToken.TokenType != TokenType.Semicolon)
                    {
                        throw ParseException.CreateError("Expected ; after member {0} in {1} but found {2}", member.Name, parentTypeName, endToken.Value);
                    }
                }

                // See if the next token is a close brace
                if (scanner.PeekNextToken().TokenType == TokenType.BraceClose)
                {
                    done = true;
                }

            } while (!(done));

            return list;
        }

        /// <summary>
        /// Process a type name pair from the scanner
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeMember ProcessTypeMember(string parentTypeName, int index)
        {
            NativeType nt = default(NativeType);
            try
            {
                // TODO: Support SAL attributes on structure members
                NativeSalAttribute sal = ProcessSalAttribute();
                nt = ProcessTypeNameOrType(parentTypeName + "_");
            }
            catch (ParseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string msg = null;
                if (!string.IsNullOrEmpty(parentTypeName))
                {
                    msg = string.Format("Error processing {0} member at index {1}: {2}", parentTypeName, index, ex.Message);
                }
                else
                {
                    msg = string.Format("Error processing member at index {1} around \"{0}\": {2}", this.PeekLineInformation(5), index, ex.Message);
                }
                throw ParseException.CreateError(msg, ex);
            }

            return ProcessNativeMemberWithType(parentTypeName, index, nt);
        }

        private NativeMember ProcessNativeMemberWithType(string parentTypeName, int index, NativeType nt)
        {
            Token nextToken = scanner.PeekNextToken();
            string name = null;

            if (nextToken.TokenType == TokenType.Word)
            {
                scanner.GetNextToken();
                name = nextToken.Value;
            }
            else
            {
                // For some reason, unions and structs can be defined with unnamed members.  
                name = string.Empty;
            }

            // Check for an array suffix on the type
            Token token = scanner.PeekNextToken();
            if (token.TokenType == TokenType.BracketOpen)
            {
                nt = ProcessArraySuffix(nt);
            }
            else if (token.TokenType == TokenType.Colon)
            {
                // This is a bitvector.  Read in the size and change the type of the 
                // member to be a proper bitvector
                scanner.GetNextToken();
                Number value;
                Token sizeToken = scanner.GetNextToken(TokenType.Number);
                if (!TokenHelper.TryConvertToNumber(sizeToken, out value))
                {
                    throw ParseException.CreateError("Expected number after bit vector specifier: {0}", sizeToken);
                }

                nt = new NativeBitVector(value.ConvertToInteger());
            }

            return new NativeMember(name, nt);
        }

        /// <summary>
        /// Read a type name from the stream
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeType ProcessTypeName()
        {

            // Remove type name precursors from the stream
            Token token = scanner.PeekNextToken();

            if (token.TokenType == TokenType.StructKeyword || token.TokenType == TokenType.UnionKeyword || token.TokenType == TokenType.EnumKeyword || token.TokenType == TokenType.ClassKeyword)
            {
                scanner.GetNextToken();
            }

            NativeType nt = ProcessShortTypeName();
            return ProcessTypeNameModifiers(nt);
        }

        private NativeDefinedType ProcessDefinedType(string namePrefix)
        {
            return ProcessDefinedTypeCore(namePrefix, true);
        }

        private NativeDefinedType ProcessDefinedTypeNoFunctionPointers(string namePrefix)
        {
            return ProcessDefinedTypeCore(namePrefix, false);
        }

        private NativeDefinedType ProcessDefinedTypeCore(string namePrefix, bool includeFunctionPointers)
        {
            ScannerMark mark = scanner.Mark();
            Token token = scanner.PeekNextToken();

            // Remove the SAL attribute if present
            if (token.TokenType == TokenType.DeclSpec)
            {
                ProcessSalAttribute();
            }

            if (token.TokenType == TokenType.StructKeyword)
            {
                scanner.GetNextToken();
                ProcessSalAttribute();

                // If the type name starts with struct there are one of
                // three possibilities.  
                //   1) Qualified name: struct foo
                //   2) Inline Type: struct { int bar; } 
                //   3) normal Struct: struct foo { int bar; }

                List<Token> peekList = scanner.PeekTokenList(2);

                if ((peekList[0].TokenType == TokenType.Word && peekList[1].TokenType == TokenType.BraceOpen) || peekList[0].TokenType == TokenType.BraceOpen)
                {
                    // If the struct is followed by any trailing typedefs then this function
                    // will take care of that as well
                    return this.ProcessStruct(namePrefix);
                }

            }
            else if (token.TokenType == TokenType.UnionKeyword)
            {
                scanner.GetNextToken();
                ProcessSalAttribute();

                List<Token> peekList = scanner.PeekTokenList(2);

                if ((peekList[0].TokenType == TokenType.Word && peekList[1].TokenType == TokenType.BraceOpen) || peekList[0].TokenType == TokenType.BraceOpen)
                {
                    return this.ProcessUnion(namePrefix);
                }

            }
            else if (token.TokenType == TokenType.EnumKeyword)
            {
                scanner.GetNextToken();
                ProcessSalAttribute();

                List<Token> peekList = scanner.PeekTokenList(2);

                if ((peekList[0].TokenType == TokenType.Word && peekList[1].TokenType == TokenType.BraceOpen) || peekList[0].TokenType == TokenType.BraceOpen)
                {
                    return this.ProcessEnum(namePrefix);
                }
            }
            else if (token.TokenType == TokenType.ClassKeyword)
            {
                scanner.GetNextToken();
                ProcessSalAttribute();

                // If the type name starts with Class there are one of
                // three possibilities.  
                //   1) Qualified name: Class foo
                //   2) Inline Type: Class { int bar; } 
                //   3) normal Class: Class foo { int bar; }

                List<Token> peekList = scanner.PeekTokenList(2);

                if ((peekList[0].TokenType == TokenType.Word && peekList[1].TokenType == TokenType.BraceOpen) || peekList[0].TokenType == TokenType.BraceOpen)
                {
                    // If the Class is followed by any trailing typedefs then this function
                    // will take care of that as well
                    return this.ProcessClass(namePrefix);
                }

            }
            else if (includeFunctionPointers)
            {
                // Last ditch effort is to parse out a function pointer
                ProcessSalAttribute();

                NativeType retType = ProcessTypeName();

                if (retType != null && scanner.PeekNextToken().TokenType == TokenType.ParenOpen)
                {
                    return this.ProcessFunctionPointer(namePrefix, retType);
                }
            }

            scanner.Rollback(mark);
            return null;
        }

        private NativeType ProcessTypeNameOrType()
        {
            return ProcessTypeNameOrType(string.Empty);
        }

        /// <summary>
        /// Process a type name, defined type or function pointer from the stream.
        /// </summary>
        /// <remarks></remarks>
        private NativeType ProcessTypeNameOrType(string namePrefix)
        {
            NativeDefinedType definedNt = ProcessDefinedType(namePrefix);
            if (definedNt != null)
            {
                return definedNt;
            }

            NativeType nt = ProcessShortTypeName();
            return ProcessTypeNameModifiers(nt);
        }

        /// <summary>
        /// Process a simple typename from the stream such as 
        ///  struct foo
        ///  int
        ///  unsigned int
        ///  signed int
        /// 
        /// Won't process type modifiers such as *,[] 
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeType ProcessShortTypeName()
        {
            bool isConst = false;
            Token qualifiedToken = scanner.PeekNextToken();
            if (qualifiedToken.TokenType == TokenType.ConstKeyword)
            {
                isConst = true;
                scanner.GetNextToken();
                qualifiedToken = scanner.PeekNextToken();
            }

            // Remove the volatile qualifier
            if (qualifiedToken.TokenType == TokenType.VolatileKeyword)
            {
                scanner.GetNextToken();
            }

            // Look for any type name qualifiers 
            if (qualifiedToken.TokenType == TokenType.StructKeyword || qualifiedToken.TokenType == TokenType.UnionKeyword || qualifiedToken.TokenType == TokenType.EnumKeyword || qualifiedToken.TokenType == TokenType.ClassKeyword)
            {
                scanner.GetNextToken();

                // It's possible to put a __declspec here.  Go ahead and remove it
                if (scanner.PeekNextToken().TokenType == TokenType.DeclSpec)
                {
                    ProcessSalAttribute();
                }
                return new NativeNamedType(qualifiedToken.Value, scanner.GetNextToken(TokenType.Word).Value);
            }

            // Down to simple types.  Look for any type prefixes
            NativeBuiltinType bt = null;
            Token token = scanner.GetNextToken();

            if (token.TokenType == TokenType.LongKeyword || token.TokenType == TokenType.SignedKeyword || token.TokenType == TokenType.UnsignedKeyword)
            {
                // If the next token is a builtin type keyword then these are modifiers of that
                // keyword
                if (scanner.PeekNextToken().IsTypeKeyword)
                {
                    NativeBuiltinType.TryConvertToBuiltinType(scanner.GetNextToken().TokenType, out bt);
                    bt.IsUnsigned = (token.TokenType == TokenType.UnsignedKeyword);
                }
                else
                {
                    NativeBuiltinType.TryConvertToBuiltinType(token.TokenType, out bt);
                }
            }
            else if (token.IsTypeKeyword)
            {
                NativeBuiltinType.TryConvertToBuiltinType(token.TokenType, out bt);
            }

            // If this is a builtin type and it's not constant then just return the builtin type.  Otherwise we 
            // have to return the named type since it holds the qualifier
            if (bt != null)
            {
                if (isConst)
                {
                    NativeNamedType named = new NativeNamedType(bt.Name, true);
                    named.RealType = bt;
                    return named;
                }
                else
                {
                    return bt;
                }
            }
            else
            {
                // It's not a builtin type.  Return the name
                return new NativeNamedType(token.Value, isConst);
            }
        }

        /// <summary>
        /// Processes the modiers, pointer, array and so on for a native type.  Scanner should be positioned
        /// right after the start of the short type name
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeType ProcessTypeNameModifiers(NativeType nt)
        {
            bool done = false;
            do
            {
                Token token = scanner.PeekNextToken();
                switch (token.TokenType)
                {
                    case TokenType.Asterisk:
                        // Wrap it in a pointer and eat the token
                        nt = new NativePointer(nt);
                        scanner.GetNextToken();

                        // Handle typeName * const name.
                        if (scanner.PeekNextToken().TokenType == TokenType.ConstKeyword)
                        {
                            scanner.GetNextToken();
                        }
                        break;
                    case TokenType.BracketOpen:
                        // Wrap it in an array.  Processing the array suffix will
                        // remove the tokens from the stream
                        nt = ProcessArraySuffix(nt);
                        break;
                    case TokenType.Word:
                        // Done once we hit the next word
                        done = true;
                        break;
                    case TokenType.ConstKeyword:
                        // If the const modifier proceeds a pointer then allow the pointer to 
                        // be processed.  Otherwise we are done
                        scanner.GetNextToken();
                        if (scanner.PeekNextToken().TokenType != TokenType.Asterisk)
                        {
                            done = true;
                        }
                        break;
                    case TokenType.VolatileKeyword:
                        // Igore the volatile qualifier
                        scanner.GetNextToken();
                        break;
                    case TokenType.Pointer32Keyword:
                    case TokenType.Pointer64Keyword:
                        // Ignore the pointer modifiers
                        scanner.GetNextToken();
                        break;
                    case TokenType.ParenOpen:
                        // Hit a function pointer inside the parameter list.  Type name is completed
                        done = true;
                        break;
                    default:
                        done = true;
                        break;
                }
            } while (!(done));

            return nt;
        }

        /// <summary>
        /// Process the type parsed out of the stream
        /// </summary>
        /// <param name="nt"></param>
        /// <remarks></remarks>
        private void ProcessParsedDefinedType(NativeDefinedType nt)
        {
            ThrowIfNull(nt);

            // It's possible for members of a defined type to not have a name.  Go ahead and add that
            // name now
            int count = 1;
            foreach (NativeMember mem in nt.Members)
            {
                if (string.IsNullOrEmpty(mem.Name))
                {
                    string prefix = "AnonymousMember";
                    if (mem.NativeTypeDigged != null)
                    {
                        switch (mem.NativeTypeDigged.Kind)
                        {
                            case NativeSymbolKind.UnionType:
                                prefix = "Union";
                                break;
                            case NativeSymbolKind.StructType:
                                prefix = "Struct";
                                break;
                        }
                    }

                    mem.Name = string.Format("{0}{1}", prefix, count);
                    count += 1;
                }
            }

            resultant.NativeDefinedTypes.Add(nt);
            resultant.ParsedTypes.Add(nt);
        }

        private void ProcessParsedTypeDef(NativeTypeDef typeDef)
        {
            ThrowIfNull(typeDef);

            resultant.NativeTypeDefs.Add(typeDef);
            resultant.ParsedTypes.Add(typeDef);
        }

        private void ProcessParsedProcedure(NativeProcedure proc)
        {
            ThrowIfNull(proc);
            resultant.NativeProcedures.Add(proc);
        }

        /// <summary>
        /// Called after a type is parsed out but an open brace is detected.  Will parse
        /// out the array definition
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeArray ProcessArraySuffix(NativeType nt)
        {
            // Create the array
            NativeArray ntArray = new NativeArray();
            ntArray.RealType = nt;
            ntArray.ElementCount = 0;

            bool anyUnbound = false;
            bool done = false;
            while (!done)
            {
                object count = null;

                // Move past the opening [
                Token token = scanner.GetNextToken();
                if (token.TokenType == TokenType.BracketOpen)
                {
                    token = scanner.GetNextToken();
                }

                // If it's a number then it's the rank of the array

                if ((token.TokenType == TokenType.Number || token.TokenType == TokenType.HexNumber) && scanner.PeekNextToken().TokenType == TokenType.BracketClose)
                {
                    Number number;
                    if (!TokenHelper.TryConvertToNumber(token, out number))
                    {
                        throw ParseException.CreateError("Could not process array length as number: {0}", token.Value);
                    }

                    // The token should now be the closing bracket.  
                    count = number.ConvertToInteger();
                    token = scanner.GetNextToken(TokenType.BracketClose);
                }
                else if (token.TokenType == TokenType.BracketClose)
                {
                    count = null;
                }
                else
                {
                    // Get the text up until the bracket and evaluate it as an expression.  Handles cases
                    // where we end up with (1+2) for [] lengths
                    List<Token> exprList = new List<Token>();
                    exprList.Add(token);

                    Token nextToken = scanner.GetNextToken();
                    while (nextToken.TokenType != TokenType.BracketClose)
                    {
                        exprList.Add(nextToken);
                        nextToken = scanner.GetNextToken();
                    }

                    ExpressionEvaluator ee = new ExpressionEvaluator();
                    ExpressionValue result = null;
                    if (ee.TryEvaluate(exprList, out result) && result.Kind != ExpressionValueKind.String)
                    {
                        count = result.ConvertToInteger();
                    }
                    else
                    {
                        count = null;
                    }
                }

                if (count == null)
                {
                    anyUnbound = true;
                }
                else
                {
                    if (ntArray.ElementCount == 0)
                    {
                        ntArray.ElementCount = Convert.ToInt32(count);
                    }
                    else
                    {
                        ntArray.ElementCount *= Convert.ToInt32(count);
                    }
                }

                if (scanner.PeekNextToken().TokenType != TokenType.BracketOpen)
                {
                    done = true;
                }
            }

            if (anyUnbound)
            {
                ntArray.ElementCount = -1;
            }

            return ntArray;
        }

        /// <summary>
        /// Read a SAL attribute from code.  They come in the following form
        ///   __declspec("directive")
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeSalAttribute ProcessSalAttribute()
        {
            Token token = scanner.PeekNextToken();
            NativeSalAttribute sal = new NativeSalAttribute();
            if (token.TokenType != TokenType.DeclSpec)
            {
                return sal;
            }

            bool done = false;
            do
            {
                scanner.GetNextToken();
                scanner.GetNextToken(TokenType.ParenOpen);
                Token directive = scanner.GetNextToken();

                // It's legal for the SAL attribute to be custom defined and as such
                // we should process the argument
                if (!directive.IsQuotedString)
                {
                    int depth = 0;
                    string text = directive.Value;
                    while (depth > 0 || scanner.PeekNextToken().TokenType != TokenType.ParenClose)
                    {
                        Token cur = scanner.GetNextToken();
                        text += cur.Value;
                        switch (cur.TokenType)
                        {
                            case TokenType.ParenOpen:
                                depth += 1;
                                break;
                            case TokenType.ParenClose:
                                depth -= 1;
                                break;
                        }
                    }
                    directive = new Token(TokenType.Text, text);
                }

                NativeSalEntry entry = ConvertSalDirectiveToEntry(directive.Value);
                if (entry != null)
                {
                    sal.SalEntryList.Add(entry);
                }

                // Get the close paren
                scanner.GetNextToken();

                // See if there are more declarations
                if (scanner.PeekNextToken().TokenType != TokenType.DeclSpec)
                {
                    done = true;
                }
            } while (!(done));

            return sal;
        }

        /// <summary>
        /// Process through a block of code ignoring all of the data inside the block.  Function should
        /// be called with the next token being the first { in the block
        /// </summary>
        /// <remarks></remarks>
        private List<Token> ProcessBlock(TokenType openType, TokenType closeType)
        {
            List<Token> list = new List<Token>();
            list.Add(scanner.GetNextToken(openType));
            return ProcessBlockRemainderCore(list, openType, closeType);
        }

        private void ProcessBlockRemainder(TokenType openType, TokenType closeType)
        {
            List<Token> list = new List<Token>();
            ProcessBlockRemainderCore(list, openType, closeType);
        }

        private List<Token> ProcessBlockRemainderCore(List<Token> list, TokenType openType, TokenType closeType)
        {
            int depth = 1;
            do
            {
                if (scanner.EndOfStream)
                {
                    throw ParseException.CreateError("Encountered end of stream while attempting to process a block");
                }

                Token nextToken = scanner.GetNextToken();
                list.Add(nextToken);
                if (nextToken.TokenType == openType)
                {
                    depth += 1;
                }
                else if (nextToken.TokenType == closeType)
                {
                    depth -= 1;
                }
            } while (!(depth == 0));

            return list;
        }


        /// <summary>
        /// Convert the sal directive to an entry
        /// </summary>
        /// <param name="directive"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private NativeSalEntry ConvertSalDirectiveToEntry(string directive)
        {

            // Remove the begining and ending quotes
            if (string.IsNullOrEmpty(directive))
            {
                return null;
            }

            if (directive[0] == '"' && directive[directive.Length - 1] == '"')
            {
                directive = directive.Substring(1, directive.Length - 2);
            }

            // If there is a ( then we need to process the inner text
            string text = string.Empty;
            SalEntryType entry = default(SalEntryType);
            Int32 index = directive.LastIndexOf('(');

            if (index >= 0)
            {
                // Find the inner data
                Int32 otherIndex = directive.IndexOf(')');
                if (otherIndex < 0 || index + 1 > otherIndex)
                {
                    return null;
                }

                text = directive.Substring(index + 1, otherIndex - (index + 1));
                directive = directive.Substring(0, index + 1) + directive.Substring(otherIndex);
            }

            if (!_salTable.TryGetValue(directive, out entry))
            {
                return null;
            }

            return new NativeSalEntry(entry, text);
        }

        /// <summary>
        /// After normal token processing occurs in the global scope this function is called
        /// to see if we hit any unsupported scenarios
        /// </summary>
        /// <remarks></remarks>
        private void ProcessGlobalTokenForUnsupportedScenario()
        {
            Token token = scanner.PeekNextToken();
            switch (token.TokenType)
            {
                case TokenType.BracketOpen:
                    List<Token> list = ProcessBlock(TokenType.BracketOpen, TokenType.BracketClose);
                    string msg = string.Format("C++ attributes are not supported: {0}", TokenHelper.TokenListToString(list));
                    throw ParseException.CreateWarningStreamOk(msg);
            }
        }

        #region "GetTokenHelpers"

        /// <summary>
        /// Eat tokens until we hit the end of stream or line.  Consumes the EndOfLine token
        /// </summary>
        /// <remarks></remarks>
        private void ChewThroughEndOfLine()
        {
            bool done = false;

            bool prevOpt = scanner.Options.HideNewLines;
            try
            {
                scanner.Options.HideNewLines = false;
                while (!done)
                {
                    if (scanner.EndOfStream)
                    {
                        done = true;
                    }
                    else
                    {
                        Token token = scanner.GetNextToken();
                        if (token.TokenType == TokenType.NewLine)
                        {
                            done = true;
                        }
                    }
                }
            }
            finally
            {
                scanner.Options.HideNewLines = prevOpt;
            }
        }


        private string PeekLineInformation(int count)
        {
            ScannerMark mark = scanner.Mark();
            bool old = scanner.Options.HideWhitespace;
            try
            {
                scanner.Options.HideWhitespace = false;

                var b = new StringBuilder();
                int found = 0;

                while (found < count)
                {
                    if (scanner.EndOfStream)
                    {
                        break; // TODO: might not be correct. Was : Exit While
                    }

                    Token cur = scanner.GetNextToken();
                    b.Append(cur.Value);
                    if (TokenType.WhiteSpace != cur.TokenType)
                    {
                        found += 1;
                    }
                }

                return b.ToString();
            }
            finally
            {
                scanner.Options.HideWhitespace = old;
                scanner.Rollback(mark);
            }
        }

        #endregion

    }
}
