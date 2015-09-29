' Copyright (c) Microsoft Corporation.  All rights reserved.
Namespace Controls

    ''' <summary>
    ''' Common interface for the controls that convert native code to managed code
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface ISignatureImportControl

        Property LanguageType() As Transform.LanguageType

        Property NativeStorage() As NativeStorage

        Property TransformKindFlags() As Transform.TransformKindFlags

        ReadOnly Property ManagedCode() As String

        Event LanguageTypeChanged As EventHandler

    End Interface

End Namespace
