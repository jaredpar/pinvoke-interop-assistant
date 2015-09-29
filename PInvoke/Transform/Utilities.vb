' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports CodeParam = System.CodeDom.CodeParameterDeclarationExpression

Namespace Transform

#Region "LanguageType"

    ''' <summary>
    ''' Supported language types to export as
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum LanguageType
        VisualBasic
        CSharp
    End Enum

#End Region

#Region "MarshalAttributeFactory"

    Public Enum StringKind
        Ascii
        Unicode
        Unknown
    End Enum

    Public Enum BooleanType
        Windows
        CStyle
        [Variant]
    End Enum

    Friend Module MarshalAttributeFactory

        Friend Function CreateInAttribute() As CodeAttributeDeclaration
            Dim decl As New CodeAttributeDeclaration( _
                New CodeTypeReference(GetType(InAttribute)))
            Return decl
        End Function

        Friend Function CreateOutAttribute() As CodeAttributeDeclaration
            Dim decl As New CodeAttributeDeclaration( _
                New CodeTypeReference(GetType(OutAttribute)))
            Return decl
        End Function

        ''' <summary>
        ''' Create an attribute to marshal the boolean type
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function CreateBooleanMarshalAttribute(ByVal type As BooleanType) As CodeAttributeDeclaration

            Dim ut As UnmanagedType
            Select Case type
                Case BooleanType.CStyle
                    ut = UnmanagedType.I1
                Case BooleanType.Windows
                    ut = UnmanagedType.Bool
                Case BooleanType.Variant
                    ut = UnmanagedType.VariantBool
                Case Else
                    InvalidEnumValue(type)
                    ut = UnmanagedType.AnsiBStr
            End Select

            Return CreateUnmanagedTypeAttribute(ut)
        End Function

        Friend Function CreateUnmanagedTypeAttribute(ByVal type As UnmanagedType) As CodeAttributeDeclaration
            Dim decl As New CodeAttributeDeclaration( _
                          New CodeTypeReference(GetType(MarshalAsAttribute)))
            decl.Arguments.Add(New CodeAttributeArgument( _
                New CodeFieldReferenceExpression( _
                    New CodeTypeReferenceExpression(GetType(UnmanagedType)), _
                    type.ToString())))
            Return decl
        End Function

        Friend Function CreateArrayParamTypeAttribute(ByVal arraySubType As UnmanagedType, ByVal sizeArg As CodeAttributeArgument) As CodeAttributeDeclaration
            Dim decl As CodeAttributeDeclaration = CreateUnmanagedTypeAttribute(UnmanagedType.LPArray)
            decl.Arguments.Add(New CodeAttributeArgument( _
                "ArraySubType", _
                New CodeFieldReferenceExpression( _
                    New CodeTypeReferenceExpression(GetType(UnmanagedType)), _
                    arraySubType.ToString())))
            decl.Arguments.Add(sizeArg)
            Return decl
        End Function

        Friend Function CreateStringMarshalAttribute(ByVal strType As CharSet) As CodeAttributeDeclaration
            Dim decl As New CodeAttributeDeclaration( _
                New CodeTypeReference(GetType(MarshalAsAttribute)))
            Dim field As String
            Select Case strType
                Case CharSet.Ansi
                    field = "LPStr"
                Case CharSet.Unicode
                    field = "LPWStr"
                Case CharSet.Auto
                    field = "LPTStr"
                Case Else
                    InvalidEnumValue(strType)
                    field = Nothing
            End Select

            decl.Arguments.Add(New CodeAttributeArgument( _
                New CodeFieldReferenceExpression( _
                    New CodeTypeReferenceExpression(GetType(UnmanagedType)), _
                    field)))
            Return decl
        End Function

        Friend Function CreateDebuggerStepThroughAttribute() As CodeAttributeDeclaration
            Return New CodeAttributeDeclaration( _
                New CodeTypeReference(GetType(DebuggerStepThroughAttribute)))
        End Function

        Friend Function CreateGeneratedCodeAttribute() As CodeAttributeDeclaration
            Dim decl As New CodeAttributeDeclaration( _
                New CodeTypeReference(GetType(System.CodeDom.Compiler.GeneratedCodeAttribute)))
            decl.Arguments.Add(New CodeAttributeArgument(New CodePrimitiveExpression(PInvoke.Constants.ProductName)))
            decl.Arguments.Add(New CodeAttributeArgument(New CodePrimitiveExpression(PInvoke.Constants.FriendlyVersion)))
            Return decl
        End Function

        ''' <summary>
        ''' Generate the StructLayoutAttribute attribute
        ''' </summary>
        ''' <param name="kind"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function CreateStructLayoutAttribute(ByVal kind As LayoutKind) As CodeAttributeDeclaration
            Dim attrRef As New CodeTypeReference(GetType(Runtime.InteropServices.StructLayoutAttribute))
            Dim attr As New CodeAttributeDeclaration( _
                New CodeTypeReference(GetType(StructLayoutAttribute)))
            attr.Arguments.Add(New CodeAttributeArgument( _
                New CodeFieldReferenceExpression( _
                    New CodeTypeReferenceExpression(GetType(LayoutKind)), _
                    kind.ToString())))
            Return attr
        End Function

        ''' <summary>
        ''' Create the FieldOffsetAttribute for the specified value
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function CreateFieldOffsetAttribute(ByVal value As Int32) As CodeAttributeDeclaration
            Dim attr As New CodeAttributeDeclaration(New CodeTypeReference(GetType(Runtime.InteropServices.FieldOffsetAttribute)))
            attr.Arguments.Add(New CodeAttributeArgument(New CodePrimitiveExpression(value)))
            Return attr
        End Function

        ''' <summary>
        ''' Create a DllImport attribute 
        ''' </summary>
        ''' <param name="dllName"></param>
        ''' <param name="entryPoint"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function CreateDllImportAttribute(ByVal dllName As String, ByVal entryPoint As String, ByVal conv As NativeCallingConvention) As CodeAttributeDeclaration
            Dim attr As New CodeAttributeDeclaration(New CodeTypeReference(GetType(Runtime.InteropServices.DllImportAttribute)))
            attr.Arguments.Add(New CodeAttributeArgument(New CodePrimitiveExpression(dllName)))
            attr.Arguments.Add(New CodeAttributeArgument( _
                "EntryPoint", _
                New CodePrimitiveExpression(entryPoint)))

            ' If the calling convention for a DLl is not the standard WinApi then output the extra information
            ' into the attribute
            Dim kind As CallingConvention
            If TryConvertToInteropCallingConvention(conv, kind) AndAlso kind <> CallingConvention.Winapi Then
                attr.Arguments.Add(New CodeAttributeArgument( _
                    "CallingConvention", _
                    New CodeFieldReferenceExpression( _
                        New CodeTypeReferenceExpression(GetType(CallingConvention)), _
                        kind.ToString())))
            End If

            Return attr
        End Function

        Friend Function CreateUnmanagedFunctionPointerAttribute(ByVal conv As NativeCallingConvention) As CodeAttributeDeclaration
            Dim attr As New CodeAttributeDeclaration(New CodeTypeReference(GetType(Runtime.InteropServices.UnmanagedFunctionPointerAttribute)))
            Dim kind As CallingConvention
            If TryConvertToInteropCallingConvention(conv, kind) Then
                attr.Arguments.Add(New CodeAttributeArgument( _
                    New CodeFieldReferenceExpression( _
                        New CodeTypeReferenceExpression(GetType(CallingConvention)), _
                        kind.ToString())))
            End If

            Return attr
        End Function

        Private Function TryConvertToInteropCallingConvention(ByVal conv As NativeCallingConvention, ByRef kind As CallingConvention) As Boolean
            Select Case conv
                Case NativeCallingConvention.WinApi
                    kind = CallingConvention.Winapi
                    Return True
                Case NativeCallingConvention.CDeclaration
                    kind = CallingConvention.Cdecl
                    Return True
                Case NativeCallingConvention.Standard
                    kind = CallingConvention.StdCall
                    Return True
                Case Else
                    Return False
            End Select
        End Function


    End Module
#End Region

#Region "CodeDomUtil"

    Friend Module CodeDomUtil

        Friend Function IsStringBuilderType(ByVal typeRef As CodeTypeReference) As Boolean
            Return IsType(typeRef, GetType(StringBuilder))
        End Function

        Friend Function IsIntPtrType(ByVal typeRef As CodeTypeReference) As Boolean
            Return IsType(typeRef, GetType(IntPtr))
        End Function

        Friend Function IsType(ByVal typeRef As CodeTypeReference, ByVal type As Type) As Boolean
            Return 0 = String.CompareOrdinal(typeRef.BaseType, type.FullName)
        End Function

        Friend Function ReferenceVarAsType(ByVal var As CodeVariableDeclarationStatement, ByVal type As Type) As CodeExpression
            Return ReferenceVarAsType(var, New CodeTypeReference(type))
        End Function

        ''' <summary>
        ''' Reference the specefied variable but make sure that it's of the specified type.  If it's
        ''' not then take the appropriate action to get it there 
        ''' </summary>
        ''' <param name="var"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function ReferenceVarAsType(ByVal var As CodeVariableDeclarationStatement, ByVal type As CodeTypeReference) As CodeExpression
            Return EnsureType( _
                New CodeVariableReferenceExpression(var.Name), _
                var.Type, _
                type)
        End Function


        ''' <summary>
        ''' Create a reference to the specifed primitive but make sure it's the correct type
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="val"></param>
        ''' <param name="target"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function CreatePrimitiveAsType(Of T)(ByVal val As T, ByVal target As CodeTypeReference) As CodeExpression
            Return EnsureType( _
                New CodePrimitiveExpression(val), _
                New CodeTypeReference(GetType(T)), _
                target)
        End Function

        ''' <summary>
        ''' Ensure the specified expression is of the correct type.  Perform casts as necessary to get it there
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <param name="cur"></param>
        ''' <param name="target"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function EnsureType(ByVal expr As CodeExpression, ByVal cur As CodeTypeReference, ByVal target As CodeTypeReference) As CodeExpression
            If AreEqual(cur, target) Then
                Return expr
            End If

            Return New CodeCastExpression(target, expr)
        End Function

        ''' <summary>
        ''' Are the two type references equal?
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function AreEqual(ByVal left As CodeTypeReference, ByVal right As CodeTypeReference) As Boolean
            If left Is Nothing AndAlso right Is Nothing Then
                Return True
            ElseIf left Is Nothing OrElse right Is Nothing Then
                Return False
            End If

            ' Base type comparison
            If 0 <> String.CompareOrdinal(left.BaseType, right.BaseType) Then
                Return False
            End If

            ' Array data
            If left.ArrayRank <> right.ArrayRank _
                OrElse Not AreEqual(left.ArrayElementType, right.ArrayElementType) Then
                Return False
            End If

            Return True
        End Function


        Friend Function AreEqual(ByVal left As Type, ByVal right As CodeTypeReference) As Boolean
            Return AreEqual(New CodeTypeReference(left), right)
        End Function
    End Module

#End Region

End Namespace
