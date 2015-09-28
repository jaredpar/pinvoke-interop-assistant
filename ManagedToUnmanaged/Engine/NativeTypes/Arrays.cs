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
using System.IO;

namespace SignatureGenerator
{
    [Serializable]
    public class ArrayNativeType : NativeType
    {
        #region ArrayKind

        private enum ArrayKind
        {
            SafeArray, NativeArray, ByValArray, Invalid
        }

        #endregion

        #region Fields

        private bool platform64bit;
        private bool isInOnly;
        private ArrayKind arrayKind;
        private NativeType elementType;
        private int length;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the size of the type including any padding.
        /// </summary>
        public override int TypeSize
        {
            get
            {
                switch (arrayKind)
                {
                    case ArrayKind.ByValArray:
                    {
                        // the array is embedded in a structure - return length times the size of the element
                        return (length * elementType.TypeSize);
                    }

                    case ArrayKind.Invalid:
                    case ArrayKind.SafeArray:
                    case ArrayKind.NativeArray:
                    {
                        // we exchange just a pointer
                        return TypeName.GetPointerSize(platform64bit);
                    }

                    default:
                    {
                        Debug.Fail(null);
                        return 0;
                    }
                }
            }
        }

        public override int AlignmentRequirement
        {
            get
            {
                switch (arrayKind)
                {
                    case ArrayKind.ByValArray:
                    {
                        // the array is embedded in a structure - return the size requirement of the element
                        return elementType.AlignmentRequirement;
                    }

                    case ArrayKind.Invalid:
                    case ArrayKind.SafeArray:
                    case ArrayKind.NativeArray:
                    {
                        // we exchange just a pointer
                        return TypeName.GetPointerSize(platform64bit);
                    }

                    default:
                    {
                        Debug.Fail(null);
                        return 0;
                    }
                }
            }
        }

        public override bool MarshalsIn
        {
            get { return (descMarshalsIn || !descMarshalsOut); }
        }

        public override bool MarshalsOut
        {
            get
            {
                // arrays marshal in only by default (unless passed by-ref of course)
                return (descMarshalsOut || (indirections > 0 && !descMarshalsIn));
            }
        }

        public override bool IsInvalid
        {
            get
            { return (arrayKind == ArrayKind.Invalid); }
        }

        #endregion

        #region Construction

        public ArrayNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(desc.Type.IsArray || desc.Type == typeof(System.Array));

            this.platform64bit = desc.IsPlatform64Bit;
            this.indirections = (desc.IsByRefParam ? 1 : 0);
            this.isInOnly = (!desc.IsByRefParam && !desc.MarshalsOut); // byval and either [In] or default

            if (!desc.IsStructField)
                this.isByrefParameter = true;

            UnmanagedType[] allowed_unmanaged_types;

            if (desc.Type == typeof(System.Array))
            {
                // System.Array
                if (desc.IsStructField)
                {
                    // interface ptr, SAFEARRAY, or by-val array
                    allowed_unmanaged_types = MarshalType.ArrayClassField;
                }
                else
                {
                    // interface ptr or SAFEARRAY
                    allowed_unmanaged_types = MarshalType.ArrayClass;
                }
            }
            else
            {
                if (desc.IsArrayElement)
                {
                    Log.Add(Errors.ERROR_NoNestedArrayMarshaling);
                    
                    this.arrayKind = ArrayKind.Invalid;
                    return;
                }

                // an array
                if (desc.IsStructField)
                {
                    // default array field marshaling is always COM style, i.e. SAFEARRAY by default
                    allowed_unmanaged_types = MarshalType.ArrayField;
                }
                else
                {
                    // default array parameter marshaling depends on whether we do COM or P/Invoke
                    allowed_unmanaged_types = (desc.IsComInterop ? MarshalType.ArrayC : MarshalType.ArrayP);
                }
            }

            // there are three possible unmanaged types for arrays:
            switch (ValidateUnmanagedType(desc, allowed_unmanaged_types))
            {
                case UnmanagedType.SafeArray:
                {
                    InitializeAsSafeArray(desc);
                    break;
                }

                case UnmanagedType.LPArray:
                {
                    InitializeAsNativeArray(desc);
                    break;
                }

                case UnmanagedType.ByValArray:
                {
                    // this unmanaged type only makes sense when we are a field of a type with layout
                    Debug.Assert(desc.IsStructField);

                    InitializeAsByValArray(desc);
                    break;
                }

                default:
                {
                    // interfaces are handled by InterfaceNativeType
                    Debug.Fail(null);
                    break;
                }
            }
        }

        /// <summary>
        /// Initializes the array as a <see cref="UnmanagedType.SafeArray"/>.
        /// </summary>
        private void InitializeAsSafeArray(NativeTypeDesc desc)
        {
            this.arrayKind = ArrayKind.SafeArray;

            Type array_mng_type = desc.Type;

            VarEnum sub_type = VarEnum.VT_EMPTY;
            if (desc.MarshalAs != null && desc.MarshalAs.SafeArraySubType != VarEnum.VT_EMPTY)
            {
                sub_type = desc.MarshalAs.SafeArraySubType;
            }
            else
            {
                // the unmanaged type may also be specified statically using one of the wrapper classes
                if (array_mng_type == typeof(UnknownWrapper[]))
                {
                    array_mng_type = typeof(object[]);
                    sub_type = VarEnum.VT_UNKNOWN;
                }
                else if (array_mng_type == typeof(DispatchWrapper[]))
                {
                    array_mng_type = typeof(object[]);
                    sub_type = VarEnum.VT_DISPATCH;
                }
                else if (array_mng_type == typeof(ErrorWrapper[]))
                {
                    array_mng_type = typeof(int[]);
                    sub_type = VarEnum.VT_ERROR;
                }
                else if (array_mng_type == typeof(CurrencyWrapper[]))
                {
                    array_mng_type = typeof(Decimal[]);
                    sub_type = VarEnum.VT_CY;
                }
                else if (array_mng_type == typeof(BStrWrapper[]))
                {
                    array_mng_type = typeof(string[]);
                    sub_type = VarEnum.VT_BSTR;
                }
            }

            // convert the SafeArraySubType to UnmanagedType
            UnmanagedType element_unmng_type = Utility.VarEnumToUnmanagedType(sub_type);

            // determine the element type
            // (this will have no effect on the C++ signature but we will check it and add it to log)
            this.elementType = NativeType.FromClrArrayElement(
                array_mng_type,
                element_unmng_type,
                (desc.Flags & ~MarshalFlags.ByRefParam) | MarshalFlags.ComInterop);

            if (sub_type == VarEnum.VT_EMPTY)
            {
                sub_type = Utility.TypeToVarEnum(array_mng_type.GetElementType());
            }

            if (!elementType.IsInvalid)
            {
                if (array_mng_type != typeof(System.Array) || sub_type != VarEnum.VT_EMPTY)
                {
                    // log the element native type
                    Log.Add(Errors.INFO_SafeArrayWillMarshalAs, sub_type.ToString());
                }
            }

            // also include the *Wrapper hint if applicable
            if (desc.Type == typeof(object[]) && (sub_type == VarEnum.VT_EMPTY || sub_type == VarEnum.VT_VARIANT))
            {
                Log.Add(Errors.INFO_SafeArrayOfVariantsWrapperUse);
            }

            ExplainMemoryManagement(desc, Resources._SafeArray);
        }

        /// <summary>
        /// Initializes the array as a <see cref="UnmanagedType.LPArray"/>.
        /// </summary>
        private void InitializeAsNativeArray(NativeTypeDesc desc)
        {
            this.arrayKind = ArrayKind.NativeArray;

            if (desc.IsByRefParam)
            {
                // SizeConst and SizeParamIndex are not allowed for byref
                if (HasSizeConst(desc) || HasSizeParamIndex(desc))
                {
                    Log.Add(Errors.ERROR_ArraySizeNotAllowedForByref);
                }
            }
            else if (!desc.IsCallbackParam)
            {
                // when marshaling from managed to native, size is implicit (warn if given explicitly)
                if (HasSizeConst(desc) || HasSizeParamIndex(desc))
                {
                    Log.Add(Errors.WARN_ArraySizesIgnored);
                }
                else
                {
                    Log.Add(Errors.INFO_ArraySizeDeterminedDynamically);
                }
            }
            else
            {
                // when marshaling from native to managed, size must be given, otherwise it's 1
                if (HasSizeParamIndex(desc))
                {
                    Debug.Assert(desc.ParameterInfo != null);
                    ParameterInfo[] parameters = ((MethodBase)desc.ParameterInfo.Member).GetParameters();

                    int param_index = desc.MarshalAs.SizeParamIndex;
                    if (param_index < 0 || param_index >= parameters.Length)
                    {
                        // index OOR (error)
                        Log.Add(Errors.ERROR_ArraySizeParamIndexOutOfRange, param_index);
                    }
                    else if (!TypeAllowedInSizeParam(parameters[param_index].ParameterType))
                    {
                        // index refers to bad param (error)
                        Log.Add(Errors.ERROR_ArraySizeParamWrongType,parameters[param_index].ParameterType);
                    }
                    else
                    {
                        // determine parameter name
                        string param_name = parameters[param_index].Name;
                        if (String.IsNullOrEmpty(param_name))
                        {
                            param_name = String.Format(Resources.Number, param_index + 1);
                        }

                        if (desc.MarshalAs.SizeConst > 0)
                        {
                            // size = [param_at_the_index] + size_const;
                            Log.Add(Errors.INFO_ArraySizeIsByParameterPlusConstant,
                                param_name, desc.MarshalAs.SizeConst);
                        }
                        else
                        {
                            // size = [param_at_the_index]
                            Log.Add(Errors.INFO_ArraySizeIsByParameter, param_name);
                        }
                    }
                }
                else if (HasSizeConst(desc))
                {
                    // size = size_const
                    Log.Add(Errors.INFO_ArraySizeIsConstant, desc.MarshalAs.SizeConst);
                }
                else
                {
                    // size = 1 (warn)
                    Log.Add(Errors.WARN_ArraySizeDefaultsToOne);
                }
            }

            UnmanagedType element_unmng_type =
                (desc.MarshalAs == null ? (UnmanagedType)0 : desc.MarshalAs.ArraySubType);
            if (element_unmng_type == (UnmanagedType)80) element_unmng_type = (UnmanagedType)0;

            // determine the element type
            this.elementType = NativeType.FromClrArrayElement(desc.Type, element_unmng_type, desc.Flags);

            ExplainMemoryManagement(desc, Resources._Array);
        }

        /// <summary>
        /// Initializes the array as a <see cref="UnmanagedType.ByValArray"/>.
        /// </summary>
        private void InitializeAsByValArray(NativeTypeDesc desc)
        {
            Debug.Assert(this.indirections == 0);

            this.arrayKind = ArrayKind.ByValArray;

            // const size must be specified and >0
            if (desc.MarshalAs == null || desc.MarshalAs.SizeConst <= 0)
            {
                Log.Add(Errors.ERROR_ByValArrayInvalidLength);
                this.length = 1;
            }
            else
            {
                // no need to output any INFO as this number will be between brackets in the code
                this.length = desc.MarshalAs.SizeConst;
            }

            UnmanagedType element_unmng_type =
                (desc.MarshalAs == null ? (UnmanagedType)0 : desc.MarshalAs.ArraySubType);
            if (element_unmng_type == (UnmanagedType)80) element_unmng_type = (UnmanagedType)0;

            // determine the element type
            this.elementType = NativeType.FromClrArrayElement(desc.Type, element_unmng_type, desc.Flags);
        }

        #endregion

        #region ICodePrintable Members

        public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            base.PrintTo(printer, logPrinter, flags);

            switch (arrayKind)
            {
                case ArrayKind.Invalid:
                {
                    // void */PVOID
                    PrimitiveNativeType.PrintNameTo(TypeName.Void, 1, printer, flags);
                    break;
                }

                case ArrayKind.SafeArray:
                {
                    // prefix with const if in-only
                    if (isInOnly)
                    {
                        printer.Print(OutputType.Keyword, "const");
                        printer.Print(OutputType.Other, " ");
                    }

                    // SAFEARRAY * or LPSAFEARRAY
                    if ((flags & PrintFlags.UsePlainC) == PrintFlags.UsePlainC)
                    {
                        TypeName.PrintTo(printer, TypeName.SafeArray.PlainC);
                        printer.Print(OutputType.Other, " ");
                        printer.Print(OutputType.Operator, "*");
                    }
                    else
                    {
                        TypeName.PrintTo(printer, "LP" + TypeName.SafeArray.WinApi);
                    }
                    break;
                }

                default:
                {
                    elementType.PrintTo(printer, logPrinter, flags);
                    break;
                }
            }

            if (indirections > 0)
            {
                // We'll suppress the [] suffix in this case; one indirection will be provided by the
                // element itself because the ByRefParam flag has been inherited by it.
                int stars = indirections;

                while (stars-- > 0) printer.Print(OutputType.Operator, "*");
            }
        }

        public override void PrintPostIdentifierTo(ICodePrinter printer, PrintFlags flags)
        {
            switch (arrayKind)
            {
                case ArrayKind.Invalid:
                case ArrayKind.SafeArray:
                {
                    // no post identifier output
                    break;
                }

                case ArrayKind.NativeArray:
                {
                    if (indirections == 0)
                    {
                        // We can only use the [] suffix when this array is not indirected with a byref. In
                        // that case, we use two stars prefix instead, because
                        // <element_type> (*array)[] is NOT a pointer to array!

                        // empty brackets "[]"
                        printer.Print(OutputType.Operator, "[]");
                    }
                    break;
                }

                case ArrayKind.ByValArray:
                {
                    // length-denoting brackets "[n]"
                    printer.Print(OutputType.Operator, "[");
                    printer.Print(OutputType.Literal, length.ToString());
                    printer.Print(OutputType.Operator, "]");
                    break;
                }
            }
            
            base.PrintPostIdentifierTo(printer, flags);
        }

        public override void PrintLog(ILogPrinter logPrinter, PrintFlags flags, string messagePrefix)
        {
            base.PrintLog(logPrinter, flags, messagePrefix);

            if (arrayKind != ArrayKind.Invalid)
            {
                elementType.PrintLog(logPrinter, flags, messagePrefix);
            }
        }

        #endregion

        #region Definition Enumeration

        internal override void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
        {
            if (arrayKind != ArrayKind.Invalid)
            {
                // if this is an array of, say, structures, we want to print the definition of the structure type
                elementType.GetDefinitionsRecursive(set, parentDef);
            }
        }

        #endregion

        #region Helpers

        private static bool HasSizeParamIndex(NativeTypeDesc desc)
        {
            // Warning: there is currently no way how we can distinguish between the case where SizeParamIndex
            // is not specified and the case where SizeParamIndex is specified and set to 0. This method should
            // be updated when the MarshalAsAttribute is improved to provide this information.
            if (desc.MarshalAs == null) return false;
            return (desc.MarshalAs.SizeParamIndex != 0);            
        }

        private static bool HasSizeConst(NativeTypeDesc desc)
        {
            // Warning: there is currently no way how we can distinguish between the case where SizeConst is
            // not specified and the case where SizeConst is specified and set to 0. This method should be
            // updated when the MarshalAsAttribute is improved to provide this information.
            if (desc.MarshalAs == null) return false;
            return (desc.MarshalAs.SizeConst != 0);
        }

        private static bool TypeAllowedInSizeParam(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                case TypeCode.UInt32:
                case TypeCode.Int32:
                case TypeCode.UInt64:
                case TypeCode.Int64: return true;

                default: return false;
            }
        }

        #endregion
    }
}
