
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
 // ERROR: Not supported in C#: OptionDeclaration
namespace Parser
{

	[DebuggerDisplay("Value={Value}")]
	public class ExpressionValue
	{

		private object _value;
		public object Value {
			get { return _value; }
			set { _value = value; }
		}

		public ExpressionValue(object value)
		{
			Contract.ThrowIfNull(value);
			_value = value;
		}

		public ExpressionValue(bool value)
		{
			if (value) {
				_value = 1;
			} else {
				_value = 0;
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

	}
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
