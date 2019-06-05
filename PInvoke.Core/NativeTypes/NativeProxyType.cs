// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System.Diagnostics;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Base class for proxy types.  That is types which are actually a simple modification on another
    /// type.  This is typically name based such as typedefs or type based such as arrays and pointers
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayName}")]
    public abstract class NativeProxyType : NativeType
    {
        private NativeType _realType;
        /// <summary>
        /// Underlying type of the array
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeType RealType
        {
            get { return _realType; }
            set { _realType = value; }
        }

        public NativeType RealTypeDigged
        {
            get
            {
                if (_realType != null)
                {
                    return _realType.DigThroughTypeDefAndNamedTypes();
                }

                return _realType;
            }
        }

        public override bool IsImmediateResolved
        {
            get { return _realType != null; }
        }

        public override NativeSymbolCategory Category
        {
            get { return NativeSymbolCategory.Proxy; }
        }

        protected NativeProxyType(string name) : base(name)
        {
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(RealType);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildSingle(oldChild, newChild, ref _realType);
        }
    }

}
