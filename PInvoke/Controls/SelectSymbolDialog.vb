' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.ComponentModel
Imports System.Windows.Forms
Imports PInvoke
Imports ConstantRow = PInvoke.NativeStorage.ConstantRow

Namespace Controls

    Public Class SelectSymbolDialog
        Private m_ns As NativeStorage
        Private m_type As SearchKind
        Private m_searchGrid As SearchDataGrid
        Private m_bag As NativeSymbolBag

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property NativeStorage() As NativeStorage
            Get
                Return m_ns
            End Get
            Set(ByVal value As NativeStorage)
                m_ns = value
                m_searchGrid.NativeStorage = value
            End Set
        End Property

        Public Property SearchKind() As SearchKind
            Get
                Return m_searchGrid.SearchKind
            End Get
            Set(ByVal value As SearchKind)
                m_searchGrid.SearchKind = value
                Select Case value
                    Case PInvoke.Controls.SearchKind.Constant
                        Me.Name = "Select a Constant"
                        Me.Label1.Text = "Constant Name"
                    Case PInvoke.Controls.SearchKind.Procedure
                        Me.Name = "Select a Procedure"
                        Me.Label1.Text = "Procedure Name"
                    Case PInvoke.Controls.SearchKind.Type
                        Me.Name = "Select a Type"
                        Me.Label1.Text = "Type Name"
                    Case PInvoke.Controls.SearchKind.All
                        Me.Name = "Select a Symbol"
                        Me.Label1.Text = "Name"
                    Case PInvoke.Controls.SearchKind.None
                        ' Do nothing
                End Select
            End Set
        End Property

        Public ReadOnly Property SelectedSymbolBag() As NativeSymbolBag
            Get
                Return m_bag
            End Get
        End Property

        Public Sub New()
            MyClass.New(SearchKind.Constant, NativeStorage.DefaultInstance)
        End Sub

        Public Sub New(ByVal kind As SearchKind, ByVal ns As NativeStorage)
            InitializeComponent()
            m_ns = ns
            m_searchGrid = New SearchDataGrid()
            m_searchGrid.Dock = DockStyle.Fill
            TableLayoutPanel1.Controls.Add(m_searchGrid, 1, 1)

            SearchKind = kind
        End Sub

        Private Sub OnNameChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_nameTb.TextChanged
            If m_searchGrid IsNot Nothing Then
                m_searchGrid.SearchText = m_nameTb.Text
            End If
        End Sub

        Private Sub m_okBtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_okBtn.Click
            Me.DialogResult = Windows.Forms.DialogResult.OK
            If m_searchGrid IsNot Nothing Then
                Me.m_bag = m_searchGrid.SelectedSymbolBag
            End If
            Me.Close()
        End Sub
    End Class

End Namespace
