' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.ComponentModel
Imports System.Windows.Forms
Imports PInvoke
Imports ConstantRow = PInvoke.NativeStorage.ConstantRow
Imports ProcedureRow = PInvoke.NativeStorage.ProcedureRow
Imports DefinedTypeRow = PInvoke.NativeStorage.DefinedTypeRow
Imports TypedefTypeRow = PInvoke.NativeStorage.TypedefTypeRow

Namespace Controls

#Region "SearchDataGrid"

    Public Enum SearchKind
        None
        Constant
        Procedure
        Type
        All
    End Enum

    Public Class SearchDataGrid
        Inherits DataGridView

        Private WithEvents m_timer As New Timer
        Private m_nameColumn As DataGridViewColumn
        Private m_valueColumn As DataGridViewColumn
        Private m_ns As NativeStorage
        Private m_search As IncrementalSearch
        Private m_searchText As String
        Private m_showInvalidData As Boolean
        Private m_list As New List(Of Object)
        Private m_kind As SearchKind
        Private m_info As SearchDataGridInfo
        Private m_handleCreated As Boolean
        Private m_selectionBag As NativeSymbolBag
        Private m_selectionList As New List(Of String)

        ''' <summary>
        ''' Have to do this to prevent the designer from serializing my columns
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Shadows ReadOnly Property Columns() As Object
            Get
                Return MyBase.Columns
            End Get
        End Property

        Public Property SearchText() As String
            Get
                Return m_searchText
            End Get
            Set(ByVal value As String)
                m_searchText = value
                StartSearch()
            End Set
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property NativeStorage() As NativeStorage
            Get
                Return m_ns
            End Get
            Set(ByVal value As NativeStorage)
                m_ns = value
                If m_info IsNot Nothing Then
                    m_info.NativeStorage = value
                    StartSearch()
                End If
            End Set
        End Property

        Public ReadOnly Property SelectedSymbols() As List(Of NativeSymbol)
            Get
                Dim list As New List(Of NativeSymbol)
                For Each row As DataGridViewRow In SelectedRows
                    Dim symbol As NativeSymbol = Nothing
                    If m_info.TryConvertToSymbol(m_list(row.Index), symbol) Then
                        list.Add(symbol)
                    End If
                Next

                Return list
            End Get
        End Property

        Public ReadOnly Property SelectedSymbolBag() As NativeSymbolBag
            Get
                ' Only calculate this once per selection because it can be very expensive
                If m_selectionBag IsNot Nothing Then
                    Return m_selectionBag
                End If

                Dim bag As New NativeSymbolBag(m_ns)
                For Each cur As NativeSymbol In SelectedSymbols
                    Try

                        If cur.Category = NativeSymbolCategory.Defined Then
                            bag.AddDefinedType(DirectCast(cur, NativeDefinedType))
                        Else
                            Select Case cur.Kind
                                Case NativeSymbolKind.Constant
                                    bag.AddConstant(DirectCast(cur, NativeConstant))
                                Case NativeSymbolKind.Procedure
                                    bag.AddProcedure(DirectCast(cur, NativeProcedure))
                                Case NativeSymbolKind.TypedefType
                                    bag.AddTypedef(DirectCast(cur, NativeTypeDef))
                                Case Else
                                    Contract.InvalidEnumValue(cur.Kind)
                            End Select
                        End If
                    Catch ex As ArgumentException
                        ' Can happen when a symbol is included in the list twice
                    End Try
                Next

                m_selectionBag = bag
                Return bag
            End Get
        End Property

        Public Property SearchKind() As SearchKind
            Get
                Return m_kind
            End Get
            Set(ByVal value As SearchKind)
                m_kind = value
                Select Case m_kind
                    Case PInvoke.Controls.SearchKind.Constant
                        m_info = New SearchDataGridInfo.ConstantsInfo()
                    Case PInvoke.Controls.SearchKind.Procedure
                        m_info = New SearchDataGridInfo.ProcedureInfo()
                    Case PInvoke.Controls.SearchKind.Type
                        m_info = New SearchDataGridInfo.TypeInfo()
                    Case PInvoke.Controls.SearchKind.None
                        m_info = New SearchDataGridInfo.EmptyInfo()
                    Case PInvoke.Controls.SearchKind.All
                        m_info = New SearchDataGridInfo.AllInfo()
                    Case Else
                        m_info = New SearchDataGridInfo.EmptyInfo
                        Contract.InvalidEnumValue(m_kind)
                End Select

                m_valueColumn.Name = m_info.ValueColumnName
                StartSearch()
            End Set
        End Property

        Public Property ShowInvalidData() As Boolean
            Get
                Return m_showInvalidData
            End Get
            Set(ByVal value As Boolean)
                If value <> m_showInvalidData Then
                    m_showInvalidData = value
                    StartSearch()
                End If
            End Set
        End Property

        <Category("Action")> _
        Public Event SelectedSymbolsChanged As EventHandler

        Public Sub New()
            m_ns = PInvoke.NativeStorage.DefaultInstance
            m_timer.Enabled = False
            m_timer.Interval = 500

            Me.VirtualMode = True
            Me.ReadOnly = True
            Me.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            Me.AllowUserToAddRows = False
            Me.AllowUserToDeleteRows = False
            Me.AllowUserToResizeRows = False
            Me.RowHeadersVisible = False

            Dim nameColumn As New DataGridViewTextBoxColumn()
            nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            nameColumn.HeaderText = "Name"
            nameColumn.ReadOnly = True
            nameColumn.Width = 180
            m_nameColumn = nameColumn

            Dim valueColumn As New DataGridViewTextBoxColumn()
            valueColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            valueColumn.HeaderText = "Value"
            valueColumn.ReadOnly = True
            valueColumn.MinimumWidth = 100
            m_valueColumn = valueColumn

            MyBase.Columns.AddRange(New DataGridViewColumn() {nameColumn, valueColumn})

            SearchKind = PInvoke.Controls.SearchKind.None
        End Sub

#Region "Overrides"

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)

            If disposing Then
                m_timer.Dispose()
            End If
        End Sub

        Protected Overrides Sub OnCellValueNeeded(ByVal e As System.Windows.Forms.DataGridViewCellValueEventArgs)
            MyBase.OnCellValueNeeded(e)
            If e.RowIndex >= m_list.Count Then
                Return
            End If

            Dim cur As Object = m_list(e.RowIndex)
            If e.ColumnIndex = m_nameColumn.Index Then
                e.Value = m_info.GetName(cur)
            ElseIf e.ColumnIndex = m_valueColumn.Index Then
                e.Value = m_info.GetValue(cur)
            Else
                Debug.Fail("Unexpected")
                e.Value = String.Empty
            End If
        End Sub

        Protected Overrides Sub OnSelectionChanged(ByVal e As System.EventArgs)
            MyBase.OnSelectionChanged(e)
            CheckForSelectedSymbolChange()
        End Sub

        Protected Overrides Sub OnCreateControl()
            MyBase.OnCreateControl()
            m_handleCreated = True
            StartSearch()
        End Sub

#End Region

#Region "Event Handlers"

        Private Sub OnTimerInterval(ByVal sender As Object, ByVal e As EventArgs) Handles m_timer.Tick
            SearchImpl()
        End Sub

#End Region

#Region "Helpers"

        Private Sub StartSearch()

            ' Don't start searching until we are actually displayed.  
            If Not m_handleCreated Then
                Return
            End If

            m_list.Clear()
            m_timer.Enabled = True
            RowCount = 0

            If m_search IsNot Nothing Then
                m_search.Cancel()
            End If

            m_info.ShowInvalidData = m_showInvalidData
            m_info.SearchText = SearchText
            m_info.NativeStorage = m_ns
            m_search = New IncrementalSearch(m_info.GetInitialData(), AddressOf m_info.ShouldAllow)

            SearchImpl()
        End Sub

        Private Sub SearchImpl()
            If m_search Is Nothing Then
                CheckForSelectedSymbolChange()
                m_timer.Enabled = False
                Return
            End If

            Dim result As Result = m_search.Search()
            If result.Completed Then
                m_search = Nothing
            End If

            m_list.AddRange(result.IncrementalFound)
            m_list.Sort(AddressOf SearchSort)
            RowCount = m_list.Count
            Refresh()
        End Sub

        ''' <summary>
        ''' Sort based on the following guidance.  For the examples assume we are searching for
        ''' "INT"
        ''' 
        '''  1) Starts with before not starts with.  "integral" before "aligned int"
        '''  2) Shorter matches over longer ones
        '''  3) Alphabetically for everything else
        ''' 
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SearchSort(ByVal left As Object, ByVal right As Object) As Integer
            Dim target As String = SearchText
            Dim leftName As String = m_info.GetName(left)
            Dim rightName As String = m_info.GetName(right)
            If Not String.IsNullOrEmpty(target) Then


                Dim leftStart As Boolean = leftName.StartsWith(target, StringComparison.OrdinalIgnoreCase)
                Dim rightStart As Boolean = rightName.StartsWith(target, StringComparison.OrdinalIgnoreCase)
                If leftStart <> rightStart Then
                    If leftStart Then
                        Return -1
                    Else
                        Return 1
                    End If
                End If

                If leftName.Length <> rightName.Length Then
                    Return leftName.Length - rightName.Length
                End If
            End If

            Return String.Compare(leftName, rightName, StringComparison.OrdinalIgnoreCase)
        End Function

        Private Function GetHashString(ByVal row As DataGridViewRow) As String
            Dim val As String
            Try
                val = TryCast(row.Cells(m_nameColumn.Index).Value, String)
            Catch ex As Exception
                val = String.Empty
            End Try

            ' Ensure that it's unique so that refreshes will be forced
            If String.IsNullOrEmpty(val) Then
                Debug.Fail("Error getting column name")
                val = Guid.NewGuid().ToString()
            End If

            Return val
        End Function

        Private Sub CheckForSelectedSymbolChange()
            Dim changed As Boolean = False
            If m_selectionList.Count <> Me.SelectedRows.Count Then
                changed = True
            Else
                For i As Integer = 0 To m_selectionList.Count - 1
                    If Not StringComparer.Ordinal.Equals(m_selectionList(i), GetHashString(SelectedRows(i))) Then
                        changed = True
                        Exit For
                    End If
                Next
            End If

            If changed Then
                m_selectionBag = Nothing    ' Eliminate the cache

                ' Cache the current selection
                m_selectionList.Clear()
                For Each row As DataGridViewRow In Me.SelectedRows
                    m_selectionList.Add(GetHashString(row))
                Next

                RaiseEvent SelectedSymbolsChanged(Me, EventArgs.Empty)
            End If
        End Sub
#End Region

    End Class

    Public MustInherit Class SearchDataGridInfo
        Public NativeStorage As NativeStorage = NativeStorage.DefaultInstance
        Public ShowInvalidData As Boolean
        Public SearchText As String

        Public Overridable ReadOnly Property ValueColumnName() As String
            Get
                Return "Value"
            End Get
        End Property

        Public Function ShouldAllow(ByVal cur As Object) As Boolean
            Return ShouldAllowCore(cur)
        End Function

        Public MustOverride Function GetInitialData() As IEnumerable
        Public MustOverride Function GetName(ByVal o As Object) As String
        Public MustOverride Function GetValue(ByVal o As Object) As String
        Public MustOverride Function TryConvertToSymbol(ByVal o As Object, ByRef symbol As NativeSymbol) As Boolean

        Protected Overridable Function ShouldAllowCore(ByVal cur As Object) As Boolean
            If String.IsNullOrEmpty(SearchText) Then
                Return True
            End If

            Dim name As String = GetName(cur)
            Return name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0
        End Function

#Region "EmptyInfo"

        Public Class EmptyInfo
            Inherits SearchDataGridInfo

            Public Overrides Function GetInitialData() As System.Collections.IEnumerable
                Return New List(Of Object)
            End Function

            Public Overrides Function GetName(ByVal o As Object) As String
                Return String.Empty
            End Function

            Public Overrides Function GetValue(ByVal o As Object) As String
                Return String.Empty
            End Function

            Public Overrides Function TryConvertToSymbol(ByVal o As Object, ByRef symbol As NativeSymbol) As Boolean
                Return False
            End Function
        End Class

#End Region

#Region "ConstantsInfo"
        Public Class ConstantsInfo
            Inherits SearchDataGridInfo

            Public Overrides Function GetInitialData() As System.Collections.IEnumerable
                Return NativeStorage.Constant
            End Function

            Public Overrides Function GetName(ByVal o As Object) As String
                Dim row As ConstantRow = DirectCast(o, ConstantRow)
                Return row.Name
            End Function

            Public Overrides Function GetValue(ByVal o As Object) As String
                Dim row As ConstantRow = DirectCast(o, ConstantRow)
                Dim c As NativeConstant = Nothing
                If NativeStorage.TryLoadConstant(row.Name, c) Then
                    Return c.Value.Expression
                Else
                    Return String.Empty
                End If

            End Function

            Public Overrides Function TryConvertToSymbol(ByVal o As Object, ByRef symbol As NativeSymbol) As Boolean
                Dim row As ConstantRow = DirectCast(o, ConstantRow)
                Dim cValue As NativeConstant = Nothing
                If NativeStorage.TryLoadConstant(row.Name, cValue) Then
                    symbol = cValue
                    Return True
                Else
                    Return False
                End If
            End Function

            Protected Overrides Function ShouldAllowCore(ByVal cur As Object) As Boolean
                If Not ShowInvalidData Then
                    Dim row As ConstantRow = DirectCast(cur, ConstantRow)
                    If Not IsValidConstant(row) Then
                        Return False
                    End If
                End If

                Return MyBase.ShouldAllowCore(cur)
            End Function

            Public Shared Function IsValidConstant(ByVal row As ConstantRow) As Boolean
                If row.Kind = ConstantKind.MacroMethod Then
                    Return False
                End If

                Dim p As New Parser.ExpressionParser
                Dim node As Parser.ExpressionNode = Nothing
                If Not p.TryParse(row.Value, node) Then
                    Return False
                End If

                Return Not ContainsInvalidKind(node)
            End Function

            Private Shared Function ContainsInvalidKind(ByVal node As Parser.ExpressionNode) As Boolean
                If node Is Nothing Then
                    Return False
                End If

                If node.Kind = Parser.ExpressionKind.Cast OrElse node.Kind = Parser.ExpressionKind.FunctionCall Then
                    Return True
                End If

                Return ContainsInvalidKind(node.LeftNode) OrElse ContainsInvalidKind(node.RightNode)
            End Function
        End Class

#End Region

#Region "ProcedureInfo"
        Public Class ProcedureInfo
            Inherits SearchDataGridInfo

            Public Overrides Function GetInitialData() As System.Collections.IEnumerable
                Return NativeStorage.Procedure.Rows
            End Function

            Public Overrides Function GetName(ByVal o As Object) As String
                Dim row As ProcedureRow = DirectCast(o, ProcedureRow)
                Return row.Name
            End Function

            Public Overrides Function GetValue(ByVal o As Object) As String
                Dim row As ProcedureRow = DirectCast(o, ProcedureRow)
                Dim proc As NativeProcedure = Nothing
                If NativeStorage.TryLoadProcedure(row.Name, proc) Then
                    Return proc.Signature.DisplayName
                End If

                Return String.Empty
            End Function

            Public Overrides Function TryConvertToSymbol(ByVal o As Object, ByRef symbol As NativeSymbol) As Boolean
                Dim row As ProcedureRow = DirectCast(o, ProcedureRow)
                Dim proc As NativeProcedure = Nothing
                If NativeStorage.TryLoadProcedure(row.Name, proc) Then
                    symbol = proc
                    Return True
                Else
                    Return False
                End If
            End Function
        End Class
#End Region

#Region "TypeInfo"
        Public Class TypeInfo
            Inherits SearchDataGridInfo

            Public Overrides Function GetInitialData() As System.Collections.IEnumerable
                Dim list As New ArrayList
                list.AddRange(NativeStorage.DefinedType.Rows)
                list.AddRange(NativeStorage.TypedefType.Rows)
                Return list
            End Function

            Public Overrides Function GetName(ByVal o As Object) As String
                Dim definedRow As DefinedTypeRow = TryCast(o, DefinedTypeRow)
                Dim typedefRow As TypedefTypeRow = TryCast(o, TypedefTypeRow)
                If definedRow IsNot Nothing Then
                    Return definedRow.Name
                ElseIf typedefRow IsNot Nothing Then
                    Return typedefRow.Name
                Else
                    Return String.Empty
                End If
            End Function

            Public Overrides Function GetValue(ByVal o As Object) As String
                Dim name As String = GetName(o)
                Dim type As NativeType = Nothing
                If NativeStorage.TryLoadByName(name, type) Then
                    Select Case type.Kind
                        Case NativeSymbolKind.StructType
                            Return "struct"
                        Case NativeSymbolKind.UnionType
                            Return "union"
                        Case NativeSymbolKind.EnumNameValue, NativeSymbolKind.EnumType
                            Return "enum"
                        Case NativeSymbolKind.FunctionPointer
                            Return "function pointer"
                        Case NativeSymbolKind.TypedefType
                            Return "typedef"
                    End Select
                End If

                Return String.Empty
            End Function

            Public Overrides Function TryConvertToSymbol(ByVal o As Object, ByRef symbol As NativeSymbol) As Boolean
                Dim name As String = GetName(o)
                Dim type As NativeType = Nothing
                If NativeStorage.TryLoadByName(name, type) Then
                    symbol = type
                    Return True
                End If

                Return False
            End Function
        End Class
#End Region

#Region "AllInfo"
        Public Class AllInfo
            Inherits SearchDataGridInfo

            Public Overrides Function GetInitialData() As System.Collections.IEnumerable
                Dim list As New ArrayList
                list.AddRange(NativeStorage.Constant.Rows)
                list.AddRange(NativeStorage.Procedure.Rows)
                list.AddRange(NativeStorage.DefinedType.Rows)
                list.AddRange(NativeStorage.TypedefType.Rows)
                Return list
            End Function

            Public Overrides Function GetName(ByVal o As Object) As String
                Dim procRow As ProcedureRow = TryCast(o, ProcedureRow)
                Dim constRow As ConstantRow = TryCast(o, ConstantRow)
                Dim definedRow As DefinedTypeRow = TryCast(o, DefinedTypeRow)
                Dim typedefRow As TypedefTypeRow = TryCast(o, TypedefTypeRow)
                If definedRow IsNot Nothing Then
                    Return definedRow.Name
                ElseIf typedefRow IsNot Nothing Then
                    Return typedefRow.Name
                ElseIf constRow IsNot Nothing Then
                    Return constRow.Name
                ElseIf procRow IsNot Nothing Then
                    Return procRow.Name
                Else
                    Return String.Empty
                End If
            End Function

            Public Overrides Function GetValue(ByVal o As Object) As String
                Dim symbol As NativeSymbol = Nothing
                Dim value As String = Nothing
                If TryGetSymbolAndValue(o, symbol, value) Then
                    Return value
                Else
                    Return String.Empty
                End If
            End Function

            Public Overrides Function TryConvertToSymbol(ByVal o As Object, ByRef symbol As NativeSymbol) As Boolean
                Dim value As String = Nothing
                Return TryGetSymbolAndValue(o, symbol, value)
            End Function

            Private Function TryGetSymbolAndValue(ByVal o As Object, ByRef symbol As NativeSymbol, ByRef value As String) As Boolean
                Dim procRow As ProcedureRow = TryCast(o, ProcedureRow)
                Dim constRow As ConstantRow = TryCast(o, ConstantRow)
                Dim definedRow As DefinedTypeRow = TryCast(o, DefinedTypeRow)
                Dim typedefRow As TypedefTypeRow = TryCast(o, TypedefTypeRow)
                Dim name As String = GetName(o)

                Dim ret As Boolean = True
                If definedRow IsNot Nothing OrElse typedefRow IsNot Nothing Then
                    Dim type As NativeType = Nothing
                    If NativeStorage.TryLoadByName(name, type) Then
                        symbol = type
                        Select Case type.Kind
                            Case NativeSymbolKind.StructType
                                value = "struct"
                            Case NativeSymbolKind.UnionType
                                value = "union"
                            Case NativeSymbolKind.EnumType
                                value = "enum"
                            Case NativeSymbolKind.EnumNameValue
                                value = "enum"
                            Case NativeSymbolKind.FunctionPointer
                                value = "function pointer"
                            Case NativeSymbolKind.TypedefType
                                value = "typedef"
                            Case Else
                                ret = False
                        End Select
                    Else
                        ret = False
                    End If
                ElseIf constRow IsNot Nothing Then
                    Dim cValue As NativeConstant = Nothing
                    If NativeStorage.TryLoadConstant(name, cValue) Then
                        symbol = cValue
                        value = cValue.Value.Expression
                    Else
                        ret = False
                    End If
                ElseIf procRow IsNot Nothing Then
                    Dim proc As NativeProcedure = Nothing
                    If NativeStorage.TryLoadProcedure(name, proc) Then
                        symbol = proc
                        value = proc.Signature.DisplayName
                    Else
                        ret = False
                    End If
                Else
                    ret = False
                End If

                Debug.Assert(ret, "Please file a bug: " & name)
                Return ret
            End Function

            Protected Overrides Function ShouldAllowCore(ByVal cur As Object) As Boolean
                If Not ShowInvalidData Then
                    Dim constRow As ConstantRow = TryCast(cur, ConstantRow)
                    If constRow IsNot Nothing AndAlso Not ConstantsInfo.IsValidConstant(constRow) Then
                        Return False
                    End If
                End If

                Return MyBase.ShouldAllowCore(cur)
            End Function
        End Class

#End Region

    End Class


#End Region

End Namespace
