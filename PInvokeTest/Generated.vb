' Copyright (c) Microsoft Corporation.  All rights reserved.
' Generated File ... Re-Run PInvokeTestGen to regenerate this file
Namespace Generated

    '''Return Type: LRESULT->LONG_PTR->int
    '''param0: HWND->HWND__*
    '''param1: UINT->unsigned int
    '''param2: WPARAM->UINT_PTR->unsigned int
    '''param3: LPARAM->LONG_PTR->int
    <System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)> _
    Public Delegate Function WNDPROC(ByVal param0 As System.IntPtr, ByVal param1 As UInteger, ByVal param2 As System.IntPtr, ByVal param3 As System.IntPtr) As Integer

    '''Return Type: BOOL->int
    '''param0: HWND->HWND__*
    '''param1: LPARAM->LONG_PTR->int
    <System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)> _
    Public Delegate Function WNDENUMPROC(ByVal param0 As System.IntPtr, ByVal param1 As System.IntPtr) As Integer

    <System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)> _
    Public Structure DCB

        '''DWORD->unsigned int
        Public DCBlength As UInteger

        '''DWORD->unsigned int
        Public BaudRate As UInteger

        '''fBinary : 1
        '''fParity : 1
        '''fOutxCtsFlow : 1
        '''fOutxDsrFlow : 1
        '''fDtrControl : 2
        '''fDsrSensitivity : 1
        '''fTXContinueOnXoff : 1
        '''fOutX : 1
        '''fInX : 1
        '''fErrorChar : 1
        '''fNull : 1
        '''fRtsControl : 2
        '''fAbortOnError : 1
        '''fDummy2 : 17
        Public bitvector1 As UInteger

        '''WORD->unsigned short
        Public wReserved As UShort

        '''WORD->unsigned short
        Public XonLim As UShort

        '''WORD->unsigned short
        Public XoffLim As UShort

        '''BYTE->unsigned char
        Public ByteSize As Byte

        '''BYTE->unsigned char
        Public Parity As Byte

        '''BYTE->unsigned char
        Public StopBits As Byte

        '''char
        Public XonChar As Byte

        '''char
        Public XoffChar As Byte

        '''char
        Public ErrorChar As Byte

        '''char
        Public EofChar As Byte

        '''char
        Public EvtChar As Byte

        '''WORD->unsigned short
        Public wReserved1 As UShort

        Public Property fBinary() As UInteger
            Get
                Return CType((Me.bitvector1 And 1UI), UInteger)
            End Get
            Set
                Me.bitvector1 = CType((value Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fParity() As UInteger
            Get
                Return CType(((Me.bitvector1 And 2UI) _
                            / 2), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 2) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fOutxCtsFlow() As UInteger
            Get
                Return CType(((Me.bitvector1 And 4UI) _
                            / 4), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 4) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fOutxDsrFlow() As UInteger
            Get
                Return CType(((Me.bitvector1 And 8UI) _
                            / 8), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 8) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fDtrControl() As UInteger
            Get
                Return CType(((Me.bitvector1 And 48UI) _
                            / 16), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 16) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fDsrSensitivity() As UInteger
            Get
                Return CType(((Me.bitvector1 And 64UI) _
                            / 64), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 64) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fTXContinueOnXoff() As UInteger
            Get
                Return CType(((Me.bitvector1 And 128UI) _
                            / 128), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 128) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fOutX() As UInteger
            Get
                Return CType(((Me.bitvector1 And 256UI) _
                            / 256), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 256) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fInX() As UInteger
            Get
                Return CType(((Me.bitvector1 And 512UI) _
                            / 512), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 512) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fErrorChar() As UInteger
            Get
                Return CType(((Me.bitvector1 And 1024UI) _
                            / 1024), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 1024) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fNull() As UInteger
            Get
                Return CType(((Me.bitvector1 And 2048UI) _
                            / 2048), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 2048) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fRtsControl() As UInteger
            Get
                Return CType(((Me.bitvector1 And 12288UI) _
                            / 4096), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 4096) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fAbortOnError() As UInteger
            Get
                Return CType(((Me.bitvector1 And 16384UI) _
                            / 16384), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 16384) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property

        Public Property fDummy2() As UInteger
            Get
                Return CType(((Me.bitvector1 And 4294934528UI) _
                            / 32768), UInteger)
            End Get
            Set
                Me.bitvector1 = CType(((value * 32768) _
                            Or Me.bitvector1), UInteger)
            End Set
        End Property
    End Structure

    <System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)> _
    Public Structure IMAGE_LINENUMBER

        '''Anonymous_9458c9ac_2eca_481f_8912_f5ffcf5913b6
        Public Type As Anonymous_9458c9ac_2eca_481f_8912_f5ffcf5913b6

        '''WORD->unsigned short
        Public Linenumber As UShort
    End Structure

    <System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)> _
    Public Structure Anonymous_9458c9ac_2eca_481f_8912_f5ffcf5913b6

        '''DWORD->unsigned int
        <System.Runtime.InteropServices.FieldOffsetAttribute(0)> _
        Public SymbolTableIndex As UInteger

        '''DWORD->unsigned int
        <System.Runtime.InteropServices.FieldOffsetAttribute(0)> _
        Public VirtualAddress As UInteger
    End Structure

    Public Enum WELL_KNOWN_SID_TYPE

        '''WinNullSid -> 0
        WinNullSid = 0

        '''WinWorldSid -> 1
        WinWorldSid = 1

        '''WinLocalSid -> 2
        WinLocalSid = 2

        '''WinCreatorOwnerSid -> 3
        WinCreatorOwnerSid = 3

        '''WinCreatorGroupSid -> 4
        WinCreatorGroupSid = 4

        '''WinCreatorOwnerServerSid -> 5
        WinCreatorOwnerServerSid = 5

        '''WinCreatorGroupServerSid -> 6
        WinCreatorGroupServerSid = 6

        '''WinNtAuthoritySid -> 7
        WinNtAuthoritySid = 7

        '''WinDialupSid -> 8
        WinDialupSid = 8

        '''WinNetworkSid -> 9
        WinNetworkSid = 9

        '''WinBatchSid -> 10
        WinBatchSid = 10

        '''WinInteractiveSid -> 11
        WinInteractiveSid = 11

        '''WinServiceSid -> 12
        WinServiceSid = 12

        '''WinAnonymousSid -> 13
        WinAnonymousSid = 13

        '''WinProxySid -> 14
        WinProxySid = 14

        '''WinEnterpriseControllersSid -> 15
        WinEnterpriseControllersSid = 15

        '''WinSelfSid -> 16
        WinSelfSid = 16

        '''WinAuthenticatedUserSid -> 17
        WinAuthenticatedUserSid = 17

        '''WinRestrictedCodeSid -> 18
        WinRestrictedCodeSid = 18

        '''WinTerminalServerSid -> 19
        WinTerminalServerSid = 19

        '''WinRemoteLogonIdSid -> 20
        WinRemoteLogonIdSid = 20

        '''WinLogonIdsSid -> 21
        WinLogonIdsSid = 21

        '''WinLocalSystemSid -> 22
        WinLocalSystemSid = 22

        '''WinLocalServiceSid -> 23
        WinLocalServiceSid = 23

        '''WinNetworkServiceSid -> 24
        WinNetworkServiceSid = 24

        '''WinBuiltinDomainSid -> 25
        WinBuiltinDomainSid = 25

        '''WinBuiltinAdministratorsSid -> 26
        WinBuiltinAdministratorsSid = 26

        '''WinBuiltinUsersSid -> 27
        WinBuiltinUsersSid = 27

        '''WinBuiltinGuestsSid -> 28
        WinBuiltinGuestsSid = 28

        '''WinBuiltinPowerUsersSid -> 29
        WinBuiltinPowerUsersSid = 29

        '''WinBuiltinAccountOperatorsSid -> 30
        WinBuiltinAccountOperatorsSid = 30

        '''WinBuiltinSystemOperatorsSid -> 31
        WinBuiltinSystemOperatorsSid = 31

        '''WinBuiltinPrintOperatorsSid -> 32
        WinBuiltinPrintOperatorsSid = 32

        '''WinBuiltinBackupOperatorsSid -> 33
        WinBuiltinBackupOperatorsSid = 33

        '''WinBuiltinReplicatorSid -> 34
        WinBuiltinReplicatorSid = 34

        '''WinBuiltinPreWindows2000CompatibleAccessSid -> 35
        WinBuiltinPreWindows2000CompatibleAccessSid = 35

        '''WinBuiltinRemoteDesktopUsersSid -> 36
        WinBuiltinRemoteDesktopUsersSid = 36

        '''WinBuiltinNetworkConfigurationOperatorsSid -> 37
        WinBuiltinNetworkConfigurationOperatorsSid = 37

        '''WinAccountAdministratorSid -> 38
        WinAccountAdministratorSid = 38

        '''WinAccountGuestSid -> 39
        WinAccountGuestSid = 39

        '''WinAccountKrbtgtSid -> 40
        WinAccountKrbtgtSid = 40

        '''WinAccountDomainAdminsSid -> 41
        WinAccountDomainAdminsSid = 41

        '''WinAccountDomainUsersSid -> 42
        WinAccountDomainUsersSid = 42

        '''WinAccountDomainGuestsSid -> 43
        WinAccountDomainGuestsSid = 43

        '''WinAccountComputersSid -> 44
        WinAccountComputersSid = 44

        '''WinAccountControllersSid -> 45
        WinAccountControllersSid = 45

        '''WinAccountCertAdminsSid -> 46
        WinAccountCertAdminsSid = 46

        '''WinAccountSchemaAdminsSid -> 47
        WinAccountSchemaAdminsSid = 47

        '''WinAccountEnterpriseAdminsSid -> 48
        WinAccountEnterpriseAdminsSid = 48

        '''WinAccountPolicyAdminsSid -> 49
        WinAccountPolicyAdminsSid = 49

        '''WinAccountRasAndIasServersSid -> 50
        WinAccountRasAndIasServersSid = 50

        '''WinNTLMAuthenticationSid -> 51
        WinNTLMAuthenticationSid = 51

        '''WinDigestAuthenticationSid -> 52
        WinDigestAuthenticationSid = 52

        '''WinSChannelAuthenticationSid -> 53
        WinSChannelAuthenticationSid = 53

        '''WinThisOrganizationSid -> 54
        WinThisOrganizationSid = 54

        '''WinOtherOrganizationSid -> 55
        WinOtherOrganizationSid = 55

        '''WinBuiltinIncomingForestTrustBuildersSid -> 56
        WinBuiltinIncomingForestTrustBuildersSid = 56

        '''WinBuiltinPerfMonitoringUsersSid -> 57
        WinBuiltinPerfMonitoringUsersSid = 57

        '''WinBuiltinPerfLoggingUsersSid -> 58
        WinBuiltinPerfLoggingUsersSid = 58

        '''WinBuiltinAuthorizationAccessSid -> 59
        WinBuiltinAuthorizationAccessSid = 59

        '''WinBuiltinTerminalServerLicenseServersSid -> 60
        WinBuiltinTerminalServerLicenseServersSid = 60

        '''WinBuiltinDCOMUsersSid -> 61
        WinBuiltinDCOMUsersSid = 61
    End Enum

    <System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet:=System.Runtime.InteropServices.CharSet.[Unicode])> _
    Public Structure SHFILEINFOW

        '''HICON->HICON__*
        Public hIcon As System.IntPtr

        '''int
        Public iIcon As Integer

        '''DWORD->unsigned int
        Public dwAttributes As UInteger

        '''WCHAR[260]
        <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst:=260)> _
        Public szDisplayName As String

        '''WCHAR[80]
        <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst:=80)> _
        Public szTypeName As String
    End Structure

    <System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)> _
    Public Structure HWND__

        '''int
        Public unused As Integer
    End Structure

    <System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet:=System.Runtime.InteropServices.CharSet.[Unicode])> _
    Public Structure WIN32_FIND_DATAW

        '''DWORD->unsigned int
        Public dwFileAttributes As UInteger

        '''FILETIME->_FILETIME
        Public ftCreationTime As FILETIME

        '''FILETIME->_FILETIME
        Public ftLastAccessTime As FILETIME

        '''FILETIME->_FILETIME
        Public ftLastWriteTime As FILETIME

        '''DWORD->unsigned int
        Public nFileSizeHigh As UInteger

        '''DWORD->unsigned int
        Public nFileSizeLow As UInteger

        '''DWORD->unsigned int
        Public dwReserved0 As UInteger

        '''DWORD->unsigned int
        Public dwReserved1 As UInteger

        '''WCHAR[260]
        <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst:=260)> _
        Public cFileName As String

        '''WCHAR[14]
        <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst:=14)> _
        Public cAlternateFileName As String
    End Structure

    <System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)> _
    Public Structure FILETIME

        '''DWORD->unsigned int
        Public dwLowDateTime As UInteger

        '''DWORD->unsigned int
        Public dwHighDateTime As UInteger
    End Structure

    <System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)> _
    Public Structure HICON__

        '''int
        Public unused As Integer
    End Structure

    Partial Public Class NativeMethods

        '''Return Type: HANDLE->void*
        '''lpFileName: LPCWSTR->WCHAR*
        '''lpFindFileData: LPWIN32_FIND_DATAW->_WIN32_FIND_DATAW*
        <System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint:="FindFirstFileW")> _
        Public Shared Function FindFirstFileW(<System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal lpFileName As String, <System.Runtime.InteropServices.OutAttribute()> ByRef lpFindFileData As WIN32_FIND_DATAW) As System.IntPtr
        End Function

        '''Return Type: BOOL->int
        '''hFindFile: HANDLE->void*
        '''lpFindFileData: LPWIN32_FIND_DATAW->_WIN32_FIND_DATAW*
        <System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint:="FindNextFileW")> _
        Public Shared Function FindNextFileW(<System.Runtime.InteropServices.InAttribute()> ByVal hFindFile As System.IntPtr, <System.Runtime.InteropServices.OutAttribute()> ByRef lpFindFileData As WIN32_FIND_DATAW) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
        End Function

        '''Return Type: BOOL->int
        '''hFindFile: HANDLE->void*
        <System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint:="FindClose")> _
        Public Shared Function FindClose(ByVal hFindFile As System.IntPtr) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
        End Function

        '''Return Type: UINT->unsigned int
        '''lpBuffer: LPWSTR->WCHAR*
        '''uSize: UINT->unsigned int
        <System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint:="GetSystemDirectoryW")> _
        Public Overloads Shared Function GetSystemDirectoryW(<System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal lpBuffer As System.Text.StringBuilder, ByVal uSize As UInteger) As UInteger
        End Function

        '''Return Type: int
        '''hWnd: HWND->HWND__*
        '''lpString: LPWSTR->WCHAR*
        '''nMaxCount: int
        <System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint:="GetWindowTextW")> _
        Public Overloads Shared Function GetWindowTextW(<System.Runtime.InteropServices.InAttribute()> ByVal hWnd As System.IntPtr, <System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal lpString As System.Text.StringBuilder, ByVal nMaxCount As Integer) As Integer
        End Function

        '''Return Type: BOOL->int
        '''lpEnumFunc: WNDENUMPROC
        '''lParam: LPARAM->LONG_PTR->int
        <System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint:="EnumWindows")> _
        Public Overloads Shared Function EnumWindows(ByVal lpEnumFunc As WNDENUMPROC, <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.SysInt)> ByVal lParam As Integer) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
        End Function

        '''Return Type: BOOL->int
        '''lpBuffer: LPWSTR->WCHAR*
        '''nSize: LPDWORD->DWORD*
        <System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint:="GetComputerNameW")> _
        Public Overloads Shared Function GetComputerNameW(<System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal lpBuffer As System.Text.StringBuilder, ByRef nSize As UInteger) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
        End Function

        '''Return Type: BOOL->int
        '''WellKnownSidType: WELL_KNOWN_SID_TYPE->Anonymous_2a66b804_5814_4d7a_8190_9d15131e188a
        '''DomainSid: PSID->PVOID->void*
        '''pSid: PSID->PVOID->void*
        '''cbSid: DWORD*
        <System.Runtime.InteropServices.DllImportAttribute("advapi32.dll", EntryPoint:="CreateWellKnownSid")> _
        Public Overloads Shared Function CreateWellKnownSid(ByVal WellKnownSidType As WELL_KNOWN_SID_TYPE, <System.Runtime.InteropServices.InAttribute()> ByVal DomainSid As System.IntPtr, ByVal pSid As System.IntPtr, ByRef cbSid As UInteger) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
        End Function

        '''Return Type: BOOL->int
        '''nDestinationSidLength: DWORD->unsigned int
        '''pDestinationSid: PSID->PVOID->void*
        '''pSourceSid: PSID->PVOID->void*
        <System.Runtime.InteropServices.DllImportAttribute("advapi32.dll", EntryPoint:="CopySid")> _
        Public Shared Function CopySid(ByVal nDestinationSidLength As UInteger, ByVal pDestinationSid As System.IntPtr, <System.Runtime.InteropServices.InAttribute()> ByVal pSourceSid As System.IntPtr) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
        End Function

        '''Return Type: DWORD_PTR->ULONG_PTR->unsigned int
        '''pszPath: LPCWSTR->WCHAR*
        '''dwFileAttributes: DWORD->unsigned int
        '''psfi: SHFILEINFOW*
        '''cbFileInfo: UINT->unsigned int
        '''uFlags: UINT->unsigned int
        <System.Runtime.InteropServices.DllImportAttribute("shell32.dll", EntryPoint:="SHGetFileInfoW", CallingConvention:=System.Runtime.InteropServices.CallingConvention.StdCall)> _
        Public Shared Function SHGetFileInfoW(<System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal pszPath As String, ByVal dwFileAttributes As UInteger, ByRef psfi As SHFILEINFOW, ByVal cbFileInfo As UInteger, ByVal uFlags As UInteger) As UInteger
        End Function

        '''Return Type: DWORD->unsigned int
        '''lpName: LPCWSTR->WCHAR*
        '''lpBuffer: LPWSTR->WCHAR*
        '''nSize: DWORD->unsigned int
        <System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint:="GetEnvironmentVariableW")> _
        Public Overloads Shared Function GetEnvironmentVariableW(<System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal lpName As String, <System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal lpBuffer As System.Text.StringBuilder, ByVal nSize As UInteger) As UInteger
        End Function

        '''Return Type: int
        '''_Str: char*
        <System.Runtime.InteropServices.DllImportAttribute("ntdll.dll", EntryPoint:="atoi", CallingConvention:=System.Runtime.InteropServices.CallingConvention.Cdecl)> _
        Public Shared Function atoi(<System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)> ByVal _Str As String) As Integer
        End Function

        <System.Diagnostics.DebuggerStepThroughAttribute(), _
         System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")> _
        Public Overloads Shared Function GetSystemDirectoryW(ByRef lpBuffer As String) As UInteger
            Dim varlpBuffer As System.Text.StringBuilder
            Dim retVar_ As UInteger
            Dim sizeVar As UInteger = 2056
PerformCall:
            varlpBuffer = New System.Text.StringBuilder(CType(sizeVar, Integer))
            retVar_ = NativeMethods.GetSystemDirectoryW(varlpBuffer, sizeVar)
            If (retVar_ >= sizeVar) Then
                sizeVar = (retVar_ + CType(1, UInteger))
                goto PerformCall
            End If
            lpBuffer = varlpBuffer.ToString
            Return retVar_
        End Function

        <System.Diagnostics.DebuggerStepThroughAttribute(), _
         System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")> _
        Public Overloads Shared Function GetWindowTextW(ByVal hWnd As System.IntPtr, ByRef lpString As String) As Integer
            Dim varlpString As System.Text.StringBuilder = New System.Text.StringBuilder(1024)
            Dim methodRetVar As Integer
            methodRetVar = NativeMethods.GetWindowTextW(hWnd, varlpString, 1024)
            lpString = varlpString.ToString
            Return methodRetVar
        End Function

        <System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint:="EnumWindows")> _
        Public Overloads Shared Function EnumWindows(ByVal lpEnumFunc As WNDENUMPROC, ByVal lParam As System.IntPtr) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
        End Function

        <System.Diagnostics.DebuggerStepThroughAttribute(), _
         System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")> _
        Public Overloads Shared Function GetComputerNameW(ByRef lpBuffer As String) As Boolean
            Dim varlpBuffer As System.Text.StringBuilder
            Dim retVar_ As Boolean
            Dim sizeVar As UInteger = 2056
            Dim oldSizeVar_ As UInteger
PerformCall:
            oldSizeVar_ = sizeVar
            varlpBuffer = New System.Text.StringBuilder(CType(sizeVar, Integer))
            retVar_ = NativeMethods.GetComputerNameW(varlpBuffer, sizeVar)
            If (oldSizeVar_ <= sizeVar) Then
                sizeVar = (sizeVar * CType(2, UInteger))
                goto PerformCall
            End If
            lpBuffer = varlpBuffer.ToString
            Return retVar_
        End Function

        <System.Diagnostics.DebuggerStepThroughAttribute(), _
         System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")> _
        Public Overloads Shared Function CreateWellKnownSid(ByVal WellKnownSidType As WELL_KNOWN_SID_TYPE, ByVal DomainSid As System.IntPtr, ByRef pSid As PInvokePointer) As Boolean
            Dim varpSid As PInvokePointer
            Dim retVar_ As Boolean
            Dim sizeVar As UInteger = 2056
            Dim oldSizeVar_ As UInteger
PerformCall:
            oldSizeVar_ = sizeVar
            varpSid = New PInvokePointer(CType(sizeVar, Integer))
            retVar_ = NativeMethods.CreateWellKnownSid(WellKnownSidType, DomainSid, varpSid.IntPtr, sizeVar)
            If (sizeVar <= oldSizeVar_) Then
                varpSid.Free
                sizeVar = (sizeVar * CType(2, UInteger))
                goto PerformCall
            End If
            pSid = varpSid
            Return retVar_
        End Function

        <System.Diagnostics.DebuggerStepThroughAttribute(), _
         System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")> _
        Public Overloads Shared Function GetEnvironmentVariableW(ByVal lpName As String, ByRef lpBuffer As String) As UInteger
            Dim varlpBuffer As System.Text.StringBuilder
            Dim retVar_ As UInteger
            Dim sizeVar As UInteger = 2056
PerformCall:
            varlpBuffer = New System.Text.StringBuilder(CType(sizeVar, Integer))
            retVar_ = NativeMethods.GetEnvironmentVariableW(lpName, varlpBuffer, sizeVar)
            If (retVar_ >= sizeVar) Then
                sizeVar = (retVar_ + CType(1, UInteger))
                goto PerformCall
            End If
            lpBuffer = varlpBuffer.ToString
            Return retVar_
        End Function
    End Class

    Public Class PInvokePointer
        Implements System.IDisposable

        Private _ptr As System.IntPtr

        Private _size As Integer

        Public Sub New(ByVal ptr As System.IntPtr, ByVal size As Integer)
            MyBase.New
            _ptr = ptr
            _size = size
        End Sub

        Public Sub New(ByVal size As Integer)
            MyBase.New
            _ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(size)
            _size = size
        End Sub

        Public Overridable ReadOnly Property IntPtr() As System.IntPtr
            Get
                Return _ptr
            End Get
        End Property

        Public Overridable Sub Free()
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(_ptr)
            _ptr = System.IntPtr.Zero
            _size = 0
        End Sub

        Public Overridable Function ToByteArray() As Byte()
            Dim arr((_size) - 1) As Byte
            System.Runtime.InteropServices.Marshal.Copy(_ptr, arr, 0, _size)
            Return arr
        End Function

        Public Overridable Sub Dispose() Implements System.IDisposable.Dispose
            Me.Free
        End Sub
    End Class

End Namespace
