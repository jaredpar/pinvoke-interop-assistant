// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.Enums;
using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a C++ struct
    /// </summary>
    /// <remarks></remarks>
    public class NativeStruct : NativeDefinedType
    {
        public override NativeSymbolKind Kind => NativeSymbolKind.StructType;

        public override NativeNameKind NameKind => NativeNameKind.Struct;

        public NativeStruct()
        {
        }

        public NativeStruct(string name) : base(name)
        {
        }

    }
}
