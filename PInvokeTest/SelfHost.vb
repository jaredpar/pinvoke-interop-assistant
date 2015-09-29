' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System
Imports System.IO
Imports System.Text
Imports System.Collections.Generic
Imports PInvokeTest.Generated
Imports Xunit

''' <summary>
''' This class is a self host unit test of the native code that I generate.  If you need more API's
''' to test then modify the PInvokeTestGen project to emit them
''' </summary>
''' <remarks></remarks>
Public Class SelfHost

    Public Sub New()
        _nameList.Clear()
    End Sub

    Private _nameList As New List(Of String)

    Private Function CaptureWindowNameCb(ByVal intPtr As IntPtr, ByVal param2 As IntPtr) As Integer
        Dim builder As New StringBuilder(256)
        If 0 <> NativeMethods.GetWindowTextW(intPtr, builder, builder.Capacity) Then
            _nameList.Add(builder.ToString())
        End If

        Return 1
    End Function

    Private Function CaptureWindowNameCb2(ByVal intPtr As IntPtr, ByVal param2 As IntPtr) As Integer
        Dim name As String = String.Empty
        If 0 <> NativeMethods.GetWindowTextW(intPtr, name) Then
            _nameList.Add(name)
        End If

        Return 1
    End Function

    ''' <summary>
    ''' Test the FindFirstFile API
    ''' </summary>
    ''' <remarks></remarks>
    <Fact>
    Public Sub FindFirstFile()
        Dim sys32Path As String = Environment.GetFolderPath(Environment.SpecialFolder.System)
        Dim data As New WIN32_FIND_DATAW
        Dim handle As IntPtr = NativeMethods.FindFirstFileW(
            Path.Combine(sys32Path, "n") & "*",
            data)

        Assert.NotEqual(handle, IntPtr.Zero)
        Dim list As New List(Of String)
        list.Add(data.cFileName)
        While NativeMethods.FindNextFileW(handle, data)
            list.Add(data.cFileName)
        End While

        Assert.True(list.Count > 3)
        NativeMethods.FindClose(handle)
    End Sub

    ''' <summary>
    ''' Enumerate the top level windows and collect the names
    ''' </summary>
    ''' <remarks></remarks>
    <Fact>
    Public Sub EnumWindows1()
        NativeMethods.EnumWindows(AddressOf Me.CaptureWindowNameCb, IntPtr.Zero)
        Assert.True(_nameList.Count > 0)
    End Sub

    ''' <summary>
    ''' Same as the other but using the cleaned up method 
    ''' </summary>
    ''' <remarks></remarks>
    <Fact>
    Public Sub EnumWindows2()
        NativeMethods.EnumWindows(AddressOf Me.CaptureWindowNameCb2, IntPtr.Zero)
        Assert.True(_nameList.Count > 0)
    End Sub

    '<Fact>
    Public Sub GetComptureName1()
        Dim builder As New StringBuilder(256)
        Dim count As UInteger = CUInt(builder.Capacity)
        Assert.True(NativeMethods.GetComputerNameW(builder, count))
        Assert.Equal(Environment.MachineName, builder.ToString(), True)
    End Sub

    '<Fact>
    Public Sub GetComputerName2()
        Dim name As String = Nothing
        Assert.True(NativeMethods.GetComputerNameW(name))
        Assert.Equal(Environment.MachineName, name, True)
    End Sub

    '<Fact>
    Public Sub CreateWellKnownSid2()
        Dim ptr As PInvokePointer = Nothing
        Assert.True(NativeMethods.CreateWellKnownSid(WELL_KNOWN_SID_TYPE.WinBuiltinAdministratorsSid, IntPtr.Zero, ptr))
        ptr.Free()
    End Sub

    ''' <summary>
    ''' Test a couple of the properties in a bitvector type.  This is a simple type with single bit values
    ''' </summary>
    ''' <remarks></remarks>
    <Fact>
    Public Sub BitVector1()
        Dim d As New DCB()

        d.fBinary = 1
        Assert.Equal(1UI, d.fBinary)

        d.fParity = 1
        Assert.Equal(1UI, d.fParity)

        d.fOutxCtsFlow = 1
        Assert.Equal(1UI, d.fOutxCtsFlow)

        d.fOutxDsrFlow = 1
        Assert.Equal(1UI, d.fOutxDsrFlow)

        d.fDtrControl = 1
        Assert.Equal(1UI, d.fDtrControl)

        d.fDsrSensitivity = 1
        Assert.Equal(1UI, d.fDsrSensitivity)

        d.fOutX = 1
        Assert.Equal(1UI, d.fOutX)
    End Sub

    ''' <summary>
    ''' Use a multy bit bitvector
    ''' </summary>
    ''' <remarks></remarks>
    <Fact>
    Public Sub BitVector2()
        Dim d As New DCB()

        d.fDtrControl = 2
        Assert.Equal(2UI, d.fDtrControl)
    End Sub

    ''' <summary>
    ''' Test a basic union structure
    ''' </summary>
    ''' <remarks></remarks>
    <Fact>
    Public Sub Union1()
        Dim l As New IMAGE_LINENUMBER()
        l.Type.VirtualAddress = 5
        Assert.Equal(5UI, l.Type.VirtualAddress)
        Assert.Equal(5UI, l.Type.SymbolTableIndex)
    End Sub

    <Fact>
    Public Sub GetEnvironmentVariable1()
        Dim value As String = Nothing
        NativeMethods.GetEnvironmentVariableW("USERPROFILE", value)
        Assert.False(String.IsNullOrEmpty(value))
    End Sub

    ''' <summary>
    ''' This is a __cdecl method
    ''' </summary>
    ''' <remarks></remarks>
    <Fact>
    Public Sub Atoi()
        Assert.Equal(5, NativeMethods.atoi("5"))
    End Sub

End Class

