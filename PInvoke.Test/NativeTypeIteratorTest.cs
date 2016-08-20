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
    public class NativeSymbolIteratorTest
    {

        private void EnsureIsSymbol(NativeSymbol sym, List<NativeSymbolRelationship> list)
        {
            foreach (NativeSymbolRelationship rel in list)
            {
                if (object.ReferenceEquals(sym, rel.Symbol))
                {
                    return;
                }
            }

            throw new Exception("Could Not find the symbol");
        }

        private void EnsureIsParent(NativeSymbol sym, List<NativeSymbolRelationship> list)
        {
            foreach (NativeSymbolRelationship rel in list)
            {
                if (object.ReferenceEquals(sym, rel.Parent))
                {
                    return;
                }
            }

            throw new Exception("Could Not find the symbol");
        }

        ///<summary>
        /// Make sure that the original parent is returned
        ///</summary>
        [Fact()]
        public void FindAllRelationships1()
        {
            NativeBuiltinType ns = new NativeBuiltinType(BuiltinType.NativeByte);
            NativeSymbolIterator it = new NativeSymbolIterator();
            List<NativeSymbolRelationship> list = it.FindAllNativeSymbolRelationships(ns);

            Assert.Equal(1, list.Count);
            EnsureIsSymbol(ns, list);
        }

        /// <summary>
        /// Make sure the parent is returned as a parent
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void FindAllRelationships2()
        {
            NativeBuiltinType child = new NativeBuiltinType(BuiltinType.NativeByte);
            NativePointer par = new NativePointer(child);
            NativeSymbolIterator it = new NativeSymbolIterator();
            List<NativeSymbolRelationship> list = it.FindAllNativeSymbolRelationships(par);

            Assert.Equal(2, list.Count);
            EnsureIsSymbol(par, list);
            EnsureIsSymbol(child, list);
            EnsureIsParent(par, list);
        }

        /// <summary>
        /// Recursive relatioship should work just fine even though this tree is not techinally legal
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void FindAllRelationships3()
        {
            NativePointer ptr1 = new NativePointer();
            NativePointer ptr2 = new NativePointer(ptr1);
            ptr1.RealType = ptr2;

            NativeSymbolIterator it = new NativeSymbolIterator();
            List<NativeSymbolRelationship> list = it.FindAllNativeSymbolRelationships(ptr1);
            EnsureIsSymbol(ptr1, list);
            EnsureIsSymbol(ptr2, list);
        }

        [Fact()]
        public void FindAllNativeSymbols1()
        {
            NativeBuiltinType child = new NativeBuiltinType(BuiltinType.NativeByte);
            NativePointer par = new NativePointer(child);
            NativeSymbolIterator it = new NativeSymbolIterator();
            List<NativeSymbol> list = it.FindAllNativeSymbols(par);

            Assert.True(list.Contains(child));
            Assert.True(list.Contains(par));
        }

    }
}
