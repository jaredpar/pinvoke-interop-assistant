// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.Text;
using static PInvoke.Contract;
using PInvoke.Parser;

namespace PInvoke
{
    #region "NativeSymbol"

    /// <summary>
    /// Category for the NativeType
    /// </summary>
    /// <remarks></remarks>
    public enum NativeSymbolCategory
    {
        Defined,
        Proxy,
        Specialized,
        Procedure,
        Extra
    }

    /// <summary>
    /// The kind of the native type.  Makes it easy to do switching
    /// </summary>
    /// <remarks></remarks>
    public enum NativeSymbolKind
    {
        StructType,
        EnumType,
        UnionType,
        ArrayType,
        PointerType,
        BuiltinType,
        TypedefType,
        BitVectorType,
        NamedType,
        Procedure,
        ProcedureSignature,
        FunctionPointer,
        Parameter,
        Member,
        EnumNameValue,
        Constant,
        SalEntry,
        SalAttribute,
        ValueExpression,
        Value,
        OpaqueType
    }

    /// <summary>
    /// Represents a native symbol we're interested in
    /// </summary>
    [DebuggerDisplay("{DisplayName}")]
    public abstract class NativeSymbol
    {
        // TODO: mutable data, destroy
        protected static List<NativeSymbol> EmptySymbolList = new List<NativeSymbol>();

        private string _name;

        /// <summary>
        /// Name of the C++ type
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Category of the NativeType
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract NativeSymbolCategory Category { get; }

        /// <summary>
        /// The kind of the type.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract NativeSymbolKind Kind { get; }

        /// <summary>
        /// Gets the full name of the type
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string DisplayName
        {
            get { return Name; }
        }

        /// <summary>
        /// Whether one of it's immediate children is not resolved
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool IsImmediateResolved
        {
            get { return true; }
        }

        protected NativeSymbol()
        {
        }

        protected NativeSymbol(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets the immediate children of this symbol
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual IEnumerable<NativeSymbol> GetChildren()
        {
            return EmptySymbolList;
        }

        public virtual void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            throw new NotImplementedException();
        }

        protected void ReplaceChildInList<T>(NativeSymbol oldChild, NativeSymbol newChild, List<T> list) where T : NativeSymbol
        {
            ThrowIfNull(list);

            if (oldChild == null)
            {
                throw new ArgumentNullException("oldChild");
            }
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }

            T oldTyped = oldChild as T;
            T newTyped = newChild as T;
            if (oldTyped == null || newTyped == null)
            {
                throw new InvalidOperationException("Operands are of the wrong type");
            }

            int index = list.IndexOf(oldTyped);
            if (index < 0)
            {
                throw new InvalidOperationException("Old operand not a current child");
            }

            list.RemoveAt(index);
            list.Insert(index, newTyped);
        }

        protected IEnumerable<NativeSymbol> GetSingleChild<T>(T child) where T : NativeSymbol
        {
            if (child == null)
            {
                return new List<NativeSymbol>();
            }

            List<NativeSymbol> list = new List<NativeSymbol>();
            list.Add(child);
            return list;
        }

        protected IEnumerable<NativeSymbol> GetListChild<T>(List<T> list) where T : NativeSymbol
        {
            List<NativeSymbol> symList = new List<NativeSymbol>();
            foreach (T value in list)
            {
                symList.Add(value);
            }

            return symList;
        }


        protected void ReplaceChildSingle<T>(NativeSymbol oldchild, NativeSymbol newChild, ref T realChild) where T : NativeSymbol
        {
            if (!object.ReferenceEquals(oldchild, realChild))
            {
                throw new InvalidOperationException("Old child is wrong");
            }

            if (newChild == null)
            {
                realChild = null;
                return;
            }

            T newTyped = newChild as T;
            if (newTyped == null)
            {
                throw new InvalidOperationException("Operands are of the wrong type");
            }

            realChild = newTyped;
        }

    }

    #endregion

    #region "NativeType"

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
        public NativeType DigThroughTypedefAndNamedTypes()
        {

            NativeType cur = this;
            while (cur != null)
            {
                if (cur.Kind == NativeSymbolKind.NamedType)
                {
                    cur = ((NativeNamedType)cur).RealType;
                }
                else if (cur.Kind == NativeSymbolKind.TypedefType)
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

        public NativeType DigThroughTypedefAndNamedTypesFor(string search)
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
                else if (cur.Kind == NativeSymbolKind.TypedefType)
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

    #endregion

    #region "NativeDefinedType"

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

    #endregion

    #region "Native Defined Types"

    #region "NativeStruct"

    /// <summary>
    /// Represents a C++ struct
    /// </summary>
    /// <remarks></remarks>
    public class NativeStruct : NativeDefinedType
    {
        public override NativeSymbolKind Kind => NativeSymbolKind.StructType;

        public override NativeNameKind NameKind => NativeNameKind.Struct;

        public NativeStruct()
        {
        }

        public NativeStruct(string name) : base(name)
        {
        }

    }

    #endregion

    #region "NativeUnion"

    /// <summary>
    /// Represents a C++ Union
    /// </summary>
    /// <remarks></remarks>
    public class NativeUnion : NativeDefinedType
    {
        public override NativeSymbolKind Kind => NativeSymbolKind.UnionType;

        public override NativeNameKind NameKind => NativeNameKind.Union;

        public NativeUnion()
        {
        }

        public NativeUnion(string name) : base(name)
        {
        }

    }
    #endregion

    #region "NativeEnum"

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

    /// <summary>
    /// An enum value
    /// </summary>
    [DebuggerDisplay("{Name} = {Value}")]
    public class NativeEnumValue : NativeExtraSymbol
    {
        private NativeValueExpression _value;

        /// <summary>
        /// Value of the value
        /// </summary>
        public NativeValueExpression Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public NativeName NativeName => new NativeName(Name, NativeNameKind.EnumValue);

        public NativeEnumValue(string name) : this(name, string.Empty)
        {
        }

        public NativeEnumValue(string name, string value)
        {
            this.Name = name;
            _value = new NativeValueExpression(value);
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.EnumNameValue; }
        }

        public override IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(_value);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildSingle(oldChild, newChild, ref _value);
        }
    }

    #endregion

    #region "NativeFunctionPointer"

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

    #endregion

    #endregion

    #region "NativeProxyType"

    /// <summary>
    /// Base class for proxy types.  That is types which are actually a simple modification on another
    /// type.  This is typically name based such as typedefs or type based such as arrays and pointers
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayName}")]
    public abstract class NativeProxyType : NativeType
    {
        private NativeType _realType;
        /// <summary>
        /// Underlying type of the array
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeType RealType
        {
            get { return _realType; }
            set { _realType = value; }
        }

        public NativeType RealTypeDigged
        {
            get
            {
                if (_realType != null)
                {
                    return _realType.DigThroughTypedefAndNamedTypes();
                }

                return _realType;
            }
        }

        public override bool IsImmediateResolved
        {
            get { return _realType != null; }
        }

        public override NativeSymbolCategory Category
        {
            get { return NativeSymbolCategory.Proxy; }
        }

        protected NativeProxyType(string name) : base(name)
        {
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(RealType);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildSingle(oldChild, newChild, ref _realType);
        }
    }

    #endregion

    #region "Proxy Types"

    #region "NativeArray"

    public class NativeArray : NativeProxyType
    {
        private int _elementCount = -1;

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.ArrayType; }
        }

        /// <summary>
        /// Element count of the array.  If the array is not bound then this will
        /// be -1
        /// TODO: use a nullable here
        /// </summary>
        public int ElementCount
        {
            get { return _elementCount; }
            set { _elementCount = value; }
        }

        /// <summary>
        /// Create the display name of the array
        /// </summary>
        public override string DisplayName
        {
            get
            {
                string suffix = null;
                if (_elementCount >= 0)
                {
                    suffix = string.Format("[{0}]", this.ElementCount);
                }
                else
                {
                    suffix = "[]";
                }

                if (RealType == null)
                {
                    return "<null>" + suffix;
                }
                else
                {
                    return RealType.DisplayName + suffix;
                }
            }
        }

        public NativeArray() : base("[]")
        {
        }

        public NativeArray(NativeType realType, Int32 elementCount) : base("[]")
        {
            this.RealType = realType;
            this.ElementCount = elementCount;
        }

        public NativeArray(BuiltinType bt, Int32 elementCount) : this(new NativeBuiltinType(bt), elementCount)
        {
        }

    }
    #endregion

    #region "NativePointer"

    /// <summary>
    /// A Pointer
    /// </summary>
    /// <remarks></remarks>
    public class NativePointer : NativeProxyType
    {

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.PointerType; }
        }

        /// <summary>
        /// Returs the pointer full type name
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string DisplayName
        {
            get
            {
                if (RealType == null)
                {
                    return "<null>*";
                }
                else
                {
                    return RealType.DisplayName + "*";
                }
            }
        }

        public NativePointer() : base("*")
        {
        }

        public NativePointer(NativeType realtype) : base("*")
        {
            this.RealType = realtype;
        }

        public NativePointer(BuiltinType bt) : base("*")
        {
            this.RealType = new NativeBuiltinType(bt);
        }

    }
    #endregion

    #region "NativeNamedType"

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
            _qualification = qualification;
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
            _qualification = qualification;
            _isConst = isConst;
        }

        public NativeNamedType(string name, NativeType realType) : base(name)
        {
            this.RealType = realType;
        }

    }

    #endregion

    #region "NativeTypeDef"

    /// <summary>
    /// TypeDef of a type.  At first it seems like this should be a NativeProxyType.  However 
    /// NativeProxyTypes aren't really types.  They are just references or modifiers to a type.  A
    /// Typedef is itself a type and accessible by name
    /// </summary>
    [DebuggerDisplay("{FullName} -> {RealTypeFullname}")]
    public class NativeTypeDef : NativeProxyType
    {
        public override NativeSymbolKind Kind => NativeSymbolKind.TypedefType;

        public NativeName NativeName => new NativeName(Name, NativeNameKind.TypeDef);

        public NativeTypeDef(string name) : base(name)
        {
        }

        public NativeTypeDef(string name, string realtypeName) : base(name)
        {
            this.RealType = new NativeNamedType(realtypeName);
        }

        public NativeTypeDef(string name, NativeType realtype) : base(name)
        {
            this.RealType = realtype;
        }

        public NativeTypeDef(string name, BuiltinType bt) : base(name)
        {
            this.RealType = new NativeBuiltinType(bt);
        }

    }

    #endregion

    #endregion

    #region "Specialized Types"

    /// <summary>
    /// Types that are specialized for generation
    /// </summary>
    /// <remarks></remarks>
    public abstract class NativeSpecializedType : NativeType
    {

        public override NativeSymbolCategory Category
        {
            get { return NativeSymbolCategory.Specialized; }
        }


        protected NativeSpecializedType()
        {
        }

        protected NativeSpecializedType(string name) : base(name)
        {
        }
    }

    #region "NativeBitVector"

    /// <summary>
    /// A native bit vector.  All bitvectors are generated as anonymous structs inside the 
    /// conttaining generated struct
    /// </summary>
    /// <remarks></remarks>
    public class NativeBitVector : NativeSpecializedType
    {
        private int _size;
        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.BitVectorType; }
        }

        /// <summary>
        /// Size of the bitvector
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public override string DisplayName
        {
            get { return "<bitvector " + Size + ">"; }
        }

        public NativeBitVector() : this(-1)
        {
        }

        public NativeBitVector(int size)
        {
            this.Name = "<bitvector>";
            _size = size;
        }

    }

    #endregion

    #region "NativeBuiltinType"

    /// <summary>
    /// Enumeration of the common C++ builtin types
    /// </summary>
    public enum BuiltinType
    {
        NativeInt16,
        NativeInt32,
        NativeInt64,
        NativeFloat,
        NativeDouble,
        NativeBoolean,
        NativeChar,
        NativeWChar,
        NativeByte,
        NativeVoid,

        /// <summary>
        /// Used for BuiltinTypes initially missed
        /// </summary>
        NativeUnknown
    }

    /// <summary>
    /// Built-in types (int, boolean, etc ...)
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayName}")]
    public class NativeBuiltinType : NativeSpecializedType
    {
        private BuiltinType _builtinType;
        private bool _isUnsigned;
        private Type _managedType;

        private System.Runtime.InteropServices.UnmanagedType _unmanagedType;
        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.BuiltinType; }
        }

        public override string DisplayName
        {
            get
            {
                if (_builtinType == PInvoke.BuiltinType.NativeUnknown)
                {
                    return Name;
                }

                string str = Name;
                if (IsUnsigned)
                {
                    str = "unsigned " + str;
                }

                return str;
            }
        }

        /// <summary>
        /// Bulitin Type
        /// </summary>
        public BuiltinType BuiltinType
        {
            get { return _builtinType; }
            set { _builtinType = value; }
        }

        public bool IsUnsigned
        {
            get { return _isUnsigned; }
            set
            {
                _isUnsigned = value;
                Init();
            }
        }

        public Type ManagedType
        {
            get { return _managedType; }
        }

        public System.Runtime.InteropServices.UnmanagedType UnmanagedType
        {
            get { return _unmanagedType; }
        }

        public NativeBuiltinType(BuiltinType bt) : base("")
        {
            _builtinType = bt;
            Init();
        }

        public NativeBuiltinType(BuiltinType bt, bool isUnsigned) : this(bt)
        {
            this.IsUnsigned = isUnsigned;
            Init();
        }

        public NativeBuiltinType(string name) : base(name)
        {
            _builtinType = PInvoke.BuiltinType.NativeUnknown;
            Init();
        }

        private void Init()
        {
            switch (this.BuiltinType)
            {
                case PInvoke.BuiltinType.NativeBoolean:
                    Name = "boolean";
                    _managedType = typeof(bool);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.Bool;
                    break;
                case PInvoke.BuiltinType.NativeByte:
                    Name = "byte";
                    _managedType = typeof(byte);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I1;
                    break;
                case PInvoke.BuiltinType.NativeInt16:
                    Name = "short";
                    if (IsUnsigned)
                    {
                        _managedType = typeof(UInt16);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.U2;
                    }
                    else
                    {
                        _managedType = typeof(Int16);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I2;
                    }
                    break;
                case PInvoke.BuiltinType.NativeInt32:
                    Name = "int";
                    if (IsUnsigned)
                    {
                        _managedType = typeof(UInt32);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.U4;
                    }
                    else
                    {
                        _managedType = typeof(Int32);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I4;
                    }
                    break;
                case PInvoke.BuiltinType.NativeInt64:
                    Name = "__int64";
                    if (IsUnsigned)
                    {
                        _managedType = typeof(UInt64);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.U8;
                    }
                    else
                    {
                        _managedType = typeof(Int64);
                        _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I8;
                    }
                    break;
                case PInvoke.BuiltinType.NativeChar:
                    Name = "char";
                    _managedType = typeof(byte);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I1;
                    break;
                case PInvoke.BuiltinType.NativeWChar:
                    Name = "wchar";
                    _managedType = typeof(char);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.I2;
                    break;
                case PInvoke.BuiltinType.NativeFloat:
                    Name = "float";
                    _managedType = typeof(float);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.R4;
                    break;
                case PInvoke.BuiltinType.NativeDouble:
                    Name = "double";
                    _managedType = typeof(double);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.R8;
                    break;
                case PInvoke.BuiltinType.NativeVoid:
                    Name = "void";
                    _managedType = typeof(void);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.AsAny;
                    break;
                case PInvoke.BuiltinType.NativeUnknown:
                    Name = "unknown";
                    _managedType = typeof(object);
                    _unmanagedType = System.Runtime.InteropServices.UnmanagedType.AsAny;
                    break;
                default:
                    ThrowInvalidEnumValue(BuiltinType);
                    break;
            }

        }

        public static bool TryConvertToBuiltinType(string name, out NativeBuiltinType nativeBt)
        {
            Parser.TokenType tt = default(Parser.TokenType);
            if (Parser.TokenHelper.KeywordMap.TryGetValue(name, out tt))
            {
                return TryConvertToBuiltinType(tt, out nativeBt);
            }

            nativeBt = null;
            return false;
        }

        public static bool TryConvertToBuiltinType(Parser.TokenType tt, out NativeBuiltinType nativeBt)
        {
            if (!Parser.TokenHelper.IsTypeKeyword(tt))
            {
                nativeBt = null;
                return false;
            }

            BuiltinType bt = default(BuiltinType);
            bool isUnsigned = false;
            switch (tt)
            {
                case Parser.TokenType.BooleanKeyword:
                    bt = PInvoke.BuiltinType.NativeBoolean;
                    break;
                case Parser.TokenType.ByteKeyword:
                    bt = PInvoke.BuiltinType.NativeByte;
                    break;
                case Parser.TokenType.ShortKeyword:
                case Parser.TokenType.Int16Keyword:
                    bt = PInvoke.BuiltinType.NativeInt16;
                    break;
                case Parser.TokenType.IntKeyword:
                case Parser.TokenType.LongKeyword:
                case Parser.TokenType.SignedKeyword:
                    bt = PInvoke.BuiltinType.NativeInt32;
                    break;
                case Parser.TokenType.UnsignedKeyword:
                    bt = PInvoke.BuiltinType.NativeInt32;
                    isUnsigned = true;
                    break;
                case Parser.TokenType.Int64Keyword:
                    bt = PInvoke.BuiltinType.NativeInt64;
                    break;
                case Parser.TokenType.CharKeyword:
                    bt = PInvoke.BuiltinType.NativeChar;
                    break;
                case Parser.TokenType.WCharKeyword:
                    bt = PInvoke.BuiltinType.NativeWChar;
                    break;
                case Parser.TokenType.FloatKeyword:
                    bt = PInvoke.BuiltinType.NativeFloat;
                    break;
                case Parser.TokenType.DoubleKeyword:
                    bt = PInvoke.BuiltinType.NativeDouble;
                    break;
                case Parser.TokenType.VoidKeyword:
                    bt = PInvoke.BuiltinType.NativeVoid;
                    break;
                default:
                    bt = PInvoke.BuiltinType.NativeUnknown;
                    ThrowInvalidEnumValue(tt);
                    break;
            }

            nativeBt = new NativeBuiltinType(bt, isUnsigned);
            return true;
        }

        public static string BuiltinTypeToName(BuiltinType bt)
        {
            NativeBuiltinType nativeBt = new NativeBuiltinType(bt);
            return nativeBt.Name;
        }

        public static bool IsNumberType(BuiltinType bt)
        {

            if (bt == PInvoke.BuiltinType.NativeInt16 || bt == PInvoke.BuiltinType.NativeInt32 || bt == PInvoke.BuiltinType.NativeInt64 || bt == PInvoke.BuiltinType.NativeFloat || bt == PInvoke.BuiltinType.NativeDouble || bt == PInvoke.BuiltinType.NativeByte)
            {
                return true;
            }

            return false;
        }

    }

    #endregion

    #region "NativeOpaqueType"

    /// <summary>
    /// Represents a type that is intentionally being hidden from the user.  Usually takes the following form
    /// typedef struct UndefinedType *PUndefinedType
    /// 
    /// PUndefinedType is a legal pointer reference and the struct "foo" can later be defined in a .c/.cpp file
    /// </summary>
    /// <remarks></remarks>
    public class NativeOpaqueType : NativeSpecializedType
    {

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.OpaqueType; }
        }

        public NativeOpaqueType() : base("Opaque")
        {
        }
    }

    #endregion

    #endregion

    #region "NativeProcedure"

    public enum NativeCallingConvention
    {
        /// <summary>
        /// Platform default
        /// </summary>
        /// <remarks></remarks>
        WinApi = 1,

        /// <summary>
        /// __stdcall
        /// </summary>
        /// <remarks></remarks>
        Standard,

        /// <summary>
        /// __cdecl
        /// </summary>
        /// <remarks></remarks>
        CDeclaration,

        /// <summary>
        /// __clrcall
        /// </summary>
        /// <remarks></remarks>
        Clr,

        /// <summary>
        /// __pascal
        /// </summary>
        /// <remarks></remarks>
        Pascal,

        /// <summary>
        /// inline, __inline, etc
        /// </summary>
        /// <remarks></remarks>
        Inline
    }

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

    #endregion

    #region "Extra Symbols"

    public abstract class NativeExtraSymbol : NativeSymbol
    {

        public override NativeSymbolCategory Category
        {
            get { return NativeSymbolCategory.Extra; }
        }

    }

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
                    return NativeType.DigThroughTypedefAndNamedTypes();
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

    /// <summary>
    /// Represents a member of a native type.
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{NativeType.FullName} {Name}")]
    public sealed class NativeMember : NativeExtraSymbol
    {
        /// <summary>
        /// Nativetype of the member
        /// </summary>
        public NativeType NativeType;

        public NativeType NativeTypeDigged
        {
            get
            {
                if (NativeType != null)
                {
                    return NativeType.DigThroughTypedefAndNamedTypes();
                }

                return null;
            }
        }

        public override bool IsImmediateResolved
        {
            get { return NativeType != null && !string.IsNullOrEmpty(Name); }
        }


        public NativeMember()
        {
        }

        public NativeMember(string name, NativeType nt)
        {
            Name = name;
            NativeType = nt;
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.Member; }
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(NativeType);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildSingle(oldChild, newChild, ref NativeType);
        }

    }

    public enum ConstantKind
    {
        Macro,
        MacroMethod
    }

    /// <summary>
    /// Constant in Native code
    /// </summary>
    public class NativeConstant : NativeExtraSymbol
    {
        private NativeValueExpression _value;

        private ConstantKind _constantKind;
        /// <summary>
        /// What type of constant is this
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public ConstantKind ConstantKind
        {
            get { return _constantKind; }
            set { _constantKind = value; }
        }

        /// <summary>
        /// Value for the constant
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeValueExpression Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string RawValue
        {
            get
            {
                if (_value == null)
                {
                    return string.Empty;
                }

                return _value.Expression;
            }
        }

        public override NativeSymbolKind Kind => NativeSymbolKind.Constant;

        public NativeName NativeName => new NativeName(Name, NativeNameKind.Constant);

        private NativeConstant()
        {
        }

        public NativeConstant(string name) : this(name, null)
        {
        }

        public NativeConstant(string name, string value) : this(name, value, PInvoke.ConstantKind.Macro)
        {
        }

        public NativeConstant(string name, string value, ConstantKind kind)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
            _constantKind = kind;

            // We don't support macro methods at this point.  Instead we will just generate out the 
            // method signature for the method and print the string out into the code
            if (ConstantKind == PInvoke.ConstantKind.MacroMethod)
            {
                value = "\"" + value + "\"";
            }

            _value = new NativeValueExpression(value);
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            return GetSingleChild(_value);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            ReplaceChildSingle(oldChild, newChild, ref _value);
        }

    }

    /// <summary>
    /// Represents the value of an experession
    /// </summary>
    public class NativeValueExpression : NativeExtraSymbol
    {
        private string _expression;
        private List<NativeValue> _valueList;
        private ExpressionNode _node;

        private bool _errorParsingExpr = false;

        /// <summary>
        /// Value of the expression
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Expression
        {
            get { return _expression; }
            set
            {
                ResetValueList();
                _expression = value;
            }
        }

        public bool IsParsable
        {
            get
            {
                EnsureValueList();
                return !_errorParsingExpr;
            }
        }

        /// <summary>
        /// Is this an empty expression
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(_expression); }
        }

        /// <summary>
        /// Root expression node
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public ExpressionNode Node
        {
            get
            {
                EnsureValueList();
                return _node;
            }
        }

        /// <summary>
        /// List of values in the expression
        /// </summary>
        public List<NativeValue> Values
        {
            get
            {
                EnsureValueList();
                return _valueList;
            }
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.ValueExpression; }
        }

        public NativeValueExpression(string expr)
        {
            this.Name = "Value";
            _expression = expr;
        }

        private void ResetValueList()
        {
            _valueList = null;
            _node = null;
        }

        public void EnsureValueList()
        {
            if (_valueList != null)
            {
                return;
            }

            if (IsEmpty)
            {
                _valueList = new List<NativeValue>();
                _errorParsingExpr = false;
                return;
            }

            var parser = new ExpressionParser();
            _valueList = new List<NativeValue>();

            // It's valid no have an invalid expression :)
            if (!parser.TryParse(_expression, out _node))
            {
                _errorParsingExpr = true;
                _node = null;
            }
            else
            {
                _errorParsingExpr = false;
            }

            CalculateValueList(_node);
        }

        private void CalculateValueList(ExpressionNode cur)
        {
            if (cur == null)
            {
                return;
            }

            if (cur.Kind == Parser.ExpressionKind.Leaf)
            {
                var ntVal = NativeValue.TryCreateForLeaf(cur, bag: null);

                if (ntVal != null)
                {
                    _valueList.Add(ntVal);
                }
                else
                {
                    _errorParsingExpr = true;
                }
            }
            else if (cur.Kind == Parser.ExpressionKind.Cast)
            {
                // Create nodes for the cast expressions.  The target should be a symbol
                _valueList.Add(NativeValue.CreateSymbolType(cur.Token.Value));
            }

            CalculateValueList(cur.LeftNode);
            CalculateValueList(cur.RightNode);
        }

        /// <summary>
        /// A Native value expression is resolved.  It may output as an error string but it will output
        /// a value.  This is needed to support constants that are defined to non-valid code but we still
        /// have to output the string value
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool IsImmediateResolved
        {
            get { return true; }
        }

        public override System.Collections.Generic.IEnumerable<NativeSymbol> GetChildren()
        {
            EnsureValueList();
            return base.GetListChild(_valueList);
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            EnsureValueList();
            base.ReplaceChildInList(oldChild, newChild, _valueList);
        }
    }

    public enum NativeValueKind
    {
        Number,
        String,
        Character,
        Boolean,

        /// <summary>
        /// Used when the value needs a Symbol which represents a Value
        /// </summary>
        /// <remarks></remarks>
        SymbolValue,

        /// <summary>
        /// Used when the value needs a Symbol which represents a Type.  For instance
        /// a Cast expression needs a Type Symbol rather than a Value symbol
        /// </summary>
        /// <remarks></remarks>
        SymbolType
    }

    /// <summary>
    /// Represents a value inside of an expression
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{Value} ({ValueKind})")]
    public class NativeValue : NativeExtraSymbol
    {
        private NativeValueKind _valueKind;

        private object _value;
        /// <summary>
        /// The actual value
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public NativeSymbol SymbolValue
        {
            get
            {
                if ((_valueKind == NativeValueKind.SymbolValue))
                {
                    return (NativeSymbol)_value;
                }

                return null;
            }
        }

        public NativeSymbol SymbolType
        {
            get
            {
                if ((_valueKind == NativeValueKind.SymbolType))
                {
                    return (NativeSymbol)_value;
                }

                return null;
            }
        }

        /// <summary>
        /// What kind of value is this
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NativeValueKind ValueKind
        {
            get { return _valueKind; }
        }

        /// <summary>
        /// Is the value resolvable
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsValueResolved
        {
            get
            {
                switch (this.ValueKind)
                {
                    case NativeValueKind.Number:
                    case NativeValueKind.String:
                    case NativeValueKind.Character:
                    case NativeValueKind.Boolean:
                        return this._value != null;
                    case NativeValueKind.SymbolType:
                        return SymbolType != null;
                    case NativeValueKind.SymbolValue:
                        return SymbolValue != null;
                    default:
                        ThrowInvalidEnumValue(this.ValueKind);
                        return false;
                }
            }
        }

        /// <summary>
        /// Get the value as a display string
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string DisplayValue
        {
            get
            {
                switch (_valueKind)
                {
                    case NativeValueKind.Number:
                        return _value.ToString();
                    case NativeValueKind.String:
                        return _value.ToString();
                    case NativeValueKind.Character:
                        return _value.ToString();
                    case NativeValueKind.SymbolType:
                        if (SymbolType != null)
                        {
                            return SymbolType.DisplayName;
                        }

                        return Name;
                    case NativeValueKind.SymbolValue:
                        if (SymbolValue != null)
                        {
                            return SymbolValue.DisplayName;
                        }

                        return Name;
                    default:
                        ThrowInvalidEnumValue(_valueKind);
                        return string.Empty;
                }
            }
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.Value; }
        }

        private NativeValue(object value, NativeValueKind kind) : this(kind.ToString(), value, kind)
        {
        }

        private NativeValue(string name, object value, NativeValueKind kind)
        {
            this.Name = name;
            _valueKind = kind;
            _value = value;
        }

        public override IEnumerable<NativeSymbol> GetChildren()
        {
            if (_valueKind == NativeValueKind.SymbolType)
            {
                return GetSingleChild(SymbolType);
            }
            else if (_valueKind == NativeValueKind.SymbolValue)
            {
                return GetSingleChild(SymbolValue);
            }
            else
            {
                return GetSingleChild<NativeSymbol>(null);
            }
        }

        public override void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            if (_valueKind == NativeValueKind.SymbolType)
            {
                NativeSymbol x = null;
                ReplaceChildSingle(SymbolType, newChild, ref x);
                Value = x;
            }
            else if (_valueKind == NativeValueKind.SymbolValue)
            {
                NativeSymbol x = null;
                ReplaceChildSingle(SymbolValue, newChild, ref x);
                Value = x;
            }
        }

        public static NativeValue CreateNumber(int i)
        {
            return CreateNumber(new Number(i));
        }

        public static NativeValue CreateNumber(Number n)
        {
            // TODO: Consider passing Number through here.
            return new NativeValue(n.Value, NativeValueKind.Number);
        }

        public static NativeValue CreateBoolean(bool b)
        {
            return new NativeValue(b, NativeValueKind.Boolean);
        }

        public static NativeValue CreateString(string s)
        {
            return new NativeValue(s, NativeValueKind.String);
        }

        public static NativeValue CreateCharacter(char c)
        {
            return new NativeValue(c, NativeValueKind.Character);
        }

        public static NativeValue CreateSymbolValue(string name)
        {
            return new NativeValue(name, null, NativeValueKind.SymbolValue);
        }

        public static NativeValue CreateSymbolValue(string name, NativeSymbol ns)
        {
            return new NativeValue(name, ns, NativeValueKind.SymbolValue);
        }

        public static NativeValue CreateSymbolType(string name)
        {
            return new NativeValue(name, null, NativeValueKind.SymbolType);
        }

        public static NativeValue CreateSymbolType(string name, NativeSymbol ns)
        {
            return new NativeValue(name, ns, NativeValueKind.SymbolType);
        }

        public static NativeValue TryCreateForLeaf(ExpressionNode cur, NativeSymbolBag bag)
        {
            ThrowIfNull(cur);
            ThrowIfFalse(cur.Kind == ExpressionKind.Leaf);

            Token token = cur.Token;
            NativeValue ntVal = null;
            if (token.IsQuotedString)
            {
                string strValue = null;
                if (TokenHelper.TryConvertToString(token, out strValue))
                {
                    ntVal = NativeValue.CreateString(strValue);
                }
            }
            else if (token.IsNumber)
            {
                Number value;
                if (TokenHelper.TryConvertToNumber(token, out value))
                {
                    ntVal = NativeValue.CreateNumber(value);
                }
            }
            else if (token.IsCharacter)
            {
                char cValue = 'c';
                if (TokenHelper.TryConvertToChar(token, out cValue))
                {
                    ntVal = NativeValue.CreateCharacter(cValue);
                }
                else
                {
                    ntVal = NativeValue.CreateString(token.Value);
                }
            }
            else if (token.TokenType == TokenType.TrueKeyword)
            {
                ntVal = NativeValue.CreateBoolean(true);
            }
            else if (token.TokenType == Parser.TokenType.FalseKeyword)
            {
                ntVal = NativeValue.CreateBoolean(false);
            }
            else if (token.IsAnyWord)
            {
                NativeConstant constant;
                NativeEnum enumeration;
                NativeEnumValue value;
                if (bag != null && bag.TryGetGlobalSymbol(token.Value, out constant))
                {
                    ntVal = NativeValue.CreateSymbolValue(token.Value, constant);
                }
                else if (bag != null && bag.TryGetEnumByValueName(token.Value, out enumeration, out value))
                {
                    ntVal = NativeValue.CreateSymbolValue(token.Value, enumeration);
                }
                else
                {
                    ntVal = NativeValue.CreateSymbolValue(token.Value);
                }
            }

            return ntVal;
        }
    }

    #region "SAL attributes"

    public enum SalEntryType
    {
        Null,
        NotNull,
        MaybeNull,
        ReadOnly,
        NotReadOnly,
        MaybeReadOnly,
        Valid,
        NotValid,
        MaybeValid,
        ReadableTo,
        ElemReadableTo,
        ByteReadableTo,
        WritableTo,
        ElemWritableTo,
        ByteWritableTo,
        Deref,
        Pre,
        Post,
        ExceptThat,
        InnerControlEntryPoint,
        InnerDataEntryPoint,
        InnerSucces,
        InnerCheckReturn,
        InnerTypefix,
        InnerOverride,
        InnerCallBack,
        InnerBlocksOn
    }

    /// <summary>
    /// Represents a SAL attribute in code
    /// </summary>
    /// <remarks></remarks>
    [DebuggerDisplay("{DisplayName}")]
    public sealed class NativeSalEntry : NativeExtraSymbol
    {
        private SalEntryType _type;

        private string _text;

        /// <summary>
        /// Type of attribute
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public SalEntryType SalEntryType
        {
            get { return _type; }
            set
            {
                _type = value;
                this.Name = value.ToString();
            }
        }

        /// <summary>
        /// Text of the attribute
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public override string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return Name;
                }
                else
                {
                    return string.Format("{0}({1})", Name, Text);
                }
            }
        }

        public override NativeSymbolKind Kind
        {
            get { return NativeSymbolKind.SalEntry; }
        }

        public NativeSalEntry() : this(SalEntryType.Null, string.Empty)
        {

        }

        public NativeSalEntry(SalEntryType type) : this(type, string.Empty)
        {
        }

        public NativeSalEntry(SalEntryType type, string text)
        {
            this.SalEntryType = type;
            _type = type;
            _text = text;
        }

        public static string GetDirectiveForEntry(SalEntryType entry)
        {
            switch (entry)
            {
                case SalEntryType.Null:
                    return "SAL_null";
                case SalEntryType.NotNull:
                    return "SAL_notnull";
                case SalEntryType.MaybeNull:
                    return "SAL_maybenull";
                case SalEntryType.ReadOnly:
                    return "SAL_readonly";
                case SalEntryType.NotReadOnly:
                    return "SAL_notreadonly";
                case SalEntryType.MaybeReadOnly:
                    return "SAL_maybereadonly";
                case SalEntryType.Valid:
                    return "SAL_valid";
                case SalEntryType.NotValid:
                    return "SAL_notvalid";
                case SalEntryType.MaybeValid:
                    return "SAL_maybevalid";
                case SalEntryType.ReadableTo:
                    return "SAL_readableTo()";
                case SalEntryType.ElemReadableTo:
                    return "SAL_readableTo(elementCount())";
                case SalEntryType.ByteReadableTo:
                    return "SAL_readableTo(byteCount())";
                case SalEntryType.WritableTo:
                    return "SAL_writableTo()";
                case SalEntryType.ElemWritableTo:
                    return "SAL_writableTo(elementCount())";
                case SalEntryType.ByteWritableTo:
                    return "SAL_writableTo(byteCount())";
                case SalEntryType.Deref:
                    return "SAL_deref";
                case SalEntryType.Pre:
                    return "SAL_pre";
                case SalEntryType.Post:
                    return "SAL_post";
                case SalEntryType.ExceptThat:
                    return "SAL_except";
                case SalEntryType.InnerControlEntryPoint:
                    return "SAL_entrypoint(controlEntry, )";
                case SalEntryType.InnerDataEntryPoint:
                    return "SAL_entrypoint(dataEntry, )";
                case SalEntryType.InnerSucces:
                    return "SAL_success()";
                case SalEntryType.InnerCheckReturn:
                    return "SAL_checkReturn";
                case SalEntryType.InnerTypefix:
                    return "SAL_typefix";
                case SalEntryType.InnerOverride:
                    return "__override";
                case SalEntryType.InnerCallBack:
                    return "__callback";
                case SalEntryType.InnerBlocksOn:
                    return "SAL_blocksOn()";
                default:
                    ThrowInvalidEnumValue(entry);
                    return string.Empty;
            }
        }

    }

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

    #endregion

    #region "NativeProcedureSignature"

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

    #endregion

    #endregion
}
