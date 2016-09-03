// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using PInvoke;
using Xunit;
namespace PInvoke.Test
{

    public class NativeStorageTest
    {

        [Fact()]
        public void LoadByName1()
        {
            NativeStruct s1 = new NativeStruct("s");
            s1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeInt32)));

            NativeType s2 = null;

            var ns = new BasicSymbolStorage();
            ns.AddDefinedType(s1);

            Assert.True(ns.TryGetGlobalSymbol(s1.Name, out s2));
        }

        [Fact()]
        public void LoadByName2()
        {
            NativeStruct s1 = new NativeStruct("s");
            s1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeInt32)));
            s1.Members.Add(new NativeMember("m2", new NativeBuiltinType(BuiltinType.NativeByte)));
            s1.Members.Add(new NativeMember("m3", new NativeBitVector(6)));
            s1.Members.Add(new NativeMember("m4", new NativePointer(new NativeBuiltinType(BuiltinType.NativeChar))));
            s1.Members.Add(new NativeMember("m5", new NativeArray(new NativeBuiltinType(BuiltinType.NativeFloat), 4)));
            s1.Members.Add(new NativeMember("m7", new NativeNamedType("bar", new NativeBuiltinType(BuiltinType.NativeDouble))));


            NativeType s2 = null;

            var ns = new BasicSymbolStorage();
            ns.AddDefinedType(s1);

            Assert.True(ns.TryGetType(s1.Name, out s2));
        }

        /// <summary>
        /// Proc without parameters and a simple return type
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc1()
        {
            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeByte);

            var ns = new BasicSymbolStorage();
            ns.AddProcedure(p1);

            NativeProcedure retp1 = null;
            Assert.True(ns.TryGetGlobalSymbol(p1.Name, out retp1));
            Assert.Equal(p1.DisplayName, retp1.DisplayName);
        }

        /// <summary>
        /// Proc with a non-trivial return type
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc2()
        {
            NativeStruct s1 = new NativeStruct("s1");
            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = s1;

            var ns = new BasicSymbolStorage();
            ns.AddProcedure(p1);

            NativeProcedure retp1 = null;
            Assert.True(ns.TryGetGlobalSymbol(p1.Name, out retp1));
            Assert.Equal(p1.DisplayName, retp1.DisplayName);
        }

        /// <summary>
        /// Proc with a simple parameter
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc3()
        {
            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeByte);
            p1.Signature.Parameters.Add(new NativeParameter("param1", new NativeBuiltinType(BuiltinType.NativeDouble)));

            var ns = new BasicSymbolStorage();
            ns.AddProcedure(p1);

            NativeProcedure retp1 = null;
            Assert.True(ns.TryGetGlobalSymbol(p1.Name, out retp1));
            Assert.Equal(p1.DisplayName, retp1.DisplayName);
        }

        /// <summary>
        /// Adding a procedure should not recursively add the type of it's parameters or return type
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Proc4()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeByte)));

            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = s1;

            var ns = new BasicSymbolStorage();
            ns.AddProcedure(p1);

            NativeProcedure retp1 = null;
            Assert.True(ns.TryGetGlobalSymbol(p1.Name, out retp1));
            Assert.Equal(p1.DisplayName, retp1.DisplayName);

            NativeDefinedType rets1 = null;
            Assert.False(ns.TryGetGlobalSymbol(s1.Name, out rets1));
        }

        /// <summary>
        /// Make sure that only a shollow save is done
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void SaveAndLoad1()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeFloat)));

            NativeStruct s2 = new NativeStruct("s2");
            s2.Members.Add(new NativeMember("m1", s1));

            var ns = new BasicSymbolStorage();
            ns.AddDefinedType(s2);

            NativeDefinedType rets2 = null;
            Assert.True(ns.TryGetGlobalSymbol(s2.Name, out rets2));
            Assert.NotNull(rets2);
            Assert.False(NativeTypeEqualityComparer.AreEqualRecursive(s2, rets2));
            Assert.True(NativeTypeEqualityComparer.AreEqualTopLevel(s2, rets2));
        }

        /// <summary>
        /// Save a type that has a reference to itself (via a pointer)
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void SaveAndLoad2()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativePointer(s1)));

            var ns = new BasicSymbolStorage();
            ns.AddDefinedType(s1);

            NativeType rets1 = null;
            Assert.True(ns.TryGetType(s1.Name, out rets1));
            Assert.NotNull(rets1);
            Assert.True(NativeTypeEqualityComparer.AreEqualTopLevel(s1, rets1));
        }

        /// <summary>
        /// Save a struct that points to a named type.  This should succeed
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void SaveAndLoad3()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeNamedType("foo")));

            var ns = new BasicSymbolStorage();
            ns.AddDefinedType(s1);

            NativeType rets1 = null;
            Assert.True(ns.TryGetType(s1.Name, out rets1));
            Assert.True(NativeTypeEqualityComparer.AreEqualTopLevel(s1, rets1));
        }

        /// <summary>
        /// Load a typedef by name
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void SaveAndLoad4()
        {
            NativeTypeDef t1 = new NativeTypeDef("t1");
            t1.RealType = new NativeBuiltinType(BuiltinType.NativeByte);

            var ns = new BasicSymbolStorage();
            ns.AddTypeDef(t1);

            NativeType rett1 = null;
            Assert.True(ns.TryGetType(t1.Name, out rett1));
            Assert.True(NativeTypeEqualityComparer.AreEqualRecursive(rett1, t1));
        }

        /// <summary>
        /// Save and load a constant
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void SaveAndLoad5()
        {
            NativeConstant c1 = new NativeConstant("c1", "v1");
            NativeConstant c2 = new NativeConstant("c2", "v2", ConstantKind.MacroMethod);
            var ns = new BasicSymbolStorage();
            ns.AddConstant(c1);
            ns.AddConstant(c2);

            NativeConstant ret = null;
            Assert.True(ns.TryGetGlobalSymbol("c1", out ret));
            Assert.Equal("c1", ret.Name);
            Assert.Equal("v1", ret.Value.Expression);
            Assert.Equal(ConstantKind.Macro, ret.ConstantKind);

            Assert.True(ns.TryGetGlobalSymbol("c2", out ret));
            Assert.Equal("c2", ret.Name);
            Assert.Equal("\"v2\"", ret.Value.Expression);
            Assert.Equal(ConstantKind.MacroMethod, ret.ConstantKind);
        }

        /// <summary>
        /// Make sure calling conventions are properly saved
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void SaveAndLoad6()
        {
            NativeFunctionPointer fptr = new NativeFunctionPointer("f1");
            Assert.Equal(NativeCallingConvention.WinApi, fptr.CallingConvention);
            fptr.CallingConvention = NativeCallingConvention.Pascal;
            fptr.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeChar);

            NativeProcedure proc = new NativeProcedure("p1");
            Assert.Equal(NativeCallingConvention.WinApi, proc.CallingConvention);
            proc.CallingConvention = NativeCallingConvention.CDeclaration;
            proc.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeChar);

            var ns = new BasicSymbolStorage();
            ns.AddProcedure(proc);
            ns.AddDefinedType(fptr);

            NativeDefinedType temp = null;
            NativeFunctionPointer retPtr = null;
            Assert.True(ns.TryGetGlobalSymbol(fptr.Name, out temp));
            retPtr = (NativeFunctionPointer)temp;
            Assert.Equal(NativeCallingConvention.Pascal, retPtr.CallingConvention);

            NativeProcedure retProc = null;
            Assert.True(ns.TryGetGlobalSymbol(proc.Name, out retProc));
            Assert.Equal(NativeCallingConvention.CDeclaration, retProc.CallingConvention);


        }

        [Fact()]
        public void BagSaveAndLoad1()
        {
            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeBoolean);

            NativeTypeDef td = new NativeTypeDef("LPWSTR", new NativePointer(BuiltinType.NativeWChar));
            p1.Signature.Parameters.Add(new NativeParameter("param1", new NativeNamedType("LPWSTR", td)));
            Assert.Equal("boolean p1(LPWSTR param1)", p1.DisplayName);
            Assert.Equal("p1(Sig(boolean)(Sal)(param1(LPWSTR(LPWSTR(*(wchar))))(Sal)))", SymbolPrinter.Convert(p1));

            var ns = new BasicSymbolStorage();
            ns.AddProcedure(p1);
            ns.AddTypeDef(td);

            NativeSymbolBag bag = new NativeSymbolBag(ns);
            NativeProcedure ret1 = null;
            Assert.True(bag.TryGetGlobalSymbol("p1", out ret1));
            bag.AddProcedure(ret1);
            Assert.True(bag.TryResolveSymbolsAndValues());
            Assert.Equal(SymbolPrinter.Convert(p1), SymbolPrinter.Convert(ret1));
        }

        [Fact()]
        public void Sal1()
        {
            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeChar);
            p1.Signature.ReturnTypeSalAttribute = new NativeSalAttribute(SalEntryType.ReadOnly);

            var ns = new BasicSymbolStorage();
            ns.AddProcedure(p1);

            NativeProcedure retp1 = null;
            Assert.True(ns.TryGetGlobalSymbol(p1.Name, out retp1));
            Assert.Equal("ReadOnly", retp1.Signature.ReturnTypeSalAttribute.DisplayName);
        }

        [Fact()]
        public void Sal2()
        {
            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeChar);
            p1.Signature.ReturnTypeSalAttribute = new NativeSalAttribute(new NativeSalEntry(SalEntryType.Deref, "foo"));

            var ns = new BasicSymbolStorage();
            ns.AddProcedure(p1);

            NativeProcedure retp1 = null;
            Assert.True(ns.TryGetGlobalSymbol(p1.Name, out retp1));
            Assert.Equal("Deref(foo)", retp1.Signature.ReturnTypeSalAttribute.DisplayName);
        }

        [Fact()]
        public void Sal3()
        {
            NativeParameter param = new NativeParameter("p");
            param.SalAttribute = new NativeSalAttribute(SalEntryType.Deref);
            param.NativeType = new NativeBuiltinType(BuiltinType.NativeChar);

            NativeProcedure p1 = new NativeProcedure("p1");
            p1.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeChar);
            p1.Signature.Parameters.Add(param);

            var ns = new BasicSymbolStorage();
            ns.AddProcedure(p1);

            NativeProcedure retp1 = null;
            Assert.True(ns.TryGetGlobalSymbol(p1.Name, out retp1));
            Assert.Equal("Deref", retp1.Signature.Parameters[0].SalAttribute.DisplayName);
        }

        [Fact()]
        public void FuncPtr1()
        {
            NativeFunctionPointer fptr = new NativeFunctionPointer("f1");
            fptr.Signature.ReturnType = new NativeBuiltinType(BuiltinType.NativeChar);
            fptr.Signature.Parameters.Add(new NativeParameter("f", new NativeBuiltinType(BuiltinType.NativeFloat)));

            var ns = new BasicSymbolStorage();
            ns.AddDefinedType(fptr);

            NativeDefinedType retFptr = null;
            Assert.True(ns.TryGetGlobalSymbol(fptr.Name, out retFptr));
            Assert.Equal("char (*f1)(float f)", ((NativeFunctionPointer)retFptr).DisplayName);
        }

    }
}
