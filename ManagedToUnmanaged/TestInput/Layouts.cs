/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TestInput
{
    /// <summary>
    /// A blittable union with everything at offset 0.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    struct Union1
    {
        [FieldOffset(0)]
        byte b1;

        [FieldOffset(0)]
        short s1;

        [FieldOffset(0)]
        int i1;

        [FieldOffset(0)]
        long l1;

        [FieldOffset(0)]
        double d1;
    }

    /// <summary>
    /// A blittable union with everything at offset 0.
    /// Mutually depends on <see cref="MisalignedStruct"/>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    unsafe struct Union2
    {
        [FieldOffset(0)]
        MisalignedStruct *ptr;

        [FieldOffset(0)]
        long l;

        [FieldOffset(0)]
        double d;
    }

    /// <summary>
    /// Wildly misaligned structure.
    /// Mutually depends on <see cref="Union2"/>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 13)]
    unsafe struct MisalignedStruct
    {
        [FieldOffset(3)]
        int i;

        [FieldOffset(5)]
        Union2 *ptr;

        [FieldOffset(7)]
        short s;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Struct1
    {
        Struct2 x;
    }

    enum Enum1
    {
        first, second, third = 100, fourth
    }

    /// <summary>
    /// Unaligned sequential structure with an odd size.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 27, Pack = 2)]
    struct Struct2
    {
        double x;
        char y;
        int c;
        char d;
        int e;
    }

    /// <summary>
    /// Class with layout (blittable).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    class ClassBlit
    {
        public int a;
        public int b;
        public double c;
    }

    /// <summary>
    /// Class with layout (non-blittable).
    /// </summary>
    class ClassNonBlit
    {
        public int a;
        public int b;
        public double c;
        string s;
    }

    /// <summary>
    /// Structure without layout.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    struct StructNoLayout
    {
        int f1;
        int f2;
    }

    /// <summary>
    /// DDB 46687 test case.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct PrimitiveStruct
    {
        public int i;
    }
    
    /// <summary>
    /// DDB 46687 test case.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct PrimitiveStruct2
    {
        public PrimitiveStruct x;
    }

    /// <summary>
    /// A structure with embdedded class with layout.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct StructWithEmbeddedClass
    {
        public ClassBlit embedded;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct StructWithEmbeddedStringAuto
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string str;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct StructWithEmbeddedStringAnsi
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string str;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct StructWithEmbeddedStringUnicode
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string str;
    }

    struct StructWithEmbeddedArrays
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public long[] a1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8, ArraySubType=UnmanagedType.LPStr)]
        public string[] a2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.LPWStr)]
        public string[] a3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.LPWStr)]
        public ClassBlit[] a4;
    }

#if NoNo

    [StructLayout(LayoutKind.Explicit)]
    struct Overlapping1
    {
        [FieldOffset(0)]
        public string str;

        [FieldOffset(0)]
        public int i;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Overlapping2
    {
        [FieldOffset(4)]
        public string str;

        [FieldOffset(6)]
        public int i;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Overlapping3
    {
        [FieldOffset(4)]
        public string str;

        [FieldOffset(2)]
        public int i;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Overlapping4
    {
        [FieldOffset(0)]
        public WithRefs x1;

        [FieldOffset(0)]
        public int i;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Overlapping5
    {
        [FieldOffset(0)]
        public string x1;

        [FieldOffset(0)]
        public WithoutRefs i;
    }

#endif

    [StructLayout(LayoutKind.Explicit)]
    struct OverlappingOK
    {
        [FieldOffset(0)]
        public WithoutRefs x1;

        [FieldOffset(0)]
        public WithoutRefs x2;
    }

    struct WithoutRefs
    {
        public int i1;
        public int i2;
    }

    struct WithRefs
    {
        public string i1;
        public int i2;
    }

    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public struct U1_a
    {
        [FieldOffset(0)]
        int b;
        [FieldOffset(4)]
        double d;
    }

    [StructLayout(LayoutKind.Explicit/*, Size = 2*/)]
    public struct U1_b
    {
        [FieldOffset(0)]
        byte x;
        [FieldOffset(2)]
        short y;

        [FieldOffset(8)]
        int b;
        [FieldOffset(16)]
        double d;
    }

    [StructLayout(LayoutKind.Explicit/*, Size = 2*/)]
    public struct U1_c
    {
        [FieldOffset(0)]
        int b;
        [FieldOffset(8)]
        double d;
    }

    [StructLayout(LayoutKind.Explicit/*, Size = 2*/)]
    public struct U1_d
    {
        [FieldOffset(1)]
        byte x;
        [FieldOffset(2)]
        short y;

        [FieldOffset(3)]
        int b;
        [FieldOffset(9)]
        double d;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SNestedArrays
    {
        char c;
        int[][] i;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct BlittableOnUnicodePlatform
    {
        char ch;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Blittable1
    {
        char ch;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Blittable2
    {
        [MarshalAs(UnmanagedType.U2)]
        char ch;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NonBlittable1
    {
        char ch;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NonBlittable2
    {
        [MarshalAs(UnmanagedType.U1)]
        char ch;
    }
}
