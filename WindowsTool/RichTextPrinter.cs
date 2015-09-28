/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using SignatureGenerator;

namespace WindowsTool
{
    /// <summary>
    /// Writes code and messages to a <see cref="RichTextBox"/> control.
    /// </summary>
    class RichTextPrinter : ICodePrinter, ILogPrinter
    {
        #region Fields

        private RichTextBox textBox;
        private int indentLevel;

        private const int spacesPerIndent = 4;

        private static Color[] logColors;
        private static Color[] codeColors;

        #endregion

        #region Construction

        public RichTextPrinter(RichTextBox textBox)
        {
            Debug.Assert(textBox != null);

            this.textBox = textBox;
        }

        static RichTextPrinter()
        {
            logColors = new Color[(int)Severity.End__];
            logColors[(int)Severity.Info]    = Color.Black;
            logColors[(int)Severity.Warning] = Color.Blue;
            logColors[(int)Severity.Error]   = Color.Red;

            codeColors = new Color[(int)OutputType.End__];
            codeColors[(int)OutputType.Keyword]    = Color.FromArgb(0, 0, 255);
            codeColors[(int)OutputType.Operator]   = Color.Black;
            codeColors[(int)OutputType.Identifier] = Color.Black;
            codeColors[(int)OutputType.TypeName]   = Color.FromArgb(26, 85, 102);
            codeColors[(int)OutputType.Literal]    = Color.FromArgb(163, 21, 21);
            codeColors[(int)OutputType.Comment]    = Color.FromArgb(0, 128, 0);
            codeColors[(int)OutputType.Other]      = Color.Black;
        }

        #endregion

        #region ICodePrinter Members

        public void Print(OutputType codeType, string str)
        {
            if (!SystemInformation.HighContrast)
            {
                textBox.SelectionColor = codeColors[(int)codeType];
            }

            textBox.AppendText(str);
        }

        public void PrintLn()
        {
            StringBuilder sb = new StringBuilder("\r\n", 2 + indentLevel * spacesPerIndent);
            sb.Append(' ', indentLevel * spacesPerIndent);

            textBox.AppendText(sb.ToString());
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

        #region ILogPrinter Members

        public void PrintEntry(Severity severity, int code, string message)
        {
            if (textBox.TextLength != 0)
            {
                textBox.AppendText("\r\n");
            }
            
            textBox.SelectionBullet = true;
            textBox.BulletIndent = 10;
            if (!SystemInformation.HighContrast)
            {
                textBox.SelectionColor = logColors[(int)severity];
            }

            textBox.AppendText(LogFormatter.Format(severity, code, indentLevel, message));
            textBox.AppendText("\r\n");

            textBox.SelectionBullet = false;
        }

        public void Separate()
        {
            textBox.AppendText("\r\n");
        }

        #endregion
    }
}
