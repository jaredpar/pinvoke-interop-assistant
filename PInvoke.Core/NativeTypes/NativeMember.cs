// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System.Diagnostics;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a member of a native type.
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{NativeType.FullName} {Name}")]
    public sealed class NativeMember : NativeExtraSymbol
    {
        /// <summary>
        /// Nativetype of the member
        /// </summary>
        public NativeType NativeType;

        public NativeType NativeTypeDigged
        {
            get
            {
                if (NativeType != null)
                {
                    return NativeType.DigThroughTypeDefAndNamedTypes();
                }

                return null;
            }
        }

        public override bool IsImmediateResolved
        {
            get { return NativeType != null && !string.IsNullOrEmpty(Name); }
        }


        public NativeMember()
        {
        }

        public NativeMember(string name, NativeType nt)
        {
            Name = name;
            NativeType = nt;
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.Member; }
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(NativeType);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildSingle(oldChild, newChild, ref NativeType);
        }

    }
}
