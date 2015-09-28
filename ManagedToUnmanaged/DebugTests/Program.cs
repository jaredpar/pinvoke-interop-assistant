/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SignatureGenerator
{
    class DebugTests
    {
        private static bool runningOn64Bit = (IntPtr.Size == 8);

        private static string[] testAssemblies = new string[]
                {
                    "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    "System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    "System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    "System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
                };

        private static void LayoutUnmanagedSizeTest()
        {
            Console.WriteLine("running ValueTypeUnmanagedSizeTest...");

            foreach (string assemblyString in testAssemblies)
            {
                Assembly assembly = Assembly.Load(assemblyString);

                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.IsAutoClass &&
                        type != typeof(void) &&
                        type != typeof(ArrayWithOffset) &&
                        type != typeof(HandleRef))
                    {
                        int size_reported_by_clr;

                        try
                        {
                            // will fail when CLR thinks the type has no layout
                            size_reported_by_clr = Marshal.SizeOf(type);
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        MarshalFlags flags = MarshalFlags.AnsiStrings | MarshalFlags.StructField;
                        if (runningOn64Bit) flags |= MarshalFlags.Platform64Bit;

                        NativeType native_type = NativeType.FromClrType(type, flags);

                        if (native_type.TypeSize != size_reported_by_clr)
                        {
                            Console.WriteLine("* size mismatch for {0} (CLR {1}, tool {2})",
                                type.FullName,
                                size_reported_by_clr,
                                native_type.TypeSize);
                        }
                    }
                }
            }
        }

        private static void LayoutUnmanagedPrintoutTest()
        {
            string file_name = "_test.cpp";

            Console.WriteLine("running ValueTypeUnmanagedPrintoutTest...");

            // we'll generate a C++ source file with all the unmanaged structures
            // - it should compile
            // - the C++ sizeof operator should return the intended size
            // - the C++ offsetof macro should return the same offsets as Marshal.OffsetOf

            Console.WriteLine("  generating definitions...");

            using (TextWriterCodePrinter printer = new TextWriterCodePrinter(new StreamWriter(file_name)))
            {
                // prepare a memory printer to print main to
                CodeMemoryPrinter main_printer = new CodeMemoryPrinter();

                main_printer.PrintLn(OutputType.Other, "#define VERIFY(cond) if (!(cond)) printf(\"* \\\"%s\\\" violated\\n\", #cond)");

                main_printer.PrintLn(OutputType.Other, "int main()");
                main_printer.Print(OutputType.Other, "{");
                main_printer.Indent();
                main_printer.PrintLn();

                LogMemoryPrinter log_printer = new LogMemoryPrinter();

                printer.PrintLn(OutputType.Other, "#include <stddef.h>");
                printer.PrintLn(OutputType.Other, "#include <stdio.h>");
                printer.PrintLn(OutputType.Other, "#include <windows.h>");
                printer.PrintLn(OutputType.Other, "#include <oaidl.h>");

                printer.PrintLn();

                int counter = 0;
                foreach (string assemblyString in testAssemblies)
                {
                    Assembly assembly = Assembly.Load(assemblyString);

                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!type.IsAutoClass)
                        {
                            int size_reported_by_clr;

                            try
                            {
                                // will fail when CLR thinks the type has no layout
                                size_reported_by_clr = Marshal.SizeOf(type);
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            MarshalFlags flags = MarshalFlags.AnsiStrings | MarshalFlags.StructField;
                            if (runningOn64Bit) flags |= MarshalFlags.Platform64Bit;

                            NativeType native_type = NativeType.FromClrType(type, flags);
                            DefinedNativeType def_native_type = native_type as DefinedNativeType;

                            if (def_native_type != null)
                            {
                                // dump the "closure" of this definition to a namespace
                                string ns_name = String.Format("_CLR_{0}", counter++);

                                printer.PrintLn(OutputType.Other, "namespace " + ns_name);
                                printer.Print(OutputType.Other, "{");
                                printer.Indent();
                                printer.PrintLn();

                                NativeTypeDefinitionSet set = new NativeTypeDefinitionSet();
                                set.Add(def_native_type.Definition);
                                def_native_type.Definition.GetDefinitionsRecursive(set, def_native_type.Definition);

                                // enumerate the closure (will respect dependencies and return definitions
                                // in a correct order introducing forward declarations if necessary)
                                foreach (NativeTypeDefinition def in set)
                                {
                                    def.PrintTo(printer, log_printer,
                                        //PrintFlags.UsePlainC |
                                        PrintFlags.UseDefinedComInterfaces |
                                        PrintFlags.MangleEnumFields);

                                    printer.PrintLn();
                                    printer.PrintLn();
                                }

                                printer.Unindent();
                                printer.PrintLn();
                                printer.PrintLn(OutputType.Other, "};");

                                // add tests to main()
                                main_printer.PrintLn(OutputType.Other, String.Format(
                                    "VERIFY(sizeof({0}::{1}) == {2});",
                                    ns_name,
                                    def_native_type.Definition.Name,
                                    size_reported_by_clr));

                                StructureDefinition struct_def = def_native_type.Definition as StructureDefinition;
                                if (struct_def != null)
                                {
                                    for (int i = 0; i < struct_def.FieldCount; i++)
                                    {
                                        NativeField nf = struct_def.GetField(i);

                                        int offset;
                                        try
                                        {
                                            offset = (int)Marshal.OffsetOf(type, nf.Name);
                                        }
                                        catch (Exception)
                                        {
                                            continue;
                                        }

                                        // if we think we know the offset of this field, we will verify it
                                        main_printer.PrintLn(OutputType.Other, String.Format(
                                            "VERIFY(offsetof({0}::{1}, {2}) == {3});",
                                            ns_name,
                                            def_native_type.Definition.Name,
                                            nf.Name,
                                            offset));
                                    }
                                }
                            }
                        }
                    }
                }

                main_printer.Print(OutputType.Other, "return 0;");
                main_printer.Unindent();
                main_printer.PrintLn();
                main_printer.PrintLn(OutputType.Other, "}");

                main_printer.ReplayTo(printer);
            }

            Console.WriteLine("  compiling the generated file...");

            ProcessStartInfo start_info = new ProcessStartInfo(
                @"c:\Program Files (x86)\Microsoft Visual Studio 8\VC\bin\cl.exe", "/DUNICODE " + file_name);
            start_info.WorkingDirectory = Directory.GetCurrentDirectory();
            start_info.UseShellExecute = false;
            start_info.CreateNoWindow = true;
            start_info.RedirectStandardOutput = true;

            Process cl_proc = Process.Start(start_info);
            cl_proc.WaitForExit();

            Console.WriteLine(cl_proc.StandardOutput.ReadToEnd());

            Console.WriteLine("  running the generated executable...");

            start_info.FileName = Path.ChangeExtension(file_name, ".exe");
            start_info.Arguments = String.Empty;

            Process tst_proc = Process.Start(start_info);
            tst_proc.WaitForExit();

            Console.WriteLine(tst_proc.StandardOutput.ReadToEnd());
        }

        public static void Main()
        {
            LayoutUnmanagedSizeTest();
            LayoutUnmanagedPrintoutTest();

            Console.WriteLine("tests done");
            Console.ReadLine();
        }
    }
}
