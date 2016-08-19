// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using CodeParam = System.CodeDom.CodeParameterDeclarationExpression;

namespace PInvoke.Transform
{

	public class CodeDomCloner
	{

		public CodeTypeReference CloneTypeReference(CodeTypeReference typeRef)
		{
			if (typeRef == null) {
				return null;
			}

			CodeTypeReference clone = new CodeTypeReference();
			clone.ArrayElementType = CloneTypeReference(typeRef.ArrayElementType);
			clone.ArrayRank = typeRef.ArrayRank;
			clone.BaseType = typeRef.BaseType;
			clone.Options = typeRef.Options;
			return clone;
		}


		public void CloneCustomAttributes(CodeAttributeDeclarationCollection source, CodeAttributeDeclarationCollection dest)
		{
			dest.Clear();
			foreach (CodeAttributeDeclaration decl in source) {
				dest.Add(CloneCustomAttribute(decl));
			}
		}

		public CodeAttributeDeclaration CloneCustomAttribute(CodeAttributeDeclaration attrib)
		{
			if (attrib == null) {
				return null;
			}

			CodeAttributeDeclaration clone = new CodeAttributeDeclaration(CloneTypeReference(attrib.AttributeType));
			clone.Name = attrib.Name;
			CloneAttributeArguments(attrib.Arguments, clone.Arguments);
			return clone;
		}

		public void CloneAttributeArguments(CodeAttributeArgumentCollection source, CodeAttributeArgumentCollection dest)
		{
			dest.Clear();
			foreach (CodeAttributeArgument arg in source) {
				dest.Add(CloneAttributeArgument(arg));
			}
		}

		public CodeAttributeArgument CloneAttributeArgument(CodeAttributeArgument arg)
		{
			if (arg == null) {
				return null;
			}

			CodeAttributeArgument clone = new CodeAttributeArgument();
			clone.Name = arg.Name;

			clone.Value = arg.Value;
			return clone;
		}

		public void CloneParameters(CodeParameterDeclarationExpressionCollection source, CodeParameterDeclarationExpressionCollection dest)
		{
			dest.Clear();
			foreach (CodeParam param in source) {
				dest.Add(CloneParam(param));
			}
		}

		public CodeParam CloneParamNoAttributes(CodeParam param)
		{
			return CloneParamImpl(param, false);
		}

		public CodeParam CloneParam(CodeParam param)
		{
			return CloneParamImpl(param, true);
		}

		private CodeParam CloneParamImpl(CodeParam param, bool copyAttrib)
		{
			if (param == null) {
				return null;
			}

			CodeParam clone = new CodeParam();
			clone.Name = param.Name;
			clone.Direction = param.Direction;
			clone.Type = CloneTypeReference(param.Type);

			if (copyAttrib) {
				CloneCustomAttributes(param.CustomAttributes, clone.CustomAttributes);
			}

			return clone;
		}

		public CodeMemberMethod CloneMethodSignature(CodeMemberMethod method)
		{
			if (method == null) {
				return null;
			}

			CodeMemberMethod clone = new CodeMemberMethod();
			clone.Name = method.Name;
			clone.ReturnType = CloneTypeReference(method.ReturnType);
			clone.Attributes = method.Attributes;
			CloneCustomAttributes(method.ReturnTypeCustomAttributes, clone.ReturnTypeCustomAttributes);
			CloneParameters(method.Parameters, clone.Parameters);
			CloneCustomAttributes(method.CustomAttributes, clone.CustomAttributes);

			return clone;
		}

	}

}
