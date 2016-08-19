// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
namespace PInvoke.Parser
{
    [DebuggerDisplay("Value={Value}")]
    public class ExpressionValue
    {
        // CTODO: get rid of dynamic
        // CTODO: get rid of equality override as they're likely wrong.
        public dynamic Value { get; set; }

        public ExpressionValue(object value)
        {
            Contract.ThrowIfNull(value);
            Value = value;
        }

        public ExpressionValue(bool value)
        {
            if (value)
            {
                Value = 1;
            }
            else
            {
                Value = 0;
            }
        }

        public static ExpressionValue operator +(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return new ExpressionValue(left.Value + right.Value);
        }

        public static ExpressionValue operator -(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return new ExpressionValue(left.Value - right.Value);
        }

        public static ExpressionValue operator -(ExpressionValue left)
        {
            Contract.ThrowIfNull(left);
            return new ExpressionValue(-(left.Value));
        }

        public static ExpressionValue operator /(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return new ExpressionValue(left.Value / right.Value);
        }

        public static bool operator >(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return left.Value > right.Value;
        }

        public static bool operator <(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return left.Value < right.Value;
        }

        public static bool operator >=(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return left.Value >= right.Value;
        }

        public static bool operator <=(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return left.Value <= right.Value;
        }

        public static bool operator !=(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return left.Value != right.Value;
        }

        public static bool operator ==(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return left.Value == right.Value;
        }

        public static ExpressionValue operator *(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return new ExpressionValue(left.Value * right.Value);
        }

        public static ExpressionValue operator <<(ExpressionValue left, Int32 count)
        {
            Contract.ThrowIfNull(left);
            return new ExpressionValue(Convert.ToInt32(left.Value) << count);
        }

        public static ExpressionValue operator >>(ExpressionValue left, Int32 count)
        {
            Contract.ThrowIfNull(left);
            return new ExpressionValue(Convert.ToInt32(left.Value) >> count);
        }

        public static bool operator true(ExpressionValue expr)
        {
            Contract.ThrowIfNull(expr);
            return Convert.ToBoolean(expr.Value);
        }

        public static bool operator false(ExpressionValue expr)
        {
            Contract.ThrowIfNull(expr);
            return !Convert.ToBoolean(expr.Value);
        }

        public static bool operator ~(ExpressionValue expr)
        {
            Contract.ThrowIfNull(expr);
            return !Convert.ToBoolean(expr.Value);
        }

        public static ExpressionValue operator &(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return new ExpressionValue(left.Value & right.Value);
        }

        public static ExpressionValue operator |(ExpressionValue left, ExpressionValue right)
        {
            Contract.ThrowIfNull(left);
            Contract.ThrowIfNull(right);
            return new ExpressionValue(left.Value | right.Value);
        }

        public static implicit operator ExpressionValue(Int32 value)
        {
            return new ExpressionValue(value);
        }

        public static implicit operator ExpressionValue(bool value)
        {
            return new ExpressionValue(value);
        }

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public override bool Equals(object obj)
        {
            var other = obj as ExpressionValue;
            if (other == null)
            {
                return false;
            }

            return other == this;
        }
    }
}
