' Copyright (c) Microsoft Corporation.  All rights reserved.

Imports System.CodeDom
Imports System.IO
Imports Pinvoke
Imports PInvoke.Parser
Imports PInvoke.Transform

Module Module1

    Sub Main(BYVal args() As String) 

        Dim ns As NativeStorage = NativeStorage.LoadFromAssemblyPath

        Dim bag As New NativeSymbolBag(ns)

        Dim ntProc As NativeProcedure = Nothing
        bag.TryFindOrLoadProcedure("FindFirstFileW", ntProc)
        bag.TryFindOrLoadProcedure("FindNextFileW", ntProc)
        bag.TryFindOrLoadProcedure("FindClose", ntProc)
        bag.TryFindOrLoadProcedure("GetSystemDirectoryW", ntProc)
        bag.TryFindOrLoadProcedure("GetWindowTextW", ntProc)
        bag.TryFindOrLoadProcedure("EnumWindows", ntProc)
        bag.TryFindOrLoadProcedure("GetComputerNameW", ntProc)
        bag.TryFindOrLoadProcedure("CreateWellKnownSid", ntProc)
        bag.TryFindOrLoadProcedure("CopySid", ntProc)
        bag.TryFindOrLoadProcedure("IsEqualSid", ntProc)
        bag.TryFindOrLoadProcedure("SHGetFileInfoW", ntProc)
        bag.TryFindOrLoadProcedure("GetEnvironmentVariableW", ntProc)
        bag.TryFindOrLoadProcedure("atoi", ntProc)

        Dim ntDefined As NativeDefinedType = Nothing
        Dim ntTypedef As NativeTypeDef = Nothing
        bag.TryFindOrLoadDefinedType("WNDPROC", ntDefined)
        bag.TryFindOrLoadDefinedType("WNDENUMPROC", ntDefined)
        bag.TryFindOrLoadDefinedType("COMSTAT", ntDefined)
        bag.TryFindOrLoadDefinedType("_DCB", ntDefined)
        bag.TryFindOrLoadDefinedType("_IMAGE_LINENUMBER", ntDefined)


        Dim convert As New BasicConverter(LanguageType.VisualBasic, ns)
        Dim code As String = convert.ConvertToPInvokeCode(bag)
        code = _
            "' Generated File ... Re-Run PInvokeTestGen to regenerate this file" & vbCrLf & _
            "Namespace Generated" & vbCrLf & _
            code & vbCrLf & _
            "End Namespace"
        IO.File.WriteAllText(args(0), code)
    End Sub
End Module
