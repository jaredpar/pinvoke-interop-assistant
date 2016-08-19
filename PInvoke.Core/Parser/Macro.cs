
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Text.RegularExpressions;

namespace Parser
{


	/// <summary>
	/// Represents a macro in native code
	/// </summary>
	/// <remarks></remarks>
	[DebuggerDisplay("{Name} -> {Value}")]
	public class Macro
	{
		private string _name;
		private string _value;
		private bool _isMethod;
		private bool _isPermanent;

		private bool _isFromParse = true;
		/// <summary>
		/// Name of the Macro
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public string Name {
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Value of the macro
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public string Value {
			get { return _value; }
			set { _value = value; }
		}

		/// <summary>
		/// Whether or not this is a method style macro
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public bool IsMethod {
			get { return _isMethod; }
			set { _isMethod = value; }
		}

		/// <summary>
		/// Represents a macro that cannot be overriden by user code.  
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public bool IsPermanent {
			get { return _isPermanent; }
			set { _isPermanent = value; }
		}

		/// <summary>
		/// Is this macro created from actually parsing code?  The alternate is that the 
		/// macro is added to the initial set of macros.  This allows the parser to determine
		/// what is actually a part of the parsed code as opposed to the setup code
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		internal bool IsFromParse {
			get { return _isFromParse; }
			set { _isFromParse = value; }
		}

		public Macro(string name)
		{
			_name = name;
		}

		public Macro(string name, string val) : this(name, val, false)
		{
		}

		public Macro(string name, string val, bool permanent)
		{
			_name = name;
			_value = val;
			_isPermanent = permanent;
		}

	}

	/// <summary>
	/// Macros that are methods
	/// </summary>
	/// <remarks></remarks>
	public class MethodMacro : Macro
	{

		private List<string> _paramList;
		private List<Token> _bodyList;

		private List<Token> _fullBodyList;
		/// <summary>
		/// Text parameters of the macro
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public List<string> Parameters {
			get { return _paramList; }
		}

		/// <summary>
		/// Tokens inside the macro body minus any whitespace characters
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public List<Token> Body {
			get { return _bodyList; }
		}

		/// <summary>
		/// Tokens inside the macro body including anywhitespace characters
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public List<Token> FullBody {
			get { return _fullBodyList; }
		}

		/// <summary>
		/// Get the text of the method signature
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public string MethodSignature {
			get {
				Text.StringBuilder b = new Text.StringBuilder();
				b.Append("(");
				for (int i = 0; i <= _paramList.Count - 1; i++) {
					b.Append(_paramList(i));
					if (i + 1 < _paramList.Count) {
						b.Append(",");
					}
				}
				b.Append(") ");
				foreach (Token cur in FullBody) {
					b.Append(cur.Value);
				}

				return b.ToString();
			}
		}

		public MethodMacro(string name, List<string> paramList, List<Token> body, List<Token> fullBody) : base(name)
		{
			base.Value = name + "()";
			base.IsMethod = true;

			_paramList = paramList;
			_bodyList = body;
			_fullBodyList = fullBody;
		}

		public List<Token> Replace(List<Token> argList)
		{
			if (argList.Count != _paramList.Count) {
				return new List<Token>();
			}

			// Replace is done in 2 passes.  The first puts the arguments into the token stream.
			List<Token> retList = new List<Token>();
			foreach (Token item in _bodyList) {
				if (item.TokenType != TokenType.Word) {
					retList.Add(item);
				} else {
					Int32 index = _paramList.IndexOf(item.Value);
					if (index >= 0) {
						retList.Add(argList(index));
					} else {
						retList.Add(item);
					}
				}
			}

			// Second phase, process all of the # entries 
			Int32 i = 0;
			while (i < retList.Count - 1) {
				Token curToken = retList(i);
				Token nextToken = retList(i + 1);

				if (curToken.TokenType == TokenType.Pound) {
					if (nextToken.TokenType == TokenType.Pound) {
						// Don't accidentally process a ## as a # token
						i += 1;
					} else if (argList.IndexOf(nextToken) >= 0) {
						if (nextToken.IsQuotedString) {
							// Already quoted so it doesn't need to be quoted again
							retList.RemoveAt(i);
							i += 1;
						} else {
							// Quote me macro
							retList(i) = new Token(TokenType.QuotedStringAnsi, "\"" + nextToken.Value + "\"");
							retList.RemoveAt(i + 1);
						}
					}
				}

				i += 1;
			}

			return retList;
		}

		public static bool TryCreateFromDeclaration(string name, string body, ref MethodMacro method)
		{
			try {
				PreProcessorEngine engine = new PreProcessorEngine(new PreProcessorOptions());
				using (IO.StringReader reader = new IO.StringReader("#define " + name + body)) {
					engine.Process(new TextReaderBag(reader));
					Macro created = null;
					if (engine.MacroMap.TryGetValue(name, created) && created.IsMethod) {
						method = (MethodMacro)created;
						return true;
					}
				}
			} catch {
				return false;
			}

			return false;
		}

	}
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
