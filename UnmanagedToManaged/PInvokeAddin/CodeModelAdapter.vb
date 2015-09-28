' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic
Imports EnvDTE
Imports EnvDTE80

Public Module CodeModelAdapterFactory
    Public Function Create(ByVal pi As ProjectItem, ByVal lang As PInvoke.Transform.LanguageType) As CodeModelAdapter
        Select Case lang
            Case PInvoke.Transform.LanguageType.VisualBasic
                Return New VBCodeModelAdapter(pi)
            Case PInvoke.Transform.LanguageType.CSharp
                Return New CSharpCodeModelAdapter(pi)
        End Select

        Throw New Exception("Could not create the adapter")
    End Function
End Module

Public MustInherit Class CodeModelAdapter

    Public DTE As DTE
    Public ProjectItem As ProjectItem

    Protected Sub New(ByVal pi As ProjectItem)
        Me.ProjectItem = pi
        Me.DTE = pi.DTE
    End Sub

    Public Function GetTextDocument() As TextDocument

        ' See if it's already open properly
        If ProjectItem.Document IsNot Nothing Then
            Dim td As TextDocument = TryCast(ProjectItem.Document.Object, TextDocument)
            If td Is Nothing Then
                ProjectItem.Document.Close(vsSaveChanges.vsSaveChangesYes)
            Else
                Return td
            End If
        End If

        If Not ProjectItem.IsOpen Then
            ProjectItem.Open(Constants.vsViewKindCode)
        End If

        Return DirectCast(ProjectItem.Document.Object, TextDocument)
    End Function

    ''' <summary>
    ''' Find a class.  Returns Nothing if a class could not be found
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindClassByName(ByVal name As String) As CodeClass2
        Try
            Return FindClassByNameImpl(name)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Try and find a structure by name
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindStructByName(ByVal name As String) As CodeStruct2
        Try
            Return FindStructByNameImpl(name)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Protected Overridable Function FindClassByNameImpl(ByVal name As String) As CodeClass2
        For Each ce As CodeElement In FindAllInFile()
            Dim cc As CodeClass2 = TryCast(ce, CodeClass2)
            If cc IsNot Nothing Then
                If 0 = String.CompareOrdinal(name, cc.Name) Then
                    Return cc
                End If
            End If
        Next

        Return Nothing
    End Function

    Protected Overridable Function FindStructByNameImpl(ByVal name As String) As CodeStruct2
        For Each ce As CodeElement In FindAllInFile()
            Dim cs As CodeStruct2 = TryCast(ce, CodeStruct2)
            If cs IsNot Nothing Then
                If 0 = String.CompareOrdinal(name, cs.Name) Then
                    Return cs
                End If
            End If
        Next

        Return Nothing
    End Function

    Protected Overridable Function FindAllInFile() As List(Of CodeElement)
        Dim list As New List(Of CodeElement)
        Dim fc As FileCodeModel2 = GetFileCodeModel()
        For Each ce As CodeElement In fc.CodeElements
            list.Add(ce)
        Next
        Return list
    End Function

    Protected Function GetFileCodeModel() As FileCodeModel2
        Return DirectCast(Me.ProjectItem.FileCodeModel, FileCodeModel2)
    End Function

End Class

#Region "VB CodeModel Adapter"

Public Class VBCodeModelAdapter
    Inherits CodeModelAdapter

    Public Sub New(ByVal pi As ProjectItem)
        MyBase.New(pi)
    End Sub

End Class

#End Region

#Region "CSharp CodeModel Adapter"

Public Class CSharpCodeModelAdapter
    Inherits CodeModelAdapter

    Public Sub New(ByVal pi As ProjectItem)
        MyBase.New(pi)
    End Sub
End Class

#End Region
