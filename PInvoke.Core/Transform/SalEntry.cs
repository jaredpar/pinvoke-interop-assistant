// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;

namespace PInvoke.Transform
{
    public class SalEntry
    {
        public SalEntryType Type;

        public string Text;
        public SalEntry(SalEntryType type)
        {
            this.Type = type;
            this.Text = string.Empty;
        }

        public SalEntry(NativeSalEntry other)
        {
            this.Type = other.SalEntryType;
            this.Text = other.Text.Trim(' ');
        }
    }

}
