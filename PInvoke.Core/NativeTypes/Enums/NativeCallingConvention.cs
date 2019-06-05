// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace PInvoke.NativeTypes.Enums
{
    public enum NativeCallingConvention
    {
        /// <summary>
        /// Platform default
        /// </summary>
        /// <remarks></remarks>
        WinApi = 1,

        /// <summary>
        /// __stdcall
        /// </summary>
        /// <remarks></remarks>
        Standard,

        /// <summary>
        /// __cdecl
        /// </summary>
        /// <remarks></remarks>
        CDeclaration,

        /// <summary>
        /// __clrcall
        /// </summary>
        /// <remarks></remarks>
        Clr,

        /// <summary>
        /// __pascal
        /// </summary>
        /// <remarks></remarks>
        Pascal,

        /// <summary>
        /// inline, __inline, etc
        /// </summary>
        /// <remarks></remarks>
        Inline
    }
}
