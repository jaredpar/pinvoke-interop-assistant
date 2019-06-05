// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a type that is intentionally being hidden from the user.  Usually takes the following form
    /// typedef struct UndefinedType *PUndefinedType
    /// 
    /// PUndefinedType is a legal pointer reference and the struct "foo" can later be defined in a .c/.cpp file
    /// </summary>
    /// <remarks></remarks>
    public class NativeOpaqueType : NativeSpecializedType
    {

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.OpaqueType; }
        }

        public NativeOpaqueType() : base("Opaque")
        {
        }
    }
}
