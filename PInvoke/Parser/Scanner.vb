' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic
Imports System.IO
Imports System.text
Imports System.Text.RegularExpressions

Namespace Parser

    ''' <summary>
    ''' Options for the Scanner
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ScannerOptions
        Public ThrowOnEndOfStream As Boolean
        Public HideComments As Boolean
        Public HideWhitespace As Boolean
        Public HideNewLines As Boolean

        Public Sub New()

        End Sub
    End Class

    ''' <summary>
    ''' Used to mark a point in the scanner to which a caller can move back to
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ScannerMark
        Private m_index As Integer
        Private m_lineNumber As Integer

        Friend ReadOnly Property Index() As Integer
            Get
                Return m_index
            End Get
        End Property

        Friend ReadOnly Property LineNumber() As Integer
            Get
                Return m_lineNumber
            End Get
        End Property

        Friend Sub New(ByVal index As Integer, ByVal lineNumber As Integer)
            m_index = index
        End Sub

    End Class


    ''' <summary>
    ''' Scans the Stream for tokens of interest
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Scanner

#Region "ScannerInternalException"
        Private Class ScannerInternalException
            Inherits Exception

            Sub New(ByVal msg As String)
                MyBase.New(msg)
            End Sub

            Sub New(ByVal msg As String, ByVal inner As Exception)
                MyBase.New(msg, inner)
            End Sub
        End Class
#End Region

#Region "ScannerBuffer"

        <DebuggerDisplay("{Display}")> _
        Private Class ScannerBuffer
            Private m_text As String
            Private m_index As Integer
            Private m_lineNumber As Integer

            ''' <summary>
            ''' Used as a debugger display property.  Gives a preview of the location in the stream
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Display() As String
                Get
                    Dim startIndex As Integer = m_index - 10
                    Dim endIndex As Integer = m_index + 15
                    If startIndex <= 0 Then
                        startIndex = 0
                    End If

                    If endIndex >= m_text.Length Then
                        endIndex = m_text.Length - 1
                    End If

                    Dim value As String = m_text.Substring(startIndex, m_index - startIndex)
                    value &= "->"
                    value &= m_text.Substring(m_index, endIndex - m_index)
                    Return value
                End Get
            End Property

            ''' <summary>
            ''' Line Number of the buffer
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property LineNumber() As Integer
                Get
                    Return m_lineNumber
                End Get
            End Property

            ''' <summary>
            ''' True when we reach the end of the stream
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property EndOfStream() As Boolean
                Get
                    Return m_index >= m_text.Length
                End Get
            End Property

            Public Sub New(ByVal reader As TextReader)
                m_text = reader.ReadToEnd()
                m_index = 0
                m_lineNumber = 1
            End Sub

            ''' <summary>
            ''' Mark the place in the stream so we can rollback to that spot
            ''' </summary>
            ''' <remarks></remarks>
            Public Function Mark() As ScannerMark
                Return New ScannerMark(m_index, m_lineNumber)
            End Function

            ''' <summary>
            ''' Rolling back unsets the mark
            ''' </summary>
            ''' <remarks></remarks>
            Public Sub RollBack(ByVal mark As ScannerMark)
                ThrowIfNull(mark, "Must be passed a valid ScannerMark")
                m_index = mark.Index
                m_lineNumber = mark.LineNumber
            End Sub

            ''' <summary>
            ''' Get a char from the stream
            ''' </summary>
            ''' <remarks></remarks>
            Public Function ReadChar() As Char
                EnsureNotEndOfStream()
                Dim ret As Char = m_text(m_index)
                m_index += 1

                ' Check for the end of the line
                If ret = CChar(vbCr) Then
                    If m_index = m_text.Length Then
                        m_lineNumber += 1
                    ElseIf PeekChar() = CChar(vbLf) Then
                        m_lineNumber += 1
                    End If
                End If

                Return ret
            End Function

            ''' <summary>
            ''' Peek the next char off of the stream
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function PeekChar() As Char
                EnsureNotEndOfStream()
                Return m_text(m_index)
            End Function

            ''' <summary>
            ''' Moves past the next char in the stream.  Often used with
            ''' PeekChar
            ''' </summary>
            ''' <remarks></remarks>
            Public Sub EatChar()
                EnsureNotEndOfStream()
                m_index += 1
            End Sub

            Public Sub MoveBack(ByVal count As Integer)
                m_index -= count
                If m_index < 0 Then
                    Throw New ScannerInternalException("Moved back before the start of the Stream")
                End If
            End Sub

            Private Sub EnsureNotEndOfStream()
                If Me.EndOfStream Then
                    Throw New ScannerInternalException("EndOfStream encountered")
                End If
            End Sub
        End Class
#End Region

        Private m_errorProvider As New ErrorProvider
        Private m_options As ScannerOptions
        Private m_readerBag As TextReaderBag

        ''' <summary>
        ''' Stream we are reading from
        ''' </summary>
        ''' <remarks></remarks>
        Private m_buffer As ScannerBuffer

        ''' <summary>
        ''' Options for the Scanner
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Options() As ScannerOptions
            Get
                Return m_options
            End Get
        End Property

        ''' <summary>
        ''' What line number are we currently on
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LineNumber() As Integer
            Get
                Return m_buffer.LineNumber
            End Get
        End Property

        ''' <summary>
        ''' ErrorProvider for the instance
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ErrorProvider() As ErrorProvider
            Get
                Return m_errorProvider
            End Get
            Set(ByVal value As ErrorProvider)
                m_errorProvider = value
            End Set
        End Property

        ''' <summary>
        ''' Name of the file
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name() As String
            Get
                Return m_readerBag.Name
            End Get
        End Property

        ''' <summary>
        ''' Return whether or not we are at the end of the stream
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property EndOfStream() As Boolean
            Get
                If m_options.ThrowOnEndOfStream Then
                    Dim ret As Boolean
                    Try
                        m_options.ThrowOnEndOfStream = False
                        ret = Me.PeekNextToken().TokenType = TokenType.EndOfStream
                    Finally
                        m_options.ThrowOnEndOfStream = True
                    End Try
                    Return ret
                Else
                    Return Me.PeekNextToken().TokenType = TokenType.EndOfStream
                End If
            End Get
        End Property

        Public Sub New(ByVal reader As TextReader)
            MyClass.New(New TextReaderBag(reader), New ScannerOptions())
        End Sub

        Public Sub New(ByVal bag As TextReaderBag, ByVal options As ScannerOptions)
            ThrowIfNull(bag)
            ThrowIfNull(options)
            m_readerBag = bag
            m_buffer = New ScannerBuffer(bag.TextReader)
            m_options = options
        End Sub



        ''' <summary>
        ''' Get the next token from the stream
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetNextToken() As Token

            Dim ret As Token = Nothing
            Dim done As Boolean = False
            Do
                ' Easy cases are out of the way.  Now we need to actually go ahead and 
                ' parse out the next token.  Mark the stream so that if scanning fails
                ' we can send back a token of the remainder of the line
                Dim mark As ScannerMark = m_buffer.Mark()
                Try
                    ret = GetNextTokenImpl()
                Catch ex As ScannerInternalException
                    AddWarning(ex.Message)
                    m_buffer.RollBack(mark)
                    ret = New Token(TokenType.Text, SafeReadTillEndOfLine())
                End Try

                done = True ' Done unless we find out otherwise
                Select Case ret.TokenType
                    Case TokenType.EndOfStream
                        If Me.Options.ThrowOnEndOfStream Then
                            Throw New EndOfStreamException("Scanner reached the end of the stream")
                        End If

                    Case TokenType.BlockComment, TokenType.LineComment
                        If Me.Options.HideComments Then
                            done = False
                        End If

                    Case TokenType.NewLine
                        If Me.Options.HideNewLines Then
                            done = False
                        End If

                    Case TokenType.WhiteSpace
                        If Me.Options.HideWhitespace Then
                            done = False
                        End If
                End Select
            Loop Until done

            Return ret
        End Function

        ''' <summary>
        ''' Peek the next token in the stream
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function PeekNextToken() As Token
            Dim token As Token
            Dim mark As ScannerMark = m_buffer.Mark()
            Try
                token = GetNextToken()
            Finally
                m_buffer.RollBack(mark)
            End Try

            Return token
        End Function

        ''' <summary>
        ''' Peek a list of tokens from the stream.  Don't throw when doing an extended peek
        ''' </summary>
        ''' <param name="count"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function PeekTokenList(ByVal count As Integer) As List(Of Token)
            Dim mark As ScannerMark = Me.Mark()
            Dim oldThrow As Boolean = m_options.ThrowOnEndOfStream
            m_options.ThrowOnEndOfStream = False
            Try
                Dim list As New List(Of Token)
                For i As Integer = 0 To count - 1
                    list.Add(Me.GetNextToken())
                Next

                Return list
            Finally
                m_options.ThrowOnEndOfStream = oldThrow
                Me.Rollback(mark)
            End Try
        End Function

        ''' <summary>
        ''' Get the next token that is not one of the specified types.  If EndOfStream
        ''' is specified it will be ignored.
        ''' </summary>
        ''' <param name="types"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetNextTokenNotOfType(ByVal ParamArray types As TokenType()) As Token
            Dim token As Token = GetNextToken()

            While Array.IndexOf(types, token.TokenType) >= 0 AndAlso token.TokenType <> TokenType.EndOfStream
                token = GetNextToken()
            End While

            Return token
        End Function

        ''' <summary>
        ''' Peek the next token not of th specified type
        ''' </summary>
        ''' <param name="types"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function PeekNextTokenNotOfType(ByVal ParamArray types As TokenType()) As Token
            Dim mark As ScannerMark = m_buffer.Mark()
            Dim token As Token
            Try
                token = GetNextTokenNotOfType(types)
            Finally
                m_buffer.RollBack(mark)
            End Try

            Return token
        End Function

        ''' <summary>
        ''' Get the next token and expect it to be of the specified type
        ''' </summary>
        ''' <param name="tt"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetNextToken(ByVal tt As TokenType) As Token
            Dim token As Token = GetNextToken()
            If token.TokenType <> tt Then
                Dim msg As String = String.Format("Expected token of type {0} but found {1} instead.", tt, token.TokenType)
                Throw New InvalidOperationException(msg)
            End If

            Return token
        End Function

        ''' <summary>
        ''' Mark the point in the Scanner so we can jump back to it at a later
        ''' time
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Mark() As ScannerMark
            Return m_buffer.Mark()
        End Function

        ''' <summary>
        ''' Rollback to the specified mark
        ''' </summary>
        ''' <param name="mark"></param>
        ''' <remarks></remarks>
        Public Sub Rollback(ByVal mark As ScannerMark)
            m_buffer.RollBack(mark)
        End Sub

        ''' <summary>
        ''' Tokenize the remainder of the stream and return the result
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Tokenize() As List(Of Token)
            Dim list As New List(Of Token)
            While Not Me.EndOfStream
                list.Add(Me.GetNextToken())
            End While

            Return list
        End Function

        ''' <summary>
        ''' Parse the next token out of the stream.  This does not consider any of the 
        ''' Options and instead returns the next token period.  Callers must take 
        ''' care to process the options correctly
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetNextTokenImpl() As Token

            ' First check and see if we're at 'EndOfStream'.  
            If m_buffer.EndOfStream Then
                Return New Token(TokenType.EndOfStream, String.Empty)
            End If

            Dim token As Token = Nothing
            Dim c As Char = m_buffer.ReadChar()

            ' First check for whitespace and return that
            If Char.IsWhiteSpace(c) _
                AndAlso c <> vbCr _
                AndAlso c <> vbLf Then

                m_buffer.MoveBack(1)
                Return ReadWhitespace()
            End If

            ' Use the first character to get the easy cases out of the way
            Select Case c
                Case "#"c
                    token = ReadPoundToken()
                Case "{"c
                    token = New Token(TokenType.BraceOpen, "{")
                Case "}"c
                    token = New Token(TokenType.BraceClose, "}")
                Case "("c
                    token = New Token(TokenType.ParenOpen, "(")
                Case ")"c
                    token = New Token(TokenType.ParenClose, ")")
                Case "["c
                    token = New Token(TokenType.BracketOpen, "[")
                Case "]"c
                    token = New Token(TokenType.BracketClose, "]")
                Case ","c
                    token = New Token(TokenType.Comma, ",")
                Case "\"c
                    token = New Token(TokenType.BackSlash, "\")

                    ' Operators 
                Case "+"c
                    token = New Token(TokenType.OpPlus, "+")
                Case "-"c
                    token = New Token(TokenType.OpMinus, "-")
                Case ";"c
                    token = New Token(TokenType.Semicolon, ";")
                Case "*"c
                    token = New Token(TokenType.Asterisk, "*")
                Case "."c
                    token = New Token(TokenType.Period, ".")
                Case ":"c
                    token = New Token(TokenType.Colon, ":")
                Case """"c
                    token = ReadDoubleQuoteOrString()
                Case "'"c
                    token = ReadSingleQuoteOrCharacter()
                Case CChar(vbCr), CChar(vbLf)
                    If Not m_buffer.EndOfStream AndAlso m_buffer.PeekChar() = vbLf Then
                        m_buffer.EatChar()
                    End If
                    Return New Token(TokenType.NewLine, vbCrLf)
            End Select

            ' If we found a token then return it
            If token IsNot Nothing Then
                Return token
            End If

            ' We've gotten past the characters that can be determined by the first character.  Now 
            ' we need to consider the second character as well.  Do an EndOfStream check here 
            ' since there could just be a single character left in the Stream
            If Not m_buffer.EndOfStream Then
                Dim c2 As String = m_buffer.ReadChar()
                Dim both As String = c & c2
                Select Case both
                    Case "//"
                        token = ReadLineComment()
                    Case "/*"
                        token = ReadBlockComment()
                    Case "&&"
                        token = New Token(TokenType.OpBoolAnd, "&&")
                    Case "||"
                        token = New Token(TokenType.OpBoolOr, "||")
                    Case "<="
                        token = New Token(TokenType.OpLessThanOrEqual, "<=")
                    Case ">="
                        token = New Token(TokenType.OpGreaterThanOrEqual, ">=")
                    Case "<<"
                        token = New Token(TokenType.OpShiftLeft, "<<")
                    Case ">>"
                        token = New Token(TokenType.OpShiftRight, ">>")
                    Case "=="
                        token = New Token(TokenType.OpEquals, "==")
                    Case "!="
                        token = New Token(TokenType.OpNotEquals, "!=")
                    Case "L'"
                        token = ReadWideCharacterOrSingleL()
                    Case "L"""
                        token = ReadWideStringOrSingleL()
                End Select

                ' If we found a token then return it
                If token IsNot Nothing Then
                    Return token
                End If

                ' Move back the character since we didn't process it
                m_buffer.MoveBack(1)
            End If

            ' There are several single character cases that are also a part of the double character 
            ' case.  For ease of reading process those now as a simple select case
            Select Case c
                Case "/"c
                    token = New Token(TokenType.OpDivide, "/")
                Case "|"c
                    token = New Token(TokenType.Pipe, "|")
                Case "&"c
                    token = New Token(TokenType.Ampersand, "&")
                Case "<"c
                    token = New Token(TokenType.OpLessThan, "<")
                Case ">"c
                    token = New Token(TokenType.OpGreaterThan, ">")
                Case "%"c
                    token = New Token(TokenType.OpModulus, "%")
                Case "="c
                    token = New Token(TokenType.OpAssign, "=")
                Case "!"c
                    token = New Token(TokenType.Bang, "!")
            End Select

            ' If we found a token then return it
            If token IsNot Nothing Then
                Return token
            End If

            ' If this isn't a letter or digit then return this as junk
            If Not Char.IsLetterOrDigit(c) AndAlso c <> "_"c Then
                Return New Token(TokenType.Text, CStr(c))
            End If

            ' This isn't a special token.  It's some type of word or number so move back to 
            ' the start of this stream and read the word.
            m_buffer.MoveBack(1)
            Return ReadWordOrNumberToken()
        End Function

        ''' <summary>
        ''' Read the whitespace into a token
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ReadWhitespace() As Token
            Dim builder As New StringBuilder()
            Dim done As Boolean = False
            Do
                Dim c As Char = m_buffer.ReadChar()
                If Not Char.IsWhiteSpace(c) Then
                    done = True
                    m_buffer.MoveBack(1)
                ElseIf c = vbCr OrElse c = vbLf Then
                    done = True
                    m_buffer.MoveBack(1)
                Else
                    builder.Append(c)
                End If
            Loop Until done OrElse m_buffer.EndOfStream

            Return New Token(TokenType.WhiteSpace, builder.ToString())
        End Function

        Private Function ReadWordOrNumberToken() As Token
            Dim word As String = ReadWord()

            ' First check and see if this is a keyword that we care about
            Dim keywordType As TokenType
            If TokenHelper.KeywordMap.TryGetValue(word, keywordType) Then
                Return New Token(keywordType, word)
            End If

            Dim numberType As TokenType = TokenType.Ampersand
            If IsNumber(word, numberType) Then

                ' Loop for a floating point number literal
                If Not m_buffer.EndOfStream AndAlso m_buffer.PeekChar() = "."c Then
                    Dim mark As ScannerMark = m_buffer.Mark()

                    m_buffer.ReadChar()
                    Dim fullWord As String = word & "." & ReadWord()
                    Dim fullNumberType As TokenType = TokenType.Ampersand
                    If IsNumber(fullWord, fullNumberType) Then
                        Return New Token(fullNumberType, fullWord)
                    Else
                        m_buffer.RollBack(mark)
                    End If
                End If

                Return New Token(numberType, word)
            End If

            ' Just a plain word
            Return New Token(TokenType.Word, word)
        End Function

        Private Function IsNumber(ByVal word As String, ByRef tt As TokenType) As Boolean
            ' Now parse out pattern words
            If Regex.IsMatch(word, "^[0-9.]+(e[0-9]+)?(([UFL]+)|(u?i64))?$", RegexOptions.Compiled Or RegexOptions.IgnoreCase) Then
                tt = TokenType.Number
                Return True
            ElseIf Regex.IsMatch(word, "^0x[0-9a-f.]+(e[0-9]+)?(([UFL]+)|(u?i64))?$", RegexOptions.Compiled Or RegexOptions.IgnoreCase) Then
                tt = TokenType.HexNumber
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Reads till the end of the line or stream.  Will not actuall consume the end
        ''' of line token
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SafeReadTillEndOfLine() As String
            Dim builder As New StringBuilder()
            Dim done As Boolean = False

            While Not done AndAlso Not m_buffer.EndOfStream
                Dim c As Char = m_buffer.PeekChar()
                If c = vbCr Or c = vbLf Then
                    done = True
                Else
                    builder.Append(c)
                    m_buffer.ReadChar()
                End If
            End While

            Return builder.ToString()
        End Function

        ''' <summary>
        ''' Read a word from the stream
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ReadWord() As String
            Dim builder As New StringBuilder()
            Dim done As Boolean = False

            While Not done AndAlso Not m_buffer.EndOfStream
                Dim c As Char = m_buffer.PeekChar()
                If Char.IsLetterOrDigit(c) _
                    OrElse c = "_" _
                    OrElse c = "$" Then
                    builder.Append(c)
                    m_buffer.EatChar()
                Else
                    done = True
                End If
            End While

            Return builder.ToString()
        End Function

        ''' <summary>
        ''' A # is already read, go ahead and read the text of the pound token
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ReadPoundToken() As Token
            Dim mark As ScannerMark = Me.Mark()
            Dim word As String = ReadWord()
            Dim token As Token = Nothing
            If TokenHelper.TryConvertToPoundToken(word, token) Then
                Return token
            End If

            ' The word didn't match any of our pound tokens so just return the pound
            Me.Rollback(mark)
            Return New Token(TokenType.Pound, "#")
        End Function


        ''' <summary>
        ''' A '//' has already been read from the stream.  Read the rest of the comment
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ReadLineComment() As Token
            Dim comment As String = SafeReadTillEndOfLine()
            Return New Token(TokenType.LineComment, "//" & comment)
        End Function

        ''' <summary>
        ''' Read a block comment.  The '/*' has already been read
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ReadBlockComment() As Token
            Dim builder As New StringBuilder()
            Dim done As Boolean = False
            builder.Append("/*")

            While Not done AndAlso Not m_buffer.EndOfStream
                Dim c As Char = m_buffer.ReadChar()
                If (c = "*"c AndAlso m_buffer.PeekChar() = "/"c) Then
                    builder.Append("*/")
                    m_buffer.EatChar()  ' Eat the /
                    done = True
                Else
                    builder.Append(c)
                End If
            End While

            Return New Token(TokenType.BlockComment, builder.ToString())
        End Function

        ''' <summary>
        ''' Read a quote or a string from the stream.  The initial quote has already been
        ''' read
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ReadDoubleQuoteOrString() As Token
            Dim mark As ScannerMark = m_buffer.Mark()
            Try
                Dim builder As New StringBuilder
                builder.Append(""""c)

                Dim done As Boolean = False
                While Not done
                    Dim c As Char = m_buffer.ReadChar()
                    Select Case c
                        Case """"c
                            builder.Append(""""c)
                            done = True
                        Case "\"c
                            builder.Append(c)
                            builder.Append(m_buffer.ReadChar())
                        Case Else
                            builder.Append(c)
                    End Select
                End While

                Return New Token(TokenType.QuotedStringAnsi, builder.ToString())
            Catch ex As ScannerInternalException
                ' If we get a scanner exception while trying to read the string then this
                ' is just a simple quote.  Rollback the buffer and return the quote token
                m_buffer.RollBack(mark)
                Return New Token(TokenType.DoubleQuote, """"c)
            End Try
        End Function

        ''' <summary>
        ''' Read a single quote or a character.  The initial single quote has already been
        ''' read
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ReadSingleQuoteOrCharacter() As Token
            If m_buffer.EndOfStream Then
                Return New Token(TokenType.SingleQuote, "'")
            End If

            Dim mark As ScannerMark = m_buffer.Mark()
            Dim token As Token = Nothing
            Try
                Dim data As Char = m_buffer.ReadChar()
                If data <> "\"c Then
                    If m_buffer.ReadChar() = "'"c Then
                        token = New Token(TokenType.CharacterAnsi, "'"c & data.ToString() & "'"c)
                    End If
                Else
                    Dim builder As New StringBuilder()
                    builder.Append(data)

                    Do
                        data = m_buffer.ReadChar()
                        If data = "'"c Then
                            token = New Token(TokenType.CharacterAnsi, "'"c & builder.ToString() & "'"c)
                            Exit Do
                        ElseIf m_buffer.EndOfStream OrElse builder.Length > 5 Then
                            Exit Do
                        Else
                            builder.Append(data)
                        End If
                    Loop
                End If
            Catch ex As ScannerInternalException
                ' Swallow the exception.  It will rollbakc when the token variable is not set
            End Try

            If token Is Nothing Then
                m_buffer.RollBack(mark)
                token = New Token(TokenType.SingleQuote, "'")
            End If

            Return token
        End Function

        ''' <summary>
        ''' Called when we hit an L' in the stream.  The buffer is pointed after the text
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ReadWideCharacterOrSingleL() As Token
            ' If we read a single quote then there was no valid character after the L.  Rollback
            ' the quote and return the word "L"
            Dim token As Token = ReadSingleQuoteOrCharacter()
            If token.TokenType = TokenType.SingleQuote Then
                m_buffer.MoveBack(1)
                Return New Token(TokenType.Word, "L")
            Else
                ThrowIfFalse(token.TokenType = TokenType.CharacterAnsi)
                Return New Token(TokenType.CharacterUnicode, "L" & token.Value)
            End If
        End Function

        ''' <summary>
        ''' Called when we hit an L" in the stream.  The buffer is pointed after the text 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ReadWideStringOrSingleL() As Token
            Dim token As Token = ReadDoubleQuoteOrString()
            If token.TokenType = TokenType.DoubleQuote Then
                ' Read a double quote which means there wasn't a valid string afterwards.  Move
                ' back over the " and return the L
                m_buffer.MoveBack(1)
                Return New Token(TokenType.Word, "L")
            Else
                ThrowIfFalse(token.TokenType = TokenType.QuotedStringAnsi)
                Return New Token(TokenType.QuotedStringUnicode, "L" & token.Value)
            End If
        End Function

        Public Shared Function TokenizeText(ByVal text As String) As List(Of Token)
            Return TokenizeText(text, New ScannerOptions())
        End Function

        Public Shared Function TokenizeText(ByVal text As String, ByVal opts As ScannerOptions) As List(Of Token)
            Using reader As New StringReader(text)
                Dim s As New Scanner(New TextReaderBag(reader), opts)
                Return s.Tokenize()
            End Using
        End Function

#Region "Error Message Helpers"

        Public Sub AddError(ByVal msg As String)
            m_errorProvider.AddError(GetMessagePrefix() & msg)
        End Sub

        Public Sub AddError(ByVal format As String, ByVal ParamArray args() As Object)
            AddError(String.Format(format, args))
        End Sub

        Public Sub AddWarning(ByVal msg As String)
            m_errorProvider.AddWarning(GetMessagePrefix() & msg)
        End Sub

        Public Sub AddWarning(ByVal format As String, ByVal ParamArray args() As Object)
            AddWarning(String.Format(format, args))
        End Sub

        Private Function GetMessagePrefix() As String
            Return String.Format("{0} {1}: ", m_readerBag.Name, m_buffer.LineNumber)
        End Function

#End Region

    End Class

End Namespace
