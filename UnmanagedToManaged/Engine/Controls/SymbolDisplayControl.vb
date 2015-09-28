' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.ComponentModel
Imports System.Windows.Forms
Imports PInvoke
Imports PInvoke.Transform

Namespace Controls

    Public Class SymbolDisplayControl

        Private m_ns As NativeStorage
        Private m_conv As BasicConverter

        ''' <summary>
        ''' Kind of search being performed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property SearchKind() As SearchKind
            Get
                Return m_searchGrid.SearchKind
            End Get
            Set(ByVal value As SearchKind)
                m_searchGrid.SearchKind = value
                If m_searchKindCb.SelectedItem IsNot Nothing Then
                    m_searchKindCb.SelectedItem = value
                End If
            End Set
        End Property

        Public Property AutoGenerate() As Boolean
            Get
                Return m_autoGenerateCBox.Checked
            End Get
            Set(ByVal value As Boolean)
                m_autoGenerateCBox.Checked = value
            End Set
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property ShowAll() As Boolean
            Get
                Return m_searchGrid.ShowInvalidData
            End Get
            Set(ByVal value As Boolean)
                m_searchGrid.ShowInvalidData = value
            End Set
        End Property

        Public Event SearchKindChanged As EventHandler

        Public Sub New()
            m_ns = PInvoke.NativeStorage.DefaultInstance
            m_conv = New BasicConverter(LanguageType.VisualBasic)

            ' This call is required by the Windows Form Designer.
            InitializeComponent()

            ' Populate the combo boxes
            m_languageCb.Items.AddRange(PInvoke.EnumUtil.GetAllValuesObject(Of LanguageType))
            m_languageCb.SelectedItem = LanguageType.VisualBasic
            m_searchKindCb.Items.AddRange(PInvoke.EnumUtil.GetAllValuesObjectExcept(SearchKind.None))
            m_searchKindCb.SelectedItem = SearchKind.All

            ' Initialize the values
            OnSearchKindChanged(Nothing, EventArgs.Empty)
            OnLanguageChanged(Nothing, EventArgs.Empty)
        End Sub

#Region "ISignatureImportControl"

        ''' <summary>
        ''' Language that we are displaying the generated values in
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property LanguageType() As LanguageType Implements ISignatureImportControl.LanguageType
            Get
                If m_languageCb.SelectedItem Is Nothing Then
                    Return Transform.LanguageType.VisualBasic
                End If

                Return DirectCast(m_languageCb.SelectedItem, LanguageType)
            End Get
            Set(ByVal value As LanguageType)
                m_languageCb.SelectedItem = value
            End Set
        End Property

        ''' <summary>
        ''' NativeStorage instance to use
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property NativeStorage() As NativeStorage Implements ISignatureImportControl.NativeStorage
            Get
                Return m_ns
            End Get
            Set(ByVal value As NativeStorage)
                m_ns = value
                m_conv.NativeStorage = value
                m_searchGrid.NativeStorage = value
            End Set
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property TransformKindFlags() As Transform.TransformKindFlags Implements ISignatureImportControl.TransformKindFlags
            Get
                Return m_conv.TransformKindFlags
            End Get
            Set(ByVal value As Transform.TransformKindFlags)
                m_conv.TransformKindFlags = value
            End Set
        End Property

        Public Event LanguageTypeChanged As EventHandler Implements ISignatureImportControl.LanguageTypeChanged

        ''' <summary>
        ''' Current displayed managed code
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ManagedCode() As String Implements ISignatureImportControl.ManagedCode
            Get
                Return m_codeBox.Text
            End Get
        End Property

#End Region

#Region "Event Handlers"

        ''' <summary>
        ''' When the search kind changes update the grid.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub OnSearchKindChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_searchKindCb.SelectedIndexChanged
            If m_searchKindCb.SelectedItem IsNot Nothing Then
                Dim kind As SearchKind = DirectCast(m_searchKindCb.SelectedItem, SearchKind)
                If m_searchGrid IsNot Nothing Then
                    m_searchGrid.SearchKind = kind
                End If

                RaiseEvent SearchKindChanged(Me, EventArgs.Empty)
            End If

        End Sub

        ''' <summary>
        ''' When the language changes make sure to rebuild the converter as it depends on the current language
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub OnLanguageChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_languageCb.SelectedIndexChanged
            ' During initialization this can be true
            m_conv.LanguageType = Me.LanguageType
            RaiseEvent LanguageTypeChanged(Me, EventArgs.Empty)

            AutoGenerateCode()
        End Sub

        Private Sub OnSearchGridSelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_searchGrid.SelectedSymbolsChanged
            AutoGenerateCode()
        End Sub

        Private Sub OnNameChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_nameTb.TextChanged
            If m_searchGrid IsNot Nothing Then
                m_searchGrid.SearchText = m_nameTb.Text
            End If
        End Sub

        Private Sub OnGenerateClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_generateBtn.Click

            If m_searchGrid.SelectedRows.Count > 400 Then
                Dim title As String = "Generation"
#If DEBUG Then
                title &= " (" & m_searchGrid.SelectedRows.Count & ")"
#End If
                Dim result As DialogResult = MessageBox.Show( _
                    "Generating the output might take a lot of time. Do you want to proceed?", _
                    title, _
                    MessageBoxButtons.YesNo, _
                    MessageBoxIcon.Information, _
                    MessageBoxDefaultButton.Button1)
                If result = DialogResult.No Then
                    Return
                End If
            End If

            Dim oldCursor As Cursor = Me.Cursor
            Try
                Cursor = Cursors.WaitCursor
                GenerateCode(True)
            Finally
                Me.Cursor = oldCursor
            End Try

        End Sub

        Private Sub OnAutoGenerateClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
            AutoGenerateCode()
        End Sub

        Private Sub m_nameTb_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles m_nameTb.KeyDown
            If e.KeyCode = Keys.A And e.Modifiers = Keys.Control Then
                m_nameTb.SelectAll()
                e.Handled = True
            End If
        End Sub

#End Region

#Region "Private Helpers"

        ''' <summary>
        ''' Regenerate the code if AutoGenerate is checked
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub AutoGenerateCode()
            If Me.AutoGenerate Then
                GenerateCode(False)
            End If
        End Sub

        Private Sub GenerateCode(ByVal force As Boolean)
            m_codeBox.Text = String.Empty

            Dim text As String
            If force OrElse m_searchGrid.SelectedRows.Count <= 5 Then
                text = m_conv.ConvertToPInvokeCode(m_searchGrid.SelectedSymbolBag)
            Else
                text = "More than 5 rows selected.  Will not autogenerate"
            End If

            m_codeBox.Code = text
        End Sub

#End Region

    End Class

End Namespace
