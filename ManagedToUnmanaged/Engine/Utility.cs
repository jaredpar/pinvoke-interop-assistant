/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SignatureGenerator
{
    #region ParameterInfoEx

    internal class ParameterInfoEx : ParameterInfo
    {
        private object[] customAttributes;

        public ParameterInfoEx(ParameterAttributes attrs, Type type, object defaultValue, MemberInfo member,
            object[] customAttributes, string name, int position)
        {
            this.AttrsImpl = attrs;
            this.ClassImpl = type;
            this.DefaultValueImpl = defaultValue;
            this.MemberImpl = member;
            this.customAttributes = customAttributes;
            this.NameImpl = name;
            this.PositionImpl = position;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return customAttributes;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            List<object> attrs = new List<object>();
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (attributeType.IsAssignableFrom(customAttributes[i].GetType()))
                {
                    attrs.Add(customAttributes[i]);
                }
            }

            return attrs.ToArray();
        }
    }

    #endregion

    #region Set<T>

    public delegate bool Transformer<T>(ref T arg);

    public class Set<T> : ICollection<T>
    {
        private enum WalkState
        {
            OnStack, ForwardYielded, Yielded
        }

        private Dictionary<T, List<T>> dictionary;
        private Transformer<T> forwardTransformer;

        #region Construction

        public Set()
            : this((Transformer<T>)null)
        { }

        public Set(Transformer<T> forwardTransformer)
        {
            this.dictionary = new Dictionary<T, List<T>>();
            this.forwardTransformer = forwardTransformer;
        }

        public Set(int capacity, Transformer<T> forwardTransformer)
        {
            this.dictionary = new Dictionary<T, List<T>>(capacity);
            this.forwardTransformer = forwardTransformer;
        }

        public Set(int capacity)
            : this(capacity, null)
        { }

        public Set(ICollection<T> collection, Transformer<T> forwardTransformer)
        {
            dictionary = new Dictionary<T, List<T>>(collection.Count);

            foreach (T item in collection)
            {
                dictionary.Add(item, null);
            }

            this.forwardTransformer = forwardTransformer;
        }

        public Set(ICollection<T> collection)
            : this(collection, null)
        { }

        #endregion

        #region Dependencies

        public void AddDependency(T parentItem, T childItem)
        {
            if (!dictionary.ContainsKey(parentItem))
                throw new InvalidOperationException();

            if (parentItem.Equals(childItem)) return;

            List<T> list = dictionary[parentItem];
            if (list == null)
            {
                list = new List<T>(1);
                dictionary[parentItem] = list;
            }

            list.Add(childItem);
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            Debug.Assert(!Contains(item));
            dictionary.Add(item, null);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            dictionary.Keys.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return dictionary.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public virtual IEnumerator<T> GetEnumerator()
        {
            Dictionary<T, WalkState> returned_items = new Dictionary<T, WalkState>(this.Count);

            foreach (KeyValuePair<T, List<T>> pair in dictionary)
            {
                foreach (T item in RecursiveEnumerator(returned_items, pair))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<T> RecursiveEnumerator(Dictionary<T, WalkState> returnedItems, KeyValuePair<T, List<T>> pair)
        {
            // prevent infinite recursion by adding it now
            WalkState flag;
            if (returnedItems.TryGetValue(pair.Key, out flag))
            {
                if (flag == WalkState.OnStack)
                {
                    // this item has not been returned, but is on the stack
                    if (forwardTransformer != null)
                    {
                        T item = pair.Key;

                        // use the transformer to return "something else"
                        if (forwardTransformer(ref item))
                        {
                            yield return item;
                        }
                    }
                    returnedItems[pair.Key] = WalkState.ForwardYielded;
                }
                yield break;
            }
            else
            {
                returnedItems.Add(pair.Key, WalkState.OnStack);
            }

            if (pair.Value != null)
            {
                foreach (T item in pair.Value)
                {
                    foreach (T subitem in RecursiveEnumerator(
                                                returnedItems,
                                                new KeyValuePair<T, List<T>>(item, dictionary[item])))
                    {
                        yield return subitem;
                    }
                }
            }

            returnedItems[pair.Key] = WalkState.Yielded;
            yield return pair.Key;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    #endregion

    static class Utility
    {
        #region Blittability

        /// <summary>
        /// Returns <B>true</B> iff the <paramref name="type"/> is classified blittable.
        /// </summary>
        /// <remarks>
        /// Blittable types are those that look exactly the same both in managed and in native.
        /// Marshaling of blittable types can be often optimized (e.g. by pinning instead of copying).
        /// </remarks>
        internal static bool IsStructBlittable(Type type, bool ansiPlatform)
        {
            BindingFlags b_flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (FieldInfo fi in type.GetFields(b_flags))
            {
                Type field_type = fi.FieldType;

                // value types that contain only blittable fields are blittable
                if (field_type.IsPrimitive)
                {
                    if (!IsPrimitiveBlittable(field_type, type, fi, ansiPlatform)) return false;
                }
                else
                {
                    if (field_type == typeof(object) ||
                        field_type.IsInterface ||
                        !IsStructBlittable(field_type, ansiPlatform)) return false;
                }
            }

            return true;
        }

        private static bool IsPrimitiveBlittable(Type type, Type declaringType, FieldInfo fi, bool ansiPlatform)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;

                case TypeCode.Object:
                    return (type.IsPointer || type == typeof(IntPtr) || type == typeof(UIntPtr));

                case TypeCode.Char:
                {
                    // chars marshaled to UnmanagedType.U2 or UnmanagedType.I2 are blittable
                    bool isAnsi = ansiPlatform;
                    if (declaringType.IsAnsiClass) isAnsi = true;
                    else if (declaringType.IsUnicodeClass) isAnsi = false;

                    if ((fi.Attributes & FieldAttributes.HasFieldMarshal) == FieldAttributes.HasFieldMarshal)
                    {
                        MarshalAsAttribute maa =
                            (MarshalAsAttribute)fi.GetCustomAttributes(typeof(MarshalAsAttribute), false)[0];
                        isAnsi = (maa.Value == UnmanagedType.I1 || maa.Value == UnmanagedType.U1);
                    }

                    return !isAnsi;
                }

                default:
                    return false;
            }
        }

        #endregion

        #region Type classification

        /// <summary>
        /// Returns <B>true</B> iff the argument is a delegate type.
        /// </summary>
        internal static bool IsDelegate(Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type);
        }

        /// <summary>
        /// Returns <B>true</B> iff the argument has the sequential or explicit layout.
        /// </summary>
        internal static bool HasLayout(Type type)
        {
            return (type.IsExplicitLayout || type.IsLayoutSequential);
        }

        #endregion

        /// <summary>
        /// Escapes special characters in a string and surrounds it with double-quotes.
        /// </summary>
        internal static string StringToLiteral(string str)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('"');

            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                switch (ch)
                {
                    case '\'': sb.Append("\\'"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\a': sb.Append(@"\a"); break;
                    case '\b': sb.Append(@"\b"); break;
                    case '\f': sb.Append(@"\f"); break;
                    case '\n': sb.Append(@"\n"); break;
                    case '\r': sb.Append(@"\r"); break;
                    case '\t': sb.Append(@"\t"); break;
                    case '\v': sb.Append(@"\v"); break;

                    default: sb.Append(ch); break;
                }
            }

            sb.Append('"');
            return sb.ToString();
        }

        /// <summary>
        /// Converts a return value parameter info into an ordinary out parameter info at the given position.
        /// </summary>
        internal static ParameterInfo MakeRetParameterInfo(ParameterInfo parameterInfo, int position)
        {
            Debug.Assert(!parameterInfo.ParameterType.IsByRef);

            return new ParameterInfoEx(
                (parameterInfo.Attributes | /*ParameterAttributes.Retval |*/ ParameterAttributes.Out),
                parameterInfo.ParameterType.MakeByRefType(),
                parameterInfo.RawDefaultValue,
                parameterInfo.Member,
                parameterInfo.GetCustomAttributes(false),
                "retVal",
                position);
        }

        /// <summary>
        /// Normalize a type by extracting pointer-sized fields from one-field structures.
        /// For example <code>struct { int x; }</code> becomes <code>int</code>.
        /// </summary>
        /// <remarks>
        /// The marshaler may work with this normalized type in some versions of the CLR.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal static Type GetNormalizedType(Type type)
        {
            Debug.Assert(type != null);

            Type orig_type = type;

            while (type.IsValueType)
            {
                FieldInfo[] fi = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                // check for "trivial" layout
                if (fi.Length == 1 && (!HasLayout(type) || Marshal.SizeOf(type) == 4 || Marshal.SizeOf(type) == 8))
                {
                    type = fi[0].FieldType;

                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64: return type;

                        case TypeCode.Object:
                        {
                            if (type.IsPointer || type == typeof(IntPtr) || type == typeof(UIntPtr)) return type;

                            // now continue the while loop checking that the field is a value type
                            continue;
                        }

                        default: return orig_type;
                    }
                }
                else break;
            }

            return orig_type;
        }

        /// <summary>
        /// Determines the Ansi/Unicode <see cref="MarshalFlag"/> according to a type.
        /// </summary>
        internal static MarshalFlags GetCharSetMarshalFlag(Type type)
        {
            if (type != null)
            {
                if (type.IsAnsiClass) return MarshalFlags.AnsiStrings;
                if (type.IsUnicodeClass) return MarshalFlags.UnicodeStrings;
            }
            return MarshalFlags.None;
        }

        /// <summary>
        /// Determines the Ansi/Unicode <see cref="MarshalFlag"/> according to a <see cref="CharSet"/>.
        /// </summary>
        /// <remarks>
        /// To be used for P/Invoke methods whose <see cref="DllImportAttribute"/> has a charset property,
        /// and for delegate definitions whose <see cref="UnmanagedFunctionPointerAttribute"/> has a charset
        /// property.
        /// </remarks>
        internal static MarshalFlags GetCharSetMarshalFlag(CharSet charSet)
        {
            switch (charSet)
            {
                case CharSet.None:
                case CharSet.Ansi:    return MarshalFlags.AnsiStrings;
                case CharSet.Unicode: return MarshalFlags.UnicodeStrings;
            }
            return MarshalFlags.None;
        }

        internal static string MakeCIdentifier(string arg)
        {
            Debug.Assert(arg != null);

            StringBuilder sb = null;

            if (arg.Length == 0) return "_id";

            for (int i = 0; i < arg.Length; i++)
            {
                if (!(i == 0 ? Char.IsLetter(arg, i) : Char.IsLetterOrDigit(arg, i)))
                {
                    if (sb == null) sb = new StringBuilder(arg);
                    sb[i] = '_';
                }
            }
            
            string result = (sb == null ? arg : sb.ToString());

            while (CppKeywords.IsKeyword(result)) result = "_" + result;

            return result;
        }

        internal static string GetNameOfType(Type type)
        {
            if (type.IsNested)
            {
                int ns_len = (String.IsNullOrEmpty(type.Namespace) ? 0 : type.Namespace.Length + 1);
                return Utility.MakeCIdentifier(type.FullName.Substring(ns_len));
            }

            return Utility.MakeCIdentifier(type.Name);
        }

        /// <summary>
        /// Returns <B>null</B> is the type is not permitted as an enum underlying type.
        /// </summary>
        internal static UnmanagedType[] GetAllowedUnmanagedTypesForEnum(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:   return MarshalType.Byte;
                case TypeCode.Int16:  return MarshalType.Int16;
                case TypeCode.Int32:  return MarshalType.Int32;
                case TypeCode.Int64:  return MarshalType.Int64;
                case TypeCode.SByte:  return MarshalType.SByte;
                case TypeCode.UInt16: return MarshalType.UInt16;
                case TypeCode.UInt32: return MarshalType.UInt32;
                case TypeCode.UInt64: return MarshalType.UInt64;

                default: return null;
            }
        }

        internal static T GetMaxEnumValue<T>(Type enumType) where T : struct, IComparable<T>
        {
            Debug.Assert(enumType.IsEnum && Enum.GetUnderlyingType(enumType) == typeof(T));

            T max = new T();
            foreach (T value in Enum.GetValues(enumType))
            {
                if (value.CompareTo(max) > 0)
                    max = value;
            }

            return max;
        }

        #region VarEnum <-> (Un)managedType Conversion

        /// <summary>
        /// Converts a <see cref="VarEnum"/> to a corresponding <see cref="UnmanagedType"/>.
        /// </summary>
        internal static UnmanagedType VarEnumToUnmanagedType(VarEnum varEnum)
        {
            switch (varEnum)
            {
                case VarEnum.VT_CARRAY: return UnmanagedType.LPArray;
                case VarEnum.VT_CY:     return UnmanagedType.Currency;

                case VarEnum.VT_STREAMED_OBJECT:
                case VarEnum.VT_STREAM:
                case VarEnum.VT_STORED_OBJECT:
                case VarEnum.VT_STORAGE:
                case VarEnum.VT_RECORD:
                case VarEnum.VT_DATE:
                case VarEnum.VT_FILETIME:
                case VarEnum.VT_DECIMAL: return UnmanagedType.Struct;

                case VarEnum.VT_ERROR:
                case VarEnum.VT_HRESULT: return UnmanagedType.Error;

                case VarEnum.VT_BOOL: return UnmanagedType.VariantBool;

                case VarEnum.VT_I1:   return UnmanagedType.I1;
                case VarEnum.VT_I2:   return UnmanagedType.I2;
                case VarEnum.VT_I4:   return UnmanagedType.I4;
                case VarEnum.VT_I8:   return UnmanagedType.I8;

                case VarEnum.VT_UI1:  return UnmanagedType.U1;
                case VarEnum.VT_UI2:  return UnmanagedType.U2;
                case VarEnum.VT_UI4:  return UnmanagedType.U4;
                case VarEnum.VT_UI8:  return UnmanagedType.U8;

                case VarEnum.VT_R4:   return UnmanagedType.R4;
                case VarEnum.VT_R8:   return UnmanagedType.R8;

                case VarEnum.VT_BSTR:   return UnmanagedType.BStr;
                case VarEnum.VT_LPSTR:  return UnmanagedType.LPStr;
                case VarEnum.VT_LPWSTR: return UnmanagedType.LPWStr;

                case VarEnum.VT_INT:
                case VarEnum.VT_PTR:  return UnmanagedType.SysInt;

                case VarEnum.VT_UINT: return UnmanagedType.SysUInt;

                case VarEnum.VT_ARRAY:
                case VarEnum.VT_SAFEARRAY: return UnmanagedType.SafeArray;

                case VarEnum.VT_DISPATCH:  return UnmanagedType.IDispatch;
                case VarEnum.VT_UNKNOWN:   return UnmanagedType.IUnknown;

                default: return (UnmanagedType)0;
            }
        }

        /// <summary>
        /// Converts a managed <see cref="Type"/> to a corresponding <see cref="VarEnum"/> value.
        /// </summary>
        internal static VarEnum TypeToVarEnum(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:  return VarEnum.VT_BOOL;
                case TypeCode.Byte:     return VarEnum.VT_UI1;
                case TypeCode.Char:     return VarEnum.VT_UI2;
                case TypeCode.DateTime: return VarEnum.VT_DATE;
                case TypeCode.DBNull:   return VarEnum.VT_NULL;
                case TypeCode.Decimal:  return VarEnum.VT_DECIMAL;
                case TypeCode.Double:   return VarEnum.VT_R8;
                case TypeCode.Empty:    return VarEnum.VT_EMPTY;
                case TypeCode.Int16:    return VarEnum.VT_I2;
                case TypeCode.Int32:    return VarEnum.VT_I4;
                case TypeCode.Int64:    return VarEnum.VT_I8;
                case TypeCode.SByte:    return VarEnum.VT_I1;
                case TypeCode.Single:   return VarEnum.VT_R4;
                case TypeCode.String:   return VarEnum.VT_BSTR;
                case TypeCode.UInt16:   return VarEnum.VT_UI2;
                case TypeCode.UInt32:   return VarEnum.VT_UI4;
                case TypeCode.UInt64:   return VarEnum.VT_UI8;

                case TypeCode.Object:
                {
                    if (type.IsArray) return VarEnum.VT_ARRAY;
                    if (type.IsPointer) return VarEnum.VT_PTR;

                    if (type == typeof(object)) return VarEnum.VT_VARIANT;
                    if (type == typeof(Guid)) return VarEnum.VT_CLSID;
                    if (type == typeof(IntPtr)) return VarEnum.VT_INT;
                    if (type == typeof(UIntPtr)) return VarEnum.VT_UINT;

                    if (type.IsValueType) return VarEnum.VT_RECORD;

                    switch (Utility.GetComInterfaceType(type))
                    {
                        case ComInterfaceType.InterfaceIsIUnknown: return VarEnum.VT_UNKNOWN;
                        case ComInterfaceType.InterfaceIsIDispatch: return VarEnum.VT_DISPATCH;
                    }

                    return VarEnum.VT_UNKNOWN;
                }
            }

            return VarEnum.VT_EMPTY;
        }

        #endregion

        #region GetComInterfaceType

        /// <summary>
        /// Determines the kind of COM interface pointer a given type will be marshaled to.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal static ComInterfaceType GetComInterfaceType(Type type, out Type ifaceType)
        {
            if (type.IsInterface)
            {
                ifaceType = type;
                return GetComInterfaceType(ifaceType);
            }

            ifaceType = null;

            // try to approximate the default interface of the class
            if (type.IsImport || Marshal.IsTypeVisibleFromCom(type))
            {
                object[] attrs = type.GetCustomAttributes(typeof(ComDefaultInterfaceAttribute), false);
                if (attrs.Length == 1)
                {
                    ifaceType = ((ComDefaultInterfaceAttribute)attrs[0]).Value;
                    return GetComInterfaceType(ifaceType);
                }

                attrs = type.GetCustomAttributes(typeof(ClassInterfaceAttribute), false);
                if (attrs.Length == 1)
                {
                    switch (((ClassInterfaceAttribute)attrs[0]).Value)
                    {
                        case ClassInterfaceType.AutoDispatch: return ComInterfaceType.InterfaceIsIDispatch;
                        case ClassInterfaceType.AutoDual: return ComInterfaceType.InterfaceIsDual;
                    }
                }

                foreach (Type itf_type in type.GetInterfaces())
                {
                    if (Marshal.IsTypeVisibleFromCom(itf_type))
                    {
                        ifaceType = itf_type;
                        return GetComInterfaceType(ifaceType);
                    }
                }
            }

            return ComInterfaceType.InterfaceIsIUnknown;
        }

        private static ComInterfaceType GetComInterfaceType(Type interfaceType)
        {
            object[] attrs = interfaceType.GetCustomAttributes(typeof(InterfaceTypeAttribute), false);
            if (attrs.Length == 1)
            {
                return ((InterfaceTypeAttribute)attrs[0]).Value;
            }
            else
            {
                return ComInterfaceType.InterfaceIsDual;
            }
        }

        #endregion
    }
}
