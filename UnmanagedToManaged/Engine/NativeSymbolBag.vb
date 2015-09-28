' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic

''' <summary>
''' Bag for NativeType instances which is used for querying and type resolution
''' </summary>
''' <remarks></remarks>
Public Class NativeSymbolBag

    Private m_constMap As New Dictionary(Of String, NativeConstant)(StringComparer.Ordinal)
    Private m_definedMap As New Dictionary(Of String, NativeDefinedType)(StringComparer.Ordinal)
    Private m_typeDefMap As New Dictionary(Of String, NativeTypeDef)(StringComparer.Ordinal)
    Private m_procMap As New Dictionary(Of String, NativeProcedure)(StringComparer.Ordinal)
    Private m_valueMap As New Dictionary(Of String, NativeSymbol)(StringComparer.Ordinal)
    Private m_storageLookup As NativeStorage

    Public ReadOnly Property Count() As Integer
        Get
            Return m_constMap.Count + m_definedMap.Count + m_typeDefMap.Count + m_procMap.Count + m_valueMap.Count
        End Get
    End Property

    ''' <summary>
    ''' List of NativeDefinedType instances in the map
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property NativeDefinedTypes() As IEnumerable(Of NativeDefinedType)
        Get
            Return m_definedMap.Values
        End Get
    End Property

    ''' <summary>
    ''' List of NativeTypedef instances in the map
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property NativeTypedefs() As IEnumerable(Of NativeTypeDef)
        Get
            Return m_typeDefMap.Values
        End Get
    End Property

    ''' <summary>
    ''' Procedures in the bag
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property NativeProcedures() As IEnumerable(Of NativeProcedure)
        Get
            Return m_procMap.Values
        End Get
    End Property

    ''' <summary>
    ''' List of NativeConstant instances
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property NativeConstants() As IEnumerable(Of NativeConstant)
        Get
            Return m_constMap.Values
        End Get
    End Property

    ''' <summary>
    ''' Backing NativeStorage for this bag.  Used to resolve NativeNamedType instances
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NativeStorageLookup() As NativeStorage
        Get
            Return m_storageLookup
        End Get
        Set(ByVal value As NativeStorage)
            m_storageLookup = value
        End Set
    End Property

    ''' <summary>
    ''' Create a new instance
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        MyClass.New(NativeStorage.DefaultInstance)
    End Sub

    Public Sub New(ByVal ns As NativeStorage)
        If ns Is Nothing Then : Throw New ArgumentNullException("ns") : End If

        m_storageLookup = ns
    End Sub

    ''' <summary>
    ''' Add the defined type into the bag
    ''' </summary>
    ''' <param name="nt"></param>
    ''' <remarks></remarks>
    Public Sub AddDefinedType(ByVal nt As NativeDefinedType)
        If nt Is Nothing Then : Throw New ArgumentNullException("nt") : End If

        If nt.IsAnonymous Then
            nt.Name = GenerateAnonymousName()
        End If

        m_definedMap.Add(nt.Name, nt)

        Dim ntEnum As NativeEnum = TryCast(nt, NativeEnum)
        If ntEnum IsNot Nothing Then
            For Each pair As NativeEnumValue In ntEnum.Values
                AddValue(pair.Name, ntEnum)
            Next
        End If
    End Sub

    ''' <summary>
    ''' Try and find a NativeDefinedType instance by name
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="nt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TryFindDefinedType(ByVal name As String, ByRef nt As NativeDefinedType) As Boolean
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If
        Return m_definedMap.TryGetValue(name, nt)
    End Function

    Public Function TryFindOrLoadDefinedType(ByVal name As String, ByRef nt As NativeDefinedType) As Boolean
        Dim notUsed As Boolean = False
        Return TryFindOrLoadDefinedType(name, nt, notUsed)
    End Function

    Public Function TryFindOrLoadDefinedType(ByVal name As String, ByRef nt As NativeDefinedType, ByRef fromStorage As Boolean) As Boolean
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If
        If TryFindDefinedType(name, nt) Then
            fromStorage = False
            Return True
        End If

        If m_storageLookup.TryLoadDefined(name, nt) Then
            AddDefinedType(nt)
            fromStorage = True
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Add a typedef to the bag
    ''' </summary>
    ''' <param name="nt"></param>
    ''' <remarks></remarks>
    Public Sub AddTypedef(ByVal nt As NativeTypeDef)
        If nt Is Nothing Then : Throw New ArgumentNullException("nt") : End If

        m_typeDefMap.Add(nt.Name, nt)
    End Sub


    ''' <summary>
    ''' Try and find a NativeTypeDef instance by name
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="nt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TryFindTypedef(ByVal name As String, ByRef nt As NativeTypeDef) As Boolean
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If
        Return m_typeDefMap.TryGetValue(name, nt)
    End Function

    Public Function TryFindOrLoadTypedef(ByVal name As String, ByRef nt As NativeTypeDef) As Boolean
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If

        If TryFindTypedef(name, nt) Then
            Return True
        End If

        If m_storageLookup.TryLoadTypedef(name, nt) Then
            AddTypedef(nt)
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Add a procedure to the bag
    ''' </summary>
    ''' <param name="proc"></param>
    ''' <remarks></remarks>
    Public Sub AddProcedure(ByVal proc As NativeProcedure)
        If proc Is Nothing Then : Throw New ArgumentNullException("proc") : End If

        m_procMap.Add(proc.Name, proc)
    End Sub

    ''' <summary>
    ''' Try and find a NativeProcedure instance by name
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="proc"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TryFindProcedure(ByVal name As String, ByRef proc As NativeProcedure) As Boolean
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If

        Return m_procMap.TryGetValue(name, proc)
    End Function

    Public Function TryFindOrLoadProcedure(ByVal name As String, ByRef proc As NativeProcedure) As Boolean
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If
        If TryFindProcedure(name, proc) Then
            Return True
        End If

        If m_storageLookup.TryLoadProcedure(name, proc) Then
            AddProcedure(proc)
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Add a constant to the bag
    ''' </summary>
    ''' <param name="nConst"></param>
    ''' <remarks></remarks>
    Public Sub AddConstant(ByVal nConst As NativeConstant)
        If nConst Is Nothing Then : Throw New ArgumentNullException("nConst") : End If

        m_constMap.Add(nConst.Name, nConst)
        AddValue(nConst.Name, nConst)
    End Sub

    ''' <summary>
    ''' Add an expression into the bag
    ''' </summary>
    ''' <param name="value"></param>
    ''' <remarks></remarks>
    Private Sub AddValue(ByVal name As String, ByVal value As NativeSymbol)
        If value Is Nothing Then : Throw New ArgumentNullException("expr") : End If

        m_valueMap(name) = value
    End Sub

    ''' <summary>
    ''' Try find a NativeConstant by name
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="nConst"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TryFindConstant(ByVal name As String, ByRef nConst As NativeConstant) As Boolean
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If
        Return m_constMap.TryGetValue(name, nConst)
    End Function

    Public Function TryFindOrLoadConstant(ByVal name As String, ByRef nConst As NativeConstant) As Boolean
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If

        If TryFindConstant(name, nConst) Then
            Return True
        End If

        If m_storageLookup.TryLoadConstant(name, nConst) Then
            AddConstant(nConst)
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Find the resolved symbols
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindResolvedNativeSymbols() As IEnumerable(Of NativeSymbol)
        Dim list As New List(Of NativeSymbol)

        For Each definedNt As NativeDefinedType In FindResolvedDefinedTypes()
            list.Add(definedNt)
        Next

        For Each typedef As NativeTypeDef In FindResolvedTypedefs()
            list.Add(typedef)
        Next

        For Each c As NativeConstant In Me.FindResolvedConstants()
            list.Add(c)
        Next

        For Each proc As NativeProcedure In Me.FindResolvedProcedures()
            list.Add(proc)
        Next

        Return list
    End Function

    ''' <summary>
    ''' find all of the reachable NativeSymbol instances in this bag
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindAllReachableNativeSymbols() As List(Of NativeSymbol)

        Dim list As New List(Of NativeSymbol)
        For Each cur As NativeSymbolRelationship In FindAllReachableNativeSymbolRelationships()
            list.Add(cur.Symbol)
        Next

        Return list
    End Function

    Public Function FindAllReachableNativeSymbolRelationships() As List(Of NativeSymbolRelationship)
        ' Build up the list of types
        Dim list As New List(Of NativeSymbol)
        For Each definedNt As NativeDefinedType In m_definedMap.Values
            list.Add(definedNt)
        Next

        For Each typedefNt As NativeTypeDef In m_typeDefMap.Values
            list.Add(typedefNt)
        Next

        For Each proc As NativeProcedure In m_procMap.Values
            list.Add(proc)
        Next

        For Each c As NativeConstant In m_constMap.Values
            list.Add(c)
        Next

        Dim iter As New NativeSymbolIterator()
        Return iter.FindAllNativeSymbolRelationships(list)
    End Function

    ''' <summary>
    ''' Find all of the NativeNamedType instances for which a type could not be found
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindUnresolvedNativeSymbolRelationships() As List(Of NativeSymbolRelationship)
        Dim list As New List(Of NativeSymbolRelationship)
        For Each rel As NativeSymbolRelationship In Me.FindAllReachableNativeSymbolRelationships()
            If Not rel.Symbol.IsImmediateResolved Then
                list.Add(rel)
            End If
        Next

        Return list
    End Function

    Public Function FindUnresolvedNativeValues() As List(Of NativeValue)
        Dim list As New List(Of NativeValue)
        For Each ns As NativeSymbol In Me.FindAllReachableNativeSymbols()
            Dim nValue As NativeValue = TryCast(ns, NativeValue)
            If nValue IsNot Nothing AndAlso Not nValue.IsValueResolved Then
                list.Add(nValue)
            End If
        Next

        Return list
    End Function

    Public Function TryFindOrLoadNativeType(ByVal namedType As NativeNamedType, ByRef nt As NativeType) As Boolean
        Dim notUsed As Boolean = False
        Return TryFindOrLoadNativeType(namedType, nt, notUsed)
    End Function

    ''' <summary>
    ''' Try and load the named type
    ''' </summary>
    ''' <param name="namedType"></param>
    ''' <param name="nt"></param>
    ''' <param name="loadFromStorage"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TryFindOrLoadNativeType(ByVal namedType As NativeNamedType, ByRef nt As NativeType, ByRef loadFromStorage As Boolean) As Boolean
        If String.IsNullOrEmpty(namedType.Qualification) Then
            ' If there is no qualification then just load the type by it's name
            Return TryFindOrLoadNativeType(namedType.Name, nt, loadFromStorage)
        End If

        ' When there is a qualification it is either struct, union or enum.  Try and load the defined type
        ' for the name and then make sure that it is the correct type 
        Dim definedNt As NativeDefinedType = Nothing
        If Not Me.TryFindOrLoadDefinedType(namedType.Name, definedNt, loadFromStorage) Then
            Return False
        End If

        Dim test As String = Nothing
        Select Case definedNt.Kind
            Case NativeSymbolKind.StructType
                test = "struct"
            Case NativeSymbolKind.UnionType
                test = "union"
            Case NativeSymbolKind.EnumType
                test = "enum"
            Case Else
                Return False
        End Select

        Dim qual As String = namedType.Qualification
        If String.Equals("class", qual, StringComparison.OrdinalIgnoreCase) Then
            qual = "struct"
        End If

        If 0 <> String.CompareOrdinal(test, qual) Then
            Return False
        End If

        nt = definedNt
        Return True
    End Function

    ''' <summary>
    ''' Try and get a NativeType from the bag with the specified name.  Prefer types in
    ''' the following order
    '''   NativeDefinedType
    '''   NativeTypeDef
    '''   NativeStorage
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="nt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TryFindOrLoadNativeType(ByVal name As String, ByRef nt As NativeType) As Boolean
        Dim notUsed As Boolean = False
        Return TryFindOrLoadNativeType(name, nt, notUsed)
    End Function


    Public Function TryFindOrLoadNativeType(ByVal name As String, ByRef nt As NativeType, ByRef loadFromStorage As Boolean) As Boolean

        ' First check the defined types
        loadFromStorage = False
        Dim definedNt As NativeDefinedType = Nothing
        If TryFindDefinedType(name, definedNt) Then
            nt = definedNt
            Return True
        End If

        ' Second, check the typedefs
        Dim typeDefNt As NativeTypeDef = Nothing
        If TryFindTypedef(name, typeDefNt) Then
            nt = typeDefNt
            Return True
        End If

        ' Lastly try and find it in the stored file
        If m_storageLookup.TryLoadByName(name, nt) Then
            ThrowIfNull(nt)
            loadFromStorage = True

            ' If this is a stored symbol we need to add it to the bag.  Otherwise we can 
            ' hit an infinite loop.  Assume we have a structure like so

            ' struct s1
            ' { 
            '   struct s1 *p;
            ' }
            '
            ' This contains a recursive reference to itself.  We need to store the looked
            ' up type to prevent an infinite loop
            If nt.Category = NativeSymbolCategory.Defined Then
                AddDefinedType(DirectCast(nt, NativeDefinedType))
            ElseIf nt.Kind = NativeSymbolKind.TypedefType Then
                AddTypedef(DirectCast(nt, NativeTypeDef))
            End If

            Return True
        End If

        nt = Nothing
        Return False
    End Function

    Public Function TryFindValue(ByVal valueName As String, ByRef ns As NativeSymbol) As Boolean
        Return m_valueMap.TryGetValue(valueName, ns)
    End Function

    Public Function TryFindOrLoadValue(ByVal valueName As String, ByRef ns As NativeSymbol) As Boolean
        Dim notUsed As Boolean = False
        Return TryFindOrLoadValue(valueName, ns, notUsed)
    End Function

    Public Function TryFindOrLoadValue(ByVal valueName As String, ByRef ns As NativeSymbol, ByRef loaded As Boolean) As Boolean
        loaded = False
        If TryFindValue(valueName, ns) Then
            Return True
        End If

        ' First look for a constant by this name
        Dim nConst As NativeConstant = Nothing
        If m_storageLookup.TryLoadConstant(valueName, nConst) Then
            AddConstant(nConst)
            loaded = True
            ns = nConst
            Return True
        End If

        ' Lastly look for enums by value 
        Dim erows As List(Of NativeStorage.EnumValueRow) = Nothing
        If m_storageLookup.EnumValue.TryFindByValueName(valueName, erows) Then
            ' Take the first one
            Dim erow As NativeStorage.EnumValueRow = erows(0)
            Dim prow As NativeStorage.DefinedTypeRow = erow.DefinedTypeRow
            Dim nt As NativeDefinedType = Nothing
            If prow IsNot Nothing Then
                Return TryFindOrLoadDefinedType(prow.Name, nt, loaded)
            End If
        End If

        Return False
    End Function

    ''' <summary>
    ''' Save all of the information into a NativeStorage database that is completely resolved
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SaveToNativeStorage() As NativeStorage
        Dim ns As NativeStorage = New NativeStorage()
        ns.CacheLookup = True

        For Each nConst As NativeConstant In Me.FindResolvedConstants()
            ns.AddConstant(nConst)
        Next

        For Each definedNt As NativeDefinedType In Me.FindResolvedDefinedTypes()
            ns.AddDefinedType(definedNt)
        Next

        For Each typeDef As NativeTypeDef In Me.FindResolvedTypedefs()
            ns.AddTypedef(typeDef)
        Next

        For Each proc As NativeProcedure In Me.FindResolvedProcedures()
            ns.AddProcedure(proc)
        Next

        ns.CacheLookup = False
        ns.AcceptChanges()
        Return ns
    End Function

    ''' <summary>
    ''' Find all of the resolved defined types.  
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindResolvedDefinedTypes() As IEnumerable(Of NativeDefinedType)
        Dim map As New Dictionary(Of NativeSymbol, Nullable(Of Boolean))
        Dim list As New List(Of NativeDefinedType)

        For Each definedNt As NativeDefinedType In m_definedMap.Values
            If IsResolved(definedNt, map) Then
                list.Add(definedNt)
            End If
        Next

        Return list
    End Function

    ''' <summary>
    ''' Find all of the resolved typedefs
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindResolvedTypedefs() As IEnumerable(Of NativeTypeDef)
        Dim map As New Dictionary(Of NativeSymbol, Nullable(Of Boolean))
        Dim list As New List(Of NativeTypeDef)

        For Each typedefNt As NativeTypeDef In m_typeDefMap.Values
            If IsResolved(typedefNt, map) Then
                list.Add(typedefNt)
            End If
        Next

        Return list
    End Function

    ''' <summary>
    ''' Find all of the resolved NativeProcedure instances
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindResolvedProcedures() As IEnumerable(Of NativeProcedure)
        Dim map As New Dictionary(Of NativeSymbol, Nullable(Of Boolean))
        Dim list As New List(Of NativeProcedure)

        For Each proc As NativeProcedure In m_procMap.Values
            If IsResolved(proc, map) Then
                list.Add(proc)
            End If
        Next

        Return list
    End Function

    Public Function FindResolvedConstants() As IEnumerable(Of NativeConstant)
        Dim map As New Dictionary(Of NativeSymbol, Nullable(Of Boolean))
        Dim list As New List(Of NativeConstant)

        For Each c As NativeConstant In m_constMap.Values
            If IsResolved(c, map) Then
                list.Add(c)
            End If
        Next

        Return list
    End Function

#Region "Resolution Functions"

    Public Function TryResolveSymbolsAndValues() As Boolean
        Using finder As New ProcedureFinder
            Return TryResolveSymbolsAndValues(finder, New ErrorProvider())
        End Using
    End Function

    Public Function TryResolveSymbolsAndValues(ByVal ep As ErrorProvider) As Boolean
        Using finder As New ProcedureFinder
            Return TryResolveSymbolsAndValues(finder, ep)
        End Using
    End Function

    ''' <summary>
    ''' Try and resolve all of the unresolved types in the bag.  Return false if the types
    ''' couldn't all be resolved
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TryResolveSymbolsAndValues(ByVal finder As ProcedureFinder, ByVal ep As ErrorProvider) As Boolean
        If ep Is Nothing Then : Throw New ArgumentNullException("ep") : End If

        ' Try and resolve the proc name
        For Each proc As NativeProcedure In m_procMap.Values
            If String.IsNullOrEmpty(proc.DllName) Then
                finder.TryFindDllNameExact(proc.Name, proc.DllName)
            End If
        Next

        Return ResolveCore(ep)
    End Function

    Private Function ResolveCore(ByVal ep As ErrorProvider) As Boolean
        Dim allResolved As Boolean
        Do
            Dim loadedSymbolFromStorage As Boolean = False
            Dim allSymbolResolved As Boolean = ResolveCoreSymbols(ep, loadedSymbolFromStorage)

            Dim loadedValueFromStorage As Boolean = False
            Dim allValuesResolved As Boolean = ResolveCoreValues(ep, loadedValueFromStorage)

            ' When an object is loaded from storage it is done a at a single level.  So we
            ' now need to walk that type and resolve any named types from it
            If Not loadedSymbolFromStorage AndAlso Not loadedValueFromStorage Then
                allResolved = (allValuesResolved AndAlso allSymbolResolved)
                Exit Do
            End If
        Loop While True

        Return allResolved
    End Function

    ''' <summary>
    ''' Try and resolve the unresolved symbols in the system
    ''' </summary>
    ''' <param name="ep"></param>
    ''' <param name="loadedSomethingFromStorage"></param>
    ''' <remarks></remarks>
    Private Function ResolveCoreSymbols(ByVal ep As ErrorProvider, ByRef loadedSomethingFromStorage As Boolean) As Boolean
        Dim allResolved As Boolean = True
        For Each rel As NativeSymbolRelationship In Me.FindUnresolvedNativeSymbolRelationships()

            ' Values and value expressions are resolved below
            If rel.Symbol.Kind = NativeSymbolKind.Value OrElse rel.Symbol.Kind = NativeSymbolKind.ValueExpression Then
                Continue For
            End If

            ' All we can resolve here are NativeNamedType instances
            Dim namedType As NativeNamedType = TryCast(rel.Symbol, NativeNamedType)
            If namedType Is Nothing Then
                ep.AddError("Failed to resolve {0} -> '{1}'", rel.Symbol.Kind, rel.Symbol.DisplayName)
                Continue For
            End If

            Dim nt As NativeType = Nothing
            Dim fromStorage As Boolean = False
            If Me.TryFindOrLoadNativeType(namedType, nt, fromStorage) Then
                If fromStorage Then
                    loadedSomethingFromStorage = True
                End If
                namedType.RealType = nt
            ElseIf rel.Parent IsNot Nothing _
                AndAlso rel.Parent.Kind = NativeSymbolKind.PointerType _
                AndAlso Not String.IsNullOrEmpty(namedType.Qualification) Then

                ' When we have a pointer to an unresolved type, treat this as an opaque type
                ep.AddWarning("Treating '{0}' as pointer to opaque type", namedType.DisplayName)
                namedType.RealType = New NativeOpaqueType()
            Else
                ep.AddError("Failed to resolve name '{0}'", namedType.DisplayName)
                allResolved = False
            End If
        Next

        Return allResolved
    End Function

    ''' <summary>
    ''' Try and resolve the unresolved values in the system.  
    ''' </summary>
    ''' <param name="ep"></param>
    ''' <param name="loadedSomethingFromStorage"></param>
    ''' <remarks></remarks>
    Private Function ResolveCoreValues(ByVal ep As ErrorProvider, ByRef loadedSomethingFromStorage As Boolean) As Boolean

        Dim allResolved As Boolean = True
        For Each nValue As NativeValue In Me.FindUnresolvedNativeValues()
            Dim fromStorage As Boolean = False

            Select Case nValue.ValueKind
                Case NativeValueKind.SymbolValue
                    Dim ns As NativeSymbol = Nothing
                    If Me.TryFindOrLoadValue(nValue.Name, ns, fromStorage) Then
                        nValue.Value = ns
                    End If
                Case NativeValueKind.SymbolType
                    Dim nt As NativeType = Nothing
                    If Me.TryFindOrLoadNativeType(nValue.Name, nt, fromStorage) Then
                        nValue.Value = nt
                    End If
            End Select

            If Not nValue.IsImmediateResolved Then
                ep.AddError("Failed to resolve value '{0}'", nValue.Name)
                allResolved = False
            End If

            If fromStorage Then
                loadedSomethingFromStorage = True
            End If
        Next

        Return allResolved
    End Function

    Private Function IsResolved(ByVal ns As NativeSymbol, ByVal map As Dictionary(Of NativeSymbol, Nullable(Of Boolean))) As Boolean
        ThrowIfNull(ns)
        ThrowIfNull(map)

        ' See if this has already been calculated
        Dim ret As Nullable(Of Boolean) = False
        If map.TryGetValue(ns, ret) Then
            If ret.HasValue Then
                Return ret.Value
            Else
                ' We're in a recursive call to the same type.  Return true here because if another type is
                ' not resolved then this will fall out
                Return True
            End If
        End If

        ' If there are no immediate children then the type is most definately resolved
        Dim it As New NativeSymbolIterator()
        Dim children As New List(Of NativeSymbol)(ns.GetChildren())
        If children.Count = 0 Then
            Return True
        End If

        ' Add an entry into the map to indicate that we are exploring this type
        map.Add(ns, New Nullable(Of Boolean)())

        ret = True
        For Each child As NativeSymbol In children
            If Not child.IsImmediateResolved OrElse Not IsResolved(child, map) Then
                ret = False
                Exit For
            End If
        Next

        ' Save the success
        map(ns) = ret
        Return ret.Value
    End Function

#End Region

#Region "Shared Helpers"

    Public Shared Function GenerateAnonymousName() As String
        Dim g As Guid = Guid.NewGuid()
        Dim name As String = g.ToString()
        name = name.Replace("-", "_")
        Return "Anonymous_" & name
    End Function

    Public Shared Function IsAnonymousName(ByVal name As String) As Boolean
        Return Text.RegularExpressions.Regex.IsMatch( _
            name, _
            "^Anonymous_((\w+_){4})(\w+)$")
    End Function

    Public Shared Function CreateFrom(ByVal result As Parser.NativeCodeAnalyzerResult) As NativeSymbolBag
        Return CreateFrom(result, NativeStorage.DefaultInstance)
    End Function

    Public Shared Function CreateFrom(ByVal result As Parser.NativeCodeAnalyzerResult, ByVal ns As NativeStorage) As NativeSymbolBag
        Return CreateFrom(result, ns, New ErrorProvider())
    End Function

    ''' <summary>
    ''' Create a NativeTypeBag from the result of a code analysis
    ''' </summary>
    ''' <param name="result"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreateFrom(ByVal result As Parser.NativeCodeAnalyzerResult, ByVal ns As NativeStorage, ByVal ep As ErrorProvider) As NativeSymbolBag
        If ep Is Nothing Then : Throw New ArgumentNullException("ep") : End If

        Dim bag As New NativeSymbolBag(ns)

        For Each nConst As NativeConstant In result.AllNativeConstants
            Try
                bag.AddConstant(nConst)
            Catch
                ep.AddError("Duplicate NativeConstant Name: {0}", nConst.Name)
            End Try
        Next

        For Each definedNt As NativeDefinedType In result.NativeDefinedTypes
            Try
                bag.AddDefinedType(definedNt)
            Catch
                ep.AddError("Duplicate NativeDefinedType Name: {0}", definedNt.Name)
            End Try
        Next

        For Each typedefNt As NativeTypeDef In result.NativeTypeDefs
            Try
                bag.AddTypedef(typedefNt)
            Catch
                ep.AddError("Duplicate NativeTypeDef Name: {0}", typedefNt.Name)
            End Try
        Next

        For Each proc As NativeProcedure In result.NativeProcedures
            Try
                bag.AddProcedure(proc)
            Catch ex As Exception
                ep.AddError("Duplicate NativeProcedure Name: {0}", proc.Name)
            End Try
        Next

        Return bag
    End Function
#End Region

End Class

