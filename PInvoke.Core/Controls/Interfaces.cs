
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace Controls
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

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
