/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using SignatureGenerator;

namespace ConsoleTool
{
    class ConsolePrinter : ICodePrinter, ILogPrinter
    {
        #region ColorScheme

        private struct CodeColorScheme
        {
            private readonly ConsoleColor[] colors;

            public static CodeColorScheme Default = new CodeColorScheme(
                ConsoleColor.Cyan,
                ConsoleColor.White,
                ConsoleColor.White,
                ConsoleColor.Yellow,
                ConsoleColor.Magenta,
                ConsoleColor.Green,
                ConsoleColor.White);

            public CodeColorScheme(
                ConsoleColor keyword,
                ConsoleColor @operator,
                ConsoleColor identifier,
                ConsoleColor typeName,
                ConsoleColor literal,
                ConsoleColor comment,
                ConsoleColor other)
            {
                this.colors = new ConsoleColor[(int)OutputType.End__];

                this.colors[(int)OutputType.Keyword] = keyword;
                this.colors[(int)OutputType.Operator] = @operator;
                this.colors[(int)OutputType.Identifier] = identifier;
                this.colors[(int)OutputType.TypeName] = typeName;
                this.colors[(int)OutputType.Literal] = literal;
                this.colors[(int)OutputType.Comment] = comment;
                this.colors[(int)OutputType.Other] = other;
            }

            public ConsoleColor GetColor(OutputType outputType)
            {
                return colors[(int)outputType];
            }
        }

        private struct LogColorScheme
        {
            private readonly ConsoleColor[] colors;

            public static LogColorScheme Default = new LogColorScheme(
                ConsoleColor.White,
                ConsoleColor.Yellow,
                ConsoleColor.Red);

            public LogColorScheme(
                ConsoleColor info,
                ConsoleColor warning,
                ConsoleColor error)
            {
                this.colors = new ConsoleColor[(int)Severity.End__];

                this.colors[(int)Severity.Info] = info;
                this.colors[(int)Severity.Warning] = warning;
                this.colors[(int)Severity.Error] = error;
            }

            public ConsoleColor GetColor(Severity severity)
            {
                return colors[(int)severity];
            }
        }

        #endregion

        #region Fields

        private int indentLevel;
        private CodeColorScheme codeScheme = CodeColorScheme.Default;
        private LogColorScheme logScheme = LogColorScheme.Default;

        private const string indent = "    ";

        #endregion

        #region ICodePrinter Members

        public void Print(OutputType codeType, string str)
        {
            ConsoleColor old_color = Console.ForegroundColor;
            Console.ForegroundColor = codeScheme.GetColor(codeType);

            Console.Write(str);

            Console.ForegroundColor = old_color;
        }

        public void PrintLn()
        {
            Console.WriteLine();

            for (int i = 0; i < indentLevel; i++) Console.Write(indent);
        }

        public void PrintLn(OutputType codeType, string str)
        {
            Print(codeType, str);
            PrintLn();
        }

        #endregion

        #region ILogPrinter Members

        public void PrintEntry(Severity severity, int code, string message)
        {
            ConsoleColor old_color = Console.ForegroundColor;
            Console.ForegroundColor = logScheme.GetColor(severity);

            Console.Error.WriteLine(LogFormatter.Format(severity, code, indentLevel, message));

            Console.ForegroundColor = old_color;
        }

        public void Separate()
        {
            Console.WriteLine();
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
    }

    public class TextWriterPrinter : TextWriterCodePrinter, ILogPrinter
    {
        #region Construction

        public TextWriterPrinter(TextWriter writer)
            : base(writer)
        { }

        #endregion

        #region ILogPrinter Members

        public void PrintEntry(Severity severity, int code, string message)
        {
            writer.WriteLine(LogFormatter.Format(severity, code, indentLevel, message));
        }

        public void Separate()
        {
            writer.WriteLine();
        }

        #endregion
    }
}
