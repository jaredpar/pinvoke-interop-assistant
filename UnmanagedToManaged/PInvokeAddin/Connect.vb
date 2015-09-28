' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System
Imports Microsoft.VisualStudio.CommandBars
Imports Extensibility
Imports EnvDTE
Imports EnvDTE80
Imports PInvoke
Imports PInvoke.Controls
Imports Microsoft.Win32

Module CommandNames
    Public Const Prefix As String = "PInvokeAddin.Connect."

    Public Const SelectConstantName As String = "SelectConstant"
    Public Const SelectConstantFullName As String = Prefix + SelectConstantName

    Public Const SelectProcedureName As String = "SelectProcedure"
    Public Const SelectProcedureFullName As String = Prefix + SelectProcedureName

    Public Const SelectTypeName As String = "SelectType"
    Public Const SelectTypeFullName As String = Prefix + SelectTypeName

    Public Const ConvertNativeCodeName As String = "ConvertNativeCode"
    Public Const ConvertNativeCodeFullName As String = Prefix + ConvertNativeCodeName

    Public Const DotNetSignature As String = "DotNetSignature"
    Public Const DotNetSignatureFullName As String = Prefix + DotNetSignature
End Module

Enum VsLanguage
    CSharp
    VisualBasic
    CPlusPlus
    Unknown
End Enum

Public Class Connect

    Implements IDTExtensibility2
    Implements IDTCommandTarget

    Private m_dte As DTE
    Private m_dte2 As DTE2
    Private m_addinInstance As AddIn
    Private m_loaded As Boolean = False
    Private m_managedToNativeDialog As WindowsTool.MainForm

    '''<summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
    Public Sub New()

    End Sub

    '''<summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
    '''<param name='application'>Root object of the host application.</param>
    '''<param name='connectMode'>Describes how the Add-in is being loaded.</param>
    '''<param name='addInInst'>Object representing this Add-in.</param>
    '''<remarks></remarks>
    Public Sub OnConnection(ByVal application As Object, ByVal connectMode As ext_ConnectMode, ByVal addInInst As Object, ByRef custom As Array) Implements IDTExtensibility2.OnConnection
        m_dte2 = CType(application, DTE2)
        m_dte = CType(application, DTE)
        m_addinInstance = CType(addInInst, AddIn)
        If connectMode = ext_ConnectMode.ext_cm_UISetup Then

            Dim commands As Commands2 = CType(m_dte2.Commands, Commands2)
            Dim toolsMenuName As String
            Try

                'If you would like to move the command to a different menu, change the word "Tools" to the 
                '  English version of the menu. This code will take the culture, append on the name of the menu
                '  then add the command to that menu. You can find a list of all the top-level menus in the file
                '  CommandBar.resx.
                Dim resourceManager As System.Resources.ResourceManager = New System.Resources.ResourceManager("PInvokeAddin.CommandBar", System.Reflection.Assembly.GetExecutingAssembly())

                Dim cultureInfo As System.Globalization.CultureInfo = New System.Globalization.CultureInfo(m_dte2.LocaleID)
                toolsMenuName = resourceManager.GetString(String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools"))

            Catch e As Exception
                'We tried to find a localized version of the word Tools, but one was not found.
                '  Default to the en-US word, which may work for the current culture.
                toolsMenuName = "Tools"
            End Try

            'Place the command on the tools menu.
            'Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            Dim commandBars As CommandBars = CType(m_dte2.CommandBars, CommandBars)
            Dim menuBarCommandBar As CommandBar = commandBars.Item("MenuBar")
            Dim codeWindowCommandBar As CommandBar = commandBars.Item("Code Window")

            'Find the Tools command bar on the MenuBar command bar:
            Dim toolsControl As CommandBarControl = menuBarCommandBar.Controls.Item(toolsMenuName)
            Dim toolsPopup As CommandBarPopup = CType(toolsControl, CommandBarPopup)

            Try
                'Add a command to the Commands collection:
                Dim bar As CommandBar = DirectCast(commands.AddCommandBar("PInvoke", vsCommandBarType.vsCommandBarTypeMenu, codeWindowCommandBar, 1), CommandBar)

                ' Managed -> Native
                Dim dotNetSignature As Command = commands.AddNamedCommand2( _
                    m_addinInstance, _
                    CommandNames.DotNetSignature, _
                    ".Net Signature", _
                    "Import a DotNet Signature to your code", _
                    True)
                dotNetSignature.AddControl(bar, 1)

                ' Command for translating code snippets into code
                Dim snippetCommand As Command = commands.AddNamedCommand2( _
                    m_addinInstance, _
                    CommandNames.ConvertNativeCodeName, _
                    "Convert Native Code", _
                    "Translate a snippet of native code into PInvoke code", _
                    True)
                snippetCommand.AddControl(bar, 1)

                ' Command for generating constants in code
                Dim constCommand As Command = commands.AddNamedCommand2( _
                    m_addinInstance, _
                    CommandNames.SelectConstantName, _
                    "Constant", _
                    "Select a constant for generation in your code", _
                    True)
                constCommand.AddControl(bar, 1)

                Dim procCommand As Command = commands.AddNamedCommand2( _
                    m_addinInstance, _
                    CommandNames.SelectProcedureName, _
                    "Procedure", _
                    "Select a procedure for generation in your code", _
                    True)
                procCommand.AddControl(bar, 1)

                Dim typeCommand As Command = commands.AddNamedCommand2( _
                    m_addinInstance, _
                    CommandNames.SelectTypeName, _
                    "Type", _
                    "Select a type for generation in your code", _
                    True)
                typeCommand.AddControl(bar, 1)


                'Find the appropriate command bar on the MenuBar command bar:
            Catch argumentException As System.ArgumentException
                'If we are here, then the exception is probably because a command with that name
                '  already exists. If so there is no need to recreate the command and we can 
                '  safely ignore the exception.
            Catch ex As Exception
                Debug.Fail("Error occurred adding commands: " & ex.Message)
            End Try

        End If
    End Sub

    '''<summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
    '''<param name='disconnectMode'>Describes how the Add-in is being unloaded.</param>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnDisconnection(ByVal disconnectMode As ext_DisconnectMode, ByRef custom As Array) Implements IDTExtensibility2.OnDisconnection
        If m_managedToNativeDialog IsNot Nothing Then
            m_managedToNativeDialog.Dispose()
        End If
    End Sub

    '''<summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification that the collection of Add-ins has changed.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnAddInsUpdate(ByRef custom As Array) Implements IDTExtensibility2.OnAddInsUpdate
    End Sub

    '''<summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnStartupComplete(ByRef custom As Array) Implements IDTExtensibility2.OnStartupComplete
    End Sub

    '''<summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnBeginShutdown(ByRef custom As Array) Implements IDTExtensibility2.OnBeginShutdown
    End Sub

    '''<summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
    '''<param name='commandName'>The name of the command to determine state for.</param>
    '''<param name='neededText'>Text that is needed for the command.</param>
    '''<param name='status'>The state of the command in the user interface.</param>
    '''<param name='commandText'>Text requested by the neededText parameter.</param>
    '''<remarks></remarks>
    Public Sub QueryStatus(ByVal commandName As String, ByVal neededText As vsCommandStatusTextWanted, ByRef status As vsCommandStatus, ByRef commandText As Object) Implements IDTCommandTarget.QueryStatus
        If neededText = vsCommandStatusTextWanted.vsCommandStatusTextWantedNone Then

            ' Our commands are only supported on TextDocument instances
            Dim vsLang As VsLanguage = GetCurrentVsLanguage()
            Dim isTextDoc As Boolean = False
            If m_dte2.ActiveDocument IsNot Nothing _
                AndAlso TryCast(m_dte2.ActiveDocument.Object, TextDocument) IsNot Nothing Then
                isTextDoc = True
            End If

            If commandName = CommandNames.SelectConstantFullName _
                OrElse commandName = CommandNames.SelectProcedureFullName _
                OrElse commandName = CommandNames.SelectTypeFullName _
                OrElse commandName = CommandNames.ConvertNativeCodeFullName Then

                ' Native -> Managed conversion
                If vsLang = VsLanguage.CSharp OrElse vsLang = VsLanguage.VisualBasic Then
                    If isTextDoc Then
                        status = vsCommandStatus.vsCommandStatusEnabled Or vsCommandStatus.vsCommandStatusSupported
                    Else
                        status = vsCommandStatus.vsCommandStatusUnsupported
                    End If
                Else
                    status = vsCommandStatus.vsCommandStatusInvisible Or vsCommandStatus.vsCommandStatusUnsupported
                End If
            ElseIf commandName = CommandNames.DotNetSignatureFullName Then

                ' Managed -> Native conversion
                If vsLang = VsLanguage.CPlusPlus Then
                    If isTextDoc Then
                        status = vsCommandStatus.vsCommandStatusEnabled Or vsCommandStatus.vsCommandStatusSupported
                    Else
                        status = vsCommandStatus.vsCommandStatusUnsupported
                    End If
                Else
                    status = vsCommandStatus.vsCommandStatusInvisible Or vsCommandStatus.vsCommandStatusUnsupported
                End If
            End If
        End If
    End Sub

    '''<summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
    '''<param name='commandName'>The name of the command to execute.</param>
    '''<param name='executeOption'>Describes how the command should be run.</param>
    '''<param name='varIn'>Parameters passed from the caller to the command handler.</param>
    '''<param name='varOut'>Parameters passed from the command handler to the caller.</param>
    '''<param name='handled'>Informs the caller if the command was handled or not.</param>
    '''<remarks></remarks>
    Public Sub Exec(ByVal commandName As String, ByVal executeOption As vsCommandExecOption, ByRef varIn As Object, ByRef varOut As Object, ByRef handled As Boolean) Implements IDTCommandTarget.Exec
        handled = False
        If executeOption = vsCommandExecOption.vsCommandExecOptionDoDefault Then
            If commandName = "PInvokeAddin.Connect.PInvokeAddin" Then
                handled = True
                Exit Sub
            End If

            Try
                EnsureNativeStorage()

                If commandName = CommandNames.SelectConstantFullName Then
                    handled = True
                    Using diag As New SelectConstantDialog()
                        If System.Windows.Forms.DialogResult.OK = diag.ShowDialog() Then
                            Dim bag As New NativeSymbolBag()
                            For Each c As NativeConstant In diag.Selected
                                bag.AddConstant(c)
                            Next
                            DisplayPInvokeCode(bag)
                        End If
                    End Using
                    Exit Sub
                ElseIf commandName = CommandNames.SelectProcedureFullName Then
                    handled = True
                    Using diag As New SelectProcedureDialog()
                        If System.Windows.Forms.DialogResult.OK = diag.ShowDialog() Then
                            Dim bag As New NativeSymbolBag()
                            For Each p As NativeProcedure In diag.Selected
                                bag.AddProcedure(p)
                            Next
                            DisplayPInvokeCode(bag)
                        End If
                    End Using
                ElseIf commandName = CommandNames.SelectTypeFullName Then
                    handled = True
                    Using diag As New SelectTypeDialog()
                        If System.Windows.Forms.DialogResult.OK = diag.ShowDialog() Then
                            Dim bag As New NativeSymbolBag()
                            For Each t As NativeType In diag.Selected
                                If t.Kind = NativeSymbolKind.TypedefType Then
                                    bag.AddTypedef(DirectCast(t, NativeTypeDef))
                                Else
                                    bag.AddDefinedType(DirectCast(t, NativeDefinedType))
                                End If
                            Next
                            DisplayPInvokeCode(bag)
                        End If
                    End Using
                ElseIf commandName = CommandNames.ConvertNativeCodeFullName Then
                    handled = True
                    Using diag As New CodeDialog()
                        If System.Windows.Forms.DialogResult.OK = diag.ShowDialog() Then
                            Dim bc As New Transform.BasicConverter(GetCurrentLanguage())
                            DisplayPInvokeCode(bc.ConvertNativeCodeToPInvokeCode(diag.Code))
                        End If
                    End Using
                ElseIf commandName = CommandNames.DotNetSignatureFullName Then
                    handled = True
                    If m_managedToNativeDialog Is Nothing Then
                        m_managedToNativeDialog = New WindowsTool.MainForm(Nothing, True)
                    End If

                    If System.Windows.Forms.DialogResult.OK = m_managedToNativeDialog.ShowDialog() Then
                        DisplayPInvokeCode(m_managedToNativeDialog.SignatureString)
                    End If
                End If
            Catch ex As Exception
                System.Windows.Forms.MessageBox.Show(ex.Message)
            End Try

        End If
    End Sub

    Private Function GetCurrentVsLanguage() As VsLanguage
        Try
            Dim doc As Document = m_dte2.ActiveDocument
            If doc Is Nothing Then
                Return VsLanguage.Unknown
            End If

            Dim name As String = doc.Name
            Dim ext As String = IO.Path.GetExtension(name)
            Select Case ext
                Case ".vb"
                    Return VsLanguage.VisualBasic
                Case ".cs"
                    Return VsLanguage.CSharp
                Case ".h", ".cpp", ".hpp", ".c", ".cxx"
                    Return VsLanguage.CPlusPlus
                Case Else
                    Return VsLanguage.Unknown
            End Select
        Catch ex As Exception
            Return VsLanguage.Unknown
        End Try
    End Function

    Private Function GetCurrentLanguage() As Transform.LanguageType
        Select Case GetCurrentVsLanguage()
            Case VsLanguage.CSharp
                Return Transform.LanguageType.CSharp
            Case VsLanguage.VisualBasic
                Return Transform.LanguageType.VisualBasic
            Case Else
                Return Transform.LanguageType.VisualBasic
        End Select
    End Function

    Private Sub DisplayPInvokeCode(ByVal bag As NativeSymbolBag)
        Dim converter As New Transform.BasicConverter(GetCurrentLanguage())
        Dim col As CodeDom.CodeTypeDeclarationCollection = converter.ConvertToCodeDom(bag, New ErrorProvider())

        Try
            Dim ins As New CodeDomInsterter(Me.m_dte, GetCurrentLanguage())
            For Each ctd As CodeDom.CodeTypeDeclaration In col
                ins.Insert(ctd)
            Next
        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            Dim code As String = converter.ConvertToPInvokeCode(bag)
            DisplayPInvokeCode(code)
        End Try
    End Sub

    Private Sub DisplayPInvokeCode(ByVal code As String)
        Dim doc As TextDocument = DirectCast(m_dte2.ActiveDocument.Object, TextDocument)
        Dim ep As EditPoint = doc.CreateEditPoint()
        ep.EndOfDocument()
        ep.Insert(code)
    End Sub

    Private Sub EnsureNativeStorage()

        If Not m_loaded Then
            Try
                NativeStorage.DefaultInstance = NativeStorage.LoadFromKnownDataFilePath()
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End If
    End Sub


End Class
