// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using PInvoke;
using PInvoke.Transform;
using Xunit;

namespace PInvoke.Test
{
    public class NativeSymbolTransformTest
    {

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

        [Fact()]
        public void CollapseNamed()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeNamedType("char", new NativeBuiltinType(BuiltinType.NativeChar))));
            VerifyTree(s1, "s1(m1(char(char)))");

            NativeSymbolTransform transform = new NativeSymbolTransform();
            transform.CollapseNamedTypes(s1);
            VerifyTree(s1, "s1(m1(char))");
        }

        /// <summary>
        /// Collapsing a null type shouldn't do anything
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void CallapseNamed2()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeNamedType("char")));
            VerifyTree(s1, "s1(m1(char))");

            NativeSymbolTransform transform = new NativeSymbolTransform();
            transform.CollapseNamedTypes(s1);
            VerifyTree(s1, "s1(m1(char))");
        }

        [Fact()]
        public void CollapseTypedef1()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeTypeDef("PCHAR", new NativePointer(new NativeBuiltinType(BuiltinType.NativeChar)))));
            VerifyTree(s1, "s1(m1(PCHAR(*(char))))");

            NativeSymbolTransform transform = new NativeSymbolTransform();
            transform.CollapseTypedefs(s1);
            VerifyTree(s1, "s1(m1(*(char)))");
        }

        /// <summary>
        /// Collapsing a null typedef shouldn't do anything
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void CollapseTypedef2()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeTypeDef("foo")));
            VerifyTree(s1, "s1(m1(foo))");

            NativeSymbolTransform transform = new NativeSymbolTransform();
            transform.CollapseNamedTypes(s1);
            VerifyTree(s1, "s1(m1(foo))");
        }

        /// <summary>
        /// Renaming a type is fun
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TypeRename1()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeChar)));

            NativeSymbolTransform transform = new NativeSymbolTransform();
            transform.RenameTypeSymbol(s1, "s1", "s2");
            VerifyTree(s1, "s2(m1(char))");
        }

        /// <summary>
        /// Rename through a typedef
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TypeRename2()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeChar)));
            NativeTypeDef td = new NativeTypeDef("foo", s1);

            NativeSymbolTransform transform = new NativeSymbolTransform();
            transform.RenameTypeSymbol(td, "s1", "s2");
            VerifyTree(td, "foo(s2(m1(char)))");
        }

        /// <summary>
        /// Don't rename members
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TypeRename3()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("s1", new NativeBuiltinType(BuiltinType.NativeChar)));

            NativeSymbolTransform transform = new NativeSymbolTransform();
            transform.RenameTypeSymbol(s1, "s1", "s2");
            VerifyTree(s1, "s2(s1(char))");
        }

        /// <summary>
        /// Make sure named types get renamed as well
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TypeRename4()
        {
            NativeStruct s1 = new NativeStruct("s1");
            s1.Members.Add(new NativeMember("m1", new NativeNamedType("s1", new NativeBuiltinType(BuiltinType.NativeByte))));

            NativeSymbolTransform transform = new NativeSymbolTransform();
            transform.RenameTypeSymbol(s1, "s1", "s2");
            VerifyTree(s1, "s2(m1(s2(byte)))");
        }
    }
}