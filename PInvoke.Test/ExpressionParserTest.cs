// Copyright (c) Microsoft Corporation.  All rights reserved.

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
    ///This is a test class for PInvoke.Parser.ExpressionParser and is intended
    ///to contain all PInvoke.Parser.ExpressionParser Unit Tests
    ///</summary>
    public class ExpressionParserTest
    {

        ///<summary>
        ///A test for TryParse(ByVal String, ByRef PInvoke.Parser.ExpressionNode)
        ///</summary>
        [Fact()]
        public void Parse1()
        {
            ExpressionParser parser = new ExpressionParser();
            ExpressionNode node = parser.Parse("1+1");
            Assert.Equal(node.DisplayString, "+ (Left: 1)(Right: 1)");
        }

        [Fact()]
        public void Parse2()
        {
            ExpressionParser parser = new ExpressionParser();
            ExpressionNode node = parser.Parse("'c'");
            Assert.Equal(node.DisplayString, "'c'");
        }

        [Fact()]
        public void Parse3()
        {
            ExpressionParser parser = new ExpressionParser();
            ExpressionNode node = parser.Parse("'c'+2");
            Assert.Equal(node.DisplayString, "+ (Left: 'c')(Right: 2)");
        }

        /// <summary>
        /// Basic math operators
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void ParseMathOperations()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("1+1");
            Assert.Equal(node.DisplayString, "+ (Left: 1)(Right: 1)");
            node = p.Parse("1-1");
            Assert.Equal(node.DisplayString, "- (Left: 1)(Right: 1)");
            node = p.Parse("1/1");
            Assert.Equal(node.DisplayString, "/ (Left: 1)(Right: 1)");
            node = p.Parse("1%1");
            Assert.Equal(node.DisplayString, "% (Left: 1)(Right: 1)");

        }

        [Fact()]
        public void ParseBooleanOperations()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("1&&1");
            Assert.Equal(node.DisplayString, "&& (Left: 1)(Right: 1)");
            node = p.Parse("1||1");
            Assert.Equal(node.DisplayString, "|| (Left: 1)(Right: 1)");
        }


        [Fact()]
        public void ParseBitwiseOperations()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("1|1");
            Assert.Equal(node.DisplayString, "| (Left: 1)(Right: 1)");
            node = p.Parse("1&1");
            Assert.Equal(node.DisplayString, "& (Left: 1)(Right: 1)");
        }

        [Fact()]
        public void Paren1()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("(1)");
            Assert.Equal(node.DisplayString, "1");
            node = p.Parse("(     1)");
            Assert.Equal(node.DisplayString, "1");
        }

        [Fact()]
        public void Paren2()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("1+(2+3)");
            Assert.Equal(node.DisplayString, "+ (Left: 1)(Right: + (Left: 2)(Right: 3))");
            node = p.Parse("(1+2)+3");
            Assert.Equal(node.DisplayString, "+ (Left: + (Left: 1)(Right: 2))(Right: 3)");
        }

        [Fact()]
        public void CallExpr1()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("defined(foo)");
            Assert.Equal("defined (Left: foo)", node.DisplayString);
        }

        [Fact()]
        public void CallExpr2()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("defined(foo, bar)");
            Assert.Equal("defined (Left: foo)(Right: , (Left: bar))", node.DisplayString);
        }

        [Fact()]
        public void CallExpr3()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("defined(foo, 1+1)");
            Assert.Equal(node.DisplayString, "defined (Left: foo)(Right: , (Left: + (Left: 1)(Right: 1)))");
        }

        [Fact()]
        public void Shift1()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("1>>2");
            Assert.Equal(node.DisplayString, ">> (Left: 1)(Right: 2)");
        }

        [Fact()]
        public void Shift2()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("1<<2");
            Assert.Equal(node.DisplayString, "<< (Left: 1)(Right: 2)");
        }

        [Fact()]
        public void Cast1()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("(FOO)1");
            Assert.Equal(node.DisplayString, "FOO (Left: 1)");
        }

        [Fact()]
        public void Cast2()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("(BAR)(42)");
            Assert.Equal(node.DisplayString, "BAR (Left: 42)");
        }

        [Fact()]
        public void Cast3()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = default(ExpressionNode);

            node = p.Parse("(FOO)(BAR)1");
            Assert.Equal(node.DisplayString, "FOO (Left: BAR (Left: 1))");
        }

        [Fact()]
        public void Complex1()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = p.Parse("((WORD)((DWORD_PTR)(l) >> 16))");
            Assert.Equal(node.DisplayString, "WORD (Left: DWORD_PTR (Left: >> (Left: l)(Right: 16)))");
        }

        [Fact()]
        public void Negative1()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = p.Parse("-1");
            Assert.Equal(node.DisplayString, "- (Left: 1)");
        }

        [Fact()]
        public void Negative2()
        {
            ExpressionParser p = new ExpressionParser();
            ExpressionNode node = p.Parse("-0.1F");
            Assert.Equal(node.DisplayString, "- (Left: 0.1F)");
        }
    }
}