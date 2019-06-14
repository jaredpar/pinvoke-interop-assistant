// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Collections.Generic;
using System.Text;

namespace PInvoke
{
    /// <summary>
    /// Provides an encapsulation for error messages and warnings
    /// </summary>
    /// <remarks></remarks>
    public class ErrorProvider
    {
        /// <summary>
        /// Errors
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Warnings
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// All messages 
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<string> AllMessages
        {
            get
            {
                var list = new List<string>();
                list.AddRange(Warnings);
                list.AddRange(Errors);
                return list;
            }
        }

        public void AddWarning(string str)
        {
            Warnings.Add(str);
        }

        public void AddWarning(string str, params object[] args)
        {
            string msg = string.Format(str, args);
            Warnings.Add(msg);
        }

        public void AddError(string str)
        {
            Errors.Add(str);
        }

        public void AddError(string str, params object[] args)
        {
            string msg = string.Format(str, args);
            AddError(msg);
        }

        /// <summary>
        /// Append the data in the passed in ErrorProvider into this instance
        /// </summary>
        /// <param name="ep"></param>
        /// <remarks></remarks>
        public void Append(ErrorProvider ep)
        {
            Errors.AddRange(ep.Errors);
            Warnings.AddRange(ep.Warnings);
        }


        public ErrorProvider()
        {
        }

        public ErrorProvider(ErrorProvider ep)
        {
            Append(ep);
        }

        public string CreateDisplayString()
        {
            var builder = new StringBuilder();
            foreach (string msg in Errors)
            {
                builder.AppendFormat("Error: {0}", msg);
                builder.AppendLine();
            }

            foreach (string msg in Warnings)
            {
                builder.AppendFormat("Warning: {0}", msg);
                builder.AppendLine();
            }

            return builder.ToString();
        }

    }
}