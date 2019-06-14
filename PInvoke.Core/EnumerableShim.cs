// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace PInvoke
{
    internal class EnumerableShim<T> : IEnumerable<T>
    {

        private class EnumeratorShim : IEnumerator<T>
        {


            private IEnumerator enumerator;
            public EnumeratorShim(IEnumerator e)
            {
                enumerator = e;
            }

            public T Current
            {
                get { return (T)enumerator.Current; }
            }

            public object Current1
            {
                get { return enumerator.Current; }
            }
            object IEnumerator.Current
            {
                get { return Current1; }
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }

            #region " IDisposable Support "
            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                // Nothing to dispose here
            }
            #endregion

        }


        private IEnumerable enumerable;
        public EnumerableShim(IEnumerable e)
        {
            enumerable = e;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EnumeratorShim(enumerable.GetEnumerator());
        }

        public IEnumerator GetEnumerator1()
        {
            return enumerable.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }
    }
}