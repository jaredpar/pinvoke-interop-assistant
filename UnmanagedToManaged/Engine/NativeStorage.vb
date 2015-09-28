' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Threading
Imports System.Text
Imports Microsoft.Win32
Imports PInvoke.Contract

Partial Class NativeStorage

    <DebuggerDisplay("Id={Id} Kind={Kind}")> _
    Public Class TypeReference
        Public Id As Integer
        Public Kind As NativeSymbolKind

        Public Sub New(ByVal id As Integer, ByVal kind As NativeSymbolKind)
            Me.Id = id
            Me.Kind = kind
        End Sub
    End Class

    <ThreadStatic()> _
    Private Shared m_default As NativeStorage

    ''' <summary>
    ''' Default Instance to use if not explicitly given one
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Property DefaultInstance() As NativeStorage
        Get
            If m_default Is Nothing Then
                m_default = New NativeStorage()
            End If
            Return m_default
        End Get
        Set(ByVal value As NativeStorage)
            m_default = value
        End Set
    End Property

    Partial Class DefinedTypeDataTable
        Private Sub DefinedTypeDataTable_ColumnChanging(ByVal sender As System.Object, ByVal e As System.Data.DataColumnChangeEventArgs) Handles Me.ColumnChanging
            If (e.Column.ColumnName = Me.IdColumn.ColumnName) Then
                'Add user code here
            End If

        End Sub
    End Class

#Region "DefinedType Table"

    Partial Class DefinedTypeDataTable

        Private m_cacheMap As Dictionary(Of String, DefinedTypeRow)

        Public Property CacheLookup() As Boolean
            Get
                Return m_cacheMap IsNot Nothing
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    If m_cacheMap Is Nothing Then

                        m_cacheMap = New Dictionary(Of String, DefinedTypeRow)(StringComparer.Ordinal)
                        For Each row As DefinedTypeRow In Me.Rows
                            m_cacheMap(row.Name) = row
                        Next
                    End If
                Else
                    m_cacheMap = Nothing
                End If
            End Set
        End Property


        Public Function Add(ByVal kind As NativeSymbolKind, ByVal name As String) As DefinedTypeRow
            Dim row As DefinedTypeRow = Me.NewDefinedTypeRow()
            row.Kind = kind
            row.Name = name
            Me.AddDefinedTypeRow(row)

            If m_cacheMap IsNot Nothing Then
                m_cacheMap(name) = row
            End If

            Return row
        End Function

        ''' <summary>
        ''' Try and find a defined type by it's name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="drow"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TryFindByName(ByVal name As String, ByRef dRow As DefinedTypeRow) As Boolean

            ' Use the map if we are caching lookups
            If m_cacheMap IsNot Nothing Then
                Return m_cacheMap.TryGetValue(name, dRow)
            End If

            dRow = Nothing
            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}='{1}'", NameColumn.ColumnName, name))
            If rows.Length = 0 Then
                Return False
            End If

            dRow = DirectCast(rows(0), DefinedTypeRow)
            Return True
        End Function

        Public Function TryFindById(ByVal id As Integer, ByRef drow As DefinedTypeRow) As Boolean
            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}={1}", IdColumn.ColumnName, id))
            If rows.Length = 0 Then
                drow = Nothing
                Return False
            End If

            drow = DirectCast(rows(0), DefinedTypeRow)
            Return True
        End Function

        Public Function FindByNamePattern(ByVal pattern As String) As List(Of DefinedTypeRow)
            Dim list As New List(Of DefinedTypeRow)
            Dim filter As String = String.Format( _
                "{0} LIKE '{1}'", _
                NameColumn.ColumnName, _
                pattern)
            Dim rows() As DataRow = Me.Select(filter)
            For Each dtRow As DefinedTypeRow In rows
                list.Add(dtRow)
            Next

            Return list
        End Function

    End Class

    Partial Class DefinedTypeRow
        Public Property Kind() As NativeSymbolKind
            Get
                Return CType(Me.KindRaw, NativeSymbolKind)
            End Get
            Set(ByVal value As NativeSymbolKind)
                KindRaw = CType(value, Integer)
            End Set
        End Property

        Public Property CallingConvention() As NativeCallingConvention
            Get
                Return CType(Me.ConventionRaw, NativeCallingConvention)
            End Get
            Set(ByVal value As NativeCallingConvention)
                Me.ConventionRaw = CType(value, Int32)
            End Set
        End Property
    End Class

#End Region

#Region "Member Table"
    Partial Class MemberDataTable

        Friend Function Add(ByVal dtRow As DefinedTypeRow, ByVal name As String, ByVal typeRef As TypeReference) As MemberRow
            Dim row As MemberRow = Me.NewMemberRow()
            row.DefinedTypeRow = dtRow
            row.Name = name
            row.TypeId = typeRef.Id
            row.TypeKind = typeRef.Kind
            Me.AddMemberRow(row)

            Return row
        End Function

        Public Function TryFindById(ByVal id As Integer, ByRef erows As List(Of MemberRow)) As Boolean

            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}={1}", Me.DefinedTypeIdColumn.ColumnName, id))
            If rows.Length = 0 Then
                erows = Nothing
                Return False
            End If

            erows = New List(Of MemberRow)
            For Each row As DataRow In rows
                erows.Add(DirectCast(row, MemberRow))
            Next
            Return True
        End Function
    End Class

    Partial Class MemberRow
        Public Property TypeKind() As NativeSymbolKind
            Get
                Return CType(TypeKindRaw, NativeSymbolKind)
            End Get
            Set(ByVal value As NativeSymbolKind)
                TypeKindRaw = CType(value, Integer)
            End Set
        End Property
    End Class

#End Region

#Region "EnumValue Table"

    Partial Class EnumValueDataTable

        Public Function Add(ByVal dtRow As DefinedTypeRow, ByVal name As String, ByVal value As String) As EnumValueRow
            Dim row As EnumValueRow = Me.NewEnumValueRow()
            row.DefinedTypeRow = dtRow
            row.Name = name
            row.Value = value
            Me.AddEnumValueRow(row)
            Return row
        End Function

        Public Function TryFindById(ByVal id As Integer, ByRef erows As List(Of EnumValueRow)) As Boolean

            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}={1}", DefinedTypeIdColumn.ColumnName, id))
            If rows.Length = 0 Then
                erows = Nothing
                Return False
            End If

            erows = New List(Of EnumValueRow)
            For Each row As DataRow In rows
                erows.Add(DirectCast(row, EnumValueRow))
            Next
            Return True
        End Function

        Public Function TryFindByValueName(ByVal valName As String, ByRef erows As List(Of EnumValueRow)) As Boolean
            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}='{1}'", NameColumn.ColumnName, valName))
            If rows.Length = 0 Then
                erows = Nothing
                Return False
            End If

            erows = New List(Of EnumValueRow)
            For Each row As DataRow In rows
                erows.Add(DirectCast(row, EnumValueRow))
            Next
            Return True
        End Function

    End Class

#End Region

#Region "TypedefType Table"

    Partial Class TypedefTypeDataTable

        Private m_cacheMap As Dictionary(Of String, TypedefTypeRow)

        Public Property CacheLookup() As Boolean
            Get
                Return m_cacheMap IsNot Nothing
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    If m_cacheMap IsNot Nothing Then
                        m_cacheMap = New Dictionary(Of String, TypedefTypeRow)(StringComparer.Ordinal)
                        For Each cur As TypedefTypeRow In Rows
                            m_cacheMap(cur.Name) = cur
                        Next
                    End If
                Else
                    m_cacheMap = Nothing
                End If
            End Set
        End Property

        Public Function Add(ByVal name As String, ByVal typeRef As TypeReference) As TypedefTypeRow
            Dim row As TypedefTypeRow = Me.NewTypedefTypeRow()
            row.Name = name
            row.RealTypeId = typeRef.Id
            row.RealTypeKind = typeRef.Kind
            Me.AddTypedefTypeRow(row)

            If m_cacheMap IsNot Nothing Then
                m_cacheMap(name) = row
            End If

            Return row
        End Function

        Public Function TryFindByName(ByVal name As String, ByRef row As TypedefTypeRow) As Boolean
            If m_cacheMap IsNot Nothing Then
                Return m_cacheMap.TryGetValue(name, row)
            End If

            Dim filter As String = String.Format( _
                "{0}='{1}'", _
                NameColumn.ColumnName, _
                name)
            Dim rows() As DataRow = Me.Select(filter)
            If rows.Length = 0 Then
                Return False
            End If

            row = DirectCast(rows(0), TypedefTypeRow)
            Return True
        End Function

        Public Function FindByNamePattern(ByVal pattern As String) As List(Of TypedefTypeRow)
            Dim list As New List(Of TypedefTypeRow)
            Dim filter As String = String.Format( _
                "{0} LIKE '{1}'", _
                NameColumn.ColumnName, _
                pattern)
            Dim rows() As DataRow = Me.Select(filter)
            For Each dtRow As TypedefTypeRow In rows
                list.Add(dtRow)
            Next

            Return list
        End Function

        Public Function TryFindById(ByVal id As Integer, ByRef drow As TypedefTypeRow) As Boolean
            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}={1}", IdColumn.ColumnName, id))
            If rows.Length = 0 Then
                drow = Nothing
                Return False
            End If

            drow = DirectCast(rows(0), TypedefTypeRow)
            Return True
        End Function

        Public Function FindByTarget(ByVal typeRef As TypeReference) As List(Of TypedefTypeRow)
            Dim list As New List(Of TypedefTypeRow)
            Dim filter As String = String.Format( _
                "{0}={1} AND {2}={3}", _
                RealTypeIdColumn.ColumnName, typeRef.Id, _
                RealTypeKindRawColumn, CInt(typeRef.Kind))
            For Each trow As TypedefTypeRow In Me.Select(filter)
                list.Add(trow)
            Next

            Return list
        End Function
    End Class

    Partial Class TypedefTypeRow
        Public Property RealTypeKind() As NativeSymbolKind
            Get
                Return CType(RealTypeKindRaw, NativeSymbolKind)
            End Get
            Set(ByVal value As NativeSymbolKind)
                RealTypeKindRaw = CType(value, Integer)
            End Set
        End Property
    End Class
#End Region

#Region "NamedType Table"

    Partial Class NamedTypeDataTable
        Private m_cacheMap As Dictionary(Of String, NamedTypeRow)

        Private Shared Function CreateMoniker(ByVal qual As String, ByVal name As String, ByVal isConst As Boolean) As String
            Return qual & "#" & name & "#" & isConst.ToString()
        End Function

        Public Property CacheLookup() As Boolean
            Get
                Return m_cacheMap IsNot Nothing
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    If m_cacheMap Is Nothing Then
                        m_cacheMap = New Dictionary(Of String, NamedTypeRow)
                        For Each row As NamedTypeRow In Rows
                            m_cacheMap.Add(CreateMoniker(row.Qualification, row.Name, row.IsConst), row)
                        Next
                    End If
                Else
                    m_cacheMap = Nothing
                End If
            End Set
        End Property


        Public Function Add(ByVal qual As String, ByVal name As String, ByVal isConst As Boolean) As NamedTypeRow

            Dim row As NamedTypeRow = Me.NewNamedTypeRow()
            row.Name = name
            row.Qualification = qual
            row.IsConst = isConst
            Me.AddNamedTypeRow(row)

            If m_cacheMap IsNot Nothing Then
                m_cacheMap.Add(CreateMoniker(qual, name, isConst), row)
            End If

            Return row
        End Function

        Public Function TryFindByName(ByVal qual As String, ByVal name As String, ByVal isConst As Boolean, ByRef row As NamedTypeRow) As Boolean
            If m_cacheMap IsNot Nothing Then
                Dim moniker As String = CreateMoniker(qual, name, isConst)
                Return m_cacheMap.TryGetValue(moniker, row)
            End If

            Dim rows() As DataRow
            Dim filter As String = String.Format( _
                "{0}='{1}' And {2}='{3}' AND {4}={5}", _
                NameColumn.ColumnName, _
                name, _
                QualificationColumn.ColumnName, _
                qual, _
                IsConstColumn.ColumnName, _
                isConst)
            rows = Me.Select(filter)

            If rows.Length = 0 Then
                Return False
            End If

            row = DirectCast(rows(0), NamedTypeRow)
            Return True
        End Function

        Public Function TryFindById(ByVal id As Integer, ByRef drow As NamedTypeRow) As Boolean
            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}={1}", IdColumn.ColumnName, id))
            If rows.Length = 0 Then
                drow = Nothing
                Return False
            End If

            drow = DirectCast(rows(0), NamedTypeRow)
            Return True
        End Function
    End Class

#End Region

#Region "PointerType Table"

    Partial Class PointerTypeDataTable

        Public Function Add(ByVal typeRef As TypeReference) As PointerTypeRow
            ThrowIfNull(typeRef)

            Dim row As PointerTypeRow = Me.NewPointerTypeRow()
            row.RealTypeId = typeRef.Id
            row.RealTypeKind = typeRef.Kind
            Me.AddPointerTypeRow(row)
            Return row
        End Function

        Public Function TryFindById(ByVal id As Integer, ByRef row As PointerTypeRow) As Boolean
            Dim filter As String = String.Format( _
                "{0}={1}", _
                IdColumn.ColumnName, id)
            Dim rows() As DataRow = Me.Select(filter)
            If rows.Length = 0 Then
                Return False
            End If

            row = DirectCast(rows(0), PointerTypeRow)
            Return True
        End Function

        Public Function TryFindByTarget(ByVal typeRef As TypeReference, ByRef row As PointerTypeRow) As Boolean
            Dim filter As String = String.Format( _
                           "{0}={1} AND {2}={3}", _
                           RealTypeIdColumn.ColumnName, typeRef.Id, _
                           RealTypeKindRawColumn.ColumnName, CInt(typeRef.Kind))
            Dim rows() As DataRow = Me.Select(filter)
            If rows.Length = 0 Then
                Return False
            End If

            row = DirectCast(rows(0), PointerTypeRow)
            Return True
        End Function

    End Class

    Partial Class PointerTypeRow
        Public Property RealTypeKind() As NativeSymbolKind
            Get
                Return CType(RealTypeKindRaw, NativeSymbolKind)
            End Get
            Set(ByVal value As NativeSymbolKind)
                RealTypeKindRaw = CType(value, Integer)
            End Set
        End Property
    End Class

#End Region

#Region "ArrayType Table"

    Partial Class ArrayTypeDataTable

        Public Function Add(ByVal count As Integer, ByVal typeRef As TypeReference) As ArrayTypeRow
            ThrowIfNull(typeRef)

            Dim row As ArrayTypeRow = Me.NewArrayTypeRow()
            row.RealTypeId = typeRef.Id
            row.RealTypeKind = typeRef.Kind
            row.ElementCountt = count
            Me.AddArrayTypeRow(row)
            Return row
        End Function

        Public Function TryFindById(ByVal id As Integer, ByRef row As ArrayTypeRow) As Boolean
            Dim filter As String = String.Format( _
                "{0}={1}", _
                IdColumn.ColumnName, id)
            Dim rows() As DataRow = Me.Select(filter)
            If rows.Length = 0 Then
                Return False
            End If

            row = DirectCast(rows(0), ArrayTypeRow)
            Return True
        End Function

        Public Function TryFindByTarget(ByVal typeRef As TypeReference, ByRef row As ArrayTypeRow) As Boolean
            Dim filter As String = String.Format( _
                           "{0}={1} AND {2}={3}", _
                           RealTypeIdColumn.ColumnName, typeRef.Id, _
                           RealTypeKindRawColumn.ColumnName, CInt(typeRef.Kind))
            Dim rows() As DataRow = Me.Select(filter)
            If rows.Length = 0 Then
                Return False
            End If

            row = DirectCast(rows(0), ArrayTypeRow)
            Return True
        End Function
    End Class

    Partial Class ArrayTypeRow
        Public Property RealTypeKind() As NativeSymbolKind
            Get
                Return CType(RealTypeKindRaw, NativeSymbolKind)
            End Get
            Set(ByVal value As NativeSymbolKind)
                RealTypeKindRaw = CType(value, Integer)
            End Set
        End Property
    End Class

#End Region

#Region "Specialized Table"
    Partial Class SpecializedTypeDataTable

        Public Function TryFindById(ByVal id As Integer, ByRef srow As SpecializedTypeRow) As Boolean
            srow = Nothing
            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}={1}", Me.IdColumn.ColumnName, id))
            If rows.Length = 0 Then
                Return False
            Else
                srow = DirectCast(rows(0), SpecializedTypeRow)
                Return True
            End If
        End Function

        Public Function TryFindBuiltin(ByVal bt As BuiltinType, ByVal isUnsigned As Boolean, ByRef srow As SpecializedTypeRow) As Boolean
            Dim rows() As DataRow = Me.Select( _
                String.Format("{0}={1} AND {2}={3}", _
                    Me.BuiltinTypeRawColumn, CInt(bt), _
                    Me.IsUnsignedColumn.ColumnName, isUnsigned))
            If rows.Length = 0 Then
                srow = Nothing
                Return False
            Else
                srow = DirectCast(rows(0), SpecializedTypeRow)
                Return True
            End If
        End Function

        Public Function TryFindBitVector(ByVal size As Integer, ByRef srow As SpecializedTypeRow) As Boolean
            Dim rows() As DataRow = Me.Select( _
                           String.Format("{0}={1}", _
                                Me.BitVectorSizeColumn, _
                                size))
            If rows.Length = 0 Then
                srow = Nothing
                Return False
            Else
                srow = DirectCast(rows(0), SpecializedTypeRow)
                Return True
            End If
        End Function
    End Class

    Partial Class SpecializedTypeRow
        Public Property Kind() As NativeSymbolKind
            Get
                Return CType(KindRaw, NativeSymbolKind)
            End Get
            Set(ByVal value As NativeSymbolKind)
                KindRaw = CType(value, Integer)
            End Set
        End Property

        Public Property BuiltinType() As BuiltinType
            Get
                Return CType(BuiltinTypeRaw, BuiltinType)
            End Get
            Set(ByVal value As BuiltinType)
                BuiltinTypeRaw = CType(value, Integer)
            End Set
        End Property
    End Class

#End Region

#Region "Constants Table"

    Partial Class ConstantDataTable

        Public Function FindByNamePattern(ByVal pattern As String) As List(Of ConstantRow)
            Dim list As New List(Of ConstantRow)
            Dim filter As String = String.Format( _
                "{0} LIKE '{1}'", _
                NameColumn.ColumnName, _
                pattern)
            Dim rows() As DataRow = Me.Select(filter)
            For Each dtRow As ConstantRow In rows
                list.Add(dtRow)
            Next

            Return list
        End Function

        Public Function TryFindByName(ByVal name As String, ByRef row As ConstantRow) As Boolean
            Dim list As New List(Of ConstantRow)
            Dim filter As String = String.Format( _
                "{0}='{1}'", _
                NameColumn.ColumnName, _
                name)
            Dim rows() As DataRow = Me.Select(filter)
            If rows.Length = 0 Then
                Return False
            End If

            row = DirectCast(rows(0), ConstantRow)
            Return True
        End Function

        ''' <summary>
        ''' Convert all of the stored constants back into Macro instances
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function LoadAllMacros() As List(Of Parser.Macro)
            Dim list As New List(Of Parser.Macro)
            For Each nRow As ConstantRow In Me.Rows
                Select Case nRow.Kind
                    Case ConstantKind.MacroMethod
                        Dim method As Parser.MethodMacro = Nothing
                        If Parser.MethodMacro.TryCreateFromDeclaration(nRow.Name, nRow.Value, method) Then
                            list.Add(method)
                        End If
                    Case ConstantKind.Macro
                        list.Add(New Parser.Macro(nRow.Name, nRow.Value))
                    Case Else
                        InvalidEnumValue(nRow.Kind)
                End Select
            Next

            Return list
        End Function

    End Class

    Partial Class ConstantRow
        Public Property Kind() As ConstantKind
            Get
                Return CType(KindRaw, ConstantKind)
            End Get
            Set(ByVal value As ConstantKind)
                KindRaw = CType(value, Int32)
            End Set
        End Property
    End Class

#End Region

#Region "Procedure Table"
    Partial Class ProcedureDataTable

        Public Function Add(ByVal name As String, ByVal dllName As String, ByVal conv As NativeCallingConvention, ByVal sigId As Integer) As ProcedureRow
            Dim row As ProcedureRow = Me.NewProcedureRow()
            row.Name = name
            row.DllName = dllName
            row.CallingConvention = conv
            row.SignatureId = sigId
            Me.AddProcedureRow(row)
            Return row
        End Function

        Public Function TryLoadByName(ByVal name As String, ByRef procRow As ProcedureRow) As Boolean
            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}='{1}'", Me.NameColumn.ColumnName, name))
            If rows.Length = 0 Then
                procRow = Nothing
                Return False
            Else
                procRow = DirectCast(rows(0), ProcedureRow)
                Return True
            End If

        End Function

        Public Function FindByNamePattern(ByVal pattern As String) As List(Of ProcedureRow)
            Dim list As New List(Of ProcedureRow)
            Dim filter As String = String.Format( _
                "{0} LIKE '{1}'", _
                NameColumn.ColumnName, _
                pattern)
            Dim rows() As DataRow = Me.Select(filter)
            For Each dtRow As ProcedureRow In rows
                list.Add(dtRow)
            Next

            Return list
        End Function

    End Class

    Partial Class ProcedureRow
        Public Property CallingConvention() As NativeCallingConvention
            Get
                Return CType(ConventionRaw, NativeCallingConvention)
            End Get
            Set(ByVal value As NativeCallingConvention)
                ConventionRaw = CType(value, Int32)
            End Set
        End Property
    End Class
#End Region

#Region "Signature Table"
    Partial Class SignatureDataTable

        Public Function TryLoadById(ByVal id As Int32, ByRef sigRow As SignatureRow) As Boolean
            Dim rows As DataRow() = Me.Select( _
                String.Format("{0}={1}", Me.IdColumn.ColumnName, id))
            If rows.Length = 0 Then
                sigRow = Nothing
                Return False
            End If

            sigRow = DirectCast(rows(0), SignatureRow)
            Return True
        End Function
    End Class

    Partial Class SignatureRow
        Public Property ReturnTypeKind() As NativeSymbolKind
            Get
                Return CType(ReturnTypeKindRaw, NativeSymbolKind)
            End Get
            Set(ByVal value As NativeSymbolKind)
                ReturnTypeKindRaw = CType(value, Integer)
            End Set
        End Property
    End Class
#End Region

#Region "Parameter Table"
    Partial Class ParameterDataTable
        Public Function Add(ByVal sig As SignatureRow, ByVal name As String, ByVal typeRef As TypeReference, ByVal salId As String) As ParameterRow
            Dim row As ParameterRow = Me.NewParameterRow()
            row.Name = name
            row.SignatureId = sig.Id
            row.TypeId = typeRef.Id
            row.TypeKind = typeRef.Kind
            row.SalId = salId
            Me.AddParameterRow(row)
            Return row
        End Function
    End Class

    Partial Class ParameterRow
        Public Property TypeKind() As NativeSymbolKind
            Get
                Return CType(TypeKindRaw, NativeSymbolKind)
            End Get
            Set(ByVal value As NativeSymbolKind)
                TypeKindRaw = CType(value, Integer)
            End Set
        End Property
    End Class
#End Region

#Region "SalEntry Table"

    Partial Class SalEntryDataTable

        Public Function Add(ByVal type As SalEntryType, ByVal text As String) As SalEntryRow
            Dim row As SalEntryRow = Me.NewSalEntryRow()
            row.Type = type
            row.Text = text
            Me.AddSalEntryRow(row)
            Return row
        End Function

        Public Function TryFindNoText(ByVal type As SalEntryType, ByRef row As SalEntryRow) As Boolean
            Dim filter As String = String.Format( _
                "{0}={1}", _
                TypeRawColumn.ColumnName, CInt(type))
            Dim rows() As DataRow = Me.Select(filter)
            For Each cur As SalEntryRow In rows
                If cur.IsTextNull() Then
                    row = cur
                    Return True
                End If
            Next

            Return False
        End Function

        Public Function TryFind(ByVal type As SalEntryType, ByVal text As String, ByRef row As SalEntryRow) As Boolean
            Dim filter As String = String.Format( _
                "{0}={1} AND {2}='{3}'", _
                TypeRawColumn.ColumnName, CInt(type), _
                TextColumn.ColumnName, text)
            Dim rows() As DataRow = Me.Select(filter)
            If rows.Length = 0 Then
                Return False
            End If

            row = DirectCast(rows(0), SalEntryRow)
            Return True
        End Function

        Public Function TryFindById(ByVal id As Integer, ByRef row As SalEntryRow) As Boolean
            Dim filter As String = String.Format( _
                "{0}={1}", _
                IdColumn.ColumnName, _
                id)
            Dim rows() As DataRow = Me.Select(filter)
            If rows.Length = 0 Then
                Return False
            End If

            row = DirectCast(rows(0), SalEntryRow)
            Return True
        End Function

    End Class

    Partial Class SalEntryRow
        Public Property Type() As SalEntryType
            Get
                Return CType(TypeRaw, SalEntryType)
            End Get
            Set(ByVal value As SalEntryType)
                TypeRaw = CType(value, Integer)
            End Set
        End Property
    End Class

#End Region

    Public Property CacheLookup() As Boolean
        Get
            Return DefinedType.CacheLookup
        End Get
        Set(ByVal value As Boolean)
            DefinedType.CacheLookup = value
            TypedefType.CacheLookup = value
            NamedType.CacheLookup = value
        End Set
    End Property

    Public Sub AddConstant(ByVal nConst As NativeConstant)
        If nConst Is Nothing Then : Throw New ArgumentNullException("nConst") : End If

        Dim constRow As ConstantRow = Constant.NewConstantRow()
        constRow.Name = nConst.Name
        constRow.Kind = nConst.ConstantKind

        Select Case nConst.ConstantKind
            Case ConstantKind.MacroMethod
                ' Macro Methods wrap the value in "" so that it will be a valid expression.  Strip them
                ' here
                constRow.Value = nConst.Value.Expression.Substring(1, nConst.Value.Expression.Length - 2)
            Case ConstantKind.Macro
                ' Save the value
                constRow.Value = nConst.Value.Expression
            Case Else
                InvalidEnumValue(nConst.ConstantKind)
        End Select

        Constant.AddConstantRow(constRow)
    End Sub

    ''' <summary>
    ''' Add a defined type to the table
    ''' </summary>
    ''' <param name="nt"></param>
    ''' <remarks></remarks>
    Public Sub AddDefinedType(ByVal nt As NativeDefinedType)
        If nt Is Nothing Then : Throw New ArgumentNullException("nt") : End If

        ' Add the core type information first.  That we when doing recursive member
        ' adds, we can query the table to see if a type has already been added
        Dim dtRow As DefinedTypeRow = Me.DefinedType.Add(nt.Kind, nt.Name)

        ' Add the members
        For Each member As NativeMember In nt.Members
            Dim typeRef As TypeReference = CreateTypeReference(member.NativeType)
            Me.Member.Add(dtRow, member.Name, typeRef)
        Next

        If nt.Kind = NativeSymbolKind.EnumType Then
            ' If this is an enum then add it to the list
            Dim ntEnum As NativeEnum = DirectCast(nt, NativeEnum)
            For Each enumVal As NativeEnumValue In ntEnum.Values
                Me.EnumValue.Add(dtRow, enumVal.Name, enumVal.Value.Expression)
            Next
        ElseIf nt.Kind = NativeSymbolKind.FunctionPointer Then
            ' if this is a function pointer then make sure to add the reference to the 
            ' signature
            Dim fPtr As NativeFunctionPointer = DirectCast(nt, NativeFunctionPointer)
            Dim sigRow As SignatureRow = Me.AddSignature(fPtr.Signature)
            dtRow.SignatureId = sigRow.Id
            dtRow.CallingConvention = fPtr.CallingConvention
        End If
    End Sub

    ''' <summary>
    ''' Add the typedef into the dataset
    ''' </summary>
    ''' <param name="nt"></param>
    ''' <remarks></remarks>
    Public Sub AddTypedef(ByVal nt As NativeTypeDef)
        If nt Is Nothing Then : Throw New ArgumentNullException("nt") : End If

        If nt.RealType Is Nothing Then
            Dim msg As String = String.Format("NativeTypedef does not point to a real type")
            Throw New InvalidOperationException(msg)
        End If

        ' First look for an existing entry
        Dim trow As TypedefTypeRow = Nothing
        If TypedefType.TryFindByName(nt.Name, trow) Then
            Return
        End If

        Dim typeRef As TypeReference = CreateTypeReference(nt.RealType)
        TypedefType.Add(nt.Name, typeRef)
    End Sub

    Public Function TryLoadTypedef(ByVal name As String, ByRef typedefNt As NativeTypeDef) As Boolean
        Dim trow As TypedefTypeRow = Nothing
        If Not TypedefType.TryFindByName(name, trow) Then
            Return False
        End If

        Dim nt As NativeType = Nothing
        If Not TryLoadType(New TypeReference(trow.RealTypeId, trow.RealTypeKind), nt) Then
            Return False
        End If

        typedefNt = New NativeTypeDef(trow.Name, nt)
        Return True
    End Function

    ''' <summary>
    ''' Add a procedure into the table
    ''' </summary>
    ''' <param name="proc"></param>
    ''' <remarks></remarks>
    Public Sub AddProcedure(ByVal proc As NativeProcedure)
        If proc Is Nothing Then : Throw New ArgumentNullException("proc") : End If

        ' Store the procedure row
        Dim sigId As Integer = AddSignature(proc.Signature).Id
        Procedure.Add(proc.Name, proc.DllName, proc.CallingConvention, sigId)
    End Sub

    Private Function AddSignature(ByVal sig As NativeSignature) As SignatureRow
        ThrowIfNull(sig)

        ' Create the row
        Dim sigRow As SignatureRow = Me.Signature.NewSignatureRow()
        If sig.ReturnType IsNot Nothing Then
            Dim typeref As TypeReference = CreateTypeReference(sig.ReturnType)
            sigRow.ReturnTypeId = typeref.Id
            sigRow.ReturnTypeKind = typeref.Kind
        End If

        sigRow.ReturnTypeSalId = AddSalAttribute(sig.ReturnTypeSalAttribute)
        Me.Signature.AddSignatureRow(sigRow)

        ' Store each of the parameters
        For Each param As NativeParameter In sig.Parameters
            Parameter.Add(sigRow, param.Name, CreateTypeReference(param.NativeType), AddSalAttribute(param.SalAttribute))
        Next

        Return sigRow
    End Function

    ''' <summary>
    ''' Save the sal attribute.  Return a comma separed list of Id's
    ''' </summary>
    ''' <param name="attr"></param>
    ''' <remarks></remarks>
    Private Function AddSalAttribute(ByVal attr As NativeSalAttribute) As String
        If attr.IsEmpty() Then
            Return Nothing
        End If

        Dim builder As New StringBuilder()
        For Each entry As NativeSalEntry In attr.SalEntryList
            Dim row As SalEntryRow = Nothing

            ' Only try and cache when there is no text
            If String.IsNullOrEmpty(entry.Text) Then
                If Not SalEntry.TryFindNoText(entry.SalEntryType, row) Then
                    row = SalEntry.Add(entry.SalEntryType, Nothing)
                End If
            Else
                If Not SalEntry.TryFind(entry.SalEntryType, entry.Text, row) Then
                    row = SalEntry.Add(entry.SalEntryType, entry.Text)
                End If
            End If

            If builder.Length > 0 Then
                builder.Append(","c)
            End If
            builder.Append(row.Id)
        Next

        Return builder.ToString()
    End Function

    ''' <summary>
    ''' Attributes are stored as a comma delimeted list of the attributes
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function TryLoadSalAttribute(ByVal str As String, ByRef attr As NativeSalAttribute) As Boolean
        Dim arr() As String = str.Split(","c)
        attr = New NativeSalAttribute()

        For Each cur As String In arr
            Dim id As Integer = 0
            Dim row As SalEntryRow = Nothing
            If Not Int32.TryParse(cur, id) _
                OrElse Not SalEntry.TryFindById(id, row) Then
                Return False
            End If

            Dim entry As New NativeSalEntry()
            entry.SalEntryType = row.Type
            If Not row.IsTextNull Then
                entry.Text = row.Text
            End If

            attr.SalEntryList.Add(entry)
        Next

        Return True
    End Function

    ''' <summary>
    ''' Search for a defined type with the specified name pattern
    ''' </summary>
    ''' <param name="namePattern"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SearchForDefinedType(ByVal namePattern As String) As List(Of NativeDefinedType)
        Dim list As New List(Of NativeDefinedType)
        For Each nRow As DefinedTypeRow In DefinedType.FindByNamePattern(namePattern)
            Dim definedNt As NativeDefinedType = Nothing
            If TryLoadDefined(nRow.Name, definedNt) Then
                list.Add(definedNt)
            End If
        Next

        Return list
    End Function

    Public Function SearchForTypedef(ByVal namePattern As String) As List(Of NativeTypeDef)
        Dim list As New List(Of NativeTypeDef)
        For Each nRow As TypedefTypeRow In TypedefType.FindByNamePattern(namePattern)
            Dim typeDef As NativeTypeDef = Nothing
            If TryLoadTypedef(nRow.Name, typeDef) Then
                list.Add(typeDef)
            End If
        Next

        Return list
    End Function

    Public Function SearchForProcedure(ByVal namePattern As String) As List(Of NativeProcedure)
        Dim list As New List(Of NativeProcedure)
        For Each nRow As ProcedureRow In Procedure.FindByNamePattern(namePattern)
            Dim proc As NativeProcedure = Nothing
            If TryLoadProcedure(nRow.Name, proc) Then
                list.Add(proc)
            End If
        Next

        Return list
    End Function

    Public Function SearchForConstant(ByVal namePattern As String) As List(Of NativeConstant)
        Dim list As New List(Of NativeConstant)
        For Each nRow As ConstantRow In Me.Constant.FindByNamePattern(namePattern)
            Dim c As NativeConstant = Nothing
            If TryLoadConstant(nRow.Name, c) Then
                list.Add(c)
            End If
        Next

        Return list
    End Function

    Public Function TryLoadDefined(ByVal name As String, ByRef definedNt As NativeDefinedType) As Boolean
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If

        Dim dtRow As DefinedTypeRow = Nothing
        If Not DefinedType.TryFindByName(name, dtRow) Then
            Return False
        End If

        Dim nt As NativeType = Nothing
        If Not TryLoadDefinedType(New TypeReference(dtRow.Id, dtRow.Kind), nt) Then
            Return False
        End If

        definedNt = DirectCast(nt, NativeDefinedType)
        Return True
    End Function

    ''' <summary>
    ''' Try and load a type by it's name
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="nt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TryLoadByName(ByVal name As String, ByRef nt As NativeType) As Boolean

        Dim definedNt As NativeDefinedType = Nothing
        If TryLoadDefined(name, definedNt) Then
            nt = definedNt
            Return True
        End If

        Dim typedef As NativeTypeDef = Nothing
        If TryLoadTypedef(name, typedef) Then
            nt = typedef
            Return True
        End If

        ' Lastly try and load the Builtin types
        Dim bt As NativeBuiltinType = Nothing
        If NativeBuiltinType.TryConvertToBuiltinType(name, bt) Then
            nt = bt
            Return True
        End If

        Return False
    End Function

    Public Function TryLoadConstant(ByVal name As String, ByRef nConst As NativeConstant) As Boolean
        Dim constRow As ConstantRow = Nothing
        If Not Constant.TryFindByName(name, constRow) Then
            Return False
        End If

        nConst = New NativeConstant(constRow.Name, constRow.Value, constRow.Kind)
        Return True
    End Function

    ''' <summary>
    ''' Try and load a procedure by it's name
    ''' </summary>
    ''' <param name="retProc"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TryLoadProcedure(ByVal name As String, ByRef retProc As NativeProcedure) As Boolean
        Dim proc As NativeProcedure = Nothing
        Dim procRow As ProcedureRow = Nothing
        If Not Procedure.TryLoadByName(name, procRow) Then
            Return False
        End If

        ' Load the procedure
        proc = New NativeProcedure()
        proc.Name = procRow.Name
        proc.CallingConvention = procRow.CallingConvention
        If Not procRow.IsDllNameNull() Then
            proc.DllName = procRow.DllName
        End If

        ' Try and load the signature
        If Not TryLoadSignature(procRow.SignatureId, proc.Signature) Then
            Return False
        End If

        retProc = proc
        Return True
    End Function

    Public Function LoadAllMacros() As List(Of Parser.Macro)
        Return Constant.LoadAllMacros()
    End Function

    Private Function TryLoadSignature(ByVal id As Int32, ByRef retSig As NativeSignature) As Boolean
        Dim sigRow As SignatureRow = Nothing
        If Not Signature.TryLoadById(id, sigRow) Then
            Return False
        End If

        Dim sig As New NativeSignature()

        If Not TryLoadType(New TypeReference(sigRow.ReturnTypeId, sigRow.ReturnTypeKind), sig.ReturnType) Then
            Return False
        End If

        ' Load the sal attribute on the return value
        If Not sigRow.IsReturnTypeSalIdNull() _
            AndAlso Not TryLoadSalAttribute(sigRow.ReturnTypeSalId, sig.ReturnTypeSalAttribute) Then
            Return False
        End If

        ' Load the parameters
        For Each paramRow As ParameterRow In sigRow.GetParameterRows()
            Dim param As New NativeParameter()

            ' When this is a function pointer, the name can be null
            param.Name = String.Empty
            If Not paramRow.IsNameNull() Then
                param.Name = paramRow.Name
            End If

            If Not TryLoadType(New TypeReference(paramRow.TypeId, paramRow.TypeKind), param.NativeType) Then
                Return False
            End If

            If Not paramRow.IsSalIdNull() _
                AndAlso Not TryLoadSalAttribute(paramRow.SalId, param.SalAttribute) Then
                Return False
            End If

            sig.Parameters.Add(param)
        Next

        retSig = sig
        Return True
    End Function

#Region "Private Methods"

#Region "Add Types"

    ''' <summary>
    ''' Create a reference to a native type.  This will add the appropriate entries into the table
    ''' to reference this type
    ''' </summary>
    ''' <param name="nt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function CreateTypeReference(ByVal nt As NativeType) As TypeReference
        ThrowIfNull(nt)

        Select Case nt.Category
            Case NativeSymbolCategory.Defined
                Return CreateTypeReferenceToName(nt)
            Case NativeSymbolCategory.Proxy
                Return CreateTypeReferenceToProxy(DirectCast(nt, NativeProxyType))
            Case NativeSymbolCategory.Specialized
                Return CreateTypeReferenceToSpecialized(DirectCast(nt, NativeSpecializedType))
            Case Else
                InvalidEnumValue(nt.Category)   ' Will throw
                Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Create a type reference to a name.  
    ''' </summary>
    ''' <param name="nt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function CreateTypeReferenceToName(ByVal nt As NativeType) As TypeReference
        ThrowIfNull(nt)

        ' Create a NativeNamedType to make the referenc to
        Dim namedNt As New NativeNamedType(nt.Name)
        Return CreateTypeReferenceToProxy(namedNt)
    End Function

    Private Function CreateTypeReferenceToNamedType(ByVal nt As NativeNamedType) As TypeReference
        Dim nRow As NamedTypeRow = Nothing
        If Not NamedType.TryFindByName(nt.Qualification, nt.Name, nt.IsConst, nRow) Then
            nRow = NamedType.Add(nt.Qualification, nt.Name, nt.IsConst)
        End If

        Return New TypeReference(nRow.Id, NativeSymbolKind.NamedType)
    End Function

    Private Function CreateTypeReferenceToTypedef(ByVal nt As NativeTypeDef) As TypeReference
        Dim nRow As TypedefTypeRow = Nothing
        If Not TypedefType.TryFindByName(nt.Name, nRow) Then
            nRow = TypedefType.Add(nt.Name, CreateTypeReference(nt.RealType))
        End If

        Return New TypeReference(nRow.Id, NativeSymbolKind.TypedefType)
    End Function

    Private Function CreateTypeReferenceToArray(ByVal nt As NativeArray) As TypeReference
        Dim typeref As TypeReference = CreateTypeReference(nt.RealType)
        Dim row As ArrayTypeRow = ArrayType.Add(nt.ElementCount, typeref)
        Return New TypeReference(row.Id, NativeSymbolKind.ArrayType)
    End Function

    Private Function CreateTypeReferenceToPointer(ByVal nt As NativePointer) As TypeReference
        Dim typeref As TypeReference = CreateTypeReference(nt.RealType)
        Dim row As PointerTypeRow = Nothing
        If Not PointerType.TryFindByTarget(typeref, row) Then
            row = PointerType.Add(typeref)
        End If

        Return New TypeReference(row.Id, NativeSymbolKind.PointerType)
    End Function

    ''' <summary>
    ''' Create a type reference to the proxy.
    ''' </summary>
    ''' <param name="nt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function CreateTypeReferenceToProxy(ByVal nt As NativeProxyType) As TypeReference
        ThrowIfNull(nt)

        ' See what kind of type reference we're adding and special case then optimized ones
        Select Case nt.Kind
            Case NativeSymbolKind.NamedType
                Return CreateTypeReferenceToNamedType(DirectCast(nt, NativeNamedType))
            Case NativeSymbolKind.TypedefType
                Return CreateTypeReferenceToTypedef(DirectCast(nt, NativeTypeDef))
            Case NativeSymbolKind.ArrayType
                Return CreateTypeReferenceToArray(DirectCast(nt, NativeArray))
            Case NativeSymbolKind.PointerType
                Return CreateTypeReferenceToPointer(DirectCast(nt, NativePointer))
            Case Else
                Throw New Exception("Invalid enum value")
        End Select
    End Function

    ''' <summary>
    ''' Create a type reference to the specialized type.  
    ''' </summary>
    ''' <param name="nt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function CreateTypeReferenceToSpecialized(ByVal nt As NativeSpecializedType) As TypeReference
        ThrowIfNull(nt)

        ' Optimization.  See if there is an entry we can reuse here
        Dim existingRow As SpecializedTypeRow = Nothing
        Select Case nt.Kind
            Case NativeSymbolKind.BuiltinType
                Dim nativeBt As NativeBuiltinType = DirectCast(nt, NativeBuiltinType)
                Me.SpecializedType.TryFindBuiltin(nativeBt.BuiltinType, nativeBt.IsUnsigned, existingRow)
            Case NativeSymbolKind.BitVectorType
                Me.SpecializedType.TryFindBitVector(DirectCast(nt, NativeBitVector).Size, existingRow)
            Case NativeSymbolKind.OpaqueType
                Return New TypeReference(0, NativeSymbolKind.OpaqueType)
        End Select

        If existingRow IsNot Nothing Then
            Return New TypeReference(existingRow.Id, nt.Kind)
        End If

        ' Get the id
        Dim row As SpecializedTypeRow = Me.SpecializedType.NewSpecializedTypeRow()
        row.Kind = nt.Kind
        Select Case nt.Kind
            Case NativeSymbolKind.BitVectorType
                Dim bitNt As NativeBitVector = DirectCast(nt, NativeBitVector)
                row.BitVectorSize = bitNt.Size
            Case NativeSymbolKind.BuiltinType
                Dim builtinNt As NativeBuiltinType = DirectCast(nt, NativeBuiltinType)
                row.BuiltinType = builtinNt.BuiltinType
                row.IsUnsigned = builtinNt.IsUnsigned
        End Select
        Me.SpecializedType.AddSpecializedTypeRow(row)
        Return New TypeReference(row.Id, nt.Kind)
    End Function

#End Region

#Region "LoadTypes"

    Public Function TryLoadType(ByVal typeRef As TypeReference, ByRef nt As NativeType) As Boolean
        ThrowIfNull(typeRef)

        Select Case typeRef.Kind
            Case NativeSymbolKind.StructType, NativeSymbolKind.UnionType, NativeSymbolKind.EnumNameValue, NativeSymbolKind.FunctionPointer
                Return TryLoadDefinedType(typeRef, nt)
            Case NativeSymbolKind.TypedefType, NativeSymbolKind.NamedType, NativeSymbolKind.ArrayType, NativeSymbolKind.PointerType
                Return TryLoadProxyType(typeRef, nt)
            Case NativeSymbolKind.BuiltinType, NativeSymbolKind.BitVectorType
                Return TryLoadSpecialized(typeRef, nt)
            Case Else
                InvalidEnumValue(typeRef.Kind)
                nt = Nothing
                Return False
        End Select

    End Function

    Private Function TryLoadDefinedType(ByVal typeRef As TypeReference, ByRef nt As NativeType) As Boolean

        Dim id As Integer = typeRef.Id
        nt = Nothing
        Dim drow As DefinedTypeRow = Nothing
        If Not DefinedType.TryFindById(id, drow) Then
            Return False
        End If

        Dim dt As NativeDefinedType = Nothing
        Select Case drow.Kind
            Case NativeSymbolKind.StructType
                dt = New NativeStruct()
            Case NativeSymbolKind.EnumType
                ' Load the enum values
                Dim et As New NativeEnum()
                Dim erows As List(Of EnumValueRow) = Nothing
                If Not Me.EnumValue.TryFindById(drow.Id, erows) Then
                    Return False
                End If

                For Each row As EnumValueRow In erows
                    et.Values.Add(New NativeEnumValue( _
                        row.Name, _
                        row.Value))
                Next

                dt = et
            Case NativeSymbolKind.UnionType
                dt = New NativeUnion()
            Case NativeSymbolKind.FunctionPointer
                Dim fptr As New NativeFunctionPointer()
                fptr.CallingConvention = drow.CallingConvention

                If Not Me.TryLoadSignature(drow.SignatureId, fptr.Signature) Then
                    Return False
                End If
                dt = fptr
            Case Else
                InvalidEnumValue(drow.Kind)
        End Select

        ' Set the common properties
        dt.Name = drow.Name
        Dim memberRows As List(Of MemberRow) = Nothing
        If Member.TryFindById(drow.Id, memberRows) Then

            For Each memberRow As MemberRow In memberRows
                Dim member As New NativeMember()
                member.Name = memberRow.Name

                If Not TryLoadType(New TypeReference(memberRow.TypeId, memberRow.TypeKind), member.NativeType) Then
                    Return False
                End If
                dt.Members.Add(member)
            Next
        End If

        nt = dt
        Return True
    End Function

    Private Function TryLoadProxyType(ByVal typeRef As TypeReference, ByRef nt As NativeType) As Boolean
        Dim id As Integer = typeRef.Id

        If NativeSymbolKind.TypedefType = typeRef.Kind Then
            Dim trow As TypedefTypeRow = Nothing
            If Not TypedefType.TryFindById(typeRef.Id, trow) Then
                Return False
            End If

            Dim realNt As NativeType = Nothing
            If Not TryLoadType(New TypeReference(trow.RealTypeId, trow.RealTypeKind), realNt) Then
                Return False
            End If

            nt = New NativeTypeDef(trow.Name, realNt)
            Return True
        ElseIf NativeSymbolKind.NamedType = typeRef.Kind Then
            Dim nrow As NamedTypeRow = Nothing
            If Not NamedType.TryFindById(typeRef.Id, nrow) Then
                Return False
            End If

            nt = New NativeNamedType(nrow.Qualification, nrow.Name, nrow.IsConst)
            Return True
        ElseIf NativeSymbolKind.ArrayType = typeRef.Kind Then
            Dim arow As ArrayTypeRow = Nothing
            If Not ArrayType.TryFindById(typeRef.Id, arow) Then
                Return False
            End If

            Dim realNt As NativeType = Nothing
            If Not TryLoadType(New TypeReference(arow.RealTypeId, arow.RealTypeKind), realNt) Then
                Return False
            End If

            nt = New NativeArray(realNt, arow.ElementCountt)
            Return True
        ElseIf NativeSymbolKind.PointerType = typeRef.Kind Then
            Dim prow As PointerTypeRow = Nothing
            If Not PointerType.TryFindById(typeRef.Id, prow) Then
                Return False
            End If

            Dim realNt As NativeType = Nothing
            If Not TryLoadType(New TypeReference(prow.RealTypeId, prow.RealTypeKind), realNt) Then
                Return False
            End If

            nt = New NativePointer(realNt)
            Return True
        Else
            InvalidEnumValue(typeRef.Kind)
            Return False
        End If
    End Function

    Private Function TryLoadSpecialized(ByVal typeRef As TypeReference, ByRef nt As NativeType) As Boolean

        If typeRef.Kind = NativeSymbolKind.OpaqueType Then
            nt = New NativeOpaqueType
            Return True
        End If

        Dim srow As SpecializedTypeRow = Nothing
        If Not SpecializedType.TryFindById(typeRef.Id, srow) Then
            nt = Nothing
            Return False
        Else
            Select Case typeRef.Kind
                Case NativeSymbolKind.BitVectorType
                    Dim bt As New NativeBitVector()
                    bt.Size = srow.BitVectorSize
                    nt = bt
                Case NativeSymbolKind.BuiltinType
                    nt = New NativeBuiltinType(srow.BuiltinType, srow.IsUnsigned)
                Case Else
                    InvalidEnumValue(typeRef.Kind)
                    Return False
            End Select

            Return True
        End If
    End Function

#End Region

#End Region

    ''' <summary>
    ''' Look for the windows.xml file in the following location
    '''  - AssemblyPath\Data\windows.xml
    '''  - AssemblyPath\windows.xml
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function LoadFromAssemblyPath() As NativeStorage
        Dim loc As String = GetType(NativeStorage).Assembly.Location
        Dim assemblyDirectory As String = IO.Path.GetDirectoryName(loc)

        Dim target As String = IO.Path.Combine(assemblyDirectory, "Data\windows.xml")
        If IO.File.Exists(target) Then
            Return LoadFromPath(target)
        End If

        target = IO.Path.Combine(assemblyDirectory, "windows.xml")
        Return LoadFromPath(target)
    End Function

    Public Shared Function LoadFromPath(ByVal target As String) As NativeStorage
        Try
            Dim ns As New NativeStorage()
            ns.ReadXml(target)
            Return ns
        Catch ex As Exception
            Debug.Fail(ex.Message)
            Return New NativeStorage()
        End Try
    End Function

End Class
