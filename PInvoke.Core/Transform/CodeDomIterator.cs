
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.CodeDom;

namespace Transform
{

	/// <summary>
	/// Used to iterate various CodeDom constructions
	/// </summary>
	/// <remarks></remarks>
	public class CodeDomIterator
	{

		public List<object> Iterate(CodeTypeDeclarationCollection col)
		{
			ThrowIfNull(col);

			List<object> list = new List<object>();
			foreach (CodeTypeDeclaration ctd in col) {
				IterateTypeMemberImpl(ctd, list);
			}

			return list;
		}

		public List<object> Iterate(CodeTypeDeclaration ctd)
		{
			ThrowIfNull(ctd);

			List<object> list = new List<object>();
			IterateTypeMemberImpl(ctd, list);
			return list;
		}

		#region "CodeMember Iteration"

		private void IterateTypeMemberImpl(CodeTypeMember ctm, List<object> list)
		{
			ThrowIfNull(ctm);
			ThrowIfNull(list);

			list.Add(ctm);
			IterateAttributesImpl(ctm.CustomAttributes, list);

			CodeMemberEvent memEvent = ctm as CodeMemberEvent;
			if (memEvent != null) {
				IterateMemberEventImpl(memEvent, list);
				return;
			}

			CodeMemberField memField = ctm as CodeMemberField;
			if (memField != null) {
				IterateMemberFieldImpl(memField, list);
				return;
			}

			CodeMemberMethod memMethod = ctm as CodeMemberMethod;
			if (memMethod != null) {
				IterateMemberMethodImpl(memMethod, list);
				return;
			}

			CodeMemberProperty memProperty = ctm as CodeMemberProperty;
			if (memProperty != null) {
				IterateMemberPropertyImpl(memProperty, list);
				return;
			}

			CodeSnippetTypeMember memSnippet = ctm as CodeSnippetTypeMember;
			if (memSnippet != null) {
				IterateSnippetTypeImpl(memSnippet, list);
				return;
			}

			CodeTypeDeclaration typeDecl = ctm as CodeTypeDeclaration;
			if (typeDecl != null) {
				IterateTypeDeclarationImpl(typeDecl, list);
				return;
			}

		}

		private void IterateTypeDeclarationImpl(CodeTypeDeclarationCollection col, List<object> list)
		{
			ThrowIfNull(col);
			ThrowIfNull(list);

			foreach (CodeTypeDeclaration ctd in col) {
				IterateTypeMemberImpl(ctd, list);
			}
		}

		private void IterateTypeDeclarationImpl(CodeTypeDeclaration ctd, List<object> list)
		{
			ThrowIfNull(ctd);
			ThrowIfNull(list);

			// Don't add to the list because this is added in the member iterater
			foreach (CodeTypeReference ctdRef in ctd.BaseTypes) {
				IterateTypeRefImpl(ctdRef, list);
			}

			foreach (CodeTypeMember member in ctd.Members) {
				IterateTypeMemberImpl(member, list);
			}

			IterateTypeParametersImpl(ctd.TypeParameters, list);

			CodeTypeDelegate delType = ctd as CodeTypeDelegate;
			if (delType != null) {
				list.Add(delType.ReturnType);

				IterateParametersImpl(delType.Parameters, list);
			}

		}

		private void IterateMemberEventImpl(CodeMemberEvent memEvent, List<object> list)
		{
			ThrowIfNull(memEvent);
			ThrowIfNull(list);

			foreach (CodeTypeReference typeRef in memEvent.ImplementationTypes) {
				IterateTypeRefImpl(typeRef, list);
			}

			IterateTypeRefImpl(memEvent.PrivateImplementationType, list);
			IterateTypeRefImpl(memEvent.Type, list);
		}

		private void IterateMemberFieldImpl(CodeMemberField memField, List<object> list)
		{
			ThrowIfNull(memField);
			ThrowIfNull(list);

			IterateTypeRefImpl(memField.Type, list);
			IterateExprImpl(memField.InitExpression, list);
		}

		private void IterateMemberMethodImpl(CodeMemberMethod memMethod, List<object> list)
		{
			ThrowIfNull(memMethod);
			ThrowIfNull(list);

			IterateTypeRefImpl(memMethod.ImplementationTypes, list);
			IterateParametersImpl(memMethod.Parameters, list);
			IterateTypeRefImpl(memMethod.PrivateImplementationType, list);
			IterateTypeRefImpl(memMethod.ReturnType, list);
			IterateAttributesImpl(memMethod.CustomAttributes, list);
			IterateStatementsImpl(memMethod.Statements, list);
			IterateTypeParametersImpl(memMethod.TypeParameters, list);
		}

		private void IterateMemberPropertyImpl(CodeMemberProperty memProperty, List<object> list)
		{
			ThrowIfNull(memProperty);
			ThrowIfNull(list);

			IterateTypeRefImpl(memProperty.Type, list);
			IterateTypeRefImpl(memProperty.ImplementationTypes, list);
			IterateParametersImpl(memProperty.Parameters, list);
			IterateTypeRefImpl(memProperty.PrivateImplementationType, list);
			if (memProperty.HasGet) {
				IterateStatementsImpl(memProperty.GetStatements, list);
			}
			if (memProperty.HasSet) {
				IterateStatementsImpl(memProperty.SetStatements, list);
			}

		}

		#endregion

		#region "CodeParameter Iteration"

		private void IterateParametersImpl(CodeParameterDeclarationExpressionCollection col, List<object> list)
		{
			ThrowIfNull(col);
			ThrowIfNull(list);

			foreach (CodeParameterDeclarationExpression param in col) {
				list.Add(param);
				IterateAttributesImpl(param.CustomAttributes, list);
				IterateTypeRefImpl(param.Type, list);
			}
		}

		#endregion

		#region "CodeAttribute Iteration"
		private void IterateAttributesImpl(CodeAttributeDeclarationCollection col, List<object> list)
		{
			ThrowIfNull(col);
			ThrowIfNull(list);

			foreach (CodeAttributeDeclaration decl in col) {
				IterateAttributesImpl(decl, list);
			}

		}

		private void IterateAttributesImpl(CodeAttributeDeclaration attr, List<object> list)
		{
			ThrowIfNull(attr);
			ThrowIfNull(list);

			list.Add(attr);
			IterateTypeRefImpl(attr.AttributeType, list);

			foreach (CodeAttributeArgument arg in attr.Arguments) {
				list.Add(arg);
				IterateExprImpl(arg.Value, list);
			}
		}

		#endregion

		#region "CodeExpression Iteration"
		private void IterateExprImpl(CodeExpression expr, List<object> list)
		{
			ThrowIfNull(list);
			if (expr == null) {
				return;
			}

			list.Add(expr);
			CodePrimitiveExpression primExpr = expr as CodePrimitiveExpression;
			if (primExpr != null) {
				list.Add(primExpr.Value);
				return;
			}

			CodeFieldReferenceExpression fieldRef = expr as CodeFieldReferenceExpression;
			if (fieldRef != null) {
				IterateExprImpl(fieldRef.TargetObject, list);
				return;
			}

			CodeTypeReferenceExpression typeRefExpr = expr as CodeTypeReferenceExpression;
			if (typeRefExpr != null) {
				IterateTypeRefImpl(typeRefExpr.Type, list);
				return;
			}

			CodeBinaryOperatorExpression binExpr = expr as CodeBinaryOperatorExpression;
			if (binExpr != null) {
				IterateExprImpl(binExpr.Left, list);
				IterateExprImpl(binExpr.Right, list);
				return;
			}

			CodePropertySetValueReferenceExpression propSetExpr = expr as CodePropertySetValueReferenceExpression;
			if (propSetExpr != null) {
				return;
			}

			CodeThisReferenceExpression thisExpr = expr as CodeThisReferenceExpression;
			if (thisExpr != null) {
				return;
			}

			CodeMethodInvokeExpression methodInvokeExpr = expr as CodeMethodInvokeExpression;
			if (methodInvokeExpr != null) {
				foreach (CodeExpression arg in methodInvokeExpr.Parameters) {
					IterateExprImpl(arg, list);
				}
				IterateExprImpl(methodInvokeExpr.Method, list);
				return;
			}

			CodeMethodReferenceExpression methodRefExpr = expr as CodeMethodReferenceExpression;
			if (methodRefExpr != null) {
				IterateExprImpl(methodRefExpr.TargetObject, list);
				return;
			}

			CodeObjectCreateExpression createExpr = expr as CodeObjectCreateExpression;
			if (createExpr != null) {
				IterateTypeRefImpl(createExpr.CreateType, list);
				return;
			}

			CodeVariableReferenceExpression varRefExpr = expr as CodeVariableReferenceExpression;
			if (varRefExpr != null) {
				return;
			}

			CodeArrayCreateExpression createArrayExpr = expr as CodeArrayCreateExpression;
			if (createArrayExpr != null) {
				IterateTypeRefImpl(createArrayExpr.CreateType, list);
				return;
			}

			CodeCastExpression castExpr = expr as CodeCastExpression;
			if (castExpr != null) {
				IterateTypeRefImpl(castExpr.TargetType, list);
				IterateExprImpl(castExpr.Expression, list);
				return;
			}

			CodeNotExpression notExpr = expr as CodeNotExpression;
			if (notExpr != null) {
				IterateExprImpl(notExpr.Expression, list);
				return;
			}

			CodeNegativeExpression negativeExpr = expr as CodeNegativeExpression;
			if (negativeExpr != null) {
				IterateExprImpl(negativeExpr.Expression, list);
				return;
			}

			CodeShiftExpression shiftExpr = expr as CodeShiftExpression;
			if (shiftExpr != null) {
				IterateExprImpl(shiftExpr.Left, list);
				IterateExprImpl(shiftExpr.Right, list);
				return;
			}

			CodeDirectionalSymbolExpression directionalExpr = expr as CodeDirectionalSymbolExpression;
			if (directionalExpr != null) {
				IterateExprImpl(directionalExpr.Expression, list);
				return;
			}

			throw new NotImplementedException("Unrecognized expression");
		}

		#endregion

		#region "CodeStatement iteration"

		private void IterateStatementsImpl(CodeStatementCollection statements, List<object> list)
		{
			foreach (CodeStatement statement in statements) {
				IterateStatementImpl(statement, list);
			}
		}

		private void IterateStatementImpl(CodeStatement state, List<object> list)
		{
			ThrowIfNull(state);
			ThrowIfNull(list);

			list.Add(state);
			CodeMethodReturnStatement retState = state as CodeMethodReturnStatement;
			if (retState != null) {
				IterateExprImpl(retState.Expression, list);
				return;
			}

			CodeAssignStatement asgState = state as CodeAssignStatement;
			if (asgState != null) {
				IterateExprImpl(asgState.Left, list);
				IterateExprImpl(asgState.Right, list);
				return;
			}

			CodeVariableDeclarationStatement varDeclState = state as CodeVariableDeclarationStatement;
			if (varDeclState != null) {
				IterateExprImpl(varDeclState.InitExpression, list);
				IterateTypeRefImpl(varDeclState.Type, list);
				return;
			}

			CodeConditionStatement condState = state as CodeConditionStatement;
			if (condState != null) {
				IterateExprImpl(condState.Condition, list);
				IterateStatementsImpl(condState.TrueStatements, list);
				IterateStatementsImpl(condState.FalseStatements, list);
				return;
			}

			CodeLabeledStatement labelState = state as CodeLabeledStatement;
			if (labelState != null) {
				return;
			}

			CodeGotoStatement gotoState = state as CodeGotoStatement;
			if (gotoState != null) {
				return;
			}

			CodeExpressionStatement exprState = state as CodeExpressionStatement;
			if (exprState != null) {
				IterateExprImpl(exprState.Expression, list);
				return;
			}

			throw new NotImplementedException("Unrecognized statement");
		}
		#endregion

		private void IterateTypeRefImpl(CodeTypeReferenceCollection col, List<object> list)
		{
			ThrowIfNull(col);
			ThrowIfNull(list);

			foreach (CodeTypeReference typeRef in col) {
				IterateTypeRefImpl(typeRef, list);
			}
		}

		private void IterateTypeRefImpl(CodeTypeReference typeRef, List<object> list)
		{
			if (typeRef == null) {
				return;
			}

			list.Add(typeRef);
			IterateTypeRefImpl(typeRef.ArrayElementType, list);
			IterateTypeRefImpl(typeRef.TypeArguments, list);
		}

		#region "Not Implemented"

		private void IterateSnippetTypeImpl(CodeSnippetTypeMember memSnippet, List<object> list)
		{
			throw new NotImplementedException();
		}

		private void IterateTypeParametersImpl(CodeTypeParameterCollection col, List<object> list)
		{
			ThrowIfNull(col);
			ThrowIfNull(list);

			foreach (CodeTypeParameter param in col) {
				throw new NotImplementedException();
			}

		}

		#endregion

	}

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
