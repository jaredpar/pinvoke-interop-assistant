/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SignatureGenerator
{
    [Serializable]
    public enum OutputType
    {
        Keyword,
        Operator,
        Identifier,
        TypeName,
        Literal,
        Comment,
        Other,
        End__
    }

    [Serializable, Flags]
    public enum PrintFlags
    {
        None                    = 0,
        UsePlainC               = 1,  // if not set, use usual WinAPI/COM types and syntax like LPSTR
        UseDefinedComInterfaces = 2,  // if not set, use undefined COM interfaces (the result may not compile)
        MangleEnumFields        = 4,  // if set, prepend enum field names with some garbage to avoid clashes
        PrintMarshalDirection   = 8   // if set, add something like /*[in]*/ to all pointer parameters
    }

    public interface ICodePrinter
    {
        void Print(OutputType codeType, string str);

        void PrintLn();
        void PrintLn(OutputType codeType, string str);

        void Indent();
        void Unindent();
    }

    public interface ILogPrinter
    {
        void PrintEntry(Severity severity, int code, string message);
        void Separate();

        void Indent();
        void Unindent();
    }

    public interface ICodePrintable
    {
        void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags);
    }

    public delegate void LogCallback(string entry);

    public class TextWriterCodePrinter : ICodePrinter, IDisposable
    {
        #region Fields

        protected TextWriter writer;
        protected int indentLevel;

        private const string indent = "    ";

        #endregion

        #region Construction

        public TextWriterCodePrinter(TextWriter writer)
        {
            this.writer = writer;
        }

        #endregion

        #region ICodePrinter Members

        public void Print(OutputType codeType, string str)
        {
            // the output type is ignored
            writer.Write(str);
        }

        public void PrintLn()
        {
            writer.WriteLine();
            for (int i = 0; i < indentLevel; i++) writer.Write(indent);
        }

        public void PrintLn(OutputType codeType, string str)
        {
            Print(codeType, str);
            PrintLn();
        }

        public void Indent()
        {
            indentLevel++;
        }

        public void Unindent()
        {
            Debug.Assert(indentLevel > 0);
            indentLevel--;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            writer.Dispose();
        }

        #endregion
    }

    public class LogMemoryPrinter : ILogPrinter
    {
        #region Fields

        private Log log = new Log(true);

        private const string IndentMarker = "#>";
        private const string UnindentMarker = "#<";

        #endregion

        #region Properties

        public bool IsEmpty
        {
            get { return log.IsEmpty; }
        }

        #endregion

        #region ILogPrinter Members

        public void PrintEntry(Severity severity, int code, string message)
        {
            log.Add(new ErrorDesc(code, severity, message));
        }

        public void Separate()
        {
            log.Add(new ErrorDesc(0, Severity.Info, String.Empty));
        }

        public void Indent()
        {
            log.Add(new ErrorDesc(0, Severity.Info, IndentMarker));
        }

        public void Unindent()
        {
            log.Add(new ErrorDesc(0, Severity.Info, UnindentMarker));
        }

        #endregion

        #region Clear and Replay

        public void Clear()
        {
            log.Clear();
        }

        public void ReplayTo(ILogPrinter anotherPrinter)
        {
            Debug.Assert(anotherPrinter != null && anotherPrinter != this);

            foreach (Log.LogEntry entry in log.Entries())
            {
                if (entry.ErrorCode == 0)
                {
                    if (entry.Message.Length == 0)
                    {
                        anotherPrinter.Separate();
                        continue;
                    }
                    else if (entry.Message == IndentMarker)
                    {
                        anotherPrinter.Indent();
                        continue;
                    }
                    else if (entry.Message == UnindentMarker)
                    {
                        anotherPrinter.Unindent();
                        continue;
                    }
                }

                // ordinary entry
                anotherPrinter.PrintEntry(entry.Severity, entry.ErrorCode, entry.Message);
            }
        }

        #endregion
    }

    public class LogCallbackPrinter : ILogPrinter
    {
        #region Fields

        protected LogCallback callback;
        protected int indentLevel;

        private const string indent = "    ";

        #endregion

        #region Construction

        public LogCallbackPrinter(LogCallback callback)
        {
            this.callback = callback;
        }

        #endregion

        #region ILogPrinter Members

        public void PrintEntry(Severity severity, int code, string message)
        {
            callback(LogFormatter.Format(severity, code, indentLevel, message));
        }

        public void Separate()
        { }

        public void Indent()
        {
            indentLevel++;
        }

        public void Unindent()
        {
            Debug.Assert(indentLevel > 0);
            indentLevel--;
        }

        #endregion
    }

    public class CodeMemoryPrinter : ICodePrinter
    {
        #region PrintEntry

        private struct PrintEntry
        {
            public PrintEntry(OutputType outputType, string str)
            {
                this.OutputType = outputType;
                this.String = str;
            }

            public OutputType OutputType;
            public string String;
        }

        #endregion

        #region Fields

        private List<PrintEntry> list = new List<PrintEntry>();

        private const string NewLineMarker = "\\n";
        private const string IndentMarker = "#>";
        private const string UnindentMarker = "#<";

        #endregion

        #region ICodePrinter Members

        public void Print(OutputType codeType, string str)
        {
            list.Add(new PrintEntry(codeType, str));
        }

        public void PrintLn()
        {
            list.Add(new PrintEntry(OutputType.End__, NewLineMarker));
        }

        public void PrintLn(OutputType codeType, string str)
        {
            Print(codeType, str);
            PrintLn();
        }

        public void Indent()
        {
            list.Add(new PrintEntry(OutputType.End__, IndentMarker));
        }

        public void Unindent()
        {
            list.Add(new PrintEntry(OutputType.End__, UnindentMarker));
        }

        #endregion

        #region Clear and Replay

        public void Clear()
        {
            list.Clear();
        }

        public void ReplayTo(ICodePrinter anotherPrinter)
        {
            Debug.Assert(anotherPrinter != null && anotherPrinter != this);

            foreach (PrintEntry entry in list)
            {
                if (entry.OutputType == OutputType.End__)
                {
                    if (ReferenceEquals(entry.String, IndentMarker)) anotherPrinter.Indent();
                    else if (ReferenceEquals(entry.String, UnindentMarker)) anotherPrinter.Unindent();
                    else
                    {
                        Debug.Assert(ReferenceEquals(entry.String, NewLineMarker));
                        anotherPrinter.PrintLn();
                    }
                }
                else
                    anotherPrinter.Print(entry.OutputType, entry.String);
            }
        }

        #endregion
    }

    public static class LogFormatter
    {
        #region Format

        public static string Format(Severity severity, int code, int indentLevel, string message)
        {
            StringBuilder sb = new StringBuilder(indentLevel + message.Length + 16);

            sb.Append("SigExp : ");

            switch (severity)
            {
                case Severity.Info: sb.Append(Resources.Info); break;
                case Severity.Warning: sb.Append(Resources.Warning); break;
                case Severity.Error: sb.Append(Resources.Error); break;

                default:
                {
                    Debug.Fail(null);
                    break;
                }
            }

            sb.AppendFormat(" SX{0:d4} : ", code);
            sb.Append(' ', indentLevel);
            sb.Append(message);

            return sb.ToString();
        }

        #endregion
    }
}
