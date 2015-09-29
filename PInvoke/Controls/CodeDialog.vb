' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Windows.Forms
Imports PInvoke
Imports PInvoke.Parser

Namespace Controls

    Public Class CodeDialog

        Private Class Data
            Public Text As String
            Public InitialMacroList As List(Of Macro)
        End Class

        Private _initialMacroList As List(Of Macro)
        Private _changed As Boolean

        Public Property Code() As String
            Get
                Return m_codeTb.Text
            End Get
            Set(ByVal value As String)
                m_codeTb.Text = value
            End Set
        End Property

        Private Sub m_okBtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_okBtn.Click
            Me.DialogResult = Windows.Forms.DialogResult.OK
            Me.Close()
        End Sub

        Private Sub RunWorker()
            _changed = False
            m_errorsTb.Text = "Parsing ..."
            If _initialMacroList Is Nothing Then
                _initialMacroList = NativeStorage.DefaultInstance.LoadAllMacros()
            End If

            Dim data As New Data()
            data.InitialMacroList = _initialMacroList
            data.Text = m_codeTb.Text
            m_bgWorker.RunWorkerAsync(data)
        End Sub

        Private Sub OnCodeChanged(ByVal sender As Object, ByVal e As EventArgs) Handles m_codeTb.TextChanged
            If m_bgWorker.IsBusy Then
                _changed = True
            Else
                RunWorker()
            End If
        End Sub

        Private Sub OnCompleteBackgroundCompile(ByVal sender As System.Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles m_bgWorker.RunWorkerCompleted
            m_errorsTb.Text = DirectCast(e.Result, String)
            If _changed Then
                RunWorker()
            End If
        End Sub

        Private Shared Sub OnDoBackgroundCompile(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles m_bgWorker.DoWork
            Dim result As String
            Try
                Dim data As Data = DirectCast(e.Argument, Data)
                Dim code As String = data.Text
                Dim analyzer As NativeCodeAnalyzer = NativeCodeAnalyzerFactory.CreateForMiniParse(OsVersion.WindowsVista, data.InitialMacroList)
                Using reader As New IO.StringReader(code)
                    Dim parseResult As NativeCodeAnalyzerResult = analyzer.Analyze(reader)
                    Dim ep As ErrorProvider = parseResult.ErrorProvider
                    If ep.Warnings.Count = 0 AndAlso ep.Errors.Count = 0 Then
                        result = "None ..."
                    Else
                        result = ep.CreateDisplayString()
                    End If
                End Using
            Catch ex As Exception
                result = ex.Message
            End Try

            e.Result = result
        End Sub

        Private Sub OnCodeKeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles m_codeTb.KeyDown
            If e.KeyCode = Keys.A AndAlso e.Modifiers = Keys.Control Then
                m_codeTb.SelectAll()
                e.Handled = True
            Else
                e.Handled = False
            End If
        End Sub
    End Class

End Namespace
