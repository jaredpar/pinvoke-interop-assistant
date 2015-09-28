' Copyright (c) Microsoft Corporation.  All rights reserved.

Namespace Controls
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
        Partial Class SelectSymbolDialog
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                    'm_search.Dispose()
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SelectSymbolDialog))
            Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel
            Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel
            Me.m_okBtn = New System.Windows.Forms.Button
            Me.m_cancelBtn = New System.Windows.Forms.Button
            Me.m_nameTb = New System.Windows.Forms.TextBox
            Me.Label1 = New System.Windows.Forms.Label
            Me.TableLayoutPanel1.SuspendLayout()
            Me.FlowLayoutPanel1.SuspendLayout()
            Me.SuspendLayout()
            '
            'TableLayoutPanel1
            '
            Me.TableLayoutPanel1.ColumnCount = 3
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle)
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle)
            Me.TableLayoutPanel1.Controls.Add(Me.FlowLayoutPanel1, 2, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.m_nameTb, 1, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 0)
            Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
            Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
            Me.TableLayoutPanel1.RowCount = 2
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle)
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel1.Size = New System.Drawing.Size(665, 243)
            Me.TableLayoutPanel1.TabIndex = 0
            '
            'FlowLayoutPanel1
            '
            Me.FlowLayoutPanel1.AutoSize = True
            Me.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.FlowLayoutPanel1.Controls.Add(Me.m_okBtn)
            Me.FlowLayoutPanel1.Controls.Add(Me.m_cancelBtn)
            Me.FlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
            Me.FlowLayoutPanel1.Location = New System.Drawing.Point(581, 3)
            Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
            Me.TableLayoutPanel1.SetRowSpan(Me.FlowLayoutPanel1, 2)
            Me.FlowLayoutPanel1.Size = New System.Drawing.Size(81, 58)
            Me.FlowLayoutPanel1.TabIndex = 3
            '
            'm_okBtn
            '
            Me.m_okBtn.Location = New System.Drawing.Point(3, 3)
            Me.m_okBtn.Name = "m_okBtn"
            Me.m_okBtn.Size = New System.Drawing.Size(75, 23)
            Me.m_okBtn.TabIndex = 3
            Me.m_okBtn.Text = "OK"
            Me.m_okBtn.UseVisualStyleBackColor = True
            '
            'm_cancelBtn
            '
            Me.m_cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.m_cancelBtn.Location = New System.Drawing.Point(3, 32)
            Me.m_cancelBtn.Name = "m_cancelBtn"
            Me.m_cancelBtn.Size = New System.Drawing.Size(75, 23)
            Me.m_cancelBtn.TabIndex = 4
            Me.m_cancelBtn.Text = "Cancel"
            Me.m_cancelBtn.UseVisualStyleBackColor = True
            '
            'm_nameTb
            '
            Me.m_nameTb.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.m_nameTb.Location = New System.Drawing.Point(89, 3)
            Me.m_nameTb.Name = "m_nameTb"
            Me.m_nameTb.Size = New System.Drawing.Size(486, 20)
            Me.m_nameTb.TabIndex = 1
            '
            'Label1
            '
            Me.Label1.Anchor = System.Windows.Forms.AnchorStyles.None
            Me.Label1.AutoSize = True
            Me.Label1.Location = New System.Drawing.Point(3, 6)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(80, 13)
            Me.Label1.TabIndex = 0
            Me.Label1.Text = "Constant &Name"
            '
            'SelectConstantDialog
            '
            Me.AcceptButton = Me.m_okBtn
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.CancelButton = Me.m_cancelBtn
            Me.ClientSize = New System.Drawing.Size(665, 243)
            Me.Controls.Add(Me.TableLayoutPanel1)
            Me.Name = "SelectConstantDialog"
            Me.Text = "Select A Constant"
            Me.TableLayoutPanel1.ResumeLayout(False)
            Me.TableLayoutPanel1.PerformLayout()
            Me.FlowLayoutPanel1.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents FlowLayoutPanel1 As System.Windows.Forms.FlowLayoutPanel
        Friend WithEvents m_okBtn As System.Windows.Forms.Button
        Friend WithEvents m_cancelBtn As System.Windows.Forms.Button
        Friend WithEvents m_nameTb As System.Windows.Forms.TextBox
        Friend WithEvents Label1 As System.Windows.Forms.Label
    End Class
End Namespace
