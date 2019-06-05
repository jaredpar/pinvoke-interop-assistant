// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// A native bit vector.  All bitvectors are generated as anonymous structs inside the 
    /// conttaining generated struct
    /// </summary>
    /// <remarks></remarks>
    public class NativeBitVector : NativeSpecializedType
    {
        private int _size;
        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.BitVectorType; }
        }

        /// <summary>
        /// Size of the bitvector
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public override string DisplayName
        {
            get { return "<bitvector " + Size + ">"; }
        }

        public NativeBitVector() : this(-1)
        {
        }

        public NativeBitVector(int size)
        {
            this.Name = "<bitvector>";
            _size = size;
        }

    }
}
