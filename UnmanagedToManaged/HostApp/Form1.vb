' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports PInvoke
Imports PInvoke.Controls
Imports PInvoke.Transform
Imports Microsoft.Win32

Public Class Form1

    Private m_ns As NativeStorage
    Private m_conv As Transform.BasicConverter

    Sub New()

        NativeStorage.DefaultInstance = NativeStorage.LoadFromAssemblyPath()
        m_ns = NativeStorage.DefaultInstance

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        SymbolDisplayControl1.NativeStorage = m_ns
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)

        SymbolDisplayControl1.LanguageType = GetSavedLanguageType()
        AddHandler SymbolDisplayControl1.LanguageTypeChanged, AddressOf OnLanguageChanged
    End Sub

    Private Const RegistryRelativePath As String = Constants.RegistryRelativePath + "\HostApp"
    Private Const RegistryLanguageTypeName As String = "Language"

    Private Sub OnLanguageChanged(ByVal sender As Object, ByVal e As EventArgs)
        SaveLanguageType(SymbolDisplayControl1.LanguageType)
    End Sub

    Private Shared Function GetSavedLanguageType() As LanguageType
        Try
            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey(RegistryRelativePath, RegistryKeyPermissionCheck.ReadWriteSubTree)
                Dim strVal As String = key.GetValue(RegistryLanguageTypeName, LanguageType.VisualBasic.ToString())
                Return DirectCast(System.Enum.Parse(GetType(LanguageType), strVal), LanguageType)
            End Using
        Catch ex As Exception
            Return LanguageType.VisualBasic
        End Try
    End Function

    Private Shared Sub SaveLanguageType(ByVal type As LanguageType)
        Try
            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey(RegistryRelativePath, RegistryKeyPermissionCheck.ReadWriteSubTree)
                key.SetValue(RegistryLanguageTypeName, type.ToString(), RegistryValueKind.String)
            End Using
        Catch ex As Exception
            Debug.Assert(False, "Error saving language key")
        End Try
    End Sub
End Class
