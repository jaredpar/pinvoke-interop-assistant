// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using PInvoke.Transform.Enums;

namespace PInvoke.Transform
{
    /// <summary>
    /// Implements a bitshift expression
    /// </summary>
    /// <remarks></remarks>
    internal class CodeShiftExpression : CodeCustomExpression
    {
        private bool shiftLeft;
        public CodeExpression Left { get; }

        public CodeExpression Right { get; }

        internal CodeShiftExpression(LanguageType lang, bool shiftLeft, CodeExpression left, CodeExpression right) : base(lang)
        {
            Left = left;
            Right = right;
            this.shiftLeft = shiftLeft;
            UpdateValue();
        }

        [SuppressMessage("Microsoft.Security", "CA2122")]
        protected override void UpdateValue()
        {
            CodeDomProvider provider = base.GetProvider();
            string expr = "(";

            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(Left, writer, new CodeGeneratorOptions());
                expr += writer.ToString();
                expr += ")";
            }

            if (shiftLeft)
            {
                expr += " << ";
            }
            else
            {
                expr += " >> ";
            }

            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(Right, writer, new CodeGeneratorOptions());
                expr += string.Format("({0})", writer.ToString());
            }

            Value = expr;
        }

    }

}
