// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.Text;
using PInvoke;
using PInvoke.Transform;
using Xunit;
using System.CodeDom.Compiler;
using System.IO;

namespace PInvoke.Test
{

    public static class CodeDomPrinter
    {

        public static string ConvertNoNamespace(CodeTypeReference @ref)
        {
            string name = @ref.BaseType;
            int index = name.LastIndexOf(".");
            if (index < 0)
            {
                return name;
            }

            name = name.Substring(index + 1);
            if (@ref.ArrayRank > 0)
            {
                name += "(" + @ref.ArrayRank + ")";
            }

            return name;
        }

        public static string Convert(CodeTypeReference type)
        {
            string name = type.BaseType;
            if (type.ArrayRank > 0)
            {
                name += "(" + type.ArrayRank + ")";
            }

            return name;
        }

        public static string Convert(CodeExpression expr)
        {
            CodePrimitiveExpression primitiveExpr = expr as CodePrimitiveExpression;
            if (primitiveExpr != null)
            {
                return primitiveExpr.Value.ToString();
            }

            CodeFieldReferenceExpression fieldExpr = expr as CodeFieldReferenceExpression;
            if (fieldExpr != null)
            {
                return string.Format("{0}.{1}", Convert(fieldExpr.TargetObject), fieldExpr.FieldName);
            }

            CodeTypeReferenceExpression typeExpr = expr as CodeTypeReferenceExpression;
            if (typeExpr != null)
            {
                return ConvertNoNamespace(typeExpr.Type);
            }

            CodeBinaryOperatorExpression opExpr = expr as CodeBinaryOperatorExpression;
            if (opExpr != null)
            {
                return string.Format("{0}({1})({2})", opExpr.Operator, Convert(opExpr.Left), Convert(opExpr.Right));
            }

            return expr.ToString();
        }

        public static string Convert(CodeAttributeDeclarationCollection col)
        {
            string str = string.Empty;
            bool first = true;
            foreach (CodeAttributeDeclaration decl in col)
            {
                if (!first)
                {
                    str += ",";
                }

                str += Convert(decl);
                first = false;
            }

            return str;
        }

        public static string Convert(CodeAttributeDeclaration decl)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("{0}(", ConvertNoNamespace(decl.AttributeType));

            bool first = true;
            foreach (CodeAttributeArgument arg in decl.Arguments)
            {
                if (!first)
                {
                    builder.Append(",");
                }
                if (string.IsNullOrEmpty(arg.Name))
                {
                    builder.Append(Convert(arg.Value));
                }
                else
                {
                    builder.AppendFormat("{0}={1}", arg.Name, Convert(arg.Value));
                }
                first = false;
            }

            builder.Append(")");
            return builder.ToString();
        }

        public static string Convert(CodeMemberMethod method)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(method.Name);
            builder.Append("(");

            bool isFirst = true;
            foreach (CodeParameterDeclarationExpression param in method.Parameters)
            {
                if (!isFirst)
                {
                    builder.Append(",");
                }

                isFirst = false;
                builder.Append(Convert(param.CustomAttributes));
                switch (param.Direction)
                {
                    case FieldDirection.In:
                        builder.Append("In ");
                        break;
                    case FieldDirection.Out:
                        builder.Append("Out ");
                        break;
                    case FieldDirection.Ref:
                        builder.Append("Ref ");
                        break;
                }
                builder.Append(ConvertNoNamespace(param.Type));
            }

            builder.Append(")");
            if (method.ReturnType != null)
            {
                builder.Append(" As ");
                builder.Append(Convert(method.ReturnTypeCustomAttributes));
                builder.Append(ConvertNoNamespace(method.ReturnType));
            }

            return builder.ToString();
        }

        public static string Convert(CodeMemberField cField)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Convert(cField.CustomAttributes));
            builder.Append(Convert(cField.Type));
            builder.Append(" ");
            builder.Append(cField.Name);
            return builder.ToString();
        }

    }

    public static class SymbolPrinter
    {
        public static string Convert(NativeSymbol sym)
        {
            string str = sym.Name;
            foreach (NativeSymbol child in sym.GetChildren())
            {
                str += "(" + Convert(child) + ")";
            }

            return str;
        }
    }

    public static class StorageFactory
    {
        /// <summary>
        /// Used to create a simple set of types that can be used for testing purposes
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static BasicSymbolStorage CreateStandard()
        {
            var storage = new BasicSymbolStorage();
            NativePointer pt1 = default(NativePointer);
            NativeTypeDef td1 = default(NativeTypeDef);
            NativeTypeDef td2 = default(NativeTypeDef);
            NativeStruct s1 = default(NativeStruct);
            NativeUnion u1 = default(NativeUnion);
            NativeNamedType n1 = default(NativeNamedType);

            // Include sal information
            List<NativeConstant> list = ProcessSal();
            foreach (NativeConstant cur in list)
            {
                storage.AddConstant(cur);
            }

            // Bool types
            storage.AddTypeDef(new NativeTypeDef("BOOL", BuiltinType.NativeInt32));
            storage.AddTypeDef(new NativeTypeDef("DWORD", new NativeBuiltinType(BuiltinType.NativeInt32, true)));

            // WPARAM 
            td1 = new NativeTypeDef("UINT_PTR", new NativeBuiltinType(BuiltinType.NativeInt32, true));
            storage.AddTypeDef(new NativeTypeDef("WPARAM", td1));
            storage.AddTypeDef(new NativeTypeDef("LPARAM", td1));

            // WCHAR
            NativeTypeDef wcharTd = new NativeTypeDef("WCHAR", new NativeBuiltinType(BuiltinType.NativeInt16, true));
            storage.AddTypeDef(wcharTd);

            // CHAR
            td1 = new NativeTypeDef("CHAR", BuiltinType.NativeChar);
            storage.AddTypeDef(td1);

            // TCHAR
            td2 = new NativeTypeDef("TCHAR", td1);
            storage.AddTypeDef(td2);

            // LPWSTR
            pt1 = new NativePointer(wcharTd);
            td2 = new NativeTypeDef("LPWSTR", pt1);
            storage.AddTypeDef(td2);

            // LPCWSTR
            n1 = new NativeNamedType(wcharTd.Name, wcharTd);
            n1.IsConst = true;
            pt1 = new NativePointer(n1);
            td2 = new NativeTypeDef("LPCWSTR", pt1);
            storage.AddTypeDef(td2);

            // LPSTR
            pt1 = new NativePointer(new NativeBuiltinType(BuiltinType.NativeChar));
            td1 = new NativeTypeDef("LPSTR", pt1);
            storage.AddTypeDef(td1);

            // LPTSTR
            storage.AddTypeDef(new NativeTypeDef("LPTSTR", td1));

            // LPCSTR
            n1 = new NativeNamedType("char", true);
            n1.RealType = new NativeBuiltinType(BuiltinType.NativeChar, false);
            pt1 = new NativePointer(n1);
            td1 = new NativeTypeDef("LPCSTR", pt1);
            storage.AddTypeDef(td1);

            // LPCTSTR
            td2 = new NativeTypeDef("LPCTSTR", td1);
            storage.AddTypeDef(td2);

            // BSTR
            storage.AddTypeDef(new NativeTypeDef("OLECHAR", BuiltinType.NativeWChar));
            storage.AddTypeDef(new NativeTypeDef("BSTR", new NativePointer(new NativeTypeDef("OLECHAR", BuiltinType.NativeWChar))));

            // Struct with a recrsive reference to itself
            s1 = new NativeStruct("RecursiveStruct");
            s1.Members.Add(new NativeMember("m1", new NativePointer(new NativeNamedType(s1.Name))));
            storage.AddDefinedType(s1);

            // Simple struct
            s1 = new NativeStruct("S1");
            s1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            storage.AddDefinedType(s1);

            // Simulate a few well known structures

            // DECIMAL
            s1 = new NativeStruct("tagDEC");
            storage.AddDefinedType(s1);
            storage.AddTypeDef(new NativeTypeDef("DECIMAL", s1));

            // CURRENCY
            u1 = new NativeUnion("tagCY");
            storage.AddDefinedType(u1);
            storage.AddTypeDef(new NativeTypeDef("CY", u1));
            storage.AddTypeDef(new NativeTypeDef("CURRENCY", new NativeTypeDef("CY", u1)));

            // BYTE
            storage.AddTypeDef(new NativeTypeDef("BYTE", new NativeBuiltinType(BuiltinType.NativeChar, true)));

            return storage;
        }

        private static List<NativeConstant> ProcessSal()
        {
            Parser.NativeCodeAnalyzer analyzer = Parser.NativeCodeAnalyzerFactory.Create(Parser.OsVersion.WindowsVista);
            Parser.NativeCodeAnalyzerResult result = analyzer.Analyze("SampleFiles\\specstrings.h");
            return result.ConvertMacrosToConstants();
        }
    }

    public static class GeneratedCodeVerification
    {

        public static void VerifyExpression(string nativeExpr, string managedExpr, NativeSymbolBag bag = null)
        {
            VerifyExpression(LanguageType.VisualBasic, nativeExpr, managedExpr, bag);
        }

        public static void VerifyExpression(string nativeExpr, string managedExpr, string managedType, NativeSymbolBag bag = null)
        {
            VerifyExpression(LanguageType.VisualBasic, nativeExpr, managedExpr, managedType, bag);
        }

        public static void VerifyCSharpExpression(string nativeExpr, string managedExpr, string managedType, NativeSymbolBag bag = null)
        {
            VerifyExpression(LanguageType.CSharp, nativeExpr, managedExpr, managedType, bag);
        }

        public static void VerifyExpression(LanguageType lang, string nativeExpr, string managedExpr, NativeSymbolBag bag = null)
        {
            VerifyExpression(lang, nativeExpr, managedExpr, null, bag);
        }

        public static void VerifyExpression(LanguageType lang, string nativeExpr, string managedExpr, string managedType, NativeSymbolBag bag)
        {
            bag = bag ?? new NativeSymbolBag();
            CodeTransform trans = new CodeTransform(lang, bag);
            NativeValueExpression nExpr = new NativeValueExpression(nativeExpr);
            CodeExpression cExpr = null;
            CodeTypeReference codeType = null;
            Exception ex = null;

            Assert.True(trans.TryGenerateValueExpression(nExpr, out cExpr, out codeType, out ex));

            var provider = default(System.CodeDom.Compiler.CodeDomProvider);
            switch (lang)
            {
                case LanguageType.CSharp:
                    provider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case LanguageType.VisualBasic:
                    provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
                default:
                    provider = null;
                    break;
            }

            Assert.NotNull(provider);
            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(cExpr, writer, new System.CodeDom.Compiler.CodeGeneratorOptions());
                Assert.Equal(managedExpr, writer.ToString());
            }

            if (managedType != null)
            {
                Assert.Equal(managedType, CodeDomPrinter.Convert(codeType));
            }
        }

        public static void VerifyConstValue(string code, string name, string val, string type)
        {
            VerifyConstValue(code, LanguageType.CSharp, name, val, type);
        }

        public static void VerifyConstValue(string code, LanguageType lang, string name, string val, string type)
        {
            CodeTypeDeclarationCollection col = ConvertToCodeDom(code);
            VerifyConstValue(col, lang, name, val, type);
        }

        public static void VerifyConstValue(NativeSymbolBag bag, string name, string val)
        {
            VerifyConstValue(LanguageType.VisualBasic, bag, name, val);
        }

        public static void VerifyConstValue(NativeSymbolBag bag, string name, string val, string type)
        {
            VerifyConstValue(LanguageType.VisualBasic, bag, name, val, type);
        }

        public static void VerifyConstValue(LanguageType lang, NativeSymbolBag bag, string name, string val)
        {
            VerifyConstValue(lang, bag, name, val, null);
        }

        public static void VerifyConstValue(LanguageType lang, NativeSymbolBag bag, string name, string val, string type)
        {
            Assert.True(bag.TryResolveSymbolsAndValues());

            BasicConverter con = new BasicConverter(lang, StorageFactory.CreateStandard());
            CodeTypeDeclarationCollection col = con.ConvertToCodeDom(bag, new ErrorProvider());

            VerifyConstValue(col, lang, name, val, type);
        }


        public static void VerifyConstValue(CodeTypeDeclarationCollection col, LanguageType lang, string name, string val, string type)
        {
            // Look for the constants class
            CodeTypeDeclaration ctd = null;
            VerifyType(col, TransformConstants.NativeConstantsName, ref ctd);

            // Find the value
            CodeTypeMember cMem = null;
            VerifyMember(ctd, name, ref cMem);

            // Make sure it's a constant value
            CodeMemberField field = cMem as CodeMemberField;
            Assert.NotNull(field);

            // Get the provider
            var provider = default(CodeDomProvider);
            switch (lang)
            {
                case LanguageType.CSharp:
                    provider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case LanguageType.VisualBasic:
                    provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
                default:
                    provider = null;
                    break;
            }

            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(field.InitExpression, writer, new CodeGeneratorOptions());
                Assert.Equal(val, writer.ToString());
            }

            if (type != null)
            {
                Assert.Equal(type, CodeDomPrinter.Convert(field.Type));
            }
        }

        public static void VerifyEnumValue(NativeSymbolBag bag, NativeEnum e, string name, string val)
        {
            VerifyEnumValue(LanguageType.VisualBasic, bag, e, name, val);
        }

        public static void VerifyEnumValue(LanguageType lang, NativeSymbolBag bag, NativeEnum e, string name, string val)
        {
            Assert.True(bag.TryResolveSymbolsAndValues());

            BasicConverter con = new BasicConverter(lang, StorageFactory.CreateStandard());
            CodeTypeDeclarationCollection col = con.ConvertToCodeDom(bag, new ErrorProvider());

            // Look for the constants class
            CodeTypeDeclaration ctd = null;
            foreach (CodeTypeDeclaration cur in col)
            {
                if (0 == string.CompareOrdinal(e.Name, cur.Name))
                {
                    ctd = cur;
                    break;
                }
            }
            Assert.NotNull(ctd);

            // Find the value
            CodeTypeMember cMem = null;
            foreach (CodeTypeMember mem in ctd.Members)
            {
                if (0 == string.CompareOrdinal(name, mem.Name))
                {
                    cMem = mem;
                    break;
                }
            }
            Assert.NotNull(cMem);

            // Make sure it's a constant value
            CodeMemberField field = cMem as CodeMemberField;
            Assert.NotNull(field);

            // Get the provider
            var provider = default(CodeDomProvider);
            switch (lang)
            {
                case LanguageType.CSharp:
                    provider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case LanguageType.VisualBasic:
                    provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
                default:
                    provider = null;
                    break;
            }

            using (var writer = new StringWriter())
            {
                provider.GenerateCodeFromExpression(field.InitExpression, writer, new CodeGeneratorOptions());
                Assert.Equal(val, writer.ToString());
            }
        }

        private static CodeTypeDeclarationCollection ConvertToCodeDom(string code)
        {
            ErrorProvider ep = new ErrorProvider();
            BasicConverter con = new BasicConverter(LanguageType.VisualBasic, StorageFactory.CreateStandard());
            CodeTypeDeclarationCollection result = con.ConvertNativeCodeToCodeDom(code, ep);
            Assert.Equal(0, ep.Errors.Count);
            return result;
        }

        public static void VerifyType(CodeTypeDeclarationCollection col, string name, ref CodeTypeDeclaration ctd)
        {
            ctd = null;
            foreach (CodeTypeDeclaration cur in col)
            {
                if (0 == string.CompareOrdinal(cur.Name, name))
                {
                    ctd = cur;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }

            if (ctd == null)
            {
                string msg = "Could not find a type named " + name + ".  Found: ";
                foreach (CodeTypeDeclaration type in col)
                {
                    msg += type.Name + " ";
                }
                throw new Exception(msg);
            }
        }

        private static List<CodeMemberMethod> ConvertToProc(string code)
        {
            CodeTypeDeclarationCollection col = ConvertToCodeDom(code);
            List<CodeMemberMethod> list = new List<CodeMemberMethod>();
            CodeTypeDeclaration ctd = null;

            VerifyType(col, "NativeMethods", ref ctd);
            foreach (CodeTypeMember mem in ctd.Members)
            {
                CodeMemberMethod method = mem as CodeMemberMethod;
                if (method != null)
                {
                    list.Add(method);
                }
            }

            return list;
        }

        private static CodeMemberMethod ConvertToSingleProc(string code, string name)
        {
            CodeMemberMethod found = null;
            foreach (CodeMemberMethod cur in ConvertToProc(code))
            {
                if (string.Equals(name, cur.Name, StringComparison.Ordinal))
                {
                    found = cur;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }

            Assert.NotNull(found);
            return found;
        }

        private static bool VerifyProcImpl(string code, string sig, ref string all)
        {
            all = string.Empty;
            foreach (CodeMemberMethod method in ConvertToProc(code))
            {
                string p = CodeDomPrinter.Convert(method);
                if (0 == string.CompareOrdinal(sig, p))
                {
                    return true;
                }
                else
                {
                    all += Environment.NewLine;
                    all += p;
                }
            }

            return false;
        }

        public static void VerifyProc(string code, params string[] sigArray)
        {
            string all = string.Empty;
            foreach (string sig in sigArray)
            {
                bool ret = VerifyProcImpl(code, sig, ref all);
                Assert.True(ret, "Could not find the method. Looking For :" + sig + PortConstants.NewLine + "Found:" + all);
            }
        }

        public static void VerifyNotProc(string code, string sig)
        {
            string all = string.Empty;
            bool ret = VerifyProcImpl(code, sig, ref all);
            Assert.True(!ret, "Found a matching method");
        }

        public static void VerifyMember(CodeTypeDeclaration ctd, string name, ref CodeTypeMember cMem)
        {
            // Find the value
            cMem = null;
            foreach (CodeTypeMember mem in ctd.Members)
            {
                if (0 == string.CompareOrdinal(name, mem.Name))
                {
                    cMem = mem;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }

            Assert.NotNull(cMem);
        }

        public static void VerifyAttribute(CodeAttributeDeclarationCollection col, Type type, ref CodeAttributeDeclaration decl)
        {
            VerifyAttributeImpl(col, type, ref decl);
            Assert.NotNull(decl);
        }

        public static void VerifyNoAttribute(CodeAttributeDeclarationCollection col, Type type)
        {
            CodeAttributeDeclaration decl = null;
            VerifyAttributeImpl(col, type, ref decl);
            Assert.Null(decl);
        }

        private static void VerifyAttributeImpl(CodeAttributeDeclarationCollection col, Type type, ref CodeAttributeDeclaration decl)
        {
            decl = null;

            string name = type.FullName;
            foreach (CodeAttributeDeclaration cur in col)
            {
                if (string.Equals(name, cur.Name, StringComparison.Ordinal))
                {
                    decl = cur;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }

        }

        private static void VerifyArgumentImpl(CodeAttributeDeclaration decl, string name, ref CodeAttributeArgument arg)
        {
            arg = null;
            foreach (CodeAttributeArgument cur in decl.Arguments)
            {
                if (string.Equals(name, cur.Name, StringComparison.Ordinal))
                {
                    arg = cur;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        public static void VerifyArgument(CodeAttributeDeclaration decl, string name, ref CodeAttributeArgument arg)
        {
            VerifyArgumentImpl(decl, name, ref arg);
            Assert.NotNull(arg);
        }

        public static void VerifyNoArgument(CodeAttributeDeclaration decl, string name)
        {
            CodeAttributeArgument arg = null;
            VerifyArgumentImpl(decl, name, ref arg);
            Assert.Null(arg);
        }

        public static void VerifyField(CodeTypeDeclaration ctd, string name, ref CodeMemberField cField)
        {
            CodeTypeMember cMem = null;
            VerifyMember(ctd, name, ref cMem);
            cField = cMem as CodeMemberField;
            Assert.NotNull(cField);
        }

        public static void VerifyField(CodeTypeDeclaration ctd, string name, string value)
        {
            CodeMemberField cField = null;
            VerifyField(ctd, name, ref cField);
            Assert.Equal(value, CodeDomPrinter.Convert(cField));
        }

        public static void VerifyTypeMembers(string code, string name, params string[] members)
        {
            CodeTypeDeclarationCollection col = ConvertToCodeDom(code);
            CodeTypeDeclaration ctd = null;
            VerifyType(col, name, ref ctd);

            for (int i = 0; i <= members.Length - 1; i += 2)
            {
                VerifyField(ctd, members[i], members[i + 1]);
            }

        }

        public static void VerifyProcCallingConvention(string code, string name, System.Runtime.InteropServices.CallingConvention conv)
        {
            CodeMemberMethod mem = ConvertToSingleProc(code, name);
            CodeAttributeDeclaration decl = null;
            VerifyAttribute(mem.CustomAttributes, typeof(System.Runtime.InteropServices.DllImportAttribute), ref decl);
            if (conv == System.Runtime.InteropServices.CallingConvention.Winapi)
            {
                VerifyNoArgument(decl, "CallingConvention");
            }
            else
            {
                CodeAttributeArgument arg = null;
                VerifyArgument(decl, "CallingConvention", ref arg);
                Assert.Equal("CallingConvention." + conv.ToString(), CodeDomPrinter.Convert(arg.Value));
            }
        }

        public static void VerifyFPtrCallingConvention(string code, string name, System.Runtime.InteropServices.CallingConvention conv)
        {
            CodeTypeDeclarationCollection col = ConvertToCodeDom(code);
            CodeTypeDeclaration type = null;
            VerifyType(col, name, ref type);


            if (conv != System.Runtime.InteropServices.CallingConvention.Winapi)
            {
                CodeAttributeDeclaration decl = null;
                VerifyAttribute(type.CustomAttributes, typeof(System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute), ref decl);

                CodeAttributeArgument arg = null;
                VerifyArgument(decl, string.Empty, ref arg);
                Assert.Equal("CallingConvention." + conv.ToString(), CodeDomPrinter.Convert(arg.Value));
            }
            else
            {
                VerifyNoAttribute(type.CustomAttributes, typeof(System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute));
            }
        }

    }
}
