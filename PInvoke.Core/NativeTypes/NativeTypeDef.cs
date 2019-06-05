// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System.Diagnostics;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// TypeDef of a type.  At first it seems like this should be a NativeProxyType.  However 
    /// NativeProxyTypes aren't really types.  They are just references or modifiers to a type.  A
    /// Typedef is itself a type and accessible by name
    /// </summary>
    [DebuggerDisplay("{FullName} -> {RealTypeFullname}")]
    public class NativeTypeDef : NativeProxyType
    {
        public override NativeSymbolKind Kind => NativeSymbolKind.TypeDefType;

        public NativeName NativeName => new NativeName(Name, NativeNameKind.TypeDef);

        public NativeTypeDef(string name) : base(name)
        {
        }

        public NativeTypeDef(string name, string realtypeName) : base(name)
        {
            this.RealType = new NativeNamedType(realtypeName);
        }

        public NativeTypeDef(string name, NativeType realtype) : base(name)
        {
            this.RealType = realtype;
        }

        public NativeTypeDef(string name, BuiltinType bt) : base(name)
        {
            this.RealType = new NativeBuiltinType(bt);
        }

    }
}
