' Copyright (c) Microsoft Corporation.  All rights reserved.
Namespace Controls
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class SymbolDisplayControl
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
            Me.SplitContainer1 = New System.Windows.Forms.SplitContainer
            Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel
            Me.Label1 = New System.Windows.Forms.Label
            Me.m_nameTb = New System.Windows.Forms.TextBox
            Me.Label2 = New System.Windows.Forms.Label
            Me.m_searchKindCb = New System.Windows.Forms.ComboBox
            Me.Label3 = New System.Windows.Forms.Label
            Me.m_languageCb = New System.Windows.Forms.ComboBox
            Me.FlowLayoutPanel2 = New System.Windows.Forms.FlowLayoutPanel
            Me.m_autoGenerateCBox = New System.Windows.Forms.CheckBox
            Me.m_generateBtn = New System.Windows.Forms.Button
            Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.DataGridViewTextBoxColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.DataGridViewTextBoxColumn6 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.DataGridViewTextBoxColumn7 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.DataGridViewTextBoxColumn8 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.m_searchGrid = New PInvoke.Controls.SearchDataGrid
            Me.DataGridViewTextBoxColumn9 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.DataGridViewTextBoxColumn10 = New System.Windows.Forms.DataGridViewTextBoxColumn
            Me.m_codeBox = New PInvoke.Controls.CodeBox
            Me.SplitContainer1.Panel1.SuspendLayout()
            Me.SplitContainer1.Panel2.SuspendLayout()
            Me.SplitContainer1.SuspendLayout()
            Me.TableLayoutPanel1.SuspendLayout()
            Me.FlowLayoutPanel2.SuspendLayout()
            CType(Me.m_searchGrid, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
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
            Me.SplitContainer1.Panel2.Controls.Add(Me.m_codeBox)
            Me.SplitContainer1.Size = New System.Drawing.Size(887, 579)
            Me.SplitContainer1.SplitterDistance = 404
            Me.SplitContainer1.TabIndex = 11
            '
            'TableLayoutPanel1
            '
            Me.TableLayoutPanel1.ColumnCount = 2
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle)
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.m_nameTb, 1, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.Label2, 0, 1)
            Me.TableLayoutPanel1.Controls.Add(Me.m_searchKindCb, 1, 1)
            Me.TableLayoutPanel1.Controls.Add(Me.Label3, 0, 2)
            Me.TableLayoutPanel1.Controls.Add(Me.m_languageCb, 1, 2)
            Me.TableLayoutPanel1.Controls.Add(Me.m_searchGrid, 0, 4)
            Me.TableLayoutPanel1.Controls.Add(Me.FlowLayoutPanel2, 0, 4)
            Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
            Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
            Me.TableLayoutPanel1.RowCount = 6
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel1.Size = New System.Drawing.Size(404, 579)
            Me.TableLayoutPanel1.TabIndex = 0
            '
            'Label1
            '
            Me.Label1.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.Label1.AutoSize = True
            Me.Label1.Location = New System.Drawing.Point(3, 6)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(35, 13)
            Me.Label1.TabIndex = 1
            Me.Label1.Text = "&Name"
            '
            'm_nameTb
            '
            Me.m_nameTb.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_nameTb.Location = New System.Drawing.Point(64, 3)
            Me.m_nameTb.Name = "m_nameTb"
            Me.m_nameTb.Size = New System.Drawing.Size(337, 20)
            Me.m_nameTb.TabIndex = 2
            '
            'Label2
            '
            Me.Label2.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.Label2.AutoSize = True
            Me.Label2.Location = New System.Drawing.Point(3, 33)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(28, 13)
            Me.Label2.TabIndex = 3
            Me.Label2.Text = "&Kind"
            '
            'm_searchKindCb
            '
            Me.m_searchKindCb.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_searchKindCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_searchKindCb.FormattingEnabled = True
            Me.m_searchKindCb.Location = New System.Drawing.Point(64, 29)
            Me.m_searchKindCb.Name = "m_searchKindCb"
            Me.m_searchKindCb.Size = New System.Drawing.Size(337, 21)
            Me.m_searchKindCb.TabIndex = 4
            '
            'Label3
            '
            Me.Label3.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.Label3.AutoSize = True
            Me.Label3.Location = New System.Drawing.Point(3, 60)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(55, 13)
            Me.Label3.TabIndex = 5
            Me.Label3.Text = "&Language"
            '
            'm_languageCb
            '
            Me.m_languageCb.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_languageCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.m_languageCb.FormattingEnabled = True
            Me.m_languageCb.Location = New System.Drawing.Point(64, 56)
            Me.m_languageCb.Name = "m_languageCb"
            Me.m_languageCb.Size = New System.Drawing.Size(337, 21)
            Me.m_languageCb.TabIndex = 6
            '
            'FlowLayoutPanel2
            '
            Me.FlowLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Right
            Me.FlowLayoutPanel2.AutoSize = True
            Me.FlowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.TableLayoutPanel1.SetColumnSpan(Me.FlowLayoutPanel2, 2)
            Me.FlowLayoutPanel2.Controls.Add(Me.m_autoGenerateCBox)
            Me.FlowLayoutPanel2.Controls.Add(Me.m_generateBtn)
            Me.FlowLayoutPanel2.Location = New System.Drawing.Point(222, 80)
            Me.FlowLayoutPanel2.Margin = New System.Windows.Forms.Padding(0)
            Me.FlowLayoutPanel2.Name = "FlowLayoutPanel2"
            Me.FlowLayoutPanel2.Size = New System.Drawing.Size(182, 29)
            Me.FlowLayoutPanel2.TabIndex = 7
            '
            'm_autoGenerateCBox
            '
            Me.m_autoGenerateCBox.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.m_autoGenerateCBox.AutoSize = True
            Me.m_autoGenerateCBox.Location = New System.Drawing.Point(3, 6)
            Me.m_autoGenerateCBox.Name = "m_autoGenerateCBox"
            Me.m_autoGenerateCBox.Size = New System.Drawing.Size(95, 17)
            Me.m_autoGenerateCBox.TabIndex = 8
            Me.m_autoGenerateCBox.Text = "&Auto Generate"
            Me.m_autoGenerateCBox.UseVisualStyleBackColor = True
            '
            'm_generateBtn
            '
            Me.m_generateBtn.Location = New System.Drawing.Point(104, 3)
            Me.m_generateBtn.Name = "m_generateBtn"
            Me.m_generateBtn.Size = New System.Drawing.Size(75, 23)
            Me.m_generateBtn.TabIndex = 9
            Me.m_generateBtn.Text = "&Generate"
            Me.m_generateBtn.UseVisualStyleBackColor = True
            '
            'DataGridViewTextBoxColumn3
            '
            Me.DataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
            Me.DataGridViewTextBoxColumn3.HeaderText = "Name"
            Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
            Me.DataGridViewTextBoxColumn3.ReadOnly = True
            Me.DataGridViewTextBoxColumn3.Width = 180
            '
            'DataGridViewTextBoxColumn4
            '
            Me.DataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
            Me.DataGridViewTextBoxColumn4.HeaderText = "Value"
            Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
            Me.DataGridViewTextBoxColumn4.ReadOnly = True
            '
            'DataGridViewTextBoxColumn1
            '
            Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
            Me.DataGridViewTextBoxColumn1.HeaderText = "Name"
            Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
            Me.DataGridViewTextBoxColumn1.ReadOnly = True
            Me.DataGridViewTextBoxColumn1.Width = 180
            '
            'DataGridViewTextBoxColumn2
            '
            Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
            Me.DataGridViewTextBoxColumn2.HeaderText = "Value"
            Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
            Me.DataGridViewTextBoxColumn2.ReadOnly = True
            '
            'DataGridViewTextBoxColumn5
            '
            Me.DataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
            Me.DataGridViewTextBoxColumn5.HeaderText = "Name"
            Me.DataGridViewTextBoxColumn5.Name = "DataGridViewTextBoxColumn5"
            Me.DataGridViewTextBoxColumn5.ReadOnly = True
            Me.DataGridViewTextBoxColumn5.Width = 180
            '
            'DataGridViewTextBoxColumn6
            '
            Me.DataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
            Me.DataGridViewTextBoxColumn6.HeaderText = "Value"
            Me.DataGridViewTextBoxColumn6.Name = "DataGridViewTextBoxColumn6"
            Me.DataGridViewTextBoxColumn6.ReadOnly = True
            '
            'DataGridViewTextBoxColumn7
            '
            Me.DataGridViewTextBoxColumn7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
            Me.DataGridViewTextBoxColumn7.HeaderText = "Name"
            Me.DataGridViewTextBoxColumn7.Name = "DataGridViewTextBoxColumn7"
            Me.DataGridViewTextBoxColumn7.ReadOnly = True
            Me.DataGridViewTextBoxColumn7.Width = 180
            '
            'DataGridViewTextBoxColumn8
            '
            Me.DataGridViewTextBoxColumn8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
            Me.DataGridViewTextBoxColumn8.HeaderText = "Value"
            Me.DataGridViewTextBoxColumn8.MinimumWidth = 100
            Me.DataGridViewTextBoxColumn8.Name = "DataGridViewTextBoxColumn8"
            Me.DataGridViewTextBoxColumn8.ReadOnly = True
            '
            'm_searchGrid
            '
            Me.m_searchGrid.AllowUserToAddRows = False
            Me.m_searchGrid.AllowUserToDeleteRows = False
            Me.m_searchGrid.AllowUserToResizeRows = False
            Me.m_searchGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
            Me.TableLayoutPanel1.SetColumnSpan(Me.m_searchGrid, 2)
            Me.m_searchGrid.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_searchGrid.Location = New System.Drawing.Point(3, 112)
            Me.m_searchGrid.Name = "m_searchGrid"
            Me.m_searchGrid.ReadOnly = True
            Me.m_searchGrid.RowHeadersVisible = False
            Me.m_searchGrid.SearchKind = PInvoke.Controls.SearchKind.None
            Me.m_searchGrid.SearchText = Nothing
            Me.m_searchGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
            Me.m_searchGrid.ShowInvalidData = False
            Me.m_searchGrid.Size = New System.Drawing.Size(398, 464)
            Me.m_searchGrid.StandardTab = True
            Me.m_searchGrid.TabIndex = 10
            Me.m_searchGrid.VirtualMode = True
            '
            'DataGridViewTextBoxColumn9
            '
            Me.DataGridViewTextBoxColumn9.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
            Me.DataGridViewTextBoxColumn9.HeaderText = "Name"
            Me.DataGridViewTextBoxColumn9.Name = "DataGridViewTextBoxColumn9"
            Me.DataGridViewTextBoxColumn9.ReadOnly = True
            Me.DataGridViewTextBoxColumn9.Width = 180
            '
            'DataGridViewTextBoxColumn10
            '
            Me.DataGridViewTextBoxColumn10.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
            Me.DataGridViewTextBoxColumn10.HeaderText = "Value"
            Me.DataGridViewTextBoxColumn10.Name = "DataGridViewTextBoxColumn10"
            Me.DataGridViewTextBoxColumn10.ReadOnly = True
            '
            'm_codeBox
            '
            Me.m_codeBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_codeBox.Location = New System.Drawing.Point(0, 0)
            Me.m_codeBox.Name = "m_codeBox"
            Me.m_codeBox.Size = New System.Drawing.Size(479, 579)
            Me.m_codeBox.TabIndex = 12
            '
            'SymbolDisplayControl
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.SplitContainer1)
            Me.Name = "SymbolDisplayControl"
            Me.Size = New System.Drawing.Size(887, 579)
            Me.SplitContainer1.Panel1.ResumeLayout(False)
            Me.SplitContainer1.Panel2.ResumeLayout(False)
            Me.SplitContainer1.ResumeLayout(False)
            Me.TableLayoutPanel1.ResumeLayout(False)
            Me.TableLayoutPanel1.PerformLayout()
            Me.FlowLayoutPanel2.ResumeLayout(False)
            Me.FlowLayoutPanel2.PerformLayout()
            CType(Me.m_searchGrid, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
        Friend WithEvents m_codeBox As PInvoke.Controls.CodeBox
        Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Friend WithEvents m_nameTb As System.Windows.Forms.TextBox
        Friend WithEvents Label2 As System.Windows.Forms.Label
        Friend WithEvents m_searchKindCb As System.Windows.Forms.ComboBox
        Friend WithEvents Label3 As System.Windows.Forms.Label
        Friend WithEvents m_languageCb As System.Windows.Forms.ComboBox
        Friend WithEvents m_searchGrid As PInvoke.Controls.SearchDataGrid
        Friend WithEvents FlowLayoutPanel2 As System.Windows.Forms.FlowLayoutPanel
        Friend WithEvents m_autoGenerateCBox As System.Windows.Forms.CheckBox
        Friend WithEvents m_generateBtn As System.Windows.Forms.Button
        Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn4 As System.Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn5 As System.Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn6 As System.Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn7 As System.Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn8 As System.Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn9 As System.Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DataGridViewTextBoxColumn10 As System.Windows.Forms.DataGridViewTextBoxColumn

    End Class
End Namespace
