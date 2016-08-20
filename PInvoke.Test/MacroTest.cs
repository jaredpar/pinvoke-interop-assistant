// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using PInvoke.Parser;
using Xunit;
namespace PInvoke.Test
{

    ///<summary>
    ///This is a test class for PInvoke.Parser.Macro and is intended
    ///to contain all PInvoke.Parser.Macro Unit Tests
    ///</summary>
    public class MacroTest
    {

        [Fact()]
        public void CreateMethod1()
        {
            MethodMacro m = null;
            Assert.True(MethodMacro.TryCreateFromDeclaration("m1", "(x) x + 2", m));
            Assert.Equal("m1", m.Name);
        }

        [Fact()]
        public void CreateMethod2()
        {
            MethodMacro m = null;
            Assert.False(MethodMacro.TryCreateFromDeclaration("m1", "2", m));
        }

        /// <summary>
        /// Make sure that whitespace is expactly preserved
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void CreateMethod3()
        {
            MethodMacro m = null;
            string sig = "(x) \"foo\"#x";
            Assert.True(MethodMacro.TryCreateFromDeclaration("m1", sig, m));
            Assert.Equal(sig, m.MethodSignature);
        }

        [Fact()]
        public void CreateMethod4()
        {
            MethodMacro m = null;
            string sig = "(x) \"foo\"#x           +    5";
            Assert.True(MethodMacro.TryCreateFromDeclaration("m1", sig, m));
            Assert.Equal(sig, m.MethodSignature);
        }

    }
}