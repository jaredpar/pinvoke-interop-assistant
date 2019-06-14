// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.Enums;
using PInvoke.NativeTypes.Enums;
using System.Collections.Generic;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Containing for a native enum type.
    /// </summary>
    public sealed class NativeEnum : NativeDefinedType
    {
        private List<NativeEnumValue> _list = new List<NativeEnumValue>();

        public override NativeSymbolKind Kind => NativeSymbolKind.EnumType;

        public override NativeNameKind NameKind => NativeNameKind.Enum;

        /// <summary>
        /// The values of the enum
        /// </summary>
        public List<NativeEnumValue> Values
        {
            get { return _list; }
        }

        public NativeEnum()
        {
        }

        public NativeEnum(string name)
        {
            this.Name = name;
        }

        public NativeEnumValue AddValue(string valueName, string value)
        {
            var e = new NativeEnumValue(Name, valueName, value);
            Values.Add(e);
            return e;
        }

        /// <summary>
        /// Enum's can't have members, just name value pairs
        /// </summary>
        public override IEnumerable<NativeSymbol> GetChildren()
        {
            List<NativeSymbol> list = new List<NativeSymbol>();
            foreach (NativeEnumValue pair in this.Values)
            {
                list.Add(pair);
            }

            return list;
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildInList(oldChild, newChild, _list);
        }

    }
}
