' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.ComponentModel
Imports System.Windows.Forms
Imports PInvoke
Imports ConstantRow = PInvoke.NativeStorage.ConstantRow

Namespace Controls

    Public Class SelectSymbolDialog
        Private _ns As NativeStorage
        Private _type As SearchKind
        Private _searchGrid As SearchDataGrid
        Private _bag As NativeSymbolBag

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property NativeStorage() As NativeStorage
            Get
                Return _ns
            End Get
            Set(ByVal value As NativeStorage)
                _ns = value
                _searchGrid.NativeStorage = value
            End Set
        End Property

        Public Property SearchKind() As SearchKind
            Get
                Return _searchGrid.SearchKind
            End Get
            Set(ByVal value As SearchKind)
                _searchGrid.SearchKind = value
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
                Return _bag
            End Get
        End Property

        Public Sub New()
            MyClass.New(SearchKind.Constant, NativeStorage.DefaultInstance)
        End Sub

        Public Sub New(ByVal kind As SearchKind, ByVal ns As NativeStorage)
            InitializeComponent()
            _ns = ns
            _searchGrid = New SearchDataGrid()
            _searchGrid.Dock = DockStyle.Fill
            TableLayoutPanel1.Controls.Add(_searchGrid, 1, 1)

            SearchKind = kind
        End Sub

        Private Sub OnNameChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_nameTb.TextChanged
            If _searchGrid IsNot Nothing Then
                _searchGrid.SearchText = m_nameTb.Text
            End If
        End Sub

        Private Sub m_okBtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles m_okBtn.Click
            Me.DialogResult = Windows.Forms.DialogResult.OK
            If _searchGrid IsNot Nothing Then
                Me._bag = _searchGrid.SelectedSymbolBag
            End If
            Me.Close()
        End Sub
    End Class

End Namespace
