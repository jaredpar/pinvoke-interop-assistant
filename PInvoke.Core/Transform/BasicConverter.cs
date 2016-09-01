
// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using PInvoke;
using PInvoke.Parser;
using PInvoke.Transform;
using static PInvoke.Contract;
using System.CodeDom.Compiler;
using System.Text;

namespace PInvoke.Transform
{

    /// <summary>
    /// Wraps a lot of the functionality into a simple few method wrapper
    /// </summary>
    /// <remarks></remarks>
    public class BasicConverter
    {
        private NativeStorage _ns;
        private LanguageType _type;

        private TransformKindFlags _transformKind = TransformKindFlags.All;
        /// <summary>
        /// Native storage to use when resolving types
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeStorage NativeStorage
        {
            get { return _ns; }
            set { _ns = value; }
        }

        /// <summary>
        /// Language to generate into
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public LanguageType LanguageType
        {
            get { return _type; }
            set { _type = value; }
        }

        public TransformKindFlags TransformKindFlags
        {
            get { return _transformKind; }
            set { _transformKind = value; }
        }

        public BasicConverter() : this(LanguageType.VisualBasic, NativeStorage.DefaultInstance)
        {
        }

        public BasicConverter(LanguageType type) : this(type, NativeStorage.DefaultInstance)
        {
        }

        public BasicConverter(LanguageType type, NativeStorage ns)
        {
            _ns = ns;
            _type = type;
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeConstant c, ErrorProvider ep)
        {
            NativeSymbolBag bag = new NativeSymbolBag(_ns);
            bag.AddConstant(c);
            return ConvertBagToCodeDom(bag, ep);
        }

        public string ConvertToPInvokeCode(NativeConstant c)
        {
            ErrorProvider ep = new ErrorProvider();
            CodeTypeDeclarationCollection col = ConvertToCodeDom(c, ep);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeTypeDef typedef, ErrorProvider ep)
        {
            NativeSymbolBag bag = new NativeSymbolBag(_ns);
            bag.AddTypeDef(typedef);
            return ConvertBagToCodeDom(bag, ep);
        }

        public string ConvertToPInvokeCode(NativeTypeDef typedef)
        {
            ErrorProvider ep = new ErrorProvider();
            CodeTypeDeclarationCollection col = ConvertToCodeDom(typedef, ep);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeDefinedType definedNt, ErrorProvider ep)
        {
            NativeSymbolBag bag = new NativeSymbolBag(_ns);
            bag.AddDefinedType(definedNt);
            return ConvertBagToCodeDom(bag, ep);
        }

        public string ConvertToPInvokeCode(NativeDefinedType definedNt)
        {
            ErrorProvider ep = new ErrorProvider();
            CodeTypeDeclarationCollection col = ConvertToCodeDom(definedNt, ep);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeProcedure proc, ErrorProvider ep)
        {
            NativeSymbolBag bag = new NativeSymbolBag(_ns);
            bag.AddProcedure(proc);
            return ConvertBagToCodeDom(bag, ep);
        }

        public string ConvertToPInvokeCode(NativeProcedure proc)
        {
            ErrorProvider ep = new ErrorProvider();
            CodeTypeDeclarationCollection col = ConvertToCodeDom(proc, ep);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeSymbolBag bag, ErrorProvider ep)
        {
            return ConvertBagToCodeDom(bag, ep);
        }

        public string ConvertToPInvokeCode(NativeSymbolBag bag)
        {
            ErrorProvider ep = new ErrorProvider();
            return ConvertBagToPInvokeCodeImpl(bag, ep);
        }

        /// <summary>
        /// Convert the block of Native code into PInvoke code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ConvertNativeCodeToPInvokeCode(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            ErrorProvider ep = new ErrorProvider();
            CodeTypeDeclarationCollection col = ConvertNativeCodeToCodeDom(code, ep);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        /// <summary>
        /// Convert the block of native code into a CodeDom hierarchy
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ep"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeDeclarationCollection ConvertNativeCodeToCodeDom(string code, ErrorProvider ep)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            if (ep == null)
            {
                throw new ArgumentNullException("ep");
            }

            NativeCodeAnalyzer analyzer = NativeCodeAnalyzerFactory.CreateForMiniParse(OsVersion.WindowsVista, _ns.LoadAllMacros());
            // CTODO: probably should delete this 
            analyzer.IncludePathList.Add("c:\\program files (x86)\\windows kits\\8.1\\include\\shared");
            NativeSymbolBag bag = default(NativeSymbolBag);
            using (System.IO.StringReader reader = new StringReader(code))
            {
                NativeCodeAnalyzerResult result = analyzer.Analyze(reader);

                ep.Append(result.ErrorProvider);
                bag = NativeSymbolBag.CreateFrom(result, _ns);
            }

            return ConvertBagToCodeDom(bag, ep);
        }

        private string ConvertBagToPInvokeCodeImpl(NativeSymbolBag bag, ErrorProvider ep)
        {
            CodeTypeDeclarationCollection col = ConvertBagToCodeDom(bag, ep);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public string ConvertCodeDomToPInvokeCode(CodeTypeDeclaration ctd)
        {
            CodeTypeDeclarationCollection col = new CodeTypeDeclarationCollection();
            col.Add(ctd);
            return ConvertCodeDomToPInvokeCodeImpl(col, new ErrorProvider());
        }

        public string ConvertCodeDomToPInvokeCode(CodeTypeDeclarationCollection col)
        {
            return ConvertCodeDomToPInvokeCodeImpl(col, new ErrorProvider());
        }

        public string ConvertCodeDomToPInvokeCode(CodeTypeDeclarationCollection col, ErrorProvider ep)
        {
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public string ConvertCodeDomToPInvokeCodeImpl(CodeTypeDeclarationCollection col, ErrorProvider ep)
        {
            return ConvertCodeDomToPInvokeCodeImpl(_type, col, ep);
        }

        public static string ConvertCodeDomToPInvokeCodeImpl(LanguageType type, CodeTypeDeclarationCollection col, ErrorProvider ep)
        {
            ThrowIfNull(col);
            ThrowIfNull(ep);

            StringWriter writer = new StringWriter();
            CodeDomProvider provider = default(CodeDomProvider);
            string commentStart = null;

            // Generate based on the language
            switch (type)
            {
                case Transform.LanguageType.VisualBasic:
                    commentStart = "'";
                    provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
                case Transform.LanguageType.CSharp:
                    commentStart = "//";
                    provider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                default:
                    ThrowInvalidEnumValue(type);
                    return string.Empty;
            }

            foreach (string warning in ep.Warnings)
            {
                writer.WriteLine("{0} Warning: {1}", commentStart, warning);
            }

            foreach (string err in ep.Errors)
            {
                writer.WriteLine("{0} Error: {1}", commentStart, err);
            }

            foreach (CodeTypeDeclaration ctd in col)
            {
                provider.GenerateCodeFromMember(ctd, writer, new CodeGeneratorOptions());
            }

            if (type == Transform.LanguageType.CSharp)
            {
                // CSharp specific fixup
                return FixupCSharpCode(writer.ToString());
            }
            else
            {
                return writer.ToString();
            }

        }

        /// <summary>
        /// Core conversion routine.  All code should just go through this 
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="ep"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private CodeTypeDeclarationCollection ConvertBagToCodeDom(NativeSymbolBag bag, ErrorProvider ep)
        {
            ThrowIfNull(bag);
            ThrowIfNull(ep);

            // Make sure than all of the referenced NativeDefinedType instances are in the correct
            // portion of the bag
            ChaseReferencedDefinedTypes(bag);

            // First step is to resolve the symbols
            bag.TryResolveSymbolsAndValues(ep);

            // Create the codedom transform
            CodeTransform transform = new CodeTransform(this._type, bag);
            MarshalTransform marshalUtil = new MarshalTransform(this._type, bag, _transformKind);
            CodeTypeDeclarationCollection col = new CodeTypeDeclarationCollection();

            // Only output the constants if there are actually any
            List<NativeConstant> list = new List<NativeConstant>(bag.FindResolvedConstants());
            if (list.Count > 0)
            {
                CodeTypeDeclaration constCtd = transform.GenerateConstants(list);
                if (constCtd.Members.Count > 0)
                {
                    col.Add(constCtd);
                }
            }

            foreach (NativeDefinedType definedNt in bag.FindResolvedDefinedTypes())
            {
                CodeTypeDeclaration ctd = transform.GenerateDeclaration(definedNt);
                marshalUtil.Process(ctd);
                col.Add(ctd);
            }

            List<NativeProcedure> procList = new List<NativeProcedure>(bag.FindResolvedProcedures());
            if (procList.Count > 0)
            {
                CodeTypeDeclaration procType = transform.GenerateProcedures(procList);
                marshalUtil.Process(procType);
                col.Add(procType);
            }

            // Add the helper types that we need
            AddHelperTypes(col);

            // Next step is to run the pretty lister on it
            CodeDomPrettyList prettyLister = new CodeDomPrettyList(bag);
            prettyLister.PerformRename(col);

            return col;
        }

        /// <summary>
        /// Make sure that any NativeDefinedType referenced is in the bag.  That way if we 
        /// have structures which point to other NativeDefinedType instances, they are automagically
        /// put into the bag 
        /// </summary>
        /// <param name="bag"></param>
        /// <remarks></remarks>
        private void ChaseReferencedDefinedTypes(NativeSymbolBag bag)
        {
            bag.TryResolveSymbolsAndValues();

            foreach (NativeSymbol sym in bag.FindAllReachableNativeSymbols())
            {
                if (NativeSymbolCategory.Defined == sym.Category)
                {
                    NativeDefinedType defined = null;
                    if (!bag.TryFindDefined(sym.Name, out defined))
                    {
                        bag.AddDefinedType((NativeDefinedType)sym);
                    }
                }
            }

        }

        /// <summary>
        /// The CodeDom cannot directly output CSharp PInvoke code because it does not support the 
        /// extern keyword which is how CSHarp defines it's PInvoke code headers.  We need to fixup
        /// the method signatures for the PInvoke methods here 
        /// 
        /// We have to be careful though to avoid wrapper methods
        /// </summary>
        /// <param name="code"></param>
        private static string FixupCSharpCode(string code)
        {
            var builder = new StringBuilder();
            using (var reader = new StringReader(code))
            {
                string line = reader.ReadLine();

                while (line != null)
                {
                    // Look for the DLLImport line
                    if (Regex.IsMatch(line, "^\\s*\\[System\\.Runtime\\.InteropServices\\.DllImport.*$"))
                    {
                        builder.AppendLine(line);

                        // Process the signature line by line
                        StringBuilder sigBuilder = new StringBuilder();
                        do
                        {
                            line = reader.ReadLine();
                            if (line == null)
                            {
                                builder.Append(sigBuilder);
                                break; // TODO: might not be correct. Was : Exit While
                            }

                            Match match = Regex.Match(line, "^\\s*public\\s+static(.*)$");
                            if (match.Success)
                            {
                                line = "public static extern " + match.Groups[1].Value;
                            }

                            match = Regex.Match(line, "(.*){\\s*$");
                            if (match.Success)
                            {
                                line = match.Groups[1].Value + ";";
                            }

                            if (Regex.IsMatch(line, "\\s*}\\s*"))
                            {
                                break; // TODO: might not be correct. Was : Exit Do
                            }
                            sigBuilder.AppendLine(line);
                        } while (true);

                        builder.AppendLine(sigBuilder.ToString());
                    }
                    else
                    {
                        builder.AppendLine(line);
                    }

                    line = reader.ReadLine();
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Add any of the helper types that we need
        /// </summary>
        /// <param name="col"></param>
        /// <remarks></remarks>
        private void AddHelperTypes(CodeTypeDeclarationCollection col)
        {
            bool addPInvokePointer = false;
            CodeDomIterator it = new CodeDomIterator();
            List<object> list = it.Iterate(col);
            foreach (object obj in list)
            {
                CodeTypeReference ctdRef = obj as CodeTypeReference;
                if (ctdRef != null && 0 == string.CompareOrdinal(ctdRef.BaseType, MarshalTypeFactory.PInvokePointerTypeName))
                {
                    addPInvokePointer = true;
                }
            }

            if (addPInvokePointer)
            {
                col.Add(MarshalTypeFactory.CreatePInvokePointerType());
            }
        }

    }

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
