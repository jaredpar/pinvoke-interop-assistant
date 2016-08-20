// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics;

namespace PInvoke.Parser
{
    public partial class ExpressionValue
    {
        private sealed class BooleanValue : ExpressionValue
        {
            private readonly bool _value;

            public override ExpressionValueKind Kind => ExpressionValueKind.Boolean;

            internal BooleanValue(bool value)
            {
                _value = value;
            }

            public override bool ConvertToBool() => _value;
            public override int ConvertToInteger() => _value ? 1 : 0;
            public override long ConvertToLong() => ConvertToInteger();
            public override string ToString() => $"Boolean Value {_value}";
        }
    }
}
