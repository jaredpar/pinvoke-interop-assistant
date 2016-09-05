// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using PInvoke;
using PInvoke.Parser;
using Xunit;

namespace PInvoke.Test
{
    public class NativeSymbolTest
    {
        private void VerifyReachable(NativeType nt, params string[] names)
        {
            NativeSymbolBag bag = new NativeSymbolBag();

            NativeDefinedType definedNt = nt as NativeDefinedType;
            NativeTypeDef typedefNt = nt as NativeTypeDef;
            if (definedNt != null)
            {
                bag.AddDefinedType((NativeDefinedType)nt);
            }
            else if (typedefNt != null)
            {
                bag.AddTypeDef((NativeTypeDef)nt);
            }
            else
            {
                throw new Exception("Not a searchable type");
            }

            VerifyReachable(bag, names);
        }

        private void VerifyReachable(NativeSymbolBag bag, params string[] names)
        {
            Assert.NotNull(bag);
            Dictionary<string, NativeType> map = new Dictionary<string, NativeType>();
            foreach (NativeSymbol curSym in bag.FindAllReachableNativeSymbols())
            {
                NativeType cur = curSym as NativeType;
                if (cur != null)
                {
                    NativeType nt = cur as NativeType;
                    if (nt != null)
                    {
                        map.Add(cur.DisplayName, nt);
                    }
                }
            }

            Assert.Equal(names.Length, map.Count);
            foreach (string name in names)
            {
                Assert.True(map.ContainsKey(name), "NativeType with name " + name + " not reachable");
            }

        }

        private string Print(NativeSymbol ns)
        {
            if (ns == null)
            {
                return "<Nothing>";
            }

            string str = ns.Name;
            foreach (NativeSymbol child in ns.GetChildren())
            {
                str += "(" + Print(child) + ")";
            }

            return str;
        }

        private void VerifyTree(NativeSymbol ns, string str)
        {
            string realStr = Print(ns);
            Assert.Equal(str, realStr);
        }

        /// <summary>
        /// simple test with no children
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Iterate1()
        {
            NativeStruct nt1 = new NativeStruct();
            nt1.Name = "s1";
            VerifyReachable(nt1, "s1");
        }

        [Fact()]
        public void Iterate2()
        {
            NativeStruct nt1 = new NativeStruct();
            nt1.Name = "s1";
            nt1.Members.Add(new NativeMember("f", new NativeBuiltinType(BuiltinType.NativeInt32)));
            VerifyReachable(nt1, "s1", "int");
        }


        /// <summary>
        /// Simple test with a couple of structs
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Iterate3()
        {
            NativeStruct nt1 = new NativeStruct();
            nt1.Name = "s1";

            NativeStruct nt2 = new NativeStruct();
            nt2.Name = "s2";

            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddDefinedType(nt1);
            bag.AddDefinedType(nt2);
            VerifyReachable(bag, "s1", "s2");
        }

        /// <summary>
        /// Test a simple proxy type
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Iterate4()
        {
            NativeTypeDef nt1 = new NativeTypeDef("td1");
            NativeNamedType nt2 = new NativeNamedType("n1");
            nt1.RealType = nt2;
            VerifyReachable(nt1, "td1", "n1");
        }

        /// <summary>
        /// Proxy within a container
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Iterate5()
        {
            NativeStruct nt1 = new NativeStruct("s1");
            NativeTypeDef nt2 = new NativeTypeDef("td1");
            NativeNamedType nt3 = new NativeNamedType("n1");
            nt2.RealType = nt3;
            nt1.Members.Add(new NativeMember("foo", nt2));
            VerifyReachable(nt1, "s1", "td1", "n1");
        }

        /// <summary>
        /// Play around with structs
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Child1()
        {
            NativeStruct s1 = new NativeStruct("s1");
            VerifyTree(s1, "s1");
            s1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeChar)));
            VerifyTree(s1, "s1(m1(char))");
            s1.Members.Add(new NativeMember("m2", new NativeBuiltinType(BuiltinType.NativeByte)));
            VerifyTree(s1, "s1(m1(char))(m2(byte))");
        }

        /// <summary>
        /// Replace the children of a struct
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Child2()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeByte)));
            s1.ReplaceChild(s1.Members[0], new NativeMember("m2", new NativeBuiltinType(BuiltinType.NativeDouble)));
            VerifyTree(s1, "s1(m2(double))");
        }

        /// <summary>
        /// Children of an enumeration
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Child3()
        {
            NativeEnum e1 = new NativeEnum("e1");
            e1.AddValue("n1", "v1");
            VerifyTree(e1, "e1(n1(Value(v1)))");
            e1.AddValue("n2", "v2");
            VerifyTree(e1, "e1(n1(Value(v1)))(n2(Value(v2)))");

        }

        /// <summary>
        /// adding a member to an enum shouldn't be part of the enumeration
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Child4()
        {
            NativeEnum e1 = new NativeEnum("e1");
            e1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeByte)));
            VerifyTree(e1, "e1");
        }

        /// <summary>
        /// Verify an enum replace 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Child5()
        {
            NativeEnum e1 = new NativeEnum("e1");
            e1.Values.Add(new NativeEnumValue("e1", "n1", "v1"));
            e1.ReplaceChild(e1.Values[0], new NativeEnumValue("e1", "n2", "v2"));
            VerifyTree(e1, "e1(n2(Value(v2)))");
        }

        /// <summary>
        /// Verify a proc
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Child6()
        {
            NativeProcedure proc = new NativeProcedure("proc");
            proc.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeByte);
            VerifyTree(proc, "proc(Sig(byte)(Sal))");
            proc.Signature.Parameters.Add(new NativeParameter("p1", new NativeBuiltinType(BuiltinType.NativeChar)));
            VerifyTree(proc, "proc(Sig(byte)(Sal)(p1(char)(Sal)))");
        }

        /// <summary>
        /// Replace the parameters of a proc
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Child7()
        {
            NativeProcedure proc = new NativeProcedure("proc");
            proc.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeByte);
            proc.Signature.Parameters.Add(new NativeParameter("p1", new NativeBuiltinType(BuiltinType.NativeChar)));
            proc.Signature.ReplaceChild(proc.Signature.ReturnType, new NativeBuiltinType(BuiltinType.NativeFloat));
            VerifyTree(proc, "proc(Sig(float)(Sal)(p1(char)(Sal)))");
            proc.Signature.ReplaceChild(proc.Signature.Parameters[0], new NativeParameter("p2", new NativeBuiltinType(BuiltinType.NativeChar)));
            VerifyTree(proc, "proc(Sig(float)(Sal)(p2(char)(Sal)))");
        }

    }

    public class NativeBuiltinTypeTest
    {

        [Fact()]
        public void TestAll()
        {
            foreach (BuiltinType bt in System.Enum.GetValues(typeof(BuiltinType)))
            {
                if (bt != BuiltinType.NativeUnknown)
                {
                    NativeBuiltinType nt = new NativeBuiltinType(bt);
                    Assert.Equal(nt.Name, NativeBuiltinType.BuiltinTypeToName(bt));
                    Assert.Equal(bt, nt.BuiltinType);
                    Assert.NotNull(nt.ManagedType);
                    Assert.NotEqual(0, Convert.ToInt32(nt.UnmanagedType));
                }
            }
        }

        /// <summary>
        /// Unknown type is used to handle situations where we just don't know what's going
        /// on so we add the unknown type.  Typically meant for use by third parties
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Unknown1()
        {
            NativeBuiltinType nt = new NativeBuiltinType("foo");
            Assert.Equal(BuiltinType.NativeUnknown, nt.BuiltinType);
            Assert.Equal("unknown", nt.Name);
            Assert.Equal("unknown", nt.DisplayName);
        }

        [Fact()]
        public void EnsureTypeKeywordToBuiltin()
        {
            foreach (TokenType cur in EnumUtil.GetAllValues<TokenType>())
            {
                if (TokenHelper.IsTypeKeyword(cur))
                {
                    NativeBuiltinType bt = null;
                    Assert.True(NativeBuiltinType.TryConvertToBuiltinType(cur, out bt));
                }
            }
        }


    }

    public class NativeProxyTypeTest
    {

        /// <summary>
        /// Basic typedef cases
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Dig1()
        {
            NativeTypeDef td = new NativeTypeDef("foo");
            td.RealType = new NativeBuiltinType(BuiltinType.NativeByte);
            Assert.Same(td.RealType, td.DigThroughTypeDefAndNamedTypes());

            NativeTypeDef outerTd = new NativeTypeDef("bar");
            outerTd.RealType = td;
            Assert.Same(td.RealType, outerTd.DigThroughTypeDefAndNamedTypes());
        }

        /// <summary>
        /// Simple tests with named types
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Dig2()
        {
            NativeNamedType named = new NativeNamedType("foo");
            named.RealType = new NativeBuiltinType(BuiltinType.NativeByte);
            Assert.Same(named.RealType, named.DigThroughTypeDefAndNamedTypes());

            NativeNamedType outerNamed = new NativeNamedType("bar");
            outerNamed.RealType = named;
            Assert.Same(named.RealType, outerNamed.DigThroughTypeDefAndNamedTypes());
        }

        /// <summary>
        /// Hit some null blocks
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Dig3()
        {
            NativeNamedType named = new NativeNamedType("foo");
            Assert.Null(named.DigThroughTypeDefAndNamedTypes());
        }

        /// <summary>
        /// Don't dig through pointers
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Dig4()
        {
            NativePointer pt = new NativePointer(BuiltinType.NativeByte);
            Assert.Same(pt, pt.DigThroughTypeDefAndNamedTypes());

            NativeTypeDef td = new NativeTypeDef("foo", pt);
            Assert.Same(pt, td.DigThroughTypeDefAndNamedTypes());
        }

        /// <summary>
        /// Dig and search
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Dig5()
        {
            NativePointer pt1 = new NativePointer(new NativeTypeDef("foo", BuiltinType.NativeFloat));
            Assert.Equal(NativeSymbolKind.BuiltinType, pt1.RealType.DigThroughTypeDefAndNamedTypes().Kind);
            Assert.Equal(NativeSymbolKind.TypeDefType, pt1.RealType.DigThroughTypeDefAndNamedTypesFor("foo").Kind);
            Assert.Null(pt1.RealType.DigThroughTypeDefAndNamedTypesFor("bar"));
        }

        /// <summary>
        /// Dig and search again
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Dig6()
        {
            NativeNamedType named = new NativeNamedType("bar", new NativeTypeDef("td1", BuiltinType.NativeFloat));
            NativeTypeDef td = new NativeTypeDef("foo", named);

            Assert.Equal(NativeSymbolKind.TypeDefType, td.DigThroughTypeDefAndNamedTypesFor("foo").Kind);
            Assert.Same(td, td.DigThroughTypeDefAndNamedTypesFor("foo"));
            Assert.Equal(NativeSymbolKind.BuiltinType, td.DigThroughTypeDefAndNamedTypes().Kind);
            Assert.Equal(NativeSymbolKind.NamedType, td.DigThroughTypeDefAndNamedTypesFor("bar").Kind);

            NativeNamedType named2 = new NativeNamedType("named2", td);
            Assert.Equal(NativeSymbolKind.TypeDefType, named2.DigThroughNamedTypesFor("foo").Kind);
            Assert.Null(named2.DigThroughNamedTypesFor("bar"));
        }

        /// <summary>
        /// Parameters should have sal attributes
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Sal1()
        {
            NativeParameter param = new NativeParameter();
            Assert.NotNull(param.SalAttribute);
        }

        /// <summary>
        /// The return type should have a sal attribute by default
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Sal2()
        {
            NativeProcedure proc = new NativeProcedure();
            Assert.NotNull(proc.Signature.ReturnTypeSalAttribute);
        }

        /// <summary>
        /// Make sure each sal entry has a directive
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Sal3()
        {
            foreach (SalEntryType e in System.Enum.GetValues(typeof(SalEntryType)))
            {
                Assert.False(string.IsNullOrEmpty(NativeSalEntry.GetDirectiveForEntry(SalEntryType.ElemReadableTo)));
            }
        }
    }

    public class NativeParameterTest
    {

        [Fact()]
        public void Pre()
        {
            NativeParameter param = new NativeParameter();
            Assert.NotNull(param.Name);
        }

        /// <summary>
        /// To be resolved we only need a type.  Function pointer parameters don't have to have names
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Resolved()
        {
            NativeParameter param = new NativeParameter();
            Assert.False(param.IsImmediateResolved);
            param.NativeType = new NativeBuiltinType(BuiltinType.NativeByte);
            Assert.True(param.IsImmediateResolved);
            param.Name = "foo";
            Assert.True(param.IsImmediateResolved);
        }
    }

    public class NativeProcedureTest
    {

        [Fact()]
        public void Pre()
        {
            NativeProcedure proc = new NativeProcedure();
            Assert.NotNull(proc.Signature);
        }

    }

    public class NativeFunctionPointerTest
    {

        [Fact()]
        public void Pre()
        {
            NativeFunctionPointer ptr = new NativeFunctionPointer("foo");
            Assert.NotNull(ptr.Signature);
        }

    }

    public class NativeValueExpressionTest
    {

        [Fact()]
        public void Value1()
        {
            NativeValueExpression expr = new NativeValueExpression("1+1");
            Assert.Equal(2, expr.Values.Count);
            Assert.Equal(NativeValueKind.Number, expr.Values[0].ValueKind);
            Assert.Equal("1", expr.Values[0].DisplayValue);
            Assert.Equal(1, Convert.ToInt32(expr.Values[0].Value));
        }

        [Fact()]
        public void Value2()
        {
            NativeValueExpression expr = new NativeValueExpression("FOO+1");
            Assert.Equal(2, expr.Values.Count);
            Assert.Equal("FOO", expr.Values[0].DisplayValue);
            Assert.Equal("FOO", expr.Values[0].Name);
            Assert.Null(expr.Values[0].SymbolValue);
        }

        [Fact()]
        public void Value3()
        {
            NativeValueExpression expr = new NativeValueExpression("FOO+BAR");
            Assert.Equal(2, expr.Values.Count);
            Assert.Equal("FOO", expr.Values[0].DisplayValue);
            Assert.Equal("BAR", expr.Values[1].DisplayValue);
        }

        [Fact()]
        public void Value4()
        {
            NativeValueExpression expr = new NativeValueExpression("\"bar\"+1");
            Assert.Equal(2, expr.Values.Count);
            Assert.Equal(NativeValueKind.String, expr.Values[0].ValueKind);
            Assert.Equal("bar", expr.Values[0].DisplayValue);
        }

        /// <summary>
        /// Test the parsing of cast operations
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Value5()
        {
            NativeValueExpression expr = new NativeValueExpression("(DWORD)5");
            Assert.Equal(2, expr.Values.Count);

            NativeValue val = expr.Values[0];
            Assert.Equal(NativeValueKind.SymbolType, val.ValueKind);
            Assert.Equal("DWORD", val.DisplayValue);

            val = expr.Values[1];
            Assert.Equal(NativeValueKind.Number, val.ValueKind);
            Assert.Equal(5, Convert.ToInt32(val.Value));
        }

        [Fact()]
        public void Value6()
        {
            NativeValueExpression expr = new NativeValueExpression("(DWORD)(5+6)");
            Assert.Equal(3, expr.Values.Count);

            NativeValue val = expr.Values[0];
            Assert.Equal(NativeValueKind.SymbolType, val.ValueKind);
            Assert.Equal("DWORD", val.DisplayValue);

            val = expr.Values[1];
            Assert.Equal(NativeValueKind.Number, val.ValueKind);
            Assert.Equal(5, Convert.ToInt32(val.Value));

            val = expr.Values[2];
            Assert.Equal(NativeValueKind.Number, val.ValueKind);
            Assert.Equal(6, Convert.ToInt32(val.Value));
        }

        /// <summary>
        /// Make sure than bad value expressions are marked as resolvable
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void BadValue1()
        {
            NativeValueExpression expr = new NativeValueExpression("&&&");
            Assert.True(expr.IsImmediateResolved);
            Assert.False(expr.IsParsable);
        }

        /// <summary>
        /// Reseting the value should cause a re-parse 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void BadValue2()
        {
            NativeValueExpression expr = new NativeValueExpression("&&&");
            Assert.True(expr.IsImmediateResolved);
            Assert.False(expr.IsParsable);
            Assert.Equal(0, expr.Values.Count);
            expr.Expression = "1+1";
            Assert.True(expr.IsImmediateResolved);
            Assert.True(expr.IsParsable);
            Assert.Equal(2, expr.Values.Count);
        }

    }

    public class NativeValueTest
    {

        [Fact()]
        public void Resolve1()
        {
            NativeValue val = NativeValue.CreateNumber(1);
            Assert.Equal(1, Convert.ToInt32(val.Value));
            Assert.Equal(NativeValueKind.Number, val.ValueKind);
            Assert.True(val.IsImmediateResolved);
        }

        [Fact()]
        public void Resolve2()
        {
            NativeValue val = NativeValue.CreateString("foo");
            Assert.Equal("foo", Convert.ToString(val.Value));
            Assert.Equal(NativeValueKind.String, val.ValueKind);
            Assert.True(val.IsImmediateResolved);
        }

        [Fact()]
        public void Resolve3()
        {
            NativeValue val = NativeValue.CreateSymbolType("foo");
            Assert.Equal("foo", val.Name);
            Assert.Equal(NativeValueKind.SymbolType, val.ValueKind);
            Assert.True(val.IsImmediateResolved);
            val.Value = new NativeBuiltinType(BuiltinType.NativeByte);
            Assert.True(val.IsImmediateResolved);
        }

        [Fact()]
        public void Resolve4()
        {
            NativeValue val = NativeValue.CreateSymbolValue("bar");
            Assert.Equal("bar", val.Name);
            Assert.Equal(NativeValueKind.SymbolValue, val.ValueKind);
            Assert.True(val.IsImmediateResolved);
            val.Value = new NativeBuiltinType(BuiltinType.NativeByte);
            Assert.True(val.IsImmediateResolved);
        }

        [Fact()]
        public void Resolve5()
        {
            NativeValue val = NativeValue.CreateSymbolValue("foo", new NativeBuiltinType(BuiltinType.NativeBoolean));
            Assert.Equal("foo", val.Name);
            Assert.Equal(NativeValueKind.SymbolValue, val.ValueKind);
            Assert.NotNull(val.SymbolValue);
            Assert.Null(val.SymbolType);
            Assert.True(val.IsImmediateResolved);
        }

        [Fact()]
        public void Resolve6()
        {
            NativeValue val = NativeValue.CreateSymbolType("foo", new NativeBuiltinType(BuiltinType.NativeBoolean));
            Assert.Equal("foo", val.Name);
            Assert.Equal(NativeValueKind.SymbolType, val.ValueKind);
            Assert.Null(val.SymbolValue);
            Assert.NotNull(val.SymbolType);
            Assert.True(val.IsImmediateResolved);
        }

        [Fact()]
        public void Resolve7()
        {
            NativeValue val = NativeValue.CreateCharacter('c');
            Assert.Equal("c", Convert.ToString(val.Value));
            Assert.Equal(NativeValueKind.Character, val.ValueKind);
            Assert.True(val.IsImmediateResolved);
        }

        /// <summary>
        /// Value should not update the enumeration 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Dynamic1()
        {
            NativeValue val = NativeValue.CreateNumber(1);
            Assert.Equal(NativeValueKind.Number, val.ValueKind);
            val.Value = 42;
            Assert.Equal(NativeValueKind.Number, val.ValueKind);
            Assert.Equal(42, Convert.ToInt32(val.Value));
        }

        /// <summary>
        /// Changing the type should not update the kind
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Dynamic2()
        {
            NativeValue val = NativeValue.CreateNumber(42);
            Assert.Equal(NativeValueKind.Number, val.ValueKind);
            val.Value = "foo";
            Assert.Equal(NativeValueKind.Number, val.ValueKind);
            Assert.Equal("foo", Convert.ToString(val.Value));
        }

        [Fact()]
        public void IsValueResolved1()
        {
            NativeValue val = NativeValue.CreateBoolean(true);
            Assert.True(val.IsValueResolved);
            val.Value = null;
            Assert.False(val.IsValueResolved);
        }

        [Fact()]
        public void IsValueResolved2()
        {
            NativeValue val = NativeValue.CreateCharacter('c');
            Assert.True(val.IsValueResolved);
            val.Value = null;
            Assert.False(val.IsValueResolved);
        }

        [Fact()]
        public void IsValueResolved3()
        {
            NativeValue val = NativeValue.CreateNumber(42);
            Assert.True(val.IsValueResolved);
            val.Value = null;
            Assert.False(val.IsValueResolved);
            val.Value = 42;
            Assert.True(val.IsValueResolved);
        }

        [Fact()]
        public void IsValueResolved4()
        {
            NativeValue val = NativeValue.CreateSymbolType("foo");
            Assert.False(val.IsValueResolved);
            val.Value = new NativeStruct("foo");
            Assert.True(val.IsValueResolved);
            val.Value = null;
            Assert.False(val.IsValueResolved);
        }

        [Fact()]
        public void IsValueResolved5()
        {
            NativeValue val = NativeValue.CreateSymbolValue("foo");
            Assert.False(val.IsValueResolved);
            val.Value = new NativeStruct("foo");
            Assert.True(val.IsValueResolved);
            val.Value = null;
            Assert.False(val.IsValueResolved);
        }

    }

    public class NativeConstantTest
    {

        [Fact()]
        public void Empty()
        {
            NativeConstant c1 = new NativeConstant("c1");
            Assert.Equal(ConstantKind.Macro, c1.ConstantKind);
            Assert.Equal("c1", c1.Name);
        }

        [Fact()]
        public void Value1()
        {
            NativeConstant c1 = new NativeConstant("p", "1+2");
            Assert.Equal(ConstantKind.Macro, c1.ConstantKind);
            Assert.Equal("1+2", c1.Value.Expression);
            Assert.Equal("p", c1.Name);
        }

        /// <summary>
        /// Make sure that we quote macro method values to ensure that they
        /// are "resolvable"
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method1()
        {
            string sig = "(x) x+1";
            NativeConstant c1 = new NativeConstant("c1", sig, ConstantKind.MacroMethod);
            Assert.Equal(ConstantKind.MacroMethod, c1.ConstantKind);
            Assert.Equal("\"" + sig + "\"", c1.Value.Expression);
            Assert.Equal("c1", c1.Name);
        }
    }
}
