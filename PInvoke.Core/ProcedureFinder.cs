// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

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
                List<string> list = new List<string>();
                list.Add("kernel32.dll");
                list.Add("ntdll.dll");
                list.Add("user32.dll");
                list.Add("advapi32.dll");
                list.Add("gdi32.dll");
                list.Add("crypt32.dll");
                list.Add("cryptnet.dll");
                list.Add("opengl32.dll");
                list.Add("ws2_32.dll");
                list.Add("shell32.dll");
                list.Add("mpr.dll");
                list.Add("mswsock.dll");
                list.Add("winmm.dll");
                list.Add("imm32.dll");
                list.Add("comdlg32.dll");
                list.Add("rpcns4.dll");
                list.Add("rpcrt4.dll");
                list.Add("urlmon.dll");
                return list;
            }
        }

        private Dictionary<string, IntPtr> _dllMap = new Dictionary<string, IntPtr>();

        private bool _loaded = false;
        /// <summary>
        /// List of dll's to look for
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<string> DllNames
        {
            get { return _dllMap.Keys; }
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
            foreach (IntPtr ptr in _dllMap.Values)
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

            _dllMap.Add(dllName, IntPtr.Zero);
            _loaded = false;
        }

        public bool TryFindDllNameExact(string procName, ref string dllName)
        {
            if (procName == null)
            {
                throw new ArgumentNullException("procName");
            }

            return TryFindDllNameImpl(procName, ref dllName);
        }

        public bool TryFindDllName(string procName, ref string dllName)
        {
            if (procName == null)
            {
                throw new ArgumentNullException("procName");
            }

            if (!TryFindDllNameImpl(procName, ref dllName) && !TryFindDllNameImpl(procName + "W", ref dllName))
            {
                return false;
            }

            return true;
        }

        private bool TryFindDllNameImpl(string procName, ref string dllName)
        {
            ThrowIfNull(procName);

            if (!_loaded)
            {
                LoadLibraryList();
            }

            foreach (KeyValuePair<string, IntPtr> pair in _dllMap)
            {
                if (pair.Value == IntPtr.Zero)
                {
                    continue;
                }

                IntPtr procPtr = NativeMethods.GetProcAddress(pair.Value, procName);
                if (procPtr != IntPtr.Zero)
                {
                    dllName = IO.Path.GetFileName(pair.Key);
                    return true;
                }
            }

            return false;
        }


        private void LoadLibraryList()
        {
            List<string> list = new List<string>(_dllMap.Keys);
            foreach (string name in list)
            {
                IntPtr ptr = _dllMap(name);
                if (ptr == IntPtr.Zero)
                {
                    ptr = NativeMethods.LoadLibraryEx(name, IntPtr.Zero, 0uL);
                    _dllMap(name) = ptr;
                }
            }

            _loaded = true;
        }

    }
}
