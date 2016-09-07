// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using PInvoke;

namespace PInvoke.Controls
{
    public delegate bool Filter(NativeName arg);

    public class Result
    {
        public List<NativeName> IncrementalFound;
        public List<NativeName> AllFound;
        public bool Completed;
    }

    /// <summary>
    /// Provides a way of doing incremental searches 
    /// </summary>
    /// <remarks></remarks>
    public class IncrementalSearch
    {
        private TimeSpan _delayTime = TimeSpan.FromSeconds(0.2);
        private IEnumerator<NativeName> _enumerator;
        private List<NativeName> _found = new List<NativeName>();

        private Filter _filter;

        /// <summary>
        /// Whether or not the search is completed
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsCompleted
        {
            get { return _enumerator == null; }
        }

        public TimeSpan DelayTime
        {
            get { return _delayTime; }
            set { _delayTime = value; }
        }

        public IncrementalSearch(IEnumerable<NativeName> enumerable, Filter cb)
        {
            _enumerator = enumerable.GetEnumerator();
            _filter = cb;
        }

        public void Cancel()
        {
            if (_enumerator != null)
            {
                _enumerator = null;
            }
        }

        public Result Search()
        {
            if (IsCompleted)
            {
                Result res2 = new Result();
                res2.Completed = true;
                res2.AllFound = _found;
                res2.IncrementalFound = new List<NativeName>();
                return res2;
            }

            DateTime start = DateTime.Now;
            var list = new List<NativeName>();
            bool completed = false;
            do
            {
                if (!_enumerator.MoveNext())
                {
                    _enumerator = null;
                    completed = true;
                    break;
                }

                var cur = _enumerator.Current;
                if (_filter(cur))
                {
                    list.Add(cur);
                }

                if ((DateTime.Now - start) > _delayTime)
                {
                    break;
                }
            } while (true);

            _found.AddRange(list);

            Result res = new Result();
            res.Completed = completed;
            res.AllFound = _found;
            res.IncrementalFound = list;
            return res;
        }
    }

}
