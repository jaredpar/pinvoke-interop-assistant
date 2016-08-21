// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using PInvoke;
using Xunit;

namespace PInvoke.Test
{
    public class ProcedureFinderTest
    {

        [Fact()]
        public void Find1()
        {
            using (ProcedureFinder finder = new ProcedureFinder())
            {
                string name = null;

                Assert.True(finder.TryFindDllNameExact("GetProcAddress", out name));
                Assert.Equal("kernel32.dll", name, true);
                Assert.True(finder.TryFindDllNameExact("SendMessageW", out name));
                Assert.Equal("user32.dll", name, true);
            }
        }

        [Fact()]
        public void Find2()
        {
            using (ProcedureFinder finder = new ProcedureFinder())
            {
                string name = null;

                Assert.False(finder.TryFindDllNameExact("DoesNotExistFunc", out name));
            }
        }

        [Fact()]
        public void Find3()
        {
            using (ProcedureFinder finder = new ProcedureFinder())
            {
                string name = null;

                Assert.False(finder.TryFindDllNameExact("SendMessage", out name));
                Assert.True(finder.TryFindDllNameExact("SendMessageW", out name));
                Assert.Equal("user32.dll", name, true);
                Assert.True(finder.TryFindDllName("SendMessage", out name));
                Assert.Equal("user32.dll", name, true);
            }
        }

    }
}
