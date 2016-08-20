// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics;

namespace PInvoke.Parser
{
    public enum ExpressionValueKind
    {
        Boolean,
        Char,
        String,
        Integer,
        Long,
        Single,
        Double
    }

    /// <summary>
    /// Used for representing values in pre-processor expressions.
    /// </summary>
    public abstract partial class ExpressionValue
    {
        public abstract ExpressionValueKind Kind { get; }
        public bool IsFloatingPoint => Kind == ExpressionValueKind.Single || Kind == ExpressionValueKind.Double;
        public bool IsIntegral => Kind == ExpressionValueKind.Integer || Kind == ExpressionValueKind.Long;

        public abstract int ConvertToInteger();
        public abstract long ConvertToLong();
        public virtual bool ConvertToBool() => ConvertToLong() != 0;
        public virtual double ConvertToDouble() => ConvertToLong();

        public static ExpressionValue Create(bool b) => new BooleanValue(b);
        public static ExpressionValue Create(char c) => new CharValue(c);
        public static ExpressionValue Create(string s) => new StringValue(s);
        public static ExpressionValue Create(int i) => Create(new Number(i));
        public static ExpressionValue Create(long l) => Create(new Number(l));
        public static ExpressionValue Create(float f) => Create(new Number(f));
        public static ExpressionValue Create(double d) => Create(new Number(d));
        public static ExpressionValue Create(Number number) => new NumberValue(number);
    }
}
