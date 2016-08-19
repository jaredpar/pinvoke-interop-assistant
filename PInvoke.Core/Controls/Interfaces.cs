// Copyright (c) Microsoft Corporation.  All rights reserved.
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace PInvoke.Controls
{

    /// <summary>
    /// Common interface for the controls that convert native code to managed code
    /// </summary>
    /// <remarks></remarks>
    public interface ISignatureImportControl
    {
        Transform.LanguageType LanguageType { get; set; }

        NativeStorage NativeStorage { get; set; }

        Transform.TransformKindFlags TransformKindFlags { get; set; }

        string ManagedCode { get; }

        event EventHandler LanguageTypeChanged;
    }
}