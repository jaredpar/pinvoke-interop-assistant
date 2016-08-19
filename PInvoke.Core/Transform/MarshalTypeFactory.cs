// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using PInvoke;
using PInvoke.Parser;
using PInvoke.Transform;

namespace PInvoke.Transform
{

    /// <summary>
    /// Used for creating types that help with Marshalling
    /// </summary>
    /// <remarks></remarks>
    public static class MarshalTypeFactory
    {

        #region "PInvokePointer"


        public const string PInvokePointerTypeName = "PInvokePointer";
        public static CodeTypeReference CreatePInvokePointerCodeTypeReference()
        {
            return new CodeTypeReference(PInvokePointerTypeName);
        }

        public static CodeTypeDeclaration CreatePInvokePointerType()
        {
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(PInvokePointerTypeName);

            ctd.Attributes = MemberAttributes.Public;
            ctd.Members.Add(new CodeMemberField(new CodeTypeReference(typeof(IntPtr)), "m_ptr"));
            ctd.Members.Add(new CodeMemberField(new CodeTypeReference(typeof(Int32)), "m_size"));

            // Add the pointer property
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Name = "IntPtr";
            prop.HasGet = true;
            prop.HasSet = false;
            prop.Type = new CodeTypeReference(typeof(IntPtr));
            prop.Attributes = MemberAttributes.Public;
            prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("m_ptr")));
            ctd.Members.Add(prop);

            // Add the size property
            prop = new CodeMemberProperty();
            prop.Name = "Size";
            prop.HasGet = true;
            prop.HasSet = false;
            prop.Attributes = MemberAttributes.Public;
            prop.Type = new CodeTypeReference(typeof(Int32));
            prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("m_size")));

            // Add the constructor
            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IntPtr)), "ptr"));
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Int32)), "size"));
            ctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("m_ptr"), new CodeVariableReferenceExpression("ptr")));
            ctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("m_size"), new CodeVariableReferenceExpression("size")));
            ctd.Members.Add(ctor);

            // Add the other constructor
            ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Int32)), "size"));
            ctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("m_ptr"), new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(System.Runtime.InteropServices.Marshal)), "AllocCoTaskMem", new CodeVariableReferenceExpression("size"))));
            ctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("m_size"), new CodeVariableReferenceExpression("size")));
            ctd.Members.Add(ctor);

            // Add the free method
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "Free";
            method.Attributes = MemberAttributes.Public;
            method.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(System.Runtime.InteropServices.Marshal)), "FreeCoTaskMem", new CodeVariableReferenceExpression("m_ptr")));
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("m_ptr"), new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(IntPtr)), "Zero")));
            method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("m_size"), new CodePrimitiveExpression(0)));
            ctd.Members.Add(method);

            // Add the method to create a byte[] over the pointer
            method = new CodeMemberMethod();
            method.Name = "ToByteArray";
            method.Attributes = MemberAttributes.Public;
            method.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(new CodeTypeReference(typeof(byte)), 1), "arr", new CodeArrayCreateExpression(new CodeTypeReference(new CodeTypeReference(typeof(byte)), 1), new CodeVariableReferenceExpression("m_size"))));
            method.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(System.Runtime.InteropServices.Marshal)), "Copy", new CodeVariableReferenceExpression("m_ptr"), new CodeVariableReferenceExpression("arr"), new CodePrimitiveExpression(0), new CodeVariableReferenceExpression("m_size")));
            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("arr")));
            method.ReturnType = new CodeTypeReference(new CodeTypeReference(typeof(byte)), 1);
            ctd.Members.Add(method);

            AddPInvokePointerDisposeLogic(ctd);

            return ctd;
        }

        private static void AddPInvokePointerDisposeLogic(CodeTypeDeclaration ctd)
        {
            ctd.BaseTypes.Add(new CodeTypeReference(typeof(IDisposable)));

            CodeMemberMethod mem = new CodeMemberMethod();
            mem.Name = "Dispose";
            mem.Attributes = MemberAttributes.Public;
            mem.ImplementationTypes.Add(new CodeTypeReference(typeof(IDisposable)));

            // Just call the free method
            CodeExpressionStatement state = new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "Free")));
            mem.Statements.Add(state);

            ctd.Members.Add(mem);
        }

        #endregion

    }
}
