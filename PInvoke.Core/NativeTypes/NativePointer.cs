// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// A Pointer
    /// </summary>
    /// <remarks></remarks>
    public class NativePointer : NativeProxyType
    {

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.PointerType; }
        }

        /// <summary>
        /// Returs the pointer full type name
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string DisplayName
        {
            get
            {
                if (RealType == null)
                {
                    return "<null>*";
                }
                else
                {
                    return RealType.DisplayName + "*";
                }
            }
        }

        public NativePointer() : base("*")
        {
        }

        public NativePointer(NativeType realtype) : base("*")
        {
            this.RealType = realtype;
        }

        public NativePointer(BuiltinType bt) : base("*")
        {
            this.RealType = new NativeBuiltinType(bt);
        }

    }
}
