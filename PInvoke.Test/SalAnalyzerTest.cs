// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using PInvoke;
using PInvoke.Transform;
using Xunit;

namespace PInvoke.Test
{
    public class SalAnalyzerTest
    {

        ///<summary>
        ///A test for IsIn()
        ///</summary>
        [Fact()]
        public void IsInTest()
        {
            NativeSalAttribute sal = new NativeSalAttribute(SalEntryType.Pre, SalEntryType.Valid, SalEntryType.Pre, SalEntryType.Deref, SalEntryType.ReadOnly);
            SalAnalyzer target = new SalAnalyzer(sal);

            Assert.True(target.IsIn());
        }

        [Fact()]
        public void IsInTest2()
        {
            NativeSalAttribute sal = new NativeSalAttribute(SalEntryType.Pre, SalEntryType.Valid, SalEntryType.Pre, SalEntryType.Deref, SalEntryType.NotReadOnly);
            SalAnalyzer target = new SalAnalyzer(sal);

            Assert.False(target.IsIn());
        }

        [Fact()]
        public void ValidIn1()
        {
            NativeSalAttribute sal = new NativeSalAttribute(SalEntryType.Pre, SalEntryType.Valid);
            SalAnalyzer analyzer = new SalAnalyzer(sal);
            Assert.True(analyzer.IsValidIn());
            Assert.True(analyzer.IsValidInOnly());
            Assert.False(analyzer.IsValidOut());
            Assert.False(analyzer.IsValidOutOnly);
            Assert.False(analyzer.IsValidInOut);
        }


        [Fact()]
        public void ValidIn2()
        {
            NativeSalAttribute sal = new NativeSalAttribute(SalEntryType.Pre, SalEntryType.Valid, SalEntryType.Post, SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly);
            SalAnalyzer analyzer = new SalAnalyzer(sal);
            Assert.True(analyzer.IsValidIn());
            Assert.False(analyzer.IsValidInOnly());
            Assert.True(analyzer.IsValidOut());
            Assert.False(analyzer.IsValidOutOnly);
            Assert.True(analyzer.IsValidInOut);
        }

        [Fact()]
        public void ValidOut1()
        {
            NativeSalAttribute sal = new NativeSalAttribute(SalEntryType.Post, SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly);
            SalAnalyzer analyzer = new SalAnalyzer(sal);
            Assert.False(analyzer.IsValidIn());
            Assert.False(analyzer.IsValidInOnly());
            Assert.True(analyzer.IsValidOut());
            Assert.True(analyzer.IsValidOutOnly);
            Assert.False(analyzer.IsValidInOut);
        }

        [Fact()]
        public void ValidOut2()
        {
            NativeSalAttribute sal = new NativeSalAttribute(SalEntryType.Pre, SalEntryType.Valid, SalEntryType.Post, SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly);
            SalAnalyzer analyzer = new SalAnalyzer(sal);
            Assert.True(analyzer.IsValidIn());
            Assert.False(analyzer.IsValidInOnly());
            Assert.True(analyzer.IsValidOut());
            Assert.False(analyzer.IsValidOutOnly);
            Assert.True(analyzer.IsValidInOut);
        }
    }
}
