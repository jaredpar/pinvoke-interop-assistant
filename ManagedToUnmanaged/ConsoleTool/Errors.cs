/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;

using SignatureGenerator;

namespace ConsoleTool
{
    internal static class ConsoleErrors
    {
        public static readonly ErrorDesc ERROR_AssemblyFileNotSpecified = new ErrorDesc(100, Severity.Error, Resources.ERROR_AssemblyFileNotSpecified);
        public static readonly ErrorDesc ERROR_MethodIsNotInterop       = new ErrorDesc(101, Severity.Error, Resources.ERROR_MethodIsNotInterop); 
        public static readonly ErrorDesc ERROR_UnableToFindMethod       = new ErrorDesc(102, Severity.Error, Resources.ERROR_UnableToFindMethod);
        public static readonly ErrorDesc ERROR_UnableToLoadAssembly     = new ErrorDesc(103, Severity.Error, Resources.ERROR_UnableToLoadAssembly);
        public static readonly ErrorDesc ERROR_UnableToLoadType         = new ErrorDesc(104, Severity.Error, Resources.ERROR_UnableToLoadType);
        public static readonly ErrorDesc ERROR_UnrecognizedOption       = new ErrorDesc(105, Severity.Error, Resources.ERROR_UnrecognizedOption);
    }
}
