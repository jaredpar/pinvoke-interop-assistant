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
using CodeParamCollection = System.CodeDom.CodeParameterDeclarationExpressionCollection;
using CodeParamPair = System.Collections.Generic.KeyValuePair<System.CodeDom.CodeParameterDeclarationExpression, System.CodeDom.CodeParameterDeclarationExpression>;
using static PInvoke.Contract;
using static PInvoke.Transform.CodeDomUtil;
using static PInvoke.Transform.MarshalAttributeFactory;

namespace PInvoke.Transform
{

    /// <summary>
    /// What kind of transformations does this plugin support
    /// </summary>
    /// <remarks></remarks>
    [Flags()]
    public enum TransformKindFlags
    {
        Signature = 0x1,
        StructMembers = 0x2,
        UnionMembers = 0x4,
        EnumMembers = 0x8,
        WrapperMethods = 0x10,

        All = TransformKindFlags.Signature | TransformKindFlags.StructMembers | TransformKindFlags.UnionMembers | TransformKindFlags.EnumMembers | TransformKindFlags.WrapperMethods
    }

    /// <summary>
    /// Base class for transformation classes that run on the CodeDom 
    /// </summary>
    /// <remarks></remarks>
    public abstract class TransformPlugin
    {

        private const string s_processedParamKey = "158d1b71-b224-4637-bd73-4f7f83b6777c";
        private const string s_processedReturnKey = "fd83becd-ba9f-4c08-9e79-c3285b74c8cb";

        private const string s_processedMemberKey = "a12fe995-7b38-4b84-82d5-38e315499806";

        private LanguageType _lang;
        public LanguageType LanguageType
        {
            get { return _lang; }
            set { _lang = value; }
        }

        public abstract TransformKindFlags TransformKind { get; }

        /// <summary>
        /// Process the parameters of the method or delegate
        /// </summary>
        /// <remarks></remarks>
        public void ProcessParameters(CodeMemberMethod method)
        {
            ThrowIfFalse(0 != (TransformKind & TransformKindFlags.Signature));

            ProcessParametersImpl(method.Parameters, false);
        }

        public void ProcessParameters(CodeTypeDelegate del)
        {
            ThrowIfFalse(0 != (TransformKind & TransformKindFlags.Signature));

            ProcessParametersImpl(del.Parameters, true);
        }

        /// <summary>
        /// Process the return type of a method
        /// </summary>
        /// <remarks></remarks>
        public void ProcessReturnType(CodeMemberMethod codeMethod)
        {
            ThrowIfFalse(0 != (TransformKind & TransformKindFlags.Signature));

            if (IsReturnProcessed(codeMethod))
            {
                return;
            }

            ProcessReturnTypeImpl(codeMethod, GetNativeReturnType(codeMethod), GetNativeReturnTypeSal(codeMethod));
        }

        public void ProcessStructMembers(CodeTypeDeclaration ctd)
        {
            ThrowIfFalse(0 != (TransformKind & TransformKindFlags.StructMembers));
            ProcessStructMembersImpl(ctd);
        }

        public void ProcessUnionMembers(CodeTypeDeclaration ctd)
        {
            ThrowIfFalse(0 != (TransformKind & TransformKindFlags.UnionMembers));
            ProcessUnionMembersImpl(ctd);
        }

        public List<CodeMemberMethod> ProcessWrapperMethods(CodeMemberMethod codeMethod)
        {
            ThrowIfFalse(0 != (TransformKind & TransformKindFlags.WrapperMethods));
            return ProcessWrapperMethodsImpl(codeMethod);
        }

        #region "Overridable"

        /// <summary>
        /// Process the return type of a method
        /// </summary>
        /// <param name="codeMethod"></param>
        /// <param name="ntType"></param>
        /// <param name="ntSal"></param>
        /// <remarks></remarks>

        protected virtual void ProcessReturnTypeImpl(CodeMemberMethod codeMethod, NativeType ntType, NativeSalAttribute ntSal)
        {
        }

        /// <summary>
        /// Override to process the parameters as a whole
        /// </summary>
        /// <param name="col"></param>
        /// <remarks></remarks>
        protected virtual void ProcessParametersImpl(CodeParameterDeclarationExpressionCollection col, bool isDelegate)
        {
            foreach (CodeParameterDeclarationExpression cur in col)
            {
                if (IsParamProcessed(cur))
                {
                    continue;
                }

                ProcessSingleParameter(cur, GetNativeParameter(cur), isDelegate);
            }
        }

        /// <summary>
        /// Process an individual parameter
        /// </summary>
        /// <param name="codeParam"></param>
        /// <param name="ntParam"></param>
        /// <remarks></remarks>

        protected virtual void ProcessSingleParameter(CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
        }

        protected virtual void ProcessStructMembersImpl(CodeTypeDeclaration ctd)
        {
            foreach (CodeTypeMember mem in ctd.Members)
            {
                if (IsMemberProcessed(mem))
                {
                    return;
                }

                CodeMemberField field = mem as CodeMemberField;
                if (field != null)
                {
                    NativeMember ntMem = GetNativeMember(field);
                    if (ntMem != null)
                    {
                        ProcessSingleStructField(ctd, field, ntMem);
                    }
                }
            }
        }


        protected virtual void ProcessSingleStructField(CodeTypeDeclaration ctd, CodeMemberField field, NativeMember ntMem)
        {
        }

        protected virtual void ProcessUnionMembersImpl(CodeTypeDeclaration ctd)
        {
            foreach (CodeTypeMember mem in ctd.Members)
            {
                if (IsMemberProcessed(mem))
                {
                    return;
                }

                CodeMemberField field = mem as CodeMemberField;
                if (field != null)
                {
                    NativeMember ntMem = GetNativeMember(field);
                    if (ntMem != null)
                    {
                        ProcessSingleUnionField(ctd, field, ntMem);
                    }
                }
            }
        }


        protected virtual void ProcessSingleUnionField(CodeTypeDeclaration ctd, CodeMemberField field, NativeMember ntMem)
        {
        }

        protected virtual List<CodeMemberMethod> ProcessWrapperMethodsImpl(CodeMemberMethod codeMethod)
        {
            CodeMemberMethod ret = ProcessSingleWrapperMethod(codeMethod);
            List<CodeMemberMethod> list = new List<CodeMemberMethod>();
            if (ret != null)
            {
                list.Add(ret);
            }

            return list;
        }

        protected virtual CodeMemberMethod ProcessSingleWrapperMethod(CodeMemberMethod codeMethod)
        {
            return null;
        }

        #endregion

        #region "Helpers"

        static internal NativeDefinedType GetDefinedType(CodeTypeDeclaration ctd)
        {
            object obj = ctd.UserData[TransformConstants.DefinedType];
            return obj as NativeDefinedType;
        }

        static internal NativeParameter GetNativeParameter(CodeParameterDeclarationExpression param)
        {
            object obj = param.UserData[TransformConstants.Param];
            return obj as NativeParameter;
        }

        static internal NativeType GetNativeReturnType(CodeMemberMethod codeMethod)
        {
            object obj = codeMethod.UserData[TransformConstants.ReturnType];
            return obj as NativeType;
        }

        static internal NativeSalAttribute GetNativeReturnTypeSal(CodeMemberMethod codeMethod)
        {
            object obj = codeMethod.UserData[TransformConstants.ReturnTypeSal];
            return obj as NativeSalAttribute;
        }

        static internal NativeType GetNativeReturnType(CodeTypeDelegate del)
        {
            object obj = del.UserData[TransformConstants.ReturnType];
            return obj as NativeType;
        }

        static internal NativeMember GetNativeMember(CodeMemberField mem)
        {
            if (mem.UserData.Contains(TransformConstants.Member))
            {
                return mem.UserData[TransformConstants.Member] as NativeMember;
            }
            return null;
        }

        protected bool IsCharsetSpecified(CodeTypeDeclaration ctd, ref CharSet charset)
        {
            foreach (CodeAttributeDeclaration attrib in ctd.CustomAttributes)
            {
                if (IsType(attrib.AttributeType, typeof(StructLayoutAttribute)))
                {
                    foreach (CodeAttributeArgument arg in attrib.Arguments)
                    {
                        if (0 == string.CompareOrdinal(arg.Name, "CharSet"))
                        {
                            CodeFieldReferenceExpression pValue = arg.Value as CodeFieldReferenceExpression;
                            if (pValue != null)
                            {
                                charset = (CharSet)Enum.Parse(typeof(CharSet), pValue.FieldName);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected void AddCharSet(CodeTypeDeclaration ctd, CharSet charSet)
        {
            CodeAttributeDeclaration attrib = null;
            foreach (CodeAttributeDeclaration cur in ctd.CustomAttributes)
            {
                if (IsType(cur.AttributeType, typeof(StructLayoutAttribute)))
                {
                    attrib = cur;
                    break;
                }
            }

            if (attrib == null)
            {
                attrib = MarshalAttributeFactory.CreateStructLayoutAttribute(LayoutKind.Auto);
            }

            attrib.Arguments.Add(new CodeAttributeArgument("CharSet", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(CharSet)), charSet.ToString())));
        }

        /// <summary>
        /// Is this one of the recognized boolean types
        /// </summary>
        /// <param name="nt"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected bool IsBooleanType(NativeType nt, ref BooleanType type)
        {
            if (nt.DigThroughTypedefAndNamedTypesFor("BOOL") != null)
            {
                type = BooleanType.Windows;
                return true;
            }

            nt = nt.DigThroughTypedefAndNamedTypes();
            if (nt.Kind == NativeSymbolKind.BuiltinType && ((NativeBuiltinType)nt).BuiltinType == BuiltinType.NativeBoolean)
            {
                type = BooleanType.CStyle;
                return true;
            }

            return false;
        }

        protected bool IsSystemIntType(NativeParameter ntParam)
        {
            return IsSystemIntType(ntParam.NativeType);
        }

        protected bool IsSystemIntType(NativeType nt)
        {
            if (nt.DigThroughTypedefAndNamedTypesFor("UINT_PTR") != null || nt.DigThroughTypedefAndNamedTypesFor("LONG_PTR") != null || nt.DigThroughTypedefAndNamedTypesFor("size_t") != null)
            {
                return true;
            }

            return false;
        }

        protected bool IsCharType(NativeType nt, ref CharSet charSet)
        {
            ThrowIfNull(nt);

            // BYTE is commonly typedef'd out to char however it is not a char
            // type persay.  Essentially BYTE[] should not convert into String or
            // StringBuilder
            if (nt.DigThroughTypedefAndNamedTypesFor("BYTE") != null)
            {
                return false;
            }

            if (nt.DigThroughTypedefAndNamedTypesFor("TCHAR") != null)
            {
                charSet = System.Runtime.InteropServices.CharSet.Auto;
                return true;
            }

            if (nt.DigThroughTypedefAndNamedTypesFor("WCHAR") != null || nt.DigThroughTypedefAndNamedTypesFor("wchar_t") != null)
            {
                charSet = System.Runtime.InteropServices.CharSet.Unicode;
                return true;
            }

            NativeType digged = nt.DigThroughTypedefAndNamedTypes();
            if (digged != null && digged.Kind == NativeSymbolKind.BuiltinType)
            {
                NativeBuiltinType bt = (NativeBuiltinType)digged;
                if (bt.BuiltinType == BuiltinType.NativeChar)
                {
                    charSet = System.Runtime.InteropServices.CharSet.Ansi;
                    return true;
                }
                else if (bt.BuiltinType == BuiltinType.NativeWChar)
                {
                    charSet = System.Runtime.InteropServices.CharSet.Unicode;
                    return true;
                }
            }

            return false;
        }

        protected bool IsArrayOfCharType(NativeType nt)
        {
            CharSet kind = System.Runtime.InteropServices.CharSet.None;
            return IsArrayOfCharType(nt, ref kind);
        }

        /// <summary>
        /// Is this an array of char's
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected bool IsArrayOfCharType(NativeType nt, ref CharSet charSet)
        {
            ThrowIfNull(nt);

            nt = nt.DigThroughTypedefAndNamedTypes();
            if (nt.Kind != NativeSymbolKind.ArrayType)
            {
                return false;
            }

            return IsCharType(((NativeArray)nt).RealType, ref charSet);
        }

        protected bool IsArrayOfBuiltin(NativeType nt, ref NativeBuiltinType bt)
        {
            ThrowIfNull(nt);

            nt = nt.DigThroughTypedefAndNamedTypes();
            if (nt.Kind != NativeSymbolKind.ArrayType)
            {
                return false;
            }

            NativeType realNt = ((NativeArray)nt).RealTypeDigged;
            if (realNt == null || realNt.Kind != NativeSymbolKind.BuiltinType)
            {
                return false;
            }

            bt = (NativeBuiltinType)realNt;
            return true;
        }

        protected bool IsPointerToBuiltin(NativeType nt, ref NativeBuiltinType bt)
        {
            ThrowIfNull(nt);

            nt = nt.DigThroughTypedefAndNamedTypes();
            if (nt.Kind != NativeSymbolKind.PointerType)
            {
                return false;
            }

            NativeType realNt = ((NativePointer)nt).RealTypeDigged;
            if (realNt.Kind != NativeSymbolKind.BuiltinType)
            {
                return false;
            }

            bt = (NativeBuiltinType)realNt;
            return true;
        }

        protected bool GetPointerTarget<T>(NativeType nt, ref T targetType) where T : NativeType
        {
            ThrowIfNull(nt);

            nt = nt.DigThroughTypedefAndNamedTypes();
            if (nt.Kind != NativeSymbolKind.PointerType)
            {
                return false;
            }

            NativePointer pointer = (NativePointer)nt;
            NativeType target = pointer.RealTypeDigged;
            if (target == null)
            {
                return false;
            }

            targetType = target as T;
            return targetType != null;
        }

        protected bool IsPointerToCharType(NativeParameter param)
        {
            CharSet kind = CharSet.None;
            return IsPointerToCharType(param, ref kind);
        }

        protected bool IsPointerToCharType(NativeParameter param, ref CharSet kind)
        {
            return IsPointerToCharType(param.NativeType, ref kind);
        }

        protected bool IsPointerToCharType(NativeMember mem, ref CharSet kind)
        {
            return IsPointerToCharType(mem.NativeType, ref kind);
        }

        protected bool IsPointerToCharType(NativeType type)
        {
            CharSet kind = CharSet.None;
            return IsPointerToCharType(type, ref kind);
        }

        protected bool IsPointerToCharType(NativeType type, ref CharSet kind)
        {
            NativeType digged = type.DigThroughTypedefAndNamedTypes();

            if (digged != null && digged.Kind == NativeSymbolKind.PointerType)
            {
                // Depending on the settings, LPTSTR and LPCTSTR are commonly going to be defined as pointing
                // to a CHAR instead of a TCHAR
                if (type.DigThroughTypedefAndNamedTypesFor("LPCTSTR") != null || type.DigThroughTypedefAndNamedTypesFor("LPTSTR") != null)
                {
                    kind = CharSet.Auto;
                    return true;
                }

                // WCHAR is commonly typedef'd into "unsigned short".  We need to manually dig through the typedefs
                // and named types looking for WCHAR
                NativePointer pt = (NativePointer)digged;
                return IsCharType(pt.RealType, ref kind);
            }

            return false;
        }

        protected bool IsPointerToNumber(NativeParameter param, ref BuiltinType bt)
        {
            NativeType paramType = param.NativeTypeDigged;
            if (paramType.Kind == NativeSymbolKind.PointerType)
            {
                NativePointer pointerNt = (NativePointer)paramType;
                if (pointerNt.RealTypeDigged.Kind == NativeSymbolKind.BuiltinType)
                {
                    bt = ((NativeBuiltinType)pointerNt.RealTypeDigged).BuiltinType;
                    if (NativeBuiltinType.IsNumberType(bt))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Is this a pointer to a constant type.  IsConst can only be applied to named types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected bool IsPointerToConst(NativeType type)
        {
            type = type.DigThroughTypedefAndNamedTypes();
            if (type.Kind != NativeSymbolKind.PointerType)
            {
                return false;
            }

            NativePointer ptr = (NativePointer)type;
            NativeNamedType named = ptr.RealType as NativeNamedType;
            if (named != null && named.IsConst)
            {
                return true;
            }

            return false;
        }

        protected bool IsReturnProcessed(CodeMemberMethod co)
        {
            return co.UserData.Contains(s_processedReturnKey);
        }

        protected void SetReturnProcessed(CodeMemberMethod co)
        {
            ThrowIfTrue(IsReturnProcessed(co));
            co.UserData[s_processedReturnKey] = true;
        }

        protected bool IsParamProcessed(CodeParam co)
        {
            return co.UserData.Contains(s_processedParamKey);
        }

        protected void SetParamProcessed(CodeParam co)
        {
            ThrowIfTrue(IsParamProcessed(co));
            co.UserData[s_processedParamKey] = true;
        }

        protected bool IsMemberProcessed(CodeTypeMember co)
        {
            return co.UserData.Contains(s_processedMemberKey);
        }

        protected void SetMemberProcessed(CodeTypeMember co)
        {
            ThrowIfTrue(IsMemberProcessed(co));
            co.UserData[s_processedMemberKey] = true;
        }

        /// <summary>
        /// Is this a HANDLE type
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected bool IsHandleType(NativeType nt)
        {
            nt = nt.DigThroughNamedTypes();
            if (nt.Kind == NativeSymbolKind.TypedefType && 0 == string.CompareOrdinal(nt.Name, "HANDLE"))
            {
                return true;
            }

            nt = nt.DigThroughTypedefAndNamedTypes();
            if (nt.Kind == NativeSymbolKind.PointerType)
            {
                NativeType realType = ((NativePointer)nt).RealTypeDigged;
                if (realType.Name.StartsWith("H") && realType.Name.EndsWith("__"))
                {
                    return true;
                }
            }

            return false;
        }

        protected bool IsVoidPointerType(NativeType nt)
        {
            NativeBuiltinType bt = null;
            if (IsPointerToBuiltin(nt, ref bt) && bt.BuiltinType == BuiltinType.NativeVoid)
            {
                return true;
            }

            return false;
        }


        protected bool IsWin32String(NativeType nt, ref CharSet kind)
        {
            bool notUsed = false;
            return IsWin32String(nt, ref kind, ref notUsed);
        }

        protected bool IsWin32String(NativeType nt, ref CharSet kind, ref bool isConst)
        {
            if (nt == null)
            {
                return false;
            }

            if (nt.DigThroughTypedefAndNamedTypesFor("LPWSTR") != null)
            {
                kind = CharSet.Unicode;
                isConst = false;
                return true;
            }

            if (nt.DigThroughTypedefAndNamedTypesFor("LPCWSTR") != null)
            {
                kind = CharSet.Unicode;
                isConst = true;
                return true;
            }

            if (nt.DigThroughTypedefAndNamedTypesFor("LPTSTR") != null)
            {
                kind = CharSet.Auto;
                isConst = false;
                return true;
            }

            if (nt.DigThroughTypedefAndNamedTypesFor("LPCTSTR") != null)
            {
                kind = CharSet.Auto;
                isConst = true;
                return true;
            }

            if (nt.DigThroughTypedefAndNamedTypesFor("LPSTR") != null)
            {
                kind = CharSet.Ansi;
                isConst = false;
                return true;
            }

            if (nt.DigThroughTypedefAndNamedTypesFor("LPCSTR") != null)
            {
                kind = CharSet.Ansi;
                isConst = true;
                return true;
            }

            return false;
        }

        public bool IsBstr(NativeType nt)
        {
            if (nt == null)
            {
                return false;
            }

            if (nt.DigThroughTypedefAndNamedTypesFor("BSTR") != null)
            {
                return true;
            }

            return false;
        }

        #endregion

    }


    #region "Signature Transforms"

    #region "BooleanTypesTransformPlugin"

    /// <summary>
    /// Look for any boolean types and mark them appropriately.  
    /// </summary>
    /// <remarks></remarks>
    internal class BooleanTypesTransformPlugin : TransformPlugin
    {

        protected override void ProcessReturnTypeImpl(CodeMemberMethod codeMethod, NativeType retNt, NativeSalAttribute retNtSal)
        {
            BooleanType bType = BooleanType.CStyle;
            if (retNt != null && IsBooleanType(retNt, ref bType))
            {
                codeMethod.ReturnType = new CodeTypeReference(typeof(bool));
                codeMethod.ReturnTypeCustomAttributes.Clear();
                codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateBooleanMarshalAttribute(bType));
                SetReturnProcessed(codeMethod);
            }
        }

        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            BooleanType bType = BooleanType.CStyle;
            if (IsBooleanType(ntParam.NativeType, ref bType))
            {
                codeParam.Type = new CodeTypeReference(typeof(bool));
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateBooleanMarshalAttribute(bType));
                SetParamProcessed(codeParam);
            }
        }

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }
    }

    #endregion

    #region "MutableStringBufferTransformPlugin"

    /// <summary>
    /// Whenever there is a char/wchar* that is not an In only parameter then it should be marshaled as 
    /// a StringBuilder.  This allows the CLR to copy data back into it
    /// </summary>
    /// <remarks></remarks>
    internal class MutableStringBufferTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }


        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            // Make sure it's a string pointer
            CharSet kind = CharSet.None;
            if (!IsPointerToCharType(ntParam, ref kind))
            {
                return;
            }

            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);

            if (analyzer.IsOutElementBuffer() || analyzer.IsOutElementBufferOptional() || analyzer.IsOutPartElementBuffer() || analyzer.IsOutPartElementBufferOptional() || analyzer.IsInOutElementBuffer())
            {
                // Switch the parameter to be a StringBuilder 
                codeParam.Type = new CodeTypeReference(typeof(StringBuilder));
                codeParam.CustomAttributes.Clear();

                // If this is an __out buffer then we should make sure to at the OutAttribute
                // so that marshalling is more efficient
                if (analyzer.IsValidOutOnly())
                {
                    codeParam.CustomAttributes.Add(CreateOutAttribute());
                }

                codeParam.CustomAttributes.Add(CreateStringMarshalAttribute(kind));
                SetParamProcessed(codeParam);
            }
        }

    }

    #endregion

    #region "ConstantStringTransformPlugin"

    /// <summary>
    /// If there is a char/wchar pointer which is In only then we should marshal it as a String with
    /// an In Marshal attribute
    /// </summary>
    /// <remarks></remarks>
    internal class ConstantStringTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }

        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            NativeType paramType = ntParam.NativeTypeDigged;

            // If this isn't a string pointer then we don't want to process it 
            CharSet kind = CharSet.None;
            if (!IsPointerToCharType(ntParam, ref kind))
            {
                return;
            }

            // Now determine if this is a constant param
            bool isConst = false;
            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
            if (analyzer.IsIn() || analyzer.IsInOptional())
            {
                isConst = true;
            }
            else if (IsPointerToConst(paramType))
            {
                isConst = true;
            }

            // If it's a constant pointer to a string so add the information
            if (isConst)
            {
                codeParam.Type = new CodeTypeReference(typeof(string));
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute());
                codeParam.CustomAttributes.Add(CreateStringMarshalAttribute(kind));
                SetParamProcessed(codeParam);
            }
        }
    }

    #endregion

    #region "SystemIntTransformPlugin"

    /// <summary>
    /// There are several types in the system that are sized differently depending on the platform (x86,amd64) and
    /// we need to marshal these types appropriately 
    /// </summary>
    /// <remarks></remarks>
    internal class SystemIntTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature | TransformKindFlags.WrapperMethods; }
        }

        protected override void ProcessReturnTypeImpl(CodeMemberMethod codeMethod, NativeType ntType, NativeSalAttribute ntSal)
        {
            if (ntType == null)
            {
                return;
            }

            if (!IsSystemIntType(ntType))
            {
                return;
            }

            // It's already an IntPtr so just return
            CodeTypeReference returnType = codeMethod.ReturnType;
            if (CodeDomUtil.IsIntPtrType(returnType))
            {
                return;
            }

            if (IsType(returnType, typeof(UInt32)))
            {
                codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysUInt));
            }
            else if (IsType(returnType, typeof(Int32)))
            {
                codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysInt));
            }
        }

        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            if (!IsSystemIntType(ntParam))
            {
                return;
            }

            // Whenever we have a delegate with a SysInt param we should marshal it as an IntPtr.  Trying to Marshal an Integer as 
            // SysInt causes a lot of exceptions no matter what the combination is.  
            if (isDelegateParam)
            {
                if (this.IsSystemIntType(ntParam))
                {
                    codeParam.Type = new CodeTypeReference(typeof(IntPtr));
                    codeParam.CustomAttributes.Clear();
                }
                return;
            }

            // If it's already an IntPtr we don't need to add any information
            if (CodeDomUtil.IsIntPtrType(codeParam.Type))
            {
                return;
            }
            else if (CodeDomUtil.IsType(codeParam.Type, typeof(UInt32)))
            {
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysUInt));
                SetParamProcessed(codeParam);
            }
            else if (CodeDomUtil.IsType(codeParam.Type, typeof(Int32)))
            {
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysInt));
                SetParamProcessed(codeParam);
            }
        }

        protected override System.Collections.Generic.List<System.CodeDom.CodeMemberMethod> ProcessWrapperMethodsImpl(System.CodeDom.CodeMemberMethod codeMethod)
        {
            List<CodeMemberMethod> list = new List<CodeMemberMethod>();
            CodeMemberMethod clone = default(CodeMemberMethod);

            clone = CloneWithPointer(codeMethod);
            if (clone != null)
            {
                list.Add(clone);
            }

            clone = CloneWithNonPointer(codeMethod);
            if (clone != null)
            {
                list.Add(clone);
            }

            return list;
        }

        private CodeMemberMethod CloneWithPointer(CodeMemberMethod origMethod)
        {
            bool anyChanged = false;
            CodeDomCloner clone = new CodeDomCloner();
            CodeMemberMethod newMethod = clone.CloneMethodSignature(origMethod);
            for (int i = 0; i <= origMethod.Parameters.Count - 1; i++)
            {
                CodeParam origParam = origMethod.Parameters[i];
                CodeParam newParam = newMethod.Parameters[i];
                NativeParameter ntParam = GetNativeParameter(origParam);
                if (ntParam == null)
                {
                    continue;
                }

                if (!IsSystemIntType(ntParam))
                {
                    continue;
                }

                if (IsType(newParam.Type, typeof(UInt32)) || IsType(newParam.Type, typeof(Int32)))
                {
                    newParam.CustomAttributes.Clear();
                    newParam.Type = new CodeTypeReference(typeof(IntPtr));
                    anyChanged = true;
                }
            }

            // Check the return type
            NativeType ret = GetNativeReturnType(origMethod);
            if (ret != null && IsSystemIntType(ret))
            {
                if (IsType(newMethod.ReturnType, typeof(Int32)) || IsType(newMethod.ReturnType, typeof(UInt32)))
                {
                    newMethod.ReturnType = new CodeTypeReference(typeof(IntPtr));
                    newMethod.ReturnTypeCustomAttributes.Clear();
                }
            }

            if (anyChanged)
            {
                return newMethod;
            }
            else
            {
                return null;
            }
        }

        private CodeMemberMethod CloneWithNonPointer(CodeMemberMethod origMethod)
        {
            bool anyChanged = false;
            CodeDomCloner clone = new CodeDomCloner();
            CodeMemberMethod newMethod = clone.CloneMethodSignature(origMethod);
            for (int i = 0; i <= origMethod.Parameters.Count - 1; i++)
            {
                CodeParam origParam = origMethod.Parameters[i];
                CodeParam newParam = newMethod.Parameters[i];
                NativeParameter ntParam = GetNativeParameter(origParam);
                if (ntParam == null)
                {
                    continue;
                }

                if (!IsSystemIntType(ntParam))
                {
                    continue;
                }

                if (IsType(newParam.Type, typeof(IntPtr)))
                {
                    newParam.CustomAttributes.Clear();
                    newParam.Type = new CodeTypeReference(typeof(UInt32));
                    newParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysUInt));
                    anyChanged = true;
                }
            }

            // Check the return type
            NativeType ret = GetNativeReturnType(origMethod);
            if (ret != null && IsSystemIntType(ret))
            {
                if (IsType(origMethod.ReturnType, typeof(IntPtr)))
                {
                    newMethod.ReturnType = new CodeTypeReference(typeof(UInt32));
                    newMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysUInt));
                }
            }

            if (anyChanged)
            {
                return newMethod;
            }
            else
            {
                return null;
            }
        }

    }

    #endregion

    #region "ArrayParameterTransformPlugin"

    /// <summary>
    /// Turn IntPtr into Type[] where appropriate.  
    /// </summary>
    /// <remarks></remarks>
    internal class ArrayParameterTransformPlugin : TransformPlugin
    {


        private CodeTransform _trans;
        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }

        internal ArrayParameterTransformPlugin(CodeTransform trans)
        {
            _trans = trans;
        }

        protected override void ProcessParametersImpl(System.CodeDom.CodeParameterDeclarationExpressionCollection col, bool isDelegate)
        {
            if (isDelegate)
            {
                return;
            }

            foreach (CodeParam p in col)
            {
                if (!IsParamProcessed(p))
                {
                    NativeParameter ntParam = GetNativeParameter(p);
                    if (ntParam == null)
                    {
                        continue;
                    }

                    ProcessParam(col, p, ntParam);
                }
            }

        }


        private void ProcessParam(CodeParamCollection col, System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam)
        {
            // If this is not an array or it's an array of characters then return.  Array's of characters should 
            // be marshaled as StringBuilders and is handled elsewhere
            CodeTypeReference ct = null;
            UnmanagedType ut = UnmanagedType.AnsiBStr;
            NativeType nt = ntParam.NativeTypeDigged;
            if (nt.Kind == NativeSymbolKind.ArrayType)
            {
                if (!ProcessArray((NativeArray)nt, ref ct, ref ut))
                {
                    return;
                }
            }
            else if (nt.Kind == NativeSymbolKind.PointerType)
            {
                if (!ProcessPointer((NativePointer)nt, ref ct, ref ut))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
            string size = null;
            if (!analyzer.IsInElementBuffer(out size))
            {
                return;
            }

            // Make sure this is not a void* element buffer.  We can't generated void[] into managed code
            // so ignore it here and let it process as a normal array
            if (CodeDomUtil.AreEqual(typeof(void), ct))
            {
                return;
            }

            CodeAttributeArgument sizeArg = null;
            if (!TryGenerateSizeArgument(col, size, ref sizeArg))
            {
                return;
            }

            // Finally, generate the attribute
            codeParam.Direction = FieldDirection.In;
            codeParam.Type = new CodeTypeReference(ct, 1);
            codeParam.CustomAttributes.Clear();
            codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateArrayParamTypeAttribute(ut, sizeArg));
            SetParamProcessed(codeParam);
        }

        private bool TryGenerateSizeArgument(CodeParamCollection col, string size, ref CodeAttributeArgument arg)
        {

            // Easy on is just a count
            Int32 count = 0;

            if (Int32.TryParse(size, out count))
            {
                // Don't process a size 1 element buffer.  That is not an array and will be handled by a 
                // different plugin.  It will be converted to a ByRef
                if (count == 1)
                {
                    return false;
                }

                arg = new CodeAttributeArgument("SizeConst", new CodePrimitiveExpression(count));
                return true;
            }

            // Now look for a named parameter
            for (int i = 0; i <= col.Count - 1; i++)
            {
                CodeParam cur = col[i];
                if (0 == string.CompareOrdinal(size, cur.Name))
                {
                    arg = new CodeAttributeArgument("SizeParamIndex", new CodePrimitiveExpression(i));
                    return true;
                }
            }

            return false;
        }

        private bool ProcessArray(NativeArray arr, ref CodeTypeReference elemType, ref UnmanagedType unmanagedType)
        {
            ThrowIfNull(arr);

            // Don't process a char[]
            if (IsArrayOfCharType(arr))
            {
                return false;
            }

            NativeBuiltinType bt = null;
            if (IsArrayOfBuiltin(arr, ref bt))
            {
                elemType = new CodeTypeReference(bt.ManagedType);
                unmanagedType = bt.UnmanagedType;
                return true;
            }
            else if (arr.RealTypeDigged.Kind == NativeSymbolKind.PointerType)
            {
                elemType = new CodeTypeReference(typeof(IntPtr));
                unmanagedType = System.Runtime.InteropServices.UnmanagedType.SysInt;
                return true;
            }
            else if (arr.RealTypeDigged.Kind == NativeSymbolKind.StructType || arr.RealTypeDigged.Kind == NativeSymbolKind.UnionType)
            {
                elemType = _trans.GenerateTypeReference(arr.RealTypeDigged);
                unmanagedType = System.Runtime.InteropServices.UnmanagedType.Struct;
                return true;
            }

            return false;
        }

        private bool ProcessPointer(NativePointer ptr, ref CodeTypeReference elemType, ref UnmanagedType unmanagedType)
        {
            ThrowIfNull(ptr);

            // Don't process a char*
            if (IsPointerToCharType(ptr))
            {
                return false;
            }

            NativeBuiltinType bt = null;
            if (IsPointerToBuiltin(ptr, ref bt))
            {
                elemType = new CodeTypeReference(bt.ManagedType);
                unmanagedType = bt.UnmanagedType;
                return true;
            }
            else if (ptr.RealTypeDigged.Kind == NativeSymbolKind.PointerType)
            {
                elemType = new CodeTypeReference(typeof(IntPtr));
                unmanagedType = System.Runtime.InteropServices.UnmanagedType.SysInt;
                return true;

            }
            else if (ptr.RealTypeDigged.Kind == NativeSymbolKind.StructType || ptr.RealTypeDigged.Kind == NativeSymbolKind.UnionType)
            {
                elemType = _trans.GenerateTypeReference(ptr.RealTypeDigged);
                unmanagedType = System.Runtime.InteropServices.UnmanagedType.Struct;
                return true;
            }

            return false;
        }

    }
    #endregion

    #region "BetterManagedTypesTransformPlugin"

    /// <summary>
    /// Occassionally we built a better Managed type that will Marshal exactly as the underlying Native
    /// type would.  In those cases we should use the Managed type 
    /// </summary>
    /// <remarks></remarks>
    internal class BetterManagedTypesTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }

        protected override void ProcessReturnTypeImpl(CodeMemberMethod codeMethod, NativeType ntType, NativeSalAttribute ntSal)
        {
            CodeTypeReference codeType = null;
            CodeAttributeDeclarationCollection codeAttrib = new CodeAttributeDeclarationCollection();
            if (HasBetterManagedType(ntType, ref codeType, codeAttrib, false))
            {
                codeMethod.ReturnType = codeType;
                codeMethod.ReturnTypeCustomAttributes.Clear();
                codeMethod.ReturnTypeCustomAttributes.AddRange(codeAttrib);
                SetReturnProcessed(codeMethod);
            }
        }

        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            if (ntParam == null || ntParam.NativeType == null)
            {
                return;
            }

            CodeTypeReference codeType = null;
            CodeAttributeDeclarationCollection codeAttrib = new CodeAttributeDeclarationCollection();
            if (HasBetterManagedType(ntParam.NativeType, ref codeType, codeAttrib, false))
            {
                codeParam.Type = codeType;
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.AddRange(codeAttrib);
                SetParamProcessed(codeParam);
                return;
            }

            // If this is a pointer type, see if the target type is a known structure
            NativeType digged = ntParam.NativeTypeDigged;
            if (digged == null || digged.Kind != NativeSymbolKind.PointerType)
            {
                return;
            }

            NativePointer ptr = (NativePointer)digged;

            if (ptr.RealType != null && HasBetterManagedType(ptr.RealType, ref codeType, codeAttrib, true))
            {
                bool isValid = true;
                SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
                if (analyzer.IsEmpty)
                {
                    codeParam.Direction = FieldDirection.Ref;
                }
                else if (analyzer.IsIn())
                {
                    codeParam.Direction = FieldDirection.Ref;
                    codeAttrib.Add(MarshalAttributeFactory.CreateInAttribute());
                }
                else if (analyzer.IsOut())
                {
                    codeParam.Direction = FieldDirection.Out;
                }
                else
                {
                    isValid = false;
                }

                if (isValid)
                {
                    codeParam.Type = codeType;
                    codeParam.CustomAttributes.Clear();
                    codeParam.CustomAttributes.AddRange(codeAttrib);
                    SetParamProcessed(codeParam);
                    return;
                }
            }
        }

        private bool HasBetterManagedType(NativeType ntType, ref CodeTypeReference codeType, CodeAttributeDeclarationCollection codeAttrib, bool isPointer)
        {
            NativeType digged = ntType.DigThroughTypedefAndNamedTypes();

            // DECIMAL Structure.  There are no additional attributes necessary to Marshal this dataStructure

            if (digged != null && 0 == string.CompareOrdinal("tagDEC", digged.Name) && digged.Kind == NativeSymbolKind.StructType)
            {
                codeType = new CodeTypeReference(typeof(decimal));
                codeAttrib.Clear();
                return true;
            }

            // CURRENCY Structure.  Use the decimal type and Marshal it as a UnmanagedType.Currency.  

            if (digged != null && 0 == string.CompareOrdinal("tagCY", digged.Name) && digged.Kind == NativeSymbolKind.UnionType)
            {
                codeType = new CodeTypeReference(typeof(decimal));
                codeAttrib.Clear();
                codeAttrib.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.Currency));
                return true;
            }


            if (digged != null && 0 == string.CompareOrdinal("GUID", digged.Name) && digged.Kind == NativeSymbolKind.StructType)
            {
                codeType = new CodeTypeReference(typeof(Guid));
                codeAttrib.Clear();
                return true;
            }

            // WCHAR is best as a Char structure.  Don't ever Marshal this as a CHAR* though, all of the String
            // logic code will do that
            CharSet kind = CharSet.None;
            if (!isPointer && IsCharType(ntType, ref kind) && CharSet.Unicode == kind)
            {
                codeType = new CodeTypeReference(typeof(char));
                codeAttrib.Clear();
                return true;
            }

            return false;
        }
    }

    #endregion

    #region "PointerToKnownTypes"

    /// <summary>
    /// In cases where we have a single pointer to a known type (say Int32) we want to generate
    /// a ByRef param to the strong type rather than an IntPtr
    /// </summary>
    /// <remarks></remarks>
    internal class PointerToKnownTypeTransformPlugin : TransformPlugin
    {


        private CodeTransform _trans;
        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }

        internal PointerToKnownTypeTransformPlugin(CodeTransform trans)
        {
            _trans = trans;
        }


        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            // There are several kinds of structures where we don't want to do this optimization.  The
            // most pertinent being handle types.  They have an ugly structure definition and 
            // everyone would just rather use the IntPtr version
            if (IsHandleType(ntParam.NativeType) || IsVoidPointerType(ntParam.NativeType))
            {
                return;
            }

            // If this is a pointer to a char type then don't convert it 
            if (IsPointerToCharType(ntParam))
            {
                return;
            }

            // Filter for Pointer types
            NativeType paramType = ntParam.NativeTypeDigged;
            if (paramType.Kind != NativeSymbolKind.PointerType)
            {
                return;
            }

            NativeType realNt = ((NativePointer)paramType).RealTypeDigged;
            if (!(realNt.Category == NativeSymbolCategory.Defined || realNt.Kind == NativeSymbolKind.BuiltinType))
            {
                return;
            }

            // Look at the SAL attribute and make sure this is just a single element pointer
            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
            bool isSingle = false;
            FieldDirection direction = default(FieldDirection);

            if (analyzer.IsEmpty)
            {
                // If there are no SAL attributes then assume this is a simple out pointer
                isSingle = true;
                direction = FieldDirection.Ref;
            }
            else
            {
                if (analyzer.IsIn())
                {
                    isSingle = true;
                    codeParam.CustomAttributes.Clear();
                    codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute());
                    direction = FieldDirection.Ref;
                }
                else if (analyzer.IsOut())
                {
                    isSingle = true;
                    codeParam.CustomAttributes.Clear();
                    codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateOutAttribute());
                    direction = FieldDirection.Out;
                }
                else if (analyzer.IsInOut())
                {
                    isSingle = true;
                    codeParam.CustomAttributes.Clear();
                    direction = FieldDirection.Ref;
                }
                else
                {
                    direction = FieldDirection.Ref;
                }
            }

            if (isSingle)
            {
                // Found one
                codeParam.Type = _trans.GenerateTypeReference(realNt);
                codeParam.Direction = direction;
                SetParamProcessed(codeParam);
            }
        }

    }

    #endregion

    #region "BStrTransformPlugin"

    /// <summary>
    /// Whenever a BSTR is used then change it to a String type
    /// </summary>
    /// <remarks></remarks>
    internal class BstrTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }

        protected override void ProcessReturnTypeImpl(System.CodeDom.CodeMemberMethod codeMethod, NativeType ntType, NativeSalAttribute ntSal)
        {
            if (!IsBstr(ntType))
            {
                return;
            }

            codeMethod.ReturnType = new CodeTypeReference(typeof(string));
            codeMethod.ReturnTypeCustomAttributes.Clear();
            codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr));
            SetReturnProcessed(codeMethod);
        }


        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            if (IsBstr(ntParam.NativeType))
            {
                ProcessBstrParam(codeParam, ntParam, isDelegateParam);
            }

            NativeType digged = ntParam.NativeTypeDigged;

            if (digged != null && digged.Kind == NativeSymbolKind.PointerType && IsBstr(((NativePointer)digged).RealType))
            {
                ProcessBstrPointerParam(codeParam, ntParam, isDelegateParam);
            }

        }


        private void ProcessBstrParam(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
            if (analyzer.IsEmpty)
            {
                codeParam.Type = new CodeTypeReference(typeof(string));
                codeParam.Direction = FieldDirection.In;
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr));
            }
            else if (analyzer.IsIn())
            {
                codeParam.Type = new CodeTypeReference(typeof(string));
                codeParam.Direction = FieldDirection.In;
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr));
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute());
            }
            else if (analyzer.IsOut())
            {
                codeParam.Type = new CodeTypeReference(typeof(string));
                codeParam.Direction = FieldDirection.Out;
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr));
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateOutAttribute());
            }
            else
            {
                // We don't understand how to Marshal a BSTR that is not __in, default or __out therefore use
                // an IntPtr.  Don't leave it as is or else it will get picked up as a normal String
                // and be Marshalled incorrectly
                codeParam.Type = new CodeTypeReference(typeof(IntPtr));
                codeParam.Direction = FieldDirection.In;
                codeParam.CustomAttributes.Clear();
            }

            SetParamProcessed(codeParam);
        }



        private void ProcessBstrPointerParam(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
            if (analyzer.IsIn())
            {
                codeParam.Type = new CodeTypeReference(typeof(string));
                codeParam.Direction = FieldDirection.Ref;
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr));
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute());
            }
            else if (analyzer.IsOut())
            {
                codeParam.Type = new CodeTypeReference(typeof(string));
                codeParam.Direction = FieldDirection.Out;
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr));
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateOutAttribute());
            }
            else
            {
                codeParam.Type = new CodeTypeReference(typeof(string));
                codeParam.Direction = FieldDirection.Ref;
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr));
            }

            SetParamProcessed(codeParam);
        }


    }

    #endregion

    #region "RawStringTransformPlugin"

    /// <summary>
    /// When there is a LP*STR member with no SAL annotations.  Go ahead and make it a StringBuilder parameter.  If it's const then
    /// just make it a String param.  If there is a SAL attribute thet it will be handled by one of the plugin designed to handle 
    /// SAL transforms
    /// </summary>
    /// <remarks></remarks>
    internal class RawStringTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }

        protected override void ProcessReturnTypeImpl(CodeMemberMethod codeMethod, NativeType ntType, NativeSalAttribute ntSal)
        {
            SalAnalyzer analyzer = new SalAnalyzer(ntSal);
            if (!analyzer.IsEmpty)
            {
                return;
            }

            CharSet kind = CharSet.None;
            if (!IsWin32String(ntType, ref kind))
            {
                return;
            }

            codeMethod.ReturnType = new CodeTypeReference(typeof(string));
            codeMethod.ReturnTypeCustomAttributes.Clear();
            codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateStringMarshalAttribute(kind));
            SetReturnProcessed(codeMethod);
        }

        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
            if (!analyzer.IsEmpty)
            {
                return;
            }

            CharSet kind = CharSet.None;
            bool isConst = false;
            if (!IsWin32String(ntParam.NativeType, ref kind, ref isConst))
            {
                return;
            }

            if (isConst)
            {
                codeParam.Type = new CodeTypeReference(typeof(string));
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute());
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateStringMarshalAttribute(kind));
            }
            else
            {
                codeParam.Type = new CodeTypeReference(typeof(StringBuilder));
                codeParam.CustomAttributes.Clear();
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateStringMarshalAttribute(kind));
            }

            this.SetParamProcessed(codeParam);
        }

    }
    #endregion

    #region "DoublePointerOutTransformPlugin"

    /// <summary>
    /// If we have a __deref_out parameter then go ahead and wrap it into an
    /// Out IntPtr parameter.
    /// </summary>
    /// <remarks></remarks>
    internal class DoublePointerOutTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }

        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            if (ntParam == null)
            {
                return;
            }

            NativeType type = ntParam.NativeType.DigThroughTypedefAndNamedTypes();
            if (type.Kind != NativeSymbolKind.PointerType)
            {
                return;
            }

            NativeType target = ((NativePointer)type).RealType.DigThroughTypedefAndNamedTypes();
            if (target.Kind != NativeSymbolKind.PointerType)
            {
                return;
            }

            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
            if (!analyzer.IsDerefOut())
            {
                return;
            }

            codeParam.Type = new CodeTypeReference(typeof(IntPtr));
            codeParam.Direction = FieldDirection.Out;
            SetParamProcessed(codeParam);
        }

    }


    #endregion

    #region "PointerPointerTransformPlugin"

    /// <summary>
    /// Convert Pointer Pointers (**) into Out/Ref/In IntPtr
    /// </summary>
    /// <remarks></remarks>
    internal class PointerPointerTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }

        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            if (ntParam == null)
            {
                return;
            }

            NativeType type = ntParam.NativeType.DigThroughTypedefAndNamedTypes();
            if (type.Kind != NativeSymbolKind.PointerType)
            {
                return;
            }

            NativeType target = ((NativePointer)type).RealType.DigThroughTypedefAndNamedTypes();
            if (target.Kind != NativeSymbolKind.PointerType)
            {
                return;
            }

            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
            if (analyzer.IsIn())
            {
                return;
            }

            codeParam.Type = new CodeTypeReference(typeof(IntPtr));
            codeParam.Direction = FieldDirection.Ref;
            SetParamProcessed(codeParam);
        }

    }

    #endregion

    #region "DirectionalModifiersTransformPlugin"

    /// <summary>
    /// As a last effort, if the parameter has SAL information and doesn't meet any other transformation
    /// then we will add the directional modifiers to the signature
    /// </summary>
    /// <remarks></remarks>
    internal class DirectionalModifiersTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.Signature; }
        }

        protected override void ProcessSingleParameter(System.CodeDom.CodeParameterDeclarationExpression codeParam, NativeParameter ntParam, bool isDelegateParam)
        {
            if (ntParam == null)
            {
                return;
            }

            // Only apply directional attributes to pointers
            if (!AreEqual(typeof(IntPtr), codeParam.Type) && !AreEqual(typeof(UIntPtr), codeParam.Type))
            {
                return;
            }

            SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
            if (analyzer.IsValidInOnly())
            {
                codeParam.CustomAttributes.Add(CreateInAttribute());
            }

            SetParamProcessed(codeParam);
        }

    }

    #endregion

    #endregion

    #region "Wrapper Methods"

    #region "OneWayStringBufferTransformPlugin"

    /// <summary>
    /// Whenever we see a String paramater that is out only tied to an in size parameter
    /// we should generate a wrapper allows the user to just deal with a single String value 
    /// </summary>
    /// <remarks></remarks>
    internal class OneWayStringBufferTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.WrapperMethods; }
        }

        protected override System.CodeDom.CodeMemberMethod ProcessSingleWrapperMethod(System.CodeDom.CodeMemberMethod codeMethod)
        {
            CodeParam bufParam = null;
            CodeParam sizeParam = null;

            if (!FindBufferAndSizeParam(codeMethod, ref bufParam, ref sizeParam))
            {
                return null;
            }

            CodeMemberMethod newMethod = GenerateWrapperSignature(codeMethod, bufParam, sizeParam);
            GenerateWrapperCode(newMethod, codeMethod, bufParam, sizeParam);
            return newMethod;
        }

        private bool FindBufferAndSizeParam(CodeMemberMethod codeMethod, ref CodeParam bufParam, ref CodeParam sizeParam)
        {
            sizeParam = null;
            bufParam = null;


            foreach (CodeParam param in codeMethod.Parameters)
            {
                if (!IsStringBuilderType(param.Type))
                {
                    continue;
                }

                // Check for a string pointer
                NativeParameter ntParam = GetNativeParameter(param);
                if (!IsPointerToCharType(ntParam))
                {
                    continue;
                }

                // See if this is an out element buffer
                SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
                string sizeParamName = null;
                if (!analyzer.IsOutElementBuffer(out sizeParamName) && !analyzer.IsOutElementBufferOptional(out sizeParamName))
                {
                    continue;
                }

                // Now look for the size parameter
                foreach (CodeParam cur in codeMethod.Parameters)
                {
                    if (!object.ReferenceEquals(cur, param) && 0 == string.CompareOrdinal(sizeParamName, cur.Name))
                    {
                        bufParam = param;
                        sizeParam = cur;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Used to generate the wrapper method
        /// </summary>
        /// <param name="origMethod"></param>
        /// <param name="bufParam"></param>
        /// <param name="sizeParam"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private CodeMemberMethod GenerateWrapperSignature(CodeMemberMethod origMethod, CodeParam bufParam, CodeParam sizeParam)
        {
            CodeDomCloner clone = new CodeDomCloner();
            CodeMemberMethod newMethod = new CodeMemberMethod();
            newMethod.Name = origMethod.Name;
            newMethod.ReturnType = clone.CloneTypeReference(origMethod.ReturnType);
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateDebuggerStepThroughAttribute());
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateGeneratedCodeAttribute());
            newMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            foreach (CodeParam origParam in origMethod.Parameters)
            {
                if (object.ReferenceEquals(origParam, bufParam))
                {
                    CodeParam newParam = new CodeParam();
                    newParam.Name = origParam.Name;
                    newParam.Direction = FieldDirection.Out;
                    newParam.Type = new CodeTypeReference(typeof(string));
                    newMethod.Parameters.Add(newParam);
                }
                else if (object.ReferenceEquals(origParam, sizeParam))
                {
                    // Don't need the size param in the new signature
                }
                else
                {
                    newMethod.Parameters.Add(clone.CloneParamNoAttributes(origParam));
                }
            }

            return newMethod;
        }

        private void GenerateWrapperCode(CodeMemberMethod newMethod, CodeMemberMethod origMethod, CodeParam bufParam, CodeParam sizeParam)
        {
            // Create the variables
            int sizeConst = 1024;
            CodeVariableDeclarationStatement bufVar = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(StringBuilder)), "var" + bufParam.Name);
            bufVar.InitExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(StringBuilder)), new CodePrimitiveExpression(sizeConst));
            newMethod.Statements.Add(bufVar);
            CodeVariableDeclarationStatement retVar = new CodeVariableDeclarationStatement(newMethod.ReturnType, "methodRetVar");
            newMethod.Statements.Add(retVar);

            // Call the method 
            List<CodeExpression> args = new List<CodeExpression>();
            foreach (CodeParam origParam in origMethod.Parameters)
            {
                if (object.ReferenceEquals(origParam, bufParam))
                {
                    CodeExpression varRef = CodeDirectionalSymbolExpression.Create(this.LanguageType, new CodeVariableReferenceExpression(bufVar.Name), origParam.Direction);
                    args.Add(varRef);
                }
                else if (object.ReferenceEquals(origParam, sizeParam))
                {
                    args.Add(new CodePrimitiveExpression(sizeConst));
                }
                else
                {
                    CodeExpression varRef = CodeDirectionalSymbolExpression.Create(this.LanguageType, new CodeVariableReferenceExpression(origParam.Name), origParam.Direction);
                    args.Add(varRef);
                }
            }
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(retVar.Name), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(TransformConstants.NativeMethodsName), origMethod.Name), args.ToArray())));

            // Assign the out string parameter
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(bufParam.Name), new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(bufVar.Name), "ToString")));

            // Return the value
            newMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(retVar.Name)));
        }

    }

    #endregion

    #region "TwoWayStringBufferTransformPlugin"

    /// <summary>
    /// When we find a two way string buffer parameter this will generate the code to test the error
    /// correction and recall the method.  Also it will generate a method with only a String parameter 
    /// </summary>
    /// <remarks></remarks>
    internal class TwoWayStringBufferTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.WrapperMethods; }
        }

        protected override System.CodeDom.CodeMemberMethod ProcessSingleWrapperMethod(System.CodeDom.CodeMemberMethod codeMethod)
        {
            CodeParam bufParam = null;
            CodeParam sizeParam = null;

            if (!FindParams(codeMethod, ref bufParam, ref sizeParam))
            {
                return null;
            }

            CodeMemberMethod newMethod = GenerateWrapperSignature(codeMethod, bufParam, sizeParam);
            GenerateWrapperCode(newMethod, codeMethod, bufParam, sizeParam);
            return newMethod;
        }

        private bool FindParams(CodeMemberMethod codeMethod, ref CodeParam bufParam, ref CodeParam sizeParam)
        {
            bufParam = null;
            sizeParam = null;
            foreach (CodeParam codeParam in codeMethod.Parameters)
            {
                NativeParameter ntParam = GetNativeParameter(codeParam);
                if (ntParam == null)
                {
                    continue;
                }

                if (!IsPointerToCharType(ntParam) || !IsStringBuilderType(codeParam.Type))
                {
                    continue;
                }

                SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
                string size = null;
                string readable = null;
                if (!analyzer.IsOutPartElementBuffer(out size, out readable) && !analyzer.IsOutPartElementBufferOptional(out size, out readable))
                {
                    continue;
                }

                // Look for the popular pattern
                //  -> __out_ecount_part(*size,*size+1)
                Match match = Regex.Match(readable, "\\*(\\w+)\\s*\\+\\s*\\d+");
                if (!match.Success || !size.StartsWith("*"))
                {
                    continue;
                }

                string str1 = size.Substring(1);
                string str2 = match.Groups[1].Value;
                if (0 != string.CompareOrdinal(str1, str2))
                {
                    continue;
                }

                // Now we just have to find the parameter
                foreach (CodeParam codeParam2 in codeMethod.Parameters)
                {
                    if (0 == string.CompareOrdinal(codeParam2.Name, str1))
                    {
                        sizeParam = codeParam2;
                        bufParam = codeParam;
                        return true;
                    }
                }
            }

            return false;
        }

        private CodeMemberMethod GenerateWrapperSignature(CodeMemberMethod origMethod, CodeParam bufParam, CodeParam sizeParam)
        {
            CodeDomCloner clone = new CodeDomCloner();
            CodeMemberMethod newMethod = new CodeMemberMethod();
            newMethod.Name = origMethod.Name;
            newMethod.ReturnType = clone.CloneTypeReference(origMethod.ReturnType);
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateDebuggerStepThroughAttribute());
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateGeneratedCodeAttribute());
            newMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            foreach (CodeParam origParam in origMethod.Parameters)
            {
                if (object.ReferenceEquals(origParam, bufParam))
                {
                    CodeParam newParam = new CodeParam();
                    newParam.Name = origParam.Name;
                    newParam.Direction = FieldDirection.Ref;
                    newParam.Type = new CodeTypeReference(typeof(string));
                    newMethod.Parameters.Add(newParam);
                }
                else if (object.ReferenceEquals(origParam, sizeParam))
                {
                    // Don't need the size param in the new signature
                }
                else
                {
                    newMethod.Parameters.Add(clone.CloneParamNoAttributes(origParam));
                }
            }

            return newMethod;

        }

        private void GenerateWrapperCode(CodeMemberMethod newMethod, CodeMemberMethod origMethod, CodeParam bufParam, CodeParam sizeParam)
        {
            // Create the variables
            CodeDomCloner clone = new CodeDomCloner();
            string jumpLabelName = "PerformCall";
            CodeVariableDeclarationStatement bufVar = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(StringBuilder)), "var" + bufParam.Name);
            newMethod.Statements.Add(bufVar);
            CodeVariableDeclarationStatement retVar = new CodeVariableDeclarationStatement(clone.CloneTypeReference(origMethod.ReturnType), "retVar_");
            newMethod.Statements.Add(retVar);
            CodeVariableDeclarationStatement sizeVar = new CodeVariableDeclarationStatement(clone.CloneTypeReference(sizeParam.Type), "sizeVar", new CodePrimitiveExpression(TransformConstants.DefaultBufferSize));
            newMethod.Statements.Add(sizeVar);
            CodeVariableDeclarationStatement oldSizeVar = new CodeVariableDeclarationStatement(clone.CloneTypeReference(sizeParam.Type), "oldSizeVar_");
            newMethod.Statements.Add(oldSizeVar);

            // Create the jump label
            newMethod.Statements.Add(new CodeLabeledStatement(jumpLabelName));

            // Save the old size
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(oldSizeVar.Name), new CodeVariableReferenceExpression(sizeVar.Name)));

            // Create the buffer
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(bufVar.Name), new CodeObjectCreateExpression(new CodeTypeReference(typeof(StringBuilder)), CodeDomUtil.ReferenceVarAsType(sizeVar, typeof(Int32)))));

            // Perform the method call
            List<CodeExpression> args = new List<CodeExpression>();
            foreach (CodeParam origParam in origMethod.Parameters)
            {
                string variableName = null;
                if (object.ReferenceEquals(origParam, bufParam))
                {
                    variableName = bufVar.Name;
                }
                else if (object.ReferenceEquals(origParam, sizeParam))
                {
                    variableName = sizeVar.Name;
                }
                else
                {
                    variableName = origParam.Name;
                }

                args.Add(CodeDirectionalSymbolExpression.Create(this.LanguageType, new CodeVariableReferenceExpression(variableName), origParam.Direction));
            }

            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(retVar.Name), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(TransformConstants.NativeMethodsName), origMethod.Name), args.ToArray())));

            // Check the return of the call 
            CodeConditionStatement recallCheck = new CodeConditionStatement();
            recallCheck.Condition = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(oldSizeVar.Name), CodeBinaryOperatorType.LessThanOrEqual, new CodeVariableReferenceExpression(sizeVar.Name));

            // Double the buffer
            recallCheck.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(sizeVar.Name), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(sizeVar.Name), CodeBinaryOperatorType.Multiply, CodeDomUtil.CreatePrimitiveAsType(2, sizeVar.Type))));

            // Jump to the label
            recallCheck.TrueStatements.Add(new CodeGotoStatement(jumpLabelName));
            newMethod.Statements.Add(recallCheck);

            // Save the return value
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(bufParam.Name), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(bufVar.Name), "ToString"))));

            // Return the value
            newMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(retVar.Name)));
        }

    }

    #endregion

    #region "TwoWayViaReturnStringBufferTransformPlugin"

    /// <summary>
    /// When we find a two way string buffer parameter this will generate the code to test the error
    /// correction and recall the method.  Also it will generate a method with only a String parameter 
    /// </summary>
    /// <remarks></remarks>
    internal class TwoWayViaReturnStringBufferTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.WrapperMethods; }
        }

        protected override System.CodeDom.CodeMemberMethod ProcessSingleWrapperMethod(System.CodeDom.CodeMemberMethod codeMethod)
        {
            CodeParam bufParam = null;
            CodeParam sizeParam = null;
            bool isPlusOne = false;

            if (!FindParams(codeMethod, ref bufParam, ref sizeParam, ref isPlusOne))
            {
                return null;
            }

            CodeMemberMethod newMethod = GenerateWrapperSignature(codeMethod, bufParam, sizeParam);
            GenerateWrapperCode(newMethod, codeMethod, bufParam, sizeParam, isPlusOne);
            return newMethod;
        }

        private bool FindParams(CodeMemberMethod codeMethod, ref CodeParam bufParam, ref CodeParam sizeParam, ref bool isPlusOne)
        {
            bufParam = null;
            sizeParam = null;
            foreach (CodeParam codeParam in codeMethod.Parameters)
            {
                NativeParameter ntParam = GetNativeParameter(codeParam);
                if (ntParam == null)
                {
                    continue;
                }

                if (!IsPointerToCharType(ntParam) || !IsStringBuilderType(codeParam.Type))
                {
                    continue;
                }

                SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
                string size = null;
                string readable = null;
                if (!analyzer.IsOutPartElementBuffer(out size, out readable) && !analyzer.IsOutPartElementBufferOptional(out size, out readable))
                {
                    continue;
                }

                // Look for the popular pattern
                //  -> __out_ecount_part(*size,return+1)
                if (!readable.StartsWith("return"))
                {
                    continue;
                }
                else if (Regex.IsMatch(readable, "return\\s*\\+\\s*1"))
                {
                    isPlusOne = true;
                }
                else
                {
                    isPlusOne = false;
                }

                if (size.StartsWith("*"))
                {
                    size = size.Substring(1);
                }

                // Now we just have to find the parameter
                foreach (CodeParam codeParam2 in codeMethod.Parameters)
                {
                    if (0 == string.CompareOrdinal(codeParam2.Name, size))
                    {
                        sizeParam = codeParam2;
                        bufParam = codeParam;
                        return true;
                    }
                }
            }

            return false;
        }

        private CodeMemberMethod GenerateWrapperSignature(CodeMemberMethod origMethod, CodeParam bufParam, CodeParam sizeParam)
        {
            CodeDomCloner clone = new CodeDomCloner();
            CodeMemberMethod newMethod = new CodeMemberMethod();
            newMethod.Name = origMethod.Name;
            newMethod.ReturnType = clone.CloneTypeReference(origMethod.ReturnType);
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateDebuggerStepThroughAttribute());
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateGeneratedCodeAttribute());
            newMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            foreach (CodeParam origParam in origMethod.Parameters)
            {
                if (object.ReferenceEquals(origParam, bufParam))
                {
                    CodeParam newParam = new CodeParam();
                    newParam.Name = origParam.Name;
                    newParam.Direction = FieldDirection.Out;
                    newParam.Type = new CodeTypeReference(typeof(string));
                    newMethod.Parameters.Add(newParam);
                }
                else if (object.ReferenceEquals(origParam, sizeParam))
                {
                    // Don't need the size param in the new signature
                }
                else
                {
                    newMethod.Parameters.Add(clone.CloneParamNoAttributes(origParam));
                }
            }

            return newMethod;
        }

        private void GenerateWrapperCode(CodeMemberMethod newMethod, CodeMemberMethod origMethod, CodeParam bufParam, CodeParam sizeParam, bool isPlusOne)
        {
            // Create the variables
            CodeDomCloner clone = new CodeDomCloner();
            string jumpLabelName = "PerformCall";
            CodeVariableDeclarationStatement bufVar = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(StringBuilder)), "var" + bufParam.Name);
            newMethod.Statements.Add(bufVar);
            CodeVariableDeclarationStatement retVar = new CodeVariableDeclarationStatement(clone.CloneTypeReference(origMethod.ReturnType), "retVar_");
            newMethod.Statements.Add(retVar);
            CodeVariableDeclarationStatement sizeVar = new CodeVariableDeclarationStatement(clone.CloneTypeReference(origMethod.ReturnType), "sizeVar", new CodePrimitiveExpression(TransformConstants.DefaultBufferSize));
            newMethod.Statements.Add(sizeVar);

            // Create the jump label
            newMethod.Statements.Add(new CodeLabeledStatement(jumpLabelName));

            // Create the buffer
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(bufVar.Name), new CodeObjectCreateExpression(new CodeTypeReference(typeof(StringBuilder)), CodeDomUtil.ReferenceVarAsType(sizeVar, typeof(Int32)))));

            // Perform the method call
            List<CodeExpression> args = new List<CodeExpression>();
            foreach (CodeParam origParam in origMethod.Parameters)
            {
                string variableName = null;
                if (object.ReferenceEquals(origParam, bufParam))
                {
                    variableName = bufVar.Name;
                }
                else if (object.ReferenceEquals(origParam, sizeParam))
                {
                    variableName = sizeVar.Name;
                }
                else
                {
                    variableName = origParam.Name;
                }

                args.Add(CodeDirectionalSymbolExpression.Create(this.LanguageType, new CodeVariableReferenceExpression(variableName), origParam.Direction));
            }

            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(retVar.Name), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(TransformConstants.NativeMethodsName), origMethod.Name), args.ToArray())));

            // Check the return of the call 
            CodeConditionStatement recallCheck = new CodeConditionStatement();
            recallCheck.Condition = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(retVar.Name), CodeBinaryOperatorType.GreaterThanOrEqual, new CodeVariableReferenceExpression(sizeVar.Name));

            // Assign the new buffer value 
            if (isPlusOne)
            {
                recallCheck.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(sizeVar.Name), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(retVar.Name), CodeBinaryOperatorType.Add, CodeDomUtil.CreatePrimitiveAsType(1, retVar.Type))));
            }
            else
            {
                recallCheck.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(sizeVar.Name), new CodeVariableReferenceExpression(retVar.Name)));
            }

            // Jump to the label
            recallCheck.TrueStatements.Add(new CodeGotoStatement(jumpLabelName));
            newMethod.Statements.Add(recallCheck);

            // Save the return value
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(bufParam.Name), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(bufVar.Name), "ToString"))));

            // Return the value
            newMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(retVar.Name)));
        }

    }

    #endregion

    #region "PInvokePointerTransformPlugin"

    internal class PInvokePointerTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.WrapperMethods; }
        }

        protected override System.CodeDom.CodeMemberMethod ProcessSingleWrapperMethod(System.CodeDom.CodeMemberMethod codeMethod)
        {
            CodeParam bufParam = null;
            CodeParam sizeParam = null;
            if (!FindParams(codeMethod, ref bufParam, ref sizeParam))
            {
                return null;
            }

            CodeMemberMethod newMethod = GenerateWrapperSignature(codeMethod, bufParam, sizeParam);
            GenerateWrapperCode(newMethod, codeMethod, bufParam, sizeParam);
            return newMethod;
        }

        private bool FindParams(CodeMemberMethod codeMethod, ref CodeParam bufParam, ref CodeParam sizeParam)
        {
            bufParam = null;
            sizeParam = null;

            foreach (CodeParam codeParam in codeMethod.Parameters)
            {
                NativeParameter ntParam = GetNativeParameter(codeParam);
                if (ntParam == null)
                {
                    continue;
                }

                NativeType ntType = ntParam.NativeTypeDigged;
                if (ntType.Kind != NativeSymbolKind.PointerType || !IsIntPtrType(codeParam.Type))
                {
                    continue;
                }

                SalAnalyzer analyzer = new SalAnalyzer(ntParam.SalAttribute);
                string str1 = null;
                string str2 = null;
                if (!analyzer.IsOutPartByteBuffer(out str1, out str2))
                {
                    continue;
                }

                // Look for the popular pattern
                //  -> __out_ecount_part(*size,*size)
                if (!str1.StartsWith("*") || 0 != string.CompareOrdinal(str1, str2))
                {
                    continue;
                }

                str1 = str1.Substring(1);

                // Now we just have to find the parameter
                foreach (CodeParameterDeclarationExpression codeParam2 in codeMethod.Parameters)
                {
                    if (0 == string.CompareOrdinal(codeParam2.Name, str1))
                    {
                        bufParam = codeParam;
                        sizeParam = codeParam2;
                        return true;
                    }
                }

            }

            return false;
        }

        private CodeMemberMethod GenerateWrapperSignature(CodeMemberMethod origMethod, CodeParam bufParam, CodeParam sizeParam)
        {
            CodeDomCloner clone = new CodeDomCloner();
            CodeMemberMethod newMethod = new CodeMemberMethod();
            newMethod.Name = origMethod.Name;
            newMethod.ReturnType = clone.CloneTypeReference(origMethod.ReturnType);
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateDebuggerStepThroughAttribute());
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateGeneratedCodeAttribute());
            newMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            foreach (CodeParam origParam in origMethod.Parameters)
            {
                if (object.ReferenceEquals(origParam, bufParam))
                {
                    CodeParam newParam = new CodeParam();
                    newParam.Name = origParam.Name;
                    newParam.Direction = FieldDirection.Out;
                    newParam.Type = MarshalTypeFactory.CreatePInvokePointerCodeTypeReference();
                    newMethod.Parameters.Add(newParam);
                }
                else if (object.ReferenceEquals(origParam, sizeParam))
                {
                    // Don't need the size param in the new signature
                }
                else
                {
                    newMethod.Parameters.Add(clone.CloneParamNoAttributes(origParam));
                }
            }

            return newMethod;
        }

        private void GenerateWrapperCode(CodeMemberMethod newMethod, CodeMemberMethod origmethod, CodeParam bufParam, CodeParam sizeParam)
        {
            // Generate the variables
            CodeDomCloner clone = new CodeDomCloner();
            string jumpLabelName = "PerformCall";
            CodeVariableDeclarationStatement bufVar = new CodeVariableDeclarationStatement(MarshalTypeFactory.CreatePInvokePointerCodeTypeReference(), "var" + bufParam.Name);
            newMethod.Statements.Add(bufVar);
            CodeVariableDeclarationStatement retVar = new CodeVariableDeclarationStatement(clone.CloneTypeReference(origmethod.ReturnType), "retVar_");
            newMethod.Statements.Add(retVar);
            CodeVariableDeclarationStatement sizeVar = new CodeVariableDeclarationStatement(clone.CloneTypeReference(sizeParam.Type), "sizeVar", new CodePrimitiveExpression(TransformConstants.DefaultBufferSize));
            newMethod.Statements.Add(sizeVar);
            CodeVariableDeclarationStatement oldSizeVar = new CodeVariableDeclarationStatement(clone.CloneTypeReference(sizeParam.Type), "oldSizeVar_");
            newMethod.Statements.Add(oldSizeVar);

            // Create the jump label
            newMethod.Statements.Add(new CodeLabeledStatement(jumpLabelName));

            // Save the old size
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(oldSizeVar.Name), new CodeVariableReferenceExpression(sizeVar.Name)));

            // Create the pointer
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(bufVar.Name), new CodeObjectCreateExpression(MarshalTypeFactory.CreatePInvokePointerCodeTypeReference(), CodeDomUtil.ReferenceVarAsType(sizeVar, typeof(Int32)))));

            // Perform the method call
            List<CodeExpression> args = new List<CodeExpression>();
            foreach (CodeParam origParam in origmethod.Parameters)
            {
                if (object.ReferenceEquals(origParam, bufParam))
                {
                    CodeFieldReferenceExpression memberRef = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(bufVar.Name), "IntPtr");
                    args.Add(CodeDirectionalSymbolExpression.Create(this.LanguageType, memberRef, origParam.Direction));
                }
                else if (object.ReferenceEquals(origParam, sizeParam))
                {
                    args.Add(CodeDirectionalSymbolExpression.Create(this.LanguageType, new CodeVariableReferenceExpression(sizeVar.Name), origParam.Direction));
                }
                else
                {
                    args.Add(CodeDirectionalSymbolExpression.Create(this.LanguageType, new CodeVariableReferenceExpression(origParam.Name), origParam.Direction));
                }
            }
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(retVar.Name), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(TransformConstants.NativeMethodsName), origmethod.Name), args.ToArray())));

            // Check the return of the call 
            CodeConditionStatement recallCheck = new CodeConditionStatement();
            recallCheck.Condition = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(sizeVar.Name), CodeBinaryOperatorType.LessThanOrEqual, new CodeVariableReferenceExpression(oldSizeVar.Name));

            // Free the buffer
            recallCheck.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(bufVar.Name), "Free"));

            // Double the size 
            recallCheck.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(sizeVar.Name), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(sizeVar.Name), CodeBinaryOperatorType.Multiply, CodeDomUtil.CreatePrimitiveAsType(2, sizeVar.Type))));

            // Jump to the label
            recallCheck.TrueStatements.Add(new CodeGotoStatement(jumpLabelName));
            newMethod.Statements.Add(recallCheck);

            // Save the pointer
            newMethod.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(bufParam.Name), new CodeVariableReferenceExpression(bufVar.Name)));

            // Return the value
            newMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(retVar.Name)));
        }

    }

    #endregion

    #endregion

    #region "Struct Members"

    #region "StringBufferStructMemberTransformPlugin"

    internal class StringBufferStructMemberTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.StructMembers; }
        }


        protected override void ProcessSingleStructField(System.CodeDom.CodeTypeDeclaration ctd, System.CodeDom.CodeMemberField codeField, NativeMember ntMem)
        {
            // Analyze the type and look for a string buffer
            CharSet foundCharSet = CharSet.None;
            if (!IsArrayOfCharType(ntMem.NativeType, ref foundCharSet))
            {
                return;
            }

            // Look at the existing charset.  If it's different than the one we found then we can't do anything
            // and should just bail out
            CharSet existingCharset = CharSet.Ansi;
            if (base.IsCharsetSpecified(ctd, ref existingCharset))
            {
                if (existingCharset != foundCharSet)
                {
                    return;
                }
            }
            else
            {
                AddCharSet(ctd, foundCharSet);
            }

            // Convert the types
            NativeArray arrayNt = (NativeArray)ntMem.NativeTypeDigged;
            codeField.Type = new CodeTypeReference(typeof(string));
            codeField.CustomAttributes.Clear();

            CodeAttributeDeclaration attr = new CodeAttributeDeclaration(new CodeTypeReference(typeof(MarshalAsAttribute)));
            attr.Arguments.Add(new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(UnmanagedType)), "ByValTStr")));
            attr.Arguments.Add(new CodeAttributeArgument("SizeConst", new CodePrimitiveExpression(arrayNt.ElementCount)));
            codeField.CustomAttributes.Add(attr);
            SetMemberProcessed(codeField);
        }
    }

    #endregion

    #region "StringPointerStructMemberTransformPlugin"

    /// <summary>
    /// If there is an IntPtr member of a structure to a String then marshal it as such
    /// </summary>
    /// <remarks></remarks>
    internal class StringPointerStructMemberTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.StructMembers; }
        }


        protected override void ProcessSingleStructField(System.CodeDom.CodeTypeDeclaration ctd, System.CodeDom.CodeMemberField field, NativeMember ntMem)
        {
            CharSet foundCharSet = default(CharSet);
            if (!IsPointerToCharType(ntMem, ref foundCharSet))
            {
                return;
            }

            field.Type = new CodeTypeReference(typeof(string));
            field.CustomAttributes.Clear();
            field.CustomAttributes.Add(CreateStringMarshalAttribute(foundCharSet));
            SetMemberProcessed(field);
        }

    }

    #endregion

    #region "BoolStructMemberTransformPlugin"

    /// <summary>
    /// Look for boolean types that are members of structures
    /// </summary>
    /// <remarks></remarks>
    internal class BoolStructMemberTransformPlugin : TransformPlugin
    {

        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.StructMembers; }
        }


        protected override void ProcessSingleStructField(System.CodeDom.CodeTypeDeclaration ctd, System.CodeDom.CodeMemberField field, NativeMember ntMem)
        {
            NativeType retNt = ntMem.NativeType;
            BooleanType bType = BooleanType.CStyle;
            if (retNt != null && IsBooleanType(retNt, ref bType))
            {
                field.Type = new CodeTypeReference(typeof(bool));
                field.CustomAttributes.Clear();
                field.CustomAttributes.Add(MarshalAttributeFactory.CreateBooleanMarshalAttribute(bType));
                SetMemberProcessed(field);
            }
        }
    }

    #endregion

    #endregion

    #region "Union Members"

    #region "BoolUnionMemberTransformPlugin"

    /// <summary>
    /// Look for boolean types that are members of a union
    /// </summary>
    /// <remarks></remarks>
    internal class BoolUnionMemberTransformPlugin : TransformPlugin
    {


        public override TransformKindFlags TransformKind
        {
            get { return TransformKindFlags.UnionMembers; }
        }

        protected override void ProcessSingleUnionField(System.CodeDom.CodeTypeDeclaration ctd, System.CodeDom.CodeMemberField field, NativeMember ntMem)
        {
            NativeType nt = ntMem.NativeType;
            BooleanType bType = BooleanType.CStyle;
            if (nt != null && IsBooleanType(nt, ref bType))
            {
                field.CustomAttributes.Add(MarshalAttributeFactory.CreateBooleanMarshalAttribute(BooleanType.CStyle));
                field.Type = new CodeTypeReference(typeof(bool));
                SetMemberProcessed(field);
            }
        }

    }

    #endregion

    #endregion

}
