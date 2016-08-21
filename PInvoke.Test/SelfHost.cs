// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using PInvoke.Test.Generated;
using Xunit;

namespace PInvoke.Test
{
    public class SelfHost
    {

        public SelfHost()
        {
            _nameList.Clear();
        }


        private List<string> _nameList = new List<string>();
        private int CaptureWindowNameCb(IntPtr intPtr, IntPtr param2)
        {
            StringBuilder builder = new StringBuilder(256);
            if (0 != NativeMethods.GetWindowTextW(intPtr, builder, builder.Capacity))
            {
                _nameList.Add(builder.ToString());
            }

            return 1;
        }

        private int CaptureWindowNameCb2(IntPtr intPtr, IntPtr param2)
        {
            string name = string.Empty;
            if (0 != NativeMethods.GetWindowTextW(intPtr, ref name))
            {
                _nameList.Add(name);
            }

            return 1;
        }

        /// <summary>
        /// Test the FindFirstFile API
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void FindFirstFile()
        {
            string sys32Path = Environment.GetFolderPath(Environment.SpecialFolder.System);
            WIN32_FIND_DATAW data = new WIN32_FIND_DATAW();
            IntPtr handle = NativeMethods.FindFirstFileW(Path.Combine(sys32Path, "n") + "*", out data);

            Assert.NotEqual(handle, IntPtr.Zero);
            List<string> list = new List<string>();
            list.Add(data.cFileName);
            while (NativeMethods.FindNextFileW(handle, out data))
            {
                list.Add(data.cFileName);
            }

            Assert.True(list.Count > 3);
            NativeMethods.FindClose(handle);
        }

        /// <summary>
        /// Enumerate the top level windows and collect the names
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void EnumWindows1()
        {
            NativeMethods.EnumWindows(this.CaptureWindowNameCb, IntPtr.Zero);
            Assert.True(_nameList.Count > 0);
        }

        /// <summary>
        /// Same as the other but using the cleaned up method 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void EnumWindows2()
        {
            NativeMethods.EnumWindows(this.CaptureWindowNameCb2, IntPtr.Zero);
            Assert.True(_nameList.Count > 0);
        }

        //<Fact>
        public void GetComptureName1()
        {
            StringBuilder builder = new StringBuilder(256);
            uint count = Convert.ToUInt32(builder.Capacity);
            Assert.True(NativeMethods.GetComputerNameW(builder, ref count));
            Assert.Equal(Environment.MachineName, builder.ToString(), true);
        }

        //<Fact>
        public void GetComputerName2()
        {
            string name = null;
            Assert.True(NativeMethods.GetComputerNameW(ref name));
            Assert.Equal(Environment.MachineName, name, true);
        }

        //<Fact>
        public void CreateWellKnownSid2()
        {
            PInvokePointer ptr = null;
            Assert.True(NativeMethods.CreateWellKnownSid(WELL_KNOWN_SID_TYPE.WinBuiltinAdministratorsSid, IntPtr.Zero, ref ptr));
            ptr.Free();
        }

        /// <summary>
        /// Test a couple of the properties in a bitvector type.  This is a simple type with single bit values
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void BitVector1()
        {
            DCB d = new DCB();

            d.fBinary = 1;
            Assert.Equal(1u, d.fBinary);

            d.fParity = 1;
            Assert.Equal(1u, d.fParity);

            d.fOutxCtsFlow = 1;
            Assert.Equal(1u, d.fOutxCtsFlow);

            d.fOutxDsrFlow = 1;
            Assert.Equal(1u, d.fOutxDsrFlow);

            d.fDtrControl = 1;
            Assert.Equal(1u, d.fDtrControl);

            d.fDsrSensitivity = 1;
            Assert.Equal(1u, d.fDsrSensitivity);

            d.fOutX = 1;
            Assert.Equal(1u, d.fOutX);
        }

        /// <summary>
        /// Use a multy bit bitvector
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void BitVector2()
        {
            DCB d = new DCB();

            d.fDtrControl = 2;
            Assert.Equal(2u, d.fDtrControl);
        }

        /// <summary>
        /// Test a basic union structure
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Union1()
        {
            IMAGE_LINENUMBER l = new IMAGE_LINENUMBER();
            l.Type.VirtualAddress = 5;
            Assert.Equal(5u, l.Type.VirtualAddress);
            Assert.Equal(5u, l.Type.SymbolTableIndex);
        }

        [Fact()]
        public void GetEnvironmentVariable1()
        {
            string value = null;
            NativeMethods.GetEnvironmentVariableW("USERPROFILE", ref value);
            Assert.False(string.IsNullOrEmpty(value));
        }

        /// <summary>
        /// This is a __cdecl method
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Atoi()
        {
            Assert.Equal(5, NativeMethods.atoi("5"));
        }

    }
}
