/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SignatureGenerator
{
    [Serializable]
    public enum Severity
    {
        Info,
        Warning,
        Error,
        End__
    }

    [Serializable]
    internal class Log
    {
        [Serializable]
        public struct LogEntry
        {
            public readonly int ErrorCode;
            public readonly Severity Severity;
            public readonly string Message;

            public LogEntry(int errorCode, Severity severity, string message)
            {
                this.ErrorCode = errorCode;
                this.Severity = severity;
                this.Message = message;
            }
        }

        private List<LogEntry> entries;
        private bool preserveOrder;

        public bool IsEmpty
        {
            get
            {
                return (entries.Count == 0);
            }
        }

        public Log() : this(false)
        { }

        public Log(bool preserveOrder)
        {
            this.entries = new List<LogEntry>();
            this.preserveOrder = preserveOrder;
        }

        public void Add(ErrorDesc error)
        {
            Add(new LogEntry(error.ErrorCode, error.Severity, error.Message));
        }

        public void Add(ErrorDesc error, object arg1)
        {
            Add(new LogEntry(error.ErrorCode, error.Severity, String.Format(error.Message, arg1)));
        }

        public void Add(ErrorDesc error, object arg1, object arg2)
        {
            Add(new LogEntry(error.ErrorCode, error.Severity, String.Format(error.Message, arg1, arg2)));
        }

        public void Add(ErrorDesc error, object arg1, object arg2, object arg3)
        {
            Add(new LogEntry(error.ErrorCode, error.Severity, String.Format(error.Message, arg1, arg2, arg3)));
        }

        public void Add(ErrorDesc error, params object[] args)
        {
            Add(new LogEntry(error.ErrorCode, error.Severity, String.Format(error.Message, args)));
        }

        private void Add(LogEntry entry)
        {
            if (!preserveOrder && entry.Severity != Severity.Info)
            {
                // keep the (errors, warnings, infos) order
                for (int i = 0; i < entries.Count; i++)
                {
                    if ((int)entries[i].Severity < (int)entry.Severity)
                    {
                        entries.Insert(i, entry);
                        return;
                    }
                }
            }

            // append
            entries.Add(entry);
        }

        public void Clear()
        {
            entries.Clear();
        }

        public IEnumerable<LogEntry> Entries()
        {
            return entries;
        }

        public void Print(ILogPrinter printer, string messagePrefix)
        {
            Debug.Assert(printer != null);

            foreach (LogEntry entry in Entries())
            {
                if (!String.IsNullOrEmpty(messagePrefix))
                {
                    printer.PrintEntry(entry.Severity, entry.ErrorCode,
                        String.Format("{0}: {1}", messagePrefix, entry.Message));
                }
                else
                {
                    printer.PrintEntry(entry.Severity, entry.ErrorCode, entry.Message);
                }
            }
        }
    }
}
