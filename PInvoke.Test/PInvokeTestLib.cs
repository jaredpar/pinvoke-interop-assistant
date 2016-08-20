// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace PInvoke.Test.Lib
{
    public partial class NativeConstants
    {

        ///PINVOKETESTLIB_API -> __declspec(dllimport)
        ///Error generating expression: Error generating function call.  Operation not implemented

        public const string PINVOKETESTLIB_API = "__declspec(dllimport)";
        ///foo -> "bar"

        public const string foo = "bar";
        ///foo2 -> "bar2"

        public const string foo2 = "bar2";
        ///VALUE_CONSTANT_1 -> 5
        public const int VALUE_CONSTANT_1 = 5;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct BitVector1
    {

        ///m1 : 5
        ///m2 : 6

        public uint bitvector1;
        public uint m1
        {
            get { return Convert.ToUInt32((this.bitvector1 & 31u)); }
            set { this.bitvector1 = Convert.ToUInt32((value | this.bitvector1)); }
        }

        public uint m2
        {
            get { return Convert.ToUInt32(((this.bitvector1 & 2016u) / 32)); }
            set { this.bitvector1 = Convert.ToUInt32(((value * 32) | this.bitvector1)); }
        }
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct BitVector2
    {

        ///m1 : 2

        public uint bitvector1;
        ///BOOL->int
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]

        public bool m2;
        ///m3 : 2

        public uint bitvector2;
        public uint m1
        {
            get { return Convert.ToUInt32((this.bitvector1 & 3u)); }
            set { this.bitvector1 = Convert.ToUInt32((value | this.bitvector1)); }
        }

        public uint m3
        {
            get { return Convert.ToUInt32((this.bitvector2 & 3u)); }
            set { this.bitvector2 = Convert.ToUInt32((value | this.bitvector2)); }
        }
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct stringInStruct
    {

        ///wchar_t*
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
        public string m1;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct structWithDiffStringTypes
    {

        ///char*
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]

        public string m1;
        ///wchar_t*
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
        public string m2;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct s1
    {

        ///int

        public int m1;
        ///double
        public double m2;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    public struct s2
    {

        ///int

        public int m1;
        ///wchar_t[250]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 250)]
        public string m2;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct s3
    {

        ///int[4]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I4)]

        public int[] m1;
        ///double[4]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = System.Runtime.InteropServices.UnmanagedType.R8)]
        public double[] m2;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct s4
    {

        ///BYTE[4]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I1)]
        public byte[] m1;
    }

    public enum e1
    {

        v1,

        ///v2 -> 5
        v2 = 5
    }

    ///Return Type: int
    public delegate int pFunctionPointerReturningInt();

    ///Return Type: int
    ///param0: int
    ///param1: int
    public delegate int structWithFunctionPointer_pAddTheValues(int param0, int param1);

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct structWithFunctionPointer
    {

        ///int

        public int m1;
        ///structWithFunctionPointer_pAddTheValues
        public structWithFunctionPointer_pAddTheValues AnonymousMember1;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct simpleClass
    {

        ///int

        public int m1;
        ///int
        public int m2;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct tagDEC
    {

        ///USHORT->unsigned short

        public ushort wReserved;
        ///Anonymous_3f4bb2d9_b5e3_482f_a1cd_89cd5b104462

        public Anonymous_3f4bb2d9_b5e3_482f_a1cd_89cd5b104462 Union1;
        ///ULONG->unsigned int

        public uint Hi32;
        ///Anonymous_a0b7eb55_1311_474a_8d40_19409efee489
        public Anonymous_a0b7eb55_1311_474a_8d40_19409efee489 Union2;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public struct tagCY
    {

        ///Anonymous_7b8f9219_bb63_4657_9da5_d001a008954a
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]

        public Anonymous_7b8f9219_bb63_4657_9da5_d001a008954a Struct1;
        ///LONGLONG->__int64
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        public long int64;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public struct Anonymous_3f4bb2d9_b5e3_482f_a1cd_89cd5b104462
    {

        ///Anonymous_3e34b4d1_5346_4e41_8d1c_a00e4f45de60
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]

        public Anonymous_3e34b4d1_5346_4e41_8d1c_a00e4f45de60 Struct1;
        ///USHORT->unsigned short
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        public ushort signscale;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public struct Anonymous_a0b7eb55_1311_474a_8d40_19409efee489
    {

        ///Anonymous_a33fa63b_8218_4e8c_9c2c_042a1f4b4a06
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]

        public Anonymous_a33fa63b_8218_4e8c_9c2c_042a1f4b4a06 Struct1;
        ///ULONGLONG->unsigned __int64
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        public ulong Lo64;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Anonymous_7b8f9219_bb63_4657_9da5_d001a008954a
    {

        ///unsigned int

        public uint Lo;
        ///int
        public int Hi;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Anonymous_3e34b4d1_5346_4e41_8d1c_a00e4f45de60
    {

        ///BYTE->unsigned char

        public byte scale;
        ///BYTE->unsigned char
        public byte sign;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Anonymous_a33fa63b_8218_4e8c_9c2c_042a1f4b4a06
    {

        ///ULONG->unsigned int

        public uint Lo32;
        ///ULONG->unsigned int
        public uint Mid32;
    }

    public partial class NativeMethods
    {

        ///Return Type: BOOL->int
        ///data: BitVector1*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "UpdateBitVector1Data")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool UpdateBitVector1Data([System.Runtime.InteropServices.InAttribute()]
ref BitVector1 data);

        ///Return Type: BOOL->int
        ///data: BitVector1*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "IsM1GreaterThanM2")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool IsM1GreaterThanM2([System.Runtime.InteropServices.InAttribute()]
ref BitVector1 data);

        ///Return Type: BitVector2
        ///m1: DWORD->unsigned int
        ///m2: BOOL->int
        ///m3: DWORD->unsigned int
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CreateBitVector2")]
        public static extern BitVector2 CreateBitVector2(uint m1, [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
bool m2, uint m3);

        ///Return Type: BOOL->int
        ///orig: LPCWSTR->WCHAR*
        ///buffer: LPWSTR->WCHAR*
        ///size: int
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "ReverseString")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool ReverseString([System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string orig, [System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
System.Text.StringBuilder buffer, int size);

        ///Return Type: boolean
        ///orig: LPCWSTR->WCHAR*
        ///size: int*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CalculateStringLength")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CalculateStringLength([System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string orig, out int size);

        ///Return Type: int
        ///buffer: LPWSTR->WCHAR*
        ///size: int
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "GetVeryLongString")]
        public static extern int GetVeryLongString([System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
System.Text.StringBuilder buffer, int size);

        ///Return Type: unsigned int
        ///buffer: LPWSTR->WCHAR*
        ///size: unsigned int
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "GetVeryLongString2")]
        public static extern uint GetVeryLongString2([System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
System.Text.StringBuilder buffer, uint size);

        ///Return Type: boolean
        ///s1: stringInStruct
        ///comp: wchar_t*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "VerifyStringInStructM1")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool VerifyStringInStructM1(stringInStruct s1, [System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string comp);

        ///Return Type: void
        ///s1: structWithDiffStringTypes*
        ///m1: char*
        ///m2: wchar_t*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "PopulateStructWithDiffStringTypes")]
        public static extern void PopulateStructWithDiffStringTypes(ref structWithDiffStringTypes s1, [System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)]
string m1, [System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string m2);

        ///Return Type: boolean
        ///i: int
        ///d: double
        ///s: s1*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "s1FakeConstructor")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool s1FakeConstructor(int i, double d, out s1 s);

        ///Return Type: s1
        ///i: int
        ///d: double
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "s1FakeConstructor2")]
        public static extern s1 s1FakeConstructor2(int i, double d);

        ///Return Type: boolean
        ///i: int
        ///data: LPCWSTR->WCHAR*
        ///s: s2*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "s2FakeConstructor")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool s2FakeConstructor(int i, [System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string data, out s2 s);

        ///Return Type: int
        ///s: s4
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "s4Add")]
        public static extern int s4Add(s4 s);

        ///Return Type: boolean
        ///s: s3*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyM1ToM2")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CopyM1ToM2(ref s3 s);

        ///Return Type: boolean
        ///p: int*
        ///sum: int*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "SumArray")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool SumArray([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPArray, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I4, SizeConst = 4)]
int[] p, out int sum);

        ///Return Type: boolean
        ///p: int*
        ///size: int
        ///sum: int*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "SumArray2")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool SumArray2([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPArray, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I4, SizeParamIndex = 1)]
int[] p, int size, out int sum);

        ///Return Type: boolean
        ///toRet: WCHAR->wchar_t->unsigned short
        ///c: WCHAR**
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "GetPointerPointerToChar")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool GetPointerPointerToChar(char toRet, ref System.IntPtr c);

        ///Return Type: boolean
        ///dec: DECIMAL->tagDEC
        ///pDec: LPDECIMAL->DECIMAL*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyDecimalToPoiner")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CopyDecimalToPoiner(decimal dec, ref decimal pDec);

        ///Return Type: DECIMAL->tagDEC
        ///dec: DECIMAL->tagDEC
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyDecimalToReturn")]
        public static extern decimal CopyDecimalToReturn(decimal dec);

        ///Return Type: boolean
        ///pDec1: LPDECIMAL->DECIMAL*
        ///pDec2: LPDECIMAL->DECIMAL*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyDecimalPointerToPointer")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CopyDecimalPointerToPointer([System.Runtime.InteropServices.InAttribute()]
ref decimal pDec1, ref decimal pDec2);

        ///Return Type: boolean
        ///c: CURRENCY->CY->tagCY
        ///pCur: CURRENCY*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyCurrencyToPointer")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CopyCurrencyToPointer([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Currency)]
decimal c, [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Currency)]
ref decimal pCur);

        ///Return Type: boolean
        ///src: BSTR->OLECHAR*
        ///dest: LPWSTR->WCHAR*
        ///size: unsigned int*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyBstrToNoramlStr")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CopyBstrToNoramlStr([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)]
string src, [System.Runtime.InteropServices.OutAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
System.Text.StringBuilder dest, ref uint size);

        ///Return Type: boolean
        ///src: LPCWSTR->WCHAR*
        ///dest: BSTR*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyToBstr")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CopyToBstr([System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string src, [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)] out string dest);

        ///Return Type: boolean
        ///src1: LPCWSTR->WCHAR*
        ///src2: LPCWSTR->WCHAR*
        ///dest: BSTR*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyBothToBstr")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CopyBothToBstr([System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string src1, [System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string src2, [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)]
ref string dest);

        ///Return Type: boolean
        ///src: BSTR->OLECHAR*
        ///dest: BSTR*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyBstrToBstr")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CopyBstrToBstr([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)]
string src, [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)]
ref string dest);

        ///Return Type: BSTR->OLECHAR*
        ///src: LPCWSTR->WCHAR*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CopyNormalStrToBstrRet")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)]
        public static extern string CopyNormalStrToBstrRet([System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
string src);

        ///Return Type: POPAQUE1->opaque1*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "CreateBasicOpaque")]
        public static extern System.IntPtr CreateBasicOpaque();

        ///Return Type: boolean
        ///p1: POPAQUE1->opaque1*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "VerifyBasicOpaque")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool VerifyBasicOpaque(System.IntPtr p1);

        ///Return Type: pFunctionPointerReturningInt
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "GetFunctionPointerReturningInt")]
        public static extern pFunctionPointerReturningInt GetFunctionPointerReturningInt();

        ///Return Type: boolean
        ///pFPtr: pFunctionPointerReturningInt
        ///value: int
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "AreResultAndValueEqual")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool AreResultAndValueEqual(pFunctionPointerReturningInt pFPtr, int value);

        ///Return Type: void
        ///param0: int
        ///ps: structWithFunctionPointer*
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "GetAStructWithASimpleFunctionPointer")]
        public static extern void GetAStructWithASimpleFunctionPointer(int param0, ref structWithFunctionPointer ps);

        ///Return Type: int
        ///x: int
        ///y: int
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "MultiplyWithCDecl", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static extern int MultiplyWithCDecl(int x, int y);

        ///Return Type: int
        ///c1: simpleClass
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "GetSimpleClassM1")]
        public static extern int GetSimpleClassM1(simpleClass c1);

        ///Return Type: int
        ///c1: simpleClass
        [System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint = "GetSimpleClassM2")]
        public static extern int GetSimpleClassM2(simpleClass c1);

        [System.Diagnostics.DebuggerStepThroughAttribute(), System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")]
        public static bool ReverseString(string orig, ref string buffer)
        {
            System.Text.StringBuilder varbuffer = new System.Text.StringBuilder(1024);
            bool methodRetVar = false;
            methodRetVar = NativeMethods.ReverseString(orig, varbuffer, 1024);
            buffer = varbuffer.ToString();
            return methodRetVar;
        }

        [System.Diagnostics.DebuggerStepThroughAttribute(), System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")]
        public static int GetVeryLongString(ref string buffer)
        {
            System.Text.StringBuilder varbuffer = default(System.Text.StringBuilder);
            int retVar_ = 0;
            int sizeVar = 2056;
            PerformCall:
            varbuffer = new System.Text.StringBuilder(sizeVar);
            retVar_ = NativeMethods.GetVeryLongString(varbuffer, sizeVar);
            if ((retVar_ >= sizeVar))
            {
                sizeVar = (retVar_ + 1);
                goto PerformCall;
            }
            buffer = varbuffer.ToString();
            return retVar_;
        }

        [System.Diagnostics.DebuggerStepThroughAttribute(), System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")]
        public static uint GetVeryLongString2(ref string buffer)
        {
            System.Text.StringBuilder varbuffer = default(System.Text.StringBuilder);
            uint retVar_ = 0;
            uint sizeVar = 2056;
            PerformCall:
            varbuffer = new System.Text.StringBuilder(Convert.ToInt32(sizeVar));
            retVar_ = NativeMethods.GetVeryLongString2(varbuffer, sizeVar);
            if ((retVar_ >= sizeVar))
            {
                sizeVar = retVar_;
                goto PerformCall;
            }
            buffer = varbuffer.ToString();
            return retVar_;
        }

        [System.Diagnostics.DebuggerStepThroughAttribute(), System.CodeDom.Compiler.GeneratedCodeAttribute("P/Invoke Interop Assistant", "1.0")]
        public static bool CopyBstrToNoramlStr(string src, ref string dest)
        {
            System.Text.StringBuilder vardest = default(System.Text.StringBuilder);
            bool retVar_ = false;
            uint sizeVar = 2056;
            uint oldSizeVar_ = 0;
            PerformCall:
            oldSizeVar_ = sizeVar;
            vardest = new System.Text.StringBuilder(Convert.ToInt32(sizeVar));
            retVar_ = NativeMethods.CopyBstrToNoramlStr(src, vardest, ref sizeVar);
            if ((oldSizeVar_ <= sizeVar))
            {
                sizeVar = (sizeVar * Convert.ToUInt32(2));
                goto PerformCall;
            }
            dest = vardest.ToString();
            return retVar_;
        }
    }
}
