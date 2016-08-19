// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace PInvoke
{
    #region "Constants"

    public static class Constants
    {
        public const string ProductName = "PInvoke Interop Assistant";
        public const string Version = "1.0.0.0";

        public const string FriendlyVersion = "1.0";
    }

    #endregion

    #region "ErrorProvider"

    /// <summary>
    /// Provides an encapsulation for error messages and warnings
    /// </summary>
    /// <remarks></remarks>
    public class ErrorProvider
    {

        private List<string> _warningList = new List<string>();

        private List<string> _errorList = new List<string>();
        /// <summary>
        /// Errors
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> Errors
        {
            get { return _errorList; }
        }

        /// <summary>
        /// Warnings
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> Warnings
        {
            get { return _warningList; }
        }

        /// <summary>
        /// All messages 
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<string> AllMessages
        {
            get
            {
                List<string> list = new List<string>();
                list.AddRange(_warningList);
                list.AddRange(_errorList);
                return list;
            }
        }

        public void AddWarning(string str)
        {
            _warningList.Add(str);
        }

        public void AddWarning(string str, params object[] args)
        {
            string msg = string.Format(str, args);
            _warningList.Add(msg);
        }

        public void AddError(string str)
        {
            _errorList.Add(str);
        }

        public void AddError(string str, params object[] args)
        {
            string msg = string.Format(str, args);
            _errorList.Add(msg);
        }

        /// <summary>
        /// Append the data in the passed in ErrorProvider into this instance
        /// </summary>
        /// <param name="ep"></param>
        /// <remarks></remarks>
        public void Append(ErrorProvider ep)
        {
            _errorList.AddRange(ep.Errors);
            _warningList.AddRange(ep.Warnings);
        }


        public ErrorProvider()
        {
        }

        public ErrorProvider(ErrorProvider ep)
        {
            Append(ep);
        }

        public string CreateDisplayString()
        {
            var builder = new StringBuilder();
            foreach (string msg in Errors)
            {
                builder.AppendFormat("Error: {0}", msg);
                builder.AppendLine();
            }

            foreach (string msg in Warnings)
            {
                builder.AppendFormat("Warning: {0}", msg);
                builder.AppendLine();
            }

            return builder.ToString();
        }

    }

    #endregion

    #region "EnumerableShim"

    internal class EnumerableShim<T> : IEnumerable<T>
    {

        private class EnumeratorShim : IEnumerator<T>
        {


            private IEnumerator _enumerator;
            public EnumeratorShim(IEnumerator e)
            {
                _enumerator = e;
            }

            public T Current
            {
                get { return (T)_enumerator.Current; }
            }

            public object Current1
            {
                get { return _enumerator.Current; }
            }
            object System.Collections.IEnumerator.Current
            {
                get { return Current1; }
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
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


        private IEnumerable _enumerable;
        public EnumerableShim(IEnumerable e)
        {
            _enumerable = e;
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return new EnumeratorShim(_enumerable.GetEnumerator());
        }

        public System.Collections.IEnumerator GetEnumerator1()
        {
            return _enumerable.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }
    }

    #endregion

    #region "EnumUtil"

    public static class EnumUtil
    {

        public static List<T> GetAllValues<T>()
        {
            List<T> list = new List<T>();
            foreach (T cur in System.Enum.GetValues(typeof(T)))
            {
                list.Add(cur);
            }

            return list;
        }

        public static object[] GetAllValuesObject<T>()
        {
            List<object> list = new List<object>();
            foreach (T cur in System.Enum.GetValues(typeof(T)))
            {
                list.Add(cur);
            }

            return list.ToArray();
        }

        public static object[] GetAllValuesObjectExcept<T>(T except)
        {
            EqualityComparer<T> comp = EqualityComparer<T>.Default;
            List<object> list = new List<object>();
            foreach (T cur in System.Enum.GetValues(typeof(T)))
            {
                if (!comp.Equals(cur, except))
                {
                    list.Add(cur);
                }
            }

            return list.ToArray();
        }

    }

    #endregion
}