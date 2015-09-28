/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SignatureGenerator;

namespace ConsoleTool
{
    static class Program
    {
        private class CommandLineOptions
        {
            #region Fields

            /// <summary>
            /// Whether to generate signatures for 64-bit target platform, <B>false</B> by default.
            /// </summary>
            public readonly bool TargetPlatform64Bit;

            /// <summary>
            /// Whether to generate signatures for ANSI target platform, <B>false</B> by default.
            /// </summary>
            public readonly bool TargetPlatformAnsi;

            /// <summary>
            /// Whether to use plain C++ data types instead of the fancy Windows ones, <B>false</B> by default.
            /// </summary>
            public readonly bool UsePlainCDataTypes;

            /// <summary>
            /// Whether to use colorful console output, <B>true</B> by default.
            /// </summary>
            public readonly bool UseColorConsoleOutput;

            /// <summary>
            /// Whether to annotate pointer parameters with marshal direction, <B>false</B> by default.
            /// </summary>
            public readonly bool PrintMarshalDirection;

            /// <summary>
            /// Whether to suppress displaying the program logo, <B>false</B> by default.
            /// </summary>
            public readonly bool SuppressLogo;

            /// <summary>
            /// Whether to suppress outputting info & warning & error messages, <B>false</B> by default.
            /// </summary>
            public readonly bool SuppressMessages;

            /// <summary>
            /// Whether to suppress outputting complex type definitions, <B>false</B> by default.
            /// </summary>
            public readonly bool SuppressTypeDefinitions;

            /// <summary>
            /// The path to an assembly file that should be reflected.
            /// </summary>
            public readonly string AssemblyFilePath;

            /// <summary>
            /// The arguments that should be interpreted according to the <see cref="Action"/>.
            /// </summary>
            public readonly List<string> Arguments;

            #endregion

            #region Construction

            /// <summary>
            /// Parses the given command line.
            /// </summary>
            public CommandLineOptions(string[] args)
            {
                UseColorConsoleOutput = true;
                Arguments = new List<string>();

                bool no_more_switches = false;

                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg.Length == 0) continue;

                    if (arg[0] == '/') arg = "-" + arg.Substring(1);

                    switch (arg.ToLowerInvariant())
                    {
                        case "-64":
                        case "-x64":
                        case "-ia64":
                        case "-win64":
                        case "-bit:64":
                        case "-bits:64":
                        {
                            TargetPlatform64Bit = true;
                            break;
                        }

                        case "-32":
                        case "-x86":
                        case "-ia32":
                        case "-win32":
                        case "-bit:32":
                        case "-bits:32":
                        {
                            TargetPlatform64Bit = false;
                            break;
                        }

                        case "-ansi":
                        case "-ansi+":
                        case "-unicode-":
                        {
                            TargetPlatformAnsi = true;
                            break;
                        }

                        case "-unicode":
                        case "-unicode+":
                        case "-ansi-":
                        {
                            TargetPlatformAnsi = false;
                            break;
                        }

                        case "-wintypes-":
                        case "-plaintypes":
                        case "-plaintypes+":
                        {
                            UsePlainCDataTypes = true;
                            break;
                        }

                        case "-wintypes":
                        case "-wintypes+":
                        case "-plaintypes-":
                        {
                            UsePlainCDataTypes = false;
                            break;
                        }

                        case "-color":
                        case "-color+":
                        case "-bw-":
                        {
                            UseColorConsoleOutput = true;
                            break;
                        }

                        case "-bw":
                        case "-bw+":
                        case "-color-":
                        {
                            UseColorConsoleOutput = false;
                            break;
                        }

                        case "-inout":
                        case "-direction":
                        {
                            PrintMarshalDirection = true;
                            break;
                        }

                        case "-nologo":
                        case "-logo-":
                        {
                            SuppressLogo = true;
                            break;
                        }

                        case "-msg-":
                        case "-nomsg":
                        {
                            SuppressMessages = true;
                            break;
                        }

                        case "-types-":
                        case "-notypes":
                        {
                            SuppressTypeDefinitions = true;
                            break;
                        }

                        case "-?":
                        case "-help":
                        {
                            PrintLogo();
                            PrintHelp();
                            Environment.Exit(0);
                            break;
                        }

                        case "-file":
                        {
                            if (i + 1 < args.Length)
                            {
                                AssemblyFilePath = args[++i];
                                try
                                {
                                    if (AssemblyFilePath == "mscorlib")
                                    {
                                        AssemblyFilePath = typeof(int).Assembly.FullName;
                                    }
                                    else
                                    {
                                        AssemblyFilePath = System.IO.Path.GetFullPath(AssemblyFilePath);
                                    }
                                }
                                catch (Exception)
                                { }
                            }
                            break;
                        }

                        case "--":
                        {
                            no_more_switches = true;
                            break;
                        }

                        default:
                        {
                            if (arg[0] == '-' && !no_more_switches)
                            {
                                if (!SuppressLogo)
                                {
                                    PrintLogo();
                                }

                                using (TextWriterPrinter twp = new TextWriterPrinter(Console.Error))
                                {
                                    ConsoleErrors.ERROR_UnrecognizedOption.PrintTo(twp, args[i]);
                                }
                                Environment.Exit(1);
                            }
                            else Arguments.Add(args[i]);
                            break;
                        }
                    }
                }

                if (!SuppressLogo)
                {
                    PrintLogo();
                }

                if (String.IsNullOrEmpty(AssemblyFilePath))
                {
                    using (TextWriterPrinter twp = new TextWriterPrinter(Console.Error))
                    {
                        ConsoleErrors.ERROR_AssemblyFileNotSpecified.PrintTo(twp);
                    }
                    Environment.Exit(1);
                }
            }

            #endregion

            #region Logo and Help

            private static void PrintLogo()
            {
//#if RAZZLE_BUILD
//                string version = ThisAssembly.InformationalVersion;
//#else // RAZZLE_BUILD
//                string version = typeof(NativeSignature).Assembly.GetName().Version.ToString();
//#endif // RAZZLE_BUILD

                // hard-coding 1.0 for the MSDN release
                string version = "1.0";

                Console.WriteLine(String.Format(Resources.INFO_CommandLineLogo, version));
            }

            private static void PrintHelp()
            {
                Console.WriteLine(Resources.INFO_CommandLineHelp);
            }

            #endregion
        }

        private static BindingFlags methodBindingFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// <B>True</B> is a type load failed, <B>false</B> if an assembly load failed.
        /// </summary>
        private static void LogLoadException(Exception e, bool typeLoad, string failedName)
        {
            string message;

            // extract LoaderExceptions if available
            ReflectionTypeLoadException rtle = e as ReflectionTypeLoadException;
            if (rtle != null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < rtle.LoaderExceptions.Length; i++)
                {
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append(rtle.LoaderExceptions[i].Message);
                }

                message = sb.ToString();
            }
            else
            {
                message = e.Message;
            }

            if (typeLoad)
            {
                ConsoleErrors.ERROR_UnableToLoadType.PrintTo(logPrinter, failedName, message);
            }
            else
            {
                ConsoleErrors.ERROR_UnableToLoadAssembly.PrintTo(logPrinter, failedName, message);
            }
        }

        private static void Generate()
        {
            if (options.Arguments.Count == 0)
            {
                Type[] types = null;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (Exception e)
                {
                    LogLoadException(e, false, options.AssemblyFilePath);
                }

                if (types != null)
                {
                    foreach (Type type in types)
                    {
                        GenerateType(type);
                    }
                }
            }
            else
            {
                for (int i = 0; i < options.Arguments.Count; i++)
                {
                    string arg = options.Arguments[i];

                    int index = arg.IndexOf("::");
                    if (index >= 0)
                    {
                        // the string looks like a method designation
                        string type_name = arg.Substring(0, index);
                        string method_name = arg.Substring(index + 2);

                        Type type;
                        try
                        {
                            // throw on error and ignore case
                            type = assembly.GetType(type_name, true, true);
                        }
                        catch (Exception e)
                        {
                            LogLoadException(e, true, type_name);
                            continue;
                        }
                        Debug.Assert(type != null);

                        MethodInfo mi = null;
                        try
                        {
                            mi = type.GetMethod(method_name, methodBindingFlags);
                        }
                        catch (AmbiguousMatchException)
                        { }

                        bool have_method = false;
                        if (mi != null)
                        {
                            have_method = true;
                            GenerateMethod(mi);
                        }
                        else
                        {
                            // exact match failed (more than one method of the specified name may exist)
                            foreach (MethodInfo mi2 in type.GetMethods(methodBindingFlags))
                            {
                                if (String.Compare(mi2.Name, method_name, true) == 0)
                                {
                                    have_method = true;
                                    GenerateMethod(mi2);
                                }
                            }
                        }

                        if (!have_method)
                        {
                            ConsoleErrors.ERROR_UnableToFindMethod.PrintTo(logPrinter, method_name, type.FullName);
                            continue;
                        }
                    }
                    else
                    {
                        // the string looks like a type designation
                        Type type;
                        try
                        {
                            // throw on error and ignore case
                            type = assembly.GetType(arg, true, true);
                        }
                        catch (Exception e)
                        {
                            LogLoadException(e, true, arg);
                            continue;
                        }
                        Debug.Assert(type != null);

                        GenerateType(type);
                    }
                }
            }
        }

        private static void GenerateMethod(MethodInfo method)
        {
            NativeSignature native_sig;

            Debug.WriteLine(method.DeclaringType.FullName + "::" + method.Name);

            if (NativeSignature.IsPInvoke(method))
            {
                // this is a P/Invoke method
                native_sig = NativeSignature.FromPInvokeSignature(
                    method,
                    options.TargetPlatformAnsi,
                    options.TargetPlatform64Bit);
            }
            else if (NativeSignature.IsRCWMethod(method))
            {
                // this is an RCW method
                native_sig = NativeSignature.FromComInteropSignature(
                    method,
                    options.TargetPlatformAnsi,
                    options.TargetPlatform64Bit);
            }
            else
            {
                ConsoleErrors.ERROR_MethodIsNotInterop.PrintTo(logPrinter, method.Name, method.DeclaringType.FullName);
                return;
            }

            PrintNativeSignature(native_sig);
        }

        private static void GenerateDelegate(Type delegateType)
        {
            Debug.WriteLine(delegateType.FullName);

            NativeSignature native_sig = NativeSignature.FromDelegateType(
                    delegateType,
                    options.TargetPlatformAnsi,
                    options.TargetPlatform64Bit);

            PrintNativeSignature(native_sig);
        }

        private static void GenerateType(Type type)
        {
            if (typeof(Delegate).IsAssignableFrom(type))
            {
                GenerateDelegate(type);
            }
            else
            {
                foreach (MethodInfo mi in type.GetMethods(methodBindingFlags))
                {
                    if (NativeSignature.IsPInvoke(mi) || NativeSignature.IsRCWMethod(mi))
                    {
                        GenerateMethod(mi);
                    }
                }
            }
        }

        private static void PrintNativeSignature(NativeSignature nativeSig)
        {
            PrintFlags p_flags = PrintFlags.None;
            if (options.UsePlainCDataTypes) p_flags |= PrintFlags.UsePlainC;
            if (options.PrintMarshalDirection) p_flags |= PrintFlags.PrintMarshalDirection;

            LogMemoryPrinter log_mem_printer = new LogMemoryPrinter();

            // print definitions first
            if (!options.SuppressTypeDefinitions)
            {
                foreach (NativeTypeDefinition def in nativeSig.GetDefinitions())
                {
                    def.PrintTo(codePrinter, log_mem_printer, p_flags);
                    codePrinter.PrintLn();
                    codePrinter.PrintLn();
                }
            }

            // and then the method signature
            nativeSig.PrintTo(codePrinter, log_mem_printer, p_flags);
            codePrinter.PrintLn();
            codePrinter.PrintLn();

            // flush the log_mem_printer to the real log printer
            if (!options.SuppressMessages)
            {
                log_mem_printer.ReplayTo(logPrinter);
                logPrinter.Separate();
            }
        }

        /// <summary>
        /// The assembly being reflected.
        /// </summary>
        private static Assembly assembly;

        private static ICodePrinter codePrinter;
        private static ILogPrinter logPrinter;

        private static CommandLineOptions options;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            options = new CommandLineOptions(args);

            // code will go to standard output
            if (options.UseColorConsoleOutput)
            {
                ConsolePrinter console_printer = new ConsolePrinter();
                codePrinter = console_printer;
                logPrinter = console_printer; // ConsolePrinter writes log to stderr
            }
            else
            {
                codePrinter = new TextWriterPrinter(Console.Out);
                logPrinter = new TextWriterPrinter(Console.Error);
            }

            try
            {
                string assembly_path = options.AssemblyFilePath;
                Debug.Assert(!String.IsNullOrEmpty(assembly_path));

                // load the assembly
                try
                {
                    assembly = Assembly.Load(assembly_path);
                }
                catch (Exception)
                {
                    assembly = null;
                }

                if (assembly == null)
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(assembly_path);
                    }
                    catch (Exception e)
                    {
                        LogLoadException(e, false, options.AssemblyFilePath);
                    }
                }

                if (assembly != null)
                {
                    Generate();
                }
            }
            finally
            {
                IDisposable disp = logPrinter as IDisposable;
                if (disp != null) disp.Dispose();

                disp = codePrinter as IDisposable;
                if (disp != null) disp.Dispose();
            }

#if DEBUG
            // wait for Enter only if we run under VS
            if (Process.GetCurrentProcess().MainModule.ModuleName.Contains("vshost"))
            {
                Console.ReadLine();
            }
#endif
        }
    }
}
