// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Types that are specialized for generation
    /// </summary>
    /// <remarks></remarks>
    public abstract class NativeSpecializedType : NativeType
    {

        public override NativeSymbolCategory Category
        {
            get { return NativeSymbolCategory.Specialized; }
        }


        protected NativeSpecializedType()
        {
        }

        protected NativeSpecializedType(string name) : base(name)
        {
        }
    }
}
