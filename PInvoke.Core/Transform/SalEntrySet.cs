// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using PInvoke.Transform.Enums;
using System.Collections.Generic;

namespace PInvoke.Transform
{
    /// <summary>
    /// Set of SAL annotation entries
    /// </summary>
    /// <remarks></remarks>
    public class SalEntrySet
    {
        public SalEntryListType Type { get; set; }

        public List<SalEntry> List { get; } = new List<SalEntry>();

        public SalEntrySet(SalEntryListType type)
        {
            Type = type;
        }

        public SalEntry FindEntry(SalEntryType type)
        {
            foreach (SalEntry entry in List)
            {
                if (entry.Type == type)
                {
                    return entry;
                }
            }

            return null;
        }
    }

}
