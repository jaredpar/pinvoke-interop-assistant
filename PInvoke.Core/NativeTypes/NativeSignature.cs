// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System.Collections.Generic;
using System.Text;

namespace PInvoke.NativeTypes
{
    public class NativeSignature : NativeExtraSymbol
    {
        private List<NativeParameter> _paramList = new List<NativeParameter>();

        /// <summary>
        /// Return type of the NativeProcedure
        /// </summary>
        public NativeType ReturnType;

        /// <summary>
        /// SAL attribute on the return type of the procedure
        /// </summary>
        public NativeSalAttribute ReturnTypeSalAttribute = new NativeSalAttribute();

        /// <summary>
        /// Parameters of the procedure
        /// </summary>
        public List<NativeParameter> Parameters
        {
            get { return _paramList; }
        }

        public override string DisplayName
        {
            get { return CalculateSignature(); }
        }

        public override NativeSymbolCategory Category
        {
            get { return NativeSymbolCategory.Procedure; }
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.ProcedureSignature; }
        }

        public NativeSignature()
        {
            this.Name = "Sig";
        }

        public string CalculateSignature(string name = null, bool includeSal = false)
        {
            StringBuilder builder = new StringBuilder();

            if (includeSal && !ReturnTypeSalAttribute.IsEmpty)
            {
                builder.Append(ReturnTypeSalAttribute.DisplayName);
                builder.Append(" ");
            }

            if (ReturnType != null)
            {
                builder.Append(ReturnType.DisplayName);
                builder.Append(" ");
            }

            if (!string.IsNullOrEmpty(name))
            {
                builder.Append(name);
            }

            builder.Append("(");

            for (int i = 0; i <= _paramList.Count - 1; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                NativeParameter cur = _paramList[i];
                if (includeSal && !cur.SalAttribute.IsEmpty)
                {
                    builder.Append(cur.SalAttribute.DisplayName);
                    builder.Append(" ");
                }

                if (string.IsNullOrEmpty(cur.Name))
                {
                    builder.Append(cur.NativeType.DisplayName);
                }
                else
                {
                    builder.AppendFormat("{0} {1}", cur.NativeType.DisplayName, cur.Name);
                }

            }

            builder.Append(")");
            return builder.ToString();
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            List<NativeSymbol> list = new List<NativeSymbol>();

            if (ReturnType != null)
            {
                list.Add(ReturnType);
            }

            if (ReturnTypeSalAttribute != null)
            {
                list.Add(ReturnTypeSalAttribute);
            }

            foreach (NativeParameter param in _paramList)
            {
                list.Add(param);
            }

            return list;
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            if (object.ReferenceEquals(oldChild, ReturnType))
            {
                ReplaceChildSingle(oldChild, newChild, ref ReturnType);
            }
            else if (object.ReferenceEquals(oldChild, ReturnTypeSalAttribute))
            {
                ReplaceChildSingle(oldChild, newChild, ref ReturnTypeSalAttribute);
            }
            else
            {
                ReplaceChildInList(oldChild, newChild, _paramList);
            }
        }
    }
}
