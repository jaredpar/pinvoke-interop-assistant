// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using static PInvoke.Contract;

namespace PInvoke
{

    /// <summary>
    /// Used to find procedures in a list of DLL's
    /// </summary>
    /// <remarks></remarks>
    public class ProcedureFinder : IDisposable
    {

        public static IEnumerable<string> DefaultDllList
        {
            get
            {
                return new List<string>
                {
                    "kernel32.dll",
                    "ntdll.dll",
                    "user32.dll",
                    "advapi32.dll",
                    "gdi32.dll",
                    "crypt32.dll",
                    "cryptnet.dll",
                    "opengl32.dll",
                    "ws2_32.dll",
                    "shell32.dll",
                    "mpr.dll",
                    "mswsock.dll",
                    "winmm.dll",
                    "imm32.dll",
                    "comdlg32.dll",
                    "rpcns4.dll",
                    "rpcrt4.dll",
                    "urlmon.dll"
                };
            }
        }

        private Dictionary<string, IntPtr> dllMap = new Dictionary<string, IntPtr>();

        private bool loaded = false;
        /// <summary>
        /// List of dll's to look for
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<string> DllNames
        {
            get { return dllMap.Keys; }
        }

        public ProcedureFinder() : this(DefaultDllList)
        {
        }

        public ProcedureFinder(IEnumerable<string> list)
        {
            foreach (string name in list)
            {
                AddDll(name);
            }
        }

        public void Dispose()
        {
            foreach (IntPtr ptr in dllMap.Values)
            {
                NativeMethods.FreeLibrary(ptr);
            }

        }

        public void AddDll(string dllName)
        {
            if (dllName == null)
            {
                throw new ArgumentNullException("dllName");
            }

            dllMap.Add(dllName, IntPtr.Zero);
            loaded = false;
        }

        public bool TryFindDllNameExact(string procName, out string dllName)
        {
            if (procName == null)
            {
                throw new ArgumentNullException("procName");
            }

            return TryFindDllNameImpl(procName, out dllName);
        }

        public bool TryFindDllName(string procName, out string dllName)
        {
            dllName = null;
            if (procName == null)
            {
                throw new ArgumentNullException("procName");
            }

            if (!TryFindDllNameImpl(procName, out dllName) && !TryFindDllNameImpl(procName + "W", out dllName))
            {
                return false;
            }

            return true;
        }

        private bool TryFindDllNameImpl(string procName, out string dllName)
        {
            ThrowIfNull(procName);

            if (!loaded)
            {
                LoadLibraryList();
            }

            foreach (KeyValuePair<string, IntPtr> pair in dllMap)
            {
                if (pair.Value == IntPtr.Zero)
                {
                    continue;
                }

                IntPtr procPtr = NativeMethods.GetProcAddress(pair.Value, procName);
                if (procPtr != IntPtr.Zero)
                {
                    dllName = Path.GetFileName(pair.Key);
                    return true;
                }
            }

            dllName = null;
            return false;
        }


        private void LoadLibraryList()
        {
            var list = new List<string>(dllMap.Keys);
            foreach (var name in list)
            {
                var ptr = dllMap[name];
                if (ptr == IntPtr.Zero)
                {
                    ptr = NativeMethods.LoadLibraryEx(name, IntPtr.Zero, 0u);
                    dllMap[name] = ptr;
                }
            }

            loaded = true;
        }

    }
}
