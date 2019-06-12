// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;
using PInvoke.Parser;
using PInvoke.Parser.Enums;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static PInvoke.Contract;

namespace PInvoke.Transform
{

    public static class TransformConstants
    {
        internal const string Procedure = "Procedure";
        internal const string Param = "Parameter";
        internal const string Type = "Type";
        internal const string ReturnType = "ReturnType";
        internal const string ReturnTypeSal = "ReturnTypeSal";
        internal const string Member = "Member";

        internal const string DefinedType = "DefinedType";

        internal const int DefaultBufferSize = 2056;
        public const string NativeMethodsName = "NativeMethods";

        public const string NativeConstantsName = "NativeConstants";
    }

    /// <summary>
    /// Used to transform from NativeType instances to actual PInvokeable instances
    /// </summary>
    /// <remarks></remarks>
    public class CodeTransform
    {
        private LanguageType _lang;
        private NativeSymbolBag _bag;
        private Dictionary<string, NativeSymbol> _typeMap = new Dictionary<string, NativeSymbol>(StringComparer.Ordinal);
        private Dictionary<string, NativeSymbol> _symbolValueMap = new Dictionary<string, NativeSymbol>(StringComparer.Ordinal);

        public CodeTransform(LanguageType lang, NativeSymbolBag bag)
        {
            _lang = lang;
            _bag = bag;
        }

        /// <summary>
        /// Generate a type reference for the specified type
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeReference GenerateTypeReference(NativeType nt)
        {
            if (nt == null)
            {
                throw new ArgumentNullException("nt");
            }

            string comment = string.Empty;
            return GenerateTypeReferenceImpl(nt, ref comment);
        }

        /// <summary>
        /// Convert the defined type into a CodeTypeDeclaration
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeDeclaration GenerateDeclaration(NativeDefinedType nt)
        {
            if (nt == null)
            {
                throw new ArgumentNullException("nt");
            }

            CodeTypeDeclaration ctd = default(CodeTypeDeclaration);
            switch (nt.Kind)
            {
                case NativeSymbolKind.StructType:
                    ctd = GenerateStruct((NativeStruct)nt);
                    break;
                case NativeSymbolKind.UnionType:
                    ctd = GenerateUnion((NativeUnion)nt);
                    break;
                case NativeSymbolKind.EnumType:
                    ctd = GenerateEnum((NativeEnum)nt);
                    break;
                case NativeSymbolKind.FunctionPointer:
                    ctd = GenerateDelegate((NativeFunctionPointer)nt);
                    break;
                default:
                    Contract.ThrowInvalidEnumValue(nt.Kind);
                    return null;
            }

            ThrowIfFalse(ctd.UserData.Contains(TransformConstants.DefinedType));
            ctd.UserData[TransformConstants.Type] = nt;
            return ctd;
        }

        /// <summary>
        /// Generate the struct definition
        /// </summary>
        /// <param name="ntStruct"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeDeclaration GenerateStruct(NativeStruct ntStruct)
        {
            if (ntStruct == null)
            {
                throw new ArgumentNullException("ntStruct");
            }

            // Generate the core type
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(ntStruct.Name);
            ctd.IsStruct = true;
            ctd.UserData[TransformConstants.DefinedType] = ntStruct;

            // Add the struct layout attribute
            ctd.CustomAttributes.Add(MarshalAttributeFactory.CreateStructLayoutAttribute(LayoutKind.Sequential));

            GenerateContainerMembers(ntStruct, ctd);

            return ctd;
        }

        /// <summary>
        /// Generate the specified enumeration
        /// </summary>
        /// <param name="ntEnum"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeDeclaration GenerateEnum(NativeEnum ntEnum)
        {
            if (ntEnum == null)
            {
                throw new ArgumentNullException("ntEnum");
            }

            // Generate the type
            CodeTypeDeclaration ctd = new CodeTypeDeclaration();
            ctd.Name = ntEnum.Name;
            ctd.IsEnum = true;
            ctd.UserData[TransformConstants.DefinedType] = ntEnum;

            // Add the values
            foreach (NativeEnumValue enumValue in ntEnum.Values)
            {
                CodeMemberField member = new CodeMemberField();
                member.Name = enumValue.Name;

                if (enumValue.Value != null && !string.IsNullOrEmpty(enumValue.Value.Expression))
                {
                    GenerateInitExpression(member, enumValue, enumValue.Value);

                    // Regardless of what the generation code believes, we want the type of the member field
                    // to be an integer because this is a specific enum value.  
                    member.Type = new CodeTypeReference(typeof(Int32));
                }

                ctd.Members.Add(member);
            }

            return ctd;
        }

        /// <summary>
        /// Generate the specified union
        /// </summary>
        /// <param name="ntUnion"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeDeclaration GenerateUnion(NativeUnion ntUnion)
        {
            if (ntUnion == null)
            {
                throw new ArgumentNullException("ntUnion");
            }

            // Generate the core type
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(ntUnion.Name);
            ctd.IsStruct = true;
            ctd.UserData[TransformConstants.DefinedType] = ntUnion;

            // Add the struct layout attribute
            ctd.CustomAttributes.Add(MarshalAttributeFactory.CreateStructLayoutAttribute(LayoutKind.Explicit));

            // Generate the container members
            GenerateContainerMembers(ntUnion, ctd);

            // Go through and put each struct back at the start of the struct to simulate the 
            // union
            foreach (CodeTypeMember member in ctd.Members)
            {
                CodeMemberField fieldMember = member as CodeMemberField;
                if (fieldMember != null)
                {
                    fieldMember.CustomAttributes.Add(MarshalAttributeFactory.CreateFieldOffsetAttribute(0));
                }
            }

            return ctd;
        }

        /// <summary>
        /// Generate a delegate in code
        /// </summary>
        /// <param name="ntFuncPtr"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeDelegate GenerateDelegate(NativeFunctionPointer ntFuncPtr)
        {
            if (ntFuncPtr == null)
            {
                throw new ArgumentNullException("ntFuncPtr");
            }

            string comment = "Return Type: ";
            CodeTypeDelegate del = new CodeTypeDelegate();
            del.Name = ntFuncPtr.Name;
            del.Attributes = MemberAttributes.Public;
            del.ReturnType = GenerateTypeReferenceImpl(ntFuncPtr.Signature.ReturnType, ref comment);
            del.Parameters.AddRange(GenerateParameters(ntFuncPtr.Signature, ref comment));

            // If there is a non-default calling convention we need to generate the attribute
            if (ntFuncPtr.CallingConvention == NativeCallingConvention.CDeclaration || ntFuncPtr.CallingConvention == NativeCallingConvention.Standard)
            {
                del.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedFunctionPointerAttribute(ntFuncPtr.CallingConvention));
            }

            del.UserData[TransformConstants.DefinedType] = ntFuncPtr;
            del.UserData[TransformConstants.ReturnType] = ntFuncPtr.Signature.ReturnType;
            del.UserData[TransformConstants.ReturnTypeSal] = ntFuncPtr.Signature.ReturnTypeSalAttribute;
            del.Comments.Add(new CodeCommentStatement(comment, true));

            return del;
        }

        /// <summary>
        /// Generate the procedures into a type
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeDeclaration GenerateProcedures(IEnumerable<NativeProcedure> enumerable, string libraryName)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            CodeTypeDeclaration ctd = new CodeTypeDeclaration();
            ctd.Name = TransformConstants.NativeMethodsName;
            ctd.Attributes = MemberAttributes.Public;
            ctd.IsPartial = true;

            foreach (NativeProcedure proc in enumerable)
            {
                ctd.Members.Add(GenerateProcedure(proc, libraryName));
            }

            return ctd;
        }

        /// <summary>
        /// Generate a procedure from the specified proc
        /// </summary>
        /// <param name="ntProc"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeMemberMethod GenerateProcedure(NativeProcedure ntProc, string libraryName)
        {
            if (ntProc == null)
            {
                throw new ArgumentNullException("ntProc");
            }

            // Create the proc
            var ntSig = ntProc.Signature;
            string procComment = "Return Type: ";
            var proc = new CodeMemberMethod
            {
                Name = ntProc.Name,
                ReturnType = GenerateTypeReferenceImpl(ntSig.ReturnType, ref procComment)
            };
            proc.UserData[TransformConstants.ReturnType] = ntSig.ReturnType;
            if (ntSig.ReturnTypeSalAttribute != null)
            {
                proc.UserData[TransformConstants.ReturnTypeSal] = ntSig.ReturnTypeSalAttribute;
            }
            else
            {
                proc.UserData[TransformConstants.ReturnTypeSal] = new NativeSalAttribute();
            }
            proc.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            proc.UserData[TransformConstants.Procedure] = ntProc;
            proc.UserData[TransformConstants.ReturnType] = ntSig.ReturnType;
            proc.UserData[TransformConstants.ReturnTypeSal] = ntSig.ReturnTypeSalAttribute;

            // Add the DLL import attribute
            string dllName = ntProc.DllName ?? libraryName;
            if (string.IsNullOrEmpty(dllName))
            {
                dllName = "<Unknown>";
            }
            proc.CustomAttributes.Add(MarshalAttributeFactory.CreateDllImportAttribute(dllName, ntProc.Name, ntProc.CallingConvention));

            // Generate the parameters
            proc.Parameters.AddRange(GenerateParameters(ntProc.Signature, ref procComment));
            proc.Comments.Add(new CodeCommentStatement(procComment, true));
            return proc;
        }

        private CodeParameterDeclarationExpressionCollection GenerateParameters(NativeSignature ntSig, ref string comments)
        {
            ThrowIfNull(ntSig);
            if (comments == null)
            {
                comments = string.Empty;
            }

            CodeParameterDeclarationExpressionCollection col = new CodeParameterDeclarationExpressionCollection();
            Int32 count = 0;
            foreach (NativeParameter ntParam in ntSig.Parameters)
            {
                string comment = null;
                CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression();
                param.Name = ntParam.Name;
                param.Type = GenerateTypeReferenceImpl(ntParam.NativeType, ref comment);
                param.UserData[TransformConstants.Param] = ntParam;
                col.Add(param);

                if (string.IsNullOrEmpty(param.Name))
                {
                    param.Name = "param" + count;
                }

                // Add the type comment to the procedure
                comments += Environment.NewLine;
                comments += param.Name + ": " + comment;
                count += 1;
            }

            return col;
        }

        /// <summary>
        /// Generate the macros as constants into a type
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public CodeTypeDeclaration GenerateConstants(IEnumerable<NativeConstant> enumerable)
        {
            CodeTypeDeclaration ctd = new CodeTypeDeclaration();
            ctd.Name = TransformConstants.NativeConstantsName;
            ctd.IsPartial = true;
            ctd.IsClass = true;

            GenerateConstants(ctd, enumerable);

            return ctd;
        }

        /// <summary>
        /// Generate the macros as constants into the specified type declaration.  This will only utilize simple 
        /// macros such as numbers and quoted string.
        /// </summary>
        /// <param name="ctd"></param>
        /// <param name="enumerable"></param>
        /// <remarks></remarks>
        public void GenerateConstants(CodeTypeDeclaration ctd, IEnumerable<NativeConstant> enumerable)
        {
            if (ctd == null)
            {
                throw new ArgumentNullException("ctd");
            }

            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            foreach (NativeConstant nConst in enumerable)
            {
                // Build up the attributes and value
                CodeMemberField cMember = new CodeMemberField();
                cMember.Name = nConst.Name;
                cMember.Attributes = MemberAttributes.Public | MemberAttributes.Const;
                ctd.Members.Add(cMember);

                if (ConstantKind.MacroMethod == nConst.ConstantKind)
                {
                    // Generation of macro methods is not supported entirely.  Right now macro methods
                    // expressions are stored as text and they are outputted as a string.  Offer an explanation
                    // here
                    cMember.Comments.Add(new CodeCommentStatement("Warning: Generation of Method Macros is not supported at this time", true));
                }

                // Set the init expression
                GenerateInitExpression(cMember, nConst, nConst.Value);
            }
        }


        /// <summary>
        /// Generate the members of the container
        /// </summary>
        /// <param name="nt"></param>
        /// <param name="ctd"></param>
        /// <remarks></remarks>
        private void GenerateContainerMembers(NativeDefinedType nt, CodeTypeDeclaration ctd)
        {
            ThrowIfNull(nt);
            ThrowIfNull(ctd);

            int bitVectorCount = 0;
            for (int i = 0; i <= nt.Members.Count - 1; i++)
            {
                NativeMember member = nt.Members[i];

                // Don't process unnamed container members
                if (string.IsNullOrEmpty(member.Name))
                {
                    continue;
                }


                if (IsBitVector(member.NativeType))
                {
                    // Get the list of bitvectors that will fit into the next int
                    int bitCount = 0;
                    List<NativeMember> list = new List<NativeMember>();
                    NativeBitVector bitVector = null;

                    while ((i < nt.Members.Count && IsBitVector(nt.Members[i].NativeType, ref bitVector) && bitCount + bitVector.Size <= 32))
                    {
                        list.Add(nt.Members[i]);
                        i += 1;
                    }
                    i -= 1;

                    // Generate the int for the list of bit vectors
                    bitVectorCount += 1;

                    CodeMemberField cMember = GenerateContainerMember(new NativeMember("bitvector" + bitVectorCount, new NativeBuiltinType(BuiltinType.NativeInt32, true)), ctd);
                    cMember.Comments.Clear();

                    CodeComment comment = new CodeComment(string.Empty, true);
                    int offset = 0;
                    for (int j = 0; j <= list.Count - 1; j++)
                    {
                        if (j > 0)
                        {
                            comment.Text += Environment.NewLine;
                        }

                        IsBitVector(list[j].NativeType, ref bitVector);
                        comment.Text += list[j].Name + " : " + bitVector.Size;
                        GenerateBitVectorProperty(list[j], offset, ctd, cMember);
                        offset += bitVector.Size;
                    }
                    cMember.Comments.Add(new CodeCommentStatement(comment));
                }
                else
                {
                    GenerateContainerMember(member, ctd);
                }
            }
        }

        private bool IsBitVector(NativeType nt)
        {
            NativeBitVector bt = null;
            return IsBitVector(nt, ref bt);
        }

        private bool IsBitVector(NativeType nt, ref NativeBitVector bitvector)
        {
            ThrowIfNull(nt);

            nt = nt.DigThroughTypeDefAndNamedTypes();

            if (nt != null && nt.Kind == NativeSymbolKind.BitVectorType)
            {
                bitvector = (NativeBitVector)nt;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generate a property to wrap the underlying bit vector
        /// </summary>
        /// <param name="ntMember"></param>
        /// <param name="offset"></param>
        /// <param name="ctd"></param>
        /// <param name="codeMember"></param>
        /// <remarks></remarks>
        private void GenerateBitVectorProperty(NativeMember ntMember, int offset, CodeTypeDeclaration ctd, CodeMemberField codeMember)
        {
            ThrowIfNull(ntMember);
            ThrowIfNull(ctd);
            ThrowIfNull(codeMember);
            ThrowIfTrue(offset < 0);

            NativeBitVector bitVector = null;
            IsBitVector(ntMember.NativeType, ref bitVector);

            // First calculate the bitmask
            uint mask = 0;
            for (int i = 0; i <= bitVector.Size - 1; i++)
            {
                mask <<= 1;
                mask = mask | 1u;
            }
            mask <<= offset;

            // Create the property
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Name = ntMember.Name;
            prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            prop.Type = new CodeTypeReference(typeof(uint));
            ctd.Members.Add(prop);

            // Build the get and set
            GenerateBitVectorPropertyGet(prop, codeMember.Name, mask, offset, bitVector);
            GenerateBitVectorPropertySet(prop, codeMember.Name, mask, offset, bitVector);

        }

        private void GenerateBitVectorPropertyGet(CodeMemberProperty prop, string fieldName, uint mask, int offset, NativeBitVector bitVector)
        {
            prop.HasGet = true;

            // Get the value from the mask
            CodeBinaryOperatorExpression exprGet = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), CodeBinaryOperatorType.BitwiseAnd, new CodePrimitiveExpression(mask));

            // Shift the result down
            CodeBinaryOperatorExpression exprShift = new CodeBinaryOperatorExpression(exprGet, CodeBinaryOperatorType.Divide, new CodePrimitiveExpression(Math.Pow(2, offset)));

            // If the offset is 0 then don't do the shift
            CodeExpression outerExpr = default(CodeExpression);
            if (0 == offset)
            {
                outerExpr = exprGet;
            }
            else
            {
                outerExpr = exprShift;
            }

            // Cast it back to an integer since we are now at a UInteger and the property is Integer 
            CodeMethodReturnStatement retStmt = new CodeMethodReturnStatement();
            retStmt.Expression = new CodeCastExpression(new CodeTypeReference(typeof(uint)), outerExpr);

            prop.GetStatements.Add(retStmt);
        }

        private void GenerateBitVectorPropertySet(CodeMemberProperty prop, string fieldName, uint mask, int offset, NativeBitVector bitVector)
        {
            prop.HasSet = true;

            // Shift it
            CodeExpression exprShift = default(CodeExpression);
            if (offset != 0)
            {
                exprShift = new CodeBinaryOperatorExpression(new CodePropertySetValueReferenceExpression(), CodeBinaryOperatorType.Multiply, new CodePrimitiveExpression(Math.Pow(2, offset)));
            }
            else
            {
                exprShift = new CodePropertySetValueReferenceExpression();
            }

            // Or it with the current
            CodeBinaryOperatorExpression exprOr = new CodeBinaryOperatorExpression(exprShift, CodeBinaryOperatorType.BitwiseOr, new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName));

            // Assign it to the field
            CodeAssignStatement asg = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeCastExpression(new CodeTypeReference(typeof(uint)), exprOr));
            prop.SetStatements.Add(asg);
        }

        /// <summary>
        /// Generate the NativeMember
        /// </summary>
        /// <param name="nt"></param>
        /// <param name="ctd"></param>
        /// <remarks></remarks>
        private CodeMemberField GenerateContainerMember(NativeMember nt, CodeTypeDeclaration ctd)
        {
            ThrowIfNull(nt);
            ThrowIfNull(ctd);
            ThrowIfTrue(nt.NativeType.Kind == NativeSymbolKind.BitVectorType);
            // Bitvector instances should be handled seperately

            // Generate the type reference and comment
            string comment = string.Empty;
            CodeMemberField member = new CodeMemberField();
            member.Name = nt.Name;
            member.Type = GenerateTypeReferenceImpl(nt.NativeType, ref comment);
            member.Attributes = MemberAttributes.Public;
            member.Comments.Add(new CodeCommentStatement(comment, true));
            member.UserData.Add(TransformConstants.Member, nt);
            ctd.Members.Add(member);

            // If this is an array then add the appropriate marshal directive if it's an inline array
            NativeArray ntArray = nt.NativeType as NativeArray;
            if (ntArray != null && ntArray.ElementCount > 0)
            {
                // Add the struct layout attribute
                CodeTypeReference attrRef = new CodeTypeReference(typeof(MarshalAsAttribute));
                CodeAttributeDeclaration attr = new CodeAttributeDeclaration(attrRef);

                // ByValArray
                CodeAttributeArgument asArg = new CodeAttributeArgument();
                asArg.Name = string.Empty;
                asArg.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(UnmanagedType)), "ByValArray");
                attr.Arguments.Add(asArg);

                // SizeConst arg
                CodeAttributeArgument sizeArg = new CodeAttributeArgument();
                sizeArg.Name = "SizeConst";
                sizeArg.Value = new CodePrimitiveExpression(ntArray.ElementCount);
                attr.Arguments.Add(sizeArg);

                // ArraySubType
                NativeType elemType = ntArray.RealTypeDigged;
                CodeAttributeArgument subTypeArg = new CodeAttributeArgument();
                subTypeArg.Name = "ArraySubType";
                if (elemType.Kind == NativeSymbolKind.BuiltinType)
                {
                    // Builtin types know their size in bytes
                    NativeBuiltinType elemBt = (NativeBuiltinType)elemType;
                    subTypeArg.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(UnmanagedType)), elemBt.UnmanagedType.ToString());

                }
                else if (elemType.Kind == NativeSymbolKind.PointerType || elemType.Kind == NativeSymbolKind.ArrayType)
                {
                    // Marshal pointers as system ints
                    subTypeArg.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(UnmanagedType)), "SysUInt");
                }
                else
                {
                    subTypeArg.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(UnmanagedType)), "Struct");

                }
                attr.Arguments.Add(subTypeArg);

                member.CustomAttributes.Add(attr);
            }

            return member;
        }

        /// <summary>
        /// Convert a NativeValueExpression into managed code and make it the initialization expression of
        /// the passed in member.
        /// 
        /// If the code is unable to generate a valid expression for the member it will make the expression
        /// a stringized version of the original native expression.  It will add information in the comments
        /// about why it could not properly generate the expression.  Lastly it will generate incompatible types
        /// to force a compile error
        /// </summary>
        /// <param name="member"></param>
        /// <param name="ntExpr"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool GenerateInitExpression(CodeMemberField member, NativeSymbol target, NativeValueExpression ntExpr)
        {
            if (ntExpr == null)
            {
                member.Comments.Add(new CodeCommentStatement("Error: No value expression", true));
                member.InitExpression = new CodePrimitiveExpression(string.Empty);
                member.Type = new CodeTypeReference(typeof(int));
                return false;
            }

            member.Comments.Add(new CodeCommentStatement(string.Format("{0} -> {1}", member.Name, ntExpr.Expression), true));

            // It's not legal for a symbol to be used as part of it's initialization expression in most languages.  
            // There for we need to mark it as the initialization member so the generated will output NULL in it's place
            _symbolValueMap.Add(target.Name, target);
            try
            {
                Exception ex = null;
                CodeExpression expr;
                CodeTypeReference typeRef;
                if (TryGenerateValueExpression(ntExpr, out expr, out typeRef, out ex))
                {
                    member.InitExpression = expr;
                    member.Type = typeRef;
                    return true;
                }
                else
                {
                    member.Comments.Add(new CodeCommentStatement(string.Format("Error generating expression: {0}", ex.Message), true));
                    member.InitExpression = new CodePrimitiveExpression(ntExpr.Expression);
                    member.Type = new CodeTypeReference(typeof(string));
                    return false;
                }
            }
            finally
            {
                _symbolValueMap.Remove(target.Name);
            }
        }

        public bool TryGenerateValueExpression(NativeValueExpression ntExpr, out CodeExpression expr, out CodeTypeReference exprType, out Exception ex)
        {
            try
            {
                if (!ntExpr.IsParsable)
                {
                    string msg = "Expression is not parsable.  Treating value as a raw string";
                    throw new InvalidOperationException(msg);
                }

                exprType = null;
                expr = GenerateValueExpressionImpl(ntExpr.Node, ref exprType);
                ex = null;
                return true;
            }
            catch (Exception ex2)
            {
                ex = ex2;
                expr = null;
                exprType = null;
                return false;
            }

        }

        #region "CodeTypeReference Generation"

        private CodeTypeReference GenerateTypeReferenceImpl(NativeType nt, ref string comment)
        {
            ThrowIfNull(nt);

            switch (nt.Category)
            {
                case NativeSymbolCategory.Defined:
                    return GenerateDefinedTypeReferenceImpl((NativeDefinedType)nt, ref comment);
                case NativeSymbolCategory.Proxy:
                    return GenerateProxyTypeReferenceImpl((NativeProxyType)nt, ref comment);
                case NativeSymbolCategory.Specialized:
                    return GenerateSpecializedTypeReferenceImpl((NativeSpecializedType)nt, ref comment);
            }

            string errorMsg = string.Format("Error generating reference to {0}", nt.DisplayName);
            throw new InvalidOperationException(errorMsg);
        }

        public CodeTypeReference GenerateDefinedTypeReferenceImpl(NativeDefinedType definedNt, ref string comment)
        {
            ThrowIfNull(definedNt);

            comment += definedNt.Name;
            return new CodeTypeReference(definedNt.Name);
        }

        public CodeTypeReference GenerateProxyTypeReferenceImpl(NativeProxyType proxyNt, ref string comment)
        {

            // Check the various proxy types
            if (proxyNt.RealType == null)
            {
                string msg = string.Format("Could not find the real type for {0}", proxyNt.DisplayName);
                throw new InvalidOperationException(msg);
            }

            switch (proxyNt.Kind)
            {
                case NativeSymbolKind.ArrayType:
                    comment += proxyNt.DisplayName;
                    NativeArray arrayNt = (NativeArray)proxyNt;
                    CodeTypeReference elemRef = GenerateTypeReference(arrayNt.RealType);
                    CodeTypeReference arrayRef = new CodeTypeReference(elemRef, 1);
                    return arrayRef;
                case NativeSymbolKind.PointerType:
                    comment += proxyNt.DisplayName;
                    NativePointer pointerNt = (NativePointer)proxyNt;
                    return new CodeTypeReference(typeof(IntPtr));
                case NativeSymbolKind.TypeDefType:
                    NativeTypeDef td = (NativeTypeDef)proxyNt;
                    comment += td.Name + "->";
                    return GenerateTypeReferenceImpl(td.RealType, ref comment);
                case NativeSymbolKind.NamedType:
                    // Don't update the comment for named types.  Otherwise you get lots of 
                    // comments like DWORD->DWORD->unsigned long
                    NativeNamedType namedNt = (NativeNamedType)proxyNt;
                    return GenerateTypeReferenceImpl(namedNt.RealType, ref comment);
                default:
                    Contract.ThrowInvalidEnumValue(proxyNt.Kind);
                    return null;
            }
        }

        public CodeTypeReference GenerateSpecializedTypeReferenceImpl(NativeSpecializedType specialNt, ref string comment)
        {
            ThrowIfNull(specialNt);

            switch (specialNt.Kind)
            {
                case NativeSymbolKind.BitVectorType:
                    NativeBitVector bitNt = (NativeBitVector)specialNt;
                    comment = string.Format("bitvector : {0}", bitNt.Size);
                    return new CodeTypeReference(GetManagedNameForBitVector(bitNt));
                case NativeSymbolKind.BuiltinType:
                    NativeBuiltinType builtNt = (NativeBuiltinType)specialNt;
                    Type realType = builtNt.ManagedType;
                    comment += builtNt.DisplayName;
                    return new CodeTypeReference(realType);
                default:
                    Contract.ThrowInvalidEnumValue(specialNt.Kind);
                    return null;
            }
        }

        #endregion

        #region "CodeExpression Generation"

        private CodeExpression GenerateValueExpressionImpl(ExpressionNode node, ref CodeTypeReference type)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            switch (node.Kind)
            {
                case ExpressionKind.FunctionCall:
                    throw new InvalidOperationException("Error generating function call.  Operation not implemented");
                case ExpressionKind.BinaryOperation:
                    return GenerateValueExpressionBinaryOperation(node, ref type);
                case ExpressionKind.NegationOperation:
                    return GenerateValueExpressionNegation(node, ref type);
                case ExpressionKind.NegativeOperation:
                    return GenerateValueExpressionNegative(node, ref type);
                case ExpressionKind.Leaf:
                    return GenerateValueExpressionLeaf(node, ref type);
                case ExpressionKind.Cast:
                    return GenerateValueExpressionCast(node, ref type);
                default:
                    ThrowInvalidEnumValue(node.Kind);
                    return null;
            }
        }

        private CodeExpression GenerateValueExpressionNegative(ExpressionNode node, ref CodeTypeReference exprType)
        {
            ThrowIfNull(node);

            CodeExpression left = this.GenerateValueExpressionImpl(node.LeftNode, ref exprType);
            return new CodeNegativeExpression(_lang, left);
        }

        private CodeExpression GenerateValueExpressionNegation(ExpressionNode node, ref CodeTypeReference exprType)
        {
            ThrowIfNull(node);

            CodeExpression left = this.GenerateValueExpressionImpl(node.LeftNode, ref exprType);
            return new CodeNotExpression(_lang, left);
        }

        private CodeExpression GenerateValueExpressionBinaryOperation(ExpressionNode node, ref CodeTypeReference exprType)
        {
            ThrowIfNull(node);

            if (node.LeftNode == null || node.RightNode == null)
            {
                throw new InvalidOperationException("Error generating operation");
            }

            if (node.Token.TokenType == TokenType.OpShiftLeft || node.Token.TokenType == TokenType.OpShiftRight)
            {
                // Shift operations are not native supported by the CodeDom so we need to create a special CodeDom node here
                return GenerateValueExpressionShift(node, ref exprType);

            }

            CodeBinaryOperatorType type = default(CodeBinaryOperatorType);

            switch (node.Token.TokenType)
            {
                case TokenType.OpBoolAnd:
                    type = CodeBinaryOperatorType.BooleanAnd;
                    break;
                case TokenType.OpBoolOr:
                    type = CodeBinaryOperatorType.BooleanOr;
                    break;
                case TokenType.OpDivide:
                    type = CodeBinaryOperatorType.Divide;
                    break;
                case TokenType.OpGreaterThan:
                    type = CodeBinaryOperatorType.GreaterThan;
                    break;
                case TokenType.OpGreaterThanOrEqual:
                    type = CodeBinaryOperatorType.GreaterThanOrEqual;
                    break;
                case TokenType.OpLessThan:
                    type = CodeBinaryOperatorType.LessThan;
                    break;
                case TokenType.OpLessThanOrEqual:
                    type = CodeBinaryOperatorType.LessThanOrEqual;
                    break;
                case TokenType.OpMinus:
                    type = CodeBinaryOperatorType.Subtract;
                    break;
                case TokenType.OpModulus:
                    type = CodeBinaryOperatorType.Modulus;
                    break;
                case TokenType.OpPlus:
                    type = CodeBinaryOperatorType.Add;
                    break;
                case TokenType.Asterisk:
                    type = CodeBinaryOperatorType.Multiply;
                    break;
                case TokenType.Pipe:
                    type = CodeBinaryOperatorType.BitwiseOr;
                    break;
                case TokenType.Ampersand:
                    type = CodeBinaryOperatorType.BitwiseAnd;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported operation");
            }

            CodeTypeReference leftType = null;
            CodeTypeReference rightType = null;
            CodeExpression expr = new CodeBinaryOperatorExpression(GenerateValueExpressionImpl(node.LeftNode, ref leftType), type, GenerateValueExpressionImpl(node.RightNode, ref rightType));
            exprType = leftType;
            return expr;
        }

        private CodeExpression GenerateValueExpressionShift(ExpressionNode node, ref CodeTypeReference exprType)
        {

            bool isLeft = false;
            switch (node.Token.TokenType)
            {
                case TokenType.OpShiftLeft:
                    isLeft = true;
                    break;
                case TokenType.OpShiftRight:
                    isLeft = false;
                    break;
                default:
                    ThrowInvalidEnumValue(node.Token.TokenType);
                    return null;
            }

            CodeTypeReference leftType = null;
            CodeTypeReference rightType = null;
            CodeExpression expr = new CodeShiftExpression(this._lang, isLeft, GenerateValueExpressionImpl(node.LeftNode, ref leftType), GenerateValueExpressionImpl(node.RightNode, ref rightType));
            exprType = leftType;
            return expr;
        }

        private CodeExpression GenerateValueExpressionLeaf(ExpressionNode node, ref CodeTypeReference leafType)
        {
            ThrowIfNull(node);

            var ntVal = NativeValue.TryCreateForLeaf(node, _bag);
            if (ntVal == null)
            {
                throw new InvalidOperationException("Expected a NativeValue");
            }

            if (!ntVal.IsValueResolved)
            {
                throw new InvalidOperationException(string.Format("Value {0} is not resolved", ntVal.Name));
            }

            switch (ntVal.ValueKind)
            {
                case NativeValueKind.Number:
                    leafType = new CodeTypeReference(ntVal.Value.GetType());
                    return new CodePrimitiveExpression(ntVal.Value);
                case NativeValueKind.Boolean:
                    leafType = new CodeTypeReference(typeof(bool));
                    return new CodePrimitiveExpression(ntVal.Value);
                case NativeValueKind.String:
                    leafType = new CodeTypeReference(typeof(string));
                    return new CodePrimitiveExpression(ntVal.Value);
                case NativeValueKind.Character:
                    leafType = new CodeTypeReference(typeof(char));
                    return new CodePrimitiveExpression(ntVal.Value);
                case NativeValueKind.SymbolValue:
                    NativeSymbol ns = ntVal.SymbolValue;

                    // Prevent the generation of a circular reference
                    if (_symbolValueMap.ContainsKey(ns.Name))
                    {
                        leafType = new CodeTypeReference(typeof(object));
                        return new CodePrimitiveExpression(null);
                    }

                    switch (ns.Kind)
                    {
                        case NativeSymbolKind.Constant:
                            leafType = CalculateConstantType((NativeConstant)ns);
                            return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(TransformConstants.NativeConstantsName), ns.Name);
                        case NativeSymbolKind.EnumType:
                            leafType = this.GenerateTypeReference((NativeEnum)ns);
                            return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(ns.Name), ntVal.Name);
                        default:
                            throw new InvalidOperationException(string.Format("Generation of {0} not supported as a value", ns.Kind));
                    }
                case NativeValueKind.SymbolType:
                    throw new InvalidOperationException("Types are not supported as leaf nodes");
                default:
                    ThrowInvalidEnumValue(ntVal.ValueKind);
                    return null;
            }
        }

        private CodeExpression GenerateValueExpressionCast(ExpressionNode node, ref CodeTypeReference exprType)
        {
            throw new InvalidOperationException("Cast expressions are not supported in constants");
        }

        /// <summary>
        /// Calculate the type of the specified constant.  It's possible for a C++ constant to refer 
        /// to itself which is strange but legal.  Imagine the following.
        /// 
        /// #define A A
        /// 
        /// In this case the type is indermenistic so we choose Object.  To detect this we record the 
        /// objects we are currently evaluating for types and whenever we recursively hit one, return
        /// Object.  This prevents a mutually exclusive scenario
        /// </summary>
        /// <param name="nConst"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private CodeTypeReference CalculateConstantType(NativeConstant nConst)
        {
            if (nConst.Value == null)
            {
                return new CodeTypeReference(typeof(int));
            }
            else if (_typeMap.ContainsKey(nConst.Name))
            {
                return new CodeTypeReference(typeof(object));
            }

            _typeMap.Add(nConst.Name, nConst);
            try
            {
                CodeExpression codeExpr = null;
                CodeTypeReference codeType = null;
                Exception ex = null;
                if (!TryGenerateValueExpression(nConst.Value, out codeExpr, out codeType, out ex))
                {
                    codeType = new CodeTypeReference(typeof(int));
                }

                return codeType;
            }
            finally
            {
                _typeMap.Remove(nConst.Name);
            }
        }

        #endregion

        private string GetManagedNameForBitVector(NativeBitVector bitNt)
        {
            ThrowIfNull(bitNt);
            return string.Format("BitVector_Size_{0}", bitNt.Size);
        }

    }

}
