// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace PInvoke
{
    public static class Contract
    {
        public static bool InUnitTest;

        public static void ThrowIfNull(object value)
        {
            Contract.ThrowIfNull(value, "Value should not be null");
        }

        public static void ThrowIfNull(object value, string message)
        {
            if ((value == null))
            {
                Contract.Violation(message);
            }
        }

        public static void ThrowIfFalse(bool value)
        {
            Contract.ThrowIfFalse(value, "Unexpected false");
        }

        public static void ThrowIfFalse(bool value, string message)
        {
            if (!value)
            {
                Contract.Violation(message);
            }
        }

        public static void ThrowIfTrue(bool value)
        {
            Contract.ThrowIfTrue(value, "Unexpected true");
        }

        public static void ThrowIfTrue(bool value, string message)
        {
            if (value)
            {
                Contract.Violation(message);
            }
        }

        public static void ThrowInvalidEnumValue<T>(T value) where T : struct
        {
            Violation(CreateInvalidEnumValueException(value));
        }

        public static Exception CreateInvalidEnumValueException<T>(T value) where T : struct
        {
            var message = $"Invalid enum value of type {typeof(T).Name}: {value}";
            return new ContractException(message);
        }

        public static void Requires(bool b)
        {
            ThrowIfFalse(b);
        }

        public static void Violation(string message)
        {
            Violation(new ContractException(message));
        }

        public static void Violation(string format, params object[] args)
        {
            Violation(string.Format(format, args));
        }

        private static void Violation(Exception exception)
        {
            Debug.Fail("Contract Violation: " + exception.Message);
            throw exception;
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
}
