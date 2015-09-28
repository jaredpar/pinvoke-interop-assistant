' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports Microsoft.Win32
Imports PInvoke.Transform

''' <summary>
''' Options for the AddIn
''' </summary>
''' <remarks></remarks>
Public Class Options

    Private Const KeyPath As String = "Software\Microsoft\PInvokeAddin"
    Private Const FileNameOptName As String = "FileName"
    Private Const LanguageOptName As String = "Language"

    Public FileName As String
    Public Language As LanguageType

    Public Sub New()
        LoadDefaults()
    End Sub

    Private Sub LoadDefaults()
        FileName = "NativeMethods"
        Language = LanguageType.VisualBasic
    End Sub

    Public Sub Load()
        Try
            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey(KeyPath)
                Me.FileName = DirectCast(key.GetValue(FileNameOptName), String)
                Me.Language = DirectCast( _
                    System.Enum.Parse( _
                        GetType(LanguageType), _
                        DirectCast(key.GetValue(LanguageOptName), String)), _
                    LanguageType)
            End Using

            If String.IsNullOrEmpty(FileName) Then
                LoadDefaults()
            End If
        Catch ex As Exception
            LoadDefaults()
        End Try
    End Sub

    Public Sub Save()
        Try
            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey(KeyPath)
                key.SetValue(FileNameOptName, Me.FileName)
                key.SetValue(LanguageOptName, Me.Language.ToString())
            End Using
        Catch ex As Exception

        End Try
    End Sub

End Class
