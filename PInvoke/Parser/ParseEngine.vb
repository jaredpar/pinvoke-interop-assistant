' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports PInvoke.Contract


Namespace Parser

    ''' <summary>
    ''' Result of a parse operation
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ParseResult
        Private _errorProvider As New ErrorProvider
        Private _bag As New NativeSymbolBag
        Private _definedList As New List(Of NativeDefinedType)
        Private _typedefList As New List(Of NativeTypeDef)
        Private _procList As New List(Of NativeProcedure)
        Private _parsedList As New List(Of NativeType)

        Public Sub New()

        End Sub

        ''' <summary>
        ''' Contains error and warning information from the Parse
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ErrorProvider() As ErrorProvider
            Get
                Return _errorProvider
            End Get
        End Property

        ''' <summary>
        ''' List of NativeDefinedTypes encountered during the parse
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NativeDefinedTypes() As List(Of NativeDefinedType)
            Get
                Return _definedList
            End Get
        End Property

        ''' <summary>
        ''' List of NativeTypedef instances encounterd during the parse
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NativeTypedefs() As List(Of NativeTypeDef)
            Get
                Return _typedefList
            End Get
        End Property

        ''' <summary>
        ''' List of NativeProcedure instances encountered during the parse
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NativeProcedures() As List(Of NativeProcedure)
            Get
                Return _procList
            End Get
        End Property

        ''' <summary>
        ''' Flat list of types parsed out of the file
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ParsedTypes() As List(Of NativeType)
            Get
                Return _parsedList
            End Get
        End Property

    End Class

    Public Class ParseException
        Inherits Exception
        Private _isError As Boolean = True
        Private _isStreamOk As Boolean

        Friend ReadOnly Property IsError() As Boolean
            Get
                Return _isError
            End Get
        End Property

        Friend ReadOnly Property IsStreamOk() As Boolean
            Get
                Return _isStreamOk
            End Get
        End Property

        Private Sub New(ByVal msg As String, ByVal isError As Boolean)
            MyBase.New(msg)
            _isError = isError
        End Sub

        Private Sub New(ByVal msg As String, ByVal inner As Exception)
            MyBase.New(msg, inner)
        End Sub

        Public Shared Function CreateError(ByVal msg As String) As ParseException
            Return New ParseException(msg, True)
        End Function

        Public Shared Function CreateError(ByVal format As String, ByVal ParamArray args As Object()) As ParseException
            Return CreateError(String.Format(format, args))
        End Function

        Public Shared Function CreateError(ByVal msg As String, ByVal inner As Exception) As ParseException
            Return New ParseException(msg, inner)
        End Function

        ''' <summary>
        ''' Warning where the stream is in a bad positition
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateWarning(ByVal msg As String) As ParseException
            Return New ParseException(msg, False)
        End Function

        Public Shared Function CreateWarning(ByVal format As String, ByVal ParamArray args As Object()) As ParseException
            Return CreateWarning(String.Format(format, args))
        End Function

        ''' <summary>
        ''' Warning where the stream is properly set past the problem 
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateWarningStreamOk(ByVal msg As String) As ParseException
            Dim ex As ParseException = CreateWarning(msg)
            ex._isStreamOk = True
            Return ex
        End Function

        Public Shared Function CreateWarningStreamOk(ByVal format As String, ByVal ParamArray args As Object()) As ParseException
            Return CreateWarningStreamOk(String.Format(format, args))
        End Function

        Public Shared Function FoundEndOfStream() As ParseException
            Return ParseException.CreateError("Unexpected end of stream encountered")
        End Function
    End Class

    ''' <summary>
    ''' Parses out Native Code to find the types, macros, typedefs and functions we are
    ''' interested in.  It does not do any type resolution nor does it attempt to do 
    ''' any sort of macro processing 
    ''' </summary>
    ''' <remarks></remarks>
    <DebuggerDisplay("{DisplayString}")>
    Public Class ParseEngine

#Region "ParseEngineException"
        Private Class ParseEngineException
            Inherits Exception

            Public Sub New(ByVal msg As String)
                MyBase.New(msg)
            End Sub

            Public Sub New(ByVal msg As String, ByVal inner As Exception)
                MyBase.New(msg, inner)
            End Sub
        End Class

#End Region

        Private _parsing As Boolean
        Private _scanner As Scanner
        Private _result As ParseResult
        Private _errorProvider As New ErrorProvider
        Private _salTable As New Dictionary(Of String, SalEntryType)(StringComparer.OrdinalIgnoreCase)

        Private ReadOnly Property DisplayString() As String
            Get
                Return "ParseEngine: " & Me.PeekLineInformation(20)
            End Get
        End Property

        ''' <summary>
        ''' Create a new Parser
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            BuildLookupTables()
        End Sub

        Private Sub BuildLookupTables()

            ' Build the SAL table 
            For Each e As SalEntryType In System.Enum.GetValues(GetType(SalEntryType))
                _salTable.Add(
                 NativeSalEntry.GetDirectiveForEntry(e),
                 e)
            Next
        End Sub

        Public Function Parse(reader As TextReader) As ParseResult
            Return ParseCore(New TextReaderBag(reader))
        End Function

        Public Function Parse(readerbag As TextReaderBag) As ParseResult
            Return ParseCore(readerbag)
        End Function

        Public Function Parse(text As String) As ParseResult
            Dim bytes = Encoding.UTF8.GetBytes(text)
            Dim stream As New MemoryStream(bytes)
            Return Parse(New StreamReader(stream))
        End Function

        Private Function ParseCore(ByVal readerBag As TextReaderBag) As ParseResult
            ThrowIfNull(readerBag)
            ThrowIfTrue(_parsing, "Recursive parsing is not supported.  Instead create a new Parser")
            Dim toReturn As ParseResult = Nothing

            Try
                ' Build the options
                Dim opts As New ScannerOptions()
                opts.ThrowOnEndOfStream = True
                opts.HideWhitespace = True
                opts.HideNewLines = True
                opts.HideComments = True

                _parsing = True
                _result = New ParseResult()
                _scanner = New Scanner(readerBag, opts)
                _scanner.ErrorProvider = _result.ErrorProvider

                ' Actually do the parsing
                ParseCoreRoutine()

                _result.ErrorProvider.Append(_errorProvider)
                toReturn = _result
            Finally
                _scanner = Nothing
                _parsing = False
                _result = Nothing
            End Try

            Return toReturn
        End Function

        ''' <summary>
        ''' Core Parsing loop
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ParseCoreRoutine()

            ' Since the parser will constantly retry the same operation after moving the scanner 
            ' just a bit we will often get the same error message multiple times.  To prevent giving 
            ' the user this error message a ton of times will keep a table to ensure it doesn't happen
            Dim parseErrorTable As New Dictionary(Of String, Object)

            Dim done As Boolean = False
            While Not done

                ' Check for the end of the stream
                If _scanner.EndOfStream Then
                    done = True
                    Continue While
                End If

                ' Setup a mark.  If the routine fails to parse we want to rollback the scanner
                ' and read past the troublesome line
                Dim mark As ScannerMark = _scanner.Mark()

                Dim token As Token = _scanner.PeekNextToken()
                Try
                    Dim ntSal As New NativeSalAttribute()
                    If token.TokenType = TokenType.DeclSpec Then
                        ntSal = ProcessSalAttribute()
                        token = _scanner.PeekNextToken()
                    End If

                    If token.TokenType = TokenType.TypedefKeyword Then
                        ProcessTypeDef()
                    ElseIf TokenHelper.IsCallTypeModifier(token.TokenType) Then
                        ProcessProcedure()
                    ElseIf token.IsAnyWord Then

                        ' Next try and process a type
                        Dim parsedType As NativeType = ProcessTypeNameOrType()
                        Dim nextToken As Token = _scanner.PeekNextToken()
                        If parsedType.Category = NativeSymbolCategory.Defined Then

                            ' If the next token is a semicolon we are done with this type 
                            If nextToken.TokenType = TokenType.Semicolon Then
                                Continue While
                            End If
                        End If

                        If nextToken.TokenType = TokenType.Word OrElse TokenHelper.IsCallTypeModifier(nextToken.TokenType) Then
                            ProcessProcedure(parsedType, ntSal, New TriState(Of NativeCallingConvention))
                        ElseIf nextToken.TokenType = TokenType.ParenOpen Then
                            ProcessFunctionPointer(String.Empty, parsedType, ntSal)
                        End If
                    Else
                        ProcessGlobalTokenForUnsupportedScenario()
                        _scanner.GetNextToken()
                    End If
                Catch ex As ParseException
                    If Not parseErrorTable.ContainsKey(ex.Message) Then
                        parseErrorTable.Add(ex.Message, Nothing)
                        If ex.IsError Then
                            _errorProvider.AddError(ex.Message)
                        Else
                            _errorProvider.AddWarning(ex.Message)
                        End If

                        ' If the thrower did not put the stream in a good place chew
                        ' through this line
                        If Not ex.IsStreamOk Then
                            ChewThroughEndOfLine()
                        End If
                    End If
                Catch ex As EndOfStreamException

                    ' Rollback the scanner and process the next line
                    _errorProvider.AddError("Unexpectedly hit the end of the stream")
                    _scanner.Rollback(mark)
                    ChewThroughEndOfLine()

                Catch ex As Exception

                    ' Rollback the scanner.  The process through this line
                    _errorProvider.AddError(ex.Message)
                    _scanner.Rollback(mark)
                    ChewThroughEndOfLine()
                End Try
            End While
        End Sub

        ''' <summary>
        ''' Process a type definition from code
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessTypeDef() As List(Of NativeTypeDef)

            ' Chew through the typedef token if it hasn't been consumed
            Dim token As Token = _scanner.PeekNextToken()
            If token.TokenType = TokenType.TypedefKeyword Then
                _scanner.GetNextToken()
            End If

            Dim sal As NativeSalAttribute
            Dim source As NativeType
            Dim typeMark As ScannerMark = _scanner.Mark()
            Try
                ' Get the type name which is the source of the typedef.  This can only 
                ' be a defined type or a type name.  Also since this could still be 
                ' a function pointer, read the possible return type sal attribute
                sal = Me.ProcessSalAttribute()
                Dim definedNt As NativeDefinedType = ProcessDefinedTypeNoFunctionPointers(String.Empty)
                If definedNt IsNot Nothing Then
                    source = definedNt
                Else
                    source = ProcessShortTypeName()
                End If
            Catch ex As ParseException
                Throw
            Catch ex As Exception
                _scanner.Rollback(typeMark)
                Dim msg As String = String.Format( _
                    "Error processing typedef ""{0}"": {1}", _
                    PeekLineInformation(4), _
                    ex.Message)
                Throw ParseException.CreateError(msg, ex)
            End Try

            ' Now just process the post members
            Return Me.ProcessTypePostTypedefs(source)
        End Function

        Private Function ProcessClass() As NativeStruct
            Return ProcessClass(String.Empty)
        End Function

        ''' <summary>
        ''' Called when we encounter a class token.  Treat this just like a struct 
        ''' </summary>
        ''' <param name="nameprefix"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessClass(ByVal nameprefix As String) As NativeStruct
            Contract.ThrowIfNull(nameprefix)

            ' If this called with a struct token still in the stream, remove it
            If _scanner.PeekNextToken().TokenType = TokenType.ClassKeyword Then
                _scanner.GetNextToken()
            End If

            Return ProcessStruct(nameprefix)
        End Function

        Private Function ProcessStruct() As NativeStruct
            Return ProcessStruct(String.Empty)
        End Function

        ''' <summary>
        ''' Called when we encounter a struct token.  
        ''' 
        ''' struct foo 
        ''' {
        '''   int i;
        '''   boolean j
        ''' };
        ''' </summary>
        ''' <remarks></remarks>
        Private Function ProcessStruct(ByVal namePrefix As String) As NativeStruct
            Contract.ThrowIfNull(namePrefix)

            ' If this called with a struct token still in the stream, remove it
            If _scanner.PeekNextToken().TokenType = TokenType.StructKeyword Then
                _scanner.GetNextToken()
            End If

            ' Remove any SAL attribute
            ' TODO: It may be worthwhile in a future version to add support for the __declspec attribute
            If _scanner.PeekNextToken().TokenType = TokenType.DeclSpec Then
                ProcessSalAttribute()
            End If

            ' Check and see if the next token is a word.  If so then it's a named 
            ' struct and otherwise it's inline
            Dim name As String
            Dim isInline As Boolean
            Dim nameToken As Token = _scanner.PeekNextToken()
            If nameToken.TokenType = TokenType.Word Then
                _scanner.GetNextToken()
                name = namePrefix & nameToken.Value
                isInline = False
            Else
                name = String.Empty
                isInline = True
            End If

            ' For forward declaration structs the next token will be a ';'.  There is nothing 
            ' to add for structures of this type
            If _scanner.PeekNextToken().TokenType = TokenType.Semicolon Then
                Return Nothing
            End If

            ' Check through the open brace structure
            _scanner.GetNextToken(TokenType.BraceOpen)

            ' Get the members
            Dim list As List(Of NativeMember) = ProcessTypeMemberList(name)

            ' Move through the close brace
            _scanner.GetNextToken(TokenType.BraceClose)

            ' Create the struct type
            Dim ntStruct As New NativeStruct()
            ntStruct.Name = name
            ntStruct.Members.AddRange(list)

            ' If this is an inline type, make sure to mark it as anonymous
            If isInline Then
                ntStruct.IsAnonymous = True
            End If

            ' Process the type
            ProcessParsedDefinedType(ntStruct)

            ' If this is not an inline definition then it's possible to add typedefs immediately
            ' after the struct definition
            If Not isInline Then
                ProcessTypePostTypedefs(ntStruct)
            End If

            Return ntStruct
        End Function

        Private Function ProcessUnion() As NativeUnion
            Return ProcessUnion(String.Empty)
        End Function

        ''' <summary>
        ''' Process a union type member
        ''' </summary>
        ''' <remarks></remarks>
        Private Function ProcessUnion(ByVal namePrefix As String) As NativeUnion
            Contract.ThrowIfNull(namePrefix)

            ' Check through the union token if it hasn't been consumed
            Dim token As Token = _scanner.PeekNextToken()
            If token.TokenType = TokenType.UnionKeyword Then
                _scanner.GetNextToken()
            End If

            ' See if this is an inline union or a named one
            Dim isInline As Boolean = False
            Dim name As String = String.Empty
            token = _scanner.PeekNextToken()
            If token.TokenType = TokenType.Word Then
                name = namePrefix & token.Value
                _scanner.GetNextToken()
            Else
                isInline = True
            End If

            ' Get the open brace
            _scanner.GetNextToken(TokenType.BraceOpen)

            Dim list As List(Of NativeMember) = ProcessTypeMemberList(name)

            ' Get the close brace
            _scanner.GetNextToken(TokenType.BraceClose)

            ' Create the union
            Dim ntUnion As New NativeUnion()
            ntUnion.Name = name
            ntUnion.IsAnonymous = isInline
            ntUnion.Members.AddRange(list)
            ProcessParsedDefinedType(ntUnion)

            ' If this is not an inline type then process the post type defs
            If Not isInline Then
                ProcessTypePostTypedefs(ntUnion)
            End If

            Return ntUnion
        End Function

        Private Function ProcessEnum() As NativeEnum
            Return ProcessEnum(String.Empty)
        End Function

        ''' <summary>
        ''' Process out the enumeration
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessEnum(ByVal namePrefix As String) As NativeEnum
            Contract.ThrowIfNull(namePrefix)

            ' Move past the enum token if it's still in the stream
            Dim token As Token = _scanner.PeekNextToken()
            If token.TokenType = TokenType.EnumKeyword Then
                _scanner.GetNextToken()
            End If

            ' Check to see if this is an inline enum
            Dim isInline As Boolean = False
            Dim name As String = String.Empty
            token = _scanner.PeekNextToken()
            If token.TokenType = TokenType.Word Then
                _scanner.GetNextToken()
                isInline = False
                name = namePrefix & token.Value
            Else
                isInline = True
            End If

            ' Get the open brace
            _scanner.GetNextToken(TokenType.BraceOpen)

            Dim list As List(Of NativeEnumValue) = ProcessEnumValues()

            ' Get the close brace
            _scanner.GetNextToken(TokenType.BraceClose)

            ' Create the enumeration
            Dim ntEnum As New NativeEnum()
            ntEnum.Name = name
            ntEnum.IsAnonymous = isInline
            ntEnum.Values.AddRange(list)
            ProcessParsedDefinedType(ntEnum)

            ' If this isnot' an inline type then process the post type defs
            If Not isInline Then
                ProcessTypePostTypedefs(ntEnum)
            End If

            Return ntEnum
        End Function

        ''' <summary>
        ''' Read the list of enum values for an enum definition
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessEnumValues() As List(Of NativeEnumValue)
            Dim list As New List(Of NativeEnumValue)

            ' Allow for an empty enum list
            If _scanner.PeekNextToken().TokenType = TokenType.BraceClose Then
                Return list
            End If

            Dim done As Boolean = False
            While Not done
                Dim nameToken As Token = _scanner.GetNextToken(TokenType.Word)

                Dim token As Token = _scanner.PeekNextToken()
                Select Case token.TokenType
                    Case TokenType.Comma
                        _scanner.GetNextToken()
                        list.Add(New NativeEnumValue(nameToken.Value))
                    Case TokenType.BraceClose
                        list.Add(New NativeEnumValue(nameToken.Value))
                    Case TokenType.OpAssign
                        _scanner.GetNextToken()
                        Dim value As String = ProcessConstantValue()
                        If _scanner.PeekNextToken().TokenType = TokenType.Comma Then
                            _scanner.GetNextToken()
                        End If
                        list.Add(New NativeEnumValue(nameToken.Value, value))
                    Case Else
                        _scanner.AddWarning("Unexpected token while processing enum values: {0}", token.TokenType)
                        done = True
                End Select

                token = _scanner.PeekNextToken()
                If token.TokenType = TokenType.BraceClose Then
                    done = True
                End If
            End While

            Return list
        End Function

        Private Function ProcessProcedure() As NativeProcedure
            Dim mark As ScannerMark = _scanner.Mark()
            Dim callmod As New TriState(Of NativeCallingConvention)
            ProcessCalltypeModifier(callmod)

            ' Process the return type sal attribute
            Dim retTypeSal As NativeSalAttribute = ProcessSalAttribute()
            ProcessCalltypeModifier(callmod)
            Dim retType As NativeType = ProcessTypeNameOrType()
            If retType Is Nothing Then
                _scanner.Rollback(mark)
                Return Nothing
            End If

            Return ProcessProcedure(retType, retTypeSal, callmod)
        End Function

        ''' <summary>
        ''' Try and Parse out a procedure from the code.  Unlike enum, struct and union, there is 
        ''' no keyword before to say that we are about to be at one so we have to do a bit of guessing here 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessProcedure(ByVal retType As NativeType, ByVal retTypeSal As NativeSalAttribute, ByVal callmod As TriState(Of NativeCallingConvention)) As NativeProcedure
            ThrowIfNull(callmod)
            Dim mark As ScannerMark = _scanner.Mark()

            ProcessCalltypeModifier(callmod)
            Dim nameToken As Token = _scanner.PeekNextToken()
            If nameToken.TokenType <> TokenType.Word Then
                _scanner.Rollback(mark)
                Return Nothing
            End If
            _scanner.GetNextToken()

            Try
                Dim list As List(Of NativeParameter) = ProcessParameterList(nameToken.Value)
                If list Is Nothing Then
                    _scanner.Rollback(mark)
                    Return Nothing
                End If

                ' Create the signature
                Dim sig As New NativeSignature()
                sig.ReturnType = retType
                sig.ReturnTypeSalAttribute = retTypeSal
                sig.Parameters.AddRange(list)

                ' Create the procedure
                Dim proc As New NativeProcedure()
                proc.Name = nameToken.Value
                proc.Signature = sig

                ' Check to see if the procedure has an inline block declared after it.  If so then process
                ' the block away
                If Not _scanner.EndOfStream AndAlso _scanner.PeekNextToken().TokenType = TokenType.BraceOpen Then
                    ProcessBlock(TokenType.BraceOpen, TokenType.BraceClose)
                    Throw ParseException.CreateWarningStreamOk( _
                        "Ignoring Procedure {0} because it is defined inline.", _
                        proc.Name)
                End If

                ' If we found a calling convention for the procedure add it to the definition
                If callmod.HasValue Then
                    proc.CallingConvention = callmod.Value
                End If

                ' Add the procedure to the parsed list
                ProcessParsedProcedure(proc)
                Return proc
            Catch ex As ParseException
                Throw
            Catch ex As Exception
                Dim msg As String = String.Format("Error processing procedure {0}: {1}", nameToken.Value, ex.Message)
                Throw ParseException.CreateError(msg, ex)
            End Try

        End Function

        Private Sub ProcessCalltypeModifier(ByRef value As TriState(Of NativeCallingConvention))
            While TokenHelper.IsCallTypeModifier(_scanner.PeekNextToken().TokenType)
                Dim token As Token = _scanner.GetNextToken()
                Dim callmod As NativeCallingConvention
                Select Case token.TokenType
                    Case TokenType.WinApiCallKeyword
                        callmod = NativeCallingConvention.WinApi
                    Case TokenType.StandardCallKeyword
                        callmod = NativeCallingConvention.Standard
                    Case TokenType.CDeclarationCallKeyword
                        callmod = NativeCallingConvention.CDeclaration
                    Case TokenType.PascalCallKeyword
                        callmod = NativeCallingConvention.Pascal
                    Case TokenType.ClrCallKeyword
                        callmod = NativeCallingConvention.Clr
                    Case TokenType.InlineKeyword
                        callmod = NativeCallingConvention.Inline
                    Case Else
                        callmod = NativeCallingConvention.WinApi
                        InvalidEnumValue(token.TokenType)
                End Select

                value.SetValue(callmod)
            End While
        End Sub

        ''' <summary>
        ''' Process the access modifier lines that can be added to native structures.  We don't need
        ''' these for our generation story but we do need to parse them out 
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ProcessAccessModifiers()
            Dim token As Token = _scanner.PeekNextToken()
            While token.IsAccessModifier
                _scanner.GetNextToken()
                token = _scanner.PeekNextToken()
                If token.TokenType = TokenType.Colon Then
                    _scanner.GetNextToken()
                    token = _scanner.PeekNextToken()
                End If
            End While
        End Sub

        Private Function ProcessFunctionPointer(ByVal namePrefix As String, ByVal retType As NativeType) As NativeFunctionPointer
            Return ProcessFunctionPointer(namePrefix, retType, New NativeSalAttribute())
        End Function

        ''' <summary>
        ''' Process a function pointer in code
        ''' 
        ''' This function is called when the parser is immediately after the return type of the 
        ''' function pointer in the scanner
        ''' </summary>
        ''' <param name="retType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessFunctionPointer(ByVal namePrefix As String, ByVal retType As NativeType, ByVal retTypeSal As NativeSalAttribute) As NativeFunctionPointer
            ThrowIfNull(namePrefix)
            ThrowIfNull(retType)

            ' It's fine for this method to be called with the scanner either immediately before or 
            ' after the opening paren
            If _scanner.PeekNextToken().TokenType = TokenType.ParenOpen Then
                _scanner.GetNextToken()
            End If

            ' Remove the calling convention
            Dim callmod As New TriState(Of NativeCallingConvention)
            ProcessCalltypeModifier(callmod)

            ' If there is a * in the name then parse that as well
            If _scanner.PeekNextToken().TokenType = TokenType.Asterisk Then
                _scanner.GetNextToken()
            End If

            ' Get the acutal name from code.  Make sure to handle the anonymous function pointer case
            Dim name As String
            If _scanner.PeekNextToken().TokenType = TokenType.Word Then
                name = namePrefix & _scanner.GetNextToken().Value
            Else
                name = NativeSymbolBag.GenerateAnonymousName()
            End If

            _scanner.GetNextToken(TokenType.ParenClose)

            Return ProcessFunctionPointerParameters(name, retType, retTypeSal, callmod)
        End Function

        Private Function ProcessFunctionPointerParameters(ByVal name As String, ByVal retType As NativeType, ByVal retTypeSal As NativeSalAttribute, ByVal callmod As TriState(Of NativeCallingConvention)) As NativeFunctionPointer
            ' Now get the parameter list
            Dim list As List(Of NativeParameter) = Me.ProcessParameterList(name)
            If list Is Nothing Then
                Throw ParseException.CreateError("Error parsing parameters for function pointer {0}", name)
            End If

            Dim ptr As New NativeFunctionPointer(name)
            ptr.Signature.ReturnType = retType
            ptr.Signature.ReturnTypeSalAttribute = retTypeSal
            ptr.Signature.Parameters.AddRange(list)
            ptr.IsAnonymous = NativeSymbolBag.IsAnonymousName(name)

            If callmod.HasValue Then
                ptr.CallingConvention = callmod.Value
            End If

            ProcessParsedDefinedType(ptr)
            Return ptr
        End Function

        ''' <summary>
        ''' Process the list of parameters.  This can be for either a function pointer or a normal
        ''' procedure.  
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessParameterList(ByVal procName As String) As List(Of NativeParameter)
            Dim list As New List(Of NativeParameter)

            Dim token As Token = _scanner.GetNextToken()
            If token.TokenType <> TokenType.ParenOpen Then
                Return Nothing
            End If

            ' Check for the (void) signature
            Dim voidList As List(Of Token) = _scanner.PeekTokenList(2)
            If voidList(1).TokenType = TokenType.ParenClose _
                AndAlso voidList(0).TokenType = TokenType.VoidKeyword Then

                ' Get the tokens for the signature off of the stream
                _scanner.GetNextToken(TokenType.VoidKeyword)
                _scanner.GetNextToken(TokenType.ParenClose)
                Return list
            End If

            Do
                token = _scanner.PeekNextToken()
                If token.TokenType = TokenType.ParenClose Then
                    _scanner.GetNextToken()
                    Exit Do
                ElseIf token.TokenType = TokenType.Period Then
                    ' Check for variable arguments signature
                    Dim varList As List(Of Token) = _scanner.PeekTokenList(3)
                    If varList(1).TokenType = TokenType.Period AndAlso varList(2).TokenType = TokenType.Period Then
                        ProcessBlockRemainder(TokenType.ParenOpen, TokenType.ParenClose)

                        ' Make sure to remove the { if it is both variable and inline
                        If Not _scanner.EndOfStream AndAlso _scanner.PeekNextToken().TokenType = TokenType.BraceOpen Then
                            ProcessBlock(TokenType.BraceOpen, TokenType.BraceClose)
                        End If

                        Throw ParseException.CreateWarningStreamOk( _
                            "Procedure {0} has a variable argument signature which is unsupported.", _
                            procName)
                    End If
                End If

                ' Process the actual parameter
                list.Add(ProcessParameter())

                If _scanner.PeekNextToken().TokenType = TokenType.Comma Then
                    _scanner.GetNextToken()
                End If
            Loop

            Return list
        End Function

        Private Function ProcessParameter() As NativeParameter

            ' Process any sal attributes
            Dim sal As NativeSalAttribute = ProcessSalAttribute()

            Dim param As New NativeParameter()
            param.NativeType = ProcessTypeName()

            If _scanner.PeekNextToken().TokenType = TokenType.Word Then
                ' Match the name if it's present
                param.Name = _scanner.GetNextToken().Value
            ElseIf _scanner.PeekNextToken().TokenType = TokenType.ParenOpen Then

                ' It's legal to have an inline function pointer as a parameter type.  In that
                ' case though the parameter will have no name and will instead take the name of 
                ' the function pointer (if it's no anonymous)
                Dim fptr As NativeFunctionPointer = ProcessFunctionPointer(String.Empty, param.NativeType)
                param.NativeType = fptr
                If Not fptr.IsAnonymous Then
                    param.Name = fptr.Name
                    fptr.IsAnonymous = True
                    fptr.Name = NativeSymbolBag.GenerateAnonymousName()
                End If

            End If

            ' It's valid for the trailing [] to come after the parameter name and we
            ' need to process them here
            While _scanner.PeekNextToken().TokenType = TokenType.BracketOpen
                _scanner.GetNextToken()
                _scanner.GetNextToken(TokenType.BracketClose)

                param.NativeType = New NativePointer(param.NativeType)
            End While

            param.SalAttribute = sal
            Return param
        End Function

        ''' <summary>
        ''' Read a constant R-Value from the token stream
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessConstantValue() As String

            ' First check and see if this is a simple single token value
            Dim value As String = String.Empty
            Dim done As Boolean = False
            Do
                Dim token As Token = _scanner.PeekNextToken()
                If token.TokenType = TokenType.Comma _
                    OrElse token.TokenType = TokenType.BraceClose Then
                    done = True
                Else
                    _scanner.GetNextToken()
                    value &= token.Value
                End If
            Loop Until done

            Return value
        End Function


        ''' <summary>
        ''' At the end of a C struct is a set of words that are type defs of the 
        ''' struct name
        ''' 
        ''' struct s1
        ''' {
        '''   int i;
        ''' } foo;
        '''  
        ''' This will create a struct named "s1" and a typedef "foo" to that 
        ''' struct
        ''' </summary>
        ''' <remarks></remarks>
        Private Function ProcessTypePostTypedefs(ByVal originalNt As NativeType) As List(Of NativeTypeDef)
            Dim done As Boolean = False
            Dim list As New List(Of NativeTypeDef)

            Try
                Do
                    If _scanner.EndOfStream Then
                        Exit Do
                    End If

                    Dim token As Token = _scanner.PeekNextToken()
                    Select Case token.TokenType
                        Case TokenType.Semicolon, TokenType.NewLine
                            ' Terminating conditions
                            done = True
                        Case TokenType.Comma
                            ' Delimiter between the type names.  Ignore it
                            _scanner.GetNextToken()
                        Case Else
                            Dim ntDef As NativeTypeDef = ProcessTypePostTypedefSingle(originalNt)
                            If ntDef IsNot Nothing Then
                                list.Add(ntDef)
                            End If
                    End Select

                Loop Until done
            Catch ex As ParseException
                Throw
            Catch ex As Exception
                Dim msg As String = String.Format( _
                    "Error processing post typedef types for {0}: {1}", _
                    originalNt.Name, _
                    ex.Message)
                Throw ParseException.CreateError(msg, ex)
            End Try

            Return list
        End Function

        Private Function ProcessTypePostTypedefSingle(ByVal nt As NativeType) As NativeTypeDef

            ' Get the modifiers.  After this we will have a complete type name
            nt = ProcessTypeNameModifiers(nt)
            Dim name As String

            ' Strip any call modifiers
            Dim callmod As New TriState(Of NativeCallingConvention)
            ProcessCalltypeModifier(callmod)

            Dim peekToken As Token = _scanner.PeekNextToken()
            If peekToken.TokenType = TokenType.ParenOpen Then

                ' Syntax for a function pointer typedef
                nt = ProcessFunctionPointer(String.Empty, nt)
                name = nt.Name
            ElseIf peekToken.TokenType = TokenType.Word Then

                ' Standard typedef
                name = _scanner.GetNextToken(TokenType.Word).Value

                ' The newer function pointer syntax allows you to forgo the parens and *
                If _scanner.PeekNextToken().TokenType = TokenType.ParenOpen Then
                    nt = ProcessFunctionPointerParameters(name, nt, New NativeSalAttribute(), callmod)
                End If
            ElseIf peekToken.IsTypeKeyword Then
                ' Ignore this typedef.  Some parts of the windows header files attempt to typedef out
                ' certain items we consider kewords.
                _scanner.GetNextToken()
                Return Nothing
            Else

                ' Unknown
                Throw ParseException.CreateError( _
                    "Error processing typedef list.  Expected word or paren open but found '{0}'.", _
                    _scanner.PeekNextToken().Value)
            End If

            ' Now that we've processed out the type, we need to once again process modifiers because
            ' it could be followed by an array suffix of sorts
            nt = ProcessTypeNameModifiers(nt)
            Dim ntDef As New NativeTypeDef(name, nt)
            ProcessParsedTypeDef(ntDef)
            Return ntDef
        End Function

        ''' <summary>
        ''' Process a list of members for a type (structs, unions, etc).  Essentially
        ''' any list of member's that are separated by semicolons
        ''' </summary>
        ''' <remarks></remarks>
        Private Function ProcessTypeMemberList(ByVal parentTypeName As String) As List(Of NativeMember)
            Dim list As New List(Of NativeMember)
            Dim token As Token = _scanner.PeekNextToken()
            If token.TokenType = TokenType.BraceClose Then
                ' Empty struct
                Return list
            End If

            Dim done As Boolean = False
            Do
                ProcessAccessModifiers()

                Dim member As NativeMember = ProcessTypeMember(parentTypeName, list.Count)
                list.Add(member)

                ' Get the end token.  Process any comma seperated list of members
                Dim endToken As Token = _scanner.GetNextToken()
                While endToken.TokenType = TokenType.Comma
                    list.Add(ProcessNativeMemberWithType(parentTypeName, list.Count, member.NativeType))
                    endToken = _scanner.GetNextToken()
                End While

                If endToken.TokenType = TokenType.ParenOpen Then
                    ' Member function.  Consume the remainder of the function and report an error
                    list.Remove(member)
                    ProcessBlockRemainder(TokenType.ParenOpen, TokenType.ParenClose)

                    ' Remove the const qualifier if present
                    If Not _scanner.EndOfStream AndAlso _scanner.PeekNextToken().TokenType = TokenType.ConstKeyword Then
                        _scanner.GetNextToken()
                    End If

                    ' Remave an inline definition
                    If Not _scanner.EndOfStream AndAlso _scanner.PeekNextToken().TokenType = TokenType.BraceOpen Then
                        ProcessBlock(TokenType.BraceOpen, TokenType.BraceClose)
                    End If

                    If Not _scanner.EndOfStream AndAlso _scanner.PeekNextToken().TokenType = TokenType.Semicolon Then
                        _scanner.GetNextToken()
                    End If

                    ' This is not a fatal parse problem.  Simply add a warning and continue with 
                    ' the rest of the members
                    _errorProvider.AddWarning( _
                        "Type member procedures are not supported: {0}.{1}", _
                        parentTypeName, _
                        member.Name)
                Else
                    If endToken.TokenType <> TokenType.Semicolon Then
                        Throw ParseException.CreateError( _
                            "Expected ; after member {0} in {1} but found {2}", _
                            member.Name, _
                            parentTypeName, endToken.Value)
                    End If
                End If

                ' See if the next token is a close brace
                If _scanner.PeekNextToken().TokenType = TokenType.BraceClose Then
                    done = True
                End If

            Loop Until done

            Return list
        End Function

        ''' <summary>
        ''' Process a type name pair from the scanner
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessTypeMember(ByVal parentTypeName As String, ByVal index As Integer) As NativeMember
            Dim nt As NativeType
            Try
                ' TODO: Support SAL attributes on structure members
                Dim sal As NativeSalAttribute = ProcessSalAttribute()
                nt = ProcessTypeNameOrType(parentTypeName & "_")
            Catch ex As ParseException
                Throw
            Catch ex As Exception
                Dim msg As String
                If Not String.IsNullOrEmpty(parentTypeName) Then
                    msg = String.Format( _
                        "Error processing {0} member at index {1}: {2}", _
                            parentTypeName, _
                            index, _
                            ex.Message)
                Else
                    msg = String.Format( _
                       "Error processing member at index {1} around ""{0}"": {2}", _
                           Me.PeekLineInformation(5), _
                           index, _
                           ex.Message)
                End If
                Throw ParseException.CreateError(msg, ex)
            End Try

            Return ProcessNativeMemberWithType(parentTypeName, index, nt)
        End Function

        Private Function ProcessNativeMemberWithType(ByVal parentTypeName As String, ByVal index As Integer, ByVal nt As NativeType) As NativeMember
            Dim nextToken As Token = _scanner.PeekNextToken()
            Dim name As String

            If nextToken.TokenType = TokenType.Word Then
                _scanner.GetNextToken()
                name = nextToken.Value
            Else
                ' For some reason, unions and structs can be defined with unnamed members.  
                name = String.Empty
            End If

            ' Check for an array suffix on the type
            Dim token As Token = _scanner.PeekNextToken()
            If token.TokenType = TokenType.BracketOpen Then
                nt = ProcessArraySuffix(nt)
            ElseIf token.TokenType = TokenType.Colon Then
                ' This is a bitvector.  Read in the size and change the type of the 
                ' member to be a proper bitvector
                _scanner.GetNextToken()
                Dim value As Object = Nothing
                Dim sizeToken As Token = _scanner.GetNextToken(TokenType.Number)
                If Not TokenHelper.TryConvertToNumber(sizeToken, value) Then
                    Throw ParseException.CreateError( _
                        "Expected number after bit vector specifier: {0}", _
                        sizeToken)
                End If

                nt = New NativeBitVector(DirectCast(value, Int32))
            End If

            Return New NativeMember(name, nt)
        End Function

        ''' <summary>
        ''' Read a type name from the stream
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessTypeName() As NativeType

            ' Remove type name precursors from the stream
            Dim token As Token = _scanner.PeekNextToken()
            If token.TokenType = TokenType.StructKeyword _
                OrElse token.TokenType = TokenType.UnionKeyword _
                OrElse token.TokenType = TokenType.EnumKeyword _
                OrElse token.TokenType = TokenType.ClassKeyword Then

                _scanner.GetNextToken()
            End If

            Dim nt As NativeType = ProcessShortTypeName()
            Return ProcessTypeNameModifiers(nt)
        End Function

        Private Function ProcessDefinedType(ByVal namePrefix As String) As NativeDefinedType
            Return ProcessDefinedTypeCore(namePrefix, True)
        End Function

        Private Function ProcessDefinedTypeNoFunctionPointers(ByVal namePrefix As String) As NativeDefinedType
            Return ProcessDefinedTypeCore(namePrefix, False)
        End Function

        Private Function ProcessDefinedTypeCore(ByVal namePrefix As String, ByVal includeFunctionPointers As Boolean) As NativeDefinedType
            Dim mark As ScannerMark = _scanner.Mark()
            Dim token As Token = _scanner.PeekNextToken()

            ' Remove the SAL attribute if present
            If token.TokenType = TokenType.DeclSpec Then
                ProcessSalAttribute()
            End If

            If token.TokenType = TokenType.StructKeyword Then
                _scanner.GetNextToken()
                ProcessSalAttribute()

                ' If the type name starts with struct there are one of
                ' three possibilities.  
                '   1) Qualified name: struct foo
                '   2) Inline Type: struct { int bar; } 
                '   3) normal Struct: struct foo { int bar; }

                Dim peekList As List(Of Token) = _scanner.PeekTokenList(2)
                If (peekList(0).TokenType = TokenType.Word AndAlso peekList(1).TokenType = TokenType.BraceOpen) _
                    OrElse peekList(0).TokenType = TokenType.BraceOpen Then

                    ' If the struct is followed by any trailing typedefs then this function
                    ' will take care of that as well
                    Return Me.ProcessStruct(namePrefix)
                End If

            ElseIf token.TokenType = TokenType.UnionKeyword Then
                _scanner.GetNextToken()
                ProcessSalAttribute()

                Dim peekList As List(Of Token) = _scanner.PeekTokenList(2)
                If (peekList(0).TokenType = TokenType.Word AndAlso peekList(1).TokenType = TokenType.BraceOpen) _
                    OrElse peekList(0).TokenType = TokenType.BraceOpen Then

                    Return Me.ProcessUnion(namePrefix)
                End If

            ElseIf token.TokenType = TokenType.EnumKeyword Then
                _scanner.GetNextToken()
                ProcessSalAttribute()

                Dim peekList As List(Of Token) = _scanner.PeekTokenList(2)
                If (peekList(0).TokenType = TokenType.Word AndAlso peekList(1).TokenType = TokenType.BraceOpen) _
                    OrElse peekList(0).TokenType = TokenType.BraceOpen Then

                    Return Me.ProcessEnum(namePrefix)
                End If
            ElseIf token.TokenType = TokenType.ClassKeyword Then
                _scanner.GetNextToken()
                ProcessSalAttribute()

                ' If the type name starts with Class there are one of
                ' three possibilities.  
                '   1) Qualified name: Class foo
                '   2) Inline Type: Class { int bar; } 
                '   3) normal Class: Class foo { int bar; }

                Dim peekList As List(Of Token) = _scanner.PeekTokenList(2)
                If (peekList(0).TokenType = TokenType.Word AndAlso peekList(1).TokenType = TokenType.BraceOpen) _
                    OrElse peekList(0).TokenType = TokenType.BraceOpen Then

                    ' If the Class is followed by any trailing typedefs then this function
                    ' will take care of that as well
                    Return Me.ProcessClass(namePrefix)
                End If

            ElseIf includeFunctionPointers Then
                ' Last ditch effort is to parse out a function pointer
                ProcessSalAttribute()

                Dim retType As NativeType = ProcessTypeName()
                If retType IsNot Nothing AndAlso _scanner.PeekNextToken().TokenType = TokenType.ParenOpen Then

                    Return Me.ProcessFunctionPointer(namePrefix, retType)
                End If
            End If

            _scanner.Rollback(mark)
            Return Nothing
        End Function

        Private Function ProcessTypeNameOrType() As NativeType
            Return ProcessTypeNameOrType(String.Empty)
        End Function

        ''' <summary>
        ''' Process a type name, defined type or function pointer from the stream.
        ''' </summary>
        ''' <remarks></remarks>
        Private Function ProcessTypeNameOrType(ByVal namePrefix As String) As NativeType
            Dim definedNt As NativeDefinedType = ProcessDefinedType(namePrefix)
            If definedNt IsNot Nothing Then
                Return definedNt
            End If

            Dim nt As NativeType = ProcessShortTypeName()
            Return ProcessTypeNameModifiers(nt)
        End Function

        ''' <summary>
        ''' Process a simple typename from the stream such as 
        '''  struct foo
        '''  int
        '''  unsigned int
        '''  signed int
        ''' 
        ''' Won't process type modifiers such as *,[] 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessShortTypeName() As NativeType
            Dim isConst As Boolean = False
            Dim qualifiedToken As Token = _scanner.PeekNextToken()
            If qualifiedToken.TokenType = TokenType.ConstKeyword Then
                isConst = True
                _scanner.GetNextToken()
                qualifiedToken = _scanner.PeekNextToken()
            End If

            ' Remove the volatile qualifier
            If qualifiedToken.TokenType = TokenType.VolatileKeyword Then
                _scanner.GetNextToken()
            End If

            ' Look for any type name qualifiers 
            If qualifiedToken.TokenType = TokenType.StructKeyword _
                OrElse qualifiedToken.TokenType = TokenType.UnionKeyword _
                OrElse qualifiedToken.TokenType = TokenType.EnumKeyword _
                OrElse qualifiedToken.TokenType = TokenType.ClassKeyword Then
                _scanner.GetNextToken()

                ' It's possible to put a __declspec here.  Go ahead and remove it
                If _scanner.PeekNextToken().TokenType = TokenType.DeclSpec Then
                    ProcessSalAttribute()
                End If
                Return New NativeNamedType(qualifiedToken.Value, _scanner.GetNextToken(TokenType.Word).Value)
            End If

            ' Down to simple types.  Look for any type prefixes
            Dim bt As NativeBuiltinType = Nothing
            Dim token As Token = _scanner.GetNextToken()
            If token.TokenType = TokenType.LongKeyword _
                OrElse token.TokenType = TokenType.SignedKeyword _
                OrElse token.TokenType = TokenType.UnsignedKeyword Then

                ' If the next token is a builtin type keyword then these are modifiers of that
                ' keyword
                If _scanner.PeekNextToken().IsTypeKeyword Then
                    NativeBuiltinType.TryConvertToBuiltinType(_scanner.GetNextToken().TokenType, bt)
                    bt.IsUnsigned = (token.TokenType = TokenType.UnsignedKeyword)
                Else
                    NativeBuiltinType.TryConvertToBuiltinType(token.TokenType, bt)
                End If
            ElseIf token.IsTypeKeyword Then
                NativeBuiltinType.TryConvertToBuiltinType(token.TokenType, bt)
            End If

            ' If this is a builtin type and it's not constant then just return the builtin type.  Otherwise we 
            ' have to return the named type since it holds the qualifier
            If bt IsNot Nothing Then
                If isConst Then
                    Dim named As New NativeNamedType(bt.Name, True)
                    named.RealType = bt
                    Return named
                Else
                    Return bt
                End If
            Else
                ' It's not a builtin type.  Return the name
                Return New NativeNamedType(token.Value, isConst)
            End If
        End Function

        ''' <summary>
        ''' Processes the modiers, pointer, array and so on for a native type.  Scanner should be positioned
        ''' right after the start of the short type name
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessTypeNameModifiers(ByVal nt As NativeType) As NativeType
            Dim done As Boolean = False
            Do
                Dim token As Token = _scanner.PeekNextToken()
                Select Case token.TokenType
                    Case TokenType.Asterisk
                        ' Wrap it in a pointer and eat the token
                        nt = New NativePointer(nt)
                        _scanner.GetNextToken()

                        ' Handle typeName * const name.
                        If _scanner.PeekNextToken().TokenType = TokenType.ConstKeyword Then
                            _scanner.GetNextToken()
                        End If
                    Case TokenType.BracketOpen
                        ' Wrap it in an array.  Processing the array suffix will
                        ' remove the tokens from the stream
                        nt = ProcessArraySuffix(nt)
                    Case TokenType.Word
                        ' Done once we hit the next word
                        done = True
                    Case TokenType.ConstKeyword
                        ' If the const modifier proceeds a pointer then allow the pointer to 
                        ' be processed.  Otherwise we are done
                        _scanner.GetNextToken()
                        If _scanner.PeekNextToken().TokenType <> TokenType.Asterisk Then
                            done = True
                        End If
                    Case TokenType.VolatileKeyword
                        ' Igore the volatile qualifier
                        _scanner.GetNextToken()
                    Case TokenType.Pointer32Keyword, TokenType.Pointer64Keyword
                        ' Ignore the pointer modifiers
                        _scanner.GetNextToken()
                    Case TokenType.ParenOpen
                        ' Hit a function pointer inside the parameter list.  Type name is completed
                        done = True
                    Case Else
                        done = True
                End Select
            Loop Until done

            Return nt
        End Function

        ''' <summary>
        ''' Process the type parsed out of the stream
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <remarks></remarks>
        Private Sub ProcessParsedDefinedType(ByVal nt As NativeDefinedType)
            ThrowIfNull(nt)

            ' It's possible for members of a defined type to not have a name.  Go ahead and add that
            ' name now
            Dim count As Integer = 1
            For Each mem As NativeMember In nt.Members
                If String.IsNullOrEmpty(mem.Name) Then
                    Dim prefix As String = "AnonymousMember"
                    If mem.NativeTypeDigged IsNot Nothing Then
                        Select Case mem.NativeTypeDigged.Kind
                            Case NativeSymbolKind.UnionType
                                prefix = "Union"
                            Case NativeSymbolKind.StructType
                                prefix = "Struct"
                        End Select
                    End If

                    mem.Name = String.Format("{0}{1}", prefix, count)
                    count += 1
                End If
            Next

            _result.NativeDefinedTypes.Add(nt)
            _result.ParsedTypes.Add(nt)
        End Sub

        Private Sub ProcessParsedTypeDef(ByVal typeDef As NativeTypeDef)
            ThrowIfNull(typeDef)

            _result.NativeTypedefs.Add(typeDef)
            _result.ParsedTypes.Add(typeDef)
        End Sub

        Private Sub ProcessParsedProcedure(ByVal proc As NativeProcedure)
            ThrowIfNull(proc)
            _result.NativeProcedures.Add(proc)
        End Sub

        ''' <summary>
        ''' Called after a type is parsed out but an open brace is detected.  Will parse
        ''' out the array definition
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessArraySuffix(ByVal nt As NativeType) As NativeArray
            ' Create the array
            Dim ntArray As New NativeArray()
            ntArray.RealType = nt
            ntArray.ElementCount = 0

            Dim anyUnbound As Boolean = False
            Dim done As Boolean = False
            While Not done
                Dim count As Object

                ' Move past the opening [
                Dim token As Token = _scanner.GetNextToken()
                If token.TokenType = TokenType.BracketOpen Then
                    token = _scanner.GetNextToken()
                End If

                ' If it's a number then it's the rank of the array
                If (token.TokenType = TokenType.Number OrElse token.TokenType = TokenType.HexNumber) _
                    AndAlso _scanner.PeekNextToken().TokenType = TokenType.BracketClose Then

                    count = Nothing
                    If Not TokenHelper.TryConvertToNumber(token, count) Then
                        Throw ParseException.CreateError( _
                            "Could not process array length as number: {0}", _
                            token.Value)
                    End If

                    ' The token should now be the closing bracket.  
                    token = _scanner.GetNextToken(TokenType.BracketClose)
                ElseIf token.TokenType = TokenType.BracketClose Then
                    count = Nothing
                Else
                    ' Get the text up until the bracket and evaluate it as an expression.  Handles cases
                    ' where we end up with (1+2) for [] lengths
                    Dim exprList As New List(Of Token)
                    exprList.Add(token)

                    Dim nextToken As Token = _scanner.GetNextToken()
                    While nextToken.TokenType <> TokenType.BracketClose
                        exprList.Add(nextToken)
                        nextToken = _scanner.GetNextToken()
                    End While

                    Dim ee As New ExpressionEvaluator()
                    Dim result As ExpressionValue = Nothing
                    If ee.TryEvaluate(exprList, result) AndAlso result.Value.GetType() Is GetType(Int32) Then
                        count = result.Value
                    Else
                        count = Nothing
                    End If
                End If

                If count Is Nothing Then
                    anyUnbound = True
                Else
                    If ntArray.ElementCount = 0 Then
                        ntArray.ElementCount = CInt(count)
                    Else
                        ntArray.ElementCount *= CInt(count)
                    End If
                End If

                If _scanner.PeekNextToken().TokenType <> TokenType.BracketOpen Then
                    done = True
                End If
            End While

            If anyUnbound Then
                ntArray.ElementCount = -1
            End If

            Return ntArray
        End Function

        ''' <summary>
        ''' Read a SAL attribute from code.  They come in the following form
        '''   __declspec("directive")
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessSalAttribute() As NativeSalAttribute
            Dim token As Token = _scanner.PeekNextToken()
            Dim sal As New NativeSalAttribute()
            If token.TokenType <> TokenType.DeclSpec Then
                Return sal
            End If

            Dim done As Boolean
            Do
                _scanner.GetNextToken()
                _scanner.GetNextToken(TokenType.ParenOpen)
                Dim directive As Token = _scanner.GetNextToken()

                ' It's legal for the SAL attribute to be custom defined and as such
                ' we should process the argument
                If Not directive.IsQuotedString Then
                    Dim depth As Integer = 0
                    Dim text As String = directive.Value
                    While depth > 0 OrElse _scanner.PeekNextToken().TokenType <> TokenType.ParenClose
                        Dim cur As Token = _scanner.GetNextToken()
                        text &= cur.Value
                        Select Case cur.TokenType
                            Case TokenType.ParenOpen
                                depth += 1
                            Case TokenType.ParenClose
                                depth -= 1
                        End Select
                    End While
                    directive = New Token(TokenType.Text, text)
                End If

                Dim entry As NativeSalEntry = ConvertSalDirectiveToEntry(directive.Value)
                If entry IsNot Nothing Then
                    sal.SalEntryList.Add(entry)
                End If

                ' Get the close paren
                _scanner.GetNextToken()

                ' See if there are more declarations
                If _scanner.PeekNextToken().TokenType <> TokenType.DeclSpec Then
                    done = True
                End If
            Loop Until done

            Return sal
        End Function

        ''' <summary>
        ''' Process through a block of code ignoring all of the data inside the block.  Function should
        ''' be called with the next token being the first { in the block
        ''' </summary>
        ''' <remarks></remarks>
        Private Function ProcessBlock(ByVal openType As TokenType, ByVal closeType As TokenType) As List(Of Token)
            Dim list As New List(Of Token)
            list.Add(_scanner.GetNextToken(openType))
            Return ProcessBlockRemainderCore(list, openType, closeType)
        End Function

        Private Sub ProcessBlockRemainder(ByVal openType As TokenType, ByVal closeType As TokenType)
            Dim list As New List(Of Token)
            ProcessBlockRemainderCore(list, openType, closeType)
        End Sub

        Private Function ProcessBlockRemainderCore(ByVal list As List(Of Token), ByVal openType As TokenType, ByVal closeType As TokenType) As List(Of Token)
            Dim depth As Integer = 1
            Do
                If _scanner.EndOfStream Then
                    Throw ParseException.CreateError("Encountered end of stream while attempting to process a block")
                End If

                Dim nextToken As Token = _scanner.GetNextToken()
                list.Add(nextToken)
                Select Case nextToken.TokenType
                    Case openType
                        depth += 1
                    Case closeType
                        depth -= 1
                End Select
            Loop Until depth = 0

            Return list
        End Function


        ''' <summary>
        ''' Convert the sal directive to an entry
        ''' </summary>
        ''' <param name="directive"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ConvertSalDirectiveToEntry(ByVal directive As String) As NativeSalEntry

            ' Remove the begining and ending quotes
            If String.IsNullOrEmpty(directive) Then
                Return Nothing
            End If

            If directive(0) = """"c AndAlso directive(directive.Length - 1) = """"c Then
                directive = directive.Substring(1, directive.Length - 2)
            End If

            ' If there is a ( then we need to process the inner text
            Dim text As String = String.Empty
            Dim entry As SalEntryType
            Dim index As Int32 = directive.LastIndexOf("("c)
            If index >= 0 Then

                ' Find the inner data
                Dim otherIndex As Int32 = directive.IndexOf(")"c)
                If otherIndex < 0 OrElse index + 1 > otherIndex Then
                    Return Nothing
                End If

                text = directive.Substring(index + 1, otherIndex - (index + 1))
                directive = directive.Substring(0, index + 1) & directive.Substring(otherIndex)
            End If

            If Not _salTable.TryGetValue(directive, entry) Then
                Return Nothing
            End If

            Return New NativeSalEntry(entry, text)
        End Function

        ''' <summary>
        ''' After normal token processing occurs in the global scope this function is called
        ''' to see if we hit any unsupported scenarios
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ProcessGlobalTokenForUnsupportedScenario()
            Dim token As Token = _scanner.PeekNextToken()
            Select Case token.TokenType
                Case TokenType.BracketOpen
                    Dim list As List(Of Token) = ProcessBlock(TokenType.BracketOpen, TokenType.BracketClose)
                    Dim msg As String = String.Format( _
                        "C++ attributes are not supported: {0}", _
                        TokenHelper.TokenListToString(list))
                    Throw ParseException.CreateWarningStreamOk(msg)
            End Select
        End Sub

#Region "GetTokenHelpers"

        ''' <summary>
        ''' Eat tokens until we hit the end of stream or line.  Consumes the EndOfLine token
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ChewThroughEndOfLine()
            Dim done As Boolean = False

            Dim prevOpt As Boolean = _scanner.Options.HideNewLines
            Try
                _scanner.Options.HideNewLines = False
                While Not done
                    If _scanner.EndOfStream Then
                        done = True
                    Else
                        Dim token As Token = _scanner.GetNextToken()
                        If token.TokenType = TokenType.NewLine Then
                            done = True
                        End If
                    End If
                End While
            Finally
                _scanner.Options.HideNewLines = prevOpt
            End Try
        End Sub


        Private Function PeekLineInformation(ByVal count As Integer) As String
            Dim mark As ScannerMark = _scanner.Mark()
            Dim old As Boolean = _scanner.Options.HideWhitespace
            Try
                _scanner.Options.HideWhitespace = False

                Dim b As New Text.StringBuilder()
                Dim found As Integer = 0

                While found < count
                    If _scanner.EndOfStream Then
                        Exit While
                    End If

                    Dim cur As Token = _scanner.GetNextToken()
                    b.Append(cur.Value)
                    If TokenType.WhiteSpace <> cur.TokenType Then
                        found += 1
                    End If
                End While

                Return b.ToString()
            Finally
                _scanner.Options.HideWhitespace = old
                _scanner.Rollback(mark)
            End Try
        End Function

#End Region

    End Class

End Namespace
