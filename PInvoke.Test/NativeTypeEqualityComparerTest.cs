// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
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

    public class NativeTypeEqualityComparerTest
    {

        ///<summary>
        /// Basic test
        ///</summary>
        [Fact()]
        public void TopLevel1()
        {
            NativeBuiltinType nt1 = new NativeBuiltinType(BuiltinType.NativeByte);
            NativeBuiltinType nt2 = new NativeBuiltinType(BuiltinType.NativeByte);

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(nt1, nt2));

            nt2 = new NativeBuiltinType(BuiltinType.NativeInt32);
            Assert.False(eq.Equals1(nt1, nt2));

        }

        [Fact()]
        public void TopLevel3()
        {
            NativeBitVector m1 = new NativeBitVector(5);
            NativeBitVector m2 = new NativeBitVector(5);

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(m1, m2));

            m2 = new NativeBitVector(6);
            Assert.False(eq.Equals1(m1, m2));
        }

        /// <summary>
        /// Some proxy tests
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TopLevel4()
        {
            NativeBitVector m1 = new NativeBitVector(5);
            NativeBitVector m2 = new NativeBitVector(5);
            NativePointer p1 = new NativePointer(m1);
            NativePointer p2 = new NativePointer(m2);

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(p1, p2));

            p2.RealType = new NativeBitVector(6);
            Assert.False(eq.Equals1(p1, p2));
        }

        [Fact()]
        public void TopLevel5()
        {
            NativeBitVector m1 = new NativeBitVector(5);
            NativeBitVector m2 = new NativeBitVector(5);
            NativeArray p1 = new NativeArray(m1, 2);
            NativeArray p2 = new NativeArray(m2, 2);

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(p1, p2));

            p2.ElementCount = 3;
            Assert.False(eq.Equals1(p1, p2));

            p2.RealType = new NativeBitVector(6);
            Assert.False(eq.Equals1(p1, p2));

        }

        [Fact()]
        public void TopLevel6()
        {
            NativeBitVector m1 = new NativeBitVector(5);
            NativeBitVector m2 = new NativeBitVector(5);
            NativeTypeDef p1 = new NativeTypeDef("foo", m1);
            NativeTypeDef p2 = new NativeTypeDef("foo", m2);

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(p1, p2));

            p2.Name = "bar";
            Assert.False(eq.Equals1(p1, p2));

            p2.Name = p1.Name;
            p2.RealType = new NativeBitVector(6);
            Assert.False(eq.Equals1(p1, p2));
        }

        [Fact()]
        public void TopLevel7()
        {
            NativeBitVector m1 = new NativeBitVector(5);
            NativeBitVector m2 = new NativeBitVector(5);
            NativeNamedType p1 = new NativeNamedType("foo", m1);
            NativeNamedType p2 = new NativeNamedType("foo", m2);

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(p1, p2));

            // We just dig right through named types unless they don't have an underlying
            // real type
            p2.Name = "bar";
            Assert.True(eq.Equals1(p1, p2));

            p2.Name = p1.Name;
            p2.RealType = new NativeBitVector(6);
            Assert.False(eq.Equals1(p1, p2));
        }

        /// <summary>
        /// Struct comparison
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TopLevel8()
        {
            NativeStruct nd1 = new NativeStruct("s");
            NativeStruct nd2 = new NativeStruct("s");

            nd1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            nd2.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(nd1, nd2));

            nd1.Members.Clear();
            Assert.False(eq.Equals1(nd1, nd2));

        }

        /// <summary>
        /// Struct with sub structs that are different.  Not a problem for TopLevel
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TopLevel12()
        {
            NativeStruct sub1 = new NativeStruct("sub");
            NativeStruct sub2 = new NativeStruct("sub");
            sub1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            sub2.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeChar)));

            NativeStruct nd1 = new NativeStruct("n");
            NativeStruct nd2 = new NativeStruct("n");

            nd1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            nd2.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));

            nd1.Members.Add(new NativeMember("m2", sub1));
            nd2.Members.Add(new NativeMember("m2", sub2));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(nd1, nd2));
        }

        /// <summary>
        /// When comparing members of a defined type at the top level, their type matches if you 
        /// are comparing defined types and names
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void TopLevel13()
        {
            NativeStruct left = new NativeStruct("s");
            left.Members.Add(new NativeMember("m1", new NativeNamedType("foo")));

            NativeStruct right = new NativeStruct("s");
            right.Members.Add(new NativeMember("m1", new NativeStruct("foo")));

            Assert.True(NativeTypeEqualityComparer.AreEqualTopLevel(left, right));
        }

        /// <summary>
        /// Recursively compare the structures
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Recursive1()
        {
            NativeStruct sub1 = new NativeStruct("sub");
            NativeStruct sub2 = new NativeStruct("sub");
            sub1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            sub2.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));

            NativeStruct nd1 = new NativeStruct("n");
            NativeStruct nd2 = new NativeStruct("n");

            nd1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            nd2.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));

            nd1.Members.Add(new NativeMember("m2", sub1));
            nd2.Members.Add(new NativeMember("m2", sub2));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.Recursive;
            Assert.True(eq.Equals1(nd1, nd2));
        }

        /// <summary>
        /// Sub structs differ so they are not recursively the same
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Recursive2()
        {
            NativeStruct sub1 = new NativeStruct("sub");
            NativeStruct sub2 = new NativeStruct("sub");
            sub1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            sub2.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeChar)));

            NativeStruct nd1 = new NativeStruct("n");
            NativeStruct nd2 = new NativeStruct("n");

            nd1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            nd2.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));

            nd1.Members.Add(new NativeMember("m2", sub1));
            nd2.Members.Add(new NativeMember("m2", sub2));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.Recursive;
            Assert.False(eq.Equals1(nd1, nd2));
        }

        /// <summary>
        /// Compare enumerations
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void EnumEquality1()
        {
            NativeEnum e1 = new NativeEnum("e");
            NativeEnum e2 = new NativeEnum("e");

            e1.Values.Add(new NativeEnumValue("v1"));
            e2.Values.Add(new NativeEnumValue("v1"));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(e1, e2));
        }

        /// <summary>
        /// Differing values mean they are not equal
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void EnumEquality2()
        {
            NativeEnum e1 = new NativeEnum("e");
            NativeEnum e2 = new NativeEnum("e");

            e1.Values.Add(new NativeEnumValue("v1", "foo"));
            e2.Values.Add(new NativeEnumValue("v1"));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.False(eq.Equals1(e1, e2));
        }

        /// <summary>
        /// Differing value names mean they are not equal
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void EnumEquality3()
        {
            NativeEnum e1 = new NativeEnum("e");
            NativeEnum e2 = new NativeEnum("e");

            e1.Values.Add(new NativeEnumValue("v1"));
            e2.Values.Add(new NativeEnumValue("v2"));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.False(eq.Equals1(e1, e2));
        }

        /// <summary>
        /// differing value order means they are not equal
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void EnumEquality4()
        {
            NativeEnum e1 = new NativeEnum("e");
            NativeEnum e2 = new NativeEnum("e");

            e1.Values.Add(new NativeEnumValue("v2"));
            e1.Values.Add(new NativeEnumValue("v1"));

            e2.Values.Add(new NativeEnumValue("v1"));
            e2.Values.Add(new NativeEnumValue("v2"));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.False(eq.Equals1(e1, e2));
        }

        /// <summary>
        /// Equality should just walk through named types
        /// </summary>
        /// <remarks></remarks>
        [Fact()]

        public void IgnoreNamedTypes()
        {
            NativeEnum e1 = new NativeEnum("e");
            NativeEnum e2 = new NativeEnum("e");
            NativeNamedType n2 = new NativeNamedType("e", e2);

            e1.Values.Add(new NativeEnumValue("v1"));
            e2.Values.Add(new NativeEnumValue("v1"));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(e1, n2));
        }

        /// <summary>
        /// Names should be ignored when comparing anonymous types
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Anonymous1()
        {
            NativeStruct nd1 = new NativeStruct("ndaaoeu");
            NativeStruct nd2 = new NativeStruct("a");
            nd1.IsAnonymous = true;
            nd2.IsAnonymous = true;

            nd1.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            nd2.Members.Add(new NativeMember("m1", new NativeBuiltinType(BuiltinType.NativeBoolean)));

            nd1.Members.Add(new NativeMember("m2", new NativeBuiltinType(BuiltinType.NativeBoolean)));
            nd2.Members.Add(new NativeMember("m2", new NativeBuiltinType(BuiltinType.NativeBoolean)));

            NativeTypeEqualityComparer eq = NativeTypeEqualityComparer.TopLevel;
            Assert.True(eq.Equals1(nd1, nd2));
        }

    }
}
