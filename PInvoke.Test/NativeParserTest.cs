// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using PInvoke;
using PInvoke.Parser;
using System.IO;
using Xunit;
namespace PInvoke.Test
{

    public class ParseEngineTest
    {

        private ParseResult ParseString(string data)
        {
            ParseEngine parser = new ParseEngine();
            using (StringReader stream = new StringReader(data))
            {
                return parser.Parse(stream);
            }
        }

        private ParseResult ParseFile(string filePath)
        {
            ParseEngine parser = new ParseEngine();
            using (StreamReader reader = new StreamReader(filePath))
            {
                return parser.Parse(new TextReaderBag(reader));
            }

        }

        private ParseResult FullParseFile(string filePath)
        {
            PreProcessorOptions opts = new PreProcessorOptions();
            PreProcessorEngine pre = new PreProcessorEngine(opts);

            using (StreamReader stream = new StreamReader(filePath))
            {
                string data = pre.Process(new TextReaderBag(stream));
                string tempPath = Path.GetTempFileName();
                try
                {
                    File.WriteAllText(tempPath, data);
                    return ParseFile(tempPath);
                }
                finally
                {
                    File.Delete(tempPath);
                }
            }
        }

        /// <summary>
        /// Parse a data.  Import the sal semantics
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private ParseResult SalParse(string text)
        {
            string salText = File.ReadAllText("SampleFiles\\Sal.txt");
            text = salText + PortConstants.NewLine + text;

            PreProcessorOptions opts = new PreProcessorOptions();
            PreProcessorEngine pre = new PreProcessorEngine(opts);
            string data = pre.Process(new TextReaderBag(new StringReader(text)));
            string tempPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempPath, data);
                return ParseFile(tempPath);
            }
            finally
            {
                File.Delete(tempPath);
            }

        }

        private void VerifyStruct(NativeType nt, string name, params string[] args)
        {
            NativeStruct ntStruct = nt as NativeStruct;
            Assert.NotNull(ntStruct);
            VerifyMembers(ntStruct, name, args);
        }

        private void VerifyUnion(NativeType nt, string name, params string[] args)
        {
            NativeUnion ntUnion = nt as NativeUnion;
            Assert.NotNull(ntUnion);
            VerifyMembers(ntUnion, name, args);
        }

        private void VerifyMembers(NativeDefinedType container, string name, params string[] args)
        {
            if (container.IsAnonymous)
            {
                Assert.True(string.IsNullOrEmpty(name));
            }
            else
            {
                Assert.Equal(name, container.Name);
            }

            int index = 0;
            foreach (NativeMember cur in container.Members)
            {
                NativeDefinedType definedType = cur.NativeType as NativeDefinedType;
                if (definedType != null && definedType.IsAnonymous)
                {
                    Assert.True(string.IsNullOrEmpty(args[index]));
                }
                else
                {
                    Assert.Equal(args[index], cur.NativeType.DisplayName);
                }
                Assert.Equal(args[index + 1], cur.Name);
                index += 2;
            }
        }

        private void VerifyProc(ParseResult result, int index, string str)
        {
            Assert.NotNull(result);

            Assert.True(index < result.NativeProcedures.Count, "Invalid procedure index");
            NativeProcedure proc = result.NativeProcedures[index];
            Assert.Equal(str, proc.DisplayName);
        }

        private void VerifyFuncPtr(ParseResult result, int index, string str)
        {
            Assert.NotNull(result);
            Assert.True(index < result.NativeDefinedTypes.Count, "Invalid index");
            NativeFunctionPointer fptr = (NativeFunctionPointer)result.NativeDefinedTypes[index];
            Assert.Equal(str, fptr.DisplayName);
        }

        private void VerifyProcSal(ParseResult result, int index, string str)
        {
            Assert.NotNull(result);

            Assert.True(index < result.NativeProcedures.Count, "Invalid procedure index");
            NativeProcedure proc = result.NativeProcedures[index];
            Assert.Equal(str, proc.Signature.CalculateSignature(proc.Name, true));
        }

        private void VerifyTypeDef(NativeType nt, string name, string targetName)
        {
            NativeTypeDef td = nt as NativeTypeDef;
            Assert.NotNull(td);
            Assert.Equal(name, td.DisplayName);
            Assert.Equal(targetName, td.RealType.DisplayName);
        }

        private void VerifyPointer(NativeType nt, string fullName)
        {
            NativePointer pt = nt as NativePointer;
            Assert.NotNull(pt);
            Assert.Equal(pt.DisplayName, fullName);
        }

        private void VerifyEnum(NativeType nt, string name, params string[] args)
        {
            NativeEnum ntEnum = nt as NativeEnum;
            Assert.NotNull(ntEnum);
            Assert.Equal(name, ntEnum.Name);

            List<NativeEnumValue> list = new List<NativeEnumValue>(ntEnum.Values);
            for (int i = 0; i <= args.Length - 1; i += 2)
            {
                Assert.True(list.Count > 0, "No more values");
                string valueName = args[i];
                string valueValue = args[i + 1];
                NativeEnumValue ntValue = list[0];
                list.RemoveAt(0);
                Assert.Equal(valueName, ntValue.Name);
                Assert.Equal(valueValue, ntValue.Value.Expression);
            }
        }

        private void VerifyPrint(ParseResult result, int index, string str)
        {
            Assert.NotNull(result);

            string realStr = SymbolPrinter.Convert(result.ParsedTypes[index]);
            Assert.Equal(str, realStr);
        }

        /// <summary>
        /// Single member struct
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TestStruct1()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct1.txt");
            NativeStruct nt = (NativeStruct)result.ParsedTypes[0];
            VerifyMembers(nt, "foo", "int", "i");
        }

        /// <summary>
        /// Single member struct with comments
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TestStruct2()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct2.txt");
            NativeStruct nt = (NativeStruct)result.ParsedTypes[0];
            VerifyMembers(nt, "bar", "double", "j");
        }

        /// <summary>
        /// Pointers inside of the struct
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TestStruct3()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct3.txt");
            NativeStruct nt = (NativeStruct)result.ParsedTypes[0];
            VerifyMembers(nt, "bar2", "bar**", "i", "foo", "j");
        }

        [Fact()]
        public void TestStruct4()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct4.txt");
            NativeStruct nt1 = (NativeStruct)result.ParsedTypes[0];
            NativeStruct nt2 = (NativeStruct)result.ParsedTypes[1];

            VerifyMembers(nt1, "s1", "int", "i", "double*", "j");
            VerifyMembers(nt2, "s2", "s1", "parent");
        }

        /// <summary>
        /// Pointers to full nested struct references
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TestStruct5()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct5.txt");
            NativeStruct nt = (NativeStruct)result.ParsedTypes[0];
            VerifyMembers(nt, "s1", "int", "i", "s1*", "next");
        }

        /// <summary>
        /// Verify the Type defs that occur after the struct definition
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TestStruct6()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct6.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "int", "i");
            VerifyTypeDef(result.ParsedTypes[1], "t1", "s1");
            VerifyStruct(result.ParsedTypes[2], "s2", "int", "i");
            VerifyTypeDef(result.ParsedTypes[3], "t2", "s2");
            VerifyTypeDef(result.ParsedTypes[4], "t3", "s2");
        }

        /// <summary>
        /// Verify the Type defs that occur after the struct definition when they
        /// contain pointer references
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TestStruct7()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct7.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "int", "i");
            VerifyTypeDef(result.ParsedTypes[1], "t1", "s1*");
            VerifyStruct(result.ParsedTypes[2], "s2", "int", "i");
            VerifyTypeDef(result.ParsedTypes[3], "t2", "s2*");
            VerifyTypeDef(result.ParsedTypes[4], "t3", "s2**");
        }

        [Fact()]
        public void TestStruct8()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct8.txt");
            VerifyStruct(result.ParsedTypes[0], string.Empty, "int", "i");
            VerifyStruct(result.ParsedTypes[1], "s1", string.Empty, "j", "int", "i");
            VerifyStruct(result.ParsedTypes[2], string.Empty, "s2*", "i");
            VerifyStruct(result.ParsedTypes[3], "s2", "int", "i", string.Empty, "j");
        }

        [Fact()]
        public void TestStruct9()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct9.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "int[]", "i");
            VerifyStruct(result.ParsedTypes[1], "s2", "int[5]", "i");
            VerifyStruct(result.ParsedTypes[2], "s3", "int[5]", "i");
        }

        [Fact()]
        public void TestStruct10()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct10.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "unsigned int", "i");
            VerifyStruct(result.ParsedTypes[1], "s2", "unsigned int", "i", "unsigned int", "j");
        }

        [Fact()]
        public void TestStruct11()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct11.txt");
            VerifyStruct(result.ParsedTypes[0], "_s1", "unsigned int", "i");
            VerifyTypeDef(result.ParsedTypes[1], "s1", "_s1");
            VerifyStruct(result.ParsedTypes[2], "_s2", "unsigned int", "i", "unsigned int", "j");
            VerifyTypeDef(result.ParsedTypes[3], "s2", "_s2");
            VerifyTypeDef(result.ParsedTypes[4], "ps2", "_s2*");

        }

        [Fact()]
        public void TestStruct12()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct12.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "<bitvector 5>", "i");
            VerifyStruct(result.ParsedTypes[1], "s2", "<bitvector 5>", "i", "<bitvector 6>", "j");
            VerifyStruct(result.ParsedTypes[2], "s3", "<bitvector 5>", "i", "<bitvector 6>", "j", "int", "k");
        }

        [Fact()]
        public void TestStruct13()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct13.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "int", "AnonymousMember1", "char", "c");
        }

        [Fact()]
        public void TestStruct14()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct14.txt");
            VerifyStruct(result.ParsedTypes[1], "s1", "int", "AnonymousMember1", "void* (*s1_pFPtr)(int)", "AnonymousMember2");
        }

        [Fact()]
        public void TestStruct15()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct15.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "char", "m1", "char", "m2");
            VerifyStruct(result.ParsedTypes[1], "s2", "char", "m1", "int", "m2", "int", "m3");
        }

        [Fact()]
        public void TestStruct16()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct16.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "int[20]", "m1");
            VerifyStruct(result.ParsedTypes[1], "s2", "int[20]", "m1");
            VerifyStruct(result.ParsedTypes[2], "s3", "int[]", "m1", "int[]", "m2");
        }

        [Fact()]
        public void TestStruct17()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct17.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "int[15]", "m1");
            VerifyStruct(result.ParsedTypes[1], "s2", "int[5]", "m1");
            VerifyStruct(result.ParsedTypes[2], "s3", "int[40]", "m1");
        }

        /// <summary>
        /// Structs with __ptr32 and __ptr64 members
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TestStruct18()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct18.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "int*", "m1", "char*", "m2");
            VerifyStruct(result.ParsedTypes[1], "s2", "int*", "m1", "char*", "m2");
            VerifyStruct(result.ParsedTypes[2], "s3", "int*", "m1", "char*", "m2");
        }

        [Fact()]
        public void TestStruct19()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct19.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "<bitvector 8>", "m1", "<bitvector 10>", "m2");
            VerifyStruct(result.ParsedTypes[1], "s2", "int", "m1", "int", "m2");
        }

        [Fact()]
        public void TestStruct20()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct20.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "unsigned char", "m1", "double", "m2", "char", "m3");
        }

        [Fact()]
        public void TestStruct21()
        {
            ParseResult result = ParseFile("SampleFiles\\Struct21.txt");
            VerifyStruct(result.ParsedTypes[0], "s1", "int", "m1", "char", "m2", "char", "m3");
            VerifyStruct(result.ParsedTypes[1], "s2", "int", "m1", "char", "m2", "char", "m3");
        }

        [Fact()]
        public void TestClass1()
        {
            ParseResult result = ParseFile("SampleFiles\\class1.txt");
            VerifyStruct(result.ParsedTypes[0], "c1", "int", "m1");
            VerifyStruct(result.ParsedTypes[1], "c2", "char*", "m1", "char", "m2");
        }

        [Fact()]
        public void TestUnion1()
        {
            ParseResult result = ParseFile("SampleFiles\\Union1.txt");
            VerifyUnion(result.ParsedTypes[0], "u1", "int", "i");
            VerifyUnion(result.ParsedTypes[1], "u2", "char", "i", "int", "j");
        }

        [Fact()]
        public void TestUnion2()
        {
            ParseResult result = ParseFile("SampleFiles\\Union2.txt");
            VerifyUnion(result.ParsedTypes[0], "u1", "int", "i");
            VerifyTypeDef(result.ParsedTypes[1], "t1", "u1");
            VerifyUnion(result.ParsedTypes[2], "u2", "char", "i", "int", "j");
            VerifyTypeDef(result.ParsedTypes[3], "t2", "u2");
            VerifyTypeDef(result.ParsedTypes[4], "t3", "u2*");
        }

        [Fact()]
        public void Mixed1()
        {
            ParseResult result = ParseFile("SampleFiles\\Mixed1.txt");
            VerifyUnion(result.ParsedTypes[0], string.Empty, "int", "i", "int", "j");
            VerifyStruct(result.ParsedTypes[1], "s1", string.Empty, "i", "char", "j");
        }

        [Fact()]
        public void Mixed2()
        {
            ParseResult result = ParseFile("SampleFiles\\Mixed2.txt");
            VerifyStruct(result.ParsedTypes[0], string.Empty, "int", "i");
            VerifyUnion(result.ParsedTypes[1], string.Empty, string.Empty, "i", "int", "j");
            VerifyStruct(result.ParsedTypes[2], "s1", string.Empty, "i", "char", "j");
        }

        [Fact()]
        public void Mixed3()
        {
            ParseResult result = ParseFile("SampleFiles\\Mixed3.txt");
            VerifyUnion(result.ParsedTypes[0], string.Empty, "int", "j", "float", "i");
            VerifyStruct(result.ParsedTypes[1], "s1", string.Empty, "Union1", "char", "k");
        }

        [Fact()]
        public void TypeDef1()
        {
            ParseResult result = ParseFile("SampleFiles\\TypeDef1.txt");
            VerifyTypeDef(result.ParsedTypes[0], "foo", "int");
            VerifyTypeDef(result.ParsedTypes[1], "bar", "char");
        }

        [Fact()]
        public void TypeDef2()
        {
            ParseResult result = ParseFile("SampleFiles\\TypeDef2.txt");
            VerifyTypeDef(result.ParsedTypes[0], "foo1", "int");
            VerifyTypeDef(result.ParsedTypes[1], "foo2", "int");
            VerifyTypeDef(result.ParsedTypes[2], "bar1", "char");
            VerifyTypeDef(result.ParsedTypes[3], "bar2", "char");
        }

        [Fact()]
        public void TypeDef3()
        {
            ParseResult result = ParseFile("SampleFiles\\TypeDef3.txt");
            VerifyTypeDef(result.ParsedTypes[0], "foo1", "int");
            VerifyTypeDef(result.ParsedTypes[1], "foo2", "int*");
            VerifyTypeDef(result.ParsedTypes[2], "bar1", "char");
            VerifyTypeDef(result.ParsedTypes[3], "bar2", "char*");
        }

        [Fact()]
        public void Typedef4()
        {
            ParseResult result = ParseFile("SampleFiles\\TypeDef4.txt");
            Assert.Equal("foo1(int)", SymbolPrinter.Convert(result.NativeTypedefs[0]));
            Assert.Equal("foo2(*(int))", SymbolPrinter.Convert(result.NativeTypedefs[1]));
            Assert.Equal("LPWSTR(*(wchar))", SymbolPrinter.Convert(result.NativeTypedefs[2]));
            Assert.Equal("FOO(*(wchar))", SymbolPrinter.Convert(result.NativeTypedefs[3]));
        }

        [Fact()]
        public void Typedef5()
        {
            ParseResult result = ParseFile("SampleFiles\\TypeDef5.txt");
            Assert.Equal("CINT(int(int))", SymbolPrinter.Convert(result.NativeTypedefs[0]));
            Assert.Equal("LPCSTR(*(WCHAR))", SymbolPrinter.Convert(result.NativeTypedefs[1]));
        }

        [Fact()]
        public void Typedef6()
        {
            ParseResult result = ParseFile("SampleFiles\\TypeDef6.txt");
            Assert.Equal("intarray([](int))", SymbolPrinter.Convert(result.NativeTypedefs[0]));
            Assert.Equal("chararray([](char))", SymbolPrinter.Convert(result.NativeTypedefs[1]));
        }

        [Fact()]
        public void Typedef7()
        {
            ParseResult result = ParseFile("SampleFiles\\TypeDef7.txt");
            Assert.Equal("s1(_s1(m1(int)))", SymbolPrinter.Convert(result.NativeTypedefs[0]));
        }

        [Fact()]
        public void Typedef8()
        {
            ParseResult result = ParseFile("SampleFiles\\TypeDef8.txt");
            Assert.Equal("f1(f1(Sig(int)(Sal)))", SymbolPrinter.Convert(result.NativeTypedefs[0]));
            Assert.Equal("f2(f2(Sig(*(int))(Sal)(param1(int)(Sal))))", SymbolPrinter.Convert(result.NativeTypedefs[1]));
        }

        [Fact()]
        public void Typedef9()
        {
            ParseResult result = ParseFile("SampleFiles\\TypeDef9.txt");
            Assert.Equal("td1(char)", SymbolPrinter.Convert(result.NativeTypedefs[0]));
            Assert.Equal("td2(char)", SymbolPrinter.Convert(result.NativeTypedefs[1]));
        }

        [Fact()]
        public void Enum1()
        {
            ParseResult result = ParseFile("SampleFiles\\Enum1.txt");
            VerifyEnum(result.ParsedTypes[0], "e1", "v1", "", "v2", "");
            VerifyEnum(result.ParsedTypes[1], "e2", "v1", "", "v2", "");
            VerifyEnum(result.ParsedTypes[2], "e3", "v1", "");
        }

        [Fact()]
        public void Enum2()
        {
            ParseResult result = ParseFile("SampleFiles\\Enum2.txt");
            VerifyEnum(result.ParsedTypes[0], "e1", "v1", "", "v2", "");
            VerifyTypeDef(result.ParsedTypes[1], "t1_e1", "e1");
            VerifyEnum(result.ParsedTypes[2], "e2", "v1", "", "v2", "");
            VerifyTypeDef(result.ParsedTypes[3], "t1_e2", "e2");
            VerifyTypeDef(result.ParsedTypes[4], "pt2_e2", "e2*");
        }

        [Fact()]
        public void Enum3()
        {
            ParseResult result = ParseFile("SampleFiles\\Enum3.txt");
            VerifyEnum(result.ParsedTypes[0], "e1", "v1", "1", "v2", "");
            VerifyEnum(result.ParsedTypes[1], "e2", "v1", "2", "v2", "v1+1");
        }

        [Fact()]
        public void Enum4()
        {
            ParseResult result = ParseFile("SampleFiles\\Enum4.txt");
            VerifyEnum(result.ParsedTypes[0], "_e1", "v1", "", "v2", "");
            VerifyTypeDef(result.ParsedTypes[1], "e1", "_e1");
            VerifyEnum(result.ParsedTypes[2], "_e2", "v1", "", "v2", "");
            VerifyTypeDef(result.ParsedTypes[3], "e2", "_e2");
            VerifyEnum(result.ParsedTypes[4], "_e3", "v1", "");
            VerifyTypeDef(result.ParsedTypes[5], "e3", "_e3");
            VerifyEnum(result.ParsedTypes[6], "e4");
        }

        [Fact()]
        public void Proc1()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc1.txt");
            VerifyProc(result, 0, "void p1()");
            VerifyProc(result, 1, "void p2(int i)");
            VerifyProc(result, 2, "void p3(int i, int j)");
            VerifyProc(result, 3, "int p4(int i, int j)");
        }

        [Fact()]
        public void Proc2()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc2.txt");
            VerifyProc(result, 0, "void p1()");
            VerifyProc(result, 1, "void p2(int i)");
            VerifyProc(result, 2, "void p3(int i, int j)");
            VerifyProc(result, 3, "int p4(int i, int j)");
        }

        [Fact()]
        public void Proc3()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc3.txt");
            VerifyProc(result, 0, "void p1(int* i)");
            VerifyProc(result, 1, "void p2(int** i)");
            VerifyProc(result, 2, "void p3(int** i)");
            VerifyProc(result, 3, "void p4(int[] i)");
        }

        [Fact()]
        public void Proc4()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc4.txt");
            VerifyProc(result, 0, "s1 p1(int* i)");
            VerifyProc(result, 1, "u1 p2(int** i)");
            VerifyProc(result, 2, "e1 p3(int** i)");
        }

        [Fact()]
        public void Proc5()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc5.txt");
            VerifyProc(result, 0, "void p1()");
            VerifyProc(result, 1, "int* p2()");
        }

        [Fact()]
        public void Proc6()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc6.txt");
            VerifyProc(result, 0, "void p1(int* p1)");
            VerifyProc(result, 1, "void p2(char** p1)");
            VerifyProc(result, 2, "void p3(char** p1)");
            VerifyProc(result, 3, "void p4(int*** p1)");
        }

        /// <summary>
        /// Ignore calltype specifiers
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc7()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc7.txt");
            VerifyProc(result, 0, "void p1()");
            VerifyProc(result, 1, "void p2()");
            VerifyProc(result, 2, "void p3()");
            VerifyProc(result, 3, "void p4()");
            VerifyProc(result, 4, "void p5()");
            VerifyProc(result, 5, "void p6()");
            Assert.Equal(6, result.NativeProcedures.Count);
        }

        /// <summary>
        /// Make sure that we are ignoring the volatile keyword
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc8()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc8.txt");
            VerifyProc(result, 0, "int p1(int a1)");
            VerifyProc(result, 1, "int p2(int a1)");
        }

        /// <summary>
        /// Function pointers as parameters and return types
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc9()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc9.txt");
            VerifyProc(result, 0, "void p1(int (*anonymous)(int) fp1)");
            VerifyProc(result, 1, "void p2(int* (*anonymous)(int* a1) fp1)");
            VerifyProc(result, 2, "void p3(int* (*anonymous)())");
        }

        /// <summary>
        /// Test the use of __ptr32 and __ptr64 in a procedure definition
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc10()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc10.txt");
            VerifyProc(result, 0, "void* p1(int* a1)");
            VerifyProc(result, 1, "void* p2(int* a1)");
            VerifyProc(result, 2, "void* p3(int* a1)");
            VerifyProc(result, 3, "void* p4(int* a1)");
        }

        [Fact()]
        public void Proc11()
        {
            ParseResult result = FullParseFile("SampleFiles\\Proc11.txt");
            VerifyProc(result, 0, "void p1(s1* a1)");
            VerifyProc(result, 1, "void p2(e1* a1)");
            VerifyProc(result, 2, "void p3(u1* a1)");
            VerifyProc(result, 3, "s1* p4()");
            VerifyProc(result, 4, "e1* p5()");
            VerifyProc(result, 5, "u1* p6()");
        }

        [Fact()]
        public void Complex1()
        {
            ParseResult result = FullParseFile("SampleFiles\\Complex1.txt");
            VerifyPrint(result, 0, "LPWSTR(*(wchar))");
            Assert.Equal(SymbolPrinter.Convert(result.NativeProcedures[0]), "p1(Sig(void)(Sal)(foo(LPWSTR)(Sal)))");
        }

        [Fact()]
        public void Sal1()
        {
            string text = "void p1(__in char c)";
            ParseResult result = SalParse(text);
            VerifyProcSal(result, 0, "void p1(Pre,Valid,Pre,Deref,ReadOnly char c)");
        }

        /// <summary>
        /// Sal with a parameter
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Sal2()
        {
            string text = "void p1(__ecount(1) char c)";
            ParseResult result = SalParse(text);
            VerifyProcSal(result, 0, "void p1(NotNull,ElemWritableTo(1) char c)");
        }

        /// <summary>
        /// __declspec with a non quoted string argument
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Sal3()
        {
            string text = "typdef __declspec(5) struct _s1 { int m1; } s1;";
            ParseResult result = SalParse(text);
            VerifyStruct(result.ParsedTypes[0], "_s1", "int", "m1");
        }

        /// <summary>
        /// __declspec with a different macro call that is just not valid
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Sal4()
        {
            string text = "typedef __declspec(align(5)) struct _s1 { int m1; } s1;";
            ParseResult result = SalParse(text);
            VerifyStruct(result.ParsedTypes[0], "_s1", "int", "m1");
        }

        [Fact()]
        public void Sal5()
        {
            string text = "struct __declspec(5) s1 { int m1; };";
            ParseResult result = SalParse(text);
            VerifyStruct(result.ParsedTypes[0], "s1", "int", "m1");
        }

        [Fact()]
        public void FuncPtr1()
        {
            ParseResult result = FullParseFile("SampleFiles\\FuncPtr1.txt");
            VerifyFuncPtr(result, 0, "int (*f1)()");
            VerifyFuncPtr(result, 1, "int (*f2)(int)");
            VerifyFuncPtr(result, 2, "int (*f3)(char f)");
            VerifyFuncPtr(result, 3, "int (*f4)(char f, int j)");
            VerifyFuncPtr(result, 4, "int* (*f5)(int j)");
        }

        [Fact()]
        public void FuncPtr2()
        {
            ParseResult result = FullParseFile("SampleFiles\\FuncPtr2.txt");
            VerifyFuncPtr(result, 0, "int (*f1)()");
            VerifyFuncPtr(result, 1, "int (*f2)()");
            VerifyFuncPtr(result, 2, "int* (*f3)()");
            VerifyFuncPtr(result, 3, "int (*f4)()");
        }

        [Fact()]
        public void FuncPtr3()
        {
            ParseResult result = FullParseFile("SampleFiles\\FuncPtr3.txt");
            VerifyFuncPtr(result, 0, "int (*f1)(int a1)");
            VerifyFuncPtr(result, 1, "int* (*f2)(int a1)");
        }

        [Fact()]
        public void FuncPtr4()
        {
            ParseResult result = FullParseFile("SampleFiles\\FuncPtr4.txt");
            VerifyFuncPtr(result, 0, "int (*f1)(int a1)");
            VerifyFuncPtr(result, 1, "int* (*f2)(int a1)");
        }

        /// <summary>
        /// List of unsupported scenarios that we are parsing
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Errors1()
        {
            ParseResult result = FullParseFile("SampleFiles\\Errors1.txt");

            // C++ attribute
            Assert.Equal("C++ attributes are not supported: [uuid(55)]", result.ErrorProvider.Warnings[0]);
            VerifyStruct(result.ParsedTypes[0], "s1", "int", "m1");

            // Inline procedure 
            Assert.Equal("Ignoring Procedure p2 because it is defined inline.", result.ErrorProvider.Warnings[1]);
            VerifyStruct(result.ParsedTypes[1], "s2", "int", "m1");

            // Variable argument
            Assert.Equal("Procedure p3 has a variable argument signature which is unsupported.", result.ErrorProvider.Warnings[2]);
            VerifyStruct(result.ParsedTypes[2], "s3", "int", "m1");

            // Variable argument and inline
            Assert.Equal("Procedure p4 has a variable argument signature which is unsupported.", result.ErrorProvider.Warnings[3]);
            VerifyStruct(result.ParsedTypes[3], "s4", "int", "m1");

            // Member procedure
            Assert.Equal("Type member procedures are not supported: s5.p1", result.ErrorProvider.Warnings[4]);
            VerifyStruct(result.ParsedTypes[4], "s5", "int", "m1");

            // Member procedure inline
            Assert.Equal("Type member procedures are not supported: s6.p1", result.ErrorProvider.Warnings[5]);
            VerifyStruct(result.ParsedTypes[5], "s6", "int", "m1");

            Assert.Equal("Type member procedures are not supported: s7.p1", result.ErrorProvider.Warnings[6]);
            VerifyStruct(result.ParsedTypes[7], "s7", "int", "m1");

            // Member procedure with const qualifier
            Assert.Equal("Type member procedures are not supported: s8.p1", result.ErrorProvider.Warnings[7]);
            VerifyStruct(result.ParsedTypes[8], "s8", "int", "m1");

        }
    }
}