/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TestInput
{
    public class PInvokes
    {
        #region Names

        [DllImport("unmanaged.dll", EntryPoint = "CustomEntryPointName", ExactSpelling = false)]
        static extern void ManagedName1();

        [DllImport("unmanaged.dll", EntryPoint = "CustomEntryPointName", ExactSpelling = true)]
        static extern void ManagedName2();

        #endregion

        #region Calling conventions

        [DllImport("unmanaged.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void StdcallMethod(long arg);

        [DllImport("unmanaged.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void CdeclMethod(long arg);

        [DllImport("unmanaged.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void StdcallVarargMethod(long arg, __arglist);

        [DllImport("unmanaged.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void CdeclVarargMethod(long arg, __arglist);

        #endregion

        #region HRESULT & signature preserving

        [DllImport("unmanaged.dll", PreserveSig = false)]
        static extern void HresultReturningMethod1();

        [DllImport("unmanaged.dll", PreserveSig = false)]
        static extern void HresultReturningMethod2(long arg1, string arg2);

        #endregion

        #region CharSet variations

        [StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
        class AutoClass
        {
            [DllImport("unmanaged.dll", CharSet = CharSet.Ansi)]
            static extern void AutoClass_PInvokeAnsiCharSet(string s);

            [DllImport("unmanaged.dll", CharSet = CharSet.Unicode)]
            static extern void AutoClass_PInvokeUnicodeCharSet(string s);

            [DllImport("unmanaged.dll", CharSet = CharSet.Auto)]
            static extern void AutoClass_PInvokeAutoCharSet(string s);
        }

        [StructLayout(LayoutKind.Auto, CharSet = CharSet.Ansi)]
        class AnsiClass
        {
            [DllImport("unmanaged.dll", CharSet = CharSet.Ansi)]
            static extern void AnsiClass_PInvokeAnsiCharSet(string s);

            [DllImport("unmanaged.dll", CharSet = CharSet.Unicode)]
            static extern void AnsiClass_PInvokeUnicodeCharSet(string s);

            [DllImport("unmanaged.dll", CharSet = CharSet.Auto)]
            static extern void AnsiClass_PInvokeAutoCharSet(string s);
        }

        [StructLayout(LayoutKind.Auto, CharSet = CharSet.Unicode)]
        class UnicodeClass
        {
            [DllImport("unmanaged.dll", CharSet = CharSet.Ansi)]
            static extern void UnicodeClass_PInvokeAnsiCharSet(string s);

            [DllImport("unmanaged.dll", CharSet = CharSet.Unicode)]
            static extern void UnicodeClass_PInvokeUnicodeCharSet(string s);

            [DllImport("unmanaged.dll", CharSet = CharSet.Auto)]
            static extern void UnicodeClass_PInvokeAutoCharSet(string s);
        }

        #endregion

        #region Strings

        [DllImport("unmanaged.dll")]
        [return: MarshalAs(UnmanagedType.AnsiBStr)]
        static extern string AnsiBStr(
            [MarshalAs(UnmanagedType.AnsiBStr)] string s,
            [MarshalAs(UnmanagedType.AnsiBStr)] ref string refs,
            [MarshalAs(UnmanagedType.AnsiBStr)] out string outs);

        [DllImport("unmanaged.dll")]
        [return: MarshalAs(UnmanagedType.TBStr)]
        static extern string TBStr(
            [MarshalAs(UnmanagedType.TBStr)] string s,
            [MarshalAs(UnmanagedType.TBStr)] ref string refs,
            [MarshalAs(UnmanagedType.TBStr)] out string outs);

        [DllImport("unmanaged.dll")]
        [return: MarshalAs(UnmanagedType.BStr)]
        static extern string BStr(
            [MarshalAs(UnmanagedType.BStr)] string s,
            [MarshalAs(UnmanagedType.BStr)] ref string refs,
            [MarshalAs(UnmanagedType.BStr)] out string outs);

        [DllImport("unmanaged.dll")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        static extern string LPStr(
            [MarshalAs(UnmanagedType.LPStr)] string s,
            [MarshalAs(UnmanagedType.LPStr)] ref string refs,
            [MarshalAs(UnmanagedType.LPStr)] out string outs);

        [DllImport("unmanaged.dll")]
        [return: MarshalAs(UnmanagedType.LPTStr)]
        static extern string LPTStr(
            [MarshalAs(UnmanagedType.LPTStr)] string s,
            [MarshalAs(UnmanagedType.LPTStr)] ref string refs,
            [MarshalAs(UnmanagedType.LPTStr)] out string outs);

        [DllImport("unmanaged.dll")]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        static extern string LPWStr(
            [MarshalAs(UnmanagedType.LPWStr)] string s,
            [MarshalAs(UnmanagedType.LPWStr)] ref string refs,
            [MarshalAs(UnmanagedType.LPWStr)] out string outs);

        [DllImport("unmanaged.dll")]
        static extern void VBByrefStr(
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string refs);

        [DllImport("unmanaged.dll")]
        static extern void ByValTStr(
            StructWithEmbeddedStringAuto sauto,
            StructWithEmbeddedStringAnsi sansi,
            StructWithEmbeddedStringUnicode sunicode);

        #endregion

        #region Callbacks

        delegate void ADel(out Struct1 mys);

        //[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        private delegate void Callback2(StringBuilder cbSb);

        private delegate int SampleCallback(double d, out byte b, out string qq, Callback2 hujer);
        
        #endregion

        #region Arrays

        [DllImport("unmanaged.dll")]
        static extern void CArray(int[] x);

        [DllImport("unmanaged.dll")]
        static extern void SafeArray([MarshalAs(UnmanagedType.SafeArray)]int[] x);

        [DllImport("unmanaged.dll")]
        static extern void ObjectSafeArray([MarshalAs(UnmanagedType.SafeArray)]object[] x);

        [DllImport("unmanaged.dll")]
        static extern void ObjectSafeArrayOfVariants([MarshalAs(UnmanagedType.SafeArray,
            SafeArraySubType=VarEnum.VT_VARIANT)]object[] x);

        [DllImport("unmanaged.dll")]
        static extern void SafeArrayOfVariants([MarshalAs(UnmanagedType.SafeArray,
            SafeArraySubType = VarEnum.VT_VARIANT)]int[] x);

        #endregion

        [DllImport("unmanaged.dll")]
        static extern object Variants(object x, ref object y, out object z);

        [StructLayout(LayoutKind.Sequential)]
        class Test
        {

        }

        [StructLayout(LayoutKind.Sequential)]
        public class Class1
        {
            object obj;
        }

        public struct Struct1
        {
            object obj;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CSelectedObject
        {
            [FieldOffset(0)]
            public short layerNo;
            [FieldOffset(2)]
            public short objNo;
            [FieldOffset(8)]
            public String objName;
        }

        [DllImport("unmanaged.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static unsafe extern bool SamplePInvoke(CSelectedObject wukwi,
            //Delegate d1,
            //[In]Delegate d2,
            //[Out] Delegate d3,
            //[In,Out] Delegate d4,
            //ref Delegate d5,
            //[In] ref Delegate d6,
            //out Delegate d7,
            //[In,Out] ref Delegate d8,
            //IComparable i1,
            //[In] IComparable i2,
            //[Out] IComparable i3,
            //[In,Out] IComparable i4,
            //ref IComparable i5,
            //[In]ref IComparable i6,
            //out IComparable i7,
            //[In,Out] ref IComparable i8,
            //object o1,
            //[In] object o2,
            //[Out] object o3,
            //[In,Out] object o4,
            //ref object o5,
            //[In]ref object o6,
            //out object o7,
            //[In,Out]ref object o8,
            [MarshalAs(UnmanagedType.IDispatch)] object c1,
            [MarshalAs(UnmanagedType.IDispatch)][In] object c2,
            [MarshalAs(UnmanagedType.IDispatch)][Out] object c3,
            [MarshalAs(UnmanagedType.IDispatch)][In, Out] object c4,
            [MarshalAs(UnmanagedType.IDispatch)]ref object c5,
            [MarshalAs(UnmanagedType.IDispatch)][In]ref object c6,
            [MarshalAs(UnmanagedType.IDispatch)]out object c7,
            [MarshalAs(UnmanagedType.IDispatch)][In, Out]ref object c8
            );

          


            //ClassBlit cls,
            //[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DummyMarshaler))] ref string hujer,
            //Hashtable ht,
            //int* pi,
            //[MarshalAs(UnmanagedType.LPArray)] string[] array,
            //StringBuilder sb,
            //[Out]S4_1 myBoolParam);
            //StructWithEmbeddedStringAnsi[] arg);

        public struct S4_1
        {
            [MarshalAs(UnmanagedType.Error)]
            public int i32;
            public TestDelegate1 myDelegate1;
            public TestDelegate2 myDelegate2;
        }
        public delegate S4_1 TestDelegate1();
        public delegate void TestDelegate2(S4_1 myStruct1);
        public class DllImport_TestStruct4
        {
            [DllImport("..\\Common\\DLLIMP_M3.DLL")]
            static extern int TestMarshalStruct4(S4_1 myStruct2);
        }

        [DllImport("..\\Common\\DLLIMP_M3.DLL")]
        private static extern int TestU1(U1_a myStruct1, U1_b myStruct2, U1_c myStruct3, U1_d myStruct4);

        [DllImport("..\\Common\\DLLIMP_M3.DLL")]
        private static extern int TestNestedArrays(SNestedArrays myStruct1);

        [DllImport("..\\Common\\DLLIMP_M3.DLL")]
        private static extern int TestBlittability(
            ref BlittableOnUnicodePlatform b1,
            ref Blittable1 b2,
            ref Blittable2 b3,
            ref NonBlittable1 b4,
            ref NonBlittable2 b5);
    }
}
