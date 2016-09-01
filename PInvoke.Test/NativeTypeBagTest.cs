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
    public class NativeSymbolBagTest
    {

        ///<summary>
        ///A test for AddDefinedType(ByVal PInvoke.Parser.NativeDefinedType)
        ///</summary>
        [Fact()]
        public void AddDefinedTypeTest()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeStruct definedNt1 = new NativeStruct("s1");
            bag.AddDefinedType(definedNt1);

            NativeType ret1 = null;
            Assert.True(bag.TryFindType(definedNt1.DisplayName, out ret1));
            Assert.Same(ret1, definedNt1);
        }

        [Fact()]
        public void AddDefinedTypeTest2()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeStruct definedNt1 = new NativeStruct("s1");
            NativeStruct definedNt2 = new NativeStruct("s2");
            bag.AddDefinedType(definedNt1);
            bag.AddDefinedType(definedNt2);

            NativeType ret1 = null;
            NativeType ret2 = null;
            Assert.True(bag.TryFindType(definedNt1.DisplayName, out ret1));
            Assert.True(bag.TryFindType(definedNt2.DisplayName, out ret2));
            Assert.Same(ret1, definedNt1);
            Assert.Same(ret2, definedNt2);
        }

        /// <summary>
        /// Adding the same type twice should throw an exception
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void AddDefinedTypeTest3()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeStruct definedNt1 = new NativeStruct("s1");
            NativeStruct definedNt2 = new NativeStruct("s1");
            bag.AddDefinedType(definedNt1);
            Assert.Throws<ArgumentException>(() => bag.AddDefinedType(definedNt2));
        }

        [Fact()]
        public void AddTypeDef1()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeTypeDef td1 = new NativeTypeDef("td1");

            bag.AddTypeDef(td1);

            NativeType ret1 = null;
            Assert.True(bag.TryFindType(td1.DisplayName, out ret1));
            Assert.Same(ret1, td1);

        }

        [Fact()]
        public void AddTypeDef2()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeTypeDef td1 = new NativeTypeDef("td1");
            NativeTypeDef td2 = new NativeTypeDef("td2");

            bag.AddTypeDef(td1);
            bag.AddTypeDef(td2);

            NativeType ret1 = null;
            NativeType ret2 = null;
            Assert.True(bag.TryFindType(td1.DisplayName, out ret1));
            Assert.Same(ret1, td1);
            Assert.True(bag.TryFindType(td2.DisplayName, out ret2));
            Assert.Same(ret2, td2);
        }

        /// <summary>
        /// adding the same typedefe twice should throw an exception
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void AddTypeDef3()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeTypeDef td1 = new NativeTypeDef("td1");
            NativeTypeDef td2 = new NativeTypeDef("td1");

            bag.AddTypeDef(td1);
            Assert.Throws<ArgumentException>(() => bag.AddTypeDef(td2));
        }

        [Fact()]
        public void AddMixed1()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeStruct definedNt1 = new NativeStruct("s1");
            bag.AddDefinedType(definedNt1);
            NativeTypeDef td1 = new NativeTypeDef("td1");
            bag.AddTypeDef(td1);

            NativeType ret = null;
            Assert.True(bag.TryFindType(definedNt1.DisplayName, out ret));
            Assert.Same(definedNt1, ret);
            Assert.True(bag.TryFindType(td1.DisplayName, out ret));
            Assert.Same(td1, ret);

        }

        /// <summary>
        /// TypeDefs and NativedefinedTypes can have the same full name.  Doesn't make
        /// much sense in the real world but it's allowed for flexibility in the parser
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void AddMixed2()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeStruct definedNt1 = new NativeStruct("s1");
            bag.AddDefinedType(definedNt1);
            NativeTypeDef td1 = new NativeTypeDef("s1");
            bag.AddTypeDef(td1);

            NativeType ret = null;
            Assert.True(bag.TryFindType(definedNt1.DisplayName, out ret));
            Assert.Same(definedNt1, ret);
            Assert.True(bag.TryFindType(td1.DisplayName, out ret));
            Assert.Same(definedNt1, ret);
        }

        /// <summary>
        /// Resolve a simple type def
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Resolve1()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeStruct s1 = new NativeStruct("s1");
            bag.AddDefinedType(s1);

            NativeTypeDef td1 = new NativeTypeDef("td1");
            NativeNamedType n1 = new NativeNamedType("s1");
            td1.RealType = n1;
            bag.AddTypeDef(td1);

            Assert.True(bag.TryResolveSymbolsAndValues());
            Assert.Same(s1, n1.RealType);
        }

        /// <summary>
        /// Resolve a pointer type
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Resolve2()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeStruct s1 = new NativeStruct("s1");
            bag.AddDefinedType(s1);

            NativeTypeDef td1 = new NativeTypeDef("td1");
            NativeNamedType n1 = new NativeNamedType("s1");
            NativePointer p1 = new NativePointer(n1);
            td1.RealType = p1;
            bag.AddTypeDef(td1);

            Assert.True(bag.TryResolveSymbolsAndValues());
            Assert.Same(s1, n1.RealType);
        }

        /// <summary>
        /// Simple proc add
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc1()
        {
            NativeSymbolBag bag = new NativeSymbolBag();

            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeDouble);

            bag.AddProcedure(p1);
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Resolve a procedure with a simple parameter
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc2()
        {
            NativeSymbolBag bag = new NativeSymbolBag();

            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeDouble);
            p1.Signature.Parameters.Add(new NativeParameter("param1", new NativeBuiltinType(BuiltinType.NativeDouble)));

            bag.AddProcedure(p1);
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Unresolvable parameter
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc3()
        {
            NativeSymbolBag bag = new NativeSymbolBag();

            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeDouble);
            p1.Signature.Parameters.Add(new NativeParameter("param1", new NativeNamedType("foo")));

            bag.AddProcedure(p1);
            Assert.False(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Unresolavable return type
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc4()
        {
            NativeSymbolBag bag = new NativeSymbolBag();

            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeNamedType("foo");
            p1.Signature.Parameters.Add(new NativeParameter("param1", new NativeBuiltinType(BuiltinType.NativeDouble)));

            bag.AddProcedure(p1);
            Assert.False(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// resolve a named return type 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc5()
        {
            NativeSymbolBag bag = new NativeSymbolBag();

            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeNamedType("foo");
            p1.Signature.Parameters.Add(new NativeParameter("param1", new NativeBuiltinType(BuiltinType.NativeDouble)));

            bag.AddProcedure(p1);
            bag.AddTypeDef(new NativeTypeDef("foo", new NativeBuiltinType(BuiltinType.NativeFloat)));
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// resolve a named parameter
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc6()
        {
            NativeSymbolBag bag = new NativeSymbolBag();

            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeInt32, true);
            p1.Signature.Parameters.Add(new NativeParameter("param1", new NativeNamedType("foo")));

            bag.AddProcedure(p1);
            bag.AddTypeDef(new NativeTypeDef("foo", new NativeBuiltinType(BuiltinType.NativeFloat)));
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        [Fact()]
        public void FindOrLoad1()
        {
            var ns = new NativeStorage();
            ns.AddDefinedType(new NativeStruct("s1"));
            NativeSymbolBag bag = new NativeSymbolBag(ns);

            NativeDefinedType s1 = null;
            Assert.False(bag.Storage.TryFindDefined("s1", out s1));
            Assert.True(bag.TryFindDefined("s1", out s1));
            Assert.True(bag.TryFindDefined("s1", out s1));
        }

        [Fact()]
        public void FindOrLoad2()
        {
            var ns = new NativeStorage();
            ns.AddTypeDef(new NativeTypeDef("td", new NativeBuiltinType(BuiltinType.NativeChar)));
            NativeSymbolBag bag = new NativeSymbolBag(ns);

            NativeTypeDef td = null;
            Assert.False(bag.Storage.TryFindTypeDef("td", out td));
            Assert.True(bag.TryFindTypeDef("td", out td));
            Assert.True(bag.TryFindTypeDef("td", out td));
        }

        [Fact()]
        public void FindOrLoad3()
        {
            var ns = new NativeStorage();
            ns.AddConstant(new NativeConstant("c1", "value"));
            NativeSymbolBag bag = new NativeSymbolBag(ns);

            NativeConstant c = null;
            Assert.False(bag.Storage.TryFindConstant("c1", out c));
            Assert.True(bag.TryFindConstant("c1", out c));
            Assert.True(bag.TryFindConstant("c1", out c));
        }

        [Fact()]
        public void FindOrLoad4()
        {
            var ns = new NativeStorage();
            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeBoolean);
            ns.AddProcedure(p1);
            NativeSymbolBag bag = new NativeSymbolBag(ns);

            NativeProcedure p = null;
            Assert.False(bag.Storage.TryFindProcedure("p1", out p));
            Assert.True(bag.TryFindProcedure("p1", out p));
            Assert.True(bag.TryFindProcedure("p1", out p));
        }

        /// <summary>
        /// Make sure we can load the cases where the user does a 
        /// "typedef struct foo foo"
        /// 
        /// This is a common C practice and we need to be able to resolve this correctly
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void FindOrLoad5()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddTypeDef(new NativeTypeDef("foo", new NativeNamedType("struct", "foo")));
            bag.AddDefinedType(new NativeStruct("foo"));

            NativeType nt = null;
            Assert.True(bag.TryResolveNamedType(new NativeNamedType("struct", "foo"), out nt));
            Assert.Equal(NativeSymbolKind.StructType, nt.Kind);
        }

        /// <summary>
        /// When a type is loaded during a resolve it should be added
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void ResolveLoad1()
        {
            NativeStorage ns = new NativeStorage();
            ns.AddDefinedType(new NativeStruct("s1"));

            NativeSymbolBag bag = new NativeSymbolBag(ns);
            bag.AddTypeDef(new NativeTypeDef("S1", "s1"));
            Assert.True(bag.TryResolveSymbolsAndValues());

            NativeDefinedType td = null;
            Assert.True(bag.TryFindDefined("s1", out td));
        }

        [Fact()]
        public void ResolveLoad2()
        {
            NativeStorage ns = new NativeStorage();
            ns.AddTypeDef(new NativeTypeDef("TEST_INT", BuiltinType.NativeInt32));

            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeNamedType("TEST_INT")));

            NativeSymbolBag bag = new NativeSymbolBag(ns);
            bag.AddDefinedType(s1);
            Assert.True(bag.TryResolveSymbolsAndValues());

            NativeTypeDef td = null;
            Assert.True(bag.TryFindTypeDef("TEST_INT", out td));
        }


        [Fact()]
        public void Anonymous1()
        {
            string name = NativeSymbolBag.GenerateAnonymousName();
            Assert.True(NativeSymbolBag.IsAnonymousName(name));
        }

        [Fact()]
        public void Anonymous2()
        {
            Assert.False(NativeSymbolBag.IsAnonymousName("foo"));
        }

        [Fact()]
        public void AnonymousType1()
        {
            NativeStruct nt = new NativeStruct();
            nt.IsAnonymous = true;
            Assert.False(string.IsNullOrEmpty(nt.Name));

            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddDefinedType(nt);
            Assert.True(NativeSymbolBag.IsAnonymousName(nt.Name));
        }

        /// <summary>
        /// Make sure we can load a type that has a recursive reference to itself
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Storage1()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.NextSymbolBag = StorageFactory.CreateStandard();

            NativeType nt = null;
            Assert.True(bag.TryFindType("RecursiveStruct", out nt));
        }

        /// <summary>
        /// Use one const to resolve the other
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Value1()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            bag.AddConstant(new NativeConstant("foo", "1"));
            bag.AddConstant(new NativeConstant("bar", "foo+2"));

            Assert.Equal(1, bag.FindUnresolvedNativeValues().Count);
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Resolve one enum value against the other
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Value2()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeEnum ntEnum = new NativeEnum("Enum1");
            ntEnum.Values.Add(new NativeEnumValue("v1", "1"));
            ntEnum.Values.Add(new NativeEnumValue("v2", "v1+1"));
            bag.AddDefinedType(ntEnum);
            Assert.Equal(1, bag.FindUnresolvedNativeValues().Count);
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Resolve a cast expression type
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Value3()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeStruct ntStruct1 = new NativeStruct("s1");
            bag.AddDefinedType(ntStruct1);

            NativeConstant ntConst1 = new NativeConstant("c1", "(s1)1");
            bag.AddConstant(ntConst1);

            Assert.Equal(1, bag.FindUnresolvedNativeValues().Count);
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Resolve a constant value
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Value4()
        {
            NativeSymbolBag bag = new NativeSymbolBag();
            NativeConstant ntConst1 = new NativeConstant("c1", "1");
            bag.AddConstant(ntConst1);
            NativeConstant ntConst2 = new NativeConstant("c2", "5+c1");
            bag.AddConstant(ntConst2);

            Assert.Equal(1, bag.FindUnresolvedNativeValues().Count);
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Make sure that a SymbolValue can be properly loaded from storage
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void ValueFromStorage1()
        {
            var ns = new NativeStorage();
            ns.AddConstant(new NativeConstant("c1", "1"));
            var bag = new NativeSymbolBag(ns);
            bag.AddConstant(new NativeConstant("c2", "5+c1"));

            Assert.Equal(1, bag.FindUnresolvedNativeValues().Count);
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Make sure an enum value can be loaded from storage
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void ValueFromStorage2()
        {
            var ns = new NativeStorage();
            var bag = new NativeSymbolBag(ns);

            NativeEnum ntEnum = new NativeEnum("e1");
            ntEnum.Values.Add(new NativeEnumValue("v1", "5"));
            bag.AddDefinedType(ntEnum);

            NativeConstant ntConst1 = new NativeConstant("c1", "5+v1");
            bag.AddConstant(ntConst1);

            Assert.Equal(1, bag.FindUnresolvedNativeValues().Count);
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Make sure cast expression types can be loaded from storage
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void ValueFromStorage3()
        {
            var ns = new NativeStorage();
            NativeSymbolBag bag = new NativeSymbolBag(ns);
            ns.AddDefinedType(new NativeStruct("s1"));

            NativeConstant ntConst1 = new NativeConstant("c1", "(s1)1");
            bag.AddConstant(ntConst1);

            Assert.Equal(1, bag.FindUnresolvedNativeValues().Count);
            Assert.True(bag.TryResolveSymbolsAndValues());
        }

        /// <summary>
        /// Make sure that opaque types are resolved
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Opaque1()
        {
            NativeNamedType named = new NativeNamedType("struct", "foo");
            NativePointer ptr = new NativePointer(named);
            NativeTypeDef td = new NativeTypeDef("FOOBAR", ptr);
            NativeSymbolBag bag = new NativeSymbolBag();

            bag.AddTypeDef(td);
            Assert.Equal(1, bag.FindUnresolvedNativeSymbolRelationships().Count);
            Assert.True(bag.TryResolveSymbolsAndValues());
            Assert.NotNull(named.RealType);
            Assert.Equal(NativeSymbolKind.OpaqueType, named.RealType.Kind);
        }

        /// <summary>
        /// Don't resolve an unqualified type name to an opaque type 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Opaque2()
        {
            NativeNamedType named = new NativeNamedType("foo");
            NativePointer ptr = new NativePointer(named);
            NativeTypeDef td = new NativeTypeDef("FOOBAR", ptr);
            NativeSymbolBag bag = new NativeSymbolBag();

            bag.AddTypeDef(td);
            Assert.Equal(1, bag.FindUnresolvedNativeSymbolRelationships().Count);
            Assert.False(bag.TryResolveSymbolsAndValues());
        }

    }
}
