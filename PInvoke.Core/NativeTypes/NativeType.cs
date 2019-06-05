// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a type in the system
    /// </summary>
    /// <remarks></remarks>
    public abstract class NativeType : NativeSymbol
    {
        protected NativeType()
        {
        }

        protected NativeType(string name) : base(name)
        {
        }

        public NativeType DigThroughNamedTypes()
        {

            NativeType cur = this;
            while (cur != null)
            {
                if (cur.Kind == NativeSymbolKind.NamedType)
                {
                    cur = ((NativeNamedType)cur).RealType;
                }
                else
                {
                    break;
                }
            }

            return cur;
        }

        public NativeType DigThroughNamedTypesFor(string search)
        {
            if (0 == string.CompareOrdinal(Name, search))
            {
                return this;
            }

            NativeType cur = this;
            while (cur != null && cur.Kind == NativeSymbolKind.NamedType)
            {
                NativeNamedType namedNt = (NativeNamedType)cur;
                cur = namedNt.RealType;

                if (cur != null && 0 == string.CompareOrdinal(cur.Name, search))
                {
                    return cur;
                }
            }

            return null;
        }

        /// <summary>
        /// Dig through this type until we get past the typedefs and named types to the real
        /// type 
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeType DigThroughTypeDefAndNamedTypes()
        {

            NativeType cur = this;
            while (cur != null)
            {
                if (cur.Kind == NativeSymbolKind.NamedType)
                {
                    cur = ((NativeNamedType)cur).RealType;
                }
                else if (cur.Kind == NativeSymbolKind.TypeDefType)
                {
                    cur = ((NativeTypeDef)cur).RealType;
                }
                else
                {
                    break; // TODO: might not be correct. Was : Exit While
                }
            }

            return cur;
        }

        public NativeType DigThroughTypeDefAndNamedTypesFor(string search)
        {
            if (0 == string.CompareOrdinal(search, this.Name))
            {
                return this;
            }

            NativeType cur = this;
            while (cur != null)
            {
                if (cur.Kind == NativeSymbolKind.NamedType)
                {
                    cur = ((NativeNamedType)cur).RealType;
                }
                else if (cur.Kind == NativeSymbolKind.TypeDefType)
                {
                    cur = ((NativeTypeDef)cur).RealType;
                }
                else
                {
                    break;
                }

                if (cur != null && 0 == string.CompareOrdinal(cur.Name, search))
                {
                    return cur;
                }
            }

            return null;
        }
    }
}
