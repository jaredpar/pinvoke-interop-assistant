' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.Collections.Generic
Imports PInvoke
Imports PInvoke.Parser
Imports PInvoke.Transform

Namespace Transform

    ''' <summary>
    ''' Used for creating types that help with Marshalling
    ''' </summary>
    ''' <remarks></remarks>
    Public Module MarshalTypeFactory

#Region "PInvokePointer"

        Public Const PInvokePointerTypeName As String = "PInvokePointer"

        Public Function CreatePInvokePointerCodeTypeReference() As CodeTypeReference
            Return New CodeTypeReference(PInvokePointerTypeName)
        End Function

        Public Function CreatePInvokePointerType() As CodeTypeDeclaration
            Dim ctd As New CodeTypeDeclaration(PInvokePointerTypeName)

            ctd.Attributes = MemberAttributes.Public
            ctd.Members.Add(New CodeMemberField(
                New CodeTypeReference(GetType(IntPtr)),
                "m_ptr"))
            ctd.Members.Add(New CodeMemberField(
                New CodeTypeReference(GetType(Int32)),
                "m_size"))

            ' Add the pointer property
            Dim prop As New CodeMemberProperty
            prop.Name = "IntPtr"
            prop.HasGet = True
            prop.HasSet = False
            prop.Type = New CodeTypeReference(GetType(IntPtr))
            prop.Attributes = MemberAttributes.Public
            prop.GetStatements.Add(New CodeMethodReturnStatement(
                New CodeVariableReferenceExpression("m_ptr")))
            ctd.Members.Add(prop)

            ' Add the size property
            prop = New CodeMemberProperty
            prop.Name = "Size"
            prop.HasGet = True
            prop.HasSet = False
            prop.Attributes = MemberAttributes.Public
            prop.Type = New CodeTypeReference(GetType(Int32))
            prop.GetStatements.Add(New CodeMethodReturnStatement(
                New CodeVariableReferenceExpression("m_size")))

            ' Add the constructor
            Dim ctor As New CodeConstructor()
            ctor.Attributes = MemberAttributes.Public
            ctor.Parameters.Add(New CodeParameterDeclarationExpression(
                New CodeTypeReference(GetType(IntPtr)),
                "ptr"))
            ctor.Parameters.Add(New CodeParameterDeclarationExpression(
                New CodeTypeReference(GetType(Int32)),
                "size"))
            ctor.Statements.Add(New CodeAssignStatement(
                New CodeVariableReferenceExpression("m_ptr"),
                New CodeVariableReferenceExpression("ptr")))
            ctor.Statements.Add(New CodeAssignStatement(
                New CodeVariableReferenceExpression("m_size"),
                New CodeVariableReferenceExpression("size")))
            ctd.Members.Add(ctor)

            ' Add the other constructor
            ctor = New CodeConstructor()
            ctor.Attributes = MemberAttributes.Public
            ctor.Parameters.Add(New CodeParameterDeclarationExpression(
                New CodeTypeReference(GetType(Int32)),
                "size"))
            ctor.Statements.Add(New CodeAssignStatement(
                New CodeVariableReferenceExpression("m_ptr"),
                New CodeMethodInvokeExpression(
                    New CodeTypeReferenceExpression(GetType(System.Runtime.InteropServices.Marshal)),
                    "AllocCoTaskMem",
                    New CodeVariableReferenceExpression("size"))))
            ctor.Statements.Add(New CodeAssignStatement(
                New CodeVariableReferenceExpression("m_size"),
                New CodeVariableReferenceExpression("size")))
            ctd.Members.Add(ctor)

            ' Add the free method
            Dim method As New CodeMemberMethod()
            method.Name = "Free"
            method.Attributes = MemberAttributes.Public
            method.Statements.Add(New CodeMethodInvokeExpression(
                New CodeTypeReferenceExpression(GetType(System.Runtime.InteropServices.Marshal)),
                "FreeCoTaskMem",
                New CodeVariableReferenceExpression("m_ptr")))
            method.Statements.Add(New CodeAssignStatement(
                New CodeVariableReferenceExpression("m_ptr"),
                New CodeFieldReferenceExpression(
                    New CodeTypeReferenceExpression(GetType(IntPtr)),
                    "Zero")))
            method.Statements.Add(New CodeAssignStatement(
                New CodeVariableReferenceExpression("m_size"),
                New CodePrimitiveExpression(0)))
            ctd.Members.Add(method)

            ' Add the method to create a byte[] over the pointer
            method = New CodeMemberMethod()
            method.Name = "ToByteArray"
            method.Attributes = MemberAttributes.Public
            method.Statements.Add(New CodeVariableDeclarationStatement(
                New CodeTypeReference(New CodeTypeReference(GetType(Byte)), 1),
                "arr",
                New CodeArrayCreateExpression(
                    New CodeTypeReference(New CodeTypeReference(GetType(Byte)), 1),
                    New CodeVariableReferenceExpression("m_size"))))
            method.Statements.Add(New CodeMethodInvokeExpression(
                New CodeTypeReferenceExpression(GetType(System.Runtime.InteropServices.Marshal)),
                "Copy",
                New CodeVariableReferenceExpression("m_ptr"),
                New CodeVariableReferenceExpression("arr"),
                New CodePrimitiveExpression(0),
                New CodeVariableReferenceExpression("m_size")))
            method.Statements.Add(New CodeMethodReturnStatement(
                New CodeVariableReferenceExpression("arr")))
            method.ReturnType = New CodeTypeReference(New CodeTypeReference(GetType(Byte)), 1)
            ctd.Members.Add(method)

            AddPInvokePointerDisposeLogic(ctd)

            Return ctd
        End Function

        Private Sub AddPInvokePointerDisposeLogic(ByVal ctd As CodeTypeDeclaration)
            ctd.BaseTypes.Add(New CodeTypeReference(GetType(IDisposable)))

            Dim mem As CodeMemberMethod = New CodeMemberMethod()
            mem.Name = "Dispose"
            mem.Attributes = MemberAttributes.Public
            mem.ImplementationTypes.Add(New CodeTypeReference(GetType(IDisposable)))

            ' Just call the free method
            Dim state As New CodeExpressionStatement(
                New CodeMethodInvokeExpression(
                    New CodeMethodReferenceExpression(New CodeThisReferenceExpression(), "Free")))
            mem.Statements.Add(state)

            ctd.Members.Add(mem)
        End Sub

#End Region

    End Module

End Namespace
