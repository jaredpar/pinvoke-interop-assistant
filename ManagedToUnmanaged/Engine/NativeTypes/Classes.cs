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
    [Serializable]
    public class InterfaceNativeType : PrimitiveNativeType
    {
        #region Fields

        private bool isVariant;
        private bool isUndefined;

        #endregion

        #region Properties

        public override bool MarshalsAsPointerWithKnownDirection
        {
            get
            {
                return (base.MarshalsAsPointerWithKnownDirection || !isVariant);
            }
        }

        public override bool MarshalsIn
        {
            get
            { return (descMarshalsIn || !descMarshalsOut); }
        }

        public override bool MarshalsOut
        {
            get
            {
                if (descMarshalsOut) return true;
                return (indirections > (isUndefined ? 1 : 0) && !descMarshalsIn);
            }
        }

        #endregion

        #region Construction

        /// <summary>
        /// Allows the interface name to be specified explicitly.
        /// </summary>
        public InterfaceNativeType(NativeTypeDesc desc, TypeName typeName)
            : base(desc)
        {
            this.typeName = typeName;
            this.isVariant = false;
            this.indirections++;
        }

        public InterfaceNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            UnmanagedType[] allowed_unmanaged_types;

            if (desc.Type == typeof(System.Object))
            {
                // System.Object
                if (desc.IsArrayElement && !desc.IsStructField)
                {
                    allowed_unmanaged_types = MarshalType.ObjectElement;
                }
                else if (desc.IsStructField)
                {
                    allowed_unmanaged_types = MarshalType.ObjectField;
                }
                else
                {
                    allowed_unmanaged_types = MarshalType.ObjectParam;
                }
            }
            else if (desc.Type == typeof(System.Array))
            {
                // System.Array
                if (desc.MarshalAs == null)
                {
                    if (desc.IsStructField)
                    {
                        Log.Add(Errors.ERROR_UnmanagedTypeRequiredForField, desc.Type.FullName);
                    }
                }

                allowed_unmanaged_types = MarshalType.ArrayClass;
            }
            else
            {
                // an interface
                allowed_unmanaged_types = MarshalType.Interface;
            }

            switch (ValidateUnmanagedType(desc, allowed_unmanaged_types))
            {
                case UnmanagedType.Struct:
                {
                    // VARIANT
                    this.typeName = TypeName.Variant;
                    this.isVariant = true;
                    break;
                }

                case UnmanagedType.Interface:
                case UnmanagedType.IUnknown:
                {
                    if (desc.IsArrayElement && !desc.IsStructField)
                    {
                        // elements of array parameters are always marshaled as VT_UNKNOWN
                        this.typeName = TypeName.IUnknown;
                    }
                    else
                    {
                        // [ifacename] *
                        SetDefaultComInterface(desc.Type);
                    }

                    break;
                }

                case UnmanagedType.IDispatch:
                {
                    // IDispatch *
                    this.typeName = TypeName.IDispatch;
                    break;
                }

                case UnmanagedType.AsAny:
                {
                    // void *
                    this.typeName = TypeName.Void;
                    this.indirections++;

                    Log.Add(Errors.INFO_AsAnyMarshaling);

                    break;
                }

                default:
                {
                    Debug.Fail(null);
                    break;
                }
            }

            if (isVariant)
            {
                // it is not allowed to marshal return values as VARIANTs
                if (!desc.IsCallbackParam && desc.IsRetValParam)
                {
                    Log.Add(Errors.ERROR_VariantReturnTypeNotSupported);
                }
            }
            else
            {
                // inform about interface pointer AddRefing/Releasing
                if (IsMarshaledNativeToManaged(desc))
                {
                    // RCW will be created
                    Log.Add(Errors.INFO_InterfacePointerToRCWMarshaling);
                }
                if (IsMarshaledManagedToNative(desc))
                {
                    // CCW will be created
                    Log.Add(Errors.INFO_ManagedObjectToCCWMarshaling);
                }
            }
        }

        /// <summary>
        /// Determines name of the default COM interface for the given managed type.
        /// </summary>
        private void SetDefaultComInterface(Type type)
        {
            // note: this is a rather simplified version of the real logic

            if (type == typeof(Object))
            {
                this.typeName = TypeName.IUnknown;
            }
            else if (type == typeof(Array))
            {
                Log.Add(Errors.INFO_PointerIsCOMInterfacePtr, type.FullName);

                this.typeName = TypeName.Array;
                this.isUndefined = true;
            }
            else
            {
                Type itf_type;
                
                switch (Utility.GetComInterfaceType(type, out itf_type))
                {
                    case ComInterfaceType.InterfaceIsDual:
                    case ComInterfaceType.InterfaceIsIDispatch:
                    {
                        this.typeName = TypeName.IDispatch;
                        break;
                    }

                    case ComInterfaceType.InterfaceIsIUnknown:
                    {
                        this.typeName = TypeName.IUnknown;
                        break;
                    }
                }

                if (itf_type != null)
                {
                    Log.Add(Errors.INFO_PointerIsCOMInterfacePtr, itf_type.FullName);

                    this.typeName = TypeName.MakeTypeName(itf_type.Name, -1);
                    this.indirections++;
                    this.isUndefined = true;
                }
            }
        }

        #endregion

        #region Properties

        public override int AlignmentRequirement
        {
            get
            {
                // VARIANT contains a LONGLONG field
                if (isVariant && indirections == 0) return 8;
                else return base.AlignmentRequirement;
            }
        }

        #endregion

        #region ICodePrintable Members

        public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            if (isUndefined && (flags & PrintFlags.UseDefinedComInterfaces) == PrintFlags.UseDefinedComInterfaces)
            {
                PrintNameTo(TypeName.IUnknown, indirections - 1, printer, flags);
            }
            else
            {
                PrintNameTo(typeName, indirections, printer, flags);
            }
        }

        #endregion

        #region Utility

        private static bool IsMarshaledNativeToManaged(NativeTypeDesc desc)
        {
            if (desc.IsCallbackParam)
            {
                return (desc.MarshalsIn || (!desc.MarshalsOut && !desc.IsRetValParam));
            }
            else
            {
                return (desc.IsRetValParam || desc.MarshalsOut);
            }
        }

        private static bool IsMarshaledManagedToNative(NativeTypeDesc desc)
        {
            if (desc.IsCallbackParam)
            {
                return (desc.IsRetValParam || desc.MarshalsOut);
            }
            else
            {
                return (desc.MarshalsIn || (!desc.MarshalsOut && !desc.IsRetValParam));
            }
        }

        #endregion
    }

    [Serializable]
    public class CustomMarshaledNativeType : PrimitiveNativeType
    {
        #region Construction

        public CustomMarshaledNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(desc.MarshalAs != null && desc.MarshalAs.Value == UnmanagedType.CustomMarshaler);

            bool added_error = false;

            if (desc.IsStructField)
            {
                // custom marshalers are not allowed on fields
                Log.Add(Errors.ERROR_CustomMarshalerNotAllowedOnFields);
                added_error = true;
            }

            if (desc.Type.IsValueType)
            {
                // custom marshalers are only allowed on reference types
                Log.Add(Errors.ERROR_CustomMarshalerNotAllowedOnValueTypes);
                added_error = true;
            }

            // try to load the custom marshaler
            Type marshaler_type = null;
            if (desc.MarshalAs.MarshalTypeRef != null)
            {
                marshaler_type = desc.MarshalAs.MarshalTypeRef;
            }
            else
            {
                try
                {
                    marshaler_type = Type.GetType(desc.MarshalAs.MarshalType);
                }
                catch (Exception)
                { }

                if (marshaler_type == null)
                {
                    Log.Add(Errors.WARN_MarshalerTypeNotFound, desc.MarshalAs.MarshalType);
                    added_error = true;
                }
            }

            if (marshaler_type != null)
            {
                // check that the type really is a marshaler
                if (!typeof(ICustomMarshaler).IsAssignableFrom(marshaler_type))
                {
                    Log.Add(Errors.ERROR_MarshalerIsNotICustomMarshaler, marshaler_type.FullName);
                    added_error = true;
                }
                else
                {
                    // check the CreateInstance static method
                    MethodInfo gi_mi;

                    Type type = marshaler_type;
                    do
                    {
                        gi_mi = type.GetMethod(
                            "GetInstance",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
                            null,
                            new Type[] { typeof(string) },
                            null);
                    }
                    while (gi_mi == null && (type = type.BaseType) != null);

                    if (gi_mi == null || gi_mi.ReturnType != typeof(ICustomMarshaler))
                    {
                        Log.Add(Errors.ERROR_MarshalerHasNoGetInstance, marshaler_type.FullName);
                        added_error = true;
                    }
                }
            }

            this.typeName = TypeName.Void;
            this.indirections = 1;

            if (!added_error)
            {
                Log.Add(Errors.INFO_CustomMarshaledParameter,
                    (marshaler_type != null ? marshaler_type.FullName : desc.MarshalAs.MarshalType));
            }
        }

        #endregion
    }
}
