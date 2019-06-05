// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a C++ Union
    /// </summary>
    /// <remarks></remarks>
    public class NativeUnion : NativeDefinedType
    {
        public override NativeSymbolKind Kind => NativeSymbolKind.UnionType;

        public override NativeNameKind NameKind => NativeNameKind.Union;

        public NativeUnion()
        {
        }

        public NativeUnion(string name) : base(name)
        {
        }

    }
}
