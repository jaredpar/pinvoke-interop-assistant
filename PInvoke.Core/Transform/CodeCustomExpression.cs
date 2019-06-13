// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.CodeDom;
using System.CodeDom.Compiler;
using static PInvoke.Contract;
using PInvoke.Transform.Enums;

namespace PInvoke.Transform
{
    internal abstract class CodeCustomExpression : CodeSnippetExpression
    {


        private LanguageType language;
        protected LanguageType LanguageType
        {
            get { return language; }
            set
            {
                language = value;
                UpdateValue();
            }
        }

        protected CodeCustomExpression(LanguageType lang)
        {
            language = lang;
        }

        public void ResetValue()
        {
            UpdateValue();
        }

        protected abstract void UpdateValue();

        protected CodeDomProvider GetProvider()
        {
            switch (language)
            {
                case LanguageType.CSharp:
                    return new Microsoft.CSharp.CSharpCodeProvider();
                case LanguageType.VisualBasic:
                    return new Microsoft.VisualBasic.VBCodeProvider();
                default:
                    ThrowInvalidEnumValue(language);
                    return null;
            }
        }

    }

}
