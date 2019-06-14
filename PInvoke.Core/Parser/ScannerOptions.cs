// Copyright (c) Microsoft Corporation.  All rights reserved.


namespace PInvoke.Parser
{
    /// <summary>
    /// Options for the Scanner
    /// </summary>
    /// <remarks></remarks>
    public class ScannerOptions
    {
        public bool HideComments { get; set; } = false;
        public bool HideWhitespace { get; set; } = false;

        public bool HideNewLines { get; set; } = false;

        public bool ThrowOnEndOfStream { get; set; }

        public ScannerOptions()
        {
        }
    }

}
