// Copyright (c) Microsoft Corporation.  All rights reserved.
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using PInvoke.Test.Lib;
using Xunit;

namespace PInvoke.Test
{
    /// <summary>
    /// Test the PInvokeTestLib DLL
    /// 
    /// These tests are designed to test the ability to marshal bits back and forth.  Some extra 
    /// parser testing is done here but mainly this concentrates on verifying the types and
    /// signatures we generate can properly marshal data accross the native managed boundary
    /// </summary>
    /// <remarks></remarks>
    public class TestPInvokeTestLib
    {

        /// <summary>
        /// Call the reverse string API
        /// </summary>
        /// <remarks></remarks>
        [Fact(Skip = "Lib not building")]
        public void ReverseString1()
        {
            string result = null;
            Assert.True(NativeMethods.ReverseString("foo", result));
            Assert.Equal("oof", result);
        }

        /// <summary>
        /// Call reverse string with bad parameters
        /// </summary>
        /// <remarks></remarks>
        [Fact(Skip = "Lib not building")]
        public void ReverseString2()
        {
            StringBuilder builder = new StringBuilder();
            builder.Capacity = 5;
            Assert.False(NativeMethods.ReverseString("longlonglonglonglongstring", builder, builder.Capacity));
        }

        /// <summary>
        /// Simple bitvector test
        /// </summary>
        /// <remarks></remarks>
        [Fact(Skip = "Lib not building")]
        public void BitVector1()
        {
            BitVector1 bt = new BitVector1();
            Assert.True(NativeMethods.UpdateBitVector1Data(bt));
            Assert.Equal(Convert.ToUInt32(5), bt.m1);
            Assert.Equal(Convert.ToUInt32(42), bt.m2);
        }

        /// <summary>
        /// Data going both ways in the bitvector
        /// </summary>
        /// <remarks></remarks>
        [Fact(Skip = "Lib not building")]
        public void BitVector2()
        {
            BitVector1 bt = new BitVector1();
            bt.m1 = 5;
            bt.m2 = 3;
            Assert.True(NativeMethods.IsM1GreaterThanM2(bt));
            bt.m2 = 7;
            Assert.False(NativeMethods.IsM1GreaterThanM2(bt));
        }

        [Fact(Skip = "Lib not building")]
        public void CalculateStringLength1()
        {
            int len = 0;
            Assert.True(NativeMethods.CalculateStringLength("foo", len));
            Assert.Equal(3, len);
        }

        [Fact(Skip = "Lib not building")]
        public void S1FakeConstructor_1()
        {
            s1 s1 = new s1();
            Assert.True(NativeMethods.s1FakeConstructor(42, 3.5, s1));
            Assert.Equal(42, s1.m1);
            Assert.Equal(3.5, s1.m2);
        }

        [Fact(Skip = "Lib not building")]
        public void S1FakeConstructor2_1()
        {
            s1 s1 = NativeMethods.s1FakeConstructor2(42, 3.5);
            Assert.Equal(42, s1.m1);
            Assert.Equal(3.5, s1.m2);
        }

        [Fact(Skip = "Lib not building")]
        public void S2FakeConstructor()
        {
            s2 s2 = new s2();
            Assert.True(NativeMethods.s2FakeConstructor(5, "foo", s2));
            Assert.Equal(5, s2.m1);
            Assert.Equal("foo", s2.m2);
        }

        [Fact(Skip = "Lib not building")]
        public void Enume1Values()
        {
            Assert.Equal(0, Convert.ToInt32(e1.v1));
            Assert.Equal(NativeConstants.VALUE_CONSTANT_1, Convert.ToInt32(e1.v2));
        }

        [Fact(Skip = "Lib not building")]
        public void CopyM1ToM2()
        {
            s3 s3 = new s3();
            s3.m1 = new int[] {
            1,
            2,
            3,
            4
        };
            s3.m2 = new double[5];
            Assert.True(NativeMethods.CopyM1ToM2(s3));
            Assert.Equal(Convert.ToDouble(1), s3.m2(0));
            Assert.Equal(Convert.ToDouble(2), s3.m2(1));
        }

        public struct TempStruct
        {
            public uint m1;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public bool m2;
            public uint m3;
        }

        /// <summary>
        /// Use a struct with multiple bitvectors that are not directly beside 
        /// each other
        /// </summary>
        /// <remarks></remarks>
        [Fact(Skip = "Lib not building")]
        public void MultiBitVector()
        {
            //Dim b As TempStruct = NativeMethods.CreateBitVector2(1, True, 2)
            //Assert.Equal(CUInt(1), b.m1)
            //Assert.True(b.m2)
            //Assert.Equal(CUInt(2), b.m2)
        }

        [Fact(Skip = "Lib not building")]
        public void SumArray()
        {
            int[] arr = new int[4];
            arr(0) = 1;
            arr(1) = 2;
            arr(2) = 3;
            arr(3) = 15;
            int sum = 0;
            Assert.True(NativeMethods.SumArray(arr, sum));
            Assert.Equal(21, sum);
        }

        [Fact(Skip = "Lib not building")]
        public void SumArray2()
        {
            int[] arr = new int[4];
            arr(0) = 1;
            arr(1) = 2;
            arr(2) = 3;
            arr(3) = 15;
            int sum = 0;
            Assert.True(NativeMethods.SumArray2(arr, arr.Length, sum));
            Assert.Equal(21, sum);
        }

        [Fact(Skip = "Lib not building")]
        public void S4Add()
        {
            s4 s = new s4();
            byte[] d = new byte[5];
            s.m1 = d;
            s.m1(0) = 1;
            s.m1(1) = 2;
            s.m1(2) = 3;
            s.m1(3) = 4;
            Assert.Equal(10, NativeMethods.s4Add(s));

        }

        [Fact(Skip = "Lib not building")]
        public void GetVeryLongString()
        {
            string b = null;
            NativeMethods.GetVeryLongString(b);
            Assert.True(b.StartsWith("012012"));
        }

        [Fact(Skip = "Lib not building")]
        public void GetVeryLongString2()
        {
            string b = null;
            NativeMethods.GetVeryLongString2(b);
            Assert.True(b.StartsWith("012012"));
        }

        [Fact(Skip = "Lib not building")]
        public void GetPointerPointerToChar()
        {
            IntPtr p = IntPtr.Zero;
            Assert.True(NativeMethods.GetPointerPointerToChar('f', p));
            object o = Marshal.PtrToStructure(p, typeof(char));
            char c = (char)o;
            Assert.Equal('f', c);
        }

        [Fact(Skip = "Lib not building")]
        public void CopyDecimalToPointer()
        {
            decimal p1 = new decimal(42);
            decimal p2 = new decimal(0);
            Assert.NotEqual(p1, p2);
            Assert.True(NativeMethods.CopyDecimalToPoiner(p1, p2));
            Assert.Equal(p1, p2);
        }

        [Fact(Skip = "Lib not building")]
        public void CopyDecimalToReturn()
        {
            decimal d1 = new decimal(42);
            decimal d2 = NativeMethods.CopyDecimalToReturn(d1);
            Assert.Equal(d1, d2);
        }

        [Fact(Skip = "Lib not building")]
        public void CopyDecimalPointerToPointer()
        {
            decimal d1 = new decimal(42);
            decimal d2 = new decimal(5);
            Assert.NotEqual(d1, d2);
            Assert.True(NativeMethods.CopyDecimalPointerToPointer(d1, d2));
            Assert.Equal(d1, d2);
        }

        [Fact(Skip = "Lib not building")]
        public void CopyCurrencyToPointer()
        {
            decimal d1 = new decimal(42);
            decimal d2 = new decimal(5);
            Assert.NotEqual(d1, d2);
            Assert.True(NativeMethods.CopyCurrencyToPointer(d1, d2));
            Assert.Equal(d1, d2);
        }

        [Fact(Skip = "Lib not building")]
        public void CopyBstrToNormalStr()
        {
            string result = null;
            Assert.True(NativeMethods.CopyBstrToNoramlStr("foo", result));
            Assert.Equal("foo", result);
        }

        [Fact(Skip = "Lib not building")]
        public void CopyToBstr()
        {
            string result = null;
            Assert.True(NativeMethods.CopyToBstr("bar", result));
            Assert.Equal("bar", result);
        }

        [Fact(Skip = "Lib not building")]
        public void CopyBothToBstr()
        {
            string result = null;
            Assert.True(NativeMethods.CopyBothToBstr("foo", "bar", result));
            Assert.Equal("foobar", result);
        }

        [Fact(Skip = "Lib not building")]
        public void CopyBstrToBstr()
        {
            string result = null;
            Assert.True(NativeMethods.CopyBstrToBstr("foo", result));
            Assert.Equal("foo", result);
        }

        [Fact(Skip = "Lib not building")]
        public void CopyNormalStrToBstrRet()
        {
            string result = NativeMethods.CopyNormalStrToBstrRet("str5");
            Assert.Equal("str5", result);
        }

        [Fact(Skip = "Lib not building")]
        public void CreateBasicOpaque()
        {
            IntPtr p = NativeMethods.CreateBasicOpaque();
            Assert.True(NativeMethods.VerifyBasicOpaque(p));
        }

        [Fact(Skip = "Lib not building")]
        public void VerifyBasicOpaque()
        {
            Assert.False(NativeMethods.VerifyBasicOpaque(IntPtr.Zero));
        }

        [Fact(Skip = "Lib not building")]
        public void GetFunctionPointerReturningInt()
        {
            pFunctionPointerReturningInt p = NativeMethods.GetFunctionPointerReturningInt();
            Assert.Equal(42, p());
        }

        public int AreResultAndValueEqualImpl()
        {
            return 56;
        }

        [Fact(Skip = "Lib not building")]
        public void AreResultAndValueEqual()
        {
            pFunctionPointerReturningInt p = AreResultAndValueEqualImpl;
            Assert.True(NativeMethods.AreResultAndValueEqual(p, 56));
            Assert.False(NativeMethods.AreResultAndValueEqual(p, 42));
        }

        [Fact(Skip = "Lib not building")]
        public void GetAStructWithASimpleFunctionPointer()
        {
            structWithFunctionPointer s = new structWithFunctionPointer();
            NativeMethods.GetAStructWithASimpleFunctionPointer(3, s);
            Assert.Equal(3, s.m1);
            Assert.Equal(5, s.AnonymousMember1(2, 3));
        }

        [Fact(Skip = "Lib not building")]
        public void MultiplyWithCDecl()
        {
            Assert.Equal(30, NativeMethods.MultiplyWithCDecl(5, 6));
        }

        [Fact(Skip = "Lib not building")]
        public void SimpleClass()
        {
            simpleClass c = new simpleClass();
            c.m1 = 42;
            c.m2 = 54;
            Assert.Equal(42, NativeMethods.GetSimpleClassM1(c));
            Assert.Equal(54, NativeMethods.GetSimpleClassM2(c));
        }

        [Fact(Skip = "Lib not building")]
        public void StringInStruct()
        {
            stringInStruct s = new stringInStruct();
            s.m1 = "foo";
            Assert.True(NativeMethods.VerifyStringInStructM1(s, "foo"));
            Assert.False(NativeMethods.VerifyStringInStructM1(s, "false"));
        }

        [Fact(Skip = "Lib not building")]
        public void StringDiffTypeInStruct()
        {
            structWithDiffStringTypes s = new structWithDiffStringTypes();
            NativeMethods.PopulateStructWithDiffStringTypes(s, "foo", "bar");
            Assert.Equal("foo", s.m1);
            Assert.Equal("bar", s.m2);
        }

    }
}
