// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using static PInvoke.Contract;

namespace PInvoke.Transform
{

    public enum SalEntryListType
    {
        Pre,
        Item,
        Post
    }

    public class SalEntry
    {
        public SalEntryType Type;

        public string Text;
        public SalEntry(SalEntryType type)
        {
            this.Type = type;
            this.Text = string.Empty;
        }

        public SalEntry(NativeSalEntry other)
        {
            this.Type = other.SalEntryType;
            this.Text = other.Text.Trim(' ');
        }
    }

    /// <summary>
    /// Set of SAL annotation entries
    /// </summary>
    /// <remarks></remarks>
    public class SalEntrySet
    {
        private SalEntryListType _type;

        private List<SalEntry> _list = new List<SalEntry>();
        public SalEntryListType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public List<SalEntry> List
        {
            get { return _list; }
        }

        public SalEntrySet(SalEntryListType type)
        {
            Type = type;
        }

        public SalEntry FindEntry(SalEntryType type)
        {
            foreach (SalEntry entry in List)
            {
                if (entry.Type == type)
                {
                    return entry;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Used to analyze SAL attributes
    /// </summary>
    /// <remarks></remarks>
    public class SalAnalyzer
    {
        private NativeSalAttribute _sal;
        private List<SalEntrySet> _preList = new List<SalEntrySet>();
        private List<SalEntrySet> _itemList = new List<SalEntrySet>();

        private List<SalEntrySet> _postList = new List<SalEntrySet>();
        public SalAnalyzer(NativeSalAttribute sal)
        {
            _sal = sal;
            BuildLists();
        }

        public bool IsEmpty
        {
            get { return _preList.Count == 0 && _postList.Count == 0; }
        }

        #region "Loose Directional Mappings"

        public bool IsValidOut()
        {
            return FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) != null || FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly, SalEntryType.ExceptThat, SalEntryType.MaybeNull) != null;
        }

        public bool IsValidOutOnly()
        {
            return IsValidOut() && !IsValidIn();
        }

        public bool IsValidIn()
        {
            return FindPre(SalEntryType.Valid) != null;
        }

        public bool IsValidInOnly()
        {
            return IsValidIn() && !IsValidOut();
        }

        public bool IsValidInOut()
        {
            return IsValidIn() && IsValidOut();
        }

        #endregion

        #region "Strict SAL mappings"

        /// <summary>
        /// Is this a single in pointer
        /// 
        /// __in
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsIn()
        {
            if (_preList.Count != 2 || _itemList.Count != 0 || _postList.Count != 0)
            {
                return false;
            }

            if (FindPre(SalEntryType.Valid) == null || FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Is this a single in pointer
        /// 
        /// __in_opt
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsInOptional()
        {
            if (_preList.Count != 2 || _itemList.Count != 0 || _postList.Count != 0)
            {
                return false;
            }

            if (FindPre(SalEntryType.Valid) == null || FindPre(SalEntryType.Deref, SalEntryType.ReadOnly, SalEntryType.ExceptThat, SalEntryType.MaybeNull) == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Is this a single out pointer
        /// 
        /// __out
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOut()
        {
            string size = null;
            if (!IsOutElementBuffer(out size) | 0 != string.CompareOrdinal("1", size))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Is this a single in/out pointer
        /// 
        /// __inout 
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsInOut()
        {

            if (_preList.Count != 1 || _itemList.Count != 0 || _postList.Count != 1)
            {
                return false;
            }


            if (FindPre(SalEntryType.Valid) == null || FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Is this a in element buffer
        /// 
        /// __in_ecount(size)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsInElementBuffer(out string sizeArg)
        {
            sizeArg = null;

            if (_preList.Count != 3 || _itemList.Count != 0 || _postList.Count != 0)
            {
                return false;
            }

            SalEntrySet bufSet = FindPre(SalEntryType.ElemReadableTo);
            if (FindPre(SalEntryType.Valid) == null || FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) == null || bufSet == null)
            {
                return false;
            }

            sizeArg = bufSet.FindEntry(SalEntryType.ElemReadableTo).Text;
            return true;
        }

        /// <summary>
        /// Is this an optional in element buffer
        /// 
        /// __in_ecount_opt(size)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsInElementBufferOptional(out string sizeArg)
        {
            sizeArg = null;

            if (_preList.Count != 3 || _itemList.Count != 0 || _postList.Count != 0)
            {
                return false;
            }

            SalEntrySet bufSet = FindPre(SalEntryType.ElemReadableTo, SalEntryType.ExceptThat, SalEntryType.MaybeNull);
            if (FindPre(SalEntryType.Valid) == null || FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) == null || bufSet == null)
            {
                return false;
            }

            sizeArg = bufSet.FindEntry(SalEntryType.ElemReadableTo).Text;
            return true;
        }

        /// <summary>
        /// Is this a in byte buffer
        /// 
        /// __in_bcount(size)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsInByteBuffer(out string sizeArg)
        {
            sizeArg = null;

            if (_preList.Count != 3 || _itemList.Count != 0 || _postList.Count != 0)
            {
                return false;
            }

            SalEntrySet bufSet = FindPre(SalEntryType.ByteReadableTo);
            if (FindPre(SalEntryType.Valid) == null || FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) == null || bufSet == null)
            {
                return false;
            }

            sizeArg = bufSet.FindEntry(SalEntryType.ByteReadableTo).Text;
            return true;
        }

        /// <summary>
        /// Is this an optional in byte buffer
        /// 
        /// __in_bcount_opt(size)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsInByteBufferOptional(out string sizeArg)
        {
            sizeArg = null;

            if (_preList.Count != 3 || _itemList.Count != 0 || _postList.Count != 0)
            {
                return false;
            }

            SalEntrySet bufSet = FindPre(SalEntryType.ByteReadableTo, SalEntryType.ExceptThat, SalEntryType.MaybeNull);
            if (FindPre(SalEntryType.Valid) == null || FindPre(SalEntryType.Deref, SalEntryType.ReadOnly) == null || bufSet == null)
            {
                return false;
            }

            sizeArg = bufSet.FindEntry(SalEntryType.ByteReadableTo).Text;
            return true;
        }

        public bool IsOutElementBuffer()
        {
            string sizeArg = null;
            return IsOutElementBuffer(out sizeArg);
        }

        /// <summary>
        /// Is this an out parameter that is a buffer of elements
        /// 
        /// __out_ecount(sizeArg)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOutElementBuffer(out string sizeArg)
        {
                sizeArg = null;
            if (_preList.Count != 0 || _itemList.Count != 1 || _postList.Count != 1)
            {
                return false;
            }

            SalEntrySet bufSet = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo);
            if (FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) == null || bufSet == null)
            {
                return false;
            }

            sizeArg = bufSet.FindEntry(SalEntryType.ElemWritableTo).Text;
            return true;
        }


        public bool IsOutElementBufferOptional()
        {
            string sizeArg = null;
            return IsOutElementBufferOptional(out sizeArg);
        }

        /// <summary>
        /// Is this an out parameter that is a buffer of elements
        /// 
        /// __out_ecount_opt(sizeArg)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOutElementBufferOptional(out string sizeArg)
        {
            sizeArg = null;

            if (_preList.Count != 0 || _itemList.Count != 1 || _postList.Count != 1)
            {
                return false;
            }

            SalEntrySet bufSet = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo);
            if (FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly, SalEntryType.ExceptThat, SalEntryType.MaybeNull) == null || bufSet == null)
            {
                return false;
            }

            sizeArg = bufSet.FindEntry(SalEntryType.ElemWritableTo).Text;
            return true;
        }

        public bool IsOutPartElementBuffer()
        {
            string sizeArg = null;
            string readableArg = null;

            return IsOutPartElementBuffer(out sizeArg, out readableArg);
        }

        /// <summary>
        /// Is this a partially readable buffer
        /// 
        /// __out_ecount_part(sizeArg, readableArg)
        /// </summary>
        /// <param name="writableSize"></param>
        /// <param name="readableSize"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOutPartElementBuffer(out string writableSize, out string readableSize)
        {
            writableSize = null;
            readableSize = null;

            if (_preList.Count != 0 || _itemList.Count != 1 || _postList.Count != 2)
            {
                return false;
            }

            SalEntrySet sizeBuf = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo);
            SalEntrySet readBuf = FindPost(SalEntryType.ElemReadableTo);
            if (FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) == null || sizeBuf == null || readBuf == null)
            {
                return false;
            }

            writableSize = sizeBuf.FindEntry(SalEntryType.ElemWritableTo).Text;
            readableSize = readBuf.FindEntry(SalEntryType.ElemReadableTo).Text;
            return true;
        }

        public bool IsOutPartElementBufferOptional()
        {
            string notUsed1 = null;
            string notUsed2 = null;
            return IsOutPartElementBufferOptional(out notUsed1, out notUsed2);
        }

        /// <summary>
        /// Is this an optional partially readable buffer
        /// 
        /// __out_ecount_part_opt(sizeArg, readableArg)
        /// </summary>
        /// <param name="writableSize"></param>
        /// <param name="readableSize"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOutPartElementBufferOptional(out string writableSize, out string readableSize)
        {
            writableSize = null;
            readableSize = null;

            if (_preList.Count != 0 || _itemList.Count != 1 || _postList.Count != 2)
            {
                return false;
            }

            SalEntrySet sizeBuf = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo);
            SalEntrySet readBuf = FindPost(SalEntryType.ElemReadableTo, SalEntryType.ExceptThat, SalEntryType.MaybeNull);
            if (FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) == null || sizeBuf == null || readBuf == null)
            {
                return false;
            }

            writableSize = sizeBuf.FindEntry(SalEntryType.ElemWritableTo).Text;
            readableSize = readBuf.FindEntry(SalEntryType.ElemReadableTo).Text;
            return true;
        }

        /// <summary>
        /// Is this an out byte bufffer
        /// 
        /// __out_bcount(sizeArg)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOutByteBuffer(ref string sizeArg)
        {
            if (_preList.Count != 0 || _itemList.Count != 1 || _postList.Count != 1)
            {
                return false;
            }

            SalEntrySet sizeBuf = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo);
            if (FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) == null || sizeBuf == null)
            {
                return false;
            }

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text;
            return true;
        }

        /// <summary>
        /// Is this an optional out byte bufffer
        /// 
        /// __out_bcount_opt(sizeArg)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOutByteBufferOptional(ref string sizeArg)
        {
            if (_preList.Count != 0 || _itemList.Count != 1 || _postList.Count != 1)
            {
                return false;
            }

            SalEntrySet sizeBuf = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo);
            if (FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly, SalEntryType.ExceptThat, SalEntryType.MaybeNull) == null || sizeBuf == null)
            {
                return false;
            }

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text;
            return true;
        }

        /// <summary>
        /// Is this an out byte bufffer which is partiaally readable
        /// 
        /// __out_bcount_part(sizeArg, readableArg)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOutPartByteBuffer(out string sizeArg, out string readableArg)
        {
            sizeArg = null;
            readableArg = null;

            if (_preList.Count != 0 || _itemList.Count != 1 || _postList.Count != 2)
            {
                return false;
            }

            SalEntrySet sizeBuf = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo);
            SalEntrySet readBuf = FindPost(SalEntryType.ByteReadableTo);
            if (FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) == null || sizeBuf == null || readBuf == null)
            {
                return false;
            }

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text;
            readableArg = readBuf.FindEntry(SalEntryType.ByteReadableTo).Text;
            return true;
        }

        /// <summary>
        /// Is this an out byte bufffer which is partiaally readable
        /// 
        /// __out_bcount_part_opt(sizeArg, readableArg)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsOutPartByteBufferOptional(ref string sizeArg, ref string readableArg)
        {
            if (_preList.Count != 0 || _itemList.Count != 1 || _postList.Count != 2)
            {
                return false;
            }

            SalEntrySet sizeBuf = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo);
            SalEntrySet readBuf = FindPost(SalEntryType.ByteReadableTo, SalEntryType.ExceptThat, SalEntryType.MaybeNull);
            if (FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) == null || sizeBuf == null || readBuf == null)
            {
                return false;
            }

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text;
            readableArg = readBuf.FindEntry(SalEntryType.ByteReadableTo).Text;
            return true;
        }


        public bool IsInOutElementBuffer()
        {
            string sizeArg = null;
            return IsInOutElementBuffer(ref sizeArg);
        }

        /// <summary>
        /// Is this an in/out element buffer
        /// 
        /// __inout_ecount(size)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsInOutElementBuffer(ref string sizeArg)
        {

            if (_preList.Count != 1 || _itemList.Count != 1 || _postList.Count != 1)
            {
                return false;
            }

            SalEntrySet sizeBuf = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo);

            if (FindPre(SalEntryType.Valid) == null || FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) == null || sizeBuf == null)
            {
                return false;
            }

            sizeArg = sizeBuf.FindEntry(SalEntryType.ElemWritableTo).Text;
            return true;
        }

        /// <summary>
        /// Is this an in/out byte buffer
        /// 
        /// __inout_bcount(size)
        /// </summary>
        /// <param name="sizeArg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsInOutByteBuffer(ref string sizeArg)
        {

            if (_preList.Count != 1 || _itemList.Count != 1 || _postList.Count != 1)
            {
                return false;
            }

            SalEntrySet sizeBuf = FindItem(SalEntryType.NotNull, SalEntryType.ByteWritableTo);

            if (FindPre(SalEntryType.Valid) == null || FindPost(SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly) == null || sizeBuf == null)
            {
                return false;
            }

            sizeArg = sizeBuf.FindEntry(SalEntryType.ByteWritableTo).Text;
            return true;

        }

        /// <summary>
        /// Is this a __deref_out single element pointer
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsDerefOut()
        {
            if (_preList.Count != 0 || _itemList.Count != 1 || _postList.Count != 4)
            {
                return false;
            }

            // Get the item part
            SalEntrySet found = FindItem(SalEntryType.NotNull, SalEntryType.ElemWritableTo);
            if (found == null || !StringComparer.Ordinal.Equals("1", found.FindEntry(SalEntryType.ElemWritableTo).Text))
            {
                return false;
            }

            // Validate first level 
            found = FindPost(SalEntryType.ElemReadableTo);
            if (found == null || !StringComparer.Ordinal.Equals("1", found.FindEntry(SalEntryType.ElemReadableTo).Text))
            {
                return false;
            }

            // Able to dereference the element
            if (FindPost(SalEntryType.Deref, SalEntryType.NotNull) == null)
            {
                return false;
            }

            found = FindPost(SalEntryType.Deref, SalEntryType.ElemWritableTo);
            if (found == null || !StringComparer.Ordinal.Equals("1", found.FindEntry(SalEntryType.ElemWritableTo).Text))
            {
                return false;
            }

            found = FindPost(SalEntryType.Deref, SalEntryType.Valid, SalEntryType.Deref, SalEntryType.NotReadOnly);
            if (found == null)
            {
                return false;
            }

            return true;
        }

        #endregion

        public SalEntrySet Find(SalEntryListType type, params SalEntryType[] args)
        {
            switch (type)
            {
                case SalEntryListType.Item:
                    return FindItem(args);
                case SalEntryListType.Pre:
                    return FindPre(args);
                case SalEntryListType.Post:
                    return FindPost(args);
                default:
                    ThrowInvalidEnumValue(type);
                    return null;
            }
        }

        public SalEntrySet FindPost(params SalEntryType[] args)
        {
            return FindSet(_postList, args);
        }

        public SalEntrySet FindPre(params SalEntryType[] args)
        {
            return FindSet(_preList, args);
        }

        public SalEntrySet FindItem(params SalEntryType[] args)
        {
            return FindSet(_itemList, args);
        }

        /// <summary>
        /// Try and find a SalEntrySet with the specified entries
        /// </summary>
        /// <param name="list"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private SalEntrySet FindSet(List<SalEntrySet> list, SalEntryType[] args)
        {
            foreach (SalEntrySet item in list)
            {
                if (item.List.Count == args.Length)
                {
                    bool match = true;
                    for (Int32 i = 0; i <= args.Length - 1; i++)
                    {
                        if (item.List[i].Type != args[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private void BuildLists()
        {
            ThrowIfNull(_sal);
            if (_sal.IsEmpty)
            {
                return;
            }

            List<NativeSalEntry> list = new List<NativeSalEntry>(_sal.SalEntryList);
            List<SalEntrySet> dest = new List<SalEntrySet>();
            SalEntrySet cur = null;
            if (list[0].SalEntryType == SalEntryType.Post)
            {
                cur = new SalEntrySet(SalEntryListType.Post);
                list.RemoveAt(0);
            }
            else if (list[0].SalEntryType == SalEntryType.Pre)
            {
                cur = new SalEntrySet(SalEntryListType.Pre);
                list.RemoveAt(0);
            }
            else
            {
                cur = new SalEntrySet(SalEntryListType.Item);
            }

            for (Int32 i = 0; i <= list.Count - 1; i++)
            {
                NativeSalEntry entry = list[i];
                if (entry.SalEntryType == SalEntryType.Pre)
                {
                    dest.Add(cur);
                    cur = new SalEntrySet(SalEntryListType.Pre);
                }
                else if (entry.SalEntryType == SalEntryType.Post)
                {
                    dest.Add(cur);
                    cur = new SalEntrySet(SalEntryListType.Post);
                }
                else
                {
                    cur.List.Add(new SalEntry(entry));
                }
            }
            dest.Add(cur);

            foreach (SalEntrySet l in dest)
            {
                if (l.List.Count == 0)
                {
                    continue;
                }

                switch (l.Type)
                {
                    case SalEntryListType.Post:
                        _postList.Add(l);
                        break;
                    case SalEntryListType.Pre:
                        _preList.Add(l);
                        break;
                    case SalEntryListType.Item:
                        _itemList.Add(l);
                        break;
                    default:
                        ThrowInvalidEnumValue(l.Type);
                        break;
                }
            }

        }

    }

}
