/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SignatureGenerator
{
    /// <summary>
    /// Represents a (possibly indirected) primitive type.
    /// </summary>
    /// <remarks>
    /// Produces output like &quot;float&quot;, &quot;int *&quot;, &quot;BYTE&quot;, &quot;LPWORD&quot;.
    /// </remarks>
    [Serializable]
    public class PrimitiveNativeType : NativeType
    {
        #region Fields

        protected TypeName typeName;
        protected bool platform64bit;

        #endregion

        #region Properties

        public override int TypeSize
        {
            get
            {
                if (this.indirections > 0) return TypeName.GetPointerSize(platform64bit);
                else return typeName.GetSize(platform64bit);
            }
        }

        public override int AlignmentRequirement
        {
            get
            { return this.TypeSize; }
        }

        public override bool MarshalsIn
        {
            get
            {
                // primitives are in/out by default
                return (descMarshalsIn || !descMarshalsOut);
            }
        }

        public override bool MarshalsOut
        {
            get
            {
                // primitives are in/out by default
                return (descMarshalsOut || !descMarshalsIn);
            }
        }

        #endregion

        #region Construction

        public PrimitiveNativeType(TypeName typeName, int indirections, bool platform64bit)
        {
            this.typeName = typeName;
            this.platform64bit = platform64bit;
            this.indirections = indirections;
        }

        public PrimitiveNativeType(TypeName typeName, bool platform64bit)
            : this(typeName, 0, platform64bit)
        { }

        public PrimitiveNativeType(NativeTypeDesc desc, UnmanagedType[] allowedUnmanagedTypes)
            : this(desc)
        {
            UnmanagedType unmng_type = ValidateUnmanagedType(desc, allowedUnmanagedTypes);

            if (desc.Type == typeof(bool))
            {
                // Boolean as I1 and I2 should print 'bool' as the plain C++ type
                if (unmng_type == UnmanagedType.I1) typeName = TypeName.I1_Bool;
                if (unmng_type == UnmanagedType.U1) typeName = TypeName.U1_Bool;
            }

            if (typeName.IsEmpty)
            {
                typeName = TypeName.GetTypeNameForUnmanagedType(unmng_type);
                Debug.Assert(!typeName.IsEmpty);
            }
            
            if (unmng_type == UnmanagedType.SysInt ||
                unmng_type == UnmanagedType.SysUInt)
            {
                // IntPtr and UIntPtr translate into "void *"
                indirections++;
            }

        }

        protected PrimitiveNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            this.platform64bit = desc.IsPlatform64Bit;

            // count the total level of indirections
            this.indirections = desc.PointerIndirections;
            if (desc.IsByRefParam) this.indirections++;

            VerifyMarshalDirection(desc);
        }

        #endregion

        #region ICodePrintable Members

        protected internal static void PrintNameTo(TypeName typeName, int indirections, ICodePrinter printer, PrintFlags flags)
        {
            string output;

            if ((flags & PrintFlags.UsePlainC) == PrintFlags.UsePlainC)
            {
                output = typeName.PlainC;
            }
            else
            {
                output = typeName.WinApi;

                if (indirections > 0)
                {
                    switch (typeName.PointerPrefix)
                    {
                        case TypeName.PtrPrefix.P_Prefix:
                        {
                            output = "P" + output;
                            indirections--;
                            break;
                        }

                        case TypeName.PtrPrefix.LP_Prefix:
                        {
                            output = "LP" + output;
                            indirections--;
                            break;
                        }
                    }
                }
            }
            TypeName.PrintTo(printer, output);

            if (indirections > 0)
            {
                if (!output.EndsWith("*")) printer.Print(OutputType.Other, " ");
                while (indirections-- > 0)
                {
                    printer.Print(OutputType.Operator, "*");
                }
            }
        }
        
        public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            base.PrintTo(printer, logPrinter, flags);
            PrintNameTo(typeName, indirections, printer, flags);
        }

        #endregion
    }

    [Serializable]
    public class DateNativeType : PrimitiveNativeType
    {
        #region Construction

        public DateNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(desc.Type == typeof(DateTime));

            ValidateUnmanagedType(desc, MarshalType.Struct);
            this.typeName = TypeName.Date;
        }

        #endregion
    }

    [Serializable]
    public class DecimalNativeType : PrimitiveNativeType
    {
        #region Fields

        private bool isLPStruct;

        #endregion

        #region Construction

        public DecimalNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(desc.Type == typeof(Decimal));

            UnmanagedType[] allowed_unmanaged_types =
                (desc.IsStructField ? MarshalType.DecimalField : MarshalType.DecimalParam);

            switch (ValidateUnmanagedType(desc, allowed_unmanaged_types))
            {
                case UnmanagedType.Currency:
                {
                    // marshals as CURRENCY COM type
                    this.typeName = TypeName.Currency;
                    break;
                }

                case UnmanagedType.Struct:
                {
                    // marshals as DECIMAL 96-bit type
                    this.typeName = TypeName.Decimal;
                    break;
                }

                case UnmanagedType.LPStruct:
                {
                    // marshals as a pointer to the DECIMAL 96-bit type
                    this.isLPStruct = true;
                    this.typeName = TypeName.Decimal;
                    this.indirections++;
                    break;
                }

                default:
                {
                    Debug.Fail(null);
                    break;
                }
            }
        }

        #endregion

        #region Properties

        public override int AlignmentRequirement
        {
            get
            {
                // struct DECIMAL contains an ULONGLONG field
                if (this.indirections > 0) return base.AlignmentRequirement;
                else return 8;
            }
        }

        public override bool MarshalsAsPointerWithKnownDirection
        {
            get
            { return (isLPStruct || base.MarshalsAsPointerWithKnownDirection); }
        }

        #endregion
    }

    [Serializable]
    public class GuidNativeType : PrimitiveNativeType
    {
        #region Fields

        private bool isLPStruct;

        #endregion

        #region Construction

        public GuidNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(desc.Type == typeof(System.Guid));

            if (ValidateUnmanagedType(desc, MarshalType.Guid) == UnmanagedType.LPStruct)
            {
                this.isLPStruct = true;
                this.indirections++;
            }

            // marshals as either GUID or PGUID
            this.typeName = TypeName.Guid;
        }

        #endregion

        #region Properties

        public override int AlignmentRequirement
        {
            get
            {
                // struct _GUID contains a ULONG field
                if (this.indirections > 0) return base.AlignmentRequirement;
                return 4;
            }
        }

        public override bool MarshalsAsPointerWithKnownDirection
        {
            get
            { return (isLPStruct || base.MarshalsAsPointerWithKnownDirection); }
        }

        #endregion
    }

    [Serializable]
    public class HandleNativeType : PrimitiveNativeType
    {
        #region Construction

        public HandleNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(
                desc.Type == typeof(System.Runtime.InteropServices.HandleRef) ||
                typeof(System.Runtime.InteropServices.SafeHandle).IsAssignableFrom(desc.Type) ||
                typeof(System.Runtime.InteropServices.CriticalHandle).IsAssignableFrom(desc.Type));

            if (desc.IsArrayElement && desc.Type != typeof(System.Runtime.InteropServices.HandleRef))
            {
                Log.Add(Errors.ERROR_HandlesNotPermittedAsArrayElements);
            }

            // non-default marshaling is not supported
            ValidateUnmanagedType(desc, MarshalType.Empty);

            // marshals as HANDLE
            this.typeName = TypeName.Handle;
        }

        #endregion
    }

    [Serializable]
    public class ColorNativeType : PrimitiveNativeType
    {
        #region Construction

        public ColorNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(desc.Type == typeof(System.Drawing.Color));

            // non-default marshaling is not supported
            ValidateUnmanagedType(desc, MarshalType.Empty);

            if (!desc.IsComInterop)
            {
                Log.Add(Errors.ERROR_MarshalingAllowedForCom, desc.Type.FullName);
            }

            // marshals as OLE_COLOR
            this.typeName = TypeName.OleColor;
        }

        #endregion
    }

    [Serializable]
    public class VariableArgumentListNativeType : PrimitiveNativeType
    {
        #region Construction

        public VariableArgumentListNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(desc.Type == typeof(System.ArgIterator));

            // non-default marshaling is not supported
            ValidateUnmanagedType(desc, MarshalType.Empty);

            // marshals as va_list
            this.typeName = TypeName.VaList;
        }

        #endregion
    }
}
