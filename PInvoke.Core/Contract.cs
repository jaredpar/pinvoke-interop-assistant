
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Runtime.Serialization;

static internal class Contract
{

	public static bool InUnitTest;
	public static void ThrowIfNull(object value)
	{
		Contract.ThrowIfNull(value, "Value should not be null");
	}

	public static void ThrowIfNull(object value, string message)
	{
		if ((value == null)) {
			Contract.ContractViolation(message);
		}
	}

	public static void ThrowIfFalse(bool value)
	{
		Contract.ThrowIfFalse(value, "Unexpected false");
	}

	public static void ThrowIfFalse(bool value, string message)
	{
		if (!value) {
			Contract.ContractViolation(message);
		}
	}

	public static void ThrowIfTrue(bool value)
	{
		Contract.ThrowIfTrue(value, "Unexpected true");
	}

	public static void ThrowIfTrue(bool value, string message)
	{
		if (value) {
			Contract.ContractViolation(message);
		}
	}

	public static void InvalidEnumValue<T>(T value) where T : struct
	{
		Contract.ThrowIfFalse(typeof(T).IsEnum, "Expected an enum type");
		Contract.Violation("Invalid Enum value of Type {0} : {1}", new object[] {
			typeof(T).Name,
			value
		});
	}

	public static void Violation(string message)
	{
		Contract.ContractViolation(message);
	}

	public static void Violation(string format, params object[] args)
	{
		Contract.ContractViolation(string.Format(format, args));
	}

	private static void ContractViolation(string message)
	{
		Debug.Fail("Contract Violation: " + message);
		bool inUnitTest = Contract.InUnitTest;
		StackTrace trace = new StackTrace();
		string text = message;
		text = text + Environment.NewLine + trace.ToString;
		throw new ContractException(message);
	}
}

[Serializable()]
internal class ContractException : Exception
{
	// Methods
	public ContractException() : this("Contract Violation")
	{
	}

	public ContractException(string message) : base(message)
	{
	}

	protected ContractException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}

	public ContractException(string message, Exception inner) : base(message, inner)
	{
	}

}


//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
