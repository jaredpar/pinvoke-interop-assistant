' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System
Imports System.Diagnostics
Imports System.Runtime.Serialization

Friend Module Contract
    Public InUnitTest As Boolean

    Public Sub ThrowIfNull(ByVal value As Object)
        Contract.ThrowIfNull(value, "Value should not be null")
    End Sub

    Public Sub ThrowIfNull(ByVal value As Object, ByVal message As String)
        If (value Is Nothing) Then
            Contract.ContractViolation(message)
        End If
    End Sub

    Public Sub ThrowIfFalse(ByVal value As Boolean)
        Contract.ThrowIfFalse(value, "Unexpected false")
    End Sub

    Public Sub ThrowIfFalse(ByVal value As Boolean, ByVal message As String)
        If Not value Then
            Contract.ContractViolation(message)
        End If
    End Sub

    Public Sub ThrowIfTrue(ByVal value As Boolean)
        Contract.ThrowIfTrue(value, "Unexpected true")
    End Sub

    Public Sub ThrowIfTrue(ByVal value As Boolean, ByVal message As String)
        If value Then
            Contract.ContractViolation(message)
        End If
    End Sub

    Public Sub InvalidEnumValue(Of T As Structure)(ByVal value As T)
        Contract.ThrowIfFalse(GetType(T).IsEnum, "Expected an enum type")
        Contract.Violation("Invalid Enum value of Type {0} : {1}", New Object() {GetType(T).Name, value})
    End Sub

    Public Sub Violation(ByVal message As String)
        Contract.ContractViolation(message)
    End Sub

    Public Sub Violation(ByVal format As String, ByVal ParamArray args As Object())
        Contract.ContractViolation(String.Format(format, args))
    End Sub

    Private Sub ContractViolation(ByVal message As String)
        Debug.Fail("Contract Violation: " & message)
        Dim inUnitTest As Boolean = Contract.InUnitTest
        Dim trace As New StackTrace
        Dim text As String = message
        text = text & Environment.NewLine & trace.ToString
        Throw New ContractException(message)
    End Sub
End Module

<Serializable()> _
Friend Class ContractException
    Inherits Exception
    ' Methods
    Public Sub New()
        Me.New("Contract Violation")
    End Sub

    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub

    Public Sub New(ByVal message As String, ByVal inner As Exception)
        MyBase.New(message, inner)
    End Sub

End Class

