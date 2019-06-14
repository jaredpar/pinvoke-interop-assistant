
// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.IO;
using PInvoke;
using PInvoke.Parser;
using PInvoke.Transform;
using PInvoke.NativeTypes;

internal static class Program
{

    /// <summary>
    /// Dll's which are somewhat more troublesome and should not be loaded by default
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public static IEnumerable<string> FullDllList
    {
        get
        {
            List<string> list = new List<string>(ProcedureFinder.DefaultDllList)
            {
                "oleaut32.dll",
                "ole32.dll",
                "ole2.dll",
                "ole2disp.dll",
                "ole2nls.dll",
                "msvcr80.dll",
                "nt64.dll",
                "msimg32.dll",
                "winscard.dll",
                "winspool.dll",
                "comctl32.dll"
            };
            return list;
        }
    }

    private static BasicSymbolStorage CreateInitialBasicSymbolStorage()
    {
        var ns = new BasicSymbolStorage();

        // Add in the basic type defs
        ns.AddTypeDef(new NativeTypeDef("SIZE_T", new NativeBuiltinType(BuiltinType.NativeInt32, true)));
        ns.AddTypeDef(new NativeTypeDef("DWORD64", new NativeBuiltinType(BuiltinType.NativeInt64, true)));
        ns.AddTypeDef(new NativeTypeDef("HWND", new NativePointer(BuiltinType.NativeVoid)));
        ns.AddTypeDef(new NativeTypeDef("HMENU", new NativePointer(BuiltinType.NativeVoid)));
        ns.AddTypeDef(new NativeTypeDef("HACCEL", new NativePointer(BuiltinType.NativeVoid)));
        ns.AddTypeDef(new NativeTypeDef("HBRUSH", new NativePointer(BuiltinType.NativeVoid)));
        ns.AddTypeDef(new NativeTypeDef("HFONT", new NativePointer(BuiltinType.NativeVoid)));
        ns.AddTypeDef(new NativeTypeDef("HDC", new NativePointer(BuiltinType.NativeVoid)));
        ns.AddTypeDef(new NativeTypeDef("HICON", new NativePointer(BuiltinType.NativeVoid)));

        return ns;
    }

    /// <summary>
    /// Verification of the generated code
    /// </summary>
    /// <param name="ns"></param>
    /// <remarks></remarks>

    private static void VerifyGeneratedStorage(BasicSymbolStorage ns)
    {
        NativeProcedure proc = null;
        VerifyTrue(ns.TryGetGlobalSymbol("SendMessageA", out proc));
        VerifyTrue(ns.TryGetGlobalSymbol("SendMessageW", out proc));
        VerifyTrue(ns.TryGetGlobalSymbol("GetForegroundWindow", out proc));
        VerifyTrue(ns.TryGetGlobalSymbol("CreateWellKnownSid", out proc));

        NativeTypeDef typedef = null;
        VerifyTrue(ns.TryGetGlobalSymbol("LPCSTR", out typedef));
        VerifyTrue(ns.TryGetGlobalSymbol("LPWSTR", out typedef));

        NativeType defined = null;
        VerifyTrue(ns.TryGetType("WNDPROC", out defined));
        VerifyTrue(ns.TryGetType("HOOKPROC", out defined));
        VerifyTrue(ns.TryGetType("tagPOINT", out defined));
        VerifyTrue(ns.TryGetType("_SYSTEM_INFO", out defined));

        NativeConstant c = null;
        VerifyTrue(ns.TryGetGlobalSymbol("WM_PAINT", out c));
        VerifyTrue(ns.TryGetGlobalSymbol("WM_LBUTTONDOWN", out c));

    }

    private static void VerifyTrue(bool value)
    {
        if (!value)
        {
            throw new Exception();
        }
    }

    private static BasicSymbolStorage Generate(TextWriter writer)
    {
        NativeCodeAnalyzer analyzer = NativeCodeAnalyzerFactory.Create(OsVersion.WindowsVista);
        analyzer.IncludePathList.AddRange(NativeCodeAnalyzerFactory.GetCommonSdkPaths());

        // Run the preprocessor
        analyzer.Trace = true;
        string winPath = Path.Combine(PInvoke.Parser.NativeCodeAnalyzerFactory.GetPlatformSdkIncludePath(), "windows.h");
        TextReaderBag tr = analyzer.RunPreProcessor(winPath);
        File.WriteAllText("d:\\temp\\windows.out.h", tr.TextReader.ReadToEnd());
        analyzer.Trace = false;

        NativeCodeAnalyzerResult result = analyzer.Analyze(winPath);
        ErrorProvider ep = result.ErrorProvider;
        if (ep.Errors.Count > 0)
        {
            Debug.Fail("Encountered an error during the parse");
        }
        NativeSymbolBag bag = NativeSymbolBag.CreateFrom(result, CreateInitialBasicSymbolStorage(), ep);

        // Resolve with the full dll list
        using (ProcedureFinder finder = new ProcedureFinder(FullDllList))
        {
            bag.TryResolveSymbolsAndValues(finder, ep);
        }

        foreach (string msg in ep.AllMessages)
        {
            writer.WriteLine("' " + msg);
        }

        // GenerateCode(writer, bag)

        // Now write out the file
        var ns = new BasicSymbolStorage();
        bag.SaveToNativeStorage(ns);
        VerifyGeneratedStorage(ns);

        // TODO: need to write to file again otherwise it's just in memory.  

        // Copy the file to the various applications
        File.Copy("windows.xml", "..\\..\\..\\ConsoleTool\\bin\\Debug\\windows.xml", true);
        File.Copy("windows.xml", "..\\..\\Data\\windows.xml", true);

        string fullInstallTarget = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), Path.Combine(PInvoke.Constants.ProductName, "Data\\windows.xml"));
        if (File.Exists(fullInstallTarget))
        {
            File.Copy("windows.xml", fullInstallTarget, true);
        }

        return ns;
    }


    public static void Main()
    {
        using (StreamWriter sw = new StreamWriter("d:\\temp\\windows.vb"))
        {
            Generate(sw);
        }

    }

}
