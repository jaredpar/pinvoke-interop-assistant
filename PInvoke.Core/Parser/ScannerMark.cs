// Copyright (c) Microsoft Corporation.  All rights reserved.


namespace PInvoke.Parser
{
    /// <summary>
    /// Used to mark a point in the scanner to which a caller can move back to
    /// </summary>
    /// <remarks></remarks>
    public class ScannerMark
    {
        public int Index { get; internal set; }

        public int LineNumber { get; internal set; }
    }

}
