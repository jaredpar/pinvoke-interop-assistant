
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.IO;

namespace Parser
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
		public string Name {
			get { return _name; }
		}

		/// <summary>
		/// The TextReader
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public IO.TextReader TextReader {
			get { return _reader; }
		}

		public TextReaderBag(IO.TextReader reader) : this(Constants.UnknownFileName, reader)
		{
		}

		public TextReaderBag(string name, IO.TextReader reader)
		{
			this._name = name;
			this._reader = reader;
		}

	}

	internal class TriState<T>
	{
		public T m_value;

		public bool m_hasValue;
		public bool HasValue {
			get { return m_hasValue; }
		}

		public T Value {
			get {
				ThrowIfFalse(HasValue);
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
			m_value = null;
		}
	}

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
