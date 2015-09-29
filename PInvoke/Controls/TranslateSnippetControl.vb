' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows.Forms
Imports PInvoke.Parser
Imports PInvoke.Transform

Namespace Controls

    Public Class TranslateSnippetControl

        Private Class RequestData
            Friend Text As String
            Friend InitialMacroList As ReadOnlyCollection(Of Macro)
        End Class

        Private Class ResponseData
            Friend ParseOutput As String
        End Class

        Private _ns As NativeStorage = NativeStorage.DefaultInstance
        Private _transKind As TransformKindFlags = TransformKindFlags.All
        Private _initialMacroList As New List(Of Macro)
        Private _changed As Boolean

        Public Property AutoGenerate() As Boolean
            Get
                Return m_autoGenerateBtn.Checked
            End Get
            Set(ByVal value As Boolean)
                m_autoGenerateBtn.Checked = value
            End Set
        End Property

        Public Sub New()

            ' This call is required by the Windows Form Designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            m_langTypeCb.Items.AddRange(PInvoke.EnumUtil.GetAllValuesObject(Of LanguageType))
            m_langTypeCb.SelectedItem = LanguageType.VisualBasic
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
                If m_langTypeCb.SelectedItem Is Nothing Then
                    Return Transform.LanguageType.VisualBasic
                End If

                Return DirectCast(m_langTypeCb.SelectedItem, LanguageType)
            End Get
            Set(ByVal value As LanguageType)
                m_langTypeCb.SelectedItem = value
            End Set
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property NativeStorage() As NativeStorage Implements ISignatureImportControl.NativeStorage
            Get
                Return _ns
            End Get
            Set(ByVal value As NativeStorage)
                _ns = value
                _initialMacroList = Nothing
            End Set
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property TransformKindFlags() As TransformKindFlags Implements ISignatureImportControl.TransformKindFlags
            Get
                Return _transKind
            End Get
            Set(ByVal value As TransformKindFlags)
                _transKind = value
            End Set
        End Property

        Public Event LanguageTypeChanged As EventHandler Implements ISignatureImportControl.LanguageTypeChanged

        Public ReadOnly Property ManagedCode() As String Implements ISignatureImportControl.ManagedCode
            Get
                Return m_managedCodeBox.Text
            End Get
        End Property

#End Region

#Region "Event Handlers"

        Private Sub OnNativeCodeChanged(ByVal sender As Object, ByVal e As EventArgs) Handles m_nativeCodeTb.TextChanged
            If m_bgWorker.IsBusy Then
                _changed = True
            Else
                RunWorker()
            End If
        End Sub

        Private Shared Sub OnDoBackgroundWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles m_bgWorker.DoWork
            Dim result As New ResponseData
            Try
                Dim req As RequestData = DirectCast(e.Argument, RequestData)
                Dim code As String = req.Text
                Dim analyzer As NativeCodeAnalyzer = NativeCodeAnalyzerFactory.CreateForMiniParse(OsVersion.WindowsVista, req.InitialMacroList)
                Using reader As New IO.StringReader(code)
                    Dim parseResult As NativeCodeAnalyzerResult = analyzer.Analyze(reader)
                    Dim ep As ErrorProvider = parseResult.ErrorProvider
                    If ep.Warnings.Count = 0 AndAlso ep.Errors.Count = 0 Then
                        result.ParseOutput = "None ..."
                    Else
                        result.ParseOutput = ep.CreateDisplayString()
                    End If
                End Using
            Catch ex As Exception
                result.ParseOutput = ex.Message
            End Try

            e.Result = result
        End Sub

        Private Sub OnBackgroundOperationCompleted(ByVal sender As System.Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles m_bgWorker.RunWorkerCompleted
            Dim response As ResponseData = DirectCast(e.Result, ResponseData)
            m_errorsTb.Text = response.ParseOutput
            If _changed Then
                RunWorker()
            ElseIf m_autoGenerateBtn.Checked Then
                GenerateCode()
            End If

        End Sub

        Private Sub OnGenerateCodeClick(ByVal sender As Object, ByVal e As EventArgs) Handles m_generateBtn.Click
            GenerateCode()
        End Sub

        Private Sub OnAutoGenerateCodeCheckChanged(ByVal sender As Object, ByVal e As EventArgs) Handles m_autoGenerateBtn.CheckedChanged
            If m_autoGenerateBtn.Checked Then
                GenerateCode()
            End If
        End Sub

        Private Sub OnLanguageTypeChanged(ByVal sender As Object, ByVal e As EventArgs) Handles m_langTypeCb.SelectedIndexChanged
            If m_autoGenerateBtn.Checked Then
                GenerateCode()
            End If

            RaiseEvent LanguageTypeChanged(Me, EventArgs.Empty)
        End Sub

        Private Sub m_nativeCodeTb_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles m_nativeCodeTb.KeyDown
            If e.KeyCode = Keys.A And e.Modifiers = Keys.Control Then
                m_nativeCodeTb.SelectAll()
                e.Handled = True
            End If
        End Sub

#End Region

#Region "Helpers"

        Private Sub RunWorker()
            _changed = False
            m_errorsTb.Text = "Parsing ..."
            If _initialMacroList Is Nothing Then
                _initialMacroList = _ns.LoadAllMacros()
            End If

            Dim data As New RequestData()
            data.InitialMacroList = New ReadOnlyCollection(Of Macro)(_initialMacroList)
            data.Text = m_nativeCodeTb.Text
            m_bgWorker.RunWorkerAsync(data)
        End Sub

        Private Sub GenerateCode()
            Try
                Dim conv As New BasicConverter(LanguageType, _ns)
                conv.TransformKindFlags = _transKind
                m_managedCodeBox.Code = conv.ConvertNativeCodeToPInvokeCode(m_nativeCodeTb.Text)
            Catch ex As Exception
                m_managedCodeBox.Code = ex.Message
            End Try
        End Sub

#End Region

    End Class

End Namespace
