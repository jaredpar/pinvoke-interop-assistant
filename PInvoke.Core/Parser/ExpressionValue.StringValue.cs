// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics;

namespace PInvoke.Parser
{
    public partial class ExpressionValue
    {
        private sealed class StringValue : ExpressionValue
        {
            internal string Value { get; }
            public override ExpressionValueKind Kind => ExpressionValueKind.String;

            internal StringValue(string s)
            {
                Value = s;
            }

            public override bool ConvertToBool() => Value != null;

            public override int ConvertToInteger()
            {
                throw new Exception($"Can't convert {nameof(StringValue)} to integer");
            }

            public override long ConvertToLong()
            {
                return ConvertToInteger();
            }

            public override string ToString() => $@"String Value ""{Value}""";
        }
    }
}
