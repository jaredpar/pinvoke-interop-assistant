
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Generated File ... Re-Run PInvokeTestGen to regenerate this file
namespace Generated
{

	///Return Type: LRESULT->LONG_PTR->int
	///param0: HWND->HWND__*
	///param1: UINT->unsigned int
	///param2: WPARAM->UINT_PTR->unsigned int
	///param3: LPARAM->LONG_PTR->int
	[System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
	public delegate int WNDPROC(System.IntPtr param0, uint param1, System.IntPtr param2, System.IntPtr param3);

	///Return Type: BOOL->int
	///param0: HWND->HWND__*
	///param1: LPARAM->LONG_PTR->int
	[System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
	public delegate int WNDENUMPROC(System.IntPtr param0, System.IntPtr param1);

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct DCB
	{

		///DWORD->unsigned int

		public uint DCBlength;
		///DWORD->unsigned int

		public uint BaudRate;
		///fBinary : 1
		///fParity : 1
		///fOutxCtsFlow : 1
		///fOutxDsrFlow : 1
		///fDtrControl : 2
		///fDsrSensitivity : 1
		///fTXContinueOnXoff : 1
		///fOutX : 1
		///fInX : 1
		///fErrorChar : 1
		///fNull : 1
		///fRtsControl : 2
		///fAbortOnError : 1
		///fDummy2 : 17

		public uint bitvector1;
		///WORD->unsigned short

		public ushort wReserved;
		///WORD->unsigned short

		public ushort XonLim;
		///WORD->unsigned short

		public ushort XoffLim;
		///BYTE->unsigned char

		public byte ByteSize;
		///BYTE->unsigned char

		public byte Parity;
		///BYTE->unsigned char

		public byte StopBits;
		///char

		public byte XonChar;
		///char

		public byte XoffChar;
		///char

		public byte ErrorChar;
		///char

		public byte EofChar;
		///char

		public byte EvtChar;
		///WORD->unsigned short

		public ushort wReserved1;
		public uint fBinary {
			get { return Convert.ToUInt32((this.bitvector1 & 1u)); }
			set { this.bitvector1 = Convert.ToUInt32((value | this.bitvector1)); }
		}

		public uint fParity {
			get { return Convert.ToUInt32(((this.bitvector1 & 2u) / 2)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 2) | this.bitvector1)); }
		}

		public uint fOutxCtsFlow {
			get { return Convert.ToUInt32(((this.bitvector1 & 4u) / 4)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 4) | this.bitvector1)); }
		}

		public uint fOutxDsrFlow {
			get { return Convert.ToUInt32(((this.bitvector1 & 8u) / 8)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 8) | this.bitvector1)); }
		}

		public uint fDtrControl {
			get { return Convert.ToUInt32(((this.bitvector1 & 48u) / 16)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 16) | this.bitvector1)); }
		}

		public uint fDsrSensitivity {
			get { return Convert.ToUInt32(((this.bitvector1 & 64u) / 64)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 64) | this.bitvector1)); }
		}

		public uint fTXContinueOnXoff {
			get { return Convert.ToUInt32(((this.bitvector1 & 128u) / 128)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 128) | this.bitvector1)); }
		}

		public uint fOutX {
			get { return Convert.ToUInt32(((this.bitvector1 & 256u) / 256)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 256) | this.bitvector1)); }
		}

		public uint fInX {
			get { return Convert.ToUInt32(((this.bitvector1 & 512u) / 512)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 512) | this.bitvector1)); }
		}

		public uint fErrorChar {
			get { return Convert.ToUInt32(((this.bitvector1 & 1024u) / 1024)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 1024) | this.bitvector1)); }
		}

		public uint fNull {
			get { return Convert.ToUInt32(((this.bitvector1 & 2048u) / 2048)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 2048) | this.bitvector1)); }
		}

		public uint fRtsControl {
			get { return Convert.ToUInt32(((this.bitvector1 & 12288u) / 4096)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 4096) | this.bitvector1)); }
		}

		public uint fAbortOnError {
			get { return Convert.ToUInt32(((this.bitvector1 & 16384u) / 16384)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 16384) | this.bitvector1)); }
		}

		public uint fDummy2 {
			get { return Convert.ToUInt32(((this.bitvector1 & 4294934528u) / 32768)); }
			set { this.bitvector1 = Convert.ToUInt32(((value * 32768) | this.bitvector1)); }
		}
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct IMAGE_LINENUMBER
	{

		///Anonymous_9458c9ac_2eca_481f_8912_f5ffcf5913b6

		public Anonymous_9458c9ac_2eca_481f_8912_f5ffcf5913b6 Type;
		///WORD->unsigned short
		public ushort Linenumber;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
	public struct Anonymous_9458c9ac_2eca_481f_8912_f5ffcf5913b6
	{

		///DWORD->unsigned int
		[System.Runtime.InteropServices.FieldOffsetAttribute(0)]

		public uint SymbolTableIndex;
		///DWORD->unsigned int
		[System.Runtime.InteropServices.FieldOffsetAttribute(0)]
		public uint VirtualAddress;
	}

	public enum WELL_KNOWN_SID_TYPE
	{

		///WinNullSid -> 0
		WinNullSid = 0,

		///WinWorldSid -> 1
		WinWorldSid = 1,

		///WinLocalSid -> 2
		WinLocalSid = 2,

		///WinCreatorOwnerSid -> 3
		WinCreatorOwnerSid = 3,

		///WinCreatorGroupSid -> 4
		WinCreatorGroupSid = 4,

		///WinCreatorOwnerServerSid -> 5
		WinCreatorOwnerServerSid = 5,

		///WinCreatorGroupServerSid -> 6
		WinCreatorGroupServerSid = 6,

		///WinNtAuthoritySid -> 7
		WinNtAuthoritySid = 7,

		///WinDialupSid -> 8
		WinDialupSid = 8,

		///WinNetworkSid -> 9
		WinNetworkSid = 9,

		///WinBatchSid -> 10
		WinBatchSid = 10,

		///WinInteractiveSid -> 11
		WinInteractiveSid = 11,

		///WinServiceSid -> 12
		WinServiceSid = 12,

		///WinAnonymousSid -> 13
		WinAnonymousSid = 13,

		///WinProxySid -> 14
		WinProxySid = 14,

		///WinEnterpriseControllersSid -> 15
		WinEnterpriseControllersSid = 15,

		///WinSelfSid -> 16
		WinSelfSid = 16,

		///WinAuthenticatedUserSid -> 17
		WinAuthenticatedUserSid = 17,

		///WinRestrictedCodeSid -> 18
		WinRestrictedCodeSid = 18,

		///WinTerminalServerSid -> 19
		WinTerminalServerSid = 19,

		///WinRemoteLogonIdSid -> 20
		WinRemoteLogonIdSid = 20,

		///WinLogonIdsSid -> 21
		WinLogonIdsSid = 21,

		///WinLocalSystemSid -> 22
		WinLocalSystemSid = 22,

		///WinLocalServiceSid -> 23
		WinLocalServiceSid = 23,

		///WinNetworkServiceSid -> 24
		WinNetworkServiceSid = 24,

		///WinBuiltinDomainSid -> 25
		WinBuiltinDomainSid = 25,

		///WinBuiltinAdministratorsSid -> 26
		WinBuiltinAdministratorsSid = 26,

		///WinBuiltinUsersSid -> 27
		WinBuiltinUsersSid = 27,

		///WinBuiltinGuestsSid -> 28
		WinBuiltinGuestsSid = 28,

		///WinBuiltinPowerUsersSid -> 29
		WinBuiltinPowerUsersSid = 29,

		///WinBuiltinAccountOperatorsSid -> 30
		WinBuiltinAccountOperatorsSid = 30,

		///WinBuiltinSystemOperatorsSid -> 31
		WinBuiltinSystemOperatorsSid = 31,

		///WinBuiltinPrintOperatorsSid -> 32
		WinBuiltinPrintOperatorsSid = 32,

		///WinBuiltinBackupOperatorsSid -> 33
		WinBuiltinBackupOperatorsSid = 33,

		///WinBuiltinReplicatorSid -> 34
		WinBuiltinReplicatorSid = 34,

		///WinBuiltinPreWindows2000CompatibleAccessSid -> 35
		WinBuiltinPreWindows2000CompatibleAccessSid = 35,

		///WinBuiltinRemoteDesktopUsersSid -> 36
		WinBuiltinRemoteDesktopUsersSid = 36,

		///WinBuiltinNetworkConfigurationOperatorsSid -> 37
		WinBuiltinNetworkConfigurationOperatorsSid = 37,

		///WinAccountAdministratorSid -> 38
		WinAccountAdministratorSid = 38,

		///WinAccountGuestSid -> 39
		WinAccountGuestSid = 39,

		///WinAccountKrbtgtSid -> 40
		WinAccountKrbtgtSid = 40,

		///WinAccountDomainAdminsSid -> 41
		WinAccountDomainAdminsSid = 41,

		///WinAccountDomainUsersSid -> 42
		WinAccountDomainUsersSid = 42,

		///WinAccountDomainGuestsSid -> 43
		WinAccountDomainGuestsSid = 43,

		///WinAccountComputersSid -> 44
		WinAccountComputersSid = 44,

		///WinAccountControllersSid -> 45
		WinAccountControllersSid = 45,

		///WinAccountCertAdminsSid -> 46
		WinAccountCertAdminsSid = 46,

		///WinAccountSchemaAdminsSid -> 47
		WinAccountSchemaAdminsSid = 47,

		///WinAccountEnterpriseAdminsSid -> 48
		WinAccountEnterpriseAdminsSid = 48,

		///WinAccountPolicyAdminsSid -> 49
		WinAccountPolicyAdminsSid = 49,

		///WinAccountRasAndIasServersSid -> 50
		WinAccountRasAndIasServersSid = 50,

		///WinNTLMAuthenticationSid -> 51
		WinNTLMAuthenticationSid = 51,

		///WinDigestAuthenticationSid -> 52
		WinDigestAuthenticationSid = 52,

		///WinSChannelAuthenticationSid -> 53
		WinSChannelAuthenticationSid = 53,

		///WinThisOrganizationSid -> 54
		WinThisOrganizationSid = 54,

		///WinOtherOrganizationSid -> 55
		WinOtherOrganizationSid = 55,

		///WinBuiltinIncomingForestTrustBuildersSid -> 56
		WinBuiltinIncomingForestTrustBuildersSid = 56,

		///WinBuiltinPerfMonitoringUsersSid -> 57
		WinBuiltinPerfMonitoringUsersSid = 57,

		///WinBuiltinPerfLoggingUsersSid -> 58
		WinBuiltinPerfLoggingUsersSid = 58,

		///WinBuiltinAuthorizationAccessSid -> 59
		WinBuiltinAuthorizationAccessSid = 59,

		///WinBuiltinTerminalServerLicenseServersSid -> 60
		WinBuiltinTerminalServerLicenseServersSid = 60,

		///WinBuiltinDCOMUsersSid -> 61
		WinBuiltinDCOMUsersSid = 61
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
	public struct SHFILEINFOW
	{

		///HICON->HICON__*

		public System.IntPtr hIcon;
		///int

		public int iIcon;
		///DWORD->unsigned int

		public uint dwAttributes;
		///WCHAR[260]
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 260)]

		public string szDisplayName;
		///WCHAR[80]
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct HWND__
	{

		///int
		public int unused;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
	public struct WIN32_FIND_DATAW
	{

		///DWORD->unsigned int

		public uint dwFileAttributes;
		///FILETIME->_FILETIME

		public FILETIME ftCreationTime;
		///FILETIME->_FILETIME

		public FILETIME ftLastAccessTime;
		///FILETIME->_FILETIME

		public FILETIME ftLastWriteTime;
		///DWORD->unsigned int

		public uint nFileSizeHigh;
		///DWORD->unsigned int

		public uint nFileSizeLow;
		///DWORD->unsigned int

		public uint dwReserved0;
		///DWORD->unsigned int

		public uint dwReserved1;
		///WCHAR[260]
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 260)]

		public string cFileName;
		///WCHAR[14]
		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 14)]
		public string cAlternateFileName;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct FILETIME
	{

		///DWORD->unsigned int

		public uint dwLowDateTime;
		///DWORD->unsigned int
		public uint dwHighDateTime;
	}

	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct HICON__
	{

		///int
		public int unused;
	}

	public partial class NativeMethods
	{

		///Return Type: HANDLE->void*
		///lpFileName: LPCWSTR->WCHAR*
		///lpFindFileData: LPWIN32_FIND_DATAW->_WIN32_FIND_DATAW*
		[System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "FindFirstFileW")]
		public static System.IntPtr FindFirstFileW(		[System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string lpFileName, 		[System.Runtime.InteropServices.OutAttribute()]
ref WIN32_FIND_DATAW lpFindFileData)
		{
		}

		///Return Type: BOOL->int
		///hFindFile: HANDLE->void*
		///lpFindFileData: LPWIN32_FIND_DATAW->_WIN32_FIND_DATAW*
		[System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "FindNextFileW")]
		[return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
		public static bool FindNextFileW(		[System.Runtime.InteropServices.InAttribute()]
System.IntPtr hFindFile, 		[System.Runtime.InteropServices.OutAttribute()]
ref WIN32_FIND_DATAW lpFindFileData)
		{
		}

		///Return Type: BOOL->int
		///hFindFile: HANDLE->void*
		[System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "FindClose")]
		[return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
		public static bool FindClose(System.IntPtr hFindFile)
		{
		}

		///Return Type: UINT->unsigned int
		///lpBuffer: LPWSTR->WCHAR*
		///uSize: UINT->unsigned int
		[System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "GetSystemDirectoryW")]
		public static uint GetSystemDirectoryW(		[System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
System.Text.StringBuilder lpBuffer, uint uSize)
		{
		}

		///Return Type: int
		///hWnd: HWND->HWND__*
		///lpString: LPWSTR->WCHAR*
		///nMaxCount: int
		[System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "GetWindowTextW")]
		public static int GetWindowTextW(		[System.Runtime.InteropServices.InAttribute()]
System.IntPtr hWnd, 		[System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
System.Text.StringBuilder lpString, int nMaxCount)
		{
		}

		///Return Type: BOOL->int
		///lpEnumFunc: WNDENUMPROC
		///lParam: LPARAM->LONG_PTR->int
		[System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "EnumWindows")]
		[return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
		public static bool EnumWindows(WNDENUMPROC lpEnumFunc, 		[System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.SysInt)]
int lParam)
		{
		}

		///Return Type: BOOL->int
		///lpBuffer: LPWSTR->WCHAR*
		///nSize: LPDWORD->DWORD*
		[System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "GetComputerNameW")]
		[return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
		public static bool GetComputerNameW(		[System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
System.Text.StringBuilder lpBuffer, ref uint nSize)
		{
		}

		///Return Type: BOOL->int
		///WellKnownSidType: WELL_KNOWN_SID_TYPE->Anonymous_2a66b804_5814_4d7a_8190_9d15131e188a
		///DomainSid: PSID->PVOID->void*
		///pSid: PSID->PVOID->void*
		///cbSid: DWORD*
		[System.Runtime.InteropServices.DllImportAttribute("advapi32.dll", EntryPoint = "CreateWellKnownSid")]
		[return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
		public static bool CreateWellKnownSid(WELL_KNOWN_SID_TYPE WellKnownSidType, 		[System.Runtime.InteropServices.InAttribute()]
System.IntPtr DomainSid, System.IntPtr pSid, ref uint cbSid)
		{
		}

		///Return Type: BOOL->int
		///nDestinationSidLength: DWORD->unsigned int
		///pDestinationSid: PSID->PVOID->void*
		///pSourceSid: PSID->PVOID->void*
		[System.Runtime.InteropServices.DllImportAttribute("advapi32.dll", EntryPoint = "CopySid")]
		[return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
		public static bool CopySid(uint nDestinationSidLength, System.IntPtr pDestinationSid, 		[System.Runtime.InteropServices.InAttribute()]
System.IntPtr pSourceSid)
		{
		}

		///Return Type: DWORD_PTR->ULONG_PTR->unsigned int
		///pszPath: LPCWSTR->WCHAR*
		///dwFileAttributes: DWORD->unsigned int
		///psfi: SHFILEINFOW*
		///cbFileInfo: UINT->unsigned int
		///uFlags: UINT->unsigned int
		[System.Runtime.InteropServices.DllImportAttribute("shell32.dll", EntryPoint = "SHGetFileInfoW", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
		public static uint SHGetFileInfoW(		[System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string pszPath, uint dwFileAttributes, ref SHFILEINFOW psfi, uint cbFileInfo, uint uFlags)
		{
		}

		///Return Type: DWORD->unsigned int
		///lpName: LPCWSTR->WCHAR*
		///lpBuffer: LPWSTR->WCHAR*
		///nSize: DWORD->unsigned int
		[System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "GetEnvironmentVariableW")]
		public static uint GetEnvironmentVariableW(		[System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string lpName, 		[System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
System.Text.StringBuilder lpBuffer, uint nSize)
		{
		}

		///Return Type: int
		///_Str: char*
		[System.Runtime.InteropServices.DllImportAttribute("ntdll.dll", EntryPoint = "atoi", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
		public static int atoi(		[System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
string _Str)
		{
		}

		[System.Diagnostics.DebuggerStepThroughAttribute(), System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")]
		public static uint GetSystemDirectoryW(ref string lpBuffer)
		{
			System.Text.StringBuilder varlpBuffer = default(System.Text.StringBuilder);
			uint retVar_ = 0;
			uint sizeVar = 2056;
			PerformCall:
			varlpBuffer = new System.Text.StringBuilder(Convert.ToInt32(sizeVar));
			retVar_ = NativeMethods.GetSystemDirectoryW(varlpBuffer, sizeVar);
			if ((retVar_ >= sizeVar)) {
				sizeVar = (retVar_ + Convert.ToUInt32(1));
				goto PerformCall;
			}
			lpBuffer = varlpBuffer.ToString;
			return retVar_;
		}

		[System.Diagnostics.DebuggerStepThroughAttribute(), System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")]
		public static int GetWindowTextW(System.IntPtr hWnd, ref string lpString)
		{
			System.Text.StringBuilder varlpString = new System.Text.StringBuilder(1024);
			int methodRetVar = 0;
			methodRetVar = NativeMethods.GetWindowTextW(hWnd, varlpString, 1024);
			lpString = varlpString.ToString;
			return methodRetVar;
		}

		[System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "EnumWindows")]
		[return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
		public static bool EnumWindows(WNDENUMPROC lpEnumFunc, System.IntPtr lParam)
		{
		}

		[System.Diagnostics.DebuggerStepThroughAttribute(), System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")]
		public static bool GetComputerNameW(ref string lpBuffer)
		{
			System.Text.StringBuilder varlpBuffer = default(System.Text.StringBuilder);
			bool retVar_ = false;
			uint sizeVar = 2056;
			uint oldSizeVar_ = 0;
			PerformCall:
			oldSizeVar_ = sizeVar;
			varlpBuffer = new System.Text.StringBuilder(Convert.ToInt32(sizeVar));
			retVar_ = NativeMethods.GetComputerNameW(varlpBuffer, ref sizeVar);
			if ((oldSizeVar_ <= sizeVar)) {
				sizeVar = (sizeVar * Convert.ToUInt32(2));
				goto PerformCall;
			}
			lpBuffer = varlpBuffer.ToString;
			return retVar_;
		}

		[System.Diagnostics.DebuggerStepThroughAttribute(), System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")]
		public static bool CreateWellKnownSid(WELL_KNOWN_SID_TYPE WellKnownSidType, System.IntPtr DomainSid, ref PInvokePointer pSid)
		{
			PInvokePointer varpSid = null;
			bool retVar_ = false;
			uint sizeVar = 2056;
			uint oldSizeVar_ = 0;
			PerformCall:
			oldSizeVar_ = sizeVar;
			varpSid = new PInvokePointer(Convert.ToInt32(sizeVar));
			retVar_ = NativeMethods.CreateWellKnownSid(WellKnownSidType, DomainSid, varpSid.IntPtr, ref sizeVar);
			if ((sizeVar <= oldSizeVar_)) {
				varpSid.Free();
				sizeVar = (sizeVar * Convert.ToUInt32(2));
				goto PerformCall;
			}
			pSid = varpSid;
			return retVar_;
		}

		[System.Diagnostics.DebuggerStepThroughAttribute(), System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")]
		public static uint GetEnvironmentVariableW(string lpName, ref string lpBuffer)
		{
			System.Text.StringBuilder varlpBuffer = default(System.Text.StringBuilder);
			uint retVar_ = 0;
			uint sizeVar = 2056;
			PerformCall:
			varlpBuffer = new System.Text.StringBuilder(Convert.ToInt32(sizeVar));
			retVar_ = NativeMethods.GetEnvironmentVariableW(lpName, varlpBuffer, sizeVar);
			if ((retVar_ >= sizeVar)) {
				sizeVar = (retVar_ + Convert.ToUInt32(1));
				goto PerformCall;
			}
			lpBuffer = varlpBuffer.ToString;
			return retVar_;
		}
	}

	public class PInvokePointer : System.IDisposable
	{


		private System.IntPtr _ptr;

		private int _size;
		public PInvokePointer(System.IntPtr ptr, int size) : base()
		{
			_ptr = ptr;
			_size = size;
		}

		public PInvokePointer(int size) : base()
		{
			_ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(size);
			_size = size;
		}

		public virtual System.IntPtr IntPtr {
			get { return _ptr; }
		}

		public virtual void Free()
		{
			System.Runtime.InteropServices.Marshal.FreeCoTaskMem(_ptr);
			_ptr = System.IntPtr.Zero;
			_size = 0;
		}

		public virtual byte[] ToByteArray()
		{
			byte[] arr = new byte[(_size)];
			System.Runtime.InteropServices.Marshal.Copy(_ptr, arr, 0, _size);
			return arr;
		}

		public virtual void Dispose()
		{
			this.Free();
		}
	}

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
