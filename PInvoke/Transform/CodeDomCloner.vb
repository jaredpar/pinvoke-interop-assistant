' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.Collections.Generic
Imports CodeParam = System.CodeDom.CodeParameterDeclarationExpression

Namespace Transform

    Public Class CodeDomCloner

        Public Function CloneTypeReference(ByVal typeRef As CodeTypeReference) As CodeTypeReference
            If typeRef Is Nothing Then
                Return Nothing
            End If

            Dim clone As New CodeTypeReference
            clone.ArrayElementType = CloneTypeReference(typeRef.ArrayElementType)
            clone.ArrayRank = typeRef.ArrayRank
            clone.BaseType = typeRef.BaseType
            clone.Options = typeRef.Options
            Return clone
        End Function


        Public Sub CloneCustomAttributes(ByVal source As CodeAttributeDeclarationCollection, ByVal dest As CodeAttributeDeclarationCollection)
            dest.Clear()
            For Each decl As CodeAttributeDeclaration In source
                dest.Add(CloneCustomAttribute(decl))
            Next
        End Sub

        Public Function CloneCustomAttribute(ByVal attrib As CodeAttributeDeclaration) As CodeAttributeDeclaration
            If attrib Is Nothing Then
                Return Nothing
            End If

            Dim clone As New CodeAttributeDeclaration(CloneTypeReference(attrib.AttributeType))
            clone.Name = attrib.Name
            CloneAttributeArguments(attrib.Arguments, clone.Arguments)
            Return clone
        End Function

        Public Sub CloneAttributeArguments(ByVal source As CodeAttributeArgumentCollection, ByVal dest As CodeAttributeArgumentCollection)
            dest.Clear()
            For Each arg As CodeAttributeArgument In source
                dest.Add(CloneAttributeArgument(arg))
            Next
        End Sub

        Public Function CloneAttributeArgument(ByVal arg As CodeAttributeArgument) As CodeAttributeArgument
            If arg Is Nothing Then
                Return Nothing
            End If

            Dim clone As New CodeAttributeArgument()
            clone.Name = arg.Name

            clone.Value = arg.Value
            Return clone
        End Function

        Public Sub CloneParameters(ByVal source As CodeParameterDeclarationExpressionCollection, ByVal dest As CodeParameterDeclarationExpressionCollection)
            dest.Clear()
            For Each param As CodeParam In source
                dest.Add(CloneParam(param))
            Next
        End Sub

        Public Function CloneParamNoAttributes(ByVal param As CodeParam) As CodeParam
            Return CloneParamImpl(param, False)
        End Function

        Public Function CloneParam(ByVal param As CodeParam) As CodeParam
            Return CloneParamImpl(param, True)
        End Function

        Private Function CloneParamImpl(ByVal param As CodeParam, ByVal copyAttrib As Boolean) As CodeParam
            If param Is Nothing Then
                Return Nothing
            End If

            Dim clone As New CodeParam
            clone.Name = param.Name
            clone.Direction = param.Direction
            clone.Type = CloneTypeReference(param.Type)

            If copyAttrib Then
                CloneCustomAttributes(param.CustomAttributes, clone.CustomAttributes)
            End If

            Return clone
        End Function

        Public Function CloneMethodSignature(ByVal method As CodeMemberMethod) As CodeMemberMethod
            If method Is Nothing Then
                Return Nothing
            End If

            Dim clone As New CodeMemberMethod
            clone.Name = method.Name
            clone.ReturnType = CloneTypeReference(method.ReturnType)
            clone.Attributes = method.Attributes
            CloneCustomAttributes(method.ReturnTypeCustomAttributes, clone.ReturnTypeCustomAttributes)
            CloneParameters(method.Parameters, clone.Parameters)
            CloneCustomAttributes(method.CustomAttributes, clone.CustomAttributes)

            Return clone
        End Function

    End Class

End Namespace
