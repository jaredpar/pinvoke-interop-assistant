' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Runtime.InteropServices

Friend Module NativeMethods

    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode)> _
    Public Function LoadLibraryEx( _
        <MarshalAs(UnmanagedType.LPTStr), [In]()> ByVal fileName As String, _
        ByVal intPtr As IntPtr, _
        ByVal flags As UInt32) As IntPtr

    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode)> _
    Public Function FreeLibrary(ByVal handle As IntPtr) As Int32

    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Ansi)> _
    Public Function GetProcAddress( _
        <[In]()> ByVal dllPtr As IntPtr, _
        <MarshalAs(UnmanagedType.LPStr), [In]()> ByVal procName As String) As IntPtr
    End Function

End Module
