' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System
Imports System.IO
Imports System.Text
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports PInvokeTest.Generated

''' <summary>
''' This class is a self host unit test of the native code that I generate.  If you need more API's
''' to test then modify the PInvokeTestGen project to emit them
''' </summary>
''' <remarks></remarks>
<TestClass()> Public Class SelfHost

#Region "Additional test attributes"
    '
    ' You can use the following additional attributes as you write your tests:
    '
    ' Use ClassInitialize to run code before running the first test in the class
    ' <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    ' End Sub
    '
    ' Use ClassCleanup to run code after all tests in a class have run
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub
    '
    ' Use TestInitialize to run code before running each test
    ' <TestInitialize()> Public Sub MyTestInitialize()
    ' End Sub
    '
    ' Use TestCleanup to run code after each test has run
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region

    <TestInitialize()> _
    Public Sub Init()
        m_nameList.Clear()
    End Sub

    Private m_nameList As New List(Of String)

    Private Function CaptureWindowNameCb(ByVal intPtr As IntPtr, ByVal param2 As IntPtr) As Integer
        Dim builder As New StringBuilder(256)
        If 0 <> NativeMethods.GetWindowTextW(intPtr, builder, builder.Capacity) Then
            m_nameList.Add(builder.ToString())
        End If

        Return 1
    End Function

    Private Function CaptureWindowNameCb2(ByVal intPtr As IntPtr, ByVal param2 As IntPtr) As Integer
        Dim name As String = String.Empty
        If 0 <> NativeMethods.GetWindowTextW(intPtr, name) Then
            m_nameList.Add(name)
        End If

        Return 1
    End Function

    ''' <summary>
    ''' Test the FindFirstFile API
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub FindFirstFile()
        Dim sys32Path As String = Environment.GetFolderPath(Environment.SpecialFolder.System)
        Dim data As New WIN32_FIND_DATAW
        Dim handle As IntPtr = NativeMethods.FindFirstFileW( _
            Path.Combine(sys32Path, "n") & "*", _
            data)

        Assert.AreNotEqual(handle, intPtr.Zero)
        Dim list As New List(Of String)
        list.Add(data.cFileName)
        While NativeMethods.FindNextFileW(handle, data)
            list.Add(data.cFileName)
        End While

        Assert.IsTrue(list.Count > 3)
        NativeMethods.FindClose(handle)
    End Sub

    ''' <summary>
    ''' Enumerate the top level windows and collect the names
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub EnumWindows1()
        NativeMethods.EnumWindows(AddressOf Me.CaptureWindowNameCb, IntPtr.Zero)
        Assert.IsTrue(m_nameList.Count > 0)
    End Sub

    ''' <summary>
    ''' Same as the other but using the cleaned up method 
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub EnumWindows2()
        NativeMethods.EnumWindows(AddressOf Me.CaptureWindowNameCb2, IntPtr.Zero)
        Assert.IsTrue(m_nameList.Count > 0)
    End Sub

    '<TestMethod()> _
    Public Sub GetComptureName1()
        Dim builder As New StringBuilder(256)
        Dim count As UInteger = CUInt(builder.Capacity)
        Assert.IsTrue(NativeMethods.GetComputerNameW(builder, count))
        Assert.AreEqual(Environment.MachineName, builder.ToString(), True)
    End Sub

    '<TestMethod()> _
    Public Sub GetComputerName2()
        Dim name As String = Nothing
        Assert.IsTrue(NativeMethods.GetComputerNameW(name))
        Assert.AreEqual(Environment.MachineName, name, True)
    End Sub

    '<TestMethod()> _
    Public Sub CreateWellKnownSid2()
        Dim ptr As PInvokePointer = Nothing
        Assert.IsTrue(NativeMethods.CreateWellKnownSid(WELL_KNOWN_SID_TYPE.WinBuiltinAdministratorsSid, IntPtr.Zero, ptr))
        ptr.Free()
    End Sub

    ''' <summary>
    ''' Test a couple of the properties in a bitvector type.  This is a simple type with single bit values
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub BitVector1()
        Dim d As New DCB()

        d.fBinary = 1
        Assert.AreEqual(1UI, d.fBinary)

        d.fParity = 1
        Assert.AreEqual(1UI, d.fParity)

        d.fOutxCtsFlow = 1
        Assert.AreEqual(1UI, d.fOutxCtsFlow)

        d.fOutxDsrFlow = 1
        Assert.AreEqual(1UI, d.fOutxDsrFlow)

        d.fDtrControl = 1
        Assert.AreEqual(1UI, d.fDtrControl)

        d.fDsrSensitivity = 1
        Assert.AreEqual(1UI, d.fDsrSensitivity)

        d.fOutX = 1
        Assert.AreEqual(1UI, d.fOutX)
    End Sub

    ''' <summary>
    ''' Use a multy bit bitvector
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub BitVector2()
        Dim d As New DCB()

        d.fDtrControl = 2
        Assert.AreEqual(2UI, d.fDtrControl)
    End Sub

    ''' <summary>
    ''' Test a basic union structure
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub Union1()
        Dim l As New IMAGE_LINENUMBER()
        l.Type.VirtualAddress = 5
        Assert.AreEqual(5UI, l.Type.VirtualAddress)
        Assert.AreEqual(5UI, l.Type.SymbolTableIndex)
    End Sub

    <TestMethod()> _
    Public Sub GetEnvironmentVariable1()
        Dim value As String = Nothing
        NativeMethods.GetEnvironmentVariableW("USERPROFILE", value)
        Assert.IsFalse(String.IsNullOrEmpty(value))
    End Sub

    ''' <summary>
    ''' This is a __cdecl method
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub Atoi()
        Assert.AreEqual(5, NativeMethods.atoi("5"))
    End Sub

End Class

