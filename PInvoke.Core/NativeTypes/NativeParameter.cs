// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System.Collections.Generic;
using System.Diagnostics;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// A parameter to a procedure in native code
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayString}")]
    public class NativeParameter : NativeExtraSymbol
    {
        /// <summary>
        /// Type of the parameter
        /// </summary>
        public NativeType NativeType;

        /// <summary>
        /// The SAL attribute for this parameter
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeSalAttribute SalAttribute = new NativeSalAttribute();

        /// <summary>
        /// NativeType after digging through typedef and named types
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeType NativeTypeDigged
        {
            get
            {
                if (NativeType != null)
                {
                    return NativeType.DigThroughTypeDefAndNamedTypes();
                }

                return null;
            }
        }

        public string DisplayString
        {
            get
            {
                string str = string.Empty;

                if (NativeType != null)
                {
                    str += NativeType.DisplayName + " ";
                }

                str += this.Name;
                return str;
            }
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.Parameter; }
        }

        /// <summary>
        /// A NativeParameter is resolved if it has a type.  
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool IsImmediateResolved
        {
            get { return NativeType != null; }
        }

        public NativeParameter()
        {
            this.Name = string.Empty;
        }

        public NativeParameter(string name)
        {
            this.Name = name;
        }

        public NativeParameter(string name, NativeType type)
        {
            Name = name;
            NativeType = type;
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            List<NativeSymbol> list = new List<NativeSymbol>();
            if (NativeType != null)
            {
                list.Add(NativeType);
            }

            if (SalAttribute != null)
            {
                list.Add(SalAttribute);
            }

            return list;
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            if (object.ReferenceEquals(oldChild, NativeType))
            {
                ReplaceChildSingle(oldChild, newChild, ref NativeType);
            }
            else
            {
                ReplaceChildSingle(oldChild, newChild, ref NativeType);
            }
        }

    }
}
