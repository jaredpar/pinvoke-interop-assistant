// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System.Collections.Generic;
using System.Diagnostics;

namespace PInvoke.NativeTypes
{
    public abstract class NativeDefinedType : NativeType
    {
        private bool _isAnonymous;

        private List<NativeMember> _members = new List<NativeMember>();

        /// <summary>
        /// Whether or not this type is anonymous
        /// TODO: this isn't imported / exported in the new system
        /// </summary>
        public bool IsAnonymous
        {
            get { return _isAnonymous; }
            set
            {
                _isAnonymous = value;
                if (_isAnonymous)
                {
                    if (string.IsNullOrEmpty(Name))
                    {
                        Name = NativeSymbolBag.GenerateAnonymousName();
                    }
                    Debug.Assert(NativeSymbolBag.IsAnonymousName(Name));
                }
            }
        }

        public NativeName NativeName => new NativeName(Name, NameKind);

        public abstract NativeNameKind NameKind { get; }

        public override NativeSymbolCategory Category
        {
            get { return NativeSymbolCategory.Defined; }
        }

        /// <summary>
        /// Members of the native type
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<NativeMember> Members
        {
            get { return _members; }
        }

        protected NativeDefinedType()
        {
        }

        protected NativeDefinedType(string name)
        {
            this.Name = name;
        }

        public override IEnumerable<NativeSymbol> GetChildren()
        {
            List<NativeSymbol> list = new List<NativeSymbol>();
            foreach (NativeMember member in Members)
            {
                list.Add(member);
            }

            return list;
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildInList(oldChild, newChild, _members);
        }

    }
}
