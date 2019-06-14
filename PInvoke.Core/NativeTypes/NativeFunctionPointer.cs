// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.Enums;
using PInvoke.NativeTypes.Enums;
using System.Diagnostics;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a native function pointer
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayName}")]
    public class NativeFunctionPointer : NativeDefinedType
    {
        private NativeCallingConvention _conv = NativeCallingConvention.WinApi;

        /// <summary>
        /// Get the signature of the function pointer
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeSignature Signature = new NativeSignature();

        public NativeCallingConvention CallingConvention
        {
            get { return _conv; }
            set { _conv = value; }
        }

        public override NativeSymbolKind Kind => NativeSymbolKind.FunctionPointer;

        public override NativeNameKind NameKind => NativeNameKind.FunctionPointer;

        public override string DisplayName
        {
            get
            {
                string dispName = Name;
                if (NativeSymbolBag.IsAnonymousName(dispName))
                {
                    dispName = "anonymous";
                }

                if (Signature == null)
                {
                    return dispName;
                }

                return Signature.CalculateSignature("(*" + dispName + ")");
            }
        }


        public NativeFunctionPointer()
        {
        }

        public NativeFunctionPointer(string name)
        {
            this.Name = name;
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(Signature);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildSingle(oldChild, newChild, ref Signature);
        }

    }
}
