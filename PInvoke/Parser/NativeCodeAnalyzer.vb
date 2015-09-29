' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.RegularExpressions
Imports PInvoke.Contract

Namespace Parser

#Region "NativeCodeAnalyerResult"

    ''' <summary>
    ''' Result of analyzing the Native Code
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NativeCodeAnalyzerResult
        Private _macroMap As New Dictionary(Of String, Macro)
        Private _typedefList As New List(Of NativeTypeDef)
        Private _definedTypeListt As New List(Of NativeDefinedType)
        Private _procList As New List(Of NativeProcedure)
        Private _constList As New List(Of NativeConstant)
        Private _ep As New ErrorProvider

        ''' <summary>
        ''' Final set of the macros once the code is analyzed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MacroMap() As Dictionary(Of String, Macro)
            Get
                Return _macroMap
            End Get
        End Property

        ''' <summary>
        ''' Map of #typedefs encountered in the code
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NativeTypeDefs() As List(Of NativeTypeDef)
            Get
                Return _typedefList
            End Get
        End Property

        ''' <summary>
        ''' Map of defined types encountered in the code
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NativeDefinedTypes() As List(Of NativeDefinedType)
            Get
                Return _definedTypeListt
            End Get
        End Property

        ''' <summary>
        ''' List of NativeProcedure instances parsed from the code
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
        ''' List of NativeConstants that were parsed out of the file
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NativeConstants() As List(Of NativeConstant)
            Get
                Return _constList
            End Get
        End Property

        ''' <summary>
        ''' Combination of both the typed constants and the macros that are converted into
        ''' constants
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AllNativeConstants() As List(Of NativeConstant)
            Get
                Dim list As New List(Of NativeConstant)
                list.AddRange(NativeConstants)
                list.AddRange(ConvertMacrosToConstants())
                Return list
            End Get
        End Property

        ''' <summary>
        ''' ErrorProvider for the result
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ErrorProvider() As ErrorProvider
            Get
                Return _ep
            End Get
        End Property

        Public Sub New()

        End Sub

        ''' <summary>
        ''' Convert the macros in the result into a list of constants
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ConvertMacrosToConstants() As List(Of NativeConstant)
            Dim list As New List(Of NativeConstant)

            For Each macro As Macro In _macroMap.Values
                If macro.IsMethod Then
                    Dim method As MethodMacro = DirectCast(macro, MethodMacro)
                    list.Add(New NativeConstant(macro.Name, method.MethodSignature, ConstantKind.MacroMethod))
                Else
                    list.Add(New NativeConstant(macro.Name, macro.Value))
                End If
            Next

            Return list
        End Function

    End Class

#End Region

#Region "NativeCodeAnalyzer"

    ''' <summary>
    ''' This is the main class used to analyze native code files.  It wraps all of the other
    ''' phases of analysis and provides a simple engine and events that can be hooked into
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NativeCodeAnalyzer
        Private _includePathList As New List(Of String)
        Private _followIncludes As Boolean = True
        Private _customIncludeMap As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Private _initialMacroList As New List(Of Macro)
        Private _includeInitialMacroInResult As Boolean = True
        Private _trace As Boolean

        ''' <summary>
        ''' Whether or not #includes should be followed when encountered
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property InitialMacroList() As IEnumerable(Of Macro)
            Get
                Return _initialMacroList
            End Get
        End Property

        ''' <summary>
        ''' Whether or not the analyzer should follow #includes it finds
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FollowIncludes() As Boolean
            Get
                Return _followIncludes
            End Get
            Set(ByVal value As Boolean)
                _followIncludes = value
            End Set
        End Property

        ''' <summary>
        ''' List of paths to search when following #include directives
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IncludePathList() As List(Of String)
            Get
                Return _includePathList
            End Get
        End Property

        ''' <summary>
        ''' Trace the various parts of analysis
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Trace() As Boolean
            Get
                Return _trace
            End Get
            Set(ByVal value As Boolean)
                _trace = value
            End Set
        End Property

        Public Property IncludeInitialMacrosInResult() As Boolean
            Get
                Return _includeInitialMacroInResult
            End Get
            Set(ByVal value As Boolean)
                _includeInitialMacroInResult = value
            End Set
        End Property

        Public Sub New()

        End Sub

        Public Sub AddInitialMacro(ByVal m As Macro)
            m.IsFromParse = False
            _initialMacroList.Add(m)
        End Sub

        ''' <summary>
        ''' Analyze the passed in file
        ''' </summary>
        ''' <param name="filePath"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Analyze(ByVal filePath As String) As NativeCodeAnalyzerResult
            If String.IsNullOrEmpty(filePath) Then
                Throw New ArgumentNullException("path")
            End If

            Using reader As New StreamReader(filePath)
                Return AnalyzeImpl(New TextReaderBag(filePath, reader))
            End Using

        End Function

        ''' <summary>
        ''' Analyze the passed in stream
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Analyze(ByVal reader As TextReader) As NativeCodeAnalyzerResult
            If reader Is Nothing Then
                Throw New ArgumentNullException("reader")
            End If

            Return AnalyzeImpl(New TextReaderBag(reader))
        End Function

        ''' <summary>
        ''' Run the preprocessor on the specefied file and return a Stream to the resulting
        ''' data
        ''' </summary>
        ''' <param name="filePath"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunPreProcessor(ByVal filePath As String) As TextReaderBag
            If String.IsNullOrEmpty(filePath) Then
                Throw New ArgumentNullException("filePath")
            End If

            Dim result As New NativeCodeAnalyzerResult()
            Using fileStream As New StreamReader(filePath)
                Return RunPreProcessorImpl(result, New TextReaderBag(filePath, fileStream))
            End Using
        End Function

        Private Function AnalyzeImpl(ByVal readerbag As TextReaderBag) As NativeCodeAnalyzerResult
            ThrowIfNull(readerbag)

            Dim result As New NativeCodeAnalyzerResult()

            ' Run the procprocessor and get the resulting Textreader
            Dim readerBag2 As TextReaderBag = Me.RunPreProcessorImpl(result, readerbag)
            Using readerBag2.TextReader

                ' Run the parser 
                Me.RunParser(result, readerBag2)
            End Using

            Return result
        End Function

        ''' <summary>
        ''' Run the PreProcessor on the stream
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="readerBag"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function RunPreProcessorImpl(ByVal result As NativeCodeAnalyzerResult, ByVal readerBag As TextReaderBag) As TextReaderBag
            ThrowIfNull(result)
            ThrowIfNull(readerBag)

            ' Create the options
            Dim opts As New PreProcessorOptions()
            opts.FollowIncludes = Me.FollowIncludes
            opts.IncludePathList.AddRange(Me.IncludePathList)
            opts.InitialMacroList.AddRange(_initialMacroList)
            opts.Trace = Me.Trace

            Dim preprocessor As New PreProcessorEngine(opts)

            ' Process the file
            Dim ret As String = preprocessor.Process(readerBag)

            ' Process the results
            result.ErrorProvider.Append(preprocessor.ErrorProvider)
            For Each pair As KeyValuePair(Of String, Macro) In preprocessor.MacroMap
                If _includeInitialMacroInResult OrElse pair.Value.IsFromParse Then
                    result.MacroMap.Add(pair.Key, pair.Value)
                End If
            Next

            Return New TextReaderBag(New StringReader(ret))
        End Function

        ''' <summary>
        ''' Run the actual parser on the stream
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="readerBag"></param>
        ''' <remarks></remarks>
        Private Sub RunParser(ByVal result As NativeCodeAnalyzerResult, ByVal readerBag As TextReaderBag)
            ThrowIfNull(readerBag)

            ' Perform the parse
            Dim parser As New ParseEngine()
            Dim parseResult As ParseResult = parser.Parse(readerBag)

            ' add in the basic results
            result.ErrorProvider.Append(parseResult.ErrorProvider)

            ' Add in all of the parsed out types
            result.NativeDefinedTypes.AddRange(parseResult.NativeDefinedTypes)
            result.NativeTypeDefs.AddRange(parseResult.NativeTypedefs)
            result.NativeProcedures.AddRange(parseResult.NativeProcedures)
        End Sub

    End Class

#End Region

#Region "NativeCodeAnalyzerFactory"

    ''' <summary>
    ''' Os Version
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum OsVersion
        Windows2000
        WindowsXP
        Windows2003
        WindowsVista
    End Enum

    ''' <summary>
    ''' Factory for creating a NativeCodeAnalyzer based on common configurations
    ''' </summary>
    ''' <remarks></remarks>
    Public Module NativeCodeAnalyzerFactory

        ''' <summary>
        ''' This will create an analyzer for parsing out a full header file
        ''' </summary>
        ''' <param name="osVersion"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal osVersion As OsVersion) As NativeCodeAnalyzer
            Dim analyzer As New NativeCodeAnalyzer()
            Debug.Assert(analyzer.IncludeInitialMacrosInResult) ' Should be the default

            ' Ignore 64 bit settings.  64 bit types are emitted as SysInt or IntPtr
            analyzer.AddInitialMacro(New Macro("__w64", String.Empty))

            analyzer.AddInitialMacro(New Macro("_X86_", String.Empty))
            analyzer.AddInitialMacro(New Macro("_WIN32", String.Empty))
            analyzer.AddInitialMacro(New Macro("WINAPI", "__winapi", True))
            analyzer.AddInitialMacro(New Macro("UNICODE", "1"))
            analyzer.AddInitialMacro(New Macro("__STDC__", "1"))

            ' Add the operating system macros
            AddOSInformation(analyzer, osVersion)

            ' Common information
            AddCommonMacros(analyzer)
            Return analyzer
        End Function

        ''' <summary>
        ''' Sometimes you want to parse out a snippet of code without doing the full windows
        ''' parse.  In this case you'll need to have certain Macros already defined since
        ''' they are defined at the begining of the windows header files.  This will add
        ''' in all of those macros
        ''' </summary>
        ''' <param name="osVersion"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateForMiniParse(ByVal osVersion As OsVersion, ByVal initialMacroList As IEnumerable(Of Macro)) As NativeCodeAnalyzer
            Dim analyzer As NativeCodeAnalyzer = Create(osVersion)
            Debug.Assert(analyzer.IncludeInitialMacrosInResult) ' Should be the default

            analyzer.IncludeInitialMacrosInResult = False
            For Each m As Macro In initialMacroList
                analyzer.AddInitialMacro(m)
            Next

            Return analyzer
        End Function

        Private Sub AddOSInformation(ByVal analyzer As NativeCodeAnalyzer, ByVal os As OsVersion)
            Select Case os
                Case OsVersion.WindowsXP
                    analyzer.AddInitialMacro(New Macro("WINVER", "0x0501"))
                    analyzer.AddInitialMacro(New Macro("_WIN32_WINNT", "0x0501"))
                Case OsVersion.Windows2000
                    analyzer.AddInitialMacro(New Macro("WINVER", "0x0500"))
                    analyzer.AddInitialMacro(New Macro("_WIN32_WINNT", "0x0500"))
                Case OsVersion.Windows2003
                    analyzer.AddInitialMacro(New Macro("WINVER", "0x0502"))
                    analyzer.AddInitialMacro(New Macro("_WIN32_WINNT", "0x0502"))
                Case OsVersion.WindowsVista
                    analyzer.AddInitialMacro(New Macro("WINVER", "0x0600"))
                    analyzer.AddInitialMacro(New Macro("_WIN32_WINNT", "0x0600"))
                Case Else
                    InvalidEnumValue(os)
            End Select
        End Sub

        Private Sub AddCommonMacros(ByVal analyzer As NativeCodeAnalyzer)

            ' MCS Version
            analyzer.AddInitialMacro(New Macro("_MSC_VER", "9999"))
            analyzer.AddInitialMacro(New Macro("_MSC_FULL_VER", "99999999"))

            ' Make sure that SAL is imported
            analyzer.AddInitialMacro(New Macro("_PREFAST_", String.Empty))
        End Sub

        Public Function GetCommonSdkPaths() As List(Of String)
            Dim list As New List(Of String)
            list.Add(GetPlatformSdkIncludePath())
            list.Add(GetSdkIncludePath())
            Return list
        End Function

        ''' <summary>
        ''' Get the path to the platform SDK include files
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetPlatformSdkIncludePath() As String
            Return Path.Combine( _
                Environment.GetEnvironmentVariable("ProgramFiles"), _
                "Microsoft Visual Studio 8\VC\PlatformSDK\include")
        End Function

        Public Function GetSdkIncludePath() As String
            Return Path.Combine( _
                Environment.GetEnvironmentVariable("ProgramFiles"), _
                "Microsoft Visual Studio 8\VC\include")
        End Function
    End Module
#End Region

End Namespace
