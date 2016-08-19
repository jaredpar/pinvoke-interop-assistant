// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PInvoke
{
    static internal class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryEx([MarshalAs(UnmanagedType.LPTStr), In()] string fileName, IntPtr intPtr, UInt32 flags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 FreeLibrary(IntPtr handle);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress([In()] IntPtr dllPtr, [MarshalAs(UnmanagedType.LPStr), In()] string procName);
    }
}
