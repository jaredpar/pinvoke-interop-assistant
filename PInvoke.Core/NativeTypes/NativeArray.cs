// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System;

namespace PInvoke.NativeTypes
{
    public class NativeArray : NativeProxyType
    {
        private int _elementCount = -1;

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.ArrayType; }
        }

        /// <summary>
        /// Element count of the array.  If the array is not bound then this will
        /// be -1
        /// TODO: use a nullable here
        /// </summary>
        public int ElementCount
        {
            get { return _elementCount; }
            set { _elementCount = value; }
        }

        /// <summary>
        /// Create the display name of the array
        /// </summary>
        public override string DisplayName
        {
            get
            {
                string suffix = null;
                if (_elementCount >= 0)
                {
                    suffix = string.Format("[{0}]", this.ElementCount);
                }
                else
                {
                    suffix = "[]";
                }

                if (RealType == null)
                {
                    return "<null>" + suffix;
                }
                else
                {
                    return RealType.DisplayName + suffix;
                }
            }
        }

        public NativeArray() : base("[]")
        {
        }

        public NativeArray(NativeType realType, Int32 elementCount) : base("[]")
        {
            this.RealType = realType;
            this.ElementCount = elementCount;
        }

        public NativeArray(BuiltinType bt, Int32 elementCount) : this(new NativeBuiltinType(bt), elementCount)
        {
        }

    }
}
