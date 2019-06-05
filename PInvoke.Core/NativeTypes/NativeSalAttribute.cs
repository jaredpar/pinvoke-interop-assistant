// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.Text;
using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents the collection of SAL attributes
    /// </summary>
    [DebuggerDisplay("{DisplayName}")]
    public class NativeSalAttribute : NativeExtraSymbol
    {
        private List<NativeSalEntry> _list = new List<NativeSalEntry>();

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.SalAttribute; }
        }

        /// <summary>
        /// List of attribute entries
        /// </summary>
        public List<NativeSalEntry> SalEntryList
        {
            get { return _list; }
        }

        /// <summary>
        /// True if there are no entries in the attribute
        /// </summary>
        public bool IsEmpty
        {
            get { return _list.Count == 0; }
        }

        public override string DisplayName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                bool isFirst = true;
                foreach (NativeSalEntry entry in _list)
                {
                    if (!isFirst)
                    {
                        builder.Append(",");
                    }

                    isFirst = false;
                    builder.Append(entry.DisplayName);
                }
                return builder.ToString();
            }
        }

        public NativeSalAttribute()
        {
            this.Name = "Sal";
        }

        public NativeSalAttribute(params SalEntryType[] entryList) : this()
        {
            foreach (SalEntryType entry in entryList)
            {
                _list.Add(new NativeSalEntry(entry));
            }
        }

        public NativeSalAttribute(params NativeSalEntry[] entryList) : this()
        {
            _list.AddRange(entryList);
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            List<NativeSymbol> list = new List<NativeSymbol>();
            foreach (NativeSalEntry entry in _list)
            {
                list.Add(entry);
            }

            return list;
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            base.ReplaceChildInList(oldChild, newChild, _list);
        }

    }

}
