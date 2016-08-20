
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.CodeDom;
using System.IO;
using Pinvoke;
using PInvoke.Parser;
using PInvoke.Transform;

static class Module1
{


	public static void Main(string[] args)
	{
		NativeStorage ns = NativeStorage.LoadFromAssemblyPath;

		NativeSymbolBag bag = new NativeSymbolBag(ns);

		NativeProcedure ntProc = null;
		bag.TryFindOrLoadProcedure("FindFirstFileW", ntProc);
		bag.TryFindOrLoadProcedure("FindNextFileW", ntProc);
		bag.TryFindOrLoadProcedure("FindClose", ntProc);
		bag.TryFindOrLoadProcedure("GetSystemDirectoryW", ntProc);
		bag.TryFindOrLoadProcedure("GetWindowTextW", ntProc);
		bag.TryFindOrLoadProcedure("EnumWindows", ntProc);
		bag.TryFindOrLoadProcedure("GetComputerNameW", ntProc);
		bag.TryFindOrLoadProcedure("CreateWellKnownSid", ntProc);
		bag.TryFindOrLoadProcedure("CopySid", ntProc);
		bag.TryFindOrLoadProcedure("IsEqualSid", ntProc);
		bag.TryFindOrLoadProcedure("SHGetFileInfoW", ntProc);
		bag.TryFindOrLoadProcedure("GetEnvironmentVariableW", ntProc);
		bag.TryFindOrLoadProcedure("atoi", ntProc);

		NativeDefinedType ntDefined = null;
		NativeTypeDef ntTypedef = null;
		bag.TryFindOrLoadDefinedType("WNDPROC", ntDefined);
		bag.TryFindOrLoadDefinedType("WNDENUMPROC", ntDefined);
		bag.TryFindOrLoadDefinedType("COMSTAT", ntDefined);
		bag.TryFindOrLoadDefinedType("_DCB", ntDefined);
		bag.TryFindOrLoadDefinedType("_IMAGE_LINENUMBER", ntDefined);


		BasicConverter convert = new BasicConverter(LanguageType.VisualBasic, ns);
		string code = convert.ConvertToPInvokeCode(bag);
		code = "' Generated File ... Re-Run PInvokeTestGen to regenerate this file" + Constants.vbCrLf + "Namespace Generated" + Constants.vbCrLf + code + Constants.vbCrLf + "End Namespace";
		IO.File.WriteAllText(args(0), code);
	}
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
