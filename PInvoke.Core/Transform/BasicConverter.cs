
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
using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;
using PInvoke.Transform.Enums;

namespace PInvoke.Transform
{

    /// <summary>
    /// Wraps a lot of the functionality into a simple few method wrapper
    /// </summary>
    /// <remarks></remarks>
    public class BasicConverter
    {
        public INativeSymbolStorage Storage { get; set; }

        /// <summary>
        /// Language to generate into
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public LanguageType LanguageType { get; set; }

        public TransformKindFlags TransformKindFlags { get; set; } = TransformKindFlags.All;

        public BasicConverter(LanguageType type, INativeSymbolStorage storage)
        {
            Storage = storage;
            LanguageType = type;
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeConstant c, ErrorProvider ep)
        {
            var bag = new NativeSymbolBag(Storage);
            bag.AddConstant(c);
            return ConvertBagToCodeDom(bag, ep, null);
        }

        public string ConvertToPInvokeCode(NativeConstant c)
        {
            var ep = new ErrorProvider();
            var col = ConvertToCodeDom(c, ep);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeTypeDef typedef, ErrorProvider ep)
        {
            var bag = new NativeSymbolBag(Storage);
            bag.AddTypeDef(typedef);
            return ConvertBagToCodeDom(bag, ep, null);
        }

        public string ConvertToPInvokeCode(NativeTypeDef typedef)
        {
            var ep = new ErrorProvider();
            var col = ConvertToCodeDom(typedef, ep);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeDefinedType definedNt, ErrorProvider ep)
        {
            var bag = new NativeSymbolBag(Storage);
            bag.AddDefinedType(definedNt);
            return ConvertBagToCodeDom(bag, ep, null);
        }

        public string ConvertToPInvokeCode(NativeDefinedType definedNt)
        {
            var ep = new ErrorProvider();
            var col = ConvertToCodeDom(definedNt, ep);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeProcedure proc, ErrorProvider ep, string libraryName)
        {
            var bag = new NativeSymbolBag(Storage);
            bag.AddProcedure(proc);
            return ConvertBagToCodeDom(bag, ep, libraryName);
        }

        public string ConvertToPInvokeCode(NativeProcedure proc, string libraryName)
        {
            var ep = new ErrorProvider();
            var col = ConvertToCodeDom(proc, ep, libraryName);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public CodeTypeDeclarationCollection ConvertToCodeDom(NativeSymbolBag bag, ErrorProvider ep)
        {
            return ConvertBagToCodeDom(bag, ep, null);
        }

        //public string ConvertToPInvokeCode(NativeSymbolBag bag)
        //{
        //    ErrorProvider ep = new ErrorProvider();
        //    return ConvertBagToPInvokeCodeImpl(bag, ep);
        //}

        /// <summary>
        /// Convert the block of Native code into PInvoke code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ConvertNativeCodeToPInvokeCode(string code, string libraryName)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            var ep = new ErrorProvider();
            var col = ConvertNativeCodeToCodeDom(code, ep, libraryName);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        /// <summary>
        /// Convert the block of native code into a CodeDom hierarchy
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ep"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeDeclarationCollection ConvertNativeCodeToCodeDom(string code, ErrorProvider ep, string libraryName = null)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            if (ep == null)
            {
                throw new ArgumentNullException("ep");
            }

            var analyzer = NativeCodeAnalyzerFactory.CreateForMiniParse(OsVersion.WindowsVista, Storage.GetAllMacros());

            // TODO: probably should delete this 
            analyzer.IncludePathList.Add("c:\\program files (x86)\\windows kits\\8.1\\include\\shared");
            var bag = default(NativeSymbolBag);
            using (System.IO.StringReader reader = new StringReader(code))
            {
                var result = analyzer.Analyze(reader);

                ep.Append(result.ErrorProvider);
                bag = NativeSymbolBag.CreateFrom(result, Storage);
            }

            return ConvertBagToCodeDom(bag, ep, libraryName);
        }

        private string ConvertBagToPInvokeCodeImpl(NativeSymbolBag bag, ErrorProvider ep, string libraryName)
        {
            var col = ConvertBagToCodeDom(bag, ep, libraryName);
            return ConvertCodeDomToPInvokeCodeImpl(col, ep);
        }

        public string ConvertCodeDomToPInvokeCode(CodeTypeDeclaration ctd)
        {
            var col = new CodeTypeDeclarationCollection
            {
                ctd
            };
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
            return ConvertCodeDomToPInvokeCodeImpl(LanguageType, col, ep);
        }

        public static string ConvertCodeDomToPInvokeCodeImpl(LanguageType type, CodeTypeDeclarationCollection col, ErrorProvider ep)
        {
            ThrowIfNull(col);
            ThrowIfNull(ep);

            var writer = new StringWriter();
            var provider = default(CodeDomProvider);
            var commentStart = default(string);

            // Generate based on the language
            switch (type)
            {
                case LanguageType.VisualBasic:
                    commentStart = "'";
                    provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
                case LanguageType.CSharp:
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

            // TODO : look in to refactoring to use this code - 
            // https://docs.microsoft.com/en-us/dotnet/api/system.codedom.compiler.codedomprovider?view=netframework-4.8
            // https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/how-to-create-a-class-using-codedom
            // https://docs.microsoft.com/en-us/dotnet/api/system.codedom.compiler.codedomprovider.generatecodefrommember?view=netframework-4.8
            foreach (CodeTypeDeclaration ctd in col)
            {
                provider.GenerateCodeFromMember(
                    ctd, 
                    writer, 
                    new CodeGeneratorOptions
                    {
                        BracingStyle = "C",
                    });
            }

            if (type == LanguageType.CSharp)
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
        private CodeTypeDeclarationCollection ConvertBagToCodeDom(NativeSymbolBag bag, ErrorProvider ep, string libraryName)
        {
            ThrowIfNull(bag);
            ThrowIfNull(ep);

            // Make sure than all of the referenced NativeDefinedType instances are in the correct
            // portion of the bag
            ChaseReferencedDefinedTypes(bag);

            // First step is to resolve the symbols
            bag.TryResolveSymbolsAndValues(ep);

            // Create the codedom transform
            var transform = new CodeTransform(LanguageType, bag);
            var marshalUtil = new MarshalTransform(LanguageType, bag, TransformKindFlags);
            var col = new CodeTypeDeclarationCollection();

            // Only output the constants if there are actually any
            var list = new List<NativeConstant>(bag.FindResolvedConstants());
            if (list.Count > 0)
            {
                var constCtd = transform.GenerateConstants(list);
                if (constCtd.Members.Count > 0)
                {
                    col.Add(constCtd);
                }
            }

            foreach (var definedNt in bag.FindResolvedDefinedTypes())
            {
                var ctd = transform.GenerateDeclaration(definedNt);
                marshalUtil.Process(ctd);
                col.Add(ctd);
            }

            var procList = new List<NativeProcedure>(bag.FindResolvedProcedures());
            if (procList.Count > 0)
            {
                var procType = transform.GenerateProcedures(procList, libraryName);
                marshalUtil.Process(procType);
                col.Add(procType);
            }

            // Add the helper types that we need
            AddHelperTypes(col);

            // Next step is to run the pretty lister on it
            var prettyLister = new CodeDomPrettyList(bag);
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
                    if (!bag.TryGetGlobalSymbol(sym.Name, out defined))
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
                var line = reader.ReadLine();

                while (line != null)
                {
                    // Look for the DLLImport line
                    if (Regex.IsMatch(line, "^\\s*\\[System\\.Runtime\\.InteropServices\\.DllImport.*$"))
                    {
                        builder.AppendLine(line);

                        // Process the signature line by line
                        var sigBuilder = new StringBuilder();
                        do
                        {
                            line = reader.ReadLine();
                            if (line == null)
                            {
                                builder.Append(sigBuilder);
                                break;
                            }

                            var match = Regex.Match(line, "^\\s*public\\s+static(.*)$");
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
                                break;
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
