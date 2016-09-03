// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using PInvoke;
using PInvoke.Transform;
using System.CodeDom;
using Xunit;
using static PInvoke.Test.GeneratedCodeVerification;

namespace PInvoke.Test
{

    ///<summary>
    ///This is a test class for PInvoke.Transform.CodeTransform and is intended
    ///to contain all PInvoke.Transform.CodeTransform Unit Tests
    ///</summary>
    public class CodeTransformTest
    {
        /// <summary>
        /// Basic expressions.  These are natively supported by the codedom
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TryGenExpression1()
        {
            VerifyExpression("1+1", "(1 + 1)");
            VerifyExpression("1-1", "(1 - 1)");
            VerifyExpression("1/1", "(1 / 1)");
            VerifyExpression("1*1", "(1 * 1)");
        }

        /// <summary>
        /// Generate !/Not expressions which are not natively supported by the CodeDom 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TryGenNot()
        {
            VerifyExpression(LanguageType.VisualBasic, "!1", "Not (1)");
            VerifyExpression(LanguageType.CSharp, "!1", "! (1)");
        }

        [Fact()]
        public void TryGenShift1()
        {
            VerifyExpression("1<<1", "(1) << (1)");
            VerifyExpression("1<< 4+2", "(1) << ((4 + 2))");
        }

        [Fact()]
        public void TryGenShift2()
        {
            VerifyExpression("2>>1", "(2) >> (1)");
            VerifyExpression("8>> 1+2", "(8) >> ((1 + 2))");
        }

        /// <summary>
        /// Generate a binary | expression
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TryGenBinaryOr()
        {
            VerifyExpression("1|1", "(1 Or 1)");
        }

        /// <summary>
        /// Generate a binary and expression
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TryGenBinaryAnd()
        {
            VerifyExpression("42&3", "(42 And 3)");
        }


        /// <summary>
        /// Simple Constant refering to another contstant
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Gen1()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("C1", "1"));
            bag.AddConstant(new NativeConstant("C2", "1+C1"));
            VerifyConstValue(bag, "C2", string.Format("(1 + {0}.C1)", TransformConstants.NativeConstantsName));
        }

        /// <summary>
        /// Simple Enum referring to another enum
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Gen2()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeEnum e1 = new NativeEnum("e1");
            e1.Values.Add(new NativeEnumValue("v1", "2"));
            e1.Values.Add(new NativeEnumValue("v2", "v1+1"));
            bag.AddDefinedType(e1);
            VerifyEnumValue(bag, e1, "v1", "2");
            VerifyEnumValue(bag, e1, "v2", "(e1.v1 + 1)");
        }

        /// <summary>
        /// Cross enumeration reference
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Gen3()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeEnum e1 = new NativeEnum("e1");
            e1.Values.Add(new NativeEnumValue("v1", "2"));
            e1.Values.Add(new NativeEnumValue("v2", "v1+1"));
            NativeEnum e2 = new NativeEnum("e2");
            e2.Values.Add(new NativeEnumValue("v3", "v2+1"));
            bag.AddDefinedType(e1);
            bag.AddDefinedType(e2);
            VerifyEnumValue(bag, e2, "v3", "(e1.v2 + 1)");
        }

        /// <summary>
        /// Cross Constant to Enum reference
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Gen4()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("C1", "42"));
            NativeEnum e1 = new NativeEnum("e1");
            e1.Values.Add(new NativeEnumValue("v1", "C1+2"));
            bag.AddDefinedType(e1);
            VerifyEnumValue(bag, e1, "v1", string.Format("({0}.C1 + 2)", TransformConstants.NativeConstantsName));
        }

        /// <summary>
        /// Regression Test
        /// 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Gen5()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("A", "A"));
            VerifyConstValue(bag, "A", "Nothing", "System.Object");
        }

        /// <summary>
        /// Mutually recursive constants
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Gen6()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("A", "B"));
            bag.AddConstant(new NativeConstant("B", "A"));
            VerifyConstValue(bag, "A", string.Format("{0}.B", TransformConstants.NativeConstantsName));
            VerifyConstValue(bag, "B", string.Format("{0}.A", TransformConstants.NativeConstantsName));
        }

        [Fact()]
        public void Gen7()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("A", "'c'"));
            VerifyConstValue(LanguageType.CSharp, bag, "A", "'c'");
        }

        [Fact()]
        public void Gen8()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("A", "0x5"));
            VerifyConstValue(LanguageType.CSharp, bag, "A", "5");
        }

        [Fact()]
        public void Gen9()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("A", "1.0"));
            VerifyConstValue(LanguageType.CSharp, bag, "A", "1F", "System.Single");
        }

        [Fact()]
        public void Gen10()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("A", "'a'"));
            bag.AddConstant(new NativeConstant("B", "'c'"));
            bag.AddConstant(new NativeConstant("C", "'\\n'"));
            VerifyConstValue(LanguageType.CSharp, bag, "A", "'a'", "System.Char");
            VerifyConstValue(LanguageType.CSharp, bag, "B", "'c'", "System.Char");
            VerifyConstValue(LanguageType.CSharp, bag, "C", "'\\n'", "System.Char");
        }

        /// <summary>
        /// Make sure that unparsable constants are generated as a raw string
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Gen11()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("A", "FALSE;"));
            VerifyConstValue(LanguageType.CSharp, bag, "A", "\"FALSE;\"", "System.String");
        }

        /// <summary>
        /// Negative numbers 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Gen12()
        {
            VerifyCSharpExpression("-1", "-1", "System.Int32");
            VerifyCSharpExpression("-1.0F", "-1F", "System.Single");
            VerifyCSharpExpression("-0.1F", "-0.1F", "System.Single");
        }

        /// <summary>
        /// Boolean expressions
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Gen13()
        {
            VerifyCSharpExpression("true", "true", "System.Boolean");
            VerifyCSharpExpression("false", "false", "System.Boolean");
        }

        [Fact()]
        public void Gen14()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("A", "L'\\10'"));
            VerifyConstValue(LanguageType.CSharp, bag, "A", "'\\n'", "System.Char");
        }

        /// <summary>
        /// Make sure that an invalid constant expression will still produce a value
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Invalid1()
        {
            NativeSymbolBag bag = new NativeSymbolBag(StorageFactory.CreateStandard());
            bag.AddConstant(new NativeConstant("c1", "(S1)5"));
            VerifyConstValue(bag, "c1", "\"(S1)5\"");
        }

        /// <summary>
        /// Invalid enum values 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Invalid2()
        {
            NativeSymbolBag bag = new NativeSymbolBag(StorageFactory.CreateStandard());
            NativeEnum e1 = new NativeEnum("e1");
            e1.Values.Add(new NativeEnumValue("v1", "(S1)5"));
            bag.AddDefinedType(e1);
            VerifyEnumValue(bag, e1, "v1", "\"(S1)5\"");
        }
    }
}