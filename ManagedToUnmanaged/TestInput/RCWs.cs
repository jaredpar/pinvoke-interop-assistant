/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace TestInput
{
    [ComImport]
    [Guid("2F7DECC3-65FA-4A68-BD9A-2AD57034B142")]
    interface IMyObject
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        int DoFoo();

        [MethodImpl(MethodImplOptions.InternalCall)]
        int DoBar(int celt, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] object[] rgVar, IntPtr pceltFetched);
    }

    [ComImport]
    [Guid("611A5C32-0E7B-4085-B1FD-735BB602881F")]
    class CMyObject : IMyObject
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [PreserveSig]
        extern public int DoX([MarshalAs(UnmanagedType.LPStr)] out String s, bool b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern public int DoFoo();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern public int DoBar(int celt, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] object[] rgVar, IntPtr pceltFetched);
    }
}
