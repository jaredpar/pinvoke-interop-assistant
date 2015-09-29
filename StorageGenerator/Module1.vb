' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.IO
Imports Pinvoke
Imports PInvoke.Parser
Imports PInvoke.Transform

Module Module1

    ''' <summary>
    ''' Dll's which are somewhat more troublesome and should not be loaded by default
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property FullDllList() As IEnumerable(Of String)
        Get
            Dim list As New List(Of String)(ProcedureFinder.DefaultDllList)
            list.Add("oleaut32.dll")
            list.Add("ole32.dll")
            list.Add("ole2.dll")
            list.Add("ole2disp.dll")
            list.Add("ole2nls.dll")
            list.Add("msvcr80.dll")
            list.Add("nt64.dll")
            list.Add("msimg32.dll")
            list.Add("winscard.dll")
            list.Add("winspool.dll")
            list.Add("comctl32.dll")
            Return list
        End Get
    End Property

    Private Function CreateInitialNativeStorage() As NativeStorage
        Dim ns As New NativeStorage()

        ' Add in the basic type defs
        ns.AddTypedef(New NativeTypeDef("SIZE_T", New NativeBuiltinType(BuiltinType.NativeInt32, True)))
        ns.AddTypedef(New NativeTypeDef("DWORD64", New NativeBuiltinType(BuiltinType.NativeInt64, True)))
        ns.AddTypedef(New NativeTypeDef("HWND", New NativePointer(BuiltinType.NativeVoid)))
        ns.AddTypedef(New NativeTypeDef("HMENU", New NativePointer(BuiltinType.NativeVoid)))
        ns.AddTypedef(New NativeTypeDef("HACCEL", New NativePointer(BuiltinType.NativeVoid)))
        ns.AddTypedef(New NativeTypeDef("HBRUSH", New NativePointer(BuiltinType.NativeVoid)))
        ns.AddTypedef(New NativeTypeDef("HFONT", New NativePointer(BuiltinType.NativeVoid)))
        ns.AddTypedef(New NativeTypeDef("HDC", New NativePointer(BuiltinType.NativeVoid)))
        ns.AddTypedef(New NativeTypeDef("HICON", New NativePointer(BuiltinType.NativeVoid)))

        Return ns
    End Function

    ''' <summary>
    ''' Verification of the generated code
    ''' </summary>
    ''' <param name="ns"></param>
    ''' <remarks></remarks>
    Private Sub VerifyGeneratedStorage(ByVal ns As NativeStorage)

        Dim proc As NativeProcedure = Nothing
        VerifyTrue(ns.TryLoadProcedure("SendMessageA", proc))
        VerifyTrue(ns.TryLoadProcedure("SendMessageW", proc))
        VerifyTrue(ns.TryLoadProcedure("GetForegroundWindow", proc))
        VerifyTrue(ns.TryLoadProcedure("CreateWellKnownSid", proc))

        Dim typedef As NativeTypeDef = Nothing
        VerifyTrue(ns.TryLoadTypedef("LPCSTR", typedef))
        VerifyTrue(ns.TryLoadTypedef("LPWSTR", typedef))

        Dim defined As NativeType = Nothing
        VerifyTrue(ns.TryLoadByName("WNDPROC", defined))
        VerifyTrue(ns.TryLoadByName("HOOKPROC", defined))
        VerifyTrue(ns.TryLoadByName("tagPOINT", defined))
        VerifyTrue(ns.TryLoadByName("_SYSTEM_INFO", defined))

        Dim c As NativeConstant = Nothing
        VerifyTrue(ns.TryLoadConstant("WM_PAINT", c))
        VerifyTrue(ns.TryLoadConstant("WM_LBUTTONDOWN", c))

    End Sub

    Private Sub VerifyTrue(ByVal value As Boolean)
        If Not value Then
            Throw New Exception()
        End If
    End Sub

    Private Function Generate(ByVal writer As TextWriter) As NativeStorage
        Dim analyzer As NativeCodeAnalyzer = NativeCodeAnalyzerFactory.Create(OsVersion.WindowsVista)
        analyzer.IncludePathList.AddRange(NativeCodeAnalyzerFactory.GetCommonSdkPaths())

        ' Run the preprocessor
        analyzer.Trace = True
        Dim winPath As String = Path.Combine(GetPlatformSdkIncludePath(), "windows.h")
        Dim tr As TextReaderBag = analyzer.RunPreProcessor(winPath)
        File.WriteAllText("d:\temp\windows.out.h", tr.TextReader.ReadToEnd())
        analyzer.Trace = False

        Dim result As NativeCodeAnalyzerResult = analyzer.Analyze(winPath)
        Dim ep As ErrorProvider = result.ErrorProvider
        If ep.Errors.Count > 0 Then
            Debug.Fail("Encountered an error during the parse")
        End If
        Dim bag As NativeSymbolBag = NativeSymbolBag.CreateFrom(result, CreateInitialNativeStorage(), ep)

        ' Resolve with the full dll list
        Using finder As New ProcedureFinder(FullDllList)
            bag.TryResolveSymbolsAndValues(finder, ep)
        End Using

        For Each msg As String In ep.AllMessages
            writer.WriteLine("' " & msg)
        Next

        ' GenerateCode(writer, bag)

        ' Now write out the file
        Dim ns As NativeStorage = bag.SaveToNativeStorage()
        VerifyGeneratedStorage(ns)
        ns.WriteXml("windows.xml")

        ' Copy the file to the various applications
        File.Copy("windows.xml", "..\..\..\ConsoleTool\bin\Debug\windows.xml", True)
        File.Copy("windows.xml", "..\..\Data\windows.xml", True)

        Dim fullInstallTarget As String = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), _
                                        Path.Combine(PInvoke.Constants.ProductName, _
                                        "Data\windows.xml"))
        If File.Exists(fullInstallTarget) Then
            File.Copy("windows.xml", fullInstallTarget, True)
        End If

        Return ns
    End Function

    Sub Main()

        Using sw As New StreamWriter("d:\temp\windows.vb")
            Generate(sw)
        End Using

    End Sub

End Module
