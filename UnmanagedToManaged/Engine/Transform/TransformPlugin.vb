' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports CodeParam = System.CodeDom.CodeParameterDeclarationExpression
Imports CodeParamCollection = System.CodeDom.CodeParameterDeclarationExpressionCollection
Imports CodeParamPair = System.Collections.Generic.KeyValuePair(Of System.CodeDom.CodeParameterDeclarationExpression, System.CodeDom.CodeParameterDeclarationExpression)

Namespace Transform

    ''' <summary>
    ''' What kind of transformations does this plugin support
    ''' </summary>
    ''' <remarks></remarks>
    <Flags()> _
    Public Enum TransformKindFlags
        Signature = &H1
        StructMembers = &H2
        UnionMembers = &H4
        EnumMembers = &H8
        WrapperMethods = &H10

        All = TransformKindFlags.Signature Or _
            TransformKindFlags.StructMembers Or _
            TransformKindFlags.UnionMembers Or _
            TransformKindFlags.EnumMembers Or _
            TransformKindFlags.WrapperMethods
    End Enum

    ''' <summary>
    ''' Base class for transformation classes that run on the CodeDom 
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class TransformPlugin

        Private Const ProcessedParamKey As String = "158d1b71-b224-4637-bd73-4f7f83b6777c"
        Private Const ProcessedReturnKey As String = "fd83becd-ba9f-4c08-9e79-c3285b74c8cb"
        Private Const ProcessedMemberKey As String = "a12fe995-7b38-4b84-82d5-38e315499806"

        Private m_lang As LanguageType

        Public Property LanguageType() As LanguageType
            Get
                Return m_lang
            End Get
            Set(ByVal value As LanguageType)
                m_lang = value
            End Set
        End Property

        Public MustOverride ReadOnly Property TransformKind() As TransformKindFlags

        ''' <summary>
        ''' Process the parameters of the method or delegate
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub ProcessParameters(ByVal method As CodeMemberMethod)
            ThrowIfFalse(0 <> (TransformKind And TransformKindFlags.Signature))

            ProcessParametersImpl(method.Parameters, False)
        End Sub

        Public Sub ProcessParameters(ByVal del As CodeTypeDelegate)
            ThrowIfFalse(0 <> (TransformKind And TransformKindFlags.Signature))

            ProcessParametersImpl(del.Parameters, True)
        End Sub

        ''' <summary>
        ''' Process the return type of a method
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub ProcessReturnType(ByVal codeMethod As CodeMemberMethod)
            ThrowIfFalse(0 <> (TransformKind And TransformKindFlags.Signature))

            If IsReturnProcessed(codeMethod) Then
                Return
            End If

            ProcessReturnTypeImpl( _
                codeMethod, _
                GetNativeReturnType(codeMethod), _
                GetNativeReturnTypeSal(codeMethod))
        End Sub

        Public Sub ProcessStructMembers(ByVal ctd As CodeTypeDeclaration)
            ThrowIfFalse(0 <> (TransformKind And TransformKindFlags.StructMembers))
            ProcessStructMembersImpl(ctd)
        End Sub

        Public Sub ProcessUnionMembers(ByVal ctd As CodeTypeDeclaration)
            ThrowIfFalse(0 <> (TransformKind And TransformKindFlags.UnionMembers))
            ProcessUnionMembersImpl(ctd)
        End Sub

        Public Function ProcessWrapperMethods(ByVal codeMethod As CodeMemberMethod) As List(Of CodeMemberMethod)
            ThrowIfFalse(0 <> (TransformKind And TransformKindFlags.WrapperMethods))
            Return ProcessWrapperMethodsImpl(codeMethod)
        End Function

#Region "Overridable"

        ''' <summary>
        ''' Process the return type of a method
        ''' </summary>
        ''' <param name="codeMethod"></param>
        ''' <param name="ntType"></param>
        ''' <param name="ntSal"></param>
        ''' <remarks></remarks>
        Protected Overridable Sub ProcessReturnTypeImpl(ByVal codeMethod As CodeMemberMethod, ByVal ntType As NativeType, ByVal ntSal As NativeSalAttribute)

        End Sub

        ''' <summary>
        ''' Override to process the parameters as a whole
        ''' </summary>
        ''' <param name="col"></param>
        ''' <remarks></remarks>
        Protected Overridable Sub ProcessParametersImpl(ByVal col As CodeParameterDeclarationExpressionCollection, ByVal isDelegate As Boolean)
            For Each cur As CodeParameterDeclarationExpression In col
                If IsParamProcessed(cur) Then
                    Continue For
                End If

                ProcessSingleParameter(cur, GetNativeParameter(cur), isDelegate)
            Next
        End Sub

        ''' <summary>
        ''' Process an individual parameter
        ''' </summary>
        ''' <param name="codeParam"></param>
        ''' <param name="ntParam"></param>
        ''' <remarks></remarks>
        Protected Overridable Sub ProcessSingleParameter(ByVal codeParam As CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)

        End Sub

        Protected Overridable Sub ProcessStructMembersImpl(ByVal ctd As CodeTypeDeclaration)
            For Each mem As CodeTypeMember In ctd.Members
                If IsMemberProcessed(mem) Then
                    Return
                End If

                Dim field As CodeMemberField = TryCast(mem, CodeMemberField)
                If field IsNot Nothing Then
                    Dim ntMem As NativeMember = GetNativeMember(field)
                    If ntMem IsNot Nothing Then
                        ProcessSingleStructField(ctd, field, ntMem)
                    End If
                End If
            Next
        End Sub

        Protected Overridable Sub ProcessSingleStructField(ByVal ctd As CodeTypeDeclaration, ByVal field As CodeMemberField, ByVal ntMem As NativeMember)

        End Sub

        Protected Overridable Sub ProcessUnionMembersImpl(ByVal ctd As CodeTypeDeclaration)
            For Each mem As CodeTypeMember In ctd.Members
                If IsMemberProcessed(mem) Then
                    Return
                End If

                Dim field As CodeMemberField = TryCast(mem, CodeMemberField)
                If field IsNot Nothing Then
                    Dim ntMem As NativeMember = GetNativeMember(field)
                    If ntMem IsNot Nothing Then
                        ProcessSingleUnionField(ctd, field, ntMem)
                    End If
                End If
            Next
        End Sub

        Protected Overridable Sub ProcessSingleUnionField(ByVal ctd As CodeTypeDeclaration, ByVal field As CodeMemberField, ByVal ntMem As NativeMember)

        End Sub

        Protected Overridable Function ProcessWrapperMethodsImpl(ByVal codeMethod As CodeMemberMethod) As List(Of CodeMemberMethod)
            Dim ret As CodeMemberMethod = ProcessSingleWrapperMethod(codeMethod)
            Dim list As New List(Of CodeMemberMethod)
            If ret IsNot Nothing Then
                list.Add(ret)
            End If

            Return list
        End Function

        Protected Overridable Function ProcessSingleWrapperMethod(ByVal codeMethod As CodeMemberMethod) As CodeMemberMethod
            Return Nothing
        End Function

#End Region

#Region "Helpers"

        Friend Shared Function GetDefinedType(ByVal ctd As CodeTypeDeclaration) As NativeDefinedType
            Dim obj As Object = ctd.UserData.Item(TransformConstants.DefinedType)
            Return TryCast(obj, NativeDefinedType)
        End Function

        Friend Shared Function GetNativeParameter(ByVal param As CodeParameterDeclarationExpression) As NativeParameter
            Dim obj As Object = param.UserData.Item(TransformConstants.Param)
            Return TryCast(obj, NativeParameter)
        End Function

        Friend Shared Function GetNativeReturnType(ByVal codeMethod As CodeMemberMethod) As NativeType
            Dim obj As Object = codeMethod.UserData.Item(TransformConstants.ReturnType)
            Return TryCast(obj, NativeType)
        End Function

        Friend Shared Function GetNativeReturnTypeSal(ByVal codeMethod As CodeMemberMethod) As NativeSalAttribute
            Dim obj As Object = codeMethod.UserData.Item(TransformConstants.ReturnTypeSal)
            Return TryCast(obj, NativeSalAttribute)
        End Function

        Friend Shared Function GetNativeReturnType(ByVal del As CodeTypeDelegate) As NativeType
            Dim obj As Object = del.UserData.Item(TransformConstants.ReturnType)
            Return TryCast(obj, NativeType)
        End Function

        Friend Shared Function GetNativeMember(ByVal mem As CodeMemberField) As NativeMember
            If mem.UserData.Contains(TransformConstants.Member) Then
                Return TryCast(mem.UserData(TransformConstants.Member), NativeMember)
            End If
            Return Nothing
        End Function

        Protected Function IsCharsetSpecified(ByVal ctd As CodeTypeDeclaration, ByRef charset As CharSet) As Boolean
            For Each attrib As CodeAttributeDeclaration In ctd.CustomAttributes
                If IsType(attrib.AttributeType, GetType(StructLayoutAttribute)) Then
                    For Each arg As CodeAttributeArgument In attrib.Arguments
                        If 0 = String.CompareOrdinal(arg.Name, "CharSet") Then
                            Dim pValue As CodeFieldReferenceExpression = TryCast(arg.Value, CodeFieldReferenceExpression)
                            If pValue IsNot Nothing Then
                                charset = DirectCast([Enum].Parse(GetType(CharSet), pValue.FieldName), CharSet)
                                Return True
                            End If
                        End If
                    Next
                End If
            Next

            Return False
        End Function

        Protected Sub AddCharSet(ByVal ctd As CodeTypeDeclaration, ByVal charSet As CharSet)
            Dim attrib As CodeAttributeDeclaration = Nothing
            For Each cur As CodeAttributeDeclaration In ctd.CustomAttributes
                If IsType(cur.AttributeType, GetType(StructLayoutAttribute)) Then
                    attrib = cur
                    Exit For
                End If
            Next

            If attrib Is Nothing Then
                attrib = MarshalAttributeFactory.CreateStructLayoutAttribute(LayoutKind.Auto)
            End If

            attrib.Arguments.Add(New CodeAttributeArgument( _
                "CharSet", _
                New CodeFieldReferenceExpression( _
                    New CodeTypeReferenceExpression(GetType(CharSet)), _
                    charSet.ToString())))
        End Sub

        ''' <summary>
        ''' Is this one of the recognized boolean types
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function IsBooleanType(ByVal nt As NativeType, ByRef type As BooleanType) As Boolean
            If nt.DigThroughTypedefAndNamedTypesFor("BOOL") IsNot Nothing Then
                type = BooleanType.Windows
                Return True
            End If

            nt = nt.DigThroughTypedefAndNamedTypes()
            If nt.Kind = NativeSymbolKind.BuiltinType _
                AndAlso DirectCast(nt, NativeBuiltinType).BuiltinType = BuiltinType.NativeBoolean Then
                type = BooleanType.CStyle
                Return True
            End If

            Return False
        End Function

        Protected Function IsSystemIntType(ByVal ntParam As NativeParameter) As Boolean
            Return IsSystemIntType(ntParam.NativeType)
        End Function

        Protected Function IsSystemIntType(ByVal nt As NativeType) As Boolean
            If nt.DigThroughTypedefAndNamedTypesFor("UINT_PTR") IsNot Nothing _
                OrElse nt.DigThroughTypedefAndNamedTypesFor("LONG_PTR") IsNot Nothing _
                OrElse nt.DigThroughTypedefAndNamedTypesFor("size_t") IsNot Nothing Then
                Return True
            End If

            Return False
        End Function

        Protected Function IsCharType(ByVal nt As NativeType, ByRef charSet As CharSet) As Boolean
            ThrowIfNull(nt)

            ' BYTE is commonly typedef'd out to char however it is not a char
            ' type persay.  Essentially BYTE[] should not convert into String or
            ' StringBuilder
            If nt.DigThroughTypedefAndNamedTypesFor("BYTE") IsNot Nothing Then
                Return False
            End If

            If nt.DigThroughTypedefAndNamedTypesFor("TCHAR") IsNot Nothing Then
                charSet = Runtime.InteropServices.CharSet.Auto
                Return True
            End If

            If nt.DigThroughTypedefAndNamedTypesFor("WCHAR") IsNot Nothing _
                OrElse nt.DigThroughTypedefAndNamedTypesFor("wchar_t") IsNot Nothing Then
                charSet = Runtime.InteropServices.CharSet.Unicode
                Return True
            End If

            Dim digged As NativeType = nt.DigThroughTypedefAndNamedTypes()
            If digged IsNot Nothing AndAlso digged.Kind = NativeSymbolKind.BuiltinType Then
                Dim bt As NativeBuiltinType = DirectCast(digged, NativeBuiltinType)
                If bt.BuiltinType = BuiltinType.NativeChar Then
                    charSet = Runtime.InteropServices.CharSet.Ansi
                    Return True
                ElseIf bt.BuiltinType = BuiltinType.NativeWChar Then
                    charSet = Runtime.InteropServices.CharSet.Unicode
                    Return True
                End If
            End If

            Return False
        End Function

        Protected Function IsArrayOfCharType(ByVal nt As NativeType) As Boolean
            Dim kind As CharSet = Runtime.InteropServices.CharSet.None
            Return IsArrayOfCharType(nt, kind)
        End Function

        ''' <summary>
        ''' Is this an array of char's
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function IsArrayOfCharType(ByVal nt As NativeType, ByRef charSet As CharSet) As Boolean
            ThrowIfNull(nt)

            nt = nt.DigThroughTypedefAndNamedTypes()
            If nt.Kind <> NativeSymbolKind.ArrayType Then
                Return False
            End If

            Return IsCharType(DirectCast(nt, NativeArray).RealType, charSet)
        End Function

        Protected Function IsArrayOfBuiltin(ByVal nt As NativeType, ByRef bt As NativeBuiltinType) As Boolean
            ThrowIfNull(nt)

            nt = nt.DigThroughTypedefAndNamedTypes()
            If nt.Kind <> NativeSymbolKind.ArrayType Then
                Return False
            End If

            Dim realNt As NativeType = DirectCast(nt, NativeArray).RealTypeDigged
            If realNt Is Nothing OrElse realNt.Kind <> NativeSymbolKind.BuiltinType Then
                Return False
            End If

            bt = DirectCast(realNt, NativeBuiltinType)
            Return True
        End Function

        Protected Function IsPointerToBuiltin(ByVal nt As NativeType, ByRef bt As NativeBuiltinType) As Boolean
            ThrowIfNull(nt)

            nt = nt.DigThroughTypedefAndNamedTypes()
            If nt.Kind <> NativeSymbolKind.PointerType Then
                Return False
            End If

            Dim realNt As NativeType = DirectCast(nt, NativePointer).RealTypeDigged
            If realNt.Kind <> NativeSymbolKind.BuiltinType Then
                Return False
            End If

            bt = DirectCast(realNt, NativeBuiltinType)
            Return True
        End Function

        Protected Function GetPointerTarget(Of T As NativeType)(ByVal nt As NativeType, ByRef targetType As T) As Boolean
            ThrowIfNull(nt)

            nt = nt.DigThroughTypedefAndNamedTypes()
            If nt.Kind <> NativeSymbolKind.PointerType Then
                Return False
            End If

            Dim pointer As NativePointer = DirectCast(nt, NativePointer)
            Dim target As NativeType = pointer.RealTypeDigged
            If target Is Nothing Then
                Return False
            End If

            targetType = TryCast(target, T)
            Return targetType IsNot Nothing
        End Function

        Protected Function IsPointerToCharType(ByVal param As NativeParameter) As Boolean
            Dim kind As CharSet = CharSet.None
            Return IsPointerToCharType(param, kind)
        End Function

        Protected Function IsPointerToCharType(ByVal param As NativeParameter, ByRef kind As CharSet) As Boolean
            Return IsPointerToCharType(param.NativeType, kind)
        End Function

        Protected Function IsPointerToCharType(ByVal mem As NativeMember, ByRef kind As CharSet) As Boolean
            Return IsPointerToCharType(mem.NativeType, kind)
        End Function

        Protected Function IsPointerToCharType(ByVal type As NativeType) As Boolean
            Dim kind As CharSet = CharSet.None
            Return IsPointerToCharType(type, kind)
        End Function

        Protected Function IsPointerToCharType(ByVal type As NativeType, ByRef kind As CharSet) As Boolean
            Dim digged As NativeType = type.DigThroughTypedefAndNamedTypes()
            If digged IsNot Nothing AndAlso digged.Kind = NativeSymbolKind.PointerType Then

                ' Depending on the settings, LPTSTR and LPCTSTR are commonly going to be defined as pointing
                ' to a CHAR instead of a TCHAR
                If type.DigThroughTypedefAndNamedTypesFor("LPCTSTR") IsNot Nothing _
                    OrElse type.DigThroughTypedefAndNamedTypesFor("LPTSTR") IsNot Nothing Then
                    kind = CharSet.Auto
                    Return True
                End If

                ' WCHAR is commonly typedef'd into "unsigned short".  We need to manually dig through the typedefs
                ' and named types looking for WCHAR
                Dim pt As NativePointer = DirectCast(digged, NativePointer)
                Return IsCharType(pt.RealType, kind)
            End If

            Return False
        End Function

        Protected Function IsPointerToNumber(ByVal param As NativeParameter, ByRef bt As BuiltinType) As Boolean
            Dim paramType As NativeType = param.NativeTypeDigged
            If paramType.Kind = NativeSymbolKind.PointerType Then
                Dim pointerNt As NativePointer = DirectCast(paramType, NativePointer)
                If pointerNt.RealTypeDigged.Kind = NativeSymbolKind.BuiltinType Then
                    bt = DirectCast(pointerNt.RealTypeDigged, NativeBuiltinType).BuiltinType
                    If NativeBuiltinType.IsNumberType(bt) Then
                        Return True
                    End If
                End If
            End If

            Return False
        End Function

        ''' <summary>
        ''' Is this a pointer to a constant type.  IsConst can only be applied to named types
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function IsPointerToConst(ByVal type As NativeType) As Boolean
            type = type.DigThroughTypedefAndNamedTypes()
            If type.Kind <> NativeSymbolKind.PointerType Then
                Return False
            End If

            Dim ptr As NativePointer = DirectCast(type, NativePointer)
            Dim named As NativeNamedType = TryCast(ptr.RealType, NativeNamedType)
            If named IsNot Nothing AndAlso named.IsConst Then
                Return True
            End If

            Return False
        End Function

        Protected Function IsReturnProcessed(ByVal co As CodeMemberMethod) As Boolean
            Return co.UserData.Contains(ProcessedReturnKey)
        End Function

        Protected Sub SetReturnProcessed(ByVal co As CodeMemberMethod)
            ThrowIfTrue(IsReturnProcessed(co))
            co.UserData(ProcessedReturnKey) = True
        End Sub

        Protected Function IsParamProcessed(ByVal co As CodeParam) As Boolean
            Return co.UserData.Contains(ProcessedParamKey)
        End Function

        Protected Sub SetParamProcessed(ByVal co As CodeParam)
            ThrowIfTrue(IsParamProcessed(co))
            co.UserData(ProcessedParamKey) = True
        End Sub

        Protected Function IsMemberProcessed(ByVal co As CodeTypeMember) As Boolean
            Return co.UserData.Contains(ProcessedMemberKey)
        End Function

        Protected Sub SetMemberProcessed(ByVal co As CodeTypeMember)
            ThrowIfTrue(IsMemberProcessed(co))
            co.UserData(ProcessedMemberKey) = True
        End Sub

        ''' <summary>
        ''' Is this a HANDLE type
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function IsHandleType(ByVal nt As NativeType) As Boolean
            nt = nt.DigThroughNamedTypes()
            If nt.Kind = NativeSymbolKind.TypedefType _
                AndAlso 0 = String.CompareOrdinal(nt.Name, "HANDLE") Then
                Return True
            End If

            nt = nt.DigThroughTypedefAndNamedTypes()
            If nt.Kind = NativeSymbolKind.PointerType Then
                Dim realType As NativeType = DirectCast(nt, NativePointer).RealTypeDigged
                If realType.Name.StartsWith("H") AndAlso realType.Name.EndsWith("__") Then
                    Return True
                End If
            End If

            Return False
        End Function

        Protected Function IsVoidPointerType(ByVal nt As NativeType) As Boolean
            Dim bt As NativeBuiltinType = Nothing
            If IsPointerToBuiltin(nt, bt) AndAlso bt.BuiltinType = BuiltinType.NativeVoid Then
                Return True
            End If

            Return False
        End Function


        Protected Function IsWin32String(ByVal nt As NativeType, ByRef kind As CharSet) As Boolean
            Dim notUsed As Boolean = False
            Return IsWin32String(nt, kind, notUsed)
        End Function

        Protected Function IsWin32String(ByVal nt As NativeType, ByRef kind As CharSet, ByRef isConst As Boolean) As Boolean
            If nt Is Nothing Then
                Return False
            End If

            If nt.DigThroughTypedefAndNamedTypesFor("LPWSTR") IsNot Nothing Then
                kind = CharSet.Unicode
                isConst = False
                Return True
            End If

            If nt.DigThroughTypedefAndNamedTypesFor("LPCWSTR") IsNot Nothing Then
                kind = CharSet.Unicode
                isConst = True
                Return True
            End If

            If nt.DigThroughTypedefAndNamedTypesFor("LPTSTR") IsNot Nothing Then
                kind = CharSet.Auto
                isConst = False
                Return True
            End If

            If nt.DigThroughTypedefAndNamedTypesFor("LPCTSTR") IsNot Nothing Then
                kind = CharSet.Auto
                isConst = True
                Return True
            End If

            If nt.DigThroughTypedefAndNamedTypesFor("LPSTR") IsNot Nothing Then
                kind = CharSet.Ansi
                isConst = False
                Return True
            End If

            If nt.DigThroughTypedefAndNamedTypesFor("LPCSTR") IsNot Nothing Then
                kind = CharSet.Ansi
                isConst = True
                Return True
            End If

            Return False
        End Function

        Function IsBstr(ByVal nt As NativeType) As Boolean
            If nt Is Nothing Then
                Return False
            End If

            If nt.DigThroughTypedefAndNamedTypesFor("BSTR") IsNot Nothing Then
                Return True
            End If

            Return False
        End Function

#End Region

    End Class


#Region "Signature Transforms"

#Region "BooleanTypesTransformPlugin"

    ''' <summary>
    ''' Look for any boolean types and mark them appropriately.  
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class BooleanTypesTransformPlugin
        Inherits TransformPlugin

        Protected Overrides Sub ProcessReturnTypeImpl(ByVal codeMethod As CodeMemberMethod, ByVal retNt As NativeType, ByVal retNtSal As NativeSalAttribute)
            Dim bType As BooleanType = BooleanType.CStyle
            If retNt IsNot Nothing AndAlso IsBooleanType(retNt, bType) Then
                codeMethod.ReturnType = New CodeTypeReference(GetType(Boolean))
                codeMethod.ReturnTypeCustomAttributes.Clear()
                codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateBooleanMarshalAttribute(bType))
                SetReturnProcessed(codeMethod)
            End If
        End Sub

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)
            Dim bType As BooleanType = BooleanType.CStyle
            If IsBooleanType(ntParam.NativeType, bType) Then
                codeParam.Type = New CodeTypeReference(GetType(Boolean))
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateBooleanMarshalAttribute(bType))
                SetParamProcessed(codeParam)
            End If
        End Sub

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property
    End Class

#End Region

#Region "MutableStringBufferTransformPlugin"

    ''' <summary>
    ''' Whenever there is a char/wchar* that is not an In only parameter then it should be marshaled as 
    ''' a StringBuilder.  This allows the CLR to copy data back into it
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class MutableStringBufferTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)

            ' Make sure it's a string pointer
            Dim kind As CharSet = CharSet.None
            If Not IsPointerToCharType(ntParam, kind) Then
                Return
            End If

            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            If analyzer.IsOutElementBuffer() OrElse analyzer.IsOutElementBufferOptional() _
                OrElse analyzer.IsOutPartElementBuffer() OrElse analyzer.IsOutPartElementBufferOptional() _
                OrElse analyzer.IsInOutElementBuffer() Then

                ' Switch the parameter to be a StringBuilder 
                codeParam.Type = New CodeTypeReference(GetType(StringBuilder))
                codeParam.CustomAttributes.Clear()

                ' If this is an __out buffer then we should make sure to at the OutAttribute
                ' so that marshalling is more efficient
                If analyzer.IsValidOutOnly Then
                    codeParam.CustomAttributes.Add(CreateOutAttribute())
                End If

                codeParam.CustomAttributes.Add(CreateStringMarshalAttribute(kind))
                SetParamProcessed(codeParam)
            End If
        End Sub

    End Class

#End Region

#Region "ConstantStringTransformPlugin"

    ''' <summary>
    ''' If there is a char/wchar pointer which is In only then we should marshal it as a String with
    ''' an In Marshal attribute
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class ConstantStringTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)
            Dim paramType As NativeType = ntParam.NativeTypeDigged

            ' If this isn't a string pointer then we don't want to process it 
            Dim kind As CharSet = CharSet.None
            If Not IsPointerToCharType(ntParam, kind) Then
                Return
            End If

            ' Now determine if this is a constant param
            Dim isConst As Boolean = False
            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            If analyzer.IsIn() OrElse analyzer.IsInOptional() Then
                isConst = True
            ElseIf IsPointerToConst(paramType) Then
                isConst = True
            End If

            ' If it's a constant pointer to a string so add the information
            If isConst Then
                codeParam.Type = New CodeTypeReference(GetType(String))
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute)
                codeParam.CustomAttributes.Add(CreateStringMarshalAttribute(kind))
                SetParamProcessed(codeParam)
            End If
        End Sub
    End Class

#End Region

#Region "SystemIntTransformPlugin"

    ''' <summary>
    ''' There are several types in the system that are sized differently depending on the platform (x86,amd64) and
    ''' we need to marshal these types appropriately 
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class SystemIntTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature Or TransformKindFlags.WrapperMethods
            End Get
        End Property

        Protected Overrides Sub ProcessReturnTypeImpl(ByVal codeMethod As CodeMemberMethod, ByVal ntType As NativeType, ByVal ntSal As NativeSalAttribute)
            If ntType Is Nothing Then
                Return
            End If

            If Not IsSystemIntType(ntType) Then
                Return
            End If

            ' It's already an IntPtr so just return
            Dim returnType As CodeTypeReference = codeMethod.ReturnType
            If CodeDomUtil.IsIntPtrType(returnType) Then
                Return
            End If

            If IsType(returnType, GetType(UInt32)) Then
                codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysUInt))
            ElseIf IsType(returnType, GetType(Int32)) Then
                codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysInt))
            End If
        End Sub

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)
            If Not IsSystemIntType(ntParam) Then
                Return
            End If

            ' Whenever we have a delegate with a SysInt param we should marshal it as an IntPtr.  Trying to Marshal an Integer as 
            ' SysInt causes a lot of exceptions no matter what the combination is.  
            If isDelegateParam Then
                If Me.IsSystemIntType(ntParam) Then
                    codeParam.Type = New CodeTypeReference(GetType(IntPtr))
                    codeParam.CustomAttributes.Clear()
                End If
                Return
            End If

            ' If it's already an IntPtr we don't need to add any information
            If CodeDomUtil.IsIntPtrType(codeParam.Type) Then
                Return
            ElseIf CodeDomUtil.IsType(codeParam.Type, GetType(UInt32)) Then
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysUInt))
                SetParamProcessed(codeParam)
            ElseIf CodeDomUtil.IsType(codeParam.Type, GetType(Int32)) Then
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysInt))
                SetParamProcessed(codeParam)
            End If
        End Sub

        Protected Overrides Function ProcessWrapperMethodsImpl(ByVal codeMethod As System.CodeDom.CodeMemberMethod) As System.Collections.Generic.List(Of System.CodeDom.CodeMemberMethod)
            Dim list As New List(Of CodeMemberMethod)
            Dim clone As CodeMemberMethod

            clone = CloneWithPointer(codeMethod)
            If clone IsNot Nothing Then
                list.Add(clone)
            End If

            clone = CloneWithNonPointer(codeMethod)
            If clone IsNot Nothing Then
                list.Add(clone)
            End If

            Return list
        End Function

        Private Function CloneWithPointer(ByVal origMethod As CodeMemberMethod) As CodeMemberMethod
            Dim anyChanged As Boolean = False
            Dim clone As New CodeDomCloner
            Dim newMethod As CodeMemberMethod = clone.CloneMethodSignature(origMethod)
            For i As Integer = 0 To origMethod.Parameters.Count - 1
                Dim origParam As CodeParam = origMethod.Parameters(i)
                Dim newParam As CodeParam = newMethod.Parameters(i)
                Dim ntParam As NativeParameter = GetNativeParameter(origParam)
                If ntParam Is Nothing Then
                    Continue For
                End If

                If Not IsSystemIntType(ntParam) Then
                    Continue For
                End If

                If IsType(newParam.Type, GetType(UInt32)) OrElse IsType(newParam.Type, GetType(Int32)) Then
                    newParam.CustomAttributes.Clear()
                    newParam.Type = New CodeTypeReference(GetType(IntPtr))
                    anyChanged = True
                End If
            Next

            ' Check the return type
            Dim ret As NativeType = GetNativeReturnType(origMethod)
            If ret IsNot Nothing AndAlso IsSystemIntType(ret) Then
                If IsType(newMethod.ReturnType, GetType(Int32)) OrElse IsType(newMethod.ReturnType, GetType(UInt32)) Then
                    newMethod.ReturnType = New CodeTypeReference(GetType(IntPtr))
                    newMethod.ReturnTypeCustomAttributes.Clear()
                End If
            End If

            If anyChanged Then
                Return newMethod
            Else
                Return Nothing
            End If
        End Function

        Private Function CloneWithNonPointer(ByVal origMethod As CodeMemberMethod) As CodeMemberMethod
            Dim anyChanged As Boolean = False
            Dim clone As New CodeDomCloner
            Dim newMethod As CodeMemberMethod = clone.CloneMethodSignature(origMethod)
            For i As Integer = 0 To origMethod.Parameters.Count - 1
                Dim origParam As CodeParam = origMethod.Parameters(i)
                Dim newParam As CodeParam = newMethod.Parameters(i)
                Dim ntParam As NativeParameter = GetNativeParameter(origParam)
                If ntParam Is Nothing Then
                    Continue For
                End If

                If Not IsSystemIntType(ntParam) Then
                    Continue For
                End If

                If IsType(newParam.Type, GetType(IntPtr)) Then
                    newParam.CustomAttributes.Clear()
                    newParam.Type = New CodeTypeReference(GetType(UInt32))
                    newParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysUInt))
                    anyChanged = True
                End If
            Next

            ' Check the return type
            Dim ret As NativeType = GetNativeReturnType(origMethod)
            If ret IsNot Nothing AndAlso IsSystemIntType(ret) Then
                If IsType(origMethod.ReturnType, GetType(IntPtr)) Then
                    newMethod.ReturnType = New CodeTypeReference(GetType(UInt32))
                    newMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.SysUInt))
                End If
            End If

            If anyChanged Then
                Return newMethod
            Else
                Return Nothing
            End If
        End Function

    End Class

#End Region

#Region "ArrayParameterTransformPlugin"

    ''' <summary>
    ''' Turn IntPtr into Type[] where appropriate.  
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class ArrayParameterTransformPlugin
        Inherits TransformPlugin

        Private m_trans As CodeTransform

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Friend Sub New(ByVal trans As CodeTransform)
            m_trans = trans
        End Sub

        Protected Overrides Sub ProcessParametersImpl(ByVal col As System.CodeDom.CodeParameterDeclarationExpressionCollection, ByVal isDelegate As Boolean)
            If isDelegate Then
                Return
            End If

            For Each p As CodeParam In col
                If Not IsParamProcessed(p) Then
                    Dim ntParam As NativeParameter = GetNativeParameter(p)
                    If ntParam Is Nothing Then
                        Continue For
                    End If

                    ProcessParam(col, p, ntParam)
                End If
            Next

        End Sub

        Private Sub ProcessParam(ByVal col As CodeParamCollection, ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter)

            ' If this is not an array or it's an array of characters then return.  Array's of characters should 
            ' be marshaled as StringBuilders and is handled elsewhere
            Dim ct As CodeTypeReference = Nothing
            Dim ut As UnmanagedType = UnmanagedType.AnsiBStr
            Dim nt As NativeType = ntParam.NativeTypeDigged
            If nt.Kind = NativeSymbolKind.ArrayType Then
                If Not ProcessArray(DirectCast(nt, NativeArray), ct, ut) Then
                    Return
                End If
            ElseIf nt.Kind = NativeSymbolKind.PointerType Then
                If Not ProcessPointer(DirectCast(nt, NativePointer), ct, ut) Then
                    Return
                End If
            Else
                Return
            End If

            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            Dim size As String = Nothing
            If Not analyzer.IsInElementBuffer(size) Then
                Return
            End If

            ' Make sure this is not a void* element buffer.  We can't generated void[] into managed code
            ' so ignore it here and let it process as a normal array
            If CodeDomUtil.AreEqual(GetType(Void), ct) Then
                Return
            End If

            Dim sizeArg As CodeAttributeArgument = Nothing
            If Not TryGenerateSizeArgument(col, size, sizeArg) Then
                Return
            End If

            ' Finally, generate the attribute
            codeParam.Direction = FieldDirection.In
            codeParam.Type = New CodeTypeReference(ct, 1)
            codeParam.CustomAttributes.Clear()
            codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateArrayParamTypeAttribute(ut, sizeArg))
            SetParamProcessed(codeParam)
        End Sub

        Private Function TryGenerateSizeArgument(ByVal col As CodeParamCollection, ByVal size As String, ByRef arg As CodeAttributeArgument) As Boolean

            ' Easy on is just a count
            Dim count As Int32 = 0
            If Int32.TryParse(size, count) Then

                ' Don't process a size 1 element buffer.  That is not an array and will be handled by a 
                ' different plugin.  It will be converted to a ByRef
                If count = 1 Then
                    Return False
                End If

                arg = New CodeAttributeArgument("SizeConst", New CodePrimitiveExpression(count))
                Return True
            End If

            ' Now look for a named parameter
            For i As Integer = 0 To col.Count - 1
                Dim cur As CodeParam = col(i)
                If 0 = String.CompareOrdinal(size, cur.Name) Then
                    arg = New CodeAttributeArgument("SizeParamIndex", New CodePrimitiveExpression(i))
                    Return True
                End If
            Next

            Return False
        End Function

        Private Function ProcessArray(ByVal arr As NativeArray, ByRef elemType As CodeTypeReference, ByRef unmanagedType As UnmanagedType) As Boolean
            ThrowIfNull(arr)

            ' Don't process a char[]
            If IsArrayOfCharType(arr) Then
                Return False
            End If

            Dim bt As NativeBuiltinType = Nothing
            If IsArrayOfBuiltin(arr, bt) Then
                elemType = New CodeTypeReference(bt.ManagedType)
                unmanagedType = bt.UnmanagedType
                Return True
            ElseIf arr.RealTypeDigged.Kind = NativeSymbolKind.PointerType Then
                elemType = New CodeTypeReference(GetType(IntPtr))
                unmanagedType = Runtime.InteropServices.UnmanagedType.SysInt
                Return True
            ElseIf arr.RealTypeDigged.Kind = NativeSymbolKind.StructType _
                OrElse arr.RealTypeDigged.Kind = NativeSymbolKind.UnionType Then
                elemType = m_trans.GenerateTypeReference(arr.RealTypeDigged)
                unmanagedType = Runtime.InteropServices.UnmanagedType.Struct
                Return True
            End If

            Return False
        End Function

        Private Function ProcessPointer(ByVal ptr As NativePointer, ByRef elemType As CodeTypeReference, ByRef unmanagedType As UnmanagedType) As Boolean
            ThrowIfNull(ptr)

            ' Don't process a char*
            If IsPointerToCharType(ptr) Then
                Return False
            End If

            Dim bt As NativeBuiltinType = Nothing
            If IsPointerToBuiltin(ptr, bt) Then
                elemType = New CodeTypeReference(bt.ManagedType)
                unmanagedType = bt.UnmanagedType
                Return True
            ElseIf ptr.RealTypeDigged.Kind = NativeSymbolKind.PointerType Then
                elemType = New CodeTypeReference(GetType(IntPtr))
                unmanagedType = Runtime.InteropServices.UnmanagedType.SysInt
                Return True
            ElseIf ptr.RealTypeDigged.Kind = NativeSymbolKind.StructType _
                OrElse ptr.RealTypeDigged.Kind = NativeSymbolKind.UnionType Then

                elemType = m_trans.GenerateTypeReference(ptr.RealTypeDigged)
                unmanagedType = Runtime.InteropServices.UnmanagedType.Struct
                Return True
            End If

            Return False
        End Function

    End Class
#End Region

#Region "BetterManagedTypesTransformPlugin"

    ''' <summary>
    ''' Occassionally we built a better Managed type that will Marshal exactly as the underlying Native
    ''' type would.  In those cases we should use the Managed type 
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class BetterManagedTypesTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Protected Overrides Sub ProcessReturnTypeImpl(ByVal codeMethod As CodeMemberMethod, ByVal ntType As NativeType, ByVal ntSal As NativeSalAttribute)
            Dim codeType As CodeTypeReference = Nothing
            Dim codeAttrib As New CodeAttributeDeclarationCollection
            If HasBetterManagedType(ntType, codeType, codeAttrib, False) Then
                codeMethod.ReturnType = codeType
                codeMethod.ReturnTypeCustomAttributes.Clear()
                codeMethod.ReturnTypeCustomAttributes.AddRange(codeAttrib)
                SetReturnProcessed(codeMethod)
            End If
        End Sub

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)
            If ntParam Is Nothing OrElse ntParam.NativeType Is Nothing Then
                Return
            End If

            Dim codeType As CodeTypeReference = Nothing
            Dim codeAttrib As New CodeAttributeDeclarationCollection
            If HasBetterManagedType(ntParam.NativeType, codeType, codeAttrib, False) Then
                codeParam.Type = codeType
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.AddRange(codeAttrib)
                SetParamProcessed(codeParam)
                Return
            End If

            ' If this is a pointer type, see if the target type is a known structure
            Dim digged As NativeType = ntParam.NativeTypeDigged
            If digged Is Nothing OrElse digged.Kind <> NativeSymbolKind.PointerType Then
                Return
            End If

            Dim ptr As NativePointer = DirectCast(digged, NativePointer)
            If ptr.RealType IsNot Nothing AndAlso HasBetterManagedType(ptr.RealType, codeType, codeAttrib, True) Then

                Dim isValid As Boolean = True
                Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
                If analyzer.IsEmpty Then
                    codeParam.Direction = FieldDirection.Ref
                ElseIf analyzer.IsIn() Then
                    codeParam.Direction = FieldDirection.Ref
                    codeAttrib.Add(MarshalAttributeFactory.CreateInAttribute)
                ElseIf analyzer.IsOut() Then
                    codeParam.Direction = FieldDirection.Out
                Else
                    isValid = False
                End If

                If isValid Then
                    codeParam.Type = codeType
                    codeParam.CustomAttributes.Clear()
                    codeParam.CustomAttributes.AddRange(codeAttrib)
                    SetParamProcessed(codeParam)
                    Return
                End If
            End If
        End Sub

        Private Function HasBetterManagedType(ByVal ntType As NativeType, ByRef codeType As CodeTypeReference, ByVal codeAttrib As CodeAttributeDeclarationCollection, ByVal isPointer As Boolean) As Boolean
            Dim digged As NativeType = ntType.DigThroughTypedefAndNamedTypes()

            ' DECIMAL Structure.  There are no additional attributes necessary to Marshal this dataStructure
            If digged IsNot Nothing _
                AndAlso 0 = String.CompareOrdinal("tagDEC", digged.Name) _
                AndAlso digged.Kind = NativeSymbolKind.StructType Then

                codeType = New CodeTypeReference(GetType(Decimal))
                codeAttrib.Clear()
                Return True
            End If

            ' CURRENCY Structure.  Use the decimal type and Marshal it as a UnmanagedType.Currency.  
            If digged IsNot Nothing _
                AndAlso 0 = String.CompareOrdinal("tagCY", digged.Name) _
                AndAlso digged.Kind = NativeSymbolKind.UnionType Then

                codeType = New CodeTypeReference(GetType(Decimal))
                codeAttrib.Clear()
                codeAttrib.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.Currency))
                Return True
            End If

            If digged IsNot Nothing _
                AndAlso 0 = String.CompareOrdinal("GUID", digged.Name) _
                AndAlso digged.Kind = NativeSymbolKind.StructType Then

                codeType = New CodeTypeReference(GetType(Guid))
                codeAttrib.Clear()
                Return True
            End If

            ' WCHAR is best as a Char structure.  Don't ever Marshal this as a CHAR* though, all of the String
            ' logic code will do that
            Dim kind As CharSet = CharSet.None
            If Not isPointer AndAlso IsCharType(ntType, kind) AndAlso CharSet.Unicode = kind Then
                codeType = New CodeTypeReference(GetType(Char))
                codeAttrib.Clear()
                Return True
            End If

            Return False
        End Function

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class

#End Region

#Region "PointerToKnownTypes"

    ''' <summary>
    ''' In cases where we have a single pointer to a known type (say Int32) we want to generate
    ''' a ByRef param to the strong type rather than an IntPtr
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class PointerToKnownTypeTransformPlugin
        Inherits TransformPlugin

        Private m_trans As CodeTransform

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Friend Sub New(ByVal trans As CodeTransform)
            m_trans = trans
        End Sub

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)

            ' There are several kinds of structures where we don't want to do this optimization.  The
            ' most pertinent being handle types.  They have an ugly structure definition and 
            ' everyone would just rather use the IntPtr version
            If IsHandleType(ntParam.NativeType) OrElse IsVoidPointerType(ntParam.NativeType) Then
                Return
            End If

            ' If this is a pointer to a char type then don't convert it 
            If IsPointerToCharType(ntParam) Then
                Return
            End If

            ' Filter for Pointer types
            Dim paramType As NativeType = ntParam.NativeTypeDigged
            If paramType.Kind <> NativeSymbolKind.PointerType Then
                Return
            End If

            Dim realNt As NativeType = DirectCast(paramType, NativePointer).RealTypeDigged
            If Not (realNt.Category = NativeSymbolCategory.Defined OrElse realNt.Kind = NativeSymbolKind.BuiltinType) Then
                Return
            End If

            ' Look at the SAL attribute and make sure this is just a single element pointer
            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            Dim isSingle As Boolean = False
            Dim direction As FieldDirection

            If analyzer.IsEmpty Then
                ' If there are no SAL attributes then assume this is a simple out pointer
                isSingle = True
                direction = FieldDirection.Ref
            Else
                If analyzer.IsIn() Then
                    isSingle = True
                    codeParam.CustomAttributes.Clear()
                    codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute())
                    direction = FieldDirection.Ref
                ElseIf analyzer.IsOut() Then
                    isSingle = True
                    codeParam.CustomAttributes.Clear()
                    codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateOutAttribute())
                    direction = FieldDirection.Out
                ElseIf analyzer.IsInOut Then
                    isSingle = True
                    codeParam.CustomAttributes.Clear()
                    direction = FieldDirection.Ref
                Else
                    direction = FieldDirection.Ref
                End If
            End If

            If isSingle Then
                ' Found one
                codeParam.Type = m_trans.GenerateTypeReference(realNt)
                codeParam.Direction = direction
                SetParamProcessed(codeParam)
            End If
        End Sub

    End Class

#End Region

#Region "BStrTransformPlugin"

    ''' <summary>
    ''' Whenever a BSTR is used then change it to a String type
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class BstrTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Protected Overrides Sub ProcessReturnTypeImpl(ByVal codeMethod As System.CodeDom.CodeMemberMethod, ByVal ntType As NativeType, ByVal ntSal As NativeSalAttribute)
            If Not IsBstr(ntType) Then
                Return
            End If

            codeMethod.ReturnType = New CodeTypeReference(GetType(String))
            codeMethod.ReturnTypeCustomAttributes.Clear()
            codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr))
            SetReturnProcessed(codeMethod)
        End Sub

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)

            If IsBstr(ntParam.NativeType) Then
                ProcessBstrParam(codeParam, ntParam, isDelegateParam)
            End If

            Dim digged As NativeType = ntParam.NativeTypeDigged
            If digged IsNot Nothing _
                AndAlso digged.Kind = NativeSymbolKind.PointerType _
                AndAlso IsBstr(DirectCast(digged, NativePointer).RealType) Then

                ProcessBstrPointerParam(codeParam, ntParam, isDelegateParam)
            End If

        End Sub


        Private Sub ProcessBstrParam(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)
            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            If analyzer.IsEmpty Then
                codeParam.Type = New CodeTypeReference(GetType(String))
                codeParam.Direction = FieldDirection.In
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr))
            ElseIf analyzer.IsIn() Then
                codeParam.Type = New CodeTypeReference(GetType(String))
                codeParam.Direction = FieldDirection.In
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr))
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute())
            ElseIf analyzer.IsOut() Then
                codeParam.Type = New CodeTypeReference(GetType(String))
                codeParam.Direction = FieldDirection.Out
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr))
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateOutAttribute())
            Else
                ' We don't understand how to Marshal a BSTR that is not __in, default or __out therefore use
                ' an IntPtr.  Don't leave it as is or else it will get picked up as a normal String
                ' and be Marshalled incorrectly
                codeParam.Type = New CodeTypeReference(GetType(IntPtr))
                codeParam.Direction = FieldDirection.In
                codeParam.CustomAttributes.Clear()
            End If

            SetParamProcessed(codeParam)
        End Sub


        Private Sub ProcessBstrPointerParam(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)

            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            If analyzer.IsIn() Then
                codeParam.Type = New CodeTypeReference(GetType(String))
                codeParam.Direction = FieldDirection.Ref
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr))
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute())
            ElseIf analyzer.IsOut() Then
                codeParam.Type = New CodeTypeReference(GetType(String))
                codeParam.Direction = FieldDirection.Out
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr))
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateOutAttribute())
            Else
                codeParam.Type = New CodeTypeReference(GetType(String))
                codeParam.Direction = FieldDirection.Ref
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateUnmanagedTypeAttribute(UnmanagedType.BStr))
            End If

            SetParamProcessed(codeParam)
        End Sub


    End Class

#End Region

#Region "RawStringTransformPlugin"

    ''' <summary>
    ''' When there is a LP*STR member with no SAL annotations.  Go ahead and make it a StringBuilder parameter.  If it's const then
    ''' just make it a String param.  If there is a SAL attribute thet it will be handled by one of the plugin designed to handle 
    ''' SAL transforms
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class RawStringTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Protected Overrides Sub ProcessReturnTypeImpl(ByVal codeMethod As CodeMemberMethod, ByVal ntType As NativeType, ByVal ntSal As NativeSalAttribute)
            Dim analyzer As New SalAnalyzer(ntSal)
            If Not analyzer.IsEmpty Then
                Return
            End If

            Dim kind As CharSet = CharSet.None
            If Not IsWin32String(ntType, kind) Then
                Return
            End If

            codeMethod.ReturnType = New CodeTypeReference(GetType(String))
            codeMethod.ReturnTypeCustomAttributes.Clear()
            codeMethod.ReturnTypeCustomAttributes.Add(MarshalAttributeFactory.CreateStringMarshalAttribute(kind))
            SetReturnProcessed(codeMethod)
        End Sub

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)
            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            If Not analyzer.IsEmpty Then
                Return
            End If

            Dim kind As CharSet = CharSet.None
            Dim isConst As Boolean = False
            If Not IsWin32String(ntParam.NativeType, kind, isConst) Then
                Return
            End If

            If isConst Then
                codeParam.Type = New CodeTypeReference(GetType(String))
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateInAttribute())
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateStringMarshalAttribute(kind))
            Else
                codeParam.Type = New CodeTypeReference(GetType(StringBuilder))
                codeParam.CustomAttributes.Clear()
                codeParam.CustomAttributes.Add(MarshalAttributeFactory.CreateStringMarshalAttribute(kind))
            End If

            Me.SetParamProcessed(codeParam)
        End Sub

    End Class
#End Region

#Region "DoublePointerOutTransformPlugin"

    ''' <summary>
    ''' If we have a __deref_out parameter then go ahead and wrap it into an
    ''' Out IntPtr parameter.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class DoublePointerOutTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)
            If ntParam Is Nothing Then
                Return
            End If

            Dim type As NativeType = ntParam.NativeType.DigThroughTypedefAndNamedTypes()
            If type.Kind <> NativeSymbolKind.PointerType Then
                Return
            End If

            Dim target As NativeType = DirectCast(type, NativePointer).RealType.DigThroughTypedefAndNamedTypes()
            If target.Kind <> NativeSymbolKind.PointerType Then
                Return
            End If

            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            If Not analyzer.IsDerefOut() Then
                Return
            End If

            codeParam.Type = New CodeTypeReference(GetType(IntPtr))
            codeParam.Direction = FieldDirection.Out
            SetParamProcessed(codeParam)
        End Sub

    End Class


#End Region

#Region "PointerPointerTransformPlugin"

    ''' <summary>
    ''' Convert Pointer Pointers (**) into Out/Ref/In IntPtr
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class PointerPointerTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)
            If ntParam Is Nothing Then
                Return
            End If

            Dim type As NativeType = ntParam.NativeType.DigThroughTypedefAndNamedTypes()
            If type.Kind <> NativeSymbolKind.PointerType Then
                Return
            End If

            Dim target As NativeType = DirectCast(type, NativePointer).RealType.DigThroughTypedefAndNamedTypes()
            If target.Kind <> NativeSymbolKind.PointerType Then
                Return
            End If

            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            If analyzer.IsIn() Then
                Return
            End If

            codeParam.Type = New CodeTypeReference(GetType(IntPtr))
            codeParam.Direction = FieldDirection.Ref
            SetParamProcessed(codeParam)
        End Sub

    End Class

#End Region

#Region "DirectionalModifiersTransformPlugin"

    ''' <summary>
    ''' As a last effort, if the parameter has SAL information and doesn't meet any other transformation
    ''' then we will add the directional modifiers to the signature
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class DirectionalModifiersTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.Signature
            End Get
        End Property

        Protected Overrides Sub ProcessSingleParameter(ByVal codeParam As System.CodeDom.CodeParameterDeclarationExpression, ByVal ntParam As NativeParameter, ByVal isDelegateParam As Boolean)
            If ntParam Is Nothing Then
                Return
            End If

            ' Only apply directional attributes to pointers
            If Not AreEqual(GetType(IntPtr), codeParam.Type) AndAlso Not AreEqual(GetType(UIntPtr), codeParam.Type) Then
                Return
            End If

            Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
            If analyzer.IsValidInOnly Then
                codeParam.CustomAttributes.Add(CreateInAttribute)
            End If

            SetParamProcessed(codeParam)
        End Sub

    End Class

#End Region

#End Region

#Region "Wrapper Methods"

#Region "OneWayStringBufferTransformPlugin"

    ''' <summary>
    ''' Whenever we see a String paramater that is out only tied to an in size parameter
    ''' we should generate a wrapper allows the user to just deal with a single String value 
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class OneWayStringBufferTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.WrapperMethods
            End Get
        End Property

        Protected Overrides Function ProcessSingleWrapperMethod(ByVal codeMethod As System.CodeDom.CodeMemberMethod) As System.CodeDom.CodeMemberMethod
            Dim bufParam As CodeParam = Nothing
            Dim sizeParam As CodeParam = Nothing

            If Not FindBufferAndSizeParam(codeMethod, bufParam, sizeParam) Then
                Return Nothing
            End If

            Dim newMethod As CodeMemberMethod = GenerateWrapperSignature(codeMethod, bufParam, sizeParam)
            GenerateWrapperCode(newMethod, codeMethod, bufParam, sizeParam)
            Return newMethod
        End Function

        Private Function FindBufferAndSizeParam(ByVal codeMethod As CodeMemberMethod, ByRef bufParam As CodeParam, ByRef sizeParam As CodeParam) As Boolean
            sizeParam = Nothing
            bufParam = Nothing

            For Each param As CodeParam In codeMethod.Parameters

                If Not IsStringBuilderType(param.Type) Then
                    Continue For
                End If

                ' Check for a string pointer
                Dim ntParam As NativeParameter = GetNativeParameter(param)
                If Not IsPointerToCharType(ntParam) Then
                    Continue For
                End If

                ' See if this is an out element buffer
                Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
                Dim sizeParamName As String = Nothing
                If Not analyzer.IsOutElementBuffer(sizeParamName) _
                    AndAlso Not analyzer.IsOutElementBufferOptional(sizeParamName) Then
                    Continue For
                End If

                ' Now look for the size parameter
                For Each cur As CodeParam In codeMethod.Parameters
                    If cur IsNot param AndAlso 0 = String.CompareOrdinal(sizeParamName, cur.Name) Then
                        bufParam = param
                        sizeParam = cur
                        Return True
                    End If
                Next
            Next

            Return False
        End Function

        ''' <summary>
        ''' Used to generate the wrapper method
        ''' </summary>
        ''' <param name="origMethod"></param>
        ''' <param name="bufParam"></param>
        ''' <param name="sizeParam"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GenerateWrapperSignature(ByVal origMethod As CodeMemberMethod, ByVal bufParam As CodeParam, ByVal sizeParam As CodeParam) As CodeMemberMethod
            Dim clone As New CodeDomCloner
            Dim newMethod As New CodeMemberMethod()
            newMethod.Name = origMethod.Name
            newMethod.ReturnType = clone.CloneTypeReference(origMethod.ReturnType)
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateDebuggerStepThroughAttribute)
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateGeneratedCodeAttribute)
            newMethod.Attributes = MemberAttributes.Public Or MemberAttributes.Static
            For Each origParam As CodeParam In origMethod.Parameters
                If origParam Is bufParam Then
                    Dim newParam As New CodeParam()
                    newParam.Name = origParam.Name
                    newParam.Direction = FieldDirection.Out
                    newParam.Type = New CodeTypeReference(GetType(String))
                    newMethod.Parameters.Add(newParam)
                ElseIf origParam Is sizeParam Then
                    ' Don't need the size param in the new signature
                Else
                    newMethod.Parameters.Add(clone.CloneParamNoAttributes(origParam))
                End If
            Next

            Return newMethod
        End Function

        Private Sub GenerateWrapperCode(ByVal newMethod As CodeMemberMethod, ByVal origMethod As CodeMemberMethod, ByVal bufParam As CodeParam, ByVal sizeParam As CodeParam)
            ' Create the variables
            Dim sizeConst As Integer = 1024
            Dim bufVar As New CodeVariableDeclarationStatement( _
                New CodeTypeReference(GetType(StringBuilder)), _
                "var" & bufParam.Name)
            bufVar.InitExpression = New CodeObjectCreateExpression( _
                New CodeTypeReference(GetType(StringBuilder)), _
                New CodePrimitiveExpression(sizeConst))
            newMethod.Statements.Add(bufVar)
            Dim retVar As New CodeVariableDeclarationStatement( _
                newMethod.ReturnType, _
                "methodRetVar")
            newMethod.Statements.Add(retVar)

            ' Call the method 
            Dim args As New List(Of CodeExpression)
            For Each origParam As CodeParam In origMethod.Parameters
                If origParam Is bufParam Then
                    Dim varRef As CodeExpression = CodeDirectionalSymbolExpression.Create( _
                        Me.LanguageType, _
                        New CodeVariableReferenceExpression(bufVar.Name), _
                        origParam.Direction)
                    args.Add(varRef)
                ElseIf origParam Is sizeParam Then
                    args.Add(New CodePrimitiveExpression(sizeConst))
                Else
                    Dim varRef As CodeExpression = CodeDirectionalSymbolExpression.Create( _
                         Me.LanguageType, _
                         New CodeVariableReferenceExpression(origParam.Name), _
                         origParam.Direction)
                    args.Add(varRef)
                End If
            Next
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(retVar.Name), _
                New CodeMethodInvokeExpression( _
                    New CodeMethodReferenceExpression(New CodeTypeReferenceExpression(TransformConstants.NativeMethodsName), origMethod.Name), _
                    args.ToArray())))

            ' Assign the out string parameter
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(bufParam.Name), _
                New CodeMethodInvokeExpression( _
                    New CodeVariableReferenceExpression(bufVar.Name), _
                    "ToString")))

            ' Return the value
            newMethod.Statements.Add(New CodeMethodReturnStatement( _
                New CodeVariableReferenceExpression(retVar.Name)))
        End Sub

    End Class

#End Region

#Region "TwoWayStringBufferTransformPlugin"

    ''' <summary>
    ''' When we find a two way string buffer parameter this will generate the code to test the error
    ''' correction and recall the method.  Also it will generate a method with only a String parameter 
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class TwoWayStringBufferTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.WrapperMethods
            End Get
        End Property

        Protected Overrides Function ProcessSingleWrapperMethod(ByVal codeMethod As System.CodeDom.CodeMemberMethod) As System.CodeDom.CodeMemberMethod
            Dim bufParam As CodeParam = Nothing
            Dim sizeParam As CodeParam = Nothing

            If Not FindParams(codeMethod, bufParam, sizeParam) Then
                Return Nothing
            End If

            Dim newMethod As CodeMemberMethod = GenerateWrapperSignature(codeMethod, bufParam, sizeParam)
            GenerateWrapperCode(newMethod, codeMethod, bufParam, sizeParam)
            Return newMethod
        End Function

        Private Function FindParams(ByVal codeMethod As CodeMemberMethod, ByRef bufParam As CodeParam, ByRef sizeParam As CodeParam) As Boolean
            bufParam = Nothing
            sizeParam = Nothing
            For Each codeParam As CodeParam In codeMethod.Parameters
                Dim ntParam As NativeParameter = GetNativeParameter(codeParam)
                If ntParam Is Nothing Then
                    Continue For
                End If

                If Not IsPointerToCharType(ntParam) OrElse Not IsStringBuilderType(codeParam.Type) Then
                    Continue For
                End If

                Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
                Dim size As String = Nothing
                Dim readable As String = Nothing
                If Not analyzer.IsOutPartElementBuffer(size, readable) _
                    AndAlso Not analyzer.IsOutPartElementBufferOptional(size, readable) Then
                    Continue For
                End If

                ' Look for the popular pattern
                '  -> __out_ecount_part(*size,*size+1)
                Dim match As Match = Regex.Match(readable, "\*(\w+)\s*\+\s*\d+")
                If Not match.Success OrElse Not size.StartsWith("*") Then
                    Continue For
                End If

                Dim str1 As String = size.Substring(1)
                Dim str2 As String = match.Groups(1).Value
                If 0 <> String.CompareOrdinal(str1, str2) Then
                    Continue For
                End If

                ' Now we just have to find the parameter
                For Each codeParam2 As CodeParam In codeMethod.Parameters
                    If 0 = String.CompareOrdinal(codeParam2.Name, str1) Then
                        sizeParam = codeParam2
                        bufParam = codeParam
                        Return True
                    End If
                Next
            Next

            Return False
        End Function

        Private Function GenerateWrapperSignature(ByVal origMethod As CodeMemberMethod, ByVal bufParam As CodeParam, ByVal sizeParam As CodeParam) As CodeMemberMethod
            Dim clone As New CodeDomCloner
            Dim newMethod As New CodeMemberMethod()
            newMethod.Name = origMethod.Name
            newMethod.ReturnType = clone.CloneTypeReference(origMethod.ReturnType)
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateDebuggerStepThroughAttribute)
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateGeneratedCodeAttribute)
            newMethod.Attributes = MemberAttributes.Public Or MemberAttributes.Static
            For Each origParam As CodeParam In origMethod.Parameters
                If origParam Is bufParam Then
                    Dim newParam As New CodeParam()
                    newParam.Name = origParam.Name
                    newParam.Direction = FieldDirection.Ref
                    newParam.Type = New CodeTypeReference(GetType(String))
                    newMethod.Parameters.Add(newParam)
                ElseIf origParam Is sizeParam Then
                    ' Don't need the size param in the new signature
                Else
                    newMethod.Parameters.Add(clone.CloneParamNoAttributes(origParam))
                End If
            Next

            Return newMethod

        End Function

        Private Sub GenerateWrapperCode(ByVal newMethod As CodeMemberMethod, ByVal origMethod As CodeMemberMethod, ByVal bufParam As CodeParam, ByVal sizeParam As CodeParam)
            ' Create the variables
            Dim clone As New CodeDomCloner
            Dim jumpLabelName As String = "PerformCall"
            Dim bufVar As New CodeVariableDeclarationStatement( _
                New CodeTypeReference(GetType(StringBuilder)), _
                "var" & bufParam.Name)
            newMethod.Statements.Add(bufVar)
            Dim retVar As New CodeVariableDeclarationStatement( _
                clone.CloneTypeReference(origMethod.ReturnType), _
                "retVar_")
            newMethod.Statements.Add(retVar)
            Dim sizeVar As New CodeVariableDeclarationStatement( _
                clone.CloneTypeReference(sizeParam.Type), _
                "sizeVar", _
                New CodePrimitiveExpression(TransformConstants.DefaultBufferSize))
            newMethod.Statements.Add(sizeVar)
            Dim oldSizeVar As New CodeVariableDeclarationStatement( _
                clone.CloneTypeReference(sizeParam.Type), _
                "oldSizeVar_")
            newMethod.Statements.Add(oldSizeVar)

            ' Create the jump label
            newMethod.Statements.Add(New CodeLabeledStatement(jumpLabelName))

            ' Save the old size
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(oldSizeVar.Name), _
                New CodeVariableReferenceExpression(sizeVar.Name)))

            ' Create the buffer
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(bufVar.Name), _
                New CodeObjectCreateExpression( _
                    New CodeTypeReference(GetType(StringBuilder)), _
                    CodeDomUtil.ReferenceVarAsType(sizeVar, GetType(Int32)))))

            ' Perform the method call
            Dim args As New List(Of CodeExpression)
            For Each origParam As CodeParam In origMethod.Parameters
                Dim variableName As String
                If origParam Is bufParam Then
                    variableName = bufVar.Name
                ElseIf origParam Is sizeParam Then
                    variableName = sizeVar.Name
                Else
                    variableName = origParam.Name
                End If

                args.Add(CodeDirectionalSymbolExpression.Create( _
                    Me.LanguageType, _
                    New CodeVariableReferenceExpression(variableName), _
                    origParam.Direction))
            Next

            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(retVar.Name), _
                New CodeMethodInvokeExpression( _
                    New CodeMethodReferenceExpression(New CodeTypeReferenceExpression(TransformConstants.NativeMethodsName), origMethod.Name), _
                    args.ToArray())))

            ' Check the return of the call 
            Dim recallCheck As New CodeConditionStatement()
            recallCheck.Condition = New CodeBinaryOperatorExpression( _
                New CodeVariableReferenceExpression(oldSizeVar.Name), _
                CodeBinaryOperatorType.LessThanOrEqual, _
                New CodeVariableReferenceExpression(sizeVar.Name))

            ' Double the buffer
            recallCheck.TrueStatements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(sizeVar.Name), _
                New CodeBinaryOperatorExpression( _
                    New CodeVariableReferenceExpression(sizeVar.Name), _
                    CodeBinaryOperatorType.Multiply, _
                    CodeDomUtil.CreatePrimitiveAsType(2, sizeVar.Type))))

            ' Jump to the label
            recallCheck.TrueStatements.Add(New CodeGotoStatement(jumpLabelName))
            newMethod.Statements.Add(recallCheck)

            ' Save the return value
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(bufParam.Name), _
                New CodeMethodInvokeExpression( _
                    New CodeMethodReferenceExpression(New CodeVariableReferenceExpression(bufVar.Name), "ToString"))))

            ' Return the value
            newMethod.Statements.Add(New CodeMethodReturnStatement( _
                New CodeVariableReferenceExpression(retVar.Name)))
        End Sub

    End Class

#End Region

#Region "TwoWayViaReturnStringBufferTransformPlugin"

    ''' <summary>
    ''' When we find a two way string buffer parameter this will generate the code to test the error
    ''' correction and recall the method.  Also it will generate a method with only a String parameter 
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class TwoWayViaReturnStringBufferTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.WrapperMethods
            End Get
        End Property

        Protected Overrides Function ProcessSingleWrapperMethod(ByVal codeMethod As System.CodeDom.CodeMemberMethod) As System.CodeDom.CodeMemberMethod
            Dim bufParam As CodeParam = Nothing
            Dim sizeParam As CodeParam = Nothing
            Dim isPlusOne As Boolean = False

            If Not FindParams(codeMethod, bufParam, sizeParam, isPlusOne) Then
                Return Nothing
            End If

            Dim newMethod As CodeMemberMethod = GenerateWrapperSignature(codeMethod, bufParam, sizeParam)
            GenerateWrapperCode(newMethod, codeMethod, bufParam, sizeParam, isPlusOne)
            Return newMethod
        End Function

        Private Function FindParams(ByVal codeMethod As CodeMemberMethod, ByRef bufParam As CodeParam, ByRef sizeParam As CodeParam, ByRef isPlusOne As Boolean) As Boolean
            bufParam = Nothing
            sizeParam = Nothing
            For Each codeParam As CodeParam In codeMethod.Parameters
                Dim ntParam As NativeParameter = GetNativeParameter(codeParam)
                If ntParam Is Nothing Then
                    Continue For
                End If

                If Not IsPointerToCharType(ntParam) OrElse Not IsStringBuilderType(codeParam.Type) Then
                    Continue For
                End If

                Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
                Dim size As String = Nothing
                Dim readable As String = Nothing
                If Not analyzer.IsOutPartElementBuffer(size, readable) _
                    AndAlso Not analyzer.IsOutPartElementBufferOptional(size, readable) Then
                    Continue For
                End If

                ' Look for the popular pattern
                '  -> __out_ecount_part(*size,return+1)
                If Not readable.StartsWith("return") Then
                    Continue For
                ElseIf Regex.IsMatch(readable, "return\s*\+\s*1") Then
                    isPlusOne = True
                Else
                    isPlusOne = False
                End If

                If size.StartsWith("*") Then
                    size = size.Substring(1)
                End If

                ' Now we just have to find the parameter
                For Each codeParam2 As CodeParam In codeMethod.Parameters
                    If 0 = String.CompareOrdinal(codeParam2.Name, size) Then
                        sizeParam = codeParam2
                        bufParam = codeParam
                        Return True
                    End If
                Next
            Next

            Return False
        End Function

        Private Function GenerateWrapperSignature(ByVal origMethod As CodeMemberMethod, ByVal bufParam As CodeParam, ByVal sizeParam As CodeParam) As CodeMemberMethod
            Dim clone As New CodeDomCloner
            Dim newMethod As New CodeMemberMethod()
            newMethod.Name = origMethod.Name
            newMethod.ReturnType = clone.CloneTypeReference(origMethod.ReturnType)
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateDebuggerStepThroughAttribute)
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateGeneratedCodeAttribute)
            newMethod.Attributes = MemberAttributes.Public Or MemberAttributes.Static
            For Each origParam As CodeParam In origMethod.Parameters
                If origParam Is bufParam Then
                    Dim newParam As New CodeParam()
                    newParam.Name = origParam.Name
                    newParam.Direction = FieldDirection.Out
                    newParam.Type = New CodeTypeReference(GetType(String))
                    newMethod.Parameters.Add(newParam)
                ElseIf origParam Is sizeParam Then
                    ' Don't need the size param in the new signature
                Else
                    newMethod.Parameters.Add(clone.CloneParamNoAttributes(origParam))
                End If
            Next

            Return newMethod
        End Function

        Private Sub GenerateWrapperCode(ByVal newMethod As CodeMemberMethod, ByVal origMethod As CodeMemberMethod, ByVal bufParam As CodeParam, ByVal sizeParam As CodeParam, ByVal isPlusOne As Boolean)
            ' Create the variables
            Dim clone As New CodeDomCloner
            Dim jumpLabelName As String = "PerformCall"
            Dim bufVar As New CodeVariableDeclarationStatement( _
                New CodeTypeReference(GetType(StringBuilder)), _
                "var" & bufParam.Name)
            newMethod.Statements.Add(bufVar)
            Dim retVar As New CodeVariableDeclarationStatement( _
                clone.CloneTypeReference(origMethod.ReturnType), _
                "retVar_")
            newMethod.Statements.Add(retVar)
            Dim sizeVar As New CodeVariableDeclarationStatement( _
                clone.CloneTypeReference(origMethod.ReturnType), _
                "sizeVar", _
                New CodePrimitiveExpression(TransformConstants.DefaultBufferSize))
            newMethod.Statements.Add(sizeVar)

            ' Create the jump label
            newMethod.Statements.Add(New CodeLabeledStatement(jumpLabelName))

            ' Create the buffer
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(bufVar.Name), _
                New CodeObjectCreateExpression( _
                    New CodeTypeReference(GetType(StringBuilder)), _
                    CodeDomUtil.ReferenceVarAsType(sizeVar, GetType(Int32)))))

            ' Perform the method call
            Dim args As New List(Of CodeExpression)
            For Each origParam As CodeParam In origMethod.Parameters
                Dim variableName As String
                If origParam Is bufParam Then
                    variableName = bufVar.Name
                ElseIf origParam Is sizeParam Then
                    variableName = sizeVar.Name
                Else
                    variableName = origParam.Name
                End If

                args.Add(CodeDirectionalSymbolExpression.Create( _
                    Me.LanguageType, _
                    New CodeVariableReferenceExpression(variableName), _
                    origParam.Direction))
            Next

            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(retVar.Name), _
                New CodeMethodInvokeExpression( _
                    New CodeMethodReferenceExpression(New CodeTypeReferenceExpression(TransformConstants.NativeMethodsName), origMethod.Name), _
                    args.ToArray())))

            ' Check the return of the call 
            Dim recallCheck As New CodeConditionStatement()
            recallCheck.Condition = New CodeBinaryOperatorExpression( _
                New CodeVariableReferenceExpression(retVar.Name), _
                CodeBinaryOperatorType.GreaterThanOrEqual, _
                New CodeVariableReferenceExpression(sizeVar.Name))

            ' Assign the new buffer value 
            If isPlusOne Then
                recallCheck.TrueStatements.Add(New CodeAssignStatement( _
                    New CodeVariableReferenceExpression(sizeVar.Name), _
                    New CodeBinaryOperatorExpression( _
                        New CodeVariableReferenceExpression(retVar.Name), _
                        CodeBinaryOperatorType.Add, _
                        CodeDomUtil.CreatePrimitiveAsType(1, retVar.Type))))
            Else
                recallCheck.TrueStatements.Add(New CodeAssignStatement( _
                    New CodeVariableReferenceExpression(sizeVar.Name), _
                    New CodeVariableReferenceExpression(retVar.Name)))
            End If

            ' Jump to the label
            recallCheck.TrueStatements.Add(New CodeGotoStatement(jumpLabelName))
            newMethod.Statements.Add(recallCheck)

            ' Save the return value
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(bufParam.Name), _
                New CodeMethodInvokeExpression( _
                    New CodeMethodReferenceExpression(New CodeVariableReferenceExpression(bufVar.Name), "ToString"))))

            ' Return the value
            newMethod.Statements.Add(New CodeMethodReturnStatement( _
                New CodeVariableReferenceExpression(retVar.Name)))
        End Sub

    End Class

#End Region

#Region "PInvokePointerTransformPlugin"

    Friend Class PInvokePointerTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.WrapperMethods
            End Get
        End Property

        Protected Overrides Function ProcessSingleWrapperMethod(ByVal codeMethod As System.CodeDom.CodeMemberMethod) As System.CodeDom.CodeMemberMethod
            Dim bufParam As CodeParam = Nothing
            Dim sizeParam As CodeParam = Nothing
            If Not FindParams(codeMethod, bufParam, sizeParam) Then
                Return Nothing
            End If

            Dim newMethod As CodeMemberMethod = GenerateWrapperSignature(codeMethod, bufParam, sizeParam)
            GenerateWrapperCode(newMethod, codeMethod, bufParam, sizeParam)
            Return newMethod
        End Function

        Private Function FindParams(ByVal codeMethod As CodeMemberMethod, ByRef bufParam As CodeParam, ByRef sizeParam As CodeParam) As Boolean
            bufParam = Nothing
            sizeParam = Nothing

            For Each codeParam As CodeParam In codeMethod.Parameters
                Dim ntParam As NativeParameter = GetNativeParameter(codeParam)
                If ntParam Is Nothing Then
                    Continue For
                End If

                Dim ntType As NativeType = ntParam.NativeTypeDigged
                If ntType.Kind <> NativeSymbolKind.PointerType OrElse Not IsIntPtrType(codeParam.Type) Then
                    Continue For
                End If

                Dim analyzer As New SalAnalyzer(ntParam.SalAttribute)
                Dim str1 As String = Nothing
                Dim str2 As String = Nothing
                If Not analyzer.IsOutPartByteBuffer(str1, str2) Then
                    Continue For
                End If

                ' Look for the popular pattern
                '  -> __out_ecount_part(*size,*size)
                If Not str1.StartsWith("*") OrElse 0 <> String.CompareOrdinal(str1, str2) Then
                    Continue For
                End If

                str1 = str1.Substring(1)

                ' Now we just have to find the parameter
                For Each codeParam2 As CodeParameterDeclarationExpression In codeMethod.Parameters
                    If 0 = String.CompareOrdinal(codeParam2.Name, str1) Then
                        bufParam = codeParam
                        sizeParam = codeParam2
                        Return True
                    End If
                Next

            Next

            Return False
        End Function

        Private Function GenerateWrapperSignature(ByVal origMethod As CodeMemberMethod, ByVal bufParam As CodeParam, ByVal sizeParam As CodeParam) As CodeMemberMethod
            Dim clone As New CodeDomCloner
            Dim newMethod As New CodeMemberMethod()
            newMethod.Name = origMethod.Name
            newMethod.ReturnType = clone.CloneTypeReference(origMethod.ReturnType)
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateDebuggerStepThroughAttribute)
            newMethod.CustomAttributes.Add(MarshalAttributeFactory.CreateGeneratedCodeAttribute)
            newMethod.Attributes = MemberAttributes.Public Or MemberAttributes.Static
            For Each origParam As CodeParam In origMethod.Parameters
                If origParam Is bufParam Then
                    Dim newParam As New CodeParam()
                    newParam.Name = origParam.Name
                    newParam.Direction = FieldDirection.Out
                    newParam.Type = MarshalTypeFactory.CreatePInvokePointerCodeTypeReference()
                    newMethod.Parameters.Add(newParam)
                ElseIf origParam Is sizeParam Then
                    ' Don't need the size param in the new signature
                Else
                    newMethod.Parameters.Add(clone.CloneParamNoAttributes(origParam))
                End If
            Next

            Return newMethod
        End Function

        Private Sub GenerateWrapperCode(ByVal newMethod As CodeMemberMethod, ByVal origmethod As CodeMemberMethod, ByVal bufParam As CodeParam, ByVal sizeParam As CodeParam)
            ' Generate the variables
            Dim clone As New CodeDomCloner
            Dim jumpLabelName As String = "PerformCall"
            Dim bufVar As New CodeVariableDeclarationStatement( _
                MarshalTypeFactory.CreatePInvokePointerCodeTypeReference(), _
                "var" & bufParam.Name)
            newMethod.Statements.Add(bufVar)
            Dim retVar As New CodeVariableDeclarationStatement( _
                clone.CloneTypeReference(origmethod.ReturnType), _
                "retVar_")
            newMethod.Statements.Add(retVar)
            Dim sizeVar As New CodeVariableDeclarationStatement( _
                clone.CloneTypeReference(sizeParam.Type), _
                "sizeVar", _
                New CodePrimitiveExpression(TransformConstants.DefaultBufferSize))
            newMethod.Statements.Add(sizeVar)
            Dim oldSizeVar As New CodeVariableDeclarationStatement( _
                clone.CloneTypeReference(sizeParam.Type), _
                "oldSizeVar_")
            newMethod.Statements.Add(oldSizeVar)

            ' Create the jump label
            newMethod.Statements.Add(New CodeLabeledStatement(jumpLabelName))

            ' Save the old size
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(oldSizeVar.Name), _
                New CodeVariableReferenceExpression(sizeVar.Name)))

            ' Create the pointer
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(bufVar.Name), _
                New CodeObjectCreateExpression( _
                    MarshalTypeFactory.CreatePInvokePointerCodeTypeReference(), _
                    CodeDomUtil.ReferenceVarAsType(sizeVar, GetType(Int32)))))

            ' Perform the method call
            Dim args As New List(Of CodeExpression)
            For Each origParam As CodeParam In origmethod.Parameters
                If origParam Is bufParam Then
                    Dim memberRef As New CodeFieldReferenceExpression( _
                        New CodeVariableReferenceExpression(bufVar.Name), _
                        "IntPtr")
                    args.Add(CodeDirectionalSymbolExpression.Create( _
                        Me.LanguageType, _
                        memberRef, _
                        origParam.Direction))
                ElseIf origParam Is sizeParam Then
                    args.Add(CodeDirectionalSymbolExpression.Create( _
                        Me.LanguageType, _
                        New CodeVariableReferenceExpression(sizeVar.Name), _
                        origParam.Direction))
                Else
                    args.Add(CodeDirectionalSymbolExpression.Create( _
                        Me.LanguageType, _
                        New CodeVariableReferenceExpression(origParam.Name), _
                        origParam.Direction))
                End If
            Next
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(retVar.Name), _
                New CodeMethodInvokeExpression( _
                    New CodeMethodReferenceExpression(New CodeTypeReferenceExpression(TransformConstants.NativeMethodsName), origmethod.Name), _
                    args.ToArray())))

            ' Check the return of the call 
            Dim recallCheck As New CodeConditionStatement()
            recallCheck.Condition = New CodeBinaryOperatorExpression( _
                New CodeVariableReferenceExpression(sizeVar.Name), _
                CodeBinaryOperatorType.LessThanOrEqual, _
                New CodeVariableReferenceExpression(oldSizeVar.Name))

            ' Free the buffer
            recallCheck.TrueStatements.Add(New CodeMethodInvokeExpression( _
                New CodeVariableReferenceExpression(bufVar.Name), _
                "Free"))

            ' Double the size 
            recallCheck.TrueStatements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(sizeVar.Name), _
                New CodeBinaryOperatorExpression( _
                    New CodeVariableReferenceExpression(sizeVar.Name), _
                    CodeBinaryOperatorType.Multiply, _
                    CodeDomUtil.CreatePrimitiveAsType(2, sizeVar.Type))))

            ' Jump to the label
            recallCheck.TrueStatements.Add(New CodeGotoStatement(jumpLabelName))
            newMethod.Statements.Add(recallCheck)

            ' Save the pointer
            newMethod.Statements.Add(New CodeAssignStatement( _
                New CodeVariableReferenceExpression(bufParam.Name), _
                New CodeVariableReferenceExpression(bufVar.Name)))

            ' Return the value
            newMethod.Statements.Add(New CodeMethodReturnStatement( _
                New CodeVariableReferenceExpression(retVar.Name)))
        End Sub

    End Class

#End Region

#End Region

#Region "Struct Members"

#Region "StringBufferStructMemberTransformPlugin"

    Friend Class StringBufferStructMemberTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.StructMembers
            End Get
        End Property

        Protected Overrides Sub ProcessSingleStructField(ByVal ctd As System.CodeDom.CodeTypeDeclaration, ByVal codeField As System.CodeDom.CodeMemberField, ByVal ntMem As NativeMember)

            ' Analyze the type and look for a string buffer
            Dim foundCharSet As CharSet = CharSet.None
            If Not IsArrayOfCharType(ntMem.NativeType, foundCharSet) Then
                Return
            End If

            ' Look at the existing charset.  If it's different than the one we found then we can't do anything
            ' and should just bail out
            Dim existingCharset As CharSet = CharSet.Ansi
            If MyBase.IsCharsetSpecified(ctd, existingCharset) Then
                If existingCharset <> foundCharSet Then
                    Return
                End If
            Else
                AddCharSet(ctd, foundCharSet)
            End If

            ' Convert the types
            Dim arrayNt As NativeArray = DirectCast(ntMem.NativeTypeDigged, NativeArray)
            codeField.Type = New CodeTypeReference(GetType(String))
            codeField.CustomAttributes.Clear()

            Dim attr As New CodeAttributeDeclaration( _
                New CodeTypeReference(GetType(MarshalAsAttribute)))
            attr.Arguments.Add(New CodeAttributeArgument( _
                New CodeFieldReferenceExpression( _
                    New CodeTypeReferenceExpression(GetType(UnmanagedType)), _
                    "ByValTStr")))
            attr.Arguments.Add(New CodeAttributeArgument( _
                "SizeConst", _
                New CodePrimitiveExpression(arrayNt.ElementCount)))
            codeField.CustomAttributes.Add(attr)
            SetMemberProcessed(codeField)
        End Sub
    End Class

#End Region

#Region "StringPointerStructMemberTransformPlugin"

    ''' <summary>
    ''' If there is an IntPtr member of a structure to a String then marshal it as such
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class StringPointerStructMemberTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.StructMembers
            End Get
        End Property

        Protected Overrides Sub ProcessSingleStructField(ByVal ctd As System.CodeDom.CodeTypeDeclaration, ByVal field As System.CodeDom.CodeMemberField, ByVal ntMem As NativeMember)

            Dim foundCharSet As CharSet
            If Not IsPointerToCharType(ntMem, foundCharSet) Then
                Return
            End If

            field.Type = New CodeTypeReference(GetType(String))
            field.CustomAttributes.Clear()
            field.CustomAttributes.Add(CreateStringMarshalAttribute(foundCharSet))
            SetMemberProcessed(field)
        End Sub

    End Class

#End Region

#Region "BoolStructMemberTransformPlugin"

    ''' <summary>
    ''' Look for boolean types that are members of structures
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class BoolStructMemberTransformPlugin
        Inherits TransformPlugin

        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.StructMembers
            End Get
        End Property

        Protected Overrides Sub ProcessSingleStructField(ByVal ctd As System.CodeDom.CodeTypeDeclaration, ByVal field As System.CodeDom.CodeMemberField, ByVal ntMem As NativeMember)

            Dim retNt As NativeType = ntMem.NativeType
            Dim bType As BooleanType = BooleanType.CStyle
            If retNt IsNot Nothing AndAlso IsBooleanType(retNt, bType) Then
                field.Type = New CodeTypeReference(GetType(Boolean))
                field.CustomAttributes.Clear()
                field.CustomAttributes.Add(MarshalAttributeFactory.CreateBooleanMarshalAttribute(bType))
                SetMemberProcessed(field)
            End If
        End Sub
    End Class

#End Region

#End Region

#Region "Union Members"

#Region "BoolUnionMemberTransformPlugin"

    ''' <summary>
    ''' Look for boolean types that are members of a union
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class BoolUnionMemberTransformPlugin
        Inherits TransformPlugin


        Public Overrides ReadOnly Property TransformKind() As TransformKindFlags
            Get
                Return TransformKindFlags.UnionMembers
            End Get
        End Property

        Protected Overrides Sub ProcessSingleUnionField(ByVal ctd As System.CodeDom.CodeTypeDeclaration, ByVal field As System.CodeDom.CodeMemberField, ByVal ntMem As NativeMember)
            Dim nt As NativeType = ntMem.NativeType
            Dim bType As BooleanType = BooleanType.CStyle
            If nt IsNot Nothing AndAlso IsBooleanType(nt, bType) Then
                field.CustomAttributes.Add(MarshalAttributeFactory.CreateBooleanMarshalAttribute(BooleanType.CStyle))
                field.Type = New CodeTypeReference(GetType(Boolean))
                SetMemberProcessed(field)
            End If
        End Sub

    End Class

#End Region

#End Region

End Namespace
