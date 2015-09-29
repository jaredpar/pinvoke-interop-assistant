' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.IO
Imports PInvoke
Imports PInvoke.Transform
Imports PInvoke.Parser


Module Module1

    Public Class Options
        Public Language As LanguageType = LanguageType.VisualBasic
        Public OutputFilePath As String = "NativeMethods.vb"
        Public IncludeSdkFiles As Boolean = True
        Public HeaderFiles As New List(Of String)
        Public LibraryFiles As New List(Of String)
        Public IncludePaths As New List(Of String)
        Public GenerateCode As Boolean = True
        Public GeneratePreprocessor As Boolean = False
        Public Logo As Boolean = True
    End Class

    Public Class StorageDiff
        Public Procedures As New List(Of String)
        Public DefinedTypes As New List(Of String)
        Public Constants As New List(Of String)
    End Class

    Private Sub Logo()
        Console.WriteLine("Microsoft (R) Unmanaged to Managed Signature Converter version " & PInvoke.Constants.FriendlyVersion)
        Console.WriteLine("Copyright (C) Microsoft Corporation.  All rights reserved.")
        Console.WriteLine()
    End Sub

    Private Sub Usage()
        Logo()
        Console.WriteLine("sigimp [options] [Header File Names]")
        Console.WriteLine(vbTab & "/genCode:[yes/no]" & vbTab & "Whether or not to generate a code file (default yes)")
        Console.WriteLine(vbTab & "/genPreProc:[yes/no]" & vbTab & "Generate the preprocessor code")
        Console.WriteLine(vbTab & "/lang[uage]:lang" & vbTab & "Language to generate into; vb (default) or cs")
        Console.WriteLine(vbTab & "/out:filename" & vbTab & vbTab & "Output file name (default NativeMethods.vb)")
        Console.WriteLine(vbTab & "/useSdk:[yes/no]" & vbTab & "Whether or not to add common SDK include paths (default yes)")
        Console.WriteLine(vbTab & "/lib:name,name" & vbTab & vbTab & "List of libraries to resolve DLL's against")
        Console.WriteLine(vbTab & "/includePath:path,..." & vbTab & "Include File Path")
        Console.WriteLine(vbTab & "/nologo" & vbTab & vbTab & vbTab & "Prevent logo display")
    End Sub

    Private Function ProcessOptions(ByVal args() As String) As Options
        Dim opts As New Options()
        Dim hasSpecifiedOutFile As Boolean = False

        For i As Integer = 0 To args.Length - 1
            Dim cur As String = args(i)
            If cur.StartsWith("/"c) OrElse cur.StartsWith("-"c) Then
                cur = cur.Substring(1)
            Else
                opts.HeaderFiles.Add(cur)
                Continue For
            End If

            Dim argOption As String = Nothing
            If cur.Contains(":") Then
                Dim values() As String = cur.Split(New Char() {":"c}, 2)
                cur = values(0)
                argOption = values(1)
            End If

            Select Case cur.ToLower()
                Case "?", "help"
                    Usage()
                    Return Nothing
                Case "lang", "language"
                    If String.IsNullOrEmpty(argOption) Then
                        GoTo BadLang
                    End If

                    Select Case argOption.ToLower()
                        Case "vb"
                            opts.Language = LanguageType.VisualBasic
                        Case "cs"
                            opts.Language = LanguageType.CSharp
                            If Not hasSpecifiedOutFile Then
                                opts.OutputFilePath = "NativeMethods.cs"
                            End If
                        Case Else
                            GoTo BadLang
                    End Select
                Case "usesdk"
                    If String.IsNullOrEmpty(argOption) Then
                        GoTo BadUseSdk
                    End If

                    Select Case argOption.ToLower()
                        Case "yes"
                            opts.IncludeSdkFiles = True
                        Case "no"
                            opts.IncludeSdkFiles = False
                        Case Else
                            GoTo BadUseSdk
                    End Select
                Case "out"
                    If String.IsNullOrEmpty(argOption) Then
                        GoTo BadOut
                    End If

                    opts.OutputFilePath = argOption
                Case "lib"
                    If String.IsNullOrEmpty(argOption) Then
                        GoTo BadLib
                    End If
                    opts.LibraryFiles.AddRange(argOption.Split(","c))
                Case "gencode"
                    If String.IsNullOrEmpty(argOption) Then
                        GoTo BadGenCode
                    End If

                    Select Case argOption.ToLower()
                        Case "yes"
                            opts.GenerateCode = True
                        Case "no"
                            opts.GenerateCode = False
                        Case Else
                            GoTo BadGenCode
                    End Select
                Case "genpreproc"
                    If String.IsNullOrEmpty(argOption) Then
                        GoTo BadGenPreProc
                    End If

                    Select Case argOption.ToLower()
                        Case "yes"
                            opts.GeneratePreprocessor = True
                        Case "no"
                            opts.GeneratePreprocessor = True
                        Case Else
                            GoTo BadGenPreProc
                    End Select
                Case "includepath"
                    If String.IsNullOrEmpty(argOption) Then
                        GoTo BadIncludePath
                    End If

                    opts.IncludePaths.AddRange(argOption.Split(","c))
                Case "nologo"
                    opts.Logo = False
                Case Else
                    Console.WriteLine("Unrecognized option {0}", cur)
                    Usage()
                    Return Nothing
            End Select
        Next

        If opts.HeaderFiles.Count = 0 Then
            Logo()
            Console.WriteLine("Error: Please specify one or more header files")
            Return Nothing
        End If

        ' If the user specified header files that are not in the current directory then we
        ' should add the path of the file into the include path list
        For Each cur As String In opts.HeaderFiles
            Dim parentPath As String = Path.GetDirectoryName(cur)
            If Not String.IsNullOrEmpty(parentPath) Then
                opts.IncludePaths.Add(parentPath)
            End If
        Next

        Return opts

BadIncludePath:
        Logo()
        Console.WriteLine("Error: includePath option requires an argument")
        Return Nothing

BadLang:
        Logo()
        Console.WriteLine("Error: lang option requires an argument (vb/cs)")
        Return Nothing

BadUseSdk:
        Logo()
        Console.WriteLine("Error: useSdk requires an argument (yes/no)")
        Return Nothing

BadOut:
        Logo()
        Console.WriteLine("Error: out requires an argument")
        Return Nothing

BadLib:
        Logo()
        Console.WriteLine("Error: lib requires one or more comma separated arguments")
        Return Nothing

BadGenCode:
        Logo()
        Console.WriteLine("Error: genCode requires an argument (yes/no)")
        Return Nothing

BadGenPreProc:
        Logo()
        Console.WriteLine("Error: genPreProc requires an argument (yes/no)")
        Return Nothing

    End Function

    Private Function ProcessHeaderFiles(ByVal opts As Options) As NativeCodeAnalyzerResult
        ' Create a combined header file
        Dim tempHeaderPath As String = Path.GetTempFileName()
        Using writer As New StreamWriter(tempHeaderPath, False)
            For Each p As String In opts.HeaderFiles
                writer.Write("#include ""{0}""", p)
            Next
        End Using

        ' Parse out the file
        Dim analyzer As NativeCodeAnalyzer
        If opts.IncludeSdkFiles Then
            analyzer = NativeCodeAnalyzerFactory.Create(OsVersion.WindowsVista)
            analyzer.IncludePathList.AddRange(NativeCodeAnalyzerFactory.GetCommonSdkPaths())
        Else
            Dim ns As NativeStorage = NativeStorage.LoadFromAssemblyPath()
            analyzer = NativeCodeAnalyzerFactory.CreateForMiniParse(OsVersion.WindowsVista, ns.LoadAllMacros())
        End If
        analyzer.IncludePathList.InsertRange(0, opts.IncludePaths)

        If opts.GeneratePreprocessor Then
            Dim outFile As String = Path.ChangeExtension(opts.OutputFilePath, ".PreProcessor.h")
            Console.Write("Generating PreProcessor Data -> {0} ... ", outFile)
            analyzer.Trace = True
            Dim tr As TextReaderBag = analyzer.RunPreProcessor(tempHeaderPath)
            File.WriteAllText(outFile, tr.TextReader.ReadToEnd())
            analyzer.Trace = False
            Console.WriteLine("Done")
        End If

        ' Analyze the output file
        Console.Write("Processing header files ... ")
        Dim result As NativeCodeAnalyzerResult = analyzer.Analyze(tempHeaderPath)
        File.Delete(tempHeaderPath)
        Console.WriteLine("Done")

        If result.ErrorProvider.Errors.Count > 0 OrElse result.ErrorProvider.Warnings.Count > 0 Then
            Console.WriteLine(result.ErrorProvider.CreateDisplayString)
        End If
        Return result
    End Function

    Private Function CreateSymbolBag(ByVal opts As Options, ByVal result As NativeCodeAnalyzerResult, ByVal ep As ErrorProvider) As NativeSymbolBag
        Dim storage As NativeStorage
        storage = NativeStorage.LoadFromAssemblyPath()

        Dim bag As NativeSymbolBag = NativeSymbolBag.CreateFrom(result, storage)

        ' Try and resolve the symbol
        Using finder As New ProcedureFinder(ProcedureFinder.DefaultDllList)
            For Each libName As String In opts.LibraryFiles
                finder.AddDll(libName)
            Next

            bag.TryResolveSymbolsAndValues(finder, ep)
        End Using

        Return bag
    End Function

    Private Function DiffStorage(ByVal oldStore As NativeStorage, ByVal newStore As NativeStorage) As StorageDiff
        oldStore.CacheLookup = True
        Dim d As New StorageDiff

        ' Procedures
        For Each row As NativeStorage.ProcedureRow In newStore.Procedure.Rows
            Dim oldProc As NativeStorage.ProcedureRow = Nothing
            If Not oldStore.Procedure.TryLoadByName(row.Name, oldProc) Then
                d.Procedures.Add(row.Name)
            End If
        Next

        ' Defined Types
        For Each row As NativeStorage.DefinedTypeRow In newStore.DefinedType.Rows
            Dim oldRow As NativeStorage.DefinedTypeRow = Nothing
            If Not oldStore.DefinedType.TryFindByName(row.Name, oldRow) AndAlso Not NativeSymbolBag.IsAnonymousName(row.Name) Then
                d.DefinedTypes.Add(row.Name)
            End If
        Next

        ' Constants
        For Each row As NativeStorage.ConstantRow In newStore.Constant.Rows
            Dim oldRow As NativeStorage.ConstantRow = Nothing
            If Not oldStore.Constant.TryFindByName(row.Name, oldRow) Then
                d.Constants.Add(row.Name)
            End If
        Next

        oldStore.CacheLookup = False
        Return d
    End Function

    ''' <summary>
    ''' Generate the new functions
    ''' </summary>
    ''' <param name="opts"></param>
    ''' <param name="storage"></param>
    ''' <remarks></remarks>
    Private Sub GenerateCode(ByVal opts As Options, ByVal storage As NativeStorage)
        Dim defaultStorage As NativeStorage = NativeStorage.LoadFromAssemblyPath()
        Dim diff As StorageDiff = DiffStorage(defaultStorage, storage)

        Dim bag As New NativeSymbolBag(storage)
        For Each name As String In diff.Procedures
            Dim proc As NativeProcedure = Nothing
            bag.TryFindOrLoadProcedure(name, proc)
        Next

        For Each name As String In diff.DefinedTypes
            Dim dt As NativeDefinedType = Nothing
            bag.TryFindOrLoadDefinedType(name, dt)
        Next

        For Each name As String In diff.Constants
            Dim c As NativeConstant = Nothing
            bag.TryFindOrLoadConstant(name, c)
        Next

        ' Create the code
        Dim transform As New BasicConverter(opts.Language, storage)
        Dim code As String = transform.ConvertToPInvokeCode(bag)
        File.WriteAllText(opts.OutputFilePath, code)
    End Sub

    Sub Main(ByVal args() As String)
        Dim opts As Options = ProcessOptions(args)
        If opts Is Nothing Then
            Return
        End If

        If opts.Logo Then
            Logo()
        End If

        Dim ep As New ErrorProvider
        Dim result As NativeCodeAnalyzerResult = ProcessHeaderFiles(opts)
        Dim bag As NativeSymbolBag = CreateSymbolBag(opts, result, ep)
        Dim storage As NativeStorage = bag.SaveToNativeStorage()

        If opts.GenerateCode Then
            Console.Write("Generating Interop Code -> {0} ... ", opts.OutputFilePath)
            GenerateCode(opts, storage)
            Console.WriteLine("Done")
        Else
            Console.Write("Generating DataBase -> Database.xml ... ")
            storage.WriteXml("Database.xml")
            Console.WriteLine("Done")
        End If

    End Sub

End Module
