
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Runtime.InteropServices;

static internal class NativeMethods
{

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static IntPtr LoadLibraryEx(	[MarshalAs(UnmanagedType.LPTStr), In()]
string fileName, IntPtr intPtr, UInt32 flags)
	{

	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static Int32 FreeLibrary(IntPtr handle)
	{

	}

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	public static IntPtr GetProcAddress(	[In()]
IntPtr dllPtr, 	[MarshalAs(UnmanagedType.LPStr), In()]
string procName)
	{
	}

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
