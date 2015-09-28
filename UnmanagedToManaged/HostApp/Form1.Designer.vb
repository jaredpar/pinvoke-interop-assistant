' Copyright (c) Microsoft Corporation.  All rights reserved.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Me.SymbolDisplayControl1 = New PInvoke.Controls.SymbolDisplayControl
        Me.SuspendLayout()
        '
        'SymbolDisplayControl1
        '
        Me.SymbolDisplayControl1.AutoGenerate = False
        Me.SymbolDisplayControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SymbolDisplayControl1.Location = New System.Drawing.Point(0, 0)
        Me.SymbolDisplayControl1.Name = "SymbolDisplayControl1"
        Me.SymbolDisplayControl1.Size = New System.Drawing.Size(896, 544)
        Me.SymbolDisplayControl1.TabIndex = 0
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(896, 544)
        Me.Controls.Add(Me.SymbolDisplayControl1)
        Me.Name = "Form1"
        Me.Text = "PInvoke Generator"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents SymbolDisplayControl1 As PInvoke.Controls.SymbolDisplayControl

End Class
