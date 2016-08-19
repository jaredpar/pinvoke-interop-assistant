// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.IO;

namespace PInvoke.Transform
{

    internal abstract class CodeCustomExpression : CodeSnippetExpression
    {


        private LanguageType _lang;
        protected LanguageType LanguageType
        {
            get { return _lang; }
            set
            {
                _lang = value;
                UpdateValue();
            }
        }

        protected CodeCustomExpression(LanguageType lang)
        {
            _lang = lang;
        }

        public void ResetValue()
        {
            UpdateValue();
        }

        protected abstract void UpdateValue();

        protected CodeDomProvider GetProvider()
        {
            switch (_lang)
            {
                case Transform.LanguageType.CSharp:
                    return new Microsoft.CSharp.CSharpCodeProvider();
                case Transform.LanguageType.VisualBasic:
                    return new Microsoft.VisualBasic.VBCodeProvider();
                default:
                    InvalidEnumValue(_lang);
                    return null;
            }
        }

    }


    /// <summary>
    /// Used to perform a Not/! operation in a particular language
    /// </summary>C
    /// <remarks></remarks>
    internal class CodeNotExpression : CodeCustomExpression
    {


        private CodeExpression _expr;
        internal CodeExpression Expression
        {
            get { return _expr; }
            set
            {
                _expr = value;
                UpdateValue();
            }
        }


        internal CodeNotExpression(LanguageType lang, CodeExpression expr) : base(lang)
        {
            _expr = expr;
            UpdateValue();
        }

        [SuppressMessage("Microsoft.Security", "CA2122")]
        protected override void UpdateValue()
        {
            CodeDomProvider provider = GetProvider();
            string prefix = null;
            switch (this.LanguageType)
            {
                case Transform.LanguageType.CSharp:
                    prefix = "! (";
                    break;
                case Transform.LanguageType.VisualBasic:
                    prefix = "Not (";
                    break;
                default:
                    InvalidEnumValue(this.LanguageType);
                    return;
            }

            using (IO.StringWriter writer = new IO.StringWriter())
            {
                provider.GenerateCodeFromExpression(_expr, writer, new CodeGeneratorOptions());
                Value = prefix + writer.ToString() + ")";
            }
        }

    }

    /// <summary>
    /// Implements a bitshift expression
    /// </summary>
    /// <remarks></remarks>
    internal class CodeShiftExpression : CodeCustomExpression
    {

        private CodeExpression _leftExpr;
        private CodeExpression _rightExpr;

        private bool _shiftLeft;
        public CodeExpression Left
        {
            get { return _leftExpr; }
        }

        public CodeExpression Right
        {
            get { return _rightExpr; }
        }

        internal CodeShiftExpression(LanguageType lang, bool shiftLeft, CodeExpression left, CodeExpression right) : base(lang)
        {
            _leftExpr = left;
            _rightExpr = right;
            _shiftLeft = shiftLeft;
            UpdateValue();
        }

        [SuppressMessage("Microsoft.Security", "CA2122")]
        protected override void UpdateValue()
        {
            CodeDomProvider provider = base.GetProvider();
            string expr = "(";

            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(_leftExpr, writer, new CodeGeneratorOptions());
                expr += writer.ToString();
                expr += ")";
            }

            if (_shiftLeft)
            {
                expr += " << ";
            }
            else
            {
                expr += " >> ";
            }

            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(_rightExpr, writer, new CodeGeneratorOptions());
                expr += string.Format("({0})", writer.ToString());
            }

            Value = expr;
        }

    }


    /// <summary>
    /// Used to perform a - operation in a particular language
    /// </summary>C
    /// <remarks></remarks>
    internal class CodeNegativeExpression : CodeCustomExpression
    {


        private CodeExpression _expr;
        internal CodeExpression Expression
        {
            get { return _expr; }
            set
            {
                _expr = value;
                UpdateValue();
            }
        }


        internal CodeNegativeExpression(LanguageType lang, CodeExpression expr) : base(lang)
        {
            _expr = expr;
            UpdateValue();
        }

        [SuppressMessage("Microsoft.Security", "CA2122")]
        protected override void UpdateValue()
        {
            CodeDomProvider provider = GetProvider();
            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(_expr, writer, new CodeGeneratorOptions());
                Value = "-" + writer.ToString();
            }
        }

    }

    /// <summary>
    /// Used in C# when we need to pass a parameter in a specific direction (ref or out)
    /// </summary>
    /// <remarks></remarks>
    internal class CodeDirectionalSymbolExpression : CodeCustomExpression
    {

        private CodeExpression _symbolExpr;

        private FieldDirection _direction;
        public CodeExpression Expression
        {
            get { return _symbolExpr; }
        }

        private CodeDirectionalSymbolExpression(LanguageType lang, CodeExpression symbolExpr, FieldDirection direction) : base(lang)
        {
            _symbolExpr = symbolExpr;
            _direction = direction;
        }

        [SuppressMessage("Microsoft.Security", "CA2122")]
        protected override void UpdateValue()
        {
            CodeDomProvider provider = base.GetProvider();
            string expr = null;
            switch (_direction)
            {
                case FieldDirection.Out:
                    expr = "out ";
                    break;
                case FieldDirection.Ref:
                    expr = "ref ";
                    break;
                default:
                    expr = string.Empty;
                    break;
            }

            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(_symbolExpr, writer, new CodeGeneratorOptions());
                expr += writer.ToString();
            }

            Value = expr;
        }

        public static CodeExpression Create(LanguageType lang, CodeExpression symbolExpr, FieldDirection dir)
        {
            if (lang == Transform.LanguageType.VisualBasic)
            {
                return symbolExpr;
            }

            return new CodeDirectionalSymbolExpression(lang, symbolExpr, dir);
        }
    }

}
