// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;

namespace PInvoke
{
    public static class EnumUtil
    {

        public static List<T> GetAllValues<T>()
        {
            var list = new List<T>();
            foreach (T cur in Enum.GetValues(typeof(T)))
            {
                list.Add(cur);
            }

            return list;
        }

        public static object[] GetAllValuesObject<T>()
        {
            var list = new List<object>();
            foreach (T cur in Enum.GetValues(typeof(T)))
            {
                list.Add(cur);
            }

            return list.ToArray();
        }

        public static object[] GetAllValuesObjectExcept<T>(T except)
        {
            var comp = EqualityComparer<T>.Default;
            var list = new List<object>();
            foreach (T cur in Enum.GetValues(typeof(T)))
            {
                if (!comp.Equals(cur, except))
                {
                    list.Add(cur);
                }
            }

            return list.ToArray();
        }

    }
}