/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TestInput
{
    /// <summary>
    /// Correctly declared custom marshaler.
    /// </summary>
    class DummyMarshaler : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return null;
        }

        #region ICustomMarshaler Members

        public void CleanUpManagedData(object ManagedObj)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int GetNativeDataSize()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    /// <summary>
    /// Custom marshaler with the <B>GetInstance</B> method omitted (ERROR).
    /// </summary>
    class DummyMarshalerWithoutGetInstance : ICustomMarshaler
    {
        #region ICustomMarshaler Members

        public void CleanUpManagedData(object ManagedObj)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int GetNativeDataSize()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    /// <summary>
    /// A class with <B>GetInstance</B> that is not a marshaler.
    /// </summary>
    class DummyClassWithGetInstance
    {
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return null;
        }
    }

    /// <summary>
    /// A class that is not a marshaler.
    /// </summary>
    class DummyClass
    {
    }
}
