/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

namespace SignatureGenerator
{
    /// <summary>
    /// Describes one error message (with a unique error code).
    /// </summary>
    public struct ErrorDesc
    {
        public readonly int ErrorCode;
        public readonly Severity Severity;
        public readonly string Message;

        public ErrorDesc(int errorCode, Severity severity, string message)
        {
            Debug.Assert(message != null);

            this.ErrorCode = errorCode;
            this.Severity = severity;
            this.Message = message;
        }

        #region PrintTo

        public void PrintTo(ILogPrinter logPrinter)
        {
            Debug.Assert(logPrinter != null);
            logPrinter.PrintEntry(Severity, ErrorCode, Message);
        }

        public void PrintTo(ILogPrinter logPrinter, object arg1)
        {
            Debug.Assert(logPrinter != null);
            logPrinter.PrintEntry(Severity, ErrorCode, String.Format(Message, arg1));
        }

        public void PrintTo(ILogPrinter logPrinter, object arg1, object arg2)
        {
            Debug.Assert(logPrinter != null);
            logPrinter.PrintEntry(Severity, ErrorCode, String.Format(Message, arg1, arg2));
        }

        public void PrintTo(ILogPrinter logPrinter, object arg1, object arg2, object arg3)
        {
            Debug.Assert(logPrinter != null);
            logPrinter.PrintEntry(Severity, ErrorCode, String.Format(Message, arg1, arg2, arg3));
        }

        public void PrintTo(ILogPrinter logPrinter, params object[] args)
        {
            Debug.Assert(logPrinter != null);
            logPrinter.PrintEntry(Severity, ErrorCode, String.Format(Message, args));
        }

        #endregion
    }

    internal static class Errors
    {
#if DEBUG
        static Errors()
        {
            Set<int> codes = new Set<int>();

            // verify integrity of error codes
            foreach (FieldInfo fi in typeof(Errors).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                ErrorDesc error_desc = (ErrorDesc)fi.GetValue(null);

                switch (error_desc.Severity)
                {
                    case Severity.Error:    Debug.Assert(error_desc.ErrorCode < 2000); break;
                    case Severity.Warning:  Debug.Assert(error_desc.ErrorCode >= 2000 && error_desc.ErrorCode < 3000); break;
                    case Severity.Info:     Debug.Assert(error_desc.ErrorCode >= 3000); break;
                }

                Debug.Assert(!codes.Contains(error_desc.ErrorCode));
                codes.Add(error_desc.ErrorCode);
            }
        }
#endif

        // errors
        public static readonly ErrorDesc ERROR_ArraySizeNotAllowedForByref               = new ErrorDesc(1001, Severity.Error, Resources.ERROR_ArraySizeNotAllowedForByref);
        public static readonly ErrorDesc ERROR_ArraySizeParamIndexOutOfRange             = new ErrorDesc(1002, Severity.Error, Resources.ERROR_ArraySizeParamIndexOutOfRange);
        public static readonly ErrorDesc ERROR_ArraySizeParamWrongType                   = new ErrorDesc(1003, Severity.Error, Resources.ERROR_ArraySizeParamWrongType);
        public static readonly ErrorDesc ERROR_ByValArrayInvalidLength                   = new ErrorDesc(1004, Severity.Error, Resources.ERROR_ByValArrayInvalidLength);
        public static readonly ErrorDesc ERROR_CustomMarshalerNotAllowedOnFields         = new ErrorDesc(1005, Severity.Error, Resources.ERROR_CustomMarshalerNotAllowedOnFields);
        public static readonly ErrorDesc ERROR_CustomMarshalerNotAllowedOnValueTypes     = new ErrorDesc(1006, Severity.Error, Resources.ERROR_CustomMarshalerNotAllowedOnValueTypes);
        public static readonly ErrorDesc ERROR_GenericTypesNotAllowed                    = new ErrorDesc(1007, Severity.Error, Resources.ERROR_GenericTypesNotAllowed);
        public static readonly ErrorDesc ERROR_HandlesNotPermittedAsArrayElements        = new ErrorDesc(1008, Severity.Error, Resources.ERROR_HandlesNotPermittedAsArrayElements);
        public static readonly ErrorDesc ERROR_InvalidManagedUnmanagedCombo              = new ErrorDesc(1009, Severity.Error, Resources.ERROR_InvalidManagedUnmanagedCombo);
        public static readonly ErrorDesc ERROR_InvalidUnmanagedSize                      = new ErrorDesc(1010, Severity.Error, Resources.ERROR_InvalidUnmanagedSize);
        public static readonly ErrorDesc ERROR_MarshalerHasNoGetInstance                 = new ErrorDesc(1011, Severity.Error, Resources.ERROR_MarshalerHasNoGetInstance);
        public static readonly ErrorDesc ERROR_MarshalerIsNotICustomMarshaler            = new ErrorDesc(1012, Severity.Error, Resources.ERROR_MarshalerIsNotICustomMarshaler);
        public static readonly ErrorDesc ERROR_MarshalingAllowedForCom                   = new ErrorDesc(1013, Severity.Error, Resources.ERROR_MarshalingAllowedForCom);
        public static readonly ErrorDesc ERROR_NoFieldOffsetInSequentialLayout           = new ErrorDesc(1014, Severity.Error, Resources.ERROR_NoFieldOffsetInSequentialLayout);
        public static readonly ErrorDesc ERROR_NoNestedArrayMarshaling                   = new ErrorDesc(1015, Severity.Error, Resources.ERROR_NoNestedArrayMarshaling);
        public static readonly ErrorDesc ERROR_PInvokeIsNotStatic                        = new ErrorDesc(1016, Severity.Error, Resources.ERROR_PInvokeIsNotStatic);
        public static readonly ErrorDesc ERROR_RecursiveStructureDeclaration             = new ErrorDesc(1017, Severity.Error, Resources.ERROR_RecursiveStructureDeclaration);
        public static readonly ErrorDesc ERROR_StringBuilderFieldsDisallowed             = new ErrorDesc(1018, Severity.Error, Resources.ERROR_StringBuilderFieldsDisallowed);
        public static readonly ErrorDesc ERROR_TypeHasNoLayout                           = new ErrorDesc(1019, Severity.Error, Resources.ERROR_TypeHasNoLayout);
        public static readonly ErrorDesc ERROR_UnexpectedEnumUnderlyingType              = new ErrorDesc(1020, Severity.Error, Resources.ERROR_UnexpectedEnumUnderlyingType);
        public static readonly ErrorDesc ERROR_UnmanagedPointersToRefType                = new ErrorDesc(1021, Severity.Error, Resources.ERROR_UnmanagedPointersToRefType);
        public static readonly ErrorDesc ERROR_UnmanagedTypeRequiredForField             = new ErrorDesc(1022, Severity.Error, Resources.ERROR_UnmanagedTypeRequiredForField);
        public static readonly ErrorDesc ERROR_UnsupportedAlignment                      = new ErrorDesc(1023, Severity.Error, Resources.ERROR_UnsupportedAlignment);
        public static readonly ErrorDesc ERROR_VBByRefParamNotByRef                      = new ErrorDesc(1024, Severity.Error, Resources.ERROR_VBByRefParamNotByRef);
        public static readonly ErrorDesc ERROR_VariantReturnTypeNotSupported             = new ErrorDesc(1025, Severity.Error, Resources.ERROR_VariantReturnTypeNotSupported);
        public static readonly ErrorDesc ERROR_MisalignedReferenceTypeField              = new ErrorDesc(1026, Severity.Error, Resources.ERROR_MisalignedReferenceTypeField);
        public static readonly ErrorDesc ERROR_OverlappingReferenceTypeField             = new ErrorDesc(1027, Severity.Error, Resources.ERROR_OverlappingReferenceTypeField);

        // warnings
        public static readonly ErrorDesc WARN_ArraySizeDefaultsToOne                     = new ErrorDesc(2001, Severity.Warning, Resources.WARN_ArraySizeDefaultsToOne);
        public static readonly ErrorDesc WARN_ArraySizesIgnored                          = new ErrorDesc(2002, Severity.Warning, Resources.WARN_ArraySizesIgnored);
        public static readonly ErrorDesc WARN_ByValRefTypeMarkedOut                      = new ErrorDesc(2003, Severity.Warning, Resources.WARN_ByValRefTypeMarkedOut);
        public static readonly ErrorDesc WARN_ByValValueTypeMarkedOut                    = new ErrorDesc(2004, Severity.Warning, Resources.WARN_ByValValueTypeMarkedOut);
        public static readonly ErrorDesc WARN_InsufficientUnmanagedSize                  = new ErrorDesc(2005, Severity.Warning, Resources.WARN_InsufficientUnmanagedSize);
        public static readonly ErrorDesc WARN_LayoutBlittableMarkedIn                    = new ErrorDesc(2006, Severity.Warning, Resources.WARN_LayoutBlittableMarkedIn);
        public static readonly ErrorDesc WARN_LayoutBlittableMarkedOut                   = new ErrorDesc(2007, Severity.Warning, Resources.WARN_LayoutBlittableMarkedOut);
        public static readonly ErrorDesc WARN_MarshalerTypeNotFound                      = new ErrorDesc(2008, Severity.Warning, Resources.WARN_MarshalerTypeNotFound);
        public static readonly ErrorDesc WARN_NonSpecificDelegateUsed                    = new ErrorDesc(2009, Severity.Warning, Resources.WARN_NonSpecificDelegateUsed);
        public static readonly ErrorDesc WARN_NoPackEffectOnExplicitLayout               = new ErrorDesc(2010, Severity.Warning, Resources.WARN_NoPackEffectOnExplicitLayout);
        public static readonly ErrorDesc WARN_PInvokeIsPublic                            = new ErrorDesc(2011, Severity.Warning, Resources.WARN_PInvokeIsPublic);
        public static readonly ErrorDesc WARN_NormalizedStructure                        = new ErrorDesc(2012, Severity.Warning, Resources.WARN_NormalizedStructure);
        public static readonly ErrorDesc WARN_VarargIsNotCdecl                           = new ErrorDesc(2013, Severity.Warning, Resources.WARN_VarargIsNotCdecl);
        public static readonly ErrorDesc WARN_ByValStringMarkedOut                       = new ErrorDesc(2014, Severity.Warning, Resources.WARN_ByValStringMarkedOut);

        // info
        public static readonly ErrorDesc INFO_ArraySizeDeterminedDynamically             = new ErrorDesc(3001, Severity.Info, Resources.INFO_ArraySizeDeterminedDynamically);
        public static readonly ErrorDesc INFO_ArraySizeIsByParameter                     = new ErrorDesc(3002, Severity.Info, Resources.INFO_ArraySizeIsByParameter);
        public static readonly ErrorDesc INFO_ArraySizeIsByParameterPlusConstant         = new ErrorDesc(3003, Severity.Info, Resources.INFO_ArraySizeIsByParameterPlusConstant);
        public static readonly ErrorDesc INFO_ArraySizeIsConstant                        = new ErrorDesc(3004, Severity.Info, Resources.INFO_ArraySizeIsConstant);
        public static readonly ErrorDesc INFO_AsAnyMarshaling                            = new ErrorDesc(3005, Severity.Info, Resources.INFO_AsAnyMarshaling);
        public static readonly ErrorDesc INFO_AutoCharacterMarshaling                    = new ErrorDesc(3006, Severity.Info, Resources.INFO_AutoCharacterMarshaling);
        public static readonly ErrorDesc INFO_AutoStringMarshaling                       = new ErrorDesc(3007, Severity.Info, Resources.INFO_AutoStringMarshaling);
        public static readonly ErrorDesc INFO_BewarePrematureDelegateRelease             = new ErrorDesc(3008, Severity.Info, Resources.INFO_BewarePrematureDelegateRelease);
        public static readonly ErrorDesc INFO_BewareStringImmutability                   = new ErrorDesc(3009, Severity.Info, Resources.INFO_BewareStringImmutability);
        public static readonly ErrorDesc INFO_BufferCallbackIn                           = new ErrorDesc(3010, Severity.Info, Resources.INFO_BufferCallbackIn);
        public static readonly ErrorDesc INFO_BufferCallbackInOut                        = new ErrorDesc(3011, Severity.Info, Resources.INFO_BufferCallbackInOut);
        public static readonly ErrorDesc INFO_BufferCallbackOut                          = new ErrorDesc(3012, Severity.Info, Resources.INFO_BufferCallbackOut);
        public static readonly ErrorDesc INFO_BufferInOut                                = new ErrorDesc(3013, Severity.Info, Resources.INFO_BufferInOut);
        public static readonly ErrorDesc INFO_BufferOut                                  = new ErrorDesc(3014, Severity.Info, Resources.INFO_BufferOut);
        public static readonly ErrorDesc INFO_BufferTemporaryIn                          = new ErrorDesc(3015, Severity.Info, Resources.INFO_BufferTemporaryIn);
        public static readonly ErrorDesc INFO_CustomMarshaledParameter                   = new ErrorDesc(3016, Severity.Info, Resources.INFO_CustomMarshaledParameter);
        public static readonly ErrorDesc INFO_DefaultArrayAndRefTypeMarshaling           = new ErrorDesc(3017, Severity.Info, Resources.INFO_DefaultArrayAndRefTypeMarshaling);
        public static readonly ErrorDesc INFO_DefaultStringBuilderMarshaling             = new ErrorDesc(3018, Severity.Info, Resources.INFO_DefaultStringBuilderMarshaling);
        public static readonly ErrorDesc INFO_FixedLengthStringInvalidLength             = new ErrorDesc(3019, Severity.Info, Resources.INFO_FixedLengthStringInvalidLength);
        public static readonly ErrorDesc INFO_InterfacePointerToRCWMarshaling            = new ErrorDesc(3020, Severity.Info, Resources.INFO_InterfacePointerToRCWMarshaling);
        public static readonly ErrorDesc INFO_LayoutBlittableCallbackParameter           = new ErrorDesc(3021, Severity.Info, Resources.INFO_LayoutBlittableCallbackParameter);
        public static readonly ErrorDesc INFO_LayoutBlittableParameter                   = new ErrorDesc(3022, Severity.Info, Resources.INFO_LayoutBlittableParameter);
        public static readonly ErrorDesc INFO_LayoutDirectionBoth                        = new ErrorDesc(3023, Severity.Info, Resources.INFO_LayoutDirectionBoth);
        public static readonly ErrorDesc INFO_LayoutDirectionManagedToNative             = new ErrorDesc(3024, Severity.Info, Resources.INFO_LayoutDirectionManagedToNative);
        public static readonly ErrorDesc INFO_LayoutDirectionNativeToManaged             = new ErrorDesc(3025, Severity.Info, Resources.INFO_LayoutDirectionNativeToManaged);
        public static readonly ErrorDesc INFO_LayoutNonBlittableCallbackParameter        = new ErrorDesc(3026, Severity.Info, Resources.INFO_LayoutNonBlittableCallbackParameter);
        public static readonly ErrorDesc INFO_LayoutNonBlittableParameter                = new ErrorDesc(3027, Severity.Info, Resources.INFO_LayoutNonBlittableParameter);
        public static readonly ErrorDesc INFO_ManagedObjectToCCWMarshaling               = new ErrorDesc(3028, Severity.Info, Resources.INFO_ManagedObjectToCCWMarshaling);
        public static readonly ErrorDesc INFO_PointerIsCOMInterfacePtr                   = new ErrorDesc(3029, Severity.Info, Resources.INFO_PointerIsCOMInterfacePtr);
        public static readonly ErrorDesc INFO_PossibleAltNameLookup                      = new ErrorDesc(3030, Severity.Info, Resources.INFO_PossibleAltNameLookup);
        public static readonly ErrorDesc INFO_PossibleAutoAltNameLookup                  = new ErrorDesc(3031, Severity.Info, Resources.INFO_PossibleAutoAltNameLookup);
        public static readonly ErrorDesc INFO_SafeArrayWillMarshalAs                     = new ErrorDesc(3032, Severity.Info, Resources.INFO_SafeArrayWillMarshalAs);
        public static readonly ErrorDesc INFO_SeeMscorlibTlbForInterface                 = new ErrorDesc(3033, Severity.Info, Resources.INFO_SeeMscorlibTlbForInterface);
        public static readonly ErrorDesc INFO_StringBuilderRequiresInit                  = new ErrorDesc(3034, Severity.Info, Resources.INFO_StringBuilderRequiresInit);
        public static readonly ErrorDesc INFO_SafeArrayOfVariantsWrapperUse              = new ErrorDesc(3035, Severity.Info, Resources.INFO_SafeArrayOfVariantsWrapperUse);
    }
}
