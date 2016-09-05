// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using static PInvoke.Contract;

namespace PInvoke.Parser
{

    #region "NativeCodeAnalyerResult"

    /// <summary>
    /// Result of analyzing the Native Code
    /// </summary>
    public sealed class NativeCodeAnalyzerResult
    {
        /// <summary>
        /// Final set of the macros once the code is analyzed
        /// </summary>
        public Dictionary<string, Macro> MacroMap { get; } = new Dictionary<string, Macro>();

        /// <summary>
        /// List of global symbols that were found.
        /// </summary>
        public List<NativeGlobalSymbol> Symbols { get; } = new List<NativeGlobalSymbol>();

        /// <summary>
        /// ErrorProvider for the result
        /// </summary>
        public ErrorProvider ErrorProvider { get; } = new ErrorProvider();

        public NativeCodeAnalyzerResult()
        {

        }

        /// <summary>
        /// Convert the macros in the result into a list of constants
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<NativeConstant> ConvertMacrosToConstants()
        {
            var list = new List<NativeConstant>();

            foreach (Macro macro in MacroMap.Values)
            {
                if (macro.IsMethod)
                {
                    MethodMacro method = (MethodMacro)macro;
                    list.Add(new NativeConstant(macro.Name, method.MethodSignature, ConstantKind.MacroMethod));
                }
                else
                {
                    list.Add(new NativeConstant(macro.Name, macro.Value));
                }
            }

            return list;
        }

    }

    #endregion

    #region "NativeCodeAnalyzer"

    /// <summary>
    /// This is the main class used to analyze native code files.  It wraps all of the other
    /// phases of analysis and provides a simple engine and events that can be hooked into
    /// </summary>
    /// <remarks></remarks>
    public class NativeCodeAnalyzer
    {
        private List<string> _includePathList = new List<string>();
        private bool _followIncludes = true;
        private Dictionary<string, string> _customIncludeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private List<Macro> _initialMacroList = new List<Macro>();
        private bool _includeInitialMacroInResult = true;

        private bool _trace;
        /// <summary>
        /// Whether or not #includes should be followed when encountered
        /// </summary>
        /// <remarks></remarks>
        public IEnumerable<Macro> InitialMacroList
        {
            get { return _initialMacroList; }
        }

        /// <summary>
        /// Whether or not the analyzer should follow #includes it finds
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool FollowIncludes
        {
            get { return _followIncludes; }
            set { _followIncludes = value; }
        }

        /// <summary>
        /// List of paths to search when following #include directives
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> IncludePathList
        {
            get { return _includePathList; }
        }

        /// <summary>
        /// Trace the various parts of analysis
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Trace
        {
            get { return _trace; }
            set { _trace = value; }
        }

        public bool IncludeInitialMacrosInResult
        {
            get { return _includeInitialMacroInResult; }
            set { _includeInitialMacroInResult = value; }
        }


        public NativeCodeAnalyzer()
        {
        }

        public void AddInitialMacro(Macro m)
        {
            m.IsFromParse = false;
            _initialMacroList.Add(m);
        }

        /// <summary>
        /// Analyze the passed in file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeCodeAnalyzerResult Analyze(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                return AnalyzeImpl(new TextReaderBag(filePath, reader));
            }

        }

        /// <summary>
        /// Analyze the passed in stream
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeCodeAnalyzerResult Analyze(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            return AnalyzeImpl(new TextReaderBag(reader));
        }

        /// <summary>
        /// Run the preprocessor on the specefied file and return a Stream to the resulting
        /// data
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public TextReaderBag RunPreProcessor(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            NativeCodeAnalyzerResult result = new NativeCodeAnalyzerResult();
            using (StreamReader fileStream = new StreamReader(filePath))
            {
                return RunPreProcessorImpl(result, new TextReaderBag(filePath, fileStream));
            }
        }

        private NativeCodeAnalyzerResult AnalyzeImpl(TextReaderBag readerbag)
        {
            ThrowIfNull(readerbag);

            NativeCodeAnalyzerResult result = new NativeCodeAnalyzerResult();

            // Run the procprocessor and get the resulting Textreader
            TextReaderBag readerBag2 = this.RunPreProcessorImpl(result, readerbag);
            using (readerBag2.TextReader)
            {

                // Run the parser 
                this.RunParser(result, readerBag2);
            }

            return result;
        }

        /// <summary>
        /// Run the PreProcessor on the stream
        /// </summary>
        /// <param name="result"></param>
        /// <param name="readerBag"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private TextReaderBag RunPreProcessorImpl(NativeCodeAnalyzerResult result, TextReaderBag readerBag)
        {
            ThrowIfNull(result);
            ThrowIfNull(readerBag);

            // Create the options
            PreProcessorOptions opts = new PreProcessorOptions();
            opts.FollowIncludes = this.FollowIncludes;
            opts.IncludePathList.AddRange(this.IncludePathList);
            opts.InitialMacroList.AddRange(_initialMacroList);
            opts.Trace = this.Trace;

            PreProcessorEngine preprocessor = new PreProcessorEngine(opts);

            // Process the file
            string ret = preprocessor.Process(readerBag);

            // Process the results
            result.ErrorProvider.Append(preprocessor.ErrorProvider);
            foreach (KeyValuePair<string, Macro> pair in preprocessor.MacroMap)
            {
                if (_includeInitialMacroInResult || pair.Value.IsFromParse)
                {
                    result.MacroMap.Add(pair.Key, pair.Value);
                }
            }

            return new TextReaderBag(new StringReader(ret));
        }

        /// <summary>
        /// Run the actual parser on the stream
        /// </summary>
        /// <param name="result"></param>
        /// <param name="readerBag"></param>
        /// <remarks></remarks>
        private void RunParser(NativeCodeAnalyzerResult result, TextReaderBag readerBag)
        {
            ThrowIfNull(readerBag);

            // Perform the parse
            ParseEngine parser = new ParseEngine();
            ParseResult parseResult = parser.Parse(readerBag);

            // add in the basic results
            result.ErrorProvider.Append(parseResult.ErrorProvider);
            result.Symbols.AddRange(parseResult.NativeDefinedTypes.Select(x => new NativeGlobalSymbol(x)));
            result.Symbols.AddRange(parseResult.NativeTypeDefs.Select(x => new NativeGlobalSymbol(x)));
            result.Symbols.AddRange(parseResult.NativeProcedures.Select(x => new NativeGlobalSymbol(x)));
            result.Symbols.AddRange(parseResult.NativeEnumValues.Select(x => new NativeGlobalSymbol(x)));
        }

    }

    #endregion

    #region "NativeCodeAnalyzerFactory"

    /// <summary>
    /// Os Version
    /// </summary>
    /// <remarks></remarks>
    public enum OsVersion
    {
        Windows2000,
        WindowsXP,
        Windows2003,
        WindowsVista
    }

    /// <summary>
    /// Factory for creating a NativeCodeAnalyzer based on common configurations
    /// </summary>
    /// <remarks></remarks>
    public static class NativeCodeAnalyzerFactory
    {

        /// <summary>
        /// This will create an analyzer for parsing out a full header file
        /// </summary>
        /// <param name="osVersion"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static NativeCodeAnalyzer Create(OsVersion osVersion)
        {
            NativeCodeAnalyzer analyzer = new NativeCodeAnalyzer();
            Debug.Assert(analyzer.IncludeInitialMacrosInResult);
            // Should be the default

            // Ignore 64 bit settings.  64 bit types are emitted as SysInt or IntPtr
            analyzer.AddInitialMacro(new Macro("__w64", string.Empty));

            analyzer.AddInitialMacro(new Macro("_X86_", string.Empty));
            analyzer.AddInitialMacro(new Macro("_WIN32", string.Empty));
            analyzer.AddInitialMacro(new Macro("WINAPI", "__winapi", true));
            analyzer.AddInitialMacro(new Macro("UNICODE", "1"));
            analyzer.AddInitialMacro(new Macro("__STDC__", "1"));

            // Add the operating system macros
            AddOSInformation(analyzer, osVersion);

            // Common information
            AddCommonMacros(analyzer);
            return analyzer;
        }

        /// <summary>
        /// Sometimes you want to parse out a snippet of code without doing the full windows
        /// parse.  In this case you'll need to have certain Macros already defined since
        /// they are defined at the begining of the windows header files.  This will add
        /// in all of those macros
        /// </summary>
        /// <param name="osVersion"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static NativeCodeAnalyzer CreateForMiniParse(OsVersion osVersion, IEnumerable<Macro> initialMacroList)
        {
            NativeCodeAnalyzer analyzer = Create(osVersion);
            Debug.Assert(analyzer.IncludeInitialMacrosInResult);
            // Should be the default

            analyzer.IncludeInitialMacrosInResult = false;
            foreach (Macro m in initialMacroList)
            {
                analyzer.AddInitialMacro(m);
            }

            return analyzer;
        }

        private static void AddOSInformation(NativeCodeAnalyzer analyzer, OsVersion os)
        {
            switch (os)
            {
                case OsVersion.WindowsXP:
                    analyzer.AddInitialMacro(new Macro("WINVER", "0x0501"));
                    analyzer.AddInitialMacro(new Macro("_WIN32_WINNT", "0x0501"));
                    break;
                case OsVersion.Windows2000:
                    analyzer.AddInitialMacro(new Macro("WINVER", "0x0500"));
                    analyzer.AddInitialMacro(new Macro("_WIN32_WINNT", "0x0500"));
                    break;
                case OsVersion.Windows2003:
                    analyzer.AddInitialMacro(new Macro("WINVER", "0x0502"));
                    analyzer.AddInitialMacro(new Macro("_WIN32_WINNT", "0x0502"));
                    break;
                case OsVersion.WindowsVista:
                    analyzer.AddInitialMacro(new Macro("WINVER", "0x0600"));
                    analyzer.AddInitialMacro(new Macro("_WIN32_WINNT", "0x0600"));
                    break;
                default:
                    ThrowInvalidEnumValue(os);
                    break;
            }
        }


        private static void AddCommonMacros(NativeCodeAnalyzer analyzer)
        {
            // MCS Version
            analyzer.AddInitialMacro(new Macro("_MSC_VER", "9999"));
            analyzer.AddInitialMacro(new Macro("_MSC_FULL_VER", "99999999"));

            // Make sure that SAL is imported
            analyzer.AddInitialMacro(new Macro("_PREFAST_", string.Empty));
        }

        public static List<string> GetCommonSdkPaths()
        {
            List<string> list = new List<string>();
            list.Add(GetPlatformSdkIncludePath());
            list.Add(GetSdkIncludePath());
            return list;
        }

        /// <summary>
        /// Get the path to the platform SDK include files
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetPlatformSdkIncludePath()
        {
            return Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "Microsoft Visual Studio 8\\VC\\PlatformSDK\\include");
        }

        public static string GetSdkIncludePath()
        {
            return Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "Microsoft Visual Studio 8\\VC\\include");
        }
    }
    #endregion

}
