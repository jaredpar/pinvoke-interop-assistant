// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.IO;
using PInvoke.Parser;
using Xunit;
namespace PInvoke.Test
{

    public class ExpressionValueTest
    {
        private static ExpressionValue Create(object o)
        {
            Number n;
            if (Number.TryCreate(o, out n))
            { 
                return ExpressionValue.Create(n);
            }

            throw new Exception($"Don't know how to convert {o.GetType().Name}");
        }

        private static ExpressionValue Eval(BinaryOperator op, object x, object y)
        {
            var left = Create(x);
            var right = Create(y);
            ExpressionValue result;
            Assert.True(ExpressionEvaluator.TryEvaluateBinaryOperation(op, left, right, out result));
            return result;
        }

        private static bool Equals(ExpressionValue left, ExpressionValue right)
        {
            if (right.Kind == ExpressionValueKind.Single || right.Kind == ExpressionValueKind.Double)
            {
                return left.ConvertToDouble() == right.ConvertToDouble();
            }

            return left.ConvertToLong() == right.ConvertToLong();
        }

        private void Test(BinaryOperator op, object x, object y, object r)
        {
            var result = Eval(op, x, y);
            Assert.True(Equals(result, Create(r)));
        }

        public void TestPlus(object x, object y, object r)
        {
            Test(BinaryOperator.Add, x, y, r);
        }

        public void TestMinus(object x, object y, object r)
        {
            Test(BinaryOperator.Subtract, x, y, r);
        }

        public void TestDivide(object x, object y, object r)
        {
            Test(BinaryOperator.Divide, x, y, r);
        }

        public void TestMultiply(object x, object y, object r)
        {
            Test(BinaryOperator.Multiply, x, y, r);
        }

        public void TestShiftLeft(object x, int y, object r)
        {
            Test(BinaryOperator.ShiftLeft, x, y, r);
        }

        public void TestShiftRight(object x, int y, object r)
        {
            Test(BinaryOperator.ShiftRight, x, y, r);
        }

        public void TestGreaterThan(object x, object y, bool r)
        {
            Test(BinaryOperator.GreaterThan, x, y, r);
        }

        public void TestGreaterThanOrEqualsTo(object x, object y, bool r)
        {
            Test(BinaryOperator.GreaterThanOrEqualTo, x, y, r);
        }

        public void TestLessThan(object x, object y, bool r)
        {
            Test(BinaryOperator.LessThan, x, y, r);
        }

        public void TestLessThanOrEqualsTo(object x, object y, bool r)
        {
            Test(BinaryOperator.LessThanOrEqualTo, x, y, r);
        }

        public void TestNotEqualsTo(object x, object y, bool r)
        {
            Test(BinaryOperator.BooleanNotEquals, x, y, r);
        }

        public void TestEqualsTo(object x, object y, bool r)
        {
            Test(BinaryOperator.BooleanEquals, x, y, r);
        }

        [Fact()]
        public void TestInt32()
        {
            TestPlus(1, 2, 3);
            TestPlus(10, 15, 25);
            TestPlus(-1, 3, 2);
            TestMinus(15, 5, 10);
            TestDivide(15, 5, 3);
            TestMultiply(3, 2, 6);
            TestShiftLeft(2, 1, 4);
            TestShiftRight(4, 1, 2);
            TestGreaterThan(1, 2, false);
            TestGreaterThanOrEqualsTo(2, 2, true);
            TestLessThan(1, 2, true);
            TestLessThanOrEqualsTo(2, 2, true);
            TestEqualsTo(1, 1, true);
            TestEqualsTo(1, 2, false);
            TestNotEqualsTo(1, 1, false);
            TestNotEqualsTo(1, 2, true);

        }

        [Fact()]
        public void TestInt64()
        {
            TestPlus(1L, 2L, 3L);
            TestPlus(10L, 15L, 25L);
            TestPlus(-1L, 3L, 2L);
            TestMinus(15L, 5L, 10L);
            TestDivide(15L, 5L, 3L);
            TestMultiply(3L, 2L, 6L);
            TestGreaterThan(1L, 2L, false);
            TestGreaterThanOrEqualsTo(2L, 2L, true);
            TestLessThan(1L, 2L, true);
            TestLessThanOrEqualsTo(2L, 2L, true);
            TestEqualsTo(1L, 1L, true);
            TestEqualsTo(1L, 2L, false);
            TestNotEqualsTo(1L, 1L, false);
            TestNotEqualsTo(1L, 2L, true);
        }

        [Fact()]
        public void TestDouble()
        {
            TestPlus(1.0, 2.0, 3.0);
            TestPlus(10.0, 15.0, 25.0);
            TestPlus(-1.0, 3.0, 2.0);
            TestMinus(15.0, 5.0, 10.0);
            TestDivide(15.0, 5.0, 3.0);
            TestMultiply(3.0, 2.0, 6.0);
            TestGreaterThan(1.0, 2.0, false);
            TestGreaterThanOrEqualsTo(2.0, 2.0, true);
            TestLessThan(1.0, 2.0, true);
            TestLessThanOrEqualsTo(2.0, 2.0, true);
            TestEqualsTo(1.0, 1.0, true);
            TestEqualsTo(1.0, 2.0, false);
            TestNotEqualsTo(1.0, 1.0, false);
            TestNotEqualsTo(1.0, 2.0, true);
        }
    }
}
