// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CodeParam = System.CodeDom.CodeParameterDeclarationExpression;

namespace PInvoke.Transform
{

    #region "LanguageType"

    /// <summary>
    /// Supported language types to export as
    /// </summary>
    /// <remarks></remarks>
    public enum LanguageType
    {
        VisualBasic,
        CSharp
    }

    #endregion

    #region "MarshalAttributeFactory"

    public enum StringKind
    {
        Ascii,
        Unicode,
        Unknown
    }

    public enum BooleanType
    {
        Windows,
        CStyle,
        Variant
    }

    static internal class MarshalAttributeFactory
    {

        static internal CodeAttributeDeclaration CreateInAttribute()
        {
            CodeAttributeDeclaration decl = new CodeAttributeDeclaration(new CodeTypeReference(typeof(InAttribute)));
            return decl;
        }

        static internal CodeAttributeDeclaration CreateOutAttribute()
        {
            CodeAttributeDeclaration decl = new CodeAttributeDeclaration(new CodeTypeReference(typeof(OutAttribute)));
            return decl;
        }

        /// <summary>
        /// Create an attribute to marshal the boolean type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        static internal CodeAttributeDeclaration CreateBooleanMarshalAttribute(BooleanType type)
        {

            UnmanagedType ut = default(UnmanagedType);
            switch (type)
            {
                case BooleanType.CStyle:
                    ut = UnmanagedType.I1;
                    break;
                case BooleanType.Windows:
                    ut = UnmanagedType.Bool;
                    break;
                case BooleanType.Variant:
                    ut = UnmanagedType.VariantBool;
                    break;
                default:
                    InvalidEnumValue(type);
                    ut = UnmanagedType.AnsiBStr;
                    break;
            }

            return CreateUnmanagedTypeAttribute(ut);
        }

        static internal CodeAttributeDeclaration CreateUnmanagedTypeAttribute(UnmanagedType type)
        {
            CodeAttributeDeclaration decl = new CodeAttributeDeclaration(new CodeTypeReference(typeof(MarshalAsAttribute)));
            decl.Arguments.Add(new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(UnmanagedType)), type.ToString())));
            return decl;
        }

        static internal CodeAttributeDeclaration CreateArrayParamTypeAttribute(UnmanagedType arraySubType, CodeAttributeArgument sizeArg)
        {
            CodeAttributeDeclaration decl = CreateUnmanagedTypeAttribute(UnmanagedType.LPArray);
            decl.Arguments.Add(new CodeAttributeArgument("ArraySubType", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(UnmanagedType)), arraySubType.ToString())));
            decl.Arguments.Add(sizeArg);
            return decl;
        }

        static internal CodeAttributeDeclaration CreateStringMarshalAttribute(CharSet strType)
        {
            CodeAttributeDeclaration decl = new CodeAttributeDeclaration(new CodeTypeReference(typeof(MarshalAsAttribute)));
            string field = null;
            switch (strType)
            {
                case CharSet.Ansi:
                    field = "LPStr";
                    break;
                case CharSet.Unicode:
                    field = "LPWStr";
                    break;
                case CharSet.Auto:
                    field = "LPTStr";
                    break;
                default:
                    InvalidEnumValue(strType);
                    field = null;
                    break;
            }

            decl.Arguments.Add(new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(UnmanagedType)), field)));
            return decl;
        }

        static internal CodeAttributeDeclaration CreateDebuggerStepThroughAttribute()
        {
            return new CodeAttributeDeclaration(new CodeTypeReference(typeof(DebuggerStepThroughAttribute)));
        }

        static internal CodeAttributeDeclaration CreateGeneratedCodeAttribute()
        {
            CodeAttributeDeclaration decl = new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute)));
            decl.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(PInvoke.Constants.ProductName)));
            decl.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(PInvoke.Constants.FriendlyVersion)));
            return decl;
        }

        /// <summary>
        /// Generate the StructLayoutAttribute attribute
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        static internal CodeAttributeDeclaration CreateStructLayoutAttribute(LayoutKind kind)
        {
            CodeTypeReference attrRef = new CodeTypeReference(typeof(Runtime.InteropServices.StructLayoutAttribute));
            CodeAttributeDeclaration attr = new CodeAttributeDeclaration(new CodeTypeReference(typeof(StructLayoutAttribute)));
            attr.Arguments.Add(new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(LayoutKind)), kind.ToString())));
            return attr;
        }

        /// <summary>
        /// Create the FieldOffsetAttribute for the specified value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        static internal CodeAttributeDeclaration CreateFieldOffsetAttribute(Int32 value)
        {
            CodeAttributeDeclaration attr = new CodeAttributeDeclaration(new CodeTypeReference(typeof(Runtime.InteropServices.FieldOffsetAttribute)));
            attr.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(value)));
            return attr;
        }

        /// <summary>
        /// Create a DllImport attribute 
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="entryPoint"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        static internal CodeAttributeDeclaration CreateDllImportAttribute(string dllName, string entryPoint, NativeCallingConvention conv)
        {
            CodeAttributeDeclaration attr = new CodeAttributeDeclaration(new CodeTypeReference(typeof(Runtime.InteropServices.DllImportAttribute)));
            attr.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(dllName)));
            attr.Arguments.Add(new CodeAttributeArgument("EntryPoint", new CodePrimitiveExpression(entryPoint)));

            // If the calling convention for a DLl is not the standard WinApi then output the extra information
            // into the attribute
            CallingConvention kind = default(CallingConvention);
            if (TryConvertToInteropCallingConvention(conv, ref kind) && kind != CallingConvention.Winapi)
            {
                attr.Arguments.Add(new CodeAttributeArgument("CallingConvention", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(CallingConvention)), kind.ToString())));
            }

            return attr;
        }

        static internal CodeAttributeDeclaration CreateUnmanagedFunctionPointerAttribute(NativeCallingConvention conv)
        {
            CodeAttributeDeclaration attr = new CodeAttributeDeclaration(new CodeTypeReference(typeof(Runtime.InteropServices.UnmanagedFunctionPointerAttribute)));
            CallingConvention kind = default(CallingConvention);
            if (TryConvertToInteropCallingConvention(conv, ref kind))
            {
                attr.Arguments.Add(new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(CallingConvention)), kind.ToString())));
            }

            return attr;
        }

        private static bool TryConvertToInteropCallingConvention(NativeCallingConvention conv, ref CallingConvention kind)
        {
            switch (conv)
            {
                case NativeCallingConvention.WinApi:
                    kind = CallingConvention.Winapi;
                    return true;
                case NativeCallingConvention.CDeclaration:
                    kind = CallingConvention.Cdecl;
                    return true;
                case NativeCallingConvention.Standard:
                    kind = CallingConvention.StdCall;
                    return true;
                default:
                    return false;
            }
        }


    }
    #endregion

    #region "CodeDomUtil"

    static internal class CodeDomUtil
    {

        static internal bool IsStringBuilderType(CodeTypeReference typeRef)
        {
            return IsType(typeRef, typeof(StringBuilder));
        }

        static internal bool IsIntPtrType(CodeTypeReference typeRef)
        {
            return IsType(typeRef, typeof(IntPtr));
        }

        static internal bool IsType(CodeTypeReference typeRef, Type type)
        {
            return 0 == string.CompareOrdinal(typeRef.BaseType, type.FullName);
        }

        static internal CodeExpression ReferenceVarAsType(CodeVariableDeclarationStatement var, Type type)
        {
            return ReferenceVarAsType(var, new CodeTypeReference(type));
        }

        /// <summary>
        /// Reference the specefied variable but make sure that it's of the specified type.  If it's
        /// not then take the appropriate action to get it there 
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        static internal CodeExpression ReferenceVarAsType(CodeVariableDeclarationStatement var, CodeTypeReference type)
        {
            return EnsureType(new CodeVariableReferenceExpression(var.Name), var.Type, type);
        }


        /// <summary>
        /// Create a reference to the specifed primitive but make sure it's the correct type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        static internal CodeExpression CreatePrimitiveAsType<T>(T val, CodeTypeReference target)
        {
            return EnsureType(new CodePrimitiveExpression(val), new CodeTypeReference(typeof(T)), target);
        }

        /// <summary>
        /// Ensure the specified expression is of the correct type.  Perform casts as necessary to get it there
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="cur"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        static internal CodeExpression EnsureType(CodeExpression expr, CodeTypeReference cur, CodeTypeReference target)
        {
            if (AreEqual(cur, target))
            {
                return expr;
            }

            return new CodeCastExpression(target, expr);
        }

        /// <summary>
        /// Are the two type references equal?
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        static internal bool AreEqual(CodeTypeReference left, CodeTypeReference right)
        {
            if (left == null && right == null)
            {
                return true;
            }
            else if (left == null || right == null)
            {
                return false;
            }

            // Base type comparison
            if (0 != string.CompareOrdinal(left.BaseType, right.BaseType))
            {
                return false;
            }

            // Array data
            if (left.ArrayRank != right.ArrayRank || !AreEqual(left.ArrayElementType, right.ArrayElementType))
            {
                return false;
            }

            return true;
        }


        static internal bool AreEqual(Type left, CodeTypeReference right)
        {
            return AreEqual(new CodeTypeReference(left), right);
        }
    }

    #endregion

}
