// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes.Enums;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Base type for Fake types
    /// </summary>
    /// <remarks></remarks>
    public class NativeNamedType : NativeProxyType
    {

        private string _qualification;

        private bool _isConst;
        /// <summary>
        /// When a type is referenced by it's full name (struct, union, enum) this holds the reference 
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Qualification
        {
            get
            {
                if (_qualification == null)
                {
                    return string.Empty;
                }
                return _qualification;
            }
            set { _qualification = value; }
        }

        /// <summary>
        /// Was this created with a "const" specifier?
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsConst
        {
            get { return _isConst; }
            set { _isConst = value; }
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.NamedType; }
        }

        public string RealTypeFullName
        {
            get
            {
                if (RealType != null)
                {
                    string name = null;
                    if (string.IsNullOrEmpty(_qualification))
                    {
                        name = RealType.DisplayName;
                    }
                    else
                    {
                        name = _qualification + " " + RealType.DisplayName;
                    }

                    if (IsConst)
                    {
                        return "const " + name;
                    }

                    return name;
                }
                else
                {
                    return "<null>";
                }
            }
        }

        public NativeNamedType(string qualification, string name) : base(name)
        {
            Qualification = qualification;
        }

        public NativeNamedType(string name) : base(name)
        {
        }

        public NativeNamedType(string name, bool isConst) : base(name)
        {
            _isConst = isConst;
        }

        public NativeNamedType(string qualification, string name, bool isConst) : base(name)
        {
            Qualification = qualification;
            _isConst = isConst;
        }

        public NativeNamedType(string name, NativeType realType) : base(name)
        {
            this.RealType = realType;
        }
    }
}
