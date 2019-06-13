// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using static PInvoke.Contract;
using PInvoke.Transform.Enums;

namespace PInvoke.Transform
{
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
                case LanguageType.CSharp:
                    prefix = "! (";
                    break;
                case LanguageType.VisualBasic:
                    prefix = "Not (";
                    break;
                default:
                    ThrowInvalidEnumValue(this.LanguageType);
                    return;
            }

            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(Expression, writer, new CodeGeneratorOptions());
                Value = prefix + writer.ToString() + ")";
            }
        }

    }

}
