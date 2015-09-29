' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Drawing
Imports System.Windows.Forms

Namespace Controls

    Public Class CodeBox

        Public ReadOnly Property RichTextBox() As RichTextBox
            Get
                Return m_box
            End Get
        End Property

        Public Property Code() As String
            Get
                Return m_box.Text
            End Get
            Set(ByVal value As String)
                m_box.Text = value
                m_box.ScrollToCaret()
            End Set
        End Property

        Sub New()

            ' This call is required by the Windows Form Designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Dim color As Color = m_box.BackColor
            m_box.ReadOnly = True
            m_box.BackColor = color
        End Sub

        Public Sub SelectAll()
            m_box.SelectAll()
        End Sub

        Public Sub Copy()
            m_box.Copy()
        End Sub

        Private Sub OnCopyClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CopyToolStripMenuItem.Click
            m_box.Copy()
        End Sub

        Private Sub OnSelectAllClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectAllToolStripMenuItem.Click
            m_box.SelectAll()
        End Sub
    End Class

End Namespace
