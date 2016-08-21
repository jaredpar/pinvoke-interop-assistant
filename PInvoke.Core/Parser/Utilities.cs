// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace PInvoke.Parser
{

    /// <summary>
    /// Module for the constants in the parser
    /// </summary>
    /// <remarks></remarks>
    public static class Constants
    {

        /// <summary>
        /// Used when a file name is needed but it's not known
        /// </summary>
        /// <remarks></remarks>

        public static string UnknownFileName = "<unknown>";
    }

    /// <summary>
    /// Way of passing around a TextReader paired with it's name
    /// </summary>
    /// <remarks></remarks>
    public class TextReaderBag
    {
        private string _name;

        private TextReader _reader;
        /// <summary>
        /// Name of the stream
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The TextReader
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public System.IO.TextReader TextReader
        {
            get { return _reader; }
        }

        public TextReaderBag(System.IO.TextReader reader) : this(Constants.UnknownFileName, reader)
        {
        }

        public TextReaderBag(string name, System.IO.TextReader reader)
        {
            this._name = name;
            this._reader = reader;
        }

    }

    internal class TriState<T>
    {
        public T m_value;

        public bool m_hasValue;
        public bool HasValue
        {
            get { return m_hasValue; }
        }

        public T Value
        {
            get
            {
                Contract.ThrowIfFalse(HasValue);
                return m_value;
            }
        }

        public void SetValue(T value)
        {
            m_hasValue = true;
            m_value = value;
        }

        public void Clear()
        {
            m_hasValue = false;
            m_value = default(T);
        }
    }

}
