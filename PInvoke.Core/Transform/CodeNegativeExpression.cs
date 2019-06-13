// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using PInvoke.Transform.Enums;

namespace PInvoke.Transform
{
    /// <summary>
    /// Used to perform a - operation in a particular language
    /// </summary>C
    /// <remarks></remarks>
    internal class CodeNegativeExpression : CodeCustomExpression
    {


        private CodeExpression expression;
        internal CodeExpression Expression
        {
            get { return expression; }
            set
            {
                expression = value;
                UpdateValue();
            }
        }


        internal CodeNegativeExpression(LanguageType lang, CodeExpression expression) : base(lang)
        {
            this.expression = expression;
            UpdateValue();
        }

        [SuppressMessage("Microsoft.Security", "CA2122")]
        protected override void UpdateValue()
        {
            var provider = GetProvider();
            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(Expression, writer, new CodeGeneratorOptions());
                Value = "-" + writer.ToString();
            }
        }

    }

}
