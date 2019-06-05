// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace PInvoke
{
    /// <summary>
    /// Enumeration of the common C++ builtin types
    /// </summary>
    public enum BuiltinType
    {
        NativeInt16,
        NativeInt32,
        NativeInt64,
        NativeFloat,
        NativeDouble,
        NativeBoolean,
        NativeChar,
        NativeWChar,
        NativeByte,
        NativeVoid,

        /// <summary>
        /// Used for BuiltinTypes initially missed
        /// </summary>
        NativeUnknown
    }
}
