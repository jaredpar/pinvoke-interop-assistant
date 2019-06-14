// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.Enums;
using PInvoke.NativeTypes.Enums;
using System.Diagnostics;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Procedure symbol
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayName}")]
    public class NativeProcedure : NativeSymbol
    {
        private NativeCallingConvention _conv = NativeCallingConvention.WinApi;

        /// <summary>
        /// Name of the DLL this proc is in
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string DllName;

        /// <summary>
        /// Signature of the procedure
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

        public override NativeSymbolCategory Category => NativeSymbolCategory.Procedure;

        public override NativeSymbolKind Kind => NativeSymbolKind.Procedure;

        public NativeName NativeName => new NativeName(Name, NativeNameKind.Procedure);

        public override string DisplayName
        {
            get
            {
                if (Signature == null)
                {
                    return Name;
                }

                return Signature.CalculateSignature(this.Name);
            }
        }


        public NativeProcedure()
        {
        }

        public NativeProcedure(string name) : base(name)
        {
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(Signature);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            base.ReplaceChildSingle(oldChild, newChild, ref Signature);
        }

    }
}
