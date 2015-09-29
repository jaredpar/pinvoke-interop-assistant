' Copyright (c) Microsoft Corporation.  All rights reserved.
Namespace Controls
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class CodeBox
        Inherits System.Windows.Forms.UserControl

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
            Me.components = New System.ComponentModel.Container
            Me.m_box = New System.Windows.Forms.RichTextBox
            Me.m_menuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.CopyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
            Me.SelectAllToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
            Me.m_menuStrip.SuspendLayout()
            Me.SuspendLayout()
            '
            'm_box
            '
            Me.m_box.ContextMenuStrip = Me.m_menuStrip
            Me.m_box.Dock = System.Windows.Forms.DockStyle.Fill
            Me.m_box.Location = New System.Drawing.Point(0, 0)
            Me.m_box.Name = "m_box"
            Me.m_box.Size = New System.Drawing.Size(436, 365)
            Me.m_box.TabIndex = 0
            Me.m_box.Text = ""
            '
            'm_menuStrip
            '
            Me.m_menuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CopyToolStripMenuItem, Me.SelectAllToolStripMenuItem})
            Me.m_menuStrip.Name = "m_menuStrip"
            Me.m_menuStrip.Size = New System.Drawing.Size(129, 48)
            '
            'CopyToolStripMenuItem
            '
            Me.CopyToolStripMenuItem.Name = "CopyToolStripMenuItem"
            Me.CopyToolStripMenuItem.Size = New System.Drawing.Size(128, 22)
            Me.CopyToolStripMenuItem.Text = "Copy"
            '
            'SelectAllToolStripMenuItem
            '
            Me.SelectAllToolStripMenuItem.Name = "SelectAllToolStripMenuItem"
            Me.SelectAllToolStripMenuItem.Size = New System.Drawing.Size(128, 22)
            Me.SelectAllToolStripMenuItem.Text = "Select All"
            '
            'CodeBox
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.m_box)
            Me.Name = "CodeBox"
            Me.Size = New System.Drawing.Size(436, 365)
            Me.m_menuStrip.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents m_box As System.Windows.Forms.RichTextBox
        Friend WithEvents m_menuStrip As System.Windows.Forms.ContextMenuStrip
        Friend WithEvents CopyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents SelectAllToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

    End Class
End Namespace
