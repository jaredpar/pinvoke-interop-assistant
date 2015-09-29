' Copyright (c) Microsoft Corporation.  All rights reserved.
Namespace Controls

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class TranslateSnippetControl
        Inherits System.Windows.Forms.UserControl
        Implements ISignatureImportControl

        'UserControl overrides dispose to clean up the component list.
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
            Me.m_autoGenerateBtn = New System.Windows.Forms.CheckBox
            Me.m_generateBtn = New System.Windows.Forms.Button
            Me.m_errorsTb = New System.Windows.Forms.RichTextBox
            Me.Label1 = New System.Windows.Forms.Label
            Me.Label2 = New System.Windows.Forms.Label
            Me.Label3 = New System.Windows.Forms.Label
            Me.m_langTypeCb = New System.Windows.Forms.ComboBox
            Me.m_nativeCodeTb = New System.Windows.Forms.TextBox
            Me.SplitContainer1 = New System.Windows.Forms.SplitContainer
            Me.m_bgWorker = New System.ComponentModel.BackgroundWorker
            Me.m_managedCodeBox = New PInvoke.Controls.CodeBox
            Me.TableLayoutPanel1.SuspendLayout()
            Me.FlowLayoutPanel1.SuspendLayout()
            Me.SplitContainer1.Panel1.SuspendLayout()
            Me.SplitContainer1.Panel2.SuspendLayout()
            Me.SplitContainer1.SuspendLayout()
            Me.SuspendLayout()
            '
            'TableLayoutPanel1
            '
            Me.TableLayoutPanel1.ColumnCount = 2
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 62.0!))
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel1.Controls.Add(Me.FlowLayoutPanel1, 0, 5)
            Me.TableLayoutPanel1.Controls.Add(Me.m_errorsTb, 0, 4)
            Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 1)
            Me.TableLayoutPanel1.Controls.Add(Me.Label2, 0, 3)
            Me.TableLayoutPanel1.Controls.Add(Me.Label3, 0, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.m_langTypeCb, 1, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.m_nativeCodeTb, 0, 2)
            Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
            Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
            Me.TableLayoutPanel1.RowCount = 6
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.Size = New System.Drawing.Size(318, 450)
            Me.TableLayoutPanel1.TabIndex = 0
            '
            'FlowLayoutPanel1
            '
            Me.FlowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Right
            Me.FlowLayoutPanel1.AutoSize = True
            Me.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.TableLayoutPanel1.SetColumnSpan(Me.FlowLayoutPanel1, 2)
            Me.FlowLayoutPanel1.Controls.Add(Me.m_autoGenerateBtn)
            Me.FlowLayoutPanel1.Controls.Add(Me.m_generateBtn)
            Me.FlowLayoutPanel1.Location = New System.Drawing.Point(136, 421)
            Me.FlowLayoutPanel1.Margin = New System.Windows.Forms.Padding(0)
            Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
            Me.FlowLayoutPanel1.Size = New System.Drawing.Size(182, 29)
            Me.FlowLayoutPanel1.TabIndex = 7
            '
            'm_autoGenerateBtn
            '
            Me.m_autoGenerateBtn.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.m_autoGenerateBtn.AutoSize = True
            Me.m_autoGenerateBtn.Location = New System.Drawing.Point(3, 6)
            Me.m_autoGenerateBtn.Name = "m_autoGenerateBtn"
            Me.m_autoGenerateBtn.Size = New System.Drawing.Size(95, 17)
            Me.m_autoGenerateBtn.TabIndex = 8
            Me.m_autoGenerateBtn.Text = "&Auto Generate"
            Me.m_autoGenerateBtn.UseVisualStyleBackColor = True
            '
            'm_generateBtn
            '
            Me.m_generateBtn.Anchor = System.Windows.Forms.AnchorStyles.Right
            Me.m_generateBtn.Location = New System.Drawing.Point(104, 3)
            Me.m_generateBtn.Name = "m_generateBtn"
            Me.m_generateBtn.Size = New System.Drawing.Size(75, 23)
            Me.m_generateBtn.TabIndex = 9
            Me.m_generateBtn.Text = "&Generate"
            Me.m_generateBtn.UseVisualStyleBackColor = True
            '
            'm_errorsTb
            '
            Me.TableLayoutPanel1.SetColumnSpan(Me.m_errorsTb, 2)
            Me.m_errorsTb.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_errorsTb.Location = New System.Drawing.Point(3, 240)
            Me.m_errorsTb.Name = "m_errorsTb"
            Me.m_errorsTb.ReadOnly = True
            Me.m_errorsTb.Size = New System.Drawing.Size(312, 178)
            Me.m_errorsTb.TabIndex = 6
            Me.m_errorsTb.Text = ""
            '
            'Label1
            '
            Me.Label1.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.Label1.AutoSize = True
            Me.TableLayoutPanel1.SetColumnSpan(Me.Label1, 2)
            Me.Label1.Location = New System.Drawing.Point(3, 27)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(105, 13)
            Me.Label1.TabIndex = 3
            Me.Label1.Text = "&Native Code Snippet"
            '
            'Label2
            '
            Me.Label2.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.Label2.AutoSize = True
            Me.TableLayoutPanel1.SetColumnSpan(Me.Label2, 2)
            Me.Label2.Location = New System.Drawing.Point(3, 224)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(34, 13)
            Me.Label2.TabIndex = 5
            Me.Label2.Text = "&Errors"
            '
            'Label3
            '
            Me.Label3.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.Label3.AutoSize = True
            Me.Label3.Location = New System.Drawing.Point(3, 7)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(55, 13)
            Me.Label3.TabIndex = 1
            Me.Label3.Text = "&Language"
            '
            'm_langTypeCb
            '
            Me.m_langTypeCb.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_langTypeCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_langTypeCb.FormattingEnabled = True
            Me.m_langTypeCb.Location = New System.Drawing.Point(65, 3)
            Me.m_langTypeCb.Name = "m_langTypeCb"
            Me.m_langTypeCb.Size = New System.Drawing.Size(250, 21)
            Me.m_langTypeCb.TabIndex = 2
            '
            'm_nativeCodeTb
            '
            Me.TableLayoutPanel1.SetColumnSpan(Me.m_nativeCodeTb, 2)
            Me.m_nativeCodeTb.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_nativeCodeTb.Location = New System.Drawing.Point(3, 43)
            Me.m_nativeCodeTb.Multiline = True
            Me.m_nativeCodeTb.Name = "m_nativeCodeTb"
            Me.m_nativeCodeTb.Size = New System.Drawing.Size(312, 178)
            Me.m_nativeCodeTb.TabIndex = 4
            '
            'SplitContainer1
            '
            Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
            Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
            Me.SplitContainer1.Name = "SplitContainer1"
            '
            'SplitContainer1.Panel1
            '
            Me.SplitContainer1.Panel1.Controls.Add(Me.TableLayoutPanel1)
            Me.SplitContainer1.Panel1MinSize = 125
            '
            'SplitContainer1.Panel2
            '
            Me.SplitContainer1.Panel2.Controls.Add(Me.m_managedCodeBox)
            Me.SplitContainer1.Size = New System.Drawing.Size(673, 450)
            Me.SplitContainer1.SplitterDistance = 318
            Me.SplitContainer1.TabIndex = 10
            '
            'm_bgWorker
            '
            '
            'm_managedCodeBox
            '
            Me.m_managedCodeBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_managedCodeBox.Location = New System.Drawing.Point(0, 0)
            Me.m_managedCodeBox.Name = "m_managedCodeBox"
            Me.m_managedCodeBox.Size = New System.Drawing.Size(351, 450)
            Me.m_managedCodeBox.TabIndex = 11
            '
            'TranslateSnippetControl
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.SplitContainer1)
            Me.Name = "TranslateSnippetControl"
            Me.Size = New System.Drawing.Size(673, 450)
            Me.TableLayoutPanel1.ResumeLayout(False)
            Me.TableLayoutPanel1.PerformLayout()
            Me.FlowLayoutPanel1.ResumeLayout(False)
            Me.FlowLayoutPanel1.PerformLayout()
            Me.SplitContainer1.Panel1.ResumeLayout(False)
            Me.SplitContainer1.Panel2.ResumeLayout(False)
            Me.SplitContainer1.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents m_errorsTb As System.Windows.Forms.RichTextBox
        Friend WithEvents m_generateBtn As System.Windows.Forms.Button
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Friend WithEvents Label2 As System.Windows.Forms.Label
        Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
        Friend WithEvents m_managedCodeBox As PInvoke.Controls.CodeBox
        Friend WithEvents m_bgWorker As System.ComponentModel.BackgroundWorker
        Friend WithEvents FlowLayoutPanel1 As System.Windows.Forms.FlowLayoutPanel
        Friend WithEvents m_autoGenerateBtn As System.Windows.Forms.CheckBox
        Friend WithEvents Label3 As System.Windows.Forms.Label
        Friend WithEvents m_langTypeCb As System.Windows.Forms.ComboBox
        Friend WithEvents m_nativeCodeTb As System.Windows.Forms.TextBox

    End Class

End Namespace
