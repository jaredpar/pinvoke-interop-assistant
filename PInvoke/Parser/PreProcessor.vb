' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.iO
Imports System.Collections.Generic
Imports System.Text

Namespace Parser

#Region "PreProcessorOptions"

    ''' <summary>
    ''' Options for the preprocessor
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PreProcessorOptions
        Private m_macroList As New List(Of Macro)
        Private m_followIncludes As Boolean
        Private m_includePathList As New List(Of String)
        Private m_trace As Boolean

        ''' <summary>
        ''' Options to start the preprocessor with
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property InitialMacroList() As List(Of Macro)
            Get
                Return m_macroList
            End Get
        End Property

        ''' <summary>
        ''' Whether or not the pre-processor should follow #include's
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FollowIncludes() As Boolean
            Get
                Return m_followIncludes
            End Get
            Set(ByVal value As Boolean)
                m_followIncludes = value
            End Set
        End Property

        ''' <summary>
        ''' List of paths to search for header file that is included
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IncludePathList() As List(Of String)
            Get
                Return m_includePathList
            End Get
        End Property

        ''' <summary>
        ''' When true, the preprocessor will output comments detailing the conditional evalution into
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' </summary>
        Public Property Trace() As Boolean
            Get
                Return m_trace
            End Get
            Set(ByVal value As Boolean)
                m_trace = value
            End Set
        End Property

        Public Sub New()
        End Sub

    End Class

#End Region

    ''' <summary>
    ''' Runs the preprocessor on a stream of data and returns the result without the macros
    ''' or preprocessor junk
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PreProcessorEngine

        <DebuggerDisplay("{DisplayLine}")> _
        Private Class PreprocessorLine
            Public TokenList As List(Of Token)
            Public FirstValidToken As Token
            Public IsPreProcessorDirectiveLine As Boolean

            ''' <summary>
            ''' Useful for debugging 
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property DisplayLine() As String
                Get
                    Dim builder As New StringBuilder()
                    For Each token As Token In Me.GetValidTokens()
                        builder.AppendFormat("[{0}] ", token.Value)
                    Next

                    Return builder.ToString()
                End Get
            End Property

            Public Function GetValidTokens() As List(Of Token)
                Dim list As New List(Of Token)
                For Each token As Token In TokenList
                    Select Case token.TokenType
                        Case TokenType.WhiteSpace, TokenType.NewLine, TokenType.EndOfStream
                            ' Don't add these types
                        Case Else
                            list.Add(token)
                    End Select
                Next

                Return list
            End Function

            Public Overrides Function ToString() As String
                Dim b As New Text.StringBuilder()
                For Each cur As Token In TokenList
                    b.Append(cur.Value)
                Next

                Return b.ToString()
            End Function
        End Class

        Private Class PreProcessorException
            Inherits Exception
            Private m_isError As Boolean = True

            Friend Property IsError() As Boolean
                Get
                    Return m_isError
                End Get
                Set(ByVal value As Boolean)
                    m_isError = value
                End Set
            End Property

            Public Sub New(ByVal msg As String)
                MyBase.New(msg)
            End Sub

            Public Sub New(ByVal msg As String, ByVal isError As Boolean)
                MyBase.New(msg)
                m_isError = isError
            End Sub

            Public Sub New(ByVal msg As String, ByVal inner As Exception)
                MyBase.New(msg, inner)
            End Sub

        End Class

        Private Class PreProcessorEvaluator
            Inherits ExpressionEvaluator

            Private m_engine As PreProcessorEngine

            Public Sub New(ByVal engine As PreProcessorEngine)
                m_engine = engine
            End Sub

            ''' <summary>
            ''' Evaluate a preprocessor conditional statement and return whether or not it is true
            ''' </summary>
            ''' <param name="line"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function EvalauteConditional(ByVal line As PreprocessorLine) As Boolean
                Dim list As List(Of Token) = line.GetValidTokens()

                ' Remove the #pound token.  We don't care what type of conditional this is, this just serves
                ' to evaluate it and let the caller interpret the result
                list.RemoveAt(0)

                ' Make sure that all "defined" expressions wrap the next value in ()
                Dim i As Int32 = 0
                While i + 1 < list.Count
                    Dim cur As Token = list(i)
                    If cur.TokenType = TokenType.Word _
                        AndAlso 0 = String.CompareOrdinal("defined", cur.Value) _
                        AndAlso list(i + 1).TokenType = TokenType.Word Then

                        list.Insert(i + 1, New Token(TokenType.ParenOpen, "("))
                        list.Insert(i + 3, New Token(TokenType.ParenClose, ")"))

                        i += 3
                    End If

                    i += 1
                End While

                Dim value As ExpressionValue = Nothing
                If Not MyBase.TryEvaluate(list, value) Then
                    m_engine.m_errorProvider.AddError("Could not evaluate expression {0}", line.ToString())
                    Return False
                End If

                Return value <> New ExpressionValue(0)
            End Function

            Protected Overrides Function TryEvaluateFunctionCall(ByVal node As ExpressionNode) As Boolean
                Dim value As ExpressionValue
                If node.Token.Value = "defined" _
                                AndAlso node.LeftNode IsNot Nothing _
                                AndAlso m_engine.m_macroMap.ContainsKey(node.LeftNode.Token.Value) Then
                    value = True
                Else
                    value = False
                End If

                node.Tag = value
                Return True
            End Function

            Protected Overrides Function TryEvaluateNegation(ByVal node As ExpressionNode) As Boolean
                Dim value As ExpressionValue = DirectCast(node.LeftNode.Tag, ExpressionValue)
                value = Not value
                node.Tag = value
                Return True
            End Function

            Protected Overrides Function TryEvaluateLeaf(ByVal node As ExpressionNode) As Boolean
                If node.Kind = ExpressionKind.Leaf AndAlso node.Token.TokenType = TokenType.Word Then
                    Dim value As ExpressionValue
                    Dim m As Macro = Nothing
                    If Me.m_engine.m_macroMap.TryGetValue(node.Token.Value, m) Then
                        Dim numValue As Object = Nothing
                        If TokenHelper.TryConvertToNumber(m.Value, numValue) Then
                            value = New ExpressionValue(numValue)
                        Else
                            value = 1
                        End If
                    Else
                        value = 0
                    End If

                    node.Tag = value
                    Return True
                ElseIf TokenHelper.IsKeyword(node.Token.TokenType) Then
                    node.Tag = New ExpressionValue(1)
                    Return True
                Else
                    Return MyBase.TryEvaluateLeaf(node)
                End If
            End Function

            ''' <summary>
            ''' For a cast just return the value of thhe left node
            ''' </summary>
            ''' <param name="node"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Overrides Function TryEvaluateCast(ByVal node As ExpressionNode) As Boolean
                node.Tag = node.LeftNode.Tag
                Return True
            End Function
        End Class

        Private m_options As PreProcessorOptions
        Private m_macroMap As New Dictionary(Of String, Macro)
        Private m_processing As Boolean
        Private m_scanner As Scanner
        Private m_outputStream As TextWriter
        Private m_errorProvider As New ErrorProvider()
        Private m_eval As PreProcessorEvaluator
        Private m_metadataMap As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>
        ''' Options of the NativePreProcessor
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Options() As PreProcessorOptions
            Get
                Return m_options
            End Get
            Set(ByVal value As PreProcessorOptions)
                m_options = value
            End Set
        End Property

        ''' <summary>
        ''' List of macros encountered by the NativePreProcessor
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MacroMap() As Dictionary(Of String, Macro)
            Get
                Return m_macroMap
            End Get
        End Property

        Public Property ErrorProvider() As ErrorProvider
            Get
                Return m_errorProvider
            End Get
            Set(ByVal value As ErrorProvider)
                m_errorProvider = value
            End Set
        End Property

        Public Sub New(ByVal options As PreProcessorOptions)
            m_eval = New PreProcessorEvaluator(Me)
            m_options = options
        End Sub

        ''' <summary>
        ''' Process the given stream and return the result of removing the 
        ''' preprocessor definitions
        ''' </summary>
        ''' <param name="readerBag"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Process(ByVal readerBag As TextReaderBag) As String
            ThrowIfTrue(m_processing, "Recursive parsing not supported in this manner.")

            Dim builder As New StringBuilder()
            Try
                ' Setup the macro map
                m_macroMap.Clear()
                For Each m As Macro In m_options.InitialMacroList
                    m_macroMap(m.Name) = m
                Next

                m_outputStream = New StringWriter(builder)
                Using m_outputStream
                    m_processing = True
                    ProcessCore(readerBag)

                    If m_options.Trace Then
                        TraceMacroMap()
                    End If
                End Using
            Finally
                m_processing = False
                m_outputStream = Nothing
            End Try

            Return builder.ToString()

        End Function

        ''' <summary>
        ''' Called to process a particular stream of text.  Can be called recursively
        ''' </summary>
        ''' <param name="readerBag"></param>
        ''' <remarks></remarks>
        Private Sub ProcessCore(ByVal readerBag As TextReaderBag)
            ThrowIfFalse(m_processing)
            Dim oldScanner As Scanner = m_scanner
            Try
                ' Create the scanner
                m_scanner = New Scanner(readerBag, CreateScannerOptions())
                m_scanner.ErrorProvider = Me.ErrorProvider

                ProcessLoop()

            Finally
                m_scanner = oldScanner
            End Try

        End Sub

        Private Function CreateScannerOptions() As ScannerOptions
            Dim opts As New ScannerOptions()
            opts.HideComments = True
            opts.HideNewLines = False
            opts.HideWhitespace = False
            opts.ThrowOnEndOfStream = False
            Return opts
        End Function

        ''' <summary>
        ''' Core processing loop.  Processes blocks of text.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ProcessLoop()

            Dim done As Boolean = False
            While Not done

                Dim mark As ScannerMark = m_scanner.Mark()
                Try

                    Dim line As PreprocessorLine = Me.GetNextLine()
                    ThrowIfFalse(line.TokenList.Count > 0)

                    Dim token As Token = line.FirstValidToken
                    If token Is Nothing Then
                        WriteToStream(line)
                        Continue While
                    End If

                    Select Case token.TokenType
                        Case TokenType.PoundIf
                            ProcessPoundIf(line)
                        Case TokenType.PoundIfndef
                            ProcessPoundIfndef(line)
                        Case TokenType.PoundElse, TokenType.PoundElseIf
                            ' stop on a conditional branch end
                            ChewThroughConditionalEnd()
                            done = True
                        Case TokenType.EndOfStream, TokenType.PoundEndIf
                            done = True
                        Case TokenType.PoundPragma
                            ProcessPoundPragma(line)
                        Case TokenType.PoundDefine
                            ProcessPoundDefine(line)
                        Case TokenType.PoundUnDef
                            ProcessPoundUndefine(line)
                        Case TokenType.PoundInclude
                            ProcessPoundInclude(line)
                        Case Else
                            WriteToStream(line)
                    End Select

                Catch ex As PreProcessorException
                    If ex.IsError Then
                        m_errorProvider.AddError(ex.Message)
                    Else
                        m_errorProvider.AddWarning(ex.Message)
                    End If
                    m_scanner.Rollback(mark)
                    GetNextLine()   ' Chew through the line
                End Try
            End While
        End Sub

        ''' <summary>
        ''' Called when a define token is hit
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ProcessPoundDefine(ByVal line As PreprocessorLine)

            ' Get the non whitespace tokens
            Dim list As List(Of Token) = line.GetValidTokens()
            ThrowIfFalse(list(0).TokenType = TokenType.PoundDefine)

            Dim macro As Macro = Nothing
            If list.Count = 3 AndAlso list(1).TokenType = TokenType.Word Then
                Dim name As String = list(1).Value
                macro = New Macro(name, list(2).Value)
            ElseIf list.Count = 2 AndAlso list(1).TokenType = TokenType.Word Then
                Dim name As String = list(1).Value
                macro = New Macro(name, String.Empty)
            ElseIf list.Count = 1 Then
                m_scanner.AddWarning("Encountered an empty #define")
            ElseIf list.Count > 3 _
                AndAlso list(1).TokenType = TokenType.Word _
                AndAlso list(2).TokenType = TokenType.ParenOpen Then

                macro = ProcessPoundDefineMethod(line)
            Else
                macro = ProcessPoundDefineComplexMacro(line)
            End If

            If macro IsNot Nothing Then
                Dim oldMacro As Macro = Nothing
                If m_macroMap.TryGetValue(macro.Name, oldMacro) AndAlso oldMacro.IsPermanent Then
                    TraceToStream("Kept: {0} -> {1} Attempted Value {2}", oldMacro.Name, oldMacro.Value, macro.Value)
                Else
                    m_macroMap(macro.Name) = macro
                    If macro.IsMethod Then
                        Dim method As MethodMacro = DirectCast(macro, MethodMacro)
                        TraceToStream("Defined: {0} -> {1}", macro.Name, method.MethodSignature)
                    Else
                        TraceToStream("Defined: {0} -> {1}", macro.Name, macro.Value)
                    End If
                End If
            End If
        End Sub

        Private Function ProcessPoundDefineComplexMacro(ByVal line As PreprocessorLine) As Macro
            ' It's a complex macro.  Go ahead and get the line information
            Dim list As New List(Of Token)(line.TokenList)
            Dim i As Integer = 0

            ' Strip the newlines
            While i < list.Count
                If list(i).TokenType = TokenType.NewLine Then
                    list.RemoveAt(i)
                Else
                    i += 1
                End If
            End While
            i = 0

            ' Get the #define token
            Dim defineToken As Token = Nothing
            While i < list.Count
                If list(i).TokenType = TokenType.PoundDefine Then
                    defineToken = list(i)
                    Exit While
                End If
                i += 1
            End While

            ' Get the name token
            Dim nameToken As Token = Nothing
            While i < list.Count
                If list(i).TokenType = TokenType.Word Then
                    nameToken = list(i)
                    Exit While
                End If

                i += 1
            End While

            If defineToken Is Nothing OrElse nameToken Is Nothing Then
                m_errorProvider.AddWarning("Error processing line: {0}", line.ToString())
                Return New Macro(NativeSymbolBag.GenerateAnonymousName(), String.Empty)
            End If

            ' i now points to the name token.  Remove the range of tokens up until this point.  Now remove the
            ' whitespace on either end of the list
            list.RemoveRange(0, i + 1)
            While list.Count > 0 _
                AndAlso (list(0).TokenType = TokenType.WhiteSpace OrElse list(0).TokenType = TokenType.NewLine)
                list.RemoveAt(0)
            End While
            While list.Count > 0 _
                AndAlso (list(list.Count - 1).TokenType = TokenType.WhiteSpace OrElse list(list.Count - 1).TokenType = TokenType.NewLine)
                list.RemoveAt(list.Count - 1)
            End While

            ' Create a string for all of the tokens
            Dim b As New Text.StringBuilder
            For Each cur As Token In list
                b.Append(cur.Value)
            Next

            Return New Macro(nameToken.Value, b.ToString())
        End Function

        ''' <summary>
        ''' Process a #define that is actually a function
        ''' </summary>
        ''' <param name="line"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ProcessPoundDefineMethod(ByVal line As PreprocessorLine) As Macro
            ' First step is to parse out the name and parameters
            Dim list As List(Of Token) = line.GetValidTokens()
            Dim name As String = list(1).Value
            list.RemoveRange(0, 3)

            Dim paramList As New List(Of String)
            While (list(0).TokenType <> TokenType.ParenClose)
                If list(0).TokenType = TokenType.Word Then
                    paramList.Add(list(0).Value)
                ElseIf list(0).TokenType = TokenType.ParenOpen Then
                    ' ( is not legal inside a parameter list.  This is a simple macro
                    Return ProcessPoundDefineComplexMacro(line)
                End If
                list.RemoveAt(0)
            End While

            ' Now get the fullBody.  We need the actual text for the fullBody so search through the true token list
            Dim index As Int32 = 0
            While (line.TokenList(index).TokenType <> TokenType.ParenClose)
                index += 1
            End While

            index += 1
            Dim fullBody As List(Of Token) = line.TokenList.GetRange(index, line.TokenList.Count - index)

            ' Strip the trailing and ending whitespace on the fullBody
            While fullBody.Count > 0 _
                AndAlso (fullBody(0).TokenType = TokenType.WhiteSpace OrElse fullBody(0).TokenType = TokenType.NewLine)
                fullBody.RemoveAt(0)
            End While

            ' Don't be fooled by a simple #define that simply wraps the entire fullBody inside a
            ' set of ().  
            If (fullBody.Count = 0) Then
                Return ProcessPoundDefineComplexMacro(line)
            End If

            While fullBody.Count > 0 _
                AndAlso (fullBody(fullBody.Count - 1).TokenType = TokenType.WhiteSpace OrElse fullBody(fullBody.Count - 1).TokenType = TokenType.NewLine)
                fullBody.RemoveAt(fullBody.Count - 1)
            End While

            ' Coy the body token list since we are about to change the data
            Dim body As New List(Of Token)(fullBody)

            ' Collapse the whitespace around ## entries
            Dim i As Integer = 0
            While i + 1 < body.Count
                Dim left As Token = body(i)
                Dim right As Token = body(i + 1)

                If left.TokenType = TokenType.Pound AndAlso right.TokenType = TokenType.Pound Then
                    ' First look at the right
                    If i + 2 < body.Count AndAlso body(i + 2).TokenType = TokenType.WhiteSpace Then
                        body.RemoveAt(i + 2)
                    End If

                    ' Now look at the left
                    If i > 0 AndAlso body(i - 1).TokenType = TokenType.WhiteSpace Then
                        body.RemoveAt(i - 1)
                    End If
                End If

                i += 1
            End While

            index += 1
            Return New MethodMacro( _
                name, _
                paramList, _
                body, _
                fullBody)
        End Function

        ''' <summary>
        ''' Called for a #undef line
        ''' </summary>
        ''' <param name="line"></param>
        ''' <remarks></remarks>
        Private Sub ProcessPoundUndefine(ByVal line As PreprocessorLine)
            ' Get the none whitespace tokens
            Dim list As List(Of Token) = line.GetValidTokens()
            ThrowIfFalse(list(0).TokenType = TokenType.PoundUnDef)

            If list.Count <> 2 OrElse list(1).TokenType <> TokenType.Word Then
                m_scanner.AddWarning("Error processing #undef")
            Else
                Dim name As String = list(1).Value
                If m_macroMap.ContainsKey(name) Then
                    m_macroMap.Remove(name)
                    TraceToStream("Undefined: {0}", name)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Process a #include line.  These take typically two forms 
        '''   #include "foo.h"
        '''   #include &gt;foo.h&gt;
        ''' 
        ''' </summary>
        ''' <param name="line"></param>
        ''' <remarks></remarks>
        Private Sub ProcessPoundInclude(ByVal line As PreprocessorLine)

            If Not m_options.FollowIncludes Then
                Return
            End If

            ' if the user did a <> include then there won't be any quotes around the string
            ' so go ahead and redo the include to look like a "filename.h" include
            Dim list As New List(Of Token)(line.GetValidTokens())

            ' Get rid of the #include
            ThrowIfFalse(list(0).TokenType = TokenType.PoundInclude)
            list.RemoveAt(0)

            Dim name As String
            If list(0).TokenType = TokenType.OpLessThan Then
                name = String.Empty
                list.RemoveAt(0)
                While list(0).TokenType <> TokenType.OpGreaterThan
                    name &= list(0).Value
                    list.RemoveAt(0)
                End While
                list.RemoveAt(0)
            ElseIf list(0).IsQuotedString Then
                name = TokenHelper.ConvertToString(list(0))
            Else
                name = Nothing
            End If

            If name Is Nothing Then
                m_scanner.AddWarning("Invalid #include statement")
                Return
            End If

            ' Now actually try and find the file.  First check the custom list
            Dim found As Boolean = False
            Dim customPath As String = Nothing
            If File.Exists(name) Then
                found = True
                TraceToStream("include {0} followed -> {0}", name)
                TraceToStream("include {0} start", name)
                Using reader As New StreamReader(name)
                    ProcessCore(New TextReaderBag(name, reader))
                End Using
                TraceToStream("include {0} end", name)
            ElseIf m_options.IncludePathList.Count > 0 Then
                ' Search through the path list
                found = False
                For Each prefix As String In m_options.IncludePathList
                    Dim fullPath As String = Path.Combine(prefix, name)
                    If File.Exists(fullPath) Then
                        found = True
                        TraceToStream("include {0} followed -> {1}", name, fullPath)
                        TraceToStream("include {0} start", name)
                        Using reader As New StreamReader(fullPath)
                            ProcessCore(New TextReaderBag(fullPath, reader))
                        End Using
                        TraceToStream("include {0} end", name)
                        Exit For
                    End If
                Next
            Else
                found = False
            End If

            If Not found Then
                m_scanner.AddWarning("Could not locate include file {0}", name)
                TraceToStream("include {0} not followed", name)
            End If

        End Sub

        ''' <summary>
        ''' Process a #pragma statement.
        ''' </summary>
        ''' <param name="line"></param>
        ''' <remarks></remarks>
        Private Sub ProcessPoundPragma(ByVal line As PreprocessorLine)
            ' We don't support #pragma at this point
        End Sub

        ''' <summary>
        ''' Called when a #if is encountered.  If the condition is true,
        ''' it will eat the #if line and let parsing continue.  Otherwise
        ''' it will chew until it hits the branch that should be processed 
        ''' or it hits the #endif 
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ProcessPoundIf(ByVal line As PreprocessorLine)

            ' The object here is to find the branch of the conditional that should
            ' be processed
            Dim isCondTrue As Boolean = m_eval.EvalauteConditional(line)
            TraceToStream("{0}: {1}", isCondTrue, line.DisplayLine)
            If isCondTrue Then
                ' Start another processing loop
                Me.ProcessLoop()
            Else
                ProcessConditionalRemainder()
            End If
        End Sub

        ''' <summary>
        ''' Called when an #ifndef is encountered
        ''' </summary>
        ''' <param name="line"></param>
        ''' <remarks></remarks>
        Private Sub ProcessPoundIfndef(ByVal line As PreprocessorLine)
            Dim isCondTrue As Boolean = m_eval.EvalauteConditional(line)
            TraceToStream("{0}: {1}", isCondTrue, line.DisplayLine)
            If Not isCondTrue Then
                ' Start a processing loop
                Me.ProcessLoop()
            Else
                ProcessConditionalRemainder()
            End If
        End Sub

        ''' <summary>
        ''' Called when the #if branch of a conditional is not true.  Processes the branch
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ProcessConditionalRemainder()

            Dim done As Boolean = False
            While Not done

                ' It's possible to have unmatched #if blocks.  If we hit the end of the stream this means
                ' it is unbalanced so throw an exception
                If m_scanner.EndOfStream Then
                    Throw New PreProcessorException("Found unbalanced conditional preprocessor branch")
                End If

                ' Look at the next branch
                ChewThroughConditionalBranch()

                Dim cur As PreprocessorLine = Me.GetNextLine()
                Select Case cur.FirstValidToken.TokenType
                    Case TokenType.PoundElse
                        ' Start another processing loop
                        Me.ProcessLoop()
                        done = True
                    Case TokenType.PoundElseIf
                        If m_eval.EvalauteConditional(cur) Then
                            Me.ProcessLoop()
                            done = True
                        End If
                    Case TokenType.PoundEndIf
                        done = True
                End Select
            End While
        End Sub


        ''' <summary>
        ''' Get the next line of tokens
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetNextLine() As PreprocessorLine
            Dim line As New PreprocessorLine
            line.TokenList = New List(Of Token)

            Dim lastValidToken As Token = Nothing
            Dim done As Boolean = False
            While Not done
                Dim token As Token = m_scanner.GetNextToken()
                line.TokenList.Add(token)

                Dim isValid As Boolean
                If token.TokenType = TokenType.NewLine Then

                    ' Check and see if this is a preprocessor directive token that ends with a 
                    ' backslash.  If so then remove the backslash from the stream and continue processing
                    ' the line
                    If lastValidToken IsNot Nothing AndAlso lastValidToken.TokenType = TokenType.BackSlash Then
                        isValid = False
                        line.TokenList.Remove(lastValidToken)
                        lastValidToken = Nothing
                    Else
                        done = True
                        isValid = True
                    End If
                ElseIf token.TokenType = TokenType.EndOfStream Then
                    done = True
                    isValid = True

                    ' simulate a newline token
                    line.TokenList.RemoveAt(line.TokenList.Count - 1)
                    line.TokenList.Add(New Token(TokenType.NewLine, vbCrLf))
                ElseIf token.TokenType <> TokenType.WhiteSpace Then
                    isValid = True
                Else
                    isValid = False
                End If

                If isValid Then
                    lastValidToken = token
                    If line.FirstValidToken Is Nothing Then
                        line.FirstValidToken = token

                        ' See if this is a preprocessor line
                        If token.IsPreProcessorDirective Then
                            line.IsPreProcessorDirectiveLine = True
                        End If
                    End If
                End If
            End While

            ' This should always have at least one valid token
            ThrowIfNull(line.FirstValidToken)

            ' Check and see if the line looks like the following.  If so convert it to a valid pre-processor line
            ' #    define foo
            CollapseExpandedPreprocessorLines(line)

            ' If this is not a preprocessor directive line then we need to substitute all of the
            ' #define'd tokens in the stream
            If Not line.IsPreProcessorDirectiveLine _
                OrElse (line.FirstValidToken IsNot Nothing AndAlso line.FirstValidToken.TokenType = TokenType.PoundInclude) Then
                ReplaceDefinedTokens(line)
            End If

            ' Collapse quoted strings that are adjacent to each other
            CollapseAdjacentQuoteStrings(line)


            Return line
        End Function

        Private Sub CollapseExpandedPreprocessorLines(ByRef line As PreprocessorLine)
            If line.FirstValidToken IsNot Nothing AndAlso line.FirstValidToken.TokenType = TokenType.Pound Then
                Dim list As List(Of Token) = line.GetValidTokens()
                Dim possibleToken As Token = list(1)
                Dim poundToken As Token = Nothing
                If list.Count >= 2 AndAlso TokenHelper.TryConvertToPoundToken(possibleToken.Value, poundToken) Then

                    ' Strip out everything # -> define
                    Dim newList As New List(Of Token)(line.TokenList)
                    Dim done As Boolean = False
                    While Not done
                        If newList.Count = 0 Then
                            Debug.Fail("Non-crititcal error reducing the preprocessor line")
                            Return
                        ElseIf Object.ReferenceEquals(newList(0), possibleToken) Then
                            newList.RemoveAt(0)
                            newList.Insert(0, poundToken)
                            done = True
                        Else
                            newList.RemoveAt(0)
                        End If
                    End While

                    Dim formattedLine As New PreprocessorLine()
                    formattedLine.FirstValidToken = poundToken
                    formattedLine.IsPreProcessorDirectiveLine = True
                    formattedLine.TokenList = newList
                    line = formattedLine
                End If
            End If
        End Sub

        ''' <summary>
        ''' Peek at the next line
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function PeekNextLine() As PreprocessorLine
            Dim mark As ScannerMark = m_scanner.Mark()
            Dim line As PreprocessorLine
            Try
                line = GetNextLine()
            Finally
                m_scanner.Rollback(mark)
            End Try

            Return line
        End Function

        Private Sub ReplaceDefinedTokens(ByVal line As PreprocessorLine)
            ThrowIfNull(line)

            Dim i As Integer = 0
            Dim list As List(Of Token) = line.TokenList
            While (i < list.Count)
                Dim token As Token = list(i)
                If token.TokenType <> TokenType.Word Then
                    i += 1
                    Continue While
                End If

                Dim macro As Macro = Nothing
                If m_macroMap.TryGetValue(token.Value, macro) Then
                    ' Remove the original token
                    list.RemoveAt(i)

                    Dim replaceList As List(Of Token) = Nothing
                    If macro.IsMethod Then
                        Dim method As MethodMacro = DirectCast(macro, MethodMacro)
                        Dim args As List(Of Token) = ParseAndRemoveMacroMethodArguments(list, i)
                        If args Is Nothing Then
                            ' Parse did not succeed, move to the next token
                            i += 1
                        Else
                            ' Insert the tokens
                            replaceList = ReplaceMethodMacro(method, args)
                        End If
                    Else
                        ' Use the scanner to create the replacement tokens
                        replaceList = Scanner.TokenizeText(macro.Value, CreateScannerOptions())
                    End If

                    If replaceList IsNot Nothing Then
                        CollapseDoublePounds(replaceList)
                        list.InsertRange(i, replaceList)
                    End If
                Else
                    i += 1
                End If

            End While

            ' Do one more pass to check and see if we need a recursive replace
            Dim needAnotherPass As Boolean = False
            For Each cur As Token In line.TokenList
                If cur.TokenType = TokenType.Word AndAlso m_macroMap.ContainsKey(cur.Value) Then
                    needAnotherPass = True
                    Exit For
                End If
            Next

            If needAnotherPass Then
                ReplaceDefinedTokens(line)
            End If

        End Sub

        Private Function ReplaceMethodMacro(ByVal method As MethodMacro, ByVal args As List(Of Token)) As List(Of Token)
            ' First run the replacement 
            Dim retList As List(Of Token) = method.Replace(args)

            ' When creating the arguments for a macro, non-trivial arguments (1+2) come accross
            ' as text macros.  For those items we need to reparse them here and put them back into the stream.
            ' Have to do this after the above loop so that ## and # are processed correctly
            Dim i As Integer = 0
            While i < retList.Count
                Dim cur As Token = retList(i)
                If cur.TokenType = TokenType.Text AndAlso args.IndexOf(cur) >= 0 Then
                    retList.RemoveAt(i)
                    retList.InsertRange(i, Scanner.TokenizeText(cur.Value, m_scanner.Options))
                End If

                i += 1
            End While

            Return retList
        End Function

        Private Function ParseAndRemoveMacroMethodArguments(ByVal list As List(Of Token), ByVal start As Int32) As List(Of Token)
            Dim args As New List(Of Token)
            Dim i As Int32 = start

            ' Search for the start paren
            While i < list.Count AndAlso list(i).TokenType = TokenType.WhiteSpace
                i += 1
            End While

            If list(i).TokenType <> TokenType.ParenOpen Then
                Return Nothing
            End If
            i += 1    ' Move past the '('

            Dim depth As Int32 = 0
            Dim curArg As Token = New Token(TokenType.Text, String.Empty)

            While i < list.Count
                Dim cur As Token = list(i)
                Dim append As Boolean = False
                Select Case cur.TokenType
                    Case TokenType.Comma
                        If depth = 0 Then
                            args.Add(curArg)
                            curArg = New Token(TokenType.Text, String.Empty)
                        End If
                    Case TokenType.ParenOpen
                        depth += 1
                        append = True
                    Case TokenType.ParenClose
                        If depth = 0 Then
                            args.Add(curArg)
                            Exit While
                        Else
                            depth -= 1
                            append = True
                        End If
                    Case Else
                        append = True
                End Select

                If append Then
                    If curArg.TokenType = TokenType.Text AndAlso String.IsNullOrEmpty(curArg.Value) Then
                        curArg = cur
                    Else
                        curArg = New Token(TokenType.Text, curArg.Value & cur.Value)
                    End If
                End If

                i += 1
            End While

            If i = list.Count Then
                Return Nothing
            End If

            ' Success so remove the list.  'i' currently points at )
            list.RemoveRange(start, (i - start) + 1)
            Return args
        End Function

        ''' <summary>
        ''' When two quoted strings appear directly next to each other then make them one 
        ''' quoted string
        ''' </summary>
        ''' <param name="line"></param>
        ''' <remarks></remarks>
        Private Sub CollapseAdjacentQuoteStrings(ByVal line As PreprocessorLine)

            Dim list As List(Of Token) = line.TokenList

            ' Loop for more
            Dim index As Int32 = 0
            While index < list.Count

                If Not list(index).IsQuotedString Then
                    index += 1
                    Continue While
                End If

                ' Found a quoted string, search for a partner
                Dim nextIndex As Int32 = index + 1
                Dim nextToken As Token = Nothing
                While nextIndex < list.Count
                    Select Case list(nextIndex).TokenType
                        Case TokenType.WhiteSpace, TokenType.NewLine
                            nextIndex += 1
                        Case TokenType.QuotedStringAnsi, TokenType.QuotedStringUnicode
                            nextToken = list(nextIndex)
                            Exit While
                        Case Else
                            Exit While
                    End Select
                End While

                If nextToken IsNot Nothing Then
                    ' Create the new token
                    Dim first As String = list(index).Value
                    Dim second As String = nextToken.Value
                    Dim str As String = """" & _
                        first.Substring(1, first.Length - 2) & _
                        second.Substring(1, second.Length - 2) & """"

                    ' Remove all of the tokens between these two and the second string
                    list.RemoveRange(index, (nextIndex - index) + 1)
                    list.Insert(index, New Token(TokenType.QuotedStringAnsi, str))
                Else
                    index += 1
                End If
            End While
        End Sub

        Private Sub CollapseDoublePounds(ByVal list As List(Of Token))
            Dim i As Integer = 0
            While (i + 3) < list.Count
                Dim t1 As Token = list(i)
                Dim t2 As Token = list(i + 1)
                Dim t3 As Token = list(i + 2)
                Dim t4 As Token = list(i + 3)
                If t2.TokenType = TokenType.Pound AndAlso t3.TokenType = TokenType.Pound Then
                    list.RemoveRange(i, 4)
                    list.Insert(i, New Token(TokenType.Text, t1.Value & t4.Value))
                End If

                i += 1
            End While
        End Sub

#Region "Trace"

        Private Sub Trace(ByVal msg As String)
            If m_options.Trace Then
                m_outputStream.Write("// ")
                m_outputStream.WriteLine(msg)
            End If
        End Sub

        Private Sub TraceToStream(ByVal format As String, ByVal ParamArray args() As Object)
            If m_options.Trace Then
                Trace(String.Format(format, args))
            End If
        End Sub

        Private Sub TraceSkippedLine(ByVal line As PreprocessorLine)
            If m_options.Trace Then
                Trace(String.Format("Skipped: {0}", line.DisplayLine))
            End If
        End Sub

        Private Sub TraceMacroMap()
            If m_options.Trace Then
                Dim list As New List(Of Macro)(m_macroMap.Values)
                list.Sort(AddressOf TraceCompareMacros)
                Trace("Macro Map Dump")
                For Each cur As Macro In list
                    If cur.IsMethod Then
                        TraceToStream("{0} -> {1}", cur.Name, DirectCast(cur, MethodMacro).MethodSignature)
                    Else
                        TraceToStream("{0} -> {1}", cur.Name, cur.Value)
                    End If
                Next
            End If
        End Sub

        Private Shared Function TraceCompareMacros(ByVal x As Macro, ByVal y As Macro) As Integer
            Return String.CompareOrdinal(x.Name, y.Name)
        End Function

#End Region

        Private Sub WriteToStream(ByVal line As PreprocessorLine)
            For Each token As Token In line.TokenList
                m_outputStream.Write(token.Value)
            Next
        End Sub


        ''' <summary>
        ''' Chew through the current conditional branch.  Stop when the next valid
        ''' preprocessor branch is encountered.  This will chew through any nestede branches
        ''' with no regard for their content
        ''' 
        ''' When this method is finished, a valid pre-processor line will be the next available
        ''' line
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ChewThroughConditionalBranch()
            Dim done As Boolean = False
            Dim nestedIfCount As Integer = 0
            While Not done
                Dim line As PreprocessorLine = Me.PeekNextLine()
                If line.FirstValidToken.TokenType = TokenType.EndOfStream Then
                    Return
                End If

                Dim type As TokenType = line.FirstValidToken.TokenType
                If nestedIfCount = 0 Then
                    ' Not in a nested if, just look for the next valid preprocessor token
                    Select Case type
                        Case TokenType.PoundElse, TokenType.PoundElseIf, TokenType.PoundEndIf
                            done = True
                        Case TokenType.PoundIf, TokenType.PoundIfndef
                            nestedIfCount = +1
                    End Select
                Else
                    Select Case type
                        Case TokenType.PoundIf, TokenType.PoundIfndef
                            nestedIfCount += 1
                        Case TokenType.PoundEndIf
                            nestedIfCount -= 1
                    End Select
                End If

                ' If we're not done yet then chew through the line
                If Not done Then
                    TraceSkippedLine(line)
                    Me.GetNextLine()
                End If
            End While
        End Sub

        ''' <summary>
        ''' Chew completely through the remainder of the conditional.  Basically consume 
        ''' the #endif line to match the #if/#elsif we've already processed
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ChewThroughConditionalEnd()
            Dim done As Boolean = False
            While Not done
                ChewThroughConditionalBranch()

                Dim line As PreprocessorLine = Me.GetNextLine()
                TraceSkippedLine(line)
                If line.FirstValidToken.TokenType = TokenType.PoundEndIf Then
                    done = True
                End If
            End While
        End Sub

        Private Function ValidTokenListToString(ByVal enumerable As IEnumerable(Of Token)) As String
            Dim builder As New StringBuilder
            For Each token As Token In enumerable
                builder.Append(token.Value)
                builder.Append(" ")
            Next

            ' Remove the last space
            builder.Length -= 1
            Return builder.ToString()
        End Function

    End Class

End Namespace
