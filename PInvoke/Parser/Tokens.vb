' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Globalization

Namespace Parser

#Region "TokenType"
    Public Enum TokenType

        ' Pre-processor macros
        PoundDefine
        PoundIf
        PoundIfndef
        PoundUnDef
        PoundElse
        PoundElseIf
        PoundEndIf
        PoundInclude
        PoundPragma
        PoundError

        ' Delimeter tokens
        BraceOpen
        BraceClose
        ParenOpen
        ParenClose
        BracketOpen
        BracketClose
        Comma
        Semicolon
        Colon
        DoubleQuote
        SingleQuote
        Asterisk
        Period
        Bang
        Ampersand
        Pipe
        BackSlash
        Pound

        ' Operator tokens
        OpAssign
        OpEquals
        OpNotEquals
        OpGreaterThan
        OpLessThan
        OpGreaterThanOrEqual
        OpLessThanOrEqual
        OpBoolAnd
        OpBoolOr
        OpPlus
        OpMinus
        OpDivide
        OpModulus
        OpShiftLeft
        OpShiftRight

        WhiteSpace
        NewLine

        ' Reads through the comments
        LineComment
        BlockComment

        ' Different words
        Word
        Text
        QuotedStringAnsi
        QuotedStringUnicode
        CharacterAnsi
        CharacterUnicode
        Number
        HexNumber

        ' Keywords 
        StructKeyword
        UnionKeyword
        EnumKeyword
        ClassKeyword
        TypedefKeyword
        InlineKeyword
        VolatileKeyword
        ClrCallKeyword
        CDeclarationCallKeyword
        StandardCallKeyword
        PascalCallKeyword
        WinApiCallKeyword
        Pointer32Keyword
        Pointer64Keyword
        ConstKeyword
        TrueKeyword
        FalseKeyword
        PublicKeyword
        PrivateKeyword
        ProtectedKeyword
        SignedKeyword
        UnsignedKeyword

        ' Type Keywords
        BooleanKeyword
        ByteKeyword
        ShortKeyword
        Int16Keyword
        IntKeyword
        Int64Keyword
        LongKeyword
        CharKeyword
        WCharKeyword
        FloatKeyword
        DoubleKeyword
        VoidKeyword

        ' SAL
        DeclSpec

        ''' <summary>
        ''' End of the Stream we are scanning
        ''' </summary>
        ''' <remarks></remarks>
        EndOfStream

    End Enum

#End Region

#Region "Token"

    <DebuggerDisplay("{TokenType} - {Value}")> _
    Public Class Token
        Private _tokenType As TokenType
        Private _value As String

        ''' <summary>
        ''' Type of the token
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TokenType() As TokenType
            Get
                Return _tokenType
            End Get
        End Property

        Public ReadOnly Property Value() As String
            Get
                Return _value
            End Get
        End Property

        Public ReadOnly Property IsKeyword() As Boolean
            Get
                Return TokenHelper.IsKeyword(_tokenType)
            End Get
        End Property

        Public ReadOnly Property IsAnyWord() As Boolean
            Get
                Return _tokenType = Parser.TokenType.Word OrElse IsKeyword
            End Get
        End Property

        Public ReadOnly Property IsCallTypeModifier() As Boolean
            Get
                Return TokenHelper.IsCallTypeModifier(_tokenType)
            End Get
        End Property

        Public ReadOnly Property IsBinaryOperation() As Boolean
            Get
                Return TokenHelper.IsBinaryOperation(_tokenType)
            End Get
        End Property

        Public ReadOnly Property IsPreProcessorDirective() As Boolean
            Get
                Return TokenHelper.IsPreprocessorToken(_tokenType)
            End Get
        End Property

        Public ReadOnly Property IsCharacter() As Boolean
            Get
                Return TokenHelper.IsCharacter(_tokenType)
            End Get
        End Property

        Public ReadOnly Property IsQuotedString() As Boolean
            Get
                Return TokenHelper.IsQuotedString(_tokenType)
            End Get
        End Property

        Public ReadOnly Property IsNumber() As Boolean
            Get
                Return TokenHelper.IsNumber(_tokenType)
            End Get
        End Property

        Public ReadOnly Property IsAccessModifier() As Boolean
            Get
                Return TokenHelper.IsAccessModifier(_tokenType)
            End Get
        End Property

        Public ReadOnly Property IsTypeKeyword() As Boolean
            Get
                Return TokenHelper.IsTypeKeyword(_tokenType)
            End Get
        End Property

        Public Sub New(ByVal tType As TokenType, ByVal val As String)
            _tokenType = tType
            _value = val
        End Sub
    End Class

#End Region

#Region "TokenHelper"

    ''' <summary>
    ''' Helper methods for Tokens
    ''' </summary>
    ''' <remarks></remarks>
    Public Module TokenHelper

        Private s_keywordMap As Dictionary(Of String, TokenType)

        Public ReadOnly Property KeywordMap() As Dictionary(Of String, TokenType)
            Get
                If s_keywordMap Is Nothing Then
                    s_keywordMap = BuildKeywordMap()
                End If

                Return s_keywordMap
            End Get
        End Property

        Public Function IsKeyword(ByVal word As String) As Boolean
            Return KeywordMap.ContainsKey(word)
        End Function

        Public Function IsKeyword(ByVal tt As TokenType) As Boolean
            Return KeywordMap.ContainsValue(tt)
        End Function

        Public Function IsCallTypeModifier(ByVal tt As TokenType) As Boolean
            Select Case tt
                Case TokenType.ClrCallKeyword
                    Return True
                Case TokenType.InlineKeyword
                    Return True
                Case TokenType.CDeclarationCallKeyword
                    Return True
                Case TokenType.StandardCallKeyword
                    Return True
                Case TokenType.PascalCallKeyword
                    Return True
                Case TokenType.WinApiCallKeyword
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Private Function BuildKeywordMap() As Dictionary(Of String, TokenType)
            Dim keywordMap As New Dictionary(Of String, TokenType)(StringComparer.Ordinal)
            keywordMap("struct") = TokenType.StructKeyword
            keywordMap("union") = TokenType.UnionKeyword
            keywordMap("typedef") = TokenType.TypedefKeyword
            keywordMap("enum") = TokenType.EnumKeyword
            keywordMap("class") = TokenType.ClassKeyword
            keywordMap("__declspec") = TokenType.DeclSpec
            keywordMap("volatile") = TokenType.VolatileKeyword
            keywordMap("__inline") = TokenType.InlineKeyword
            keywordMap("__forceinline") = TokenType.InlineKeyword
            keywordMap("inline") = TokenType.InlineKeyword
            keywordMap("__clrcall") = TokenType.ClrCallKeyword
            keywordMap("__ptr32") = TokenType.Pointer32Keyword
            keywordMap("__ptr64") = TokenType.Pointer64Keyword
            keywordMap("const") = TokenType.ConstKeyword
            keywordMap("false") = TokenType.FalseKeyword
            keywordMap("true") = TokenType.TrueKeyword
            keywordMap("_cdecl") = TokenType.CDeclarationCallKeyword
            keywordMap("__cdecl") = TokenType.CDeclarationCallKeyword
            keywordMap("__stdcall") = TokenType.StandardCallKeyword
            keywordMap("__pascal") = TokenType.PascalCallKeyword
            keywordMap("__winapi") = TokenType.WinApiCallKeyword
            keywordMap("public") = TokenType.PublicKeyword
            keywordMap("private") = TokenType.PrivateKeyword
            keywordMap("protected") = TokenType.ProtectedKeyword

            ' type information
            keywordMap("signed") = TokenType.SignedKeyword
            keywordMap("unsigned") = TokenType.UnsignedKeyword

            ' Update builtin type map
            keywordMap.Add("boolean", TokenType.BooleanKeyword)
            keywordMap.Add("bool", TokenType.BooleanKeyword)
            keywordMap.Add("byte", TokenType.ByteKeyword)
            keywordMap.Add("short", TokenType.ShortKeyword)
            keywordMap.Add("__int16", TokenType.Int16Keyword)
            keywordMap.Add("int", TokenType.IntKeyword)
            keywordMap.Add("long", TokenType.LongKeyword)
            keywordMap.Add("__int32", TokenType.IntKeyword)
            keywordMap.Add("__int64", TokenType.Int64Keyword)
            keywordMap.Add("char", TokenType.CharKeyword)
            keywordMap.Add("wchar", TokenType.WCharKeyword)
            keywordMap.Add("float", TokenType.FloatKeyword)
            keywordMap.Add("double", TokenType.DoubleKeyword)
            keywordMap.Add("void", TokenType.VoidKeyword)

            ' Make sure to update iscalltypemodifier as well
            Return keywordMap
        End Function

        Public Function IsAnyWord(ByVal tt As TokenType) As Boolean
            Return tt = TokenType.Word OrElse IsKeyword(tt)
        End Function

        Private Structure NumberInfo
            Public Style As NumberStyles
            Public IsUnsigned As Boolean
            Public IsLong As Boolean
            Public IsFloatingPoint As Boolean
            Public IsOctal As Boolean
            Public IsForced64 As Boolean
            Public Exponent As Int32
        End Structure

        Public Function IsAccessModifier(ByVal tt As TokenType) As Boolean
            Select Case tt
                Case TokenType.PublicKeyword
                    Return True
                Case TokenType.PrivateKeyword
                    Return True
                Case TokenType.ProtectedKeyword
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Public Function IsTypeKeyword(ByVal tt As TokenType) As Boolean
            Select Case tt
                Case TokenType.BooleanKeyword
                    Return True
                Case TokenType.ByteKeyword
                    Return True
                Case TokenType.ShortKeyword
                    Return True
                Case TokenType.Int16Keyword
                    Return True
                Case TokenType.IntKeyword
                    Return True
                Case TokenType.Int64Keyword
                    Return True
                Case TokenType.LongKeyword
                    Return True
                Case TokenType.CharKeyword
                    Return True
                Case TokenType.WCharKeyword
                    Return True
                Case TokenType.FloatKeyword
                    Return True
                Case TokenType.DoubleKeyword
                    Return True
                Case TokenType.VoidKeyword
                    Return True
                Case TokenType.UnsignedKeyword
                    Return True
                Case TokenType.SignedKeyword
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Public Function IsNumber(ByVal tt As TokenType) As Boolean
            Return tt = TokenType.Number OrElse tt = TokenType.HexNumber
        End Function

        Public Function IsCharacter(ByVal tt As TokenType) As Boolean
            Return tt = TokenType.CharacterAnsi OrElse tt = TokenType.CharacterUnicode
        End Function

        Public Function IsQuotedString(ByVal tt As TokenType) As Boolean
            Return tt = TokenType.QuotedStringAnsi OrElse tt = TokenType.QuotedStringUnicode
        End Function

        ''' <summary>
        ''' Is this a type of binary operation.  For exampl +,-,/, etc ...
        ''' </summary>
        ''' <param name="tt"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsBinaryOperation(ByVal tt As TokenType) As Boolean
            Dim isvalid As Boolean = False
            Select Case tt
                Case TokenType.Ampersand
                    isvalid = True
                Case TokenType.Pipe
                    isvalid = True
                Case TokenType.Asterisk
                    isvalid = True
                Case TokenType.OpBoolAnd
                    isvalid = True
                Case TokenType.OpBoolOr
                    isvalid = True
                Case TokenType.OpDivide
                    isvalid = True
                Case TokenType.OpEquals
                    isvalid = True
                Case TokenType.OpNotEquals
                    isvalid = True
                Case TokenType.OpAssign
                    isvalid = True
                Case TokenType.OpGreaterThan
                    isvalid = True
                Case TokenType.OpLessThan
                    isvalid = True
                Case TokenType.OpGreaterThanOrEqual
                    isvalid = True
                Case TokenType.OpLessThanOrEqual
                    isvalid = True
                Case TokenType.OpMinus
                    isvalid = True
                Case TokenType.OpModulus
                    isvalid = True
                Case TokenType.OpPlus
                    isvalid = True
                Case TokenType.OpShiftLeft
                    isvalid = True
                Case TokenType.OpShiftRight
                    isvalid = True
            End Select

            Return isvalid
        End Function

        ''' <summary>
        ''' Is this a preprocessor token
        ''' </summary>
        ''' <param name="tt"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsPreprocessorToken(ByVal tt As TokenType) As Boolean
            Dim isValid As Boolean = False
            Select Case tt
                Case TokenType.PoundDefine
                    isValid = True
                Case TokenType.PoundElse
                    isValid = True
                Case TokenType.PoundElse
                    isValid = True
                Case TokenType.PoundElseIf
                    isValid = True
                Case TokenType.PoundEndIf
                    isValid = True
                Case TokenType.PoundError
                    isValid = True
                Case TokenType.PoundIf
                    isValid = True
                Case TokenType.PoundIfndef
                    isValid = True
                Case TokenType.PoundInclude
                    isValid = True
                Case TokenType.PoundPragma
                    isValid = True
                Case TokenType.PoundUnDef
                    isValid = True
            End Select

            Return isValid
        End Function

        Public Function ConvertToString(ByVal token As Token) As String
            Dim sValue As String = Nothing
            If Not TryConvertToString(token, sValue) Then
                Throw New InvalidOperationException("Unable to convert to string: " & token.Value)
            End If

            Return sValue
        End Function

        Public Function TryConvertToString(ByVal token As Token, ByRef str As String) As Boolean
            If Not token.IsQuotedString Then
                Return False
            End If

            str = token.Value
            If token.TokenType = TokenType.QuotedStringUnicode Then
                ThrowIfFalse(str(0) = "L")
                str = str.Substring(1)
            End If

            If str.Length < 2 OrElse str(0) <> """" OrElse str(str.Length - 1) <> """" Then
                Return False
            End If

            str = str.Substring(1, str.Length - 2)
            Return True
        End Function

        Public Function TryConvertToChar(ByVal token As Token, ByRef retChar As Char) As Boolean
            If Not token.IsCharacter Then
                Return False
            End If
            Dim val As String = token.Value

            ' Strip out the L
            If token.TokenType = TokenType.CharacterUnicode Then
                val = val.Substring(1)
            End If

            If String.IsNullOrEmpty(val) _
                    OrElse val.Length < 3 _
                    OrElse val(0) <> "'"c _
                    OrElse val(val.Length - 1) <> "'"c Then
                Return False
            End If

            val = val.Substring(1, val.Length - 2)  ' Strip the quotes
            If val(0) <> "\" Then
                Return Char.TryParse(val, retChar)
            End If

            ' Look for the simple escape codes
            Dim found As Boolean = False
            If val.Length = 2 Then
                found = True
                Select Case val(1)
                    Case "\"c
                        retChar = "\"c
                    Case "'"c
                        retChar = "'"c
                    Case """"c
                        retChar = """"c
                    Case "?"c
                        retChar = "?"c
                    Case "0"c
                        retChar = ControlChars.NullChar
                    Case "a"c
                        retChar = ChrW(7)
                    Case "b"c
                        retChar = ControlChars.Back
                    Case "f"c
                        retChar = ControlChars.FormFeed
                    Case "n"c
                        retChar = ChrW(10)
                    Case "r"c
                        retChar = ControlChars.Cr
                    Case "t"c
                        retChar = ControlChars.Tab
                    Case "v"c
                        retChar = ControlChars.VerticalTab
                    Case Else
                        found = False
                End Select
            End If

            If found Then
                Return True
            End If

            ' It's an escape sequence
            val = val.Substring(1)
            If String.IsNullOrEmpty(val) Then
                Return False
            End If

            Dim number As Object = Nothing
            If Char.ToLower(val(0)) = "x"c Then
                If Not TryConvertToNumber("0x" & val.Substring(1), number) Then
                    Return False
                End If
            ElseIf Char.ToLower(val(0)) = "u"c Then
                If Not TryConvertToNumber(val.Substring(1), number) Then
                    Return False
                End If
            Else
                If Not TryConvertToNumber(val, number) Then
                    Return False
                End If
            End If

            Try
                retChar = ChrW(CInt(number))
                Return True
            Catch ex As Exception
                Debug.Fail("Error converting to integer")
                Return False
            End Try
        End Function


        ''' <summary>
        ''' Try and convert the token into a number
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="val"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TryConvertToNumber(ByVal t As Token, ByRef val As Object) As Boolean
            Return TryConvertToNumber(t.Value, val)
        End Function

        ''' <summary>
        ''' Try convert the value to a number
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TryConvertToNumber(ByVal str As String, ByRef retValue As Object) As Boolean
            Dim info As NumberInfo
            If Not ProcessNumberInfo(str, info) Then
                Return False
            End If

            ' If this is an octal value then we need to convert the octal value into an int32 value and get 
            ' the string back as a base 10 number
            If info.IsOctal Then
                Dim base10Value As String = String.Empty
                If Not TryParseOctalNumber(str, info.IsUnsigned, base10Value) Then
                    Return False
                End If

                str = base10Value
            End If

            Dim ret As Boolean = True
            Dim val As Object = Nothing
            If info.IsFloatingPoint Then
                ' Mulitiplier is only valid for floating point numbers
                Dim mult As Long = 1
                If info.Exponent <> 0 Then
                    mult = CLng(Math.Pow(10, info.Exponent))
                End If

                Dim floatVal As Single = 0
                Dim doubleVal As Double = 0
                If Single.TryParse(str, info.Style, CultureInfo.CurrentCulture, floatVal) Then
                    val = CSng(floatVal * mult)
                ElseIf Double.TryParse(str, info.Style, CultureInfo.CurrentCulture, doubleVal) Then
                    val = CDbl(doubleVal * mult)
                Else
                    ret = False
                End If
            ElseIf info.IsUnsigned Then
                Dim uint32Value As UInt32 = 0
                Dim uint64Value As UInt64 = 0
                If Not info.IsForced64 AndAlso UInt32.TryParse(str, info.Style, CultureInfo.CurrentCulture, uint32Value) Then
                    val = uint32Value
                ElseIf UInt64.TryParse(str, info.Style, CultureInfo.CurrentCulture, uint64Value) Then
                    val = uint64Value
                Else
                    ret = False
                End If
            Else
                Dim int32Value As Int32 = 0
                Dim int64Value As Int64 = 0
                If Not info.IsForced64 AndAlso Int32.TryParse(str, info.Style, CultureInfo.CurrentCulture, int32Value) Then
                    val = int32Value
                ElseIf Int64.TryParse(str, info.Style, CultureInfo.CurrentCulture, int64Value) Then
                    val = int64Value
                Else
                    ret = False
                End If
            End If

            retValue = val
            Return ret
        End Function

        Private Function ProcessNumberInfo(ByRef str As String, ByRef info As NumberInfo) As Boolean
            ' Get the hex out of the number
            If str.StartsWith("0x", StringComparison.OrdinalIgnoreCase) Then
                str = str.Substring(2)
                info.Style = NumberStyles.HexNumber
            Else
                info.Style = NumberStyles.Number
            End If

            If String.IsNullOrEmpty(str) Then
                Return False
            End If

            ' if the number ends with u?i64 then it is a 64 bit type.  Just process the i64 here
            ' and the next loop will grab the U suffix
            If str.Length > 3 AndAlso str.EndsWith("i64", StringComparison.OrdinalIgnoreCase) Then
                info.IsForced64 = True
                str = str.Substring(0, str.Length - 3)
            End If


            ' If it ends with an LUF then we need to process that suffix 
            Do
                Dim last As Char = Char.ToLower(str(str.Length - 1))
                If last = "u"c Then
                    info.IsUnsigned = True
                ElseIf last = "f"c AndAlso info.Style <> NumberStyles.HexNumber Then
                    ' F is a valid number value in a hex number
                    info.IsFloatingPoint = True
                ElseIf last = "l"c Then
                    info.IsLong = True
                Else
                    Exit Do
                End If

                str = str.Substring(0, str.Length - 1)
                If String.IsNullOrEmpty(str) Then
                    Exit Do
                End If
            Loop

            ' Exponent is 0 unless there is an exponent.  Can't have an exponent with
            ' a hex number
            If info.Style <> NumberStyles.HexNumber Then
                For i As Integer = 0 To str.Length - 1
                    Dim cur As Char = Char.ToLower(str(i))
                    If cur = "e"c Then
                        If Not Int32.TryParse(str.Substring(i + 1), info.Exponent) Then
                            Return False
                        End If

                        info.IsFloatingPoint = True
                        str = str.Substring(0, i)
                        Exit For
                    ElseIf cur = "."c Then
                        info.IsFloatingPoint = True
                    End If
                Next

                ' Check for octal
                If str.Length > 0 AndAlso "0" = str(0) AndAlso Not info.IsFloatingPoint Then
                    info.IsOctal = True
                End If
            End If


            Return True
        End Function

        Public Function TryParseOctalNumber(ByVal number As String, ByVal isUnsigned As Boolean, ByRef base10Value As String) As Boolean
            If String.IsNullOrEmpty(number) Then
                base10Value = "0"
                Return True
            End If

            Dim exponent As Integer = 0
            Dim index As Integer = number.Length - 1
            Dim unsignedValue As UInt64 = 0
            Dim signedValue As UInt64 = 0
            While index >= 0
                Dim mult As Integer = CInt(Math.Pow(8, exponent))
                Dim digit As Integer = 0
                If Not Int32.TryParse(number(index), digit) Then
                    Return False
                End If

                If isUnsigned Then
                    unsignedValue += CULng(digit * mult)
                Else
                    signedValue += CULng(digit * mult)
                End If

                index -= 1
                exponent += 1
            End While

            If isUnsigned Then
                base10Value = unsignedValue.ToString()
            Else
                base10Value = signedValue.ToString()
            End If

            Return True
        End Function


        Public Function TryConvertToPoundToken(ByVal word As String, ByRef token As Token) As Boolean

            token = Nothing
            Select Case word.ToLower()
                Case "define"
                    token = New Token(TokenType.PoundDefine, "define")
                Case "include"
                    token = New Token(TokenType.PoundInclude, "include")
                Case "pragma"
                    token = New Token(TokenType.PoundPragma, "pragma")
                Case "if", "ifdef"
                    token = New Token(TokenType.PoundIf, "if")
                Case "ifndef"
                    token = New Token(TokenType.PoundIfndef, "ifndef")
                Case "else"
                    token = New Token(TokenType.PoundElse, "else")
                Case "elseif", "elif"
                    token = New Token(TokenType.PoundElseIf, "elseif")
                Case "endif"
                    token = New Token(TokenType.PoundEndIf, "endif")
                Case "undefine", "undef"
                    token = New Token(TokenType.PoundUnDef, "undef")
                Case "error"
                    token = New Token(TokenType.PoundError, "error")
            End Select

            Return token IsNot Nothing
        End Function

        Public Function TokenListToString(ByVal enumerable As IEnumerable(Of Token)) As String
            Dim builder As New Text.StringBuilder
            For Each cur As Token In enumerable
                builder.Append(cur.Value)
            Next

            Return builder.ToString()
        End Function
    End Module

#End Region

End Namespace
