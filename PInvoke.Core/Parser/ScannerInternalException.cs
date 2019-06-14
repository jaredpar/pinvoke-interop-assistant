// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace PInvoke.Parser
{
    public class ScannerInternalException : Exception
    {

        public ScannerInternalException(string msg) : base(msg)
        {
        }

        public ScannerInternalException(string msg, Exception inner) : base(msg, inner)
        {
        }
    }

}
