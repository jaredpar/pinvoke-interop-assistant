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
    [Serializable]
    public class CharNativeType : PrimitiveNativeType
    {
        #region Construction

        public CharNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            UnmanagedType ut = ValidateUnmanagedType(desc, MarshalType.Char, UnmanagedType.AsAny);
            if (ut == UnmanagedType.AsAny)
            {
                // default marshaling
                if (desc.IsComInterop) ut = UnmanagedType.U2;
                else
                {
                    if (desc.AnsiStrings) ut = UnmanagedType.I1;
                    else if (desc.UnicodeStrings) ut = UnmanagedType.U2;
                    else
                    {
                        // "TCHAR"
                        this.typeName = (desc.AnsiPlatform ? TypeName.TCharA : TypeName.TCharW);

                        Log.Add(Errors.INFO_AutoCharacterMarshaling);
                        return;
                    }
                }
            }

            switch (ut)
            {
                case UnmanagedType.I1: this.typeName = TypeName.I1; break;
                case UnmanagedType.I2: this.typeName = TypeName.I2; break;
                case UnmanagedType.U1: this.typeName = TypeName.UChar; break;
                case UnmanagedType.U2: this.typeName = TypeName.WChar; break;

                default:
                    Debug.Fail(null); break;
            }
        }

        #endregion
    }

    [Serializable]
    public class StringNativeType : PrimitiveNativeType
    {
        #region Fields

        private int? fixedLength;
        private bool fixedLengthAnsi;
        private bool immutable;

        #endregion

        #region Properties

        public override bool MarshalsOut
        {
            get
            {
                if (indirections == 0 && this.immutable) return false;
                else return base.MarshalsOut;
            }
        }

        #endregion

        #region Construction

        public StringNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            // the "String *" or "StringBuilder *" type is not allowed
            CheckPointersToReferenceType(desc);

            UnmanagedType ut = DetermineUnmanagedType(desc, out this.immutable);

            // convert the UnmanagedType to TypeName
            switch (ut)
            {
                case UnmanagedType.LPStr:    this.typeName = (immutable ? TypeName.LPCStr : TypeName.LPStr); break;
                case UnmanagedType.LPWStr:   this.typeName = (immutable ? TypeName.LPCWStr : TypeName.LPWStr); break;
                case UnmanagedType.LPTStr:
                {
                    if (desc.AnsiPlatform)
                        this.typeName = (immutable ? TypeName.LPCTStrA : TypeName.LPTStrA);
                    else
                        this.typeName = (immutable ? TypeName.LPCTStrW : TypeName.LPTStrW);

                    Log.Add(Errors.INFO_AutoStringMarshaling);
                    break;
                }

                case UnmanagedType.BStr:     this.typeName = TypeName.BStr;     break;
                case UnmanagedType.AnsiBStr: this.typeName = TypeName.AnsiBStr; break;
                case UnmanagedType.TBStr:
                {
                    this.typeName = (desc.AnsiPlatform ? TypeName.TBStrA : TypeName.TBStrW);

                    Log.Add(Errors.INFO_AutoStringMarshaling);
                    break;
                }

                case UnmanagedType.VBByRefStr:
                {
                    if (!desc.IsByRefParam || desc.PointerIndirections != 0 ||
                        desc.IsCallbackParam || desc.MarshalsIn != desc.MarshalsOut)
                    {
                        Log.Add(Errors.ERROR_VBByRefParamNotByRef);
                    }
                    else
                    {
                        // the marshaler will turn this into (char *) and create a new String object
                        // with the post-call contents of the buffer
                        indirections--;
                    }

                    immutable = false;

                    if (desc.AnsiStrings) goto case UnmanagedType.LPStr;
                    if (desc.UnicodeStrings) goto case UnmanagedType.LPWStr;
                    goto case UnmanagedType.LPTStr;
                }

                case UnmanagedType.ByValTStr:
                {
                    Debug.Assert(desc.IsStructField && this.indirections == 0);

                    // a fixed-length character array will be embedded in the containing structure
                    int length = desc.MarshalAs.SizeConst;
                    if (length <= 0)
                    {
                        Log.Add(Errors.INFO_FixedLengthStringInvalidLength);
                    }

                    this.fixedLength = length;
                    this.fixedLengthAnsi = desc.ShouldMarshalStringsAnsi;

                    if (desc.AnsiStrings) typeName = TypeName.I1;
                    else if (desc.UnicodeStrings) typeName = TypeName.WChar;
                    else typeName = (desc.AnsiPlatform ? TypeName.TCharA : TypeName.TCharW);

                    break;
                }
            }

            // string is a reference type so we are passing ref always
            this.isByrefParameter = !desc.IsStructField;

            if (!desc.IsArrayElement)
            {
                if (immutable && !desc.IsCallbackParam && !desc.IsStructField)
                {
                    if (!desc.IsByRefParam && desc.PointerIndirections == 0 && desc.MarshalsOut)
                    {
                        // we stripped the [Out]
                        Log.Add(Errors.WARN_ByValStringMarkedOut);
                    }
                    else if (!this.MarshalsOut)
                    {
                        Log.Add(Errors.INFO_BewareStringImmutability);
                    }
                }
                else if (desc.Type == typeof(StringBuilder))
                {
                    if (!desc.MarshalsIn && !desc.MarshalsOut)
                    {
                        // StringBuilder goes in/out by default
                        Log.Add(Errors.INFO_DefaultStringBuilderMarshaling);
                    }
                }
            }

            if (ut != UnmanagedType.VBByRefStr &&
                ut != UnmanagedType.ByValTStr) ExplainMemoryManagement(desc, Resources._String);
        }

        private UnmanagedType DetermineUnmanagedType(NativeTypeDesc desc, out bool immutable)
        {
            UnmanagedType ut;

            if (desc.Type == typeof(string))
            {
                // only out or in/out by-ref string is mutable
                immutable = (!desc.IsByRefParam || (desc.MarshalsIn && !desc.MarshalsOut));

                UnmanagedType[] allowed_types;
                if (desc.IsStructField)
                {
                    allowed_types = MarshalType.StringField;
                }
                else
                {
                    allowed_types = (desc.IsComInterop ? MarshalType.StringC : MarshalType.StringP);
                }

                ut = ValidateUnmanagedType(desc, allowed_types, UnmanagedType.AsAny);

                if (ut == UnmanagedType.AsAny && !desc.IsStructField && desc.IsComInterop)
                {
                    // BSTR is the default for COM
                    ut = UnmanagedType.BStr;
                }
            }
            else
            {
                Debug.Assert(desc.Type == typeof(StringBuilder));

                immutable = false;

                if (desc.IsStructField)
                {
                    // StringBuilder is only supported by the parameter marshaler
                    Log.Add(Errors.ERROR_StringBuilderFieldsDisallowed);

                    ut = UnmanagedType.LPStr;
                }
                else
                {
                    UnmanagedType[] allowed_types = (desc.IsComInterop ? MarshalType.SBuilderC : MarshalType.SBuilderP);
                    ut = ValidateUnmanagedType(desc, allowed_types, UnmanagedType.AsAny);

                    if (ut == UnmanagedType.AsAny && desc.IsComInterop)
                    {
                        // wide string is the default for COM
                        ut = UnmanagedType.LPWStr;
                    }

                    if (!desc.IsCallbackParam)
                        Log.Add(Errors.INFO_StringBuilderRequiresInit);
                }
            }

            // now determine the default for field marshaling or P/Invoke
            if (ut == UnmanagedType.AsAny)
            {
                Debug.Assert(desc.IsStructField || !desc.IsComInterop);

                if (desc.AnsiStrings) ut = UnmanagedType.LPStr;
                else if (desc.UnicodeStrings) ut = UnmanagedType.LPWStr;
                else ut = UnmanagedType.LPTStr;
            }

            return ut;
        }

        #endregion

        #region Properties

        public override int TypeSize
        {
	        get 
	        { 
                if (fixedLength.HasValue) return fixedLength.Value * (fixedLengthAnsi ? 1 : 2);
                else return base.TypeSize;
	        }
        }

        public override int AlignmentRequirement
        {
	        get 
	        { 
                if (fixedLength.HasValue) return (fixedLengthAnsi ? 1 : 2);
		        else return base.AlignmentRequirement;
	        }
        }

        #endregion

        #region Printing

        public override void PrintPostIdentifierTo(ICodePrinter printer, PrintFlags flags)
        {
            if (fixedLength.HasValue)
            {
                printer.Print(OutputType.Operator, "[");
                printer.Print(OutputType.Literal, fixedLength.Value.ToString());
                printer.Print(OutputType.Operator, "]");
            }
        }

        #endregion
    }
}
