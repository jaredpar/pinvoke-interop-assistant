// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke.NativeTypes.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static PInvoke.Contract;

namespace PInvoke.NativeTypes
{
    /// <summary>
    /// Represents a native symbol we're interested in
    /// </summary>
    [DebuggerDisplay("{DisplayName}")]
    public abstract class NativeSymbol
    {
        // TODO: mutable data, destroy
        protected static List<NativeSymbol> EmptySymbolList = new List<NativeSymbol>();

        private string _name;

        /// <summary>
        /// Name of the C++ type
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Category of the NativeType
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract NativeSymbolCategory Category { get; }

        /// <summary>
        /// The kind of the type.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract NativeSymbolKind Kind { get; }

        /// <summary>
        /// Gets the full name of the type
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string DisplayName
        {
            get { return Name; }
        }

        /// <summary>
        /// Whether one of it's immediate children is not resolved
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool IsImmediateResolved
        {
            get { return true; }
        }

        protected NativeSymbol()
        {
        }

        protected NativeSymbol(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets the immediate children of this symbol
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual IEnumerable<NativeSymbol> GetChildren()
        {
            return EmptySymbolList;
        }

        public virtual void ReplaceChild(NativeSymbol oldChild, NativeSymbol newChild)
        {
            throw new NotImplementedException();
        }

        protected void ReplaceChildInList<T>(NativeSymbol oldChild, NativeSymbol newChild, List<T> list) where T : NativeSymbol
        {
            ThrowIfNull(list);

            if (oldChild == null)
            {
                throw new ArgumentNullException("oldChild");
            }
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }

            T oldTyped = oldChild as T;
            T newTyped = newChild as T;
            if (oldTyped == null || newTyped == null)
            {
                throw new InvalidOperationException("Operands are of the wrong type");
            }

            int index = list.IndexOf(oldTyped);
            if (index < 0)
            {
                throw new InvalidOperationException("Old operand not a current child");
            }

            list.RemoveAt(index);
            list.Insert(index, newTyped);
        }

        protected IEnumerable<NativeSymbol> GetSingleChild<T>(T child) where T : NativeSymbol
        {
            if (child == null)
            {
                return new List<NativeSymbol>();
            }

            List<NativeSymbol> list = new List<NativeSymbol>();
            list.Add(child);
            return list;
        }

        protected IEnumerable<NativeSymbol> GetListChild<T>(List<T> list) where T : NativeSymbol
        {
            List<NativeSymbol> symList = new List<NativeSymbol>();
            foreach (T value in list)
            {
                symList.Add(value);
            }

            return symList;
        }


        protected void ReplaceChildSingle<T>(NativeSymbol oldchild, NativeSymbol newChild, ref T realChild) where T : NativeSymbol
        {
            if (!object.ReferenceEquals(oldchild, realChild))
            {
                throw new InvalidOperationException("Old child is wrong");
            }

            if (newChild == null)
            {
                realChild = null;
                return;
            }

            T newTyped = newChild as T;
            if (newTyped == null)
            {
                throw new InvalidOperationException("Operands are of the wrong type");
            }

            realChild = newTyped;
        }

    }
}
