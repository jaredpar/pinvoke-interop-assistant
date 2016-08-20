// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics;

namespace PInvoke.Parser
{
    public partial class ExpressionValue
    {
        private sealed class NumberValue : ExpressionValue
        {
            internal Number Number { get; }
            public override ExpressionValueKind Kind { get; }

            internal NumberValue(Number number)
            {
                Number = number;
                Kind = Convert(number.Kind);
            }

            public override int ConvertToInteger() => Number.ConvertToInteger();
            public override long ConvertToLong() => Number.ConvertToLong();
            public override double ConvertToDouble() => Number.ConvertToDouble();
            public override string ToString() => $"NumberValue {Number.Kind} {Number.Value}";

            internal static ExpressionValueKind Convert(NumberKind kind)
            {
                switch (kind)
                {
                    case NumberKind.Integer: return ExpressionValueKind.Integer;
                    case NumberKind.Long: return ExpressionValueKind.Long;
                    case NumberKind.Single: return ExpressionValueKind.Single;
                    case NumberKind.Double: return ExpressionValueKind.Double;
                    default: throw Contract.CreateInvalidEnumValueException(kind);
                }
            }
        }
    }
}
