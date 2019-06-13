// Copyright (c) Microsoft Corporation.  All rights reserved.

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
using PInvoke.Transform.Enums;

namespace PInvoke.Transform
{

    /// <summary>
    /// Used in C# when we need to pass a parameter in a specific direction (ref or out)
    /// </summary>
    /// <remarks></remarks>
    internal class CodeDirectionalSymbolExpression : CodeCustomExpression
    {
        private FieldDirection direction;
        public CodeExpression Expression { get; }

        private CodeDirectionalSymbolExpression(LanguageType lang, CodeExpression symbolExpr, FieldDirection direction) : base(lang)
        {
            Expression = symbolExpr;
            this.direction = direction;
        }

        [SuppressMessage("Microsoft.Security", "CA2122")]
        protected override void UpdateValue()
        {
            var provider = base.GetProvider();
            string expr = null;
            switch (direction)
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
                provider.GenerateCodeFromExpression(Expression, writer, new CodeGeneratorOptions());
                expr += writer.ToString();
            }

            Value = expr;
        }

        public static CodeExpression Create(LanguageType lang, CodeExpression symbolExpr, FieldDirection dir)
        {
            if (lang == LanguageType.VisualBasic)
            {
                return symbolExpr;
            }

            return new CodeDirectionalSymbolExpression(lang, symbolExpr, dir);
        }
    }

}
