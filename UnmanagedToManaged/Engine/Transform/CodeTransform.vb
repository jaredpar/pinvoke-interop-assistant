' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic
Imports System.CodeDom
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports Pinvoke.Parser

Namespace Transform

    Public Module TransformConstants
        Friend Const Procedure As String = "Procedure"
        Friend Const Param As String = "Parameter"
        Friend Const Type As String = "Type"
        Friend Const ReturnType As String = "ReturnType"
        Friend Const ReturnTypeSal As String = "ReturnTypeSal"
        Friend Const Member As String = "Member"
        Friend Const DefinedType As String = "DefinedType"

        Friend Const DefaultBufferSize As Integer = 2056

        Public Const NativeMethodsName As String = "NativeMethods"
        Public Const NativeConstantsName As String = "NativeConstants"

    End Module

    ''' <summary>
    ''' Used to transform from NativeType instances to actual PInvokeable instances
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CodeTransform

        Private m_lang As LanguageType
        Private m_typeMap As New Dictionary(Of String, NativeSymbol)(StringComparer.Ordinal)
        Private m_symbolValueMap As New Dictionary(Of String, NativeSymbol)(StringComparer.Ordinal)

        Public Sub New(ByVal lang As LanguageType)
            m_lang = lang
        End Sub

        ''' <summary>
        ''' Generate a type reference for the specified type
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateTypeReference(ByVal nt As NativeType) As CodeTypeReference
            If nt Is Nothing Then : Throw New ArgumentNullException("nt") : End If

            Dim comment As String = String.Empty
            Return GenerateTypeReferenceImpl(nt, comment)
        End Function

        ''' <summary>
        ''' Convert the defined type into a CodeTypeDeclaration
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateDeclaration(ByVal nt As NativeDefinedType) As CodeTypeDeclaration
            If nt Is Nothing Then
                Throw New ArgumentNullException("nt")
            End If

            Dim ctd As CodeTypeDeclaration
            Select Case nt.Kind
                Case NativeSymbolKind.StructType
                    ctd = GenerateStruct(DirectCast(nt, NativeStruct))
                Case NativeSymbolKind.UnionType
                    ctd = GenerateUnion(DirectCast(nt, NativeUnion))
                Case NativeSymbolKind.EnumType
                    ctd = GenerateEnum(DirectCast(nt, NativeEnum))
                Case NativeSymbolKind.FunctionPointer
                    ctd = GenerateDelegate(DirectCast(nt, NativeFunctionPointer))
                Case Else
                    Contract.InvalidEnumValue(nt.Kind)
                    Return Nothing
            End Select

            ThrowIfFalse(ctd.UserData.Contains(TransformConstants.DefinedType))
            ctd.UserData(TransformConstants.Type) = nt
            Return ctd
        End Function

        ''' <summary>
        ''' Generate the struct definition
        ''' </summary>
        ''' <param name="ntStruct"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateStruct(ByVal ntStruct As NativeStruct) As CodeTypeDeclaration
            If ntStruct Is Nothing Then
                Throw New ArgumentNullException("ntStruct")
            End If

            ' Generate the core type
            Dim ctd As New CodeTypeDeclaration(ntStruct.Name)
            ctd.IsStruct = True
            ctd.UserData(TransformConstants.DefinedType) = ntStruct

            ' Add the struct layout attribute
            ctd.CustomAttributes.Add(MarshalAttributeFactory.CreateStructLayoutAttribute(LayoutKind.Sequential))

            GenerateContainerMembers(ntStruct, ctd)

            Return ctd
        End Function

        ''' <summary>
        ''' Generate the specified enumeration
        ''' </summary>
        ''' <param name="ntEnum"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateEnum(ByVal ntEnum As NativeEnum) As CodeTypeDeclaration
            If ntEnum Is Nothing Then
                Throw New ArgumentNullException("ntEnum")
            End If

            ' Generate the type
            Dim ctd As New CodeTypeDeclaration()
            ctd.Name = ntEnum.Name
            ctd.IsEnum = True
            ctd.UserData(TransformConstants.DefinedType) = ntEnum

            ' Add the values
            For Each enumValue As NativeEnumValue In ntEnum.Values
                Dim member As New CodeMemberField()
                member.Name = enumValue.Name

                If enumValue.Value IsNot Nothing AndAlso Not String.IsNullOrEmpty(enumValue.Value.Expression) Then
                    GenerateInitExpression(member, enumValue, enumValue.Value)

                    ' Regardless of what the generation code believes, we want the type of the member field
                    ' to be an integer because this is a specific enum value.  
                    member.Type = New CodeTypeReference(GetType(Int32))
                End If

                ctd.Members.Add(member)
            Next

            Return ctd
        End Function

        ''' <summary>
        ''' Generate the specified union
        ''' </summary>
        ''' <param name="ntUnion"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateUnion(ByVal ntUnion As NativeUnion) As CodeTypeDeclaration
            If ntUnion Is Nothing Then
                Throw New ArgumentNullException("ntUnion")
            End If

            ' Generate the core type
            Dim ctd As New CodeTypeDeclaration(ntUnion.Name)
            ctd.IsStruct = True
            ctd.UserData(TransformConstants.DefinedType) = ntUnion

            ' Add the struct layout attribute
            ctd.CustomAttributes.Add(MarshalAttributeFactory.CreateStructLayoutAttribute(LayoutKind.Explicit))

            ' Generate the container members
            GenerateContainerMembers(ntUnion, ctd)

            ' Go through and put each struct back at the start of the struct to simulate the 
            ' union
            For Each member As CodeTypeMember In ctd.Members
                Dim fieldMember As CodeMemberField = TryCast(member, CodeMemberField)
                If fieldMember IsNot Nothing Then
                    fieldMember.CustomAttributes.Add(
                        MarshalAttributeFactory.CreateFieldOffsetAttribute(0))
                End If
            Next

            Return ctd
        End Function

        ''' <summary>
        ''' Generate a delegate in code
        ''' </summary>
        ''' <param name="ntFuncPtr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateDelegate(ByVal ntFuncPtr As NativeFunctionPointer) As CodeTypeDelegate
            If ntFuncPtr Is Nothing Then : Throw New ArgumentNullException("ntFuncPtr") : End If

            Dim comment As String = "Return Type: "
            Dim del As New CodeTypeDelegate()
            del.Name = ntFuncPtr.Name
            del.Attributes = MemberAttributes.Public
            del.ReturnType = GenerateTypeReferenceImpl(ntFuncPtr.Signature.ReturnType, comment)
            del.Parameters.AddRange(GenerateParameters(ntFuncPtr.Signature, comment))

            ' If there is a non-default calling convention we need to generate the attribute
            If ntFuncPtr.CallingConvention = NativeCallingConvention.CDeclaration _
                OrElse ntFuncPtr.CallingConvention = NativeCallingConvention.Standard Then
                del.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedFunctionPointerAttribute(ntFuncPtr.CallingConvention))
            End If

            del.UserData.Item(TransformConstants.DefinedType) = ntFuncPtr
            del.UserData.Item(TransformConstants.ReturnType) = ntFuncPtr.Signature.ReturnType
            del.UserData.Item(TransformConstants.ReturnTypeSal) = ntFuncPtr.Signature.ReturnTypeSalAttribute
            del.Comments.Add(New CodeCommentStatement(comment, True))

            Return del
        End Function

        ''' <summary>
        ''' Generate the procedures into a type
        ''' </summary>
        ''' <param name="enumerable"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateProcedures(ByVal enumerable As IEnumerable(Of NativeProcedure)) As CodeTypeDeclaration
            If enumerable Is Nothing Then : Throw New ArgumentNullException("enumerable") : End If

            Dim ctd As New CodeTypeDeclaration()
            ctd.Name = TransformConstants.NativeMethodsName
            ctd.Attributes = MemberAttributes.Public
            ctd.IsPartial = True

            For Each proc As NativeProcedure In enumerable
                ctd.Members.Add(GenerateProcedure(proc))
            Next

            Return ctd
        End Function

        ''' <summary>
        ''' Generate a procedure from the specified proc
        ''' </summary>
        ''' <param name="ntProc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateProcedure(ByVal ntProc As NativeProcedure) As CodeMemberMethod
            If ntProc Is Nothing Then : Throw New ArgumentNullException("ntProc") : End If

            ' Create the proc
            Dim ntSig As NativeSignature = ntProc.Signature
            Dim procComment As String = "Return Type: "
            Dim proc As New CodeMemberMethod()
            proc.Name = ntProc.Name
            proc.ReturnType = GenerateTypeReferenceImpl(ntSig.ReturnType, procComment)
            proc.UserData.Item(TransformConstants.ReturnType) = ntSig.ReturnType
            If ntSig.ReturnTypeSalAttribute IsNot Nothing Then
                proc.UserData.Item(TransformConstants.ReturnTypeSal) = ntSig.ReturnTypeSalAttribute
            Else
                proc.UserData.Item(TransformConstants.ReturnTypeSal) = New NativeSalAttribute()
            End If
            proc.Attributes = MemberAttributes.Public Or MemberAttributes.Static
            proc.UserData.Item(TransformConstants.Procedure) = ntProc
            proc.UserData.Item(TransformConstants.ReturnType) = ntSig.ReturnType
            proc.UserData.Item(TransformConstants.ReturnTypeSal) = ntSig.ReturnTypeSalAttribute

            ' Add the DLL import attribute
            Dim dllName As String = ntProc.DllName
            If String.IsNullOrEmpty(dllName) Then
                dllName = "<Unknown>"
            End If
            proc.CustomAttributes.Add(
                MarshalAttributeFactory.CreateDllImportAttribute(dllName, ntProc.Name, ntProc.CallingConvention))

            ' Generate the parameters
            proc.Parameters.AddRange(GenerateParameters(ntProc.Signature, procComment))
            proc.Comments.Add(New CodeCommentStatement(procComment, True))
            Return proc
        End Function

        Private Function GenerateParameters(ByVal ntSig As NativeSignature, Optional ByRef comments As String = Nothing) As CodeParameterDeclarationExpressionCollection
            ThrowIfNull(ntSig)
            If comments Is Nothing Then
                comments = String.Empty
            End If

            Dim col As New CodeParameterDeclarationExpressionCollection()
            Dim count As Int32 = 0
            For Each ntParam As NativeParameter In ntSig.Parameters
                Dim comment As String = Nothing
                Dim param As New CodeParameterDeclarationExpression()
                param.Name = ntParam.Name
                param.Type = GenerateTypeReferenceImpl(ntParam.NativeType, comment)
                param.UserData.Item(TransformConstants.Param) = ntParam
                col.Add(param)

                If String.IsNullOrEmpty(param.Name) Then
                    param.Name = "param" & count
                End If

                ' Add the type comment to the procedure
                comments &= vbCrLf
                comments &= param.Name & ": " & comment
                count += 1
            Next

            Return col
        End Function

        ''' <summary>
        ''' Generate the macros as constants into a type
        ''' </summary>
        ''' <param name="enumerable"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateConstants(ByVal enumerable As IEnumerable(Of NativeConstant)) As CodeTypeDeclaration
            Dim ctd As New CodeTypeDeclaration()
            ctd.Name = TransformConstants.NativeConstantsName
            ctd.IsPartial = True
            ctd.IsClass = True

            GenerateConstants(ctd, enumerable)

            Return ctd
        End Function

        ''' <summary>
        ''' Generate the macros as constants into the specified type declaration.  This will only utilize simple 
        ''' macros such as numbers and quoted string.
        ''' </summary>
        ''' <param name="ctd"></param>
        ''' <param name="enumerable"></param>
        ''' <remarks></remarks>
        Public Sub GenerateConstants(ByVal ctd As CodeTypeDeclaration, ByVal enumerable As IEnumerable(Of NativeConstant))
            If ctd Is Nothing Then
                Throw New ArgumentNullException("ctd")
            End If

            If enumerable Is Nothing Then
                Throw New ArgumentNullException("enumerable")
            End If

            For Each nConst As NativeConstant In enumerable
                ' Build up the attributes and value
                Dim cMember As New CodeMemberField
                cMember.Name = nConst.Name
                cMember.Attributes = MemberAttributes.Public Or MemberAttributes.Const
                ctd.Members.Add(cMember)

                If ConstantKind.MacroMethod = nConst.ConstantKind Then
                    ' Generation of macro methods is not supported entirely.  Right now macro methods
                    ' expressions are stored as text and they are outputted as a string.  Offer an explanation
                    ' here
                    cMember.Comments.Add(New CodeCommentStatement(
                        "Warning: Generation of Method Macros is not supported at this time", True))
                End If

                ' Set the init expression
                GenerateInitExpression(cMember, nConst, nConst.Value)
            Next
        End Sub


        ''' <summary>
        ''' Generate the members of the container
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <param name="ctd"></param>
        ''' <remarks></remarks>
        Private Sub GenerateContainerMembers(ByVal nt As NativeDefinedType, ByVal ctd As CodeTypeDeclaration)
            ThrowIfNull(nt)
            ThrowIfNull(ctd)

            Dim bitVectorCount As Integer = 0
            For i As Integer = 0 To nt.Members.Count - 1
                Dim member As NativeMember = nt.Members(i)

                ' Don't process unnamed container members
                If String.IsNullOrEmpty(member.Name) Then
                    Continue For
                End If

                If IsBitVector(member.NativeType) Then

                    ' Get the list of bitvectors that will fit into the next int
                    Dim bitCount As Integer = 0
                    Dim list As New List(Of NativeMember)
                    Dim bitVector As NativeBitVector = Nothing

                    While (i < nt.Members.Count _
                            AndAlso IsBitVector(nt.Members(i).NativeType, bitVector) _
                            AndAlso bitCount + bitVector.Size <= 32)
                        list.Add(nt.Members(i))
                        i += 1
                    End While
                    i -= 1

                    ' Generate the int for the list of bit vectors
                    bitVectorCount += 1

                    Dim cMember As CodeMemberField = GenerateContainerMember(
                        New NativeMember("bitvector" & bitVectorCount, New NativeBuiltinType(BuiltinType.NativeInt32, True)),
                        ctd)
                    cMember.Comments.Clear()

                    Dim comment As New CodeComment(String.Empty, True)
                    Dim offset As Integer = 0
                    For j As Integer = 0 To list.Count - 1
                        If j > 0 Then
                            comment.Text &= vbCrLf
                        End If

                        IsBitVector(list(j).NativeType, bitVector)
                        comment.Text &= list(j).Name & " : " & bitVector.Size
                        GenerateBitVectorProperty(list(j), offset, ctd, cMember)
                        offset += bitVector.Size
                    Next
                    cMember.Comments.Add(New CodeCommentStatement(comment))
                Else
                    GenerateContainerMember(member, ctd)
                End If
            Next
        End Sub

        Private Function IsBitVector(ByVal nt As NativeType) As Boolean
            Dim bt As NativeBitVector = Nothing
            Return IsBitVector(nt, bt)
        End Function

        Private Function IsBitVector(ByVal nt As NativeType, ByRef bitvector As NativeBitVector) As Boolean
            ThrowIfNull(nt)

            nt = nt.DigThroughTypedefAndNamedTypes()

            If nt IsNot Nothing AndAlso nt.Kind = NativeSymbolKind.BitVectorType Then
                bitvector = DirectCast(nt, NativeBitVector)
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Generate a property to wrap the underlying bit vector
        ''' </summary>
        ''' <param name="ntMember"></param>
        ''' <param name="offset"></param>
        ''' <param name="ctd"></param>
        ''' <param name="codeMember"></param>
        ''' <remarks></remarks>
        Private Sub GenerateBitVectorProperty(ByVal ntMember As NativeMember, ByVal offset As Integer, ByVal ctd As CodeTypeDeclaration, ByVal codeMember As CodeMemberField)
            ThrowIfNull(ntMember)
            ThrowIfNull(ctd)
            ThrowIfNull(codeMember)
            ThrowIfTrue(offset < 0)

            Dim bitVector As NativeBitVector = Nothing
            IsBitVector(ntMember.NativeType, bitVector)

            ' First calculate the bitmask
            Dim mask As UInteger = 0
            For i As Integer = 0 To bitVector.Size - 1
                mask <<= 1
                mask = mask Or 1UI
            Next
            mask <<= offset

            ' Create the property
            Dim prop As New CodeMemberProperty()
            prop.Name = ntMember.Name
            prop.Attributes = MemberAttributes.Public Or MemberAttributes.Final
            prop.Type = New CodeTypeReference(GetType(UInteger))
            ctd.Members.Add(prop)

            ' Build the get and set
            GenerateBitVectorPropertyGet(prop, codeMember.Name, mask, offset, bitVector)
            GenerateBitVectorPropertySet(prop, codeMember.Name, mask, offset, bitVector)

        End Sub

        Private Sub GenerateBitVectorPropertyGet(ByVal prop As CodeMemberProperty, ByVal fieldName As String, ByVal mask As UInteger, ByVal offset As Integer, ByVal bitVector As NativeBitVector)
            prop.HasGet = True

            ' Get the value from the mask
            Dim exprGet As New CodeBinaryOperatorExpression(
                New CodeFieldReferenceExpression(New CodeThisReferenceExpression(), fieldName),
                CodeBinaryOperatorType.BitwiseAnd,
                New CodePrimitiveExpression(mask))

            ' Shift the result down
            Dim exprShift As New CodeBinaryOperatorExpression(
                exprGet,
                CodeBinaryOperatorType.Divide,
                New CodePrimitiveExpression(Math.Pow(2, offset)))

            ' If the offset is 0 then don't do the shift
            Dim outerExpr As CodeExpression
            If 0 = offset Then
                outerExpr = exprGet
            Else
                outerExpr = exprShift
            End If

            ' Cast it back to an integer since we are now at a UInteger and the property is Integer 
            Dim retStmt As New CodeMethodReturnStatement()
            retStmt.Expression = New CodeCastExpression(
                New CodeTypeReference(GetType(UInteger)),
                outerExpr)

            prop.GetStatements.Add(retStmt)
        End Sub

        Private Sub GenerateBitVectorPropertySet(ByVal prop As CodeMemberProperty, ByVal fieldName As String, ByVal mask As UInteger, ByVal offset As Integer, ByVal bitVector As NativeBitVector)
            prop.HasSet = True

            ' Shift it
            Dim exprShift As CodeExpression
            If offset <> 0 Then
                exprShift = New CodeBinaryOperatorExpression(
                    New CodePropertySetValueReferenceExpression(),
                    CodeBinaryOperatorType.Multiply,
                    New CodePrimitiveExpression(Math.Pow(2, offset)))
            Else
                exprShift = New CodePropertySetValueReferenceExpression()
            End If

            ' Or it with the current
            Dim exprOr As New CodeBinaryOperatorExpression(
                exprShift,
                CodeBinaryOperatorType.BitwiseOr,
                New CodeFieldReferenceExpression(New CodeThisReferenceExpression(), fieldName))

            ' Assign it to the field
            Dim asg As New CodeAssignStatement(
                New CodeFieldReferenceExpression(New CodeThisReferenceExpression(), fieldName),
                New CodeCastExpression(
                    New CodeTypeReference(GetType(UInteger)),
                    exprOr))
            prop.SetStatements.Add(asg)
        End Sub

        ''' <summary>
        ''' Generate the NativeMember
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <param name="ctd"></param>
        ''' <remarks></remarks>
        Private Function GenerateContainerMember(ByVal nt As NativeMember, ByVal ctd As CodeTypeDeclaration) As CodeMemberField
            ThrowIfNull(nt)
            ThrowIfNull(ctd)
            ThrowIfTrue(nt.NativeType.Kind = NativeSymbolKind.BitVectorType)  ' Bitvector instances should be handled seperately

            ' Generate the type reference and comment
            Dim comment As String = String.Empty
            Dim member As New CodeMemberField()
            member.Name = nt.Name
            member.Type = GenerateTypeReferenceImpl(nt.NativeType, comment)
            member.Attributes = MemberAttributes.Public
            member.Comments.Add(New CodeCommentStatement(comment, True))
            member.UserData.Add(TransformConstants.Member, nt)
            ctd.Members.Add(member)

            ' If this is an array then add the appropriate marshal directive if it's an inline array
            Dim ntArray As NativeArray = TryCast(nt.NativeType, NativeArray)
            If ntArray IsNot Nothing AndAlso ntArray.ElementCount > 0 Then
                ' Add the struct layout attribute
                Dim attrRef As New CodeTypeReference(GetType(MarshalAsAttribute))
                Dim attr As New CodeAttributeDeclaration(attrRef)

                ' ByValArray
                Dim asArg As New CodeAttributeArgument()
                asArg.Name = String.Empty
                asArg.Value = New CodeFieldReferenceExpression(
                    New CodeTypeReferenceExpression(GetType(UnmanagedType)),
                    "ByValArray")
                attr.Arguments.Add(asArg)

                ' SizeConst arg
                Dim sizeArg As New CodeAttributeArgument()
                sizeArg.Name = "SizeConst"
                sizeArg.Value = New CodePrimitiveExpression(ntArray.ElementCount)
                attr.Arguments.Add(sizeArg)

                ' ArraySubType
                Dim elemType As NativeType = ntArray.RealTypeDigged
                Dim subTypeArg As New CodeAttributeArgument()
                subTypeArg.Name = "ArraySubType"
                If elemType.Kind = NativeSymbolKind.BuiltinType Then
                    ' Builtin types know their size in bytes
                    Dim elemBt As NativeBuiltinType = DirectCast(elemType, NativeBuiltinType)
                    subTypeArg.Value = New CodeFieldReferenceExpression(
                        New CodeTypeReferenceExpression(GetType(UnmanagedType)),
                        elemBt.UnmanagedType.ToString())
                ElseIf elemType.Kind = NativeSymbolKind.PointerType OrElse elemType.Kind = NativeSymbolKind.ArrayType Then

                    ' Marshal pointers as system ints
                    subTypeArg.Value = New CodeFieldReferenceExpression(
                        New CodeTypeReferenceExpression(GetType(UnmanagedType)),
                        "SysUInt")
                Else
                    subTypeArg.Value = New CodeFieldReferenceExpression(
                        New CodeTypeReferenceExpression(GetType(UnmanagedType)),
                        "Struct")

                End If
                attr.Arguments.Add(subTypeArg)

                member.CustomAttributes.Add(attr)
            End If

            Return member
        End Function

        ''' <summary>
        ''' Convert a NativeValueExpression into managed code and make it the initialization expression of
        ''' the passed in member.
        ''' 
        ''' If the code is unable to generate a valid expression for the member it will make the expression
        ''' a stringized version of the original native expression.  It will add information in the comments
        ''' about why it could not properly generate the expression.  Lastly it will generate incompatible types
        ''' to force a compile error
        ''' </summary>
        ''' <param name="member"></param>
        ''' <param name="ntExpr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateInitExpression(ByVal member As CodeMemberField, ByVal target As NativeSymbol, ByVal ntExpr As NativeValueExpression) As Boolean
            If ntExpr Is Nothing Then
                member.Comments.Add(New CodeCommentStatement("Error: No value expression", True))
                member.InitExpression = New CodePrimitiveExpression(String.Empty)
                member.Type = New CodeTypeReference(GetType(Integer))
                Return False
            End If

            member.Comments.Add(New CodeCommentStatement(
                String.Format("{0} -> {1}", member.Name, ntExpr.Expression),
                True))

            ' It's not legal for a symbol to be used as part of it's initialization expression in most languages.  
            ' There for we need to mark it as the initialization member so the generated will output NULL in it's place
            m_symbolValueMap.Add(target.Name, target)
            Try
                Dim ex As Exception = Nothing
                If TryGenerateValueExpression(ntExpr, member.InitExpression, member.Type, ex) Then
                    Return True
                Else
                    member.Comments.Add(New CodeCommentStatement(
                        String.Format("Error generating expression: {0}", ex.Message),
                        True))
                    member.InitExpression = New CodePrimitiveExpression(ntExpr.Expression)
                    member.Type = New CodeTypeReference(GetType(String))
                    Return False
                End If
            Finally
                m_symbolValueMap.Remove(target.Name)
            End Try
        End Function

        Public Function TryGenerateValueExpression(ByVal ntExpr As NativeValueExpression, ByRef expr As CodeExpression, ByRef exprType As CodeTypeReference, ByRef ex As Exception) As Boolean
            Try
                If Not ntExpr.IsParsable Then
                    Dim msg As String = "Expression is not parsable.  Treating value as a raw string"
                    Throw New InvalidOperationException(msg)
                End If

                expr = GenerateValueExpressionImpl(ntExpr.Node, exprType)
                ex = Nothing
                Return True
            Catch ex2 As Exception
                ex = ex2
                Return False
            End Try

        End Function

#Region "CodeTypeReference Generation"

        Private Function GenerateTypeReferenceImpl(ByVal nt As NativeType, ByRef comment As String) As CodeTypeReference
            ThrowIfNull(nt)

            Select Case nt.Category
                Case NativeSymbolCategory.Defined
                    Return GenerateDefinedTypeReferenceImpl(DirectCast(nt, NativeDefinedType), comment)
                Case NativeSymbolCategory.Proxy
                    Return GenerateProxyTypeReferenceImpl(DirectCast(nt, NativeProxyType), comment)
                Case NativeSymbolCategory.Specialized
                    Return GenerateSpecializedTypeReferenceImpl(DirectCast(nt, NativeSpecializedType), comment)
            End Select

            Dim errorMsg As String = String.Format("Error generating reference to {0}", nt.DisplayName)
            Throw New InvalidOperationException(errorMsg)
        End Function

        Public Function GenerateDefinedTypeReferenceImpl(ByVal definedNt As NativeDefinedType, ByRef comment As String) As CodeTypeReference
            ThrowIfNull(definedNt)

            comment &= definedNt.Name
            Return New CodeTypeReference(definedNt.Name)
        End Function

        Public Function GenerateProxyTypeReferenceImpl(ByVal proxyNt As NativeProxyType, ByRef comment As String) As CodeTypeReference

            ' Check the various proxy types
            If proxyNt.RealType Is Nothing Then
                Dim msg As String = String.Format("Could not find the real type for {0}", proxyNt.DisplayName)
                Throw New InvalidOperationException(msg)
            End If

            Select Case proxyNt.Kind
                Case NativeSymbolKind.ArrayType
                    comment &= proxyNt.DisplayName
                    Dim arrayNt As NativeArray = DirectCast(proxyNt, NativeArray)
                    Dim elemRef As CodeTypeReference = GenerateTypeReference(arrayNt.RealType)
                    Dim arrayRef As New CodeTypeReference(elemRef, 1)
                    Return arrayRef
                Case NativeSymbolKind.PointerType
                    comment &= proxyNt.DisplayName
                    Dim pointerNt As NativePointer = DirectCast(proxyNt, NativePointer)
                    Return New CodeTypeReference(GetType(IntPtr))
                Case NativeSymbolKind.TypedefType
                    Dim td As NativeTypeDef = DirectCast(proxyNt, NativeTypeDef)
                    comment &= td.Name & "->"
                    Return GenerateTypeReferenceImpl(td.RealType, comment)
                Case NativeSymbolKind.NamedType
                    ' Don't update the comment for named types.  Otherwise you get lots of 
                    ' comments like DWORD->DWORD->unsigned long
                    Dim namedNt As NativeNamedType = DirectCast(proxyNt, NativeNamedType)
                    Return GenerateTypeReferenceImpl(namedNt.RealType, comment)
                Case Else
                    Contract.InvalidEnumValue(proxyNt.Kind)
                    Return Nothing
            End Select
        End Function

        Public Function GenerateSpecializedTypeReferenceImpl(ByVal specialNt As NativeSpecializedType, ByRef comment As String) As CodeTypeReference
            ThrowIfNull(specialNt)

            Select Case specialNt.Kind
                Case NativeSymbolKind.BitVectorType
                    Dim bitNt As NativeBitVector = DirectCast(specialNt, NativeBitVector)
                    comment = String.Format("bitvector : {0}", bitNt.Size)
                    Return New CodeTypeReference(GetManagedNameForBitVector(bitNt))
                Case NativeSymbolKind.BuiltinType
                    Dim builtNt As NativeBuiltinType = DirectCast(specialNt, NativeBuiltinType)
                    Dim realType As Type = builtNt.ManagedType
                    comment &= builtNt.DisplayName
                    Return New CodeTypeReference(realType)
                Case Else
                    Contract.InvalidEnumValue(specialNt.Kind)
                    Return Nothing
            End Select
        End Function

#End Region

#Region "CodeExpression Generation"

        Private Function GenerateValueExpressionImpl(ByVal node As ExpressionNode, ByRef type As CodeTypeReference) As CodeExpression
            If node Is Nothing Then
                Throw New ArgumentNullException("node")
            End If

            Select Case node.Kind
                Case ExpressionKind.FunctionCall
                    Throw New InvalidOperationException("Error generating function call.  Operation not implemented")
                Case ExpressionKind.BinaryOperation
                    Return GenerateValueExpressionBinaryOperation(node, type)
                Case ExpressionKind.NegationOperation
                    Return GenerateValueExpressionNegation(node, type)
                Case ExpressionKind.NegativeOperation
                    Return GenerateValueExpressionNegative(node, type)
                Case ExpressionKind.Leaf
                    Return GenerateValueExpressionLeaf(node, type)
                Case ExpressionKind.Cast
                    Return GenerateValueExpressionCast(node, type)
                Case Else
                    InvalidEnumValue(node.Kind)
                    Return Nothing
            End Select
        End Function

        Private Function GenerateValueExpressionNegative(ByVal node As ExpressionNode, ByRef exprType As CodeTypeReference) As CodeExpression
            ThrowIfNull(node)

            Dim left As CodeExpression = Me.GenerateValueExpressionImpl(node.LeftNode, exprType)
            Return New CodeNegativeExpression(m_lang, left)
        End Function

        Private Function GenerateValueExpressionNegation(ByVal node As ExpressionNode, ByRef exprType As CodeTypeReference) As CodeExpression
            ThrowIfNull(node)

            Dim left As CodeExpression = Me.GenerateValueExpressionImpl(node.LeftNode, exprType)
            Return New CodeNotExpression(m_lang, left)
        End Function

        Private Function GenerateValueExpressionBinaryOperation(ByVal node As ExpressionNode, ByRef exprType As CodeTypeReference) As CodeExpression
            ThrowIfNull(node)

            If node.LeftNode Is Nothing OrElse node.RightNode Is Nothing Then
                Throw New InvalidOperationException("Error generating operation")
            End If

            If node.Token.TokenType = TokenType.OpShiftLeft OrElse node.Token.TokenType = TokenType.OpShiftRight Then
                ' Shift operations are not native supported by the CodeDom so we need to create a special CodeDom node here
                Return GenerateValueExpressionShift(node, exprType)

            End If

            Dim type As CodeBinaryOperatorType

            Select Case node.Token.TokenType
                Case TokenType.OpBoolAnd
                    type = CodeBinaryOperatorType.BooleanAnd
                Case TokenType.OpBoolOr
                    type = CodeBinaryOperatorType.BooleanOr
                Case TokenType.OpDivide
                    type = CodeBinaryOperatorType.Divide
                Case TokenType.OpGreaterThan
                    type = CodeBinaryOperatorType.GreaterThan
                Case TokenType.OpGreaterThanOrEqual
                    type = CodeBinaryOperatorType.GreaterThanOrEqual
                Case TokenType.OpLessThan
                    type = CodeBinaryOperatorType.LessThan
                Case TokenType.OpLessThanOrEqual
                    type = CodeBinaryOperatorType.LessThanOrEqual
                Case TokenType.OpMinus
                    type = CodeBinaryOperatorType.Subtract
                Case TokenType.OpModulus
                    type = CodeBinaryOperatorType.Modulus
                Case TokenType.OpPlus
                    type = CodeBinaryOperatorType.Add
                Case TokenType.Asterisk
                    type = CodeBinaryOperatorType.Multiply
                Case TokenType.Pipe
                    type = CodeBinaryOperatorType.BitwiseOr
                Case TokenType.Ampersand
                    type = CodeBinaryOperatorType.BitwiseAnd
                Case Else
                    Throw New InvalidOperationException("Unsupported operation")
            End Select

            Dim leftType As CodeTypeReference = Nothing
            Dim rightType As CodeTypeReference = Nothing
            Dim expr As CodeExpression = New CodeBinaryOperatorExpression(
                GenerateValueExpressionImpl(node.LeftNode, leftType),
                type,
                GenerateValueExpressionImpl(node.RightNode, rightType))
            exprType = leftType
            Return expr
        End Function

        Private Function GenerateValueExpressionShift(ByVal node As ExpressionNode, ByRef exprType As CodeTypeReference) As CodeExpression

            Dim isLeft As Boolean
            Select Case node.Token.TokenType
                Case TokenType.OpShiftLeft
                    isLeft = True
                Case TokenType.OpShiftRight
                    isLeft = False
                Case Else
                    InvalidEnumValue(node.Token.TokenType)
                    Return Nothing
            End Select

            Dim leftType As CodeTypeReference = Nothing
            Dim rightType As CodeTypeReference = Nothing
            Dim expr As CodeExpression = New CodeShiftExpression(
                Me.m_lang,
                isLeft,
                GenerateValueExpressionImpl(node.LeftNode, leftType),
                GenerateValueExpressionImpl(node.RightNode, rightType))
            exprType = leftType
            Return expr
        End Function

        Private Function GenerateValueExpressionLeaf(ByVal node As ExpressionNode, ByRef leafType As CodeTypeReference) As CodeExpression
            ThrowIfNull(node)

            Dim ntVal As NativeValue = DirectCast(node.Tag, NativeValue)
            If ntVal Is Nothing Then
                Throw New InvalidOperationException("Expected a NativeValue")
            End If

            If Not ntVal.IsValueResolved Then
                Throw New InvalidOperationException(String.Format("Value {0} is not resolved", ntVal.Name))
            End If

            Select Case ntVal.ValueKind
                Case NativeValueKind.Number
                    leafType = New CodeTypeReference(ntVal.Value.GetType())
                    Return New CodePrimitiveExpression(ntVal.Value)
                Case NativeValueKind.Boolean
                    leafType = New CodeTypeReference(GetType(Boolean))
                    Return New CodePrimitiveExpression(ntVal.Value)
                Case NativeValueKind.String
                    leafType = New CodeTypeReference(GetType(String))
                    Return New CodePrimitiveExpression(ntVal.Value)
                Case NativeValueKind.Character
                    leafType = New CodeTypeReference(GetType(Char))
                    Return New CodePrimitiveExpression(ntVal.Value)
                Case NativeValueKind.SymbolValue
                    Dim ns As NativeSymbol = ntVal.SymbolValue

                    ' Prevent the generation of a circular reference
                    If m_symbolValueMap.ContainsKey(ns.Name) Then
                        leafType = New CodeTypeReference(GetType(Object))
                        Return New CodePrimitiveExpression(Nothing)
                    End If

                    Select Case ns.Kind
                        Case NativeSymbolKind.Constant
                            leafType = CalculateConstantType(DirectCast(ns, NativeConstant))
                            Return New CodeFieldReferenceExpression(
                                New CodeTypeReferenceExpression(TransformConstants.NativeConstantsName),
                                ns.Name)
                        Case NativeSymbolKind.EnumType
                            leafType = Me.GenerateTypeReference(DirectCast(ns, NativeEnum))
                            Return New CodeFieldReferenceExpression(
                                New CodeTypeReferenceExpression(ns.Name),
                                ntVal.Name)
                        Case Else
                            Throw New InvalidOperationException(String.Format("Generation of {0} not supported as a value", ns.Kind))
                    End Select
                Case NativeValueKind.SymbolType
                    Throw New InvalidOperationException("Types are not supported as leaf nodes")
                Case Else
                    InvalidEnumValue(ntVal.ValueKind)
                    Return Nothing
            End Select
        End Function

        Private Function GenerateValueExpressionCast(ByVal node As ExpressionNode, ByRef exprType As CodeTypeReference) As CodeExpression
            Throw New InvalidOperationException("Cast expressions are not supported in constants")
        End Function

        ''' <summary>
        ''' Calculate the type of the specified constant.  It's possible for a C++ constant to refer 
        ''' to itself which is strange but legal.  Imagine the following.
        ''' 
        ''' #define A A
        ''' 
        ''' In this case the type is indermenistic so we choose Object.  To detect this we record the 
        ''' objects we are currently evaluating for types and whenever we recursively hit one, return
        ''' Object.  This prevents a mutually exclusive scenario
        ''' </summary>
        ''' <param name="nConst"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CalculateConstantType(ByVal nConst As NativeConstant) As CodeTypeReference
            If nConst.Value Is Nothing Then
                Return New CodeTypeReference(GetType(Integer))
            ElseIf m_typeMap.ContainsKey(nConst.Name) Then
                Return New CodeTypeReference(GetType(Object))
            End If

            m_typeMap.Add(nConst.Name, nConst)
            Try
                Dim codeExpr As CodeExpression = Nothing
                Dim codeType As CodeTypeReference = Nothing
                Dim ex As Exception = Nothing
                If Not TryGenerateValueExpression(nConst.Value, codeExpr, codeType, ex) Then
                    codeType = New CodeTypeReference(GetType(Integer))
                End If

                Return codeType
            Finally
                m_typeMap.Remove(nConst.Name)
            End Try
        End Function

#End Region

        Private Function GetManagedNameForBitVector(ByVal bitNt As NativeBitVector) As String
            ThrowIfNull(bitNt)
            Return String.Format("BitVector_Size_{0}", bitNt.Size)
        End Function

    End Class

End Namespace
