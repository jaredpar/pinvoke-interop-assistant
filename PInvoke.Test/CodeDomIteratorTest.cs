// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.Transform;
using PInvoke.Transform.Enums;
using System;
using System.CodeDom;
using System.Collections.Generic;
using Xunit;

namespace PInvoke.Test
{
    ///<summary>
    ///This is a test class for PInvoke.Transform.CodeDomIterator and is intended
    ///to contain all PInvoke.Transform.CodeDomIterator Unit Tests
    ///</summary>
    public class CodeDomIteratorTest
    {
        private List<object> Convert(string code)
        {
            var converter = new BasicConverter(LanguageType.VisualBasic, StorageFactory.CreateStandard());
            var ctd = converter.ConvertNativeCodeToCodeDom(code, new ErrorProvider());
            var it = new CodeDomIterator();
            return it.Iterate(ctd);
        }

        private void ExpectTypeRef(string name, List<object> list)
        {
            foreach (object obj in list)
            {
                if (obj is CodeTypeReference typeRef)
                {
                    if (0 == string.CompareOrdinal(name, typeRef.BaseType))
                    {
                        return;
                    }
                }
            }

            throw new Exception("Could not find the type reference: " + name);
        }

        private void ExpectField(string name, List<object> list)
        {
            foreach (object obj in list)
            {
                if (obj is CodeMemberField field && 0 == string.CompareOrdinal(field.Name, name))
                {
                    return;
                }
            }

            throw new Exception("Could not find the field: " + name);
        }

        private void ExpectProc(string name, List<object> list)
        {
            foreach (object obj in list)
            {
                if (obj is CodeMemberMethod field && 0 == string.CompareOrdinal(field.Name, name))
                {
                    return;
                }
            }

            throw new Exception("Could not find the proc: " + name);
        }
        private void ExpectType(string name, List<object> list)
        {
            foreach (object obj in list)
            {
                if (obj is CodeTypeDeclaration ctd && 0 == string.CompareOrdinal(ctd.Name, name))
                {
                    return;
                }
            }

            throw new Exception("Could not find the type: " + name);
        }
        /// <summary>
        /// Normal types
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Depth1()
        {
            string code = "struct foo { int i; }; ";
            List<object> list = Convert(code);
            ExpectTypeRef("System.Int32", list);
        }

        [Fact()]
        public void Depth2()
        {
            string code = "union foo { int i; char j; }";
            List<object> list = Convert(code);
            ExpectTypeRef("System.Int32", list);
            ExpectTypeRef("System.Byte", list);
            ExpectField("j", list);
            ExpectField("i", list);
        }

        /// <summary>
        /// Enumeration members
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Depth3()
        {
            string code = "enum foo { v1, v2, };";
            List<object> list = Convert(code);
            ExpectField("v1", list);
            ExpectField("v2", list);
            ExpectType("foo", list);
        }

        [Fact()]
        public void IterateBitVector()
        {
            string code = "struct bar { int i: 4; }; ";
            List<object> list = Convert(code);
            ExpectType("bar", list);
        }

        [Fact()]
        public void IterateProc()
        {
            string code = "int foo(char* y);";
            List<object> list = Convert(code);
            ExpectTypeRef("System.Int32", list);
            ExpectProc("foo", list);
        }
    }
}