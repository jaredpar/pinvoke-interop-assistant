// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace PInvoke.NativeTypes.Enums
{
    public enum NativeValueKind
    {
        Number,
        String,
        Character,
        Boolean,

        /// <summary>
        /// Used when the value needs a Symbol which represents a Value
        /// </summary>
        /// <remarks></remarks>
        SymbolValue,

        /// <summary>
        /// Used when the value needs a Symbol which represents a Type.  For instance
        /// a Cast expression needs a Type Symbol rather than a Value symbol
        /// </summary>
        /// <remarks></remarks>
        SymbolType
    }
}
