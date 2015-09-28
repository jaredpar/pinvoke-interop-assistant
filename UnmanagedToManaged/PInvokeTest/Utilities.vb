' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.Text
Imports System.Collections.Generic
Imports PInvoke
Imports PInvoke.Transform
Imports Xunit

Public Module CodeDomPrinter

    Public Function ConvertNoNamespace(ByVal ref As CodeTypeReference) As String
        Dim name As String = ref.BaseType
        Dim index As Integer = name.LastIndexOf(".")
        If index < 0 Then
            Return name
        End If

        name = name.Substring(index + 1)
        If ref.ArrayRank > 0 Then
            name &= "(" & ref.ArrayRank & ")"
        End If

        Return name
    End Function

    Public Function Convert(ByVal type As CodeTypeReference) As String
        Dim name As String = type.BaseType
        If type.ArrayRank > 0 Then
            name &= "(" & type.ArrayRank & ")"
        End If

        Return name
    End Function

    Public Function Convert(ByVal expr As CodeExpression) As String
        Dim primitiveExpr As CodePrimitiveExpression = TryCast(expr, CodePrimitiveExpression)
        If primitiveExpr IsNot Nothing Then
            Return primitiveExpr.Value.ToString()
        End If

        Dim fieldExpr As CodeFieldReferenceExpression = TryCast(expr, CodeFieldReferenceExpression)
        If fieldExpr IsNot Nothing Then
            Return String.Format("{0}.{1}", _
                Convert(fieldExpr.TargetObject), _
                fieldExpr.FieldName)
        End If

        Dim typeExpr As CodeTypeReferenceExpression = TryCast(expr, CodeTypeReferenceExpression)
        If typeExpr IsNot Nothing Then
            Return ConvertNoNamespace(typeExpr.Type)
        End If

        Dim opExpr As CodeBinaryOperatorExpression = TryCast(expr, CodeBinaryOperatorExpression)
        If opExpr IsNot Nothing Then
            Return String.Format("{0}({1})({2})", _
                opExpr.Operator, _
                Convert(opExpr.Left), _
                Convert(opExpr.Right))
        End If

        Return expr.ToString()
    End Function

    Public Function Convert(ByVal col As CodeAttributeDeclarationCollection) As String
        Dim str As String = String.Empty
        Dim first As Boolean = True
        For Each decl As CodeAttributeDeclaration In col
            If Not first Then
                str &= ","
            End If

            str &= Convert(decl)
            first = False
        Next

        Return str
    End Function

    Public Function Convert(ByVal decl As CodeAttributeDeclaration) As String
        Dim builder As New StringBuilder

        builder.AppendFormat("{0}(", ConvertNoNamespace(decl.AttributeType))

        Dim first As Boolean = True
        For Each arg As CodeAttributeArgument In decl.Arguments
            If Not first Then
                builder.Append(",")
            End If
            If String.IsNullOrEmpty(arg.Name) Then
                builder.Append(Convert(arg.Value))
            Else
                builder.AppendFormat("{0}={1}", arg.Name, Convert(arg.Value))
            End If
            first = False
        Next

        builder.Append(")")
        Return builder.ToString()
    End Function

    Public Function Convert(ByVal method As CodeMemberMethod) As String
        Dim builder As New StringBuilder
        builder.Append(method.Name)
        builder.Append("(")

        Dim isFirst As Boolean = True
        For Each param As CodeParameterDeclarationExpression In method.Parameters
            If Not isFirst Then
                builder.Append(",")
            End If

            isFirst = False
            builder.Append(Convert(param.CustomAttributes))
            Select Case param.Direction
                Case FieldDirection.In
                    builder.Append("In ")
                Case FieldDirection.Out
                    builder.Append("Out ")
                Case FieldDirection.Ref
                    builder.Append("Ref ")
            End Select
            builder.Append(ConvertNoNamespace(param.Type))
        Next

        builder.Append(")")
        If method.ReturnType IsNot Nothing Then
            builder.Append(" As ")
            builder.Append(Convert(method.ReturnTypeCustomAttributes))
            builder.Append(ConvertNoNamespace(method.ReturnType))
        End If

        Return builder.ToString()
    End Function

    Public Function Convert(ByVal cField As CodeMemberField) As String
        Dim builder As New StringBuilder
        builder.Append(Convert(cField.CustomAttributes))
        builder.Append(Convert(cField.Type))
        builder.Append(" ")
        builder.Append(cField.Name)
        Return builder.ToString()
    End Function

End Module

Public Module SymbolPrinter

    Public Function Convert(ByVal sym As NativeSymbol) As String
        Dim str As String = sym.Name
        For Each child As NativeSymbol In sym.GetChildren()
            str &= "(" & Convert(child) & ")"
        Next

        Return str
    End Function
End Module

Public Module StorageFactory

    ''' <summary>
    ''' Used to create a simple set of types that can be used for testing purposes
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateStandard() As NativeStorage
        Dim ns As New NativeStorage()
        Dim pt1 As NativePointer
        Dim td1 As NativeTypeDef
        Dim td2 As NativeTypeDef
        Dim s1 As NativeStruct
        Dim u1 As NativeUnion
        Dim n1 As NativeNamedType

        ' Include sal information
        Dim list As List(Of NativeConstant) = ProcessSal()
        For Each cur As NativeConstant In list
            ns.AddConstant(cur)
        Next

        ' Bool types
        ns.AddTypedef(New NativeTypeDef("BOOL", BuiltinType.NativeInt32))
        ns.AddTypedef(New NativeTypeDef("DWORD", New NativeBuiltinType(BuiltinType.NativeInt32, True)))

        ' WPARAM 
        td1 = New NativeTypeDef("UINT_PTR", New NativeBuiltinType(BuiltinType.NativeInt32, True))
        ns.AddTypedef(New NativeTypeDef("WPARAM", td1))
        ns.AddTypedef(New NativeTypeDef("LPARAM", td1))

        ' WCHAR
        Dim wcharTd As New NativeTypeDef("WCHAR", New NativeBuiltinType(BuiltinType.NativeInt16, True))
        ns.AddTypedef(wcharTd)

        ' CHAR
        td1 = New NativeTypeDef("CHAR", BuiltinType.NativeChar)
        ns.AddTypedef(td1)

        ' TCHAR
        td2 = New NativeTypeDef("TCHAR", td1)
        ns.AddTypedef(td2)

        ' LPWSTR
        pt1 = New NativePointer(wcharTd)
        td2 = New NativeTypeDef("LPWSTR", pt1)
        ns.AddTypedef(td2)

        ' LPCWSTR
        n1 = New NativeNamedType(wcharTd.Name, wcharTd)
        n1.IsConst = True
        pt1 = New NativePointer(n1)
        td2 = New NativeTypeDef("LPCWSTR", pt1)
        ns.AddTypedef(td2)

        ' LPSTR
        pt1 = New NativePointer(New NativeBuiltinType(BuiltinType.NativeChar))
        td1 = New NativeTypeDef("LPSTR", pt1)
        ns.AddTypedef(td1)

        ' LPTSTR
        ns.AddTypedef(New NativeTypeDef("LPTSTR", td1))

        ' LPCSTR
        n1 = New NativeNamedType("char", True)
        n1.RealType = New NativeBuiltinType(BuiltinType.NativeChar, False)
        pt1 = New NativePointer(n1)
        td1 = New NativeTypeDef("LPCSTR", pt1)
        ns.AddTypedef(td1)

        ' LPCTSTR
        td2 = New NativeTypeDef("LPCTSTR", td1)
        ns.AddTypedef(td2)

        ' BSTR
        ns.AddTypedef(New NativeTypeDef("OLECHAR", BuiltinType.NativeWChar))
        ns.AddTypedef(New NativeTypeDef("BSTR", New NativePointer(New NativeTypeDef("OLECHAR", BuiltinType.NativeWChar))))

        ' Struct with a recrsive reference to itself
        s1 = New NativeStruct("RecursiveStruct")
        s1.Members.Add(New NativeMember("m1", New NativePointer(New NativeNamedType(s1.Name))))
        ns.AddDefinedType(s1)

        ' Simple struct
        s1 = New NativeStruct("S1")
        s1.Members.Add(New NativeMember("m1", New NativeBuiltinType(BuiltinType.NativeBoolean)))
        ns.AddDefinedType(s1)

        ' Simulate a few well known structures

        ' DECIMAL
        s1 = New NativeStruct("tagDEC")
        ns.AddDefinedType(s1)
        ns.AddTypedef(New NativeTypeDef("DECIMAL", s1))

        ' CURRENCY
        u1 = New NativeUnion("tagCY")
        ns.AddDefinedType(u1)
        ns.AddTypedef(New NativeTypeDef("CY", u1))
        ns.AddTypedef(New NativeTypeDef("CURRENCY", New NativeTypeDef("CY", u1)))

        ' BYTE
        ns.AddTypedef(new NativeTypeDef("BYTE", new NativeBuiltinType(BuiltinType.NativeChar, True)))

        ns.AcceptChanges()
        Return ns
    End Function

    Private Function ProcessSal() As List(Of NativeConstant)
        Dim analyzer As Parser.NativeCodeAnalyzer = Parser.NativeCodeAnalyzerFactory.Create(Parser.OsVersion.WindowsVista)
        Dim result As Parser.NativeCodeAnalyzerResult = analyzer.Analyze("specstrings.h")
        Return result.ConvertMacrosToConstants()
    End Function

End Module

Public Module GeneratedCodeVerification

    Public Sub VerifyExpression(ByVal nativeExpr As String, ByVal managedExpr As String)
        VerifyExpression(LanguageType.VisualBasic, nativeExpr, managedExpr)
    End Sub

    Public Sub VerifyExpression(ByVal nativeExpr As String, ByVal managedExpr As String, ByVal managedType As String)
        VerifyExpression(LanguageType.VisualBasic, nativeExpr, managedExpr, managedType)
    End Sub

    Public Sub VerifyCSharpExpression(ByVal nativeExpr As String, ByVal managedExpr As String, ByVal managedType As String)
        VerifyExpression(LanguageType.CSharp, nativeExpr, managedExpr, managedType)
    End Sub

    Public Sub VerifyExpression(ByVal lang As LanguageType, ByVal nativeExpr As String, ByVal managedExpr As String)
        VerifyExpression(lang, nativeExpr, managedExpr, Nothing)
    End Sub

    Public Sub VerifyExpression(ByVal lang As LanguageType, ByVal nativeExpr As String, ByVal managedExpr As String, ByVal managedType As String)
        Dim trans As New CodeTransform(lang)
        Dim nExpr As New NativeValueExpression(nativeExpr)
        Dim cExpr As CodeExpression = Nothing
        Dim codeType As CodeTypeReference = Nothing
        Dim ex As Exception = Nothing

        Assert.True(trans.TryGenerateValueExpression(nExpr, cExpr, codeType, ex))

        Dim provider As Compiler.CodeDomProvider
        Select Case lang
            Case LanguageType.CSharp
                provider = New Microsoft.CSharp.CSharpCodeProvider
            Case LanguageType.VisualBasic
                provider = New Microsoft.VisualBasic.VBCodeProvider
            Case Else
                provider = Nothing
        End Select

        Assert.NotNull(provider)
        Using writer As New IO.StringWriter
            provider.GenerateCodeFromExpression(cExpr, writer, New Compiler.CodeGeneratorOptions())
            Assert.Equal(managedExpr, writer.ToString())
        End Using

        If managedType IsNot Nothing Then
            Assert.Equal(managedType, CodeDomPrinter.Convert(codeType))
        End If
    End Sub

    Public Sub VerifyConstValue(ByVal code As String, ByVal name As String, ByVal val As String, ByVal type As String)
        VerifyConstValue(code, LanguageType.CSharp, name, val, type)
    End Sub

    Public Sub VerifyConstValue(ByVal code As String, ByVal lang As LanguageType, ByVal name As String, ByVal val As String, ByVal type As String)
        Dim col As CodeTypeDeclarationCollection = ConvertToCodeDom(code)
        VerifyConstValue(col, lang, name, val, type)
    End Sub

    Public Sub VerifyConstValue(ByVal bag As NativeSymbolBag, ByVal name As String, ByVal val As String)
        VerifyConstValue(LanguageType.VisualBasic, bag, name, val)
    End Sub

    Public Sub VerifyConstValue(ByVal bag As NativeSymbolBag, ByVal name As String, ByVal val As String, ByVal type As String)
        VerifyConstValue(LanguageType.VisualBasic, bag, name, val, type)
    End Sub

    Public Sub VerifyConstValue(ByVal lang As LanguageType, ByVal bag As NativeSymbolBag, ByVal name As String, ByVal val As String)
        VerifyConstValue(lang, bag, name, val, Nothing)
    End Sub

    Public Sub VerifyConstValue(ByVal lang As LanguageType, ByVal bag As NativeSymbolBag, ByVal name As String, ByVal val As String, ByVal type As String)
        Assert.True(bag.TryResolveSymbolsAndValues())

        Dim con As New BasicConverter()
        Dim col As CodeTypeDeclarationCollection = con.ConvertToCodeDom(bag, New ErrorProvider())

        VerifyConstValue(col, lang, name, val, type)
    End Sub

    Public Sub VerifyConstValue(ByVal col As CodeTypeDeclarationCollection, ByVal lang As LanguageType, ByVal name As String, ByVal val As String, ByVal type As String)

        ' Look for the constants class
        Dim ctd As CodeTypeDeclaration = Nothing
        VerifyType(col, TransformConstants.NativeConstantsName, ctd)

        ' Find the value
        Dim cMem As CodeTypeMember = Nothing
        VerifyMember(ctd, name, cMem)

        ' Make sure it's a constant value
        Dim field As CodeMemberField = TryCast(cMem, CodeMemberField)
        Assert.NotNull(field)

        ' Get the provider
        Dim provider As Compiler.CodeDomProvider
        Select Case lang
            Case LanguageType.CSharp
                provider = New Microsoft.CSharp.CSharpCodeProvider
            Case LanguageType.VisualBasic
                provider = New Microsoft.VisualBasic.VBCodeProvider
            Case Else
                provider = Nothing
        End Select

        Using writer As New IO.StringWriter
            provider.GenerateCodeFromExpression(field.InitExpression, writer, New Compiler.CodeGeneratorOptions())
            Assert.Equal(val, writer.ToString())
        End Using

        If type IsNot Nothing Then
            Assert.Equal(type, CodeDomPrinter.Convert(field.Type))
        End If
    End Sub

    Public Sub VerifyEnumValue(ByVal bag As NativeSymbolBag, ByVal e As NativeEnum, ByVal name As String, ByVal val As String)
        VerifyEnumValue(LanguageType.VisualBasic, bag, e, name, val)
    End Sub

    Public Sub VerifyEnumValue(ByVal lang As LanguageType, ByVal bag As NativeSymbolBag, ByVal e As NativeEnum, ByVal name As String, ByVal val As String)
        Assert.True(bag.TryResolveSymbolsAndValues())

        Dim con As New BasicConverter()
        Dim col As CodeTypeDeclarationCollection = con.ConvertToCodeDom(bag, New ErrorProvider())

        ' Look for the constants class
        Dim ctd As CodeTypeDeclaration = Nothing
        For Each cur As CodeTypeDeclaration In col
            If 0 = String.CompareOrdinal(e.Name, cur.Name) Then
                ctd = cur
                Exit For
            End If
        Next
        Assert.NotNull(ctd)

        ' Find the value
        Dim cMem As CodeTypeMember = Nothing
        For Each mem As CodeTypeMember In ctd.Members
            If 0 = String.CompareOrdinal(name, mem.Name) Then
                cMem = mem
                Exit For
            End If
        Next
        Assert.NotNull(cMem)

        ' Make sure it's a constant value
        Dim field As CodeMemberField = TryCast(cMem, CodeMemberField)
        Assert.NotNull(field)

        ' Get the provider
        Dim provider As Compiler.CodeDomProvider
        Select Case lang
            Case LanguageType.CSharp
                provider = New Microsoft.CSharp.CSharpCodeProvider
            Case LanguageType.VisualBasic
                provider = New Microsoft.VisualBasic.VBCodeProvider
            Case Else
                provider = Nothing
        End Select

        Using writer As New IO.StringWriter
            provider.GenerateCodeFromExpression(field.InitExpression, writer, New Compiler.CodeGeneratorOptions())
            Assert.Equal(val, writer.ToString())
        End Using
    End Sub

    Private Function ConvertToCodeDom(ByVal code As String) As CodeTypeDeclarationCollection
        Dim ep As New ErrorProvider()
        Dim con As New BasicConverter(LanguageType.VisualBasic)
        Dim result As CodeTypeDeclarationCollection = con.ConvertNativeCodeToCodeDom(code, ep)
        Assert.Equal(0, ep.Errors.Count)
        Return result
    End Function

    Public Sub VerifyType(ByVal col As CodeTypeDeclarationCollection, ByVal name As String, ByRef ctd As CodeTypeDeclaration)
        ctd = Nothing
        For Each cur As CodeTypeDeclaration In col
            If 0 = String.CompareOrdinal(cur.Name, name) Then
                ctd = cur
                Exit For
            End If
        Next

        If ctd Is Nothing Then
            Dim msg As String = "Could not find a type named " & name & ".  Found: "
            For Each type As CodeTypeDeclaration In col
                msg &= type.Name & " "
            Next
            Throw New Exception(msg)
        End If
    End Sub

    Private Function ConvertToProc(ByVal code As String) As List(Of CodeMemberMethod)
        Dim col As CodeTypeDeclarationCollection = ConvertToCodeDom(code)
        Dim list As New List(Of CodeMemberMethod)
        Dim ctd As CodeTypeDeclaration = Nothing

        VerifyType(col, "NativeMethods", ctd)
        For Each mem As CodeTypeMember In ctd.Members
            Dim method As CodeMemberMethod = TryCast(mem, CodeMemberMethod)
            If method IsNot Nothing Then
                list.Add(method)
            End If
        Next

        Return list
    End Function

    Private Function ConvertToSingleProc(ByVal code As String, ByVal name As String) As CodeMemberMethod
        Dim found As CodeMemberMethod = Nothing
        For Each cur As CodeMemberMethod In ConvertToProc(code)
            If String.Equals(name, cur.Name, StringComparison.Ordinal) Then
                found = cur
                Exit For
            End If
        Next

        Assert.NotNull(found)
        Return found
    End Function

    Private Function VerifyProcImpl(ByVal code As String, ByVal sig As String, ByRef all As String) As Boolean
        all = String.Empty
        For Each method As CodeMemberMethod In ConvertToProc(code)
            Dim p As String = CodeDomPrinter.Convert(method)
            If 0 = String.CompareOrdinal(sig, p) Then
                Return True
            Else
                all &= Environment.NewLine
                all &= p
            End If
        Next

        Return False
    End Function

    Public Sub VerifyProc(ByVal code As String, ByVal ParamArray sigArray As String())
        Dim all As String = String.Empty
        For Each sig As String In sigArray
            Dim ret As Boolean = VerifyProcImpl(code, sig, all)
            Assert.True(ret, "Could not find the method. Looking For :" & sig & vbCrLf & "Found:" & all)
        Next
    End Sub

    Public Sub VerifyNotProc(ByVal code As String, ByVal sig As String)
        Dim all As String = String.Empty
        Dim ret As Boolean = VerifyProcImpl(code, sig, all)
        Assert.True(Not ret, "Found a matching method")
    End Sub

    Public Sub VerifyMember(ByVal ctd As CodeTypeDeclaration, ByVal name As String, ByRef cMem As CodeTypeMember)
        ' Find the value
        cMem = Nothing
        For Each mem As CodeTypeMember In ctd.Members
            If 0 = String.CompareOrdinal(name, mem.Name) Then
                cMem = mem
                Exit For
            End If
        Next

        Assert.NotNull(cMem)
    End Sub

    Public Sub VerifyAttribute(ByVal col As CodeAttributeDeclarationCollection, ByVal type As Type, ByRef decl As CodeAttributeDeclaration)
        VerifyAttributeImpl(col, type, decl)
        Assert.NotNull(decl)
    End Sub

    Public Sub VerifyNoAttribute(ByVal col As CodeAttributeDeclarationCollection, ByVal type As Type)
        Dim decl As CodeAttributeDeclaration = Nothing
        VerifyAttributeImpl(col, type, decl)
        Assert.Null(decl)
    End Sub

    Private Sub VerifyAttributeImpl(ByVal col As CodeAttributeDeclarationCollection, ByVal type As Type, ByRef decl As CodeAttributeDeclaration)
        decl = Nothing

        Dim name As String = type.FullName
        For Each cur As CodeAttributeDeclaration In col
            If String.Equals(name, cur.Name, StringComparison.Ordinal) Then
                decl = cur
                Exit For
            End If
        Next

    End Sub

    Private Sub VerifyArgumentImpl(ByVal decl As CodeAttributeDeclaration, ByVal name As String, ByRef arg As CodeAttributeArgument)
        arg = Nothing
        For Each cur As CodeAttributeArgument In decl.Arguments
            If String.Equals(name, cur.Name, StringComparison.Ordinal) Then
                arg = cur
                Exit For
            End If
        Next
    End Sub

    Public Sub VerifyArgument(ByVal decl As CodeAttributeDeclaration, ByVal name As String, ByRef arg As CodeAttributeArgument)
        VerifyArgumentImpl(decl, name, arg)
        Assert.NotNull(arg)
    End Sub

    Public Sub VerifyNoArgument(ByVal decl As CodeAttributeDeclaration, ByVal name As String)
        Dim arg As CodeAttributeArgument = Nothing
        VerifyArgumentImpl(decl, name, arg)
        Assert.Null(arg)
    End Sub

    Public Sub VerifyField(ByVal ctd As CodeTypeDeclaration, ByVal name As String, ByRef cField As CodeMemberField)
        Dim cMem As CodeTypeMember = Nothing
        VerifyMember(ctd, name, cMem)
        cField = TryCast(cMem, CodeMemberField)
        Assert.NotNull(cField)
    End Sub

    Public Sub VerifyField(ByVal ctd As CodeTypeDeclaration, ByVal name As String, ByVal value As String)
        Dim cField As CodeMemberField = Nothing
        VerifyField(ctd, name, cField)
        Assert.Equal(value, CodeDomPrinter.Convert(cField))
    End Sub

    Public Sub VerifyTypeMembers(ByVal code As String, ByVal name As String, ByVal ParamArray members() As String)
        Dim col As CodeTypeDeclarationCollection = ConvertToCodeDom(code)
        Dim ctd As CodeTypeDeclaration = Nothing
        VerifyType(col, name, ctd)

        For i As Integer = 0 To members.Length - 1 Step 2
            VerifyField(ctd, members(i), members(i + 1))
        Next

    End Sub

    Public Sub VerifyProcCallingConvention(ByVal code As String, ByVal name As String, ByVal conv As System.Runtime.InteropServices.CallingConvention)
        Dim mem As CodeMemberMethod = ConvertToSingleProc(code, name)
        Dim decl As CodeAttributeDeclaration = Nothing
        VerifyAttribute(mem.CustomAttributes, GetType(Runtime.InteropServices.DllImportAttribute), decl)
        If conv = Runtime.InteropServices.CallingConvention.Winapi Then
            VerifyNoArgument(decl, "CallingConvention")
        Else
            Dim arg As CodeAttributeArgument = Nothing
            VerifyArgument(decl, "CallingConvention", arg)
            Assert.Equal("CallingConvention." & conv.ToString(), CodeDomPrinter.Convert(arg.Value))
        End If
    End Sub

    Public Sub VerifyFPtrCallingConvention(ByVal code As String, ByVal name As String, ByVal conv As System.Runtime.InteropServices.CallingConvention)
        Dim col As CodeTypeDeclarationCollection = ConvertToCodeDom(code)
        Dim type As CodeTypeDeclaration = Nothing
        VerifyType(col, name, type)

        If conv <> Runtime.InteropServices.CallingConvention.Winapi Then

            Dim decl As CodeAttributeDeclaration = Nothing
            VerifyAttribute(type.CustomAttributes, GetType(Runtime.InteropServices.UnmanagedFunctionPointerAttribute), decl)

            Dim arg As CodeAttributeArgument = Nothing
            VerifyArgument(decl, String.Empty, arg)
            Assert.Equal("CallingConvention." & conv.ToString(), CodeDomPrinter.Convert(arg.Value))
        Else
            VerifyNoAttribute(type.CustomAttributes, GetType(Runtime.InteropServices.UnmanagedFunctionPointerAttribute))
        End If
    End Sub

End Module
