' Copyright (c) Microsoft Corporation.  All rights reserved.
Namespace Controls

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class CodeDialog
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel
            Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel
            Me.m_okBtn = New System.Windows.Forms.Button
            Me.m_cancelBtn = New System.Windows.Forms.Button
            Me.Label1 = New System.Windows.Forms.Label
            Me.m_codeTb = New System.Windows.Forms.TextBox
            Me.Label2 = New System.Windows.Forms.Label
            Me.m_errorsTb = New System.Windows.Forms.TextBox
            Me.m_bgWorker = New System.ComponentModel.BackgroundWorker
            Me.TableLayoutPanel1.SuspendLayout()
            Me.FlowLayoutPanel1.SuspendLayout()
            Me.SuspendLayout()
            '
            'TableLayoutPanel1
            '
            Me.TableLayoutPanel1.ColumnCount = 2
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle)
            Me.TableLayoutPanel1.Controls.Add(Me.FlowLayoutPanel1, 1, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.m_codeTb, 0, 1)
            Me.TableLayoutPanel1.Controls.Add(Me.Label2, 0, 2)
            Me.TableLayoutPanel1.Controls.Add(Me.m_errorsTb, 0, 3)
            Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
            Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
            Me.TableLayoutPanel1.RowCount = 4
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
            Me.TableLayoutPanel1.Size = New System.Drawing.Size(644, 460)
            Me.TableLayoutPanel1.TabIndex = 0
            '
            'FlowLayoutPanel1
            '
            Me.FlowLayoutPanel1.AutoSize = True
            Me.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.FlowLayoutPanel1.Controls.Add(Me.m_okBtn)
            Me.FlowLayoutPanel1.Controls.Add(Me.m_cancelBtn)
            Me.FlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
            Me.FlowLayoutPanel1.Location = New System.Drawing.Point(560, 3)
            Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
            Me.TableLayoutPanel1.SetRowSpan(Me.FlowLayoutPanel1, 2)
            Me.FlowLayoutPanel1.Size = New System.Drawing.Size(81, 58)
            Me.FlowLayoutPanel1.TabIndex = 1
            '
            'm_okBtn
            '
            Me.m_okBtn.Location = New System.Drawing.Point(3, 3)
            Me.m_okBtn.Name = "m_okBtn"
            Me.m_okBtn.Size = New System.Drawing.Size(75, 23)
            Me.m_okBtn.TabIndex = 0
            Me.m_okBtn.Text = "OK"
            Me.m_okBtn.UseVisualStyleBackColor = True
            '
            'm_cancelBtn
            '
            Me.m_cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_cancelBtn.Location = New System.Drawing.Point(3, 32)
            Me.m_cancelBtn.Name = "m_cancelBtn"
            Me.m_cancelBtn.Size = New System.Drawing.Size(75, 23)
            Me.m_cancelBtn.TabIndex = 1
            Me.m_cancelBtn.Text = "Cancel"
            Me.m_cancelBtn.UseVisualStyleBackColor = True
            '
            'Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Location = New System.Drawing.Point(3, 0)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(60, 13)
            Me.Label1.TabIndex = 0
            Me.Label1.Text = "Enter Code"
            '
            'm_codeTb
            '
            Me.m_codeTb.AcceptsReturn = True
            Me.m_codeTb.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_codeTb.Location = New System.Drawing.Point(3, 16)
            Me.m_codeTb.Multiline = True
            Me.m_codeTb.Name = "m_codeTb"
            Me.m_codeTb.ScrollBars = System.Windows.Forms.ScrollBars.Both
            Me.m_codeTb.Size = New System.Drawing.Size(551, 297)
            Me.m_codeTb.TabIndex = 1
            '
            'Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Location = New System.Drawing.Point(3, 316)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(103, 13)
            Me.Label2.TabIndex = 2
            Me.Label2.Text = "Errors and Warnings"
            '
            'm_errorsTb
            '
            Me.m_errorsTb.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_errorsTb.Location = New System.Drawing.Point(3, 332)
            Me.m_errorsTb.Multiline = True
            Me.m_errorsTb.Name = "m_errorsTb"
            Me.m_errorsTb.ReadOnly = True
            Me.m_errorsTb.Size = New System.Drawing.Size(551, 125)
            Me.m_errorsTb.TabIndex = 3
            '
            'm_bgWorker
            '
            '
            'CodeDialog
            '
            Me.AcceptButton = Me.m_okBtn
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.CancelButton = Me.m_cancelBtn
            Me.ClientSize = New System.Drawing.Size(644, 460)
            Me.Controls.Add(Me.TableLayoutPanel1)
            Me.Name = "CodeDialog"
            Me.Text = "Code Dialog"
            Me.TableLayoutPanel1.ResumeLayout(False)
            Me.TableLayoutPanel1.PerformLayout()
            Me.FlowLayoutPanel1.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Friend WithEvents m_codeTb As System.Windows.Forms.TextBox
        Friend WithEvents FlowLayoutPanel1 As System.Windows.Forms.FlowLayoutPanel
        Friend WithEvents m_okBtn As System.Windows.Forms.Button
        Friend WithEvents m_cancelBtn As System.Windows.Forms.Button
        Friend WithEvents Label2 As System.Windows.Forms.Label
        Friend WithEvents m_errorsTb As System.Windows.Forms.TextBox
        Friend WithEvents m_bgWorker As System.ComponentModel.BackgroundWorker
    End Class


End Namespace
