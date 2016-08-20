// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics;

namespace PInvoke.Parser
{
    public partial class ExpressionValue
    {
        private sealed class CharValue : ExpressionValue
        {
            private readonly char _value;

            public override ExpressionValueKind Kind => ExpressionValueKind.Char;

            internal CharValue(char c)
            {
                _value = c;
            }

            public override int ConvertToInteger() => (int)_value;
            public override long ConvertToLong() => ConvertToInteger();
            public override string ToString() => $"Char Value {_value}";
        }
    }
}
