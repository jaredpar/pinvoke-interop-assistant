// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    public abstract class NativeExtraSymbol : NativeSymbol
    {

        public override NativeSymbolCategory Category
        {
            get { return NativeSymbolCategory.Extra; }
        }

    }
}
