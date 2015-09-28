' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports PInvoke.Transform
Imports EnvDTE
Imports EnvDTE80

''' <summary>
''' Responsible for inserting code into the active document
''' </summary>
''' <remarks></remarks>
Public Class CodeDomInsterter

    Private m_dte As DTE
    Private m_lang As LanguageType
    Private m_conv As BasicConverter

    Public Sub New(ByVal dte As DTE, ByVal lang As LanguageType)
        m_dte = dte
        m_lang = lang
        m_conv = New BasicConverter(lang)
    End Sub

    ''' <summary>
    ''' Insert the specified code type declaration into the code
    ''' </summary>
    ''' <param name="ctd"></param>
    ''' <remarks></remarks>
    Public Sub Insert(ByVal ctd As CodeTypeDeclaration)
        Dim adapter As CodeModelAdapter = GetAdapterForFile(GetDefaultFileName())
        If ctd.IsStruct Then

            ' If there is already a struct by this name then just return
            Dim cs As CodeStruct2 = adapter.FindStructByName(ctd.Name)
            If cs IsNot Nothing Then
                Return
            End If
        ElseIf ctd.IsClass _
            AndAlso 0 <> String.CompareOrdinal(ctd.Name, PInvoke.Transform.TransformConstants.NativeMethodsName) _
            AndAlso 0 <> String.CompareOrdinal(ctd.Name, PInvoke.Transform.TransformConstants.NativeConstantsName) Then

            ' If this is not the NativeMethods class then don't insert a class twice
            Dim cc As CodeClass2 = adapter.FindClassByName(ctd.Name)
            If cc IsNot Nothing Then
                Return
            End If
        End If

        Dim doc As TextDocument = adapter.GetTextDocument()
        Dim ep As EditPoint = doc.CreateEditPoint()
        ep.EndOfDocument()
        ep.Insert(m_conv.ConvertCodeDomToPInvokeCode(ctd))
    End Sub

    Private Function GetDefaultFileName() As String
        Dim opts As New Options()
        opts.Load()
        Select Case m_lang
            Case LanguageType.VisualBasic
                Return opts.FileName & ".vb"
            Case LanguageType.CSharp
                Return opts.FileName & ".cs"
        End Select

        Return String.Empty
    End Function

    Private Function GetAdapterForFile(ByVal name As String) As CodeModelAdapter
        Dim project As Project = m_dte.ActiveDocument.ProjectItem.ContainingProject
        Dim pi As ProjectItem = Nothing
        Try
            pi = project.ProjectItems.Item(name)
        Catch ex As Exception

        End Try

        If pi Is Nothing Then
            Dim path As String = Nothing
            Dim sol2 As Solution2 = DirectCast(m_dte.Solution, Solution2)
            Select Case m_lang
                Case LanguageType.VisualBasic
                    path = sol2.GetProjectItemTemplate("CodeFile.zip", "VisualBasic")
                Case LanguageType.CSharp
                    path = sol2.GetProjectItemTemplate("CodeFile.zip", "CSharp")
            End Select

            ' Adding doesn't actually return the project item, instead you have to 
            ' query for it after you've added it
            project.ProjectItems.AddFromTemplate(path, name)
            pi = project.ProjectItems.Item(name)
        End If

        If pi Is Nothing Then
            Throw New Exception("Could not get the project file")
        End If

        Return CodeModelAdapterFactory.Create(pi, m_lang)
    End Function

End Class
