' Copyright (c) Microsoft Corporation.  All rights reserved.

''' <summary>
''' Used to find procedures in a list of DLL's
''' </summary>
''' <remarks></remarks>
Public Class ProcedureFinder
    Implements IDisposable

    Public Shared ReadOnly Property DefaultDllList() As IEnumerable(Of String)
        Get
            Dim list As New List(Of String)
            list.Add("kernel32.dll")
            list.Add("ntdll.dll")
            list.Add("user32.dll")
            list.Add("advapi32.dll")
            list.Add("gdi32.dll")
            list.Add("crypt32.dll")
            list.Add("cryptnet.dll")
            list.Add("opengl32.dll")
            list.Add("ws2_32.dll")
            list.Add("shell32.dll")
            list.Add("mpr.dll")
            list.Add("mswsock.dll")
            list.Add("winmm.dll")
            list.Add("imm32.dll")
            list.Add("comdlg32.dll")
            list.Add("rpcns4.dll")
            list.Add("rpcrt4.dll")
            list.Add("urlmon.dll")
            Return list
        End Get
    End Property

    Private _dllMap As New Dictionary(Of String, IntPtr)
    Private _loaded As Boolean = False

    ''' <summary>
    ''' List of dll's to look for
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property DllNames() As IEnumerable(Of String)
        Get
            Return _dllMap.Keys
        End Get
    End Property

    Public Sub New()
        MyClass.New(DefaultDllList)
    End Sub

    Public Sub New(ByVal list As IEnumerable(Of String))
        For Each name As String In list
            AddDll(name)
        Next
    End Sub

    Public Sub Dispose() Implements System.IDisposable.Dispose
        For Each ptr As IntPtr In _dllMap.Values
            NativeMethods.FreeLibrary(ptr)
        Next

    End Sub

    Public Sub AddDll(ByVal dllName As String)
        If dllName Is Nothing Then : Throw New ArgumentNullException("dllName") : End If

        _dllMap.Add(dllName, IntPtr.Zero)
        _loaded = False
    End Sub

    Public Function TryFindDllNameExact(ByVal procName As String, ByRef dllName As String) As Boolean
        If procName Is Nothing Then : Throw New ArgumentNullException("procName") : End If

        Return TryFindDllNameImpl(procName, dllName)
    End Function

    Public Function TryFindDllName(ByVal procName As String, ByRef dllName As String) As Boolean
        If procName Is Nothing Then : Throw New ArgumentNullException("procName") : End If

        If Not TryFindDllNameImpl(procName, dllName) _
            AndAlso Not TryFindDllNameImpl(procName & "W", dllName) Then
            Return False
        End If

        Return True
    End Function

    Private Function TryFindDllNameImpl(ByVal procName As String, ByRef dllName As String) As Boolean
        ThrowIfNull(procName)

        If Not _loaded Then
            LoadLibraryList()
        End If

        For Each pair As KeyValuePair(Of String, IntPtr) In _dllMap
            If pair.Value = IntPtr.Zero Then
                Continue For
            End If

            Dim procPtr As IntPtr = NativeMethods.GetProcAddress(pair.Value, procName)
            If procPtr <> IntPtr.Zero Then
                dllName = IO.Path.GetFileName(pair.Key)
                Return True
            End If
        Next

        Return False
    End Function

    Private Sub LoadLibraryList()

        Dim list As New List(Of String)(_dllMap.Keys)
        For Each name As String In list
            Dim ptr As IntPtr = _dllMap(name)
            If ptr = IntPtr.Zero Then
                ptr = NativeMethods.LoadLibraryEx(name, IntPtr.Zero, 0UL)
                _dllMap(name) = ptr
            End If
        Next

        _loaded = True
    End Sub

End Class
