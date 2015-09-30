using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PInvoke.Parser;
using System.Runtime.InteropServices;

namespace PInvoke.Roslyn
{
    internal sealed class CSharpNativeFactory
    {
        private readonly CSharpExprFactory _exprGenerator;

        internal CSharpNativeFactory()
        {
            _exprGenerator = new CSharpExprFactory();
        }

        internal SyntaxNode GenerateDeclaration(NativeDefinedType decl)
        {
            switch (decl.Kind)
            {
                case NativeSymbolKind.StructType:
                    return GenerateStruct((NativeStruct)decl);
                case NativeSymbolKind.UnionType:
                    return GenerateUnion((NativeUnion)decl);
                case NativeSymbolKind.EnumType:
                    return GenerateEnum((NativeEnum)decl);
                case NativeSymbolKind.FunctionPointer:
                    return GenerateFunctionPointer((NativeFunctionPointer)decl);
                default:
                    throw Contract.InvalidEnum(decl.Kind);
            }
        }

        internal StructDeclarationSyntax GenerateStruct(NativeStruct nativeStruct)
        {
            var structSyntax = SyntaxFactory.StructDeclaration(nativeStruct.Name);

            structSyntax = structSyntax.WithAttributeLists(GenerateSimpleAttribute(GenerateStructLayout()));

            return structSyntax;
        }

        private static SyntaxList<AttributeListSyntax> GenerateSimpleAttribute(AttributeSyntax attribute)
        {
            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { attribute }));
            return SyntaxFactory.List(new[] { attributeList });
        }

        private AttributeSyntax GenerateStructLayout()
        {
            var arg = SyntaxFactory.AttributeArgument(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(nameof(LayoutKind)),
                    SyntaxFactory.IdentifierName(nameof(LayoutKind.Sequential))));

            return SyntaxFactory.Attribute(
                SyntaxFactory.ParseName("System.Runtime.InteropServices.StructLayoutAttribute"),
                SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[] { arg })));
        }

        internal StructDeclarationSyntax GenerateUnion(NativeUnion nativeUnion)
        {
            throw new NotSupportedException();
        }

        internal EnumDeclarationSyntax GenerateEnum(NativeEnum nativeEnum)
        {
            throw new NotSupportedException();
        }

        internal DelegateDeclarationSyntax GenerateFunctionPointer(NativeFunctionPointer nativeFuncPointer)
        {
            throw new NotSupportedException();
        }

        internal FieldDeclarationSyntax GenerateConstant(NativeConstant constant)
        {
            switch (constant.ConstantKind)
            {
                case ConstantKind.Macro:
                    return GenerateConstantValue(constant);
                case ConstantKind.MacroMethod:
                    throw new NotImplementedException();
                default:
                    throw Contract.InvalidEnum(constant.ConstantKind);
            }
        }

        internal FieldDeclarationSyntax GenerateConstantValue(NativeConstant constant)
        {
            var declarator = SyntaxFactory.VariableDeclarator(
                SyntaxFactory.Identifier(constant.Name),
                SyntaxFactory.BracketedArgumentList(),
                SyntaxFactory.EqualsValueClause(GenerateExpression(constant.Value)));

            var declaration = SyntaxFactory.VariableDeclaration(
                SyntaxFactory.ParseTypeName("int"),
                SyntaxFactory.SeparatedList(new[] { declarator }));

            return SyntaxFactory.FieldDeclaration(declaration);
        }

        internal ExpressionSyntax GenerateExpression(NativeValueExpression expr)
        {
            return _exprGenerator.GenerateValue(expr.Node);
        }
    }
}
