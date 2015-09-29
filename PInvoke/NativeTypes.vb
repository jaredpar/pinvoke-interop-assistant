' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic
Imports System.CodeDom
Imports System.Text
Imports PInvoke.Contract

#Region "NativeSymbol"

''' <summary>
''' Category for the NativeType
''' </summary>
''' <remarks></remarks>
Public Enum NativeSymbolCategory
    Defined
    Proxy
    Specialized
    Procedure
    Extra
End Enum

''' <summary>
''' The kind of the native type.  Makes it easy to do switching
''' </summary>
''' <remarks></remarks>
Public Enum NativeSymbolKind
    StructType
    EnumType
    UnionType
    ArrayType
    PointerType
    BuiltinType
    TypedefType
    BitVectorType
    NamedType
    Procedure
    ProcedureSignature
    FunctionPointer
    Parameter
    Member
    EnumNameValue
    Constant
    SalEntry
    SalAttribute
    ValueExpression
    Value
    OpaqueType
End Enum

''' <summary>
''' Represents a native symbol we're interested in
''' </summary>
<DebuggerDisplay("{DisplayName}")> _
Public MustInherit Class NativeSymbol

    Protected Shared EmptySymbolList As New List(Of NativeSymbol)

    Private m_name As String

    ''' <summary>
    ''' Name of the C++ type
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            m_name = value
        End Set
    End Property

    ''' <summary>
    ''' Category of the NativeType
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public MustOverride ReadOnly Property Category() As NativeSymbolCategory

    ''' <summary>
    ''' The kind of the type.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public MustOverride ReadOnly Property Kind() As NativeSymbolKind

    ''' <summary>
    ''' Gets the full name of the type
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property DisplayName() As String
        Get
            Return Name
        End Get
    End Property

    ''' <summary>
    ''' Whether one of it's immediate children is not resolved
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property IsImmediateResolved() As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Sub New()

    End Sub

    Protected Sub New(ByVal name As String)
        m_name = name
    End Sub

    ''' <summary>
    ''' Gets the immediate children of this symbol
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function GetChildren() As IEnumerable(Of NativeSymbol)
        Return EmptySymbolList
    End Function

    Public Overridable Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        Throw New NotImplementedException()
    End Sub

    Protected Sub ReplaceChildInList(Of T As NativeSymbol)(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol, ByVal list As List(Of T))
        ThrowIfNull(list)

        If oldChild Is Nothing Then : Throw New ArgumentNullException("oldChild") : End If
        If newChild Is Nothing Then : Throw New ArgumentNullException("newChild") : End If

        Dim oldTyped As T = TryCast(oldChild, T)
        Dim newTyped As T = TryCast(newChild, T)
        If oldTyped Is Nothing OrElse newTyped Is Nothing Then
            Throw New InvalidOperationException("Operands are of the wrong type")
        End If

        Dim index As Integer = list.IndexOf(oldTyped)
        If index < 0 Then
            Throw New InvalidOperationException("Old operand not a current child")
        End If

        list.RemoveAt(index)
        list.Insert(index, newTyped)
    End Sub

    Protected Function GetSingleChild(Of T As NativeSymbol)(ByVal child As T) As IEnumerable(Of NativeSymbol)
        If child Is Nothing Then
            Return New List(Of NativeSymbol)()
        End If

        Dim list As New List(Of NativeSymbol)
        list.Add(child)
        Return list
    End Function

    Protected Function GetListChild(Of T As NativeSymbol)(ByVal list As List(Of T)) As IEnumerable(Of NativeSymbol)
        Dim symList As New List(Of NativeSymbol)
        For Each value As T In list
            symList.Add(value)
        Next

        Return symList
    End Function


    Protected Sub ReplaceChildSingle(Of T As NativeSymbol)(ByVal oldchild As NativeSymbol, ByVal newChild As NativeSymbol, ByRef realChild As T)
        If Not Object.ReferenceEquals(oldchild, realChild) Then
            Throw New InvalidOperationException("Old child is wrong")
        End If

        If newChild Is Nothing Then
            realChild = Nothing
            Return
        End If

        Dim newTyped As T = TryCast(newChild, T)
        If newTyped Is Nothing Then
            Throw New InvalidOperationException("Operands are of the wrong type")
        End If

        realChild = newTyped
    End Sub

End Class

#End Region

#Region "NativeType"

''' <summary>
''' Represents a type in the system
''' </summary>
''' <remarks></remarks>
Public MustInherit Class NativeType
    Inherits NativeSymbol

    Protected Sub New()

    End Sub

    Protected Sub New(ByVal name As String)
        MyBase.New(name)
    End Sub

    Public Function DigThroughNamedTypes() As NativeType

        Dim cur As NativeType = Me
        While cur IsNot Nothing
            If cur.Kind = NativeSymbolKind.NamedType Then
                cur = DirectCast(cur, NativeNamedType).RealType
            Else
                Exit While
            End If
        End While

        Return cur
    End Function

    Public Function DigThroughNamedTypesFor(ByVal search As String) As NativeType
        If 0 = String.CompareOrdinal(Name, search) Then
            Return Me
        End If

        Dim cur As NativeType = Me
        While cur IsNot Nothing AndAlso cur.Kind = NativeSymbolKind.NamedType
            Dim namedNt As NativeNamedType = DirectCast(cur, NativeNamedType)
            cur = namedNt.RealType

            If cur isNot Nothing andalso 0 = String.CompareOrdinal(cur.Name, search) Then
                Return cur
            End If
        End While

        Return Nothing
    End Function

    ''' <summary>
    ''' Dig through this type until we get past the typedefs and named types to the real
    ''' type 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DigThroughTypedefAndNamedTypes() As NativeType

        Dim cur As NativeType = Me
        While cur IsNot Nothing
            If cur.Kind = NativeSymbolKind.NamedType Then
                cur = DirectCast(cur, NativeNamedType).RealType
            ElseIf cur.Kind = NativeSymbolKind.TypedefType Then
                cur = DirectCast(cur, NativeTypeDef).RealType
            Else
                Exit While
            End If
        End While

        Return cur
    End Function

    Public Function DigThroughTypedefAndNamedTypesFor(ByVal search As String) As NativeType
        If 0 = String.CompareOrdinal(search, Me.Name) Then
            Return Me
        End If

        Dim cur As NativeType = Me
        While cur IsNot Nothing
            If cur.Kind = NativeSymbolKind.NamedType Then
                cur = DirectCast(cur, NativeNamedType).RealType
            ElseIf cur.Kind = NativeSymbolKind.TypedefType Then
                cur = DirectCast(cur, NativeTypeDef).RealType
            Else
                Exit While
            End If

            If cur IsNot Nothing AndAlso 0 = String.CompareOrdinal(cur.Name, search) Then
                Return cur
            End If
        End While

        Return Nothing
    End Function

End Class

#End Region

#Region "NativeDefinedType"

Public MustInherit Class NativeDefinedType
    Inherits NativeType

    Private m_isAnonymous As Boolean
    Private m_members As New List(Of NativeMember)

    ''' <summary>
    ''' Whether or not this type is anonymous
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsAnonymous() As Boolean
        Get
            Return m_isAnonymous
        End Get
        Set(ByVal value As Boolean)
            m_isAnonymous = value
        End Set
    End Property

    Public Overrides ReadOnly Property Category() As NativeSymbolCategory
        Get
            Return NativeSymbolCategory.Defined
        End Get
    End Property

    ''' <summary>
    ''' Members of the native type
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Members() As List(Of NativeMember)
        Get
            Return m_members
        End Get
    End Property

    Protected Sub New()

    End Sub

    Protected Sub New(ByVal name As String)
        Me.Name = name
    End Sub

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Dim list As New List(Of NativeSymbol)
        For Each member As NativeMember In Members
            list.Add(member)
        Next

        Return list
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        ReplaceChildInList(oldChild, newChild, m_members)
    End Sub

End Class

#End Region

#Region "Native Defined Types"

#Region "NativeStruct"

''' <summary>
''' Represents a C++ struct
''' </summary>
''' <remarks></remarks>
Public Class NativeStruct
    Inherits NativeDefinedType

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.StructType
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal name As String)
        MyBase.New(name)
    End Sub

End Class

#End Region

#Region "NativeUnion"

''' <summary>
''' Represents a C++ Union
''' </summary>
''' <remarks></remarks>
Public Class NativeUnion
    Inherits NativeDefinedType

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.UnionType
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal name As String)
        MyBase.New(name)
    End Sub

End Class
#End Region

#Region "NativeEnum"

''' <summary>
''' Native enum 
''' </summary>
''' <remarks></remarks>
Public Class NativeEnum
    Inherits NativeDefinedType

    Private m_list As New List(Of NativeEnumValue)

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.EnumType
        End Get
    End Property

    ''' <summary>
    ''' The values of the enum
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Values() As List(Of NativeEnumValue)
        Get
            Return m_list
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal name As String)
        Me.Name = name
    End Sub

    ''' <summary>
    ''' Enum's can't have members, just name value pairs
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Dim list As New List(Of NativeSymbol)
        For Each pair As NativeEnumValue In Me.Values
            list.Add(pair)
        Next

        Return list
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        ReplaceChildInList(oldChild, newChild, m_list)
    End Sub

End Class

''' <summary>
''' An enum value
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{Name} = {Value}")> _
Public Class NativeEnumValue
    Inherits NativeExtraSymbol

    Private m_value As NativeValueExpression

    ''' <summary>
    ''' Value of the value
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Value() As NativeValueExpression
        Get
            Return m_value
        End Get
        Set(ByVal value As NativeValueExpression)
            m_value = value
        End Set
    End Property

    Public Sub New(ByVal name As String)
        MyClass.New(name, String.Empty)
    End Sub

    Public Sub New(ByVal name As String, ByVal value As String)
        Me.Name = name
        m_value = New NativeValueExpression(value)
    End Sub

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.EnumNameValue
        End Get
    End Property

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Return GetSingleChild(m_value)
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        ReplaceChildSingle(oldChild, newChild, m_value)
    End Sub

End Class

#End Region

#Region "NativeFunctionPointer"

''' <summary>
''' Represents a native function pointer
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{DisplayName}")> _
Public Class NativeFunctionPointer
    Inherits NativeDefinedType

    Private m_sig As New NativeSignature
    Private m_conv As NativeCallingConvention = NativeCallingConvention.WinApi

    ''' <summary>
    ''' Get the signature of the function pointer
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Signature() As NativeSignature
        Get
            Return m_sig
        End Get
        Set(ByVal value As NativeSignature)
            m_sig = value
        End Set
    End Property

    Public Property CallingConvention() As NativeCallingConvention
        Get
            Return m_conv
        End Get
        Set(ByVal value As NativeCallingConvention)
            m_conv = value
        End Set
    End Property


    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.FunctionPointer
        End Get
    End Property

    Public Overrides ReadOnly Property DisplayName() As String
        Get
            Dim dispName As String = Name
            If NativeSymbolBag.IsAnonymousName(dispName) Then
                dispName = "anonymous"
            End If

            If Signature Is Nothing Then
                Return dispName
            End If

            Return Signature.CalculateSignature("(*" & dispName & ")")
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal name As String)
        Me.Name = name
    End Sub

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Return GetSingleChild(Signature)
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        ReplaceChildSingle(oldChild, newChild, m_sig)
    End Sub

End Class

#End Region

#End Region

#Region "NativeProxyType"

''' <summary>
''' Base class for proxy types.  That is types which are actually a simple modification on another
''' type.  This is typically name based such as typedefs or type based such as arrays and pointers
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{DisplayName}")> _
Public MustInherit Class NativeProxyType
    Inherits NativeType

    Private m_realType As NativeType

    ''' <summary>
    ''' Underlying type of the array
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property RealType() As NativeType
        Get
            Return m_realType
        End Get
        Set(ByVal value As NativeType)
            m_realType = value
        End Set
    End Property

    Public ReadOnly Property RealTypeDigged() As NativeType
        Get
            If m_realType IsNot Nothing Then
                Return m_realType.DigThroughTypedefAndNamedTypes()
            End If

            Return m_realType
        End Get
    End Property

    Public Overrides ReadOnly Property IsImmediateResolved() As Boolean
        Get
            Return m_realType IsNot Nothing
        End Get
    End Property

    Public Overrides ReadOnly Property Category() As NativeSymbolCategory
        Get
            Return NativeSymbolCategory.Proxy
        End Get
    End Property

    Protected Sub New(ByVal name As String)
        MyBase.New(name)
    End Sub

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Return GetSingleChild(RealType)
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        ReplaceChildSingle(oldChild, newChild, m_realType)
    End Sub

End Class

#End Region

#Region "Proxy Types"

#Region "NativeArray"

Public Class NativeArray
    Inherits NativeProxyType

    Private m_elementCount As Integer = -1

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.ArrayType
        End Get
    End Property

    ''' <summary>
    ''' Element count of the array.  If the array is not bound then this will
    ''' be -1
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ElementCount() As Integer
        Get
            Return m_elementCount
        End Get
        Set(ByVal value As Integer)
            m_elementCount = value
        End Set
    End Property

    ''' <summary>
    ''' Create the display name of the array
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property DisplayName() As String
        Get
            Dim suffix As String
            If m_elementCount >= 0 Then
                suffix = String.Format("[{0}]", Me.ElementCount)
            Else
                suffix = "[]"
            End If

            If RealType Is Nothing Then
                Return "<null>" & suffix
            Else
                Return RealType.DisplayName & suffix
            End If
        End Get
    End Property

    Public Sub New()
        MyBase.New("[]")
    End Sub

    Public Sub New(ByVal realType As NativeType, ByVal elementCount As Int32)
        MyBase.New("[]")
        Me.RealType = realType
        Me.ElementCount = elementCount
    End Sub

    Public Sub New(ByVal bt As BuiltinType, ByVal elementCount As Int32)
        MyClass.New(New NativeBuiltinType(bt), elementCount)
    End Sub

End Class
#End Region

#Region "NativePointer"

''' <summary>
''' A Pointer
''' </summary>
''' <remarks></remarks>
Public Class NativePointer
    Inherits NativeProxyType

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.PointerType
        End Get
    End Property

    ''' <summary>
    ''' Returs the pointer full type name
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property DisplayName() As String
        Get
            If RealType Is Nothing Then
                Return "<null>*"
            Else
                Return RealType.DisplayName & "*"
            End If
        End Get
    End Property

    Public Sub New()
        MyBase.New("*")
    End Sub

    Public Sub New(ByVal realtype As NativeType)
        MyBase.New("*")
        Me.RealType = realtype
    End Sub

    Public Sub New(ByVal bt As BuiltinType)
        MyBase.New("*")
        Me.RealType = New NativeBuiltinType(bt)
    End Sub

End Class
#End Region

#Region "NativeNamedType"

''' <summary>
''' Base type for Fake types
''' </summary>
''' <remarks></remarks>
Public Class NativeNamedType
    Inherits NativeProxyType

    Private m_qualification As String
    Private m_isConst As Boolean

    ''' <summary>
    ''' When a type is referenced by it's full name (struct, union, enum) this holds the reference 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Qualification() As String
        Get
            If m_qualification Is Nothing Then
                Return String.Empty
            End If
            Return m_qualification
        End Get
        Set(ByVal value As String)
            m_qualification = value
        End Set
    End Property

    ''' <summary>
    ''' Was this created with a "const" specifier?
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsConst() As Boolean
        Get
            Return m_isConst
        End Get
        Set(ByVal value As Boolean)
            m_isConst = value
        End Set
    End Property


    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.NamedType
        End Get
    End Property

    Public ReadOnly Property RealTypeFullName() As String
        Get
            If RealType IsNot Nothing Then
                Dim name As String
                If String.IsNullOrEmpty(m_qualification) Then
                    name = RealType.DisplayName
                Else
                    name = m_qualification & " " & RealType.DisplayName
                End If

                If IsConst Then
                    Return "const " & name
                End If

                Return name
            Else
                Return "<null>"
            End If
        End Get
    End Property

    Public Sub New(ByVal qualification As String, ByVal name As String)
        MyBase.New(name)
        m_qualification = qualification
    End Sub

    Public Sub New(ByVal name As String)
        MyBase.New(name)
    End Sub

    Public Sub New(ByVal name As String, ByVal isConst As Boolean)
        MyBase.New(name)
        m_isConst = isConst
    End Sub

    Public Sub New(ByVal qualification As String, ByVal name As String, ByVal isConst As Boolean)
        MyBase.New(name)
        m_qualification = qualification
        m_isConst = isConst
    End Sub

    Public Sub New(ByVal name As String, ByVal realType As NativeType)
        MyBase.New(name)
        Me.RealType = realType
    End Sub

End Class

#End Region

#Region "NativeTypeDef"

''' <summary>
''' TypeDef of a type.  At first it seems like this should be a NativeProxyType.  However 
''' NativeProxyTypes aren't really types.  They are just references or modifiers to a type.  A
''' Typedef is itself a type and accessible by name
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{FullName} -> {RealTypeFullname}")> _
Public Class NativeTypeDef
    Inherits NativeProxyType

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.TypedefType
        End Get
    End Property

    Public Sub New(ByVal name As String)
        MyBase.New(name)
    End Sub

    Public Sub New(ByVal name As String, ByVal realtypeName As String)
        MyBase.New(name)
        Me.RealType = New NativeNamedType(realtypeName)
    End Sub

    Public Sub New(ByVal name As String, ByVal realtype As NativeType)
        MyBase.New(name)
        Me.RealType = realtype
    End Sub

    Public Sub New(ByVal name As String, ByVal bt As BuiltinType)
        MyBase.New(name)
        Me.RealType = New NativeBuiltinType(bt)
    End Sub

End Class

#End Region

#End Region

#Region "Specialized Types"

''' <summary>
''' Types that are specialized for generation
''' </summary>
''' <remarks></remarks>
Public MustInherit Class NativeSpecializedType
    Inherits NativeType

    Public Overrides ReadOnly Property Category() As NativeSymbolCategory
        Get
            Return NativeSymbolCategory.Specialized
        End Get
    End Property

    Protected Sub New()

    End Sub

    Protected Sub New(ByVal name As String)
        MyBase.New(name)
    End Sub
End Class

#Region "NativeBitVector"

''' <summary>
''' A native bit vector.  All bitvectors are generated as anonymous structs inside the 
''' conttaining generated struct
''' </summary>
''' <remarks></remarks>
Public Class NativeBitVector
    Inherits NativeSpecializedType

    Private m_size As Integer

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.BitVectorType
        End Get
    End Property

    ''' <summary>
    ''' Size of the bitvector
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Size() As Integer
        Get
            Return m_size
        End Get
        Set(ByVal value As Integer)
            m_size = value
        End Set
    End Property

    Public Overrides ReadOnly Property DisplayName() As String
        Get
            Return "<bitvector " & Size & ">"
        End Get
    End Property

    Public Sub New()
        MyClass.New(-1)
    End Sub

    Public Sub New(ByVal size As Integer)
        Me.Name = "<bitvector>"
        m_size = size
    End Sub

End Class

#End Region

#Region "NativeBuiltinType"

''' <summary>
''' Enumeration of the common C++ builtin types
''' </summary>
''' <remarks></remarks>
Public Enum BuiltinType
    NativeInt16
    NativeInt32
    NativeInt64
    NativeFloat
    NativeDouble
    NativeBoolean
    NativeChar
    NativeWChar
    NativeByte
    NativeVoid

    ''' <summary>
    ''' Used for BuiltinTypes initially missed
    ''' </summary>
    ''' <remarks></remarks>
    NativeUnknown
End Enum

''' <summary>
''' Built-in types (int, boolean, etc ...)
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{DisplayName}")> _
Public Class NativeBuiltinType
    Inherits NativeSpecializedType

    Private Shared m_lookupMap As Dictionary(Of String, NativeBuiltinType)

    Private m_builtinType As BuiltinType
    Private m_isUnsigned As Boolean
    Private m_managedType As Type
    Private m_unmanagedType As System.Runtime.InteropServices.UnmanagedType

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.BuiltinType
        End Get
    End Property

    Public Overrides ReadOnly Property DisplayName() As String
        Get
            If m_builtinType = PInvoke.BuiltinType.NativeUnknown Then
                Return Name
            End If

            Dim str As String = Name
            If IsUnsigned Then
                str = "unsigned " & str
            End If

            Return str
        End Get
    End Property

    ''' <summary>
    ''' Bulitin Type
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property BuiltinType() As BuiltinType
        Get
            Return m_builtinType
        End Get
        Set(ByVal value As BuiltinType)
            m_builtinType = value
        End Set
    End Property

    Public Property IsUnsigned() As Boolean
        Get
            Return m_isUnsigned
        End Get
        Set(ByVal value As Boolean)
            m_isUnsigned = value
            Init()
        End Set
    End Property

    Public ReadOnly Property ManagedType() As Type
        Get
            Return m_managedType
        End Get
    End Property

    Public ReadOnly Property UnmanagedType() As Runtime.InteropServices.UnmanagedType
        Get
            Return m_unmanagedType
        End Get
    End Property

    Public Sub New(ByVal bt As BuiltinType)
        MyBase.New("")
        m_builtinType = bt
        Init()
    End Sub

    Public Sub New(ByVal bt As BuiltinType, ByVal isUnsigned As Boolean)
        MyClass.New(bt)
        Me.IsUnsigned = isUnsigned
        Init()
    End Sub

    Public Sub New(ByVal name As String)
        MyBase.New(name)
        m_builtinType = PInvoke.BuiltinType.NativeUnknown
        Init()
    End Sub

    Private Sub Init()
        Select Case Me.BuiltinType
            Case PInvoke.BuiltinType.NativeBoolean
                Name = "boolean"
                m_managedType = GetType(Boolean)
                m_unmanagedType = Runtime.InteropServices.UnmanagedType.Bool
            Case PInvoke.BuiltinType.NativeByte
                Name = "byte"
                m_managedType = GetType(Byte)
                m_unmanagedType = Runtime.InteropServices.UnmanagedType.I1
            Case PInvoke.BuiltinType.NativeInt16
                Name = "short"
                If IsUnsigned Then
                    m_managedType = GetType(UInt16)
                    m_unmanagedType = Runtime.InteropServices.UnmanagedType.U2
                Else
                    m_managedType = GetType(Int16)
                    m_unmanagedType = Runtime.InteropServices.UnmanagedType.I2
                End If
            Case PInvoke.BuiltinType.NativeInt32
                Name = "int"
                If IsUnsigned Then
                    m_managedType = GetType(UInt32)
                    m_unmanagedType = Runtime.InteropServices.UnmanagedType.U4
                Else
                    m_managedType = GetType(Int32)
                    m_unmanagedType = Runtime.InteropServices.UnmanagedType.I4
                End If
            Case PInvoke.BuiltinType.NativeInt64
                Name = "__int64"
                If IsUnsigned Then
                    m_managedType = GetType(UInt64)
                    m_unmanagedType = Runtime.InteropServices.UnmanagedType.U8
                Else
                    m_managedType = GetType(Int64)
                    m_unmanagedType = Runtime.InteropServices.UnmanagedType.I8
                End If
            Case PInvoke.BuiltinType.NativeChar
                Name = "char"
                m_managedType = GetType(Byte)
                m_unmanagedType = Runtime.InteropServices.UnmanagedType.I1
            Case PInvoke.BuiltinType.NativeWChar
                Name = "wchar"
                m_managedType = GetType(Char)
                m_unmanagedType = Runtime.InteropServices.UnmanagedType.I2
            Case PInvoke.BuiltinType.NativeFloat
                Name = "float"
                m_managedType = GetType(Single)
                m_unmanagedType = Runtime.InteropServices.UnmanagedType.R4
            Case PInvoke.BuiltinType.NativeDouble
                Name = "double"
                m_managedType = GetType(Double)
                m_unmanagedType = Runtime.InteropServices.UnmanagedType.R8
            Case PInvoke.BuiltinType.NativeVoid
                Name = "void"
                m_managedType = GetType(Void)
                m_unmanagedType = Runtime.InteropServices.UnmanagedType.AsAny
            Case PInvoke.BuiltinType.NativeUnknown
                Name = "unknown"
                m_managedType = GetType(Object)
                m_unmanagedType = Runtime.InteropServices.UnmanagedType.AsAny
            Case Else
                InvalidEnumValue(BuiltinType)
        End Select

    End Sub

    Public Shared Function TryConvertToBuiltinType(ByVal name As String, ByRef nativeBt As NativeBuiltinType) As Boolean
        Dim tt As Parser.TokenType
        If Parser.TokenHelper.KeywordMap.TryGetValue(name, tt) Then
            Return TryConvertToBuiltinType(tt, nativeBt)
        End If

        Return False
    End Function

    Public Shared Function TryConvertToBuiltinType(ByVal tt As Parser.TokenType, ByRef nativeBt As NativeBuiltinType) As Boolean
        If Not Parser.TokenHelper.IsTypeKeyword(tt) Then
            Return False
        End If

        Dim bt As BuiltinType
        Dim isUnsigned As Boolean = False
        Select Case tt
            Case Parser.TokenType.BooleanKeyword
                bt = PInvoke.BuiltinType.NativeBoolean
            Case Parser.TokenType.ByteKeyword
                bt = PInvoke.BuiltinType.NativeByte
            Case Parser.TokenType.ShortKeyword, Parser.TokenType.Int16Keyword
                bt = PInvoke.BuiltinType.NativeInt16
            Case Parser.TokenType.IntKeyword, Parser.TokenType.LongKeyword, Parser.TokenType.SignedKeyword
                bt = PInvoke.BuiltinType.NativeInt32
            Case Parser.TokenType.UnsignedKeyword
                bt = PInvoke.BuiltinType.NativeInt32
                isUnsigned = True
            Case Parser.TokenType.Int64Keyword
                bt = PInvoke.BuiltinType.NativeInt64
            Case Parser.TokenType.CharKeyword
                bt = PInvoke.BuiltinType.NativeChar
            Case Parser.TokenType.WCharKeyword
                bt = PInvoke.BuiltinType.NativeWChar
            Case Parser.TokenType.FloatKeyword
                bt = PInvoke.BuiltinType.NativeFloat
            Case Parser.TokenType.DoubleKeyword
                bt = PInvoke.BuiltinType.NativeDouble
            Case Parser.TokenType.VoidKeyword
                bt = PInvoke.BuiltinType.NativeVoid
            Case Else
                bt = PInvoke.BuiltinType.NativeUnknown
                InvalidEnumValue(tt)
        End Select

        nativeBt = New NativeBuiltinType(bt, isUnsigned)
        Return True
    End Function

    Public Shared Function BuiltinTypeToName(ByVal bt As BuiltinType) As String
        Dim nativeBt As New NativeBuiltinType(bt)
        Return nativeBt.Name
    End Function

    Public Shared Function IsNumberType(ByVal bt As BuiltinType) As Boolean
        If bt = PInvoke.BuiltinType.NativeInt16 _
            OrElse bt = PInvoke.BuiltinType.NativeInt32 _
            OrElse bt = PInvoke.BuiltinType.NativeInt64 _
            OrElse bt = PInvoke.BuiltinType.NativeFloat _
            OrElse bt = PInvoke.BuiltinType.NativeDouble _
            OrElse bt = PInvoke.BuiltinType.NativeByte Then

            Return True
        End If

        Return False
    End Function

End Class

#End Region

#Region "NativeOpaqueType"

''' <summary>
''' Represents a type that is intentionally being hidden from the user.  Usually takes the following form
''' typedef struct UndefinedType *PUndefinedType
''' 
''' PUndefinedType is a legal pointer reference and the struct "foo" can later be defined in a .c/.cpp file
''' </summary>
''' <remarks></remarks>
Public Class NativeOpaqueType
    Inherits NativeSpecializedType

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.OpaqueType
        End Get
    End Property

    Public Sub New()
        MyBase.New("Opaque")
    End Sub
End Class

#End Region

#End Region

#Region "NativeProcedure"

Public Enum NativeCallingConvention
    ''' <summary>
    ''' Platform default
    ''' </summary>
    ''' <remarks></remarks>
    WinApi = 1

    ''' <summary>
    ''' __stdcall
    ''' </summary>
    ''' <remarks></remarks>
    Standard

    ''' <summary>
    ''' __cdecl
    ''' </summary>
    ''' <remarks></remarks>
    CDeclaration

    ''' <summary>
    ''' __clrcall
    ''' </summary>
    ''' <remarks></remarks>
    Clr

    ''' <summary>
    ''' __pascal
    ''' </summary>
    ''' <remarks></remarks>
    Pascal

    ''' <summary>
    ''' inline, __inline, etc
    ''' </summary>
    ''' <remarks></remarks>
    Inline

End Enum

''' <summary>
''' Procedure symbol
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{DisplayName}")> _
Public Class NativeProcedure
    Inherits NativeSymbol

    Private m_dllName As String
    Private m_sig As New NativeSignature
    Private m_conv As NativeCallingConvention = NativeCallingConvention.WinApi

    ''' <summary>
    ''' Name of the DLL this proc is in
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DllName() As String
        Get
            Return m_dllName
        End Get
        Set(ByVal value As String)
            m_dllName = value
        End Set
    End Property

    ''' <summary>
    ''' Signature of the procedure
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Signature() As NativeSignature
        Get
            Return m_sig
        End Get
        Set(ByVal value As NativeSignature)
            m_sig = value
        End Set
    End Property

    Public Property CallingConvention() As NativeCallingConvention
        Get
            Return m_conv
        End Get
        Set(ByVal value As NativeCallingConvention)
            m_conv = value
        End Set
    End Property

    Public Overrides ReadOnly Property Category() As NativeSymbolCategory
        Get
            Return NativeSymbolCategory.Procedure
        End Get
    End Property

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.Procedure
        End Get
    End Property

    Public Overrides ReadOnly Property DisplayName() As String
        Get
            If Signature Is Nothing Then
                Return Name
            End If

            Return Signature.CalculateSignature(Me.Name)
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal name As String)
        MyBase.New(name)
    End Sub

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Return GetSingleChild(m_sig)
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        MyBase.ReplaceChildSingle(oldChild, newChild, m_sig)
    End Sub

End Class

#End Region

#Region "Extra Symbols"

Public MustInherit Class NativeExtraSymbol
    Inherits NativeSymbol

    Public Overrides ReadOnly Property Category() As NativeSymbolCategory
        Get
            Return NativeSymbolCategory.Extra
        End Get
    End Property

End Class

''' <summary>
''' A parameter to a procedure in native code
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{DisplayString}")> _
Public Class NativeParameter
    Inherits NativeExtraSymbol

    Private m_type As NativeType
    Private m_salAttribute As NativeSalAttribute = New NativeSalAttribute

    ''' <summary>
    ''' Type of the parameter
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NativeType() As NativeType
        Get
            Return m_type
        End Get
        Set(ByVal value As NativeType)
            m_type = value
        End Set
    End Property

    ''' <summary>
    ''' The SAL attribute for this parameter
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SalAttribute() As NativeSalAttribute
        Get
            Return m_salAttribute
        End Get
        Set(ByVal value As NativeSalAttribute)
            ThrowIfNull(value)
            m_salAttribute = value
        End Set
    End Property

    ''' <summary>
    ''' NativeType after digging through typedef and named types
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property NativeTypeDigged() As NativeType
        Get
            If m_type IsNot Nothing Then
                Return m_type.DigThroughTypedefAndNamedTypes()
            End If

            Return Nothing
        End Get
    End Property

    Public ReadOnly Property DisplayString() As String
        Get
            Dim str As String = String.Empty

            If NativeType IsNot Nothing Then
                str &= NativeType.DisplayName & " "
            End If

            str &= Me.Name
            Return str
        End Get
    End Property

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.Parameter
        End Get
    End Property

    ''' <summary>
    ''' A NativeParameter is resolved if it has a type.  
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property IsImmediateResolved() As Boolean
        Get
            Return m_type IsNot Nothing
        End Get
    End Property

    Public Sub New()
        Me.Name = String.Empty
    End Sub

    Public Sub New(ByVal name As String)
        Me.Name = name
    End Sub

    Public Sub New(ByVal name As String, ByVal type As NativeType)
        Me.Name = name
        m_type = type
    End Sub

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Dim list As New List(Of NativeSymbol)
        If m_type IsNot Nothing Then
            list.Add(m_type)
        End If

        If m_salAttribute IsNot Nothing Then
            list.Add(m_salAttribute)
        End If

        Return list
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        If Object.ReferenceEquals(oldChild, m_type) Then
            ReplaceChildSingle(oldChild, newChild, m_type)
        Else
            ReplaceChildSingle(oldChild, newChild, m_salAttribute)
        End If
    End Sub

End Class

''' <summary>
''' Represents a member of a native type.
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{NativeType.FullName} {Name}")> _
Public Class NativeMember
    Inherits NativeExtraSymbol

    Private m_nativeType As NativeType

    ''' <summary>
    ''' Nativetype of the member
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NativeType() As NativeType
        Get
            Return m_nativeType
        End Get
        Set(ByVal value As NativeType)
            m_nativeType = value
        End Set
    End Property

    Public ReadOnly Property NativeTypeDigged() As NativeType
        Get
            If m_nativeType IsNot Nothing Then
                Return m_nativeType.DigThroughTypedefAndNamedTypes()
            End If

            Return Nothing
        End Get
    End Property

    Public Overrides ReadOnly Property IsImmediateResolved() As Boolean
        Get
            Return m_nativeType IsNot Nothing AndAlso Not String.IsNullOrEmpty(Name)
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal name As String, ByVal nt As NativeType)
        Me.Name = name
        m_nativeType = nt
    End Sub

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.Member
        End Get
    End Property

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Return GetSingleChild(m_nativeType)
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        ReplaceChildSingle(oldChild, newChild, m_nativeType)
    End Sub

End Class

Public Enum ConstantKind
    Macro
    MacroMethod
End Enum

''' <summary>
''' Constant in Native code
''' </summary>
''' <remarks></remarks>
Public Class NativeConstant
    Inherits NativeExtraSymbol
    Private m_value As NativeValueExpression
    Private m_constantKind As ConstantKind

    ''' <summary>
    ''' What type of constant is this
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ConstantKind() As ConstantKind
        Get
            Return m_constantKind
        End Get
        Set(ByVal value As ConstantKind)
            m_constantKind = value
        End Set
    End Property

    ''' <summary>
    ''' Value for the constant
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Value() As NativeValueExpression
        Get
            Return m_value
        End Get
        Set(ByVal value As NativeValueExpression)
            m_value = value
        End Set
    End Property

    Public ReadOnly Property RawValue() As String
        Get
            If m_value Is Nothing Then
                Return String.Empty
            End If

            Return m_value.Expression
        End Get
    End Property

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.Constant
        End Get
    End Property

    Private Sub New()

    End Sub

    Public Sub New(ByVal name As String)
        MyClass.New(name, Nothing)
    End Sub

    Public Sub New(ByVal name As String, ByVal value As String)
        MyClass.New(name, value, PInvoke.ConstantKind.Macro)
    End Sub

    Public Sub New(ByVal name As String, ByVal value As String, ByVal kind As ConstantKind)
        If name Is Nothing Then : Throw New ArgumentNullException("name") : End If

        Me.Name = name
        m_constantKind = kind

        ' We don't support macro methods at this point.  Instead we will just generate out the 
        ' method signature for the method and print the string out into the code
        If ConstantKind = PInvoke.ConstantKind.MacroMethod Then
            value = """" & value & """"
        End If

        m_value = New NativeValueExpression(value)
    End Sub

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Return GetSingleChild(m_value)
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        ReplaceChildSingle(oldChild, newChild, m_value)
    End Sub

End Class

''' <summary>
''' Represents the value of an experession
''' </summary>
''' <remarks></remarks>
Public Class NativeValueExpression
    Inherits NativeExtraSymbol

    Private m_expression As String
    Private m_valueList As List(Of NativeValue)
    Private m_node As Parser.ExpressionNode
    Private m_errorParsingExpr As Boolean = False

    ''' <summary>
    ''' Value of the expression
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Expression() As String
        Get
            Return m_expression
        End Get
        Set(ByVal value As String)
            ResetValueList()
            m_expression = value
        End Set
    End Property

    Public ReadOnly Property IsParsable() As Boolean
        Get
            EnsureValueList()
            Return Not m_errorParsingExpr
        End Get
    End Property

    ''' <summary>
    ''' Is this an empty expression
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IsEmpty() As Boolean
        Get
            Return String.IsNullOrEmpty(m_expression)
        End Get
    End Property

    ''' <summary>
    ''' Root expression node
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Node() As Parser.ExpressionNode
        Get
            EnsureValueList()
            Return m_node
        End Get
    End Property

    ''' <summary>
    ''' List of values in the expression
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Values() As List(Of NativeValue)
        Get
            EnsureValueList()
            Return m_valueList
        End Get
    End Property

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.ValueExpression
        End Get
    End Property

    Public Sub New(ByVal expr As String)
        Me.Name = "Value"
        m_expression = expr
    End Sub

    Private Sub ResetValueList()
        m_valueList = Nothing
        m_node = Nothing
    End Sub

    Private Sub EnsureValueList()
        If m_valueList IsNot Nothing Then
            Return
        End If

        If IsEmpty Then
            m_valueList = New List(Of NativeValue)()
            m_errorParsingExpr = False
            Return
        End If

        Dim parser As New Parser.ExpressionParser()
        m_valueList = New List(Of NativeValue)()

        ' It's valid no have an invalid expression :)
        If Not parser.TryParse(m_expression, m_node) Then
            m_errorParsingExpr = True
            m_node = Nothing
        Else
            m_errorParsingExpr = False
        End If

        CalculateValueList(m_node)
    End Sub

    Private Sub CalculateValueList(ByVal cur As Parser.ExpressionNode)
        If cur Is Nothing Then
            Return
        End If

        If cur.Kind = Parser.ExpressionKind.Leaf Then
            Dim token As Parser.Token = cur.Token
            Dim ntVal As NativeValue = Nothing
            If token.IsQuotedString Then
                Dim strValue As String = Nothing
                If Parser.TokenHelper.TryConvertToString(token, strValue) Then
                    ntVal = NativeValue.CreateString(strValue)
                End If
            ElseIf token.IsNumber Then
                Dim value As Object = Nothing
                If Parser.TokenHelper.TryConvertToNumber(token, value) Then
                    ntVal = NativeValue.CreateNumber(value)
                End If
            ElseIf token.IsCharacter Then
                Dim cValue As Char = "c"c
                If Parser.TokenHelper.TryConvertToChar(token, cValue) Then
                    ntVal = NativeValue.CreateCharacter(cValue)
                Else
                    ntVal = NativeValue.CreateString(token.Value)
                End If
            ElseIf token.TokenType = Parser.TokenType.TrueKeyword Then
                ntVal = NativeValue.CreateBoolean(True)
            ElseIf token.TokenType = Parser.TokenType.FalseKeyword Then
                ntVal = NativeValue.CreateBoolean(False)
            ElseIf token.IsAnyWord Then
                ntVal = NativeValue.CreateSymbolValue(token.Value)
            End If

            If ntVal IsNot Nothing Then
                m_valueList.Add(ntVal)
                cur.Tag = ntVal
            Else
                m_errorParsingExpr = True
            End If
        ElseIf cur.Kind = Parser.ExpressionKind.Cast Then
            ' Create nodes for the cast expressions.  The target should be a symbol
            m_valueList.Add(NativeValue.CreateSymbolType(cur.Token.Value))
        End If

            CalculateValueList(cur.LeftNode)
            CalculateValueList(cur.RightNode)
    End Sub

    ''' <summary>
    ''' A Native value expression is resolved.  It may output as an error string but it will output
    ''' a value.  This is needed to support constants that are defined to non-valid code but we still
    ''' have to output the string value
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property IsImmediateResolved() As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        EnsureValueList()
        Return MyBase.GetListChild(m_valueList)
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        EnsureValueList()
        MyBase.ReplaceChildInList(oldChild, newChild, m_valueList)
    End Sub


End Class

Public Enum NativeValueKind
    Number
    [String]
    Character
    [Boolean]

    ''' <summary>
    ''' Used when the value needs a Symbol which represents a Value
    ''' </summary>
    ''' <remarks></remarks>
    SymbolValue

    ''' <summary>
    ''' Used when the value needs a Symbol which represents a Type.  For instance
    ''' a Cast expression needs a Type Symbol rather than a Value symbol
    ''' </summary>
    ''' <remarks></remarks>
    SymbolType
End Enum

''' <summary>
''' Represents a value inside of an expression
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{Value} ({ValueKind})")> _
Public Class NativeValue
    Inherits NativeExtraSymbol

    Private m_valueKind As NativeValueKind
    Private m_value As Object

    ''' <summary>
    ''' The actual value
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Value() As Object
        Get
            Return m_value
        End Get
        Set(ByVal value As Object)
            m_value = value
        End Set
    End Property

    Public ReadOnly Property SymbolValue() As NativeSymbol
        Get
            If (m_valueKind = NativeValueKind.SymbolValue) Then
                Return DirectCast(m_value, NativeSymbol)
            End If

            Return Nothing
        End Get
    End Property

    Public ReadOnly Property SymbolType() As NativeSymbol
        Get
            If (m_valueKind = NativeValueKind.SymbolType) Then
                Return DirectCast(m_value, NativeSymbol)
            End If

            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' What kind of value is this
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ValueKind() As NativeValueKind
        Get
            Return m_valueKind
        End Get
    End Property

    ''' <summary>
    ''' Is the value resolvable
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IsValueResolved() As Boolean
        Get
            Select Case Me.ValueKind
                Case NativeValueKind.Number, NativeValueKind.String, NativeValueKind.Character, NativeValueKind.Boolean
                    Return Me.m_value IsNot Nothing
                Case NativeValueKind.SymbolType
                    Return SymbolType IsNot Nothing
                Case NativeValueKind.SymbolValue
                    Return SymbolValue IsNot Nothing
                Case Else
                    InvalidEnumValue(Me.ValueKind)
                    Return False
            End Select
        End Get
    End Property

    ''' <summary>
    ''' Get the value as a display string
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property DisplayValue() As String
        Get
            Select Case m_valueKind
                Case NativeValueKind.Number
                    Return m_value.ToString()
                Case NativeValueKind.String
                    Return m_value.ToString()
                Case NativeValueKind.Character
                    Return m_value.ToString()
                Case NativeValueKind.SymbolType
                    If SymbolType IsNot Nothing Then
                        Return SymbolType.DisplayName
                    End If

                    Return Name
                Case NativeValueKind.SymbolValue
                    If SymbolValue IsNot Nothing Then
                        Return SymbolValue.DisplayName
                    End If

                    Return Name
                Case Else
                    InvalidEnumValue(m_valueKind)
                    Return String.Empty
            End Select
        End Get
    End Property

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.Value
        End Get
    End Property

    Private Sub New(ByVal value As Object, ByVal kind As NativeValueKind)
        MyClass.New(kind.ToString(), value, kind)
    End Sub

    Private Sub New(ByVal name As String, ByVal value As Object, ByVal kind As NativeValueKind)
        Me.Name = name
        m_valueKind = kind
        m_value = value
    End Sub

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        If m_valueKind = NativeValueKind.SymbolType Then
            Return GetSingleChild(SymbolType)
        ElseIf m_valueKind = NativeValueKind.SymbolValue Then
            Return GetSingleChild(SymbolValue)
        Else
            Return GetSingleChild(Of NativeSymbol)(Nothing)
        End If
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        If m_valueKind = NativeValueKind.SymbolType Then
            Dim x As NativeSymbol = Nothing
            ReplaceChildSingle(SymbolType, newChild, x)
            Value = x
        ElseIf m_valueKind = NativeValueKind.SymbolValue Then
            Dim x As NativeSymbol = Nothing
            ReplaceChildSingle(SymbolValue, newChild, x)
            Value = x
        End If
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Public Shared Function CreateNumber(ByVal o As Object) As NativeValue
        Return New NativeValue(o, NativeValueKind.Number)
    End Function

    Public Shared Function CreateBoolean(ByVal b As Boolean) As NativeValue
        Return New NativeValue(b, NativeValueKind.Boolean)
    End Function

    Public Shared Function CreateString(ByVal s As String) As NativeValue
        Return New NativeValue(s, NativeValueKind.String)
    End Function

    Public Shared Function CreateCharacter(ByVal c As Char) As NativeValue
        Return New NativeValue(c, NativeValueKind.Character)
    End Function

    Public Shared Function CreateSymbolValue(ByVal name As String) As NativeValue
        Return New NativeValue(name, Nothing, NativeValueKind.SymbolValue)
    End Function

    Public Shared Function CreateSymbolValue(ByVal name As String, ByVal ns As NativeSymbol) As NativeValue
        Return New NativeValue(name, ns, NativeValueKind.SymbolValue)
    End Function

    Public Shared Function CreateSymbolType(ByVal name As String) As NativeValue
        Return New NativeValue(name, Nothing, NativeValueKind.SymbolType)
    End Function

    Public Shared Function CreateSymbolType(ByVal name As String, ByVal ns As NativeSymbol) As NativeValue
        Return New NativeValue(name, ns, NativeValueKind.SymbolType)
    End Function

End Class

#Region "SAL attributes"

<Flags()> _
Public Enum SalEntryType
    [Null]
    NotNull
    MaybeNull
    [ReadOnly]
    NotReadOnly
    MaybeReadOnly
    Valid
    NotValid
    MaybeValid
    ReadableTo
    ElemReadableTo
    ByteReadableTo
    WritableTo
    ElemWritableTo
    ByteWritableTo
    Deref
    Pre
    Post
    ExceptThat
    InnerControlEntryPoint
    InnerDataEntryPoint
    InnerSucces
    InnerCheckReturn
    InnerTypefix
    InnerOverride
    InnerCallBack
    InnerBlocksOn
End Enum

''' <summary>
''' Represents a SAL attribute in code
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{DisplayName}")> _
Public Class NativeSalEntry
    Inherits NativeExtraSymbol

    Private m_type As SalEntryType
    Private m_text As String

    ''' <summary>
    ''' Type of attribute
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SalEntryType() As SalEntryType
        Get
            Return m_type
        End Get
        Set(ByVal value As SalEntryType)
            m_type = value
            Me.Name = value.ToString()
        End Set
    End Property

    ''' <summary>
    ''' Text of the attribute
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Text() As String
        Get
            Return m_text
        End Get
        Set(ByVal value As String)
            m_text = value
        End Set
    End Property

    Public Overrides ReadOnly Property DisplayName() As String
        Get
            If String.IsNullOrEmpty(Text) Then
                Return Name
            Else
                Return String.Format("{0}({1})", Name, Text)
            End If
        End Get
    End Property

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.SalEntry
        End Get
    End Property

    Public Sub New()
        MyClass.New(SalEntryType.Null, String.Empty)

    End Sub

    Public Sub New(ByVal type As SalEntryType)
        MyClass.New(type, String.Empty)
    End Sub

    Public Sub New(ByVal type As SalEntryType, ByVal text As String)
        Me.SalEntryType = type
        m_type = type
        m_text = text
    End Sub

    Public Shared Function GetDirectiveForEntry(ByVal entry As SalEntryType) As String
        Select Case entry
            Case PInvoke.SalEntryType.Null
                Return "SAL_null"
            Case PInvoke.SalEntryType.NotNull
                Return "SAL_notnull"
            Case PInvoke.SalEntryType.MaybeNull
                Return "SAL_maybenull"
            Case PInvoke.SalEntryType.ReadOnly
                Return "SAL_readonly"
            Case PInvoke.SalEntryType.NotReadOnly
                Return "SAL_notreadonly"
            Case PInvoke.SalEntryType.MaybeReadOnly
                Return "SAL_maybereadonly"
            Case PInvoke.SalEntryType.Valid
                Return "SAL_valid"
            Case PInvoke.SalEntryType.NotValid
                Return "SAL_notvalid"
            Case PInvoke.SalEntryType.MaybeValid
                Return "SAL_maybevalid"
            Case PInvoke.SalEntryType.ReadableTo
                Return "SAL_readableTo()"
            Case PInvoke.SalEntryType.ElemReadableTo
                Return "SAL_readableTo(elementCount())"
            Case PInvoke.SalEntryType.ByteReadableTo
                Return "SAL_readableTo(byteCount())"
            Case PInvoke.SalEntryType.WritableTo
                Return "SAL_writableTo()"
            Case PInvoke.SalEntryType.ElemWritableTo
                Return "SAL_writableTo(elementCount())"
            Case PInvoke.SalEntryType.ByteWritableTo
                Return "SAL_writableTo(byteCount())"
            Case PInvoke.SalEntryType.Deref
                Return "SAL_deref"
            Case PInvoke.SalEntryType.Pre
                Return "SAL_pre"
            Case PInvoke.SalEntryType.Post
                Return "SAL_post"
            Case PInvoke.SalEntryType.ExceptThat
                Return "SAL_except"
            Case PInvoke.SalEntryType.InnerControlEntryPoint
                Return "SAL_entrypoint(controlEntry, )"
            Case PInvoke.SalEntryType.InnerDataEntryPoint
                Return "SAL_entrypoint(dataEntry, )"
            Case PInvoke.SalEntryType.InnerSucces
                Return "SAL_success()"
            Case PInvoke.SalEntryType.InnerCheckReturn
                Return "SAL_checkReturn"
            Case PInvoke.SalEntryType.InnerTypefix
                Return "SAL_typefix"
            Case PInvoke.SalEntryType.InnerOverride
                Return "__override"
            Case PInvoke.SalEntryType.InnerCallBack
                Return "__callback"
            Case PInvoke.SalEntryType.InnerBlocksOn
                Return "SAL_blocksOn()"
            Case Else
                InvalidEnumValue(entry)
                Return String.Empty
        End Select
    End Function

End Class

''' <summary>
''' Represents the collection of SAL attributes
''' </summary>
''' <remarks></remarks>
<DebuggerDisplay("{DisplayName}")> _
Public Class NativeSalAttribute
    Inherits NativeExtraSymbol

    Private m_list As New List(Of NativeSalEntry)

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.SalAttribute
        End Get
    End Property

    ''' <summary>
    ''' List of attribute entries
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property SalEntryList() As List(Of NativeSalEntry)
        Get
            Return m_list
        End Get
    End Property

    ''' <summary>
    ''' True if there are no entries in the attribute
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IsEmpty() As Boolean
        Get
            Return m_list.Count = 0
        End Get
    End Property

    Public Overrides ReadOnly Property DisplayName() As String
        Get
            Dim builder As New StringBuilder()
            Dim isFirst As Boolean = True
            For Each entry As NativeSalEntry In m_list
                If Not isFirst Then
                    builder.Append(",")
                End If

                isFirst = False
                builder.Append(entry.DisplayName)
            Next
            Return builder.ToString()
        End Get
    End Property

    Public Sub New()
        Me.Name = "Sal"
    End Sub

    Public Sub New(ByVal ParamArray entryList As SalEntryType())
        MyClass.New()
        For Each entry As SalEntryType In entryList
            m_list.Add(New NativeSalEntry(entry))
        Next
    End Sub

    Public Sub New(ByVal ParamArray entryList As NativeSalEntry())
        MyClass.New()
        m_list.AddRange(entryList)
    End Sub

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Dim list As New List(Of NativeSymbol)
        For Each entry As NativeSalEntry In m_list
            list.Add(entry)
        Next

        Return list
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        MyBase.ReplaceChildInList(oldChild, newChild, m_list)
    End Sub

End Class

#End Region

#Region "NativeProcedureSignature"

Public Class NativeSignature
    Inherits NativeExtraSymbol

    Private m_returnType As NativeType
    Private m_returnTypeSalAttribute As NativeSalAttribute = New NativeSalAttribute()
    Private m_paramList As New List(Of NativeParameter)

    ''' <summary>
    ''' Return type of the NativeProcedure
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ReturnType() As NativeType
        Get
            Return m_returnType
        End Get
        Set(ByVal value As NativeType)
            m_returnType = value
        End Set
    End Property

    ''' <summary>
    ''' SAL attribute on the return type of the procedure
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ReturnTypeSalAttribute() As NativeSalAttribute
        Get
            Return m_returnTypeSalAttribute
        End Get
        Set(ByVal value As NativeSalAttribute)
            m_returnTypeSalAttribute = value
        End Set
    End Property

    ''' <summary>
    ''' Parameters of the procedure
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Parameters() As List(Of NativeParameter)
        Get
            Return m_paramList
        End Get
    End Property

    Public Overrides ReadOnly Property DisplayName() As String
        Get
            Return CalculateSignature()
        End Get
    End Property

    Public Overrides ReadOnly Property Category() As NativeSymbolCategory
        Get
            Return NativeSymbolCategory.Procedure
        End Get
    End Property

    Public Overrides ReadOnly Property Kind() As NativeSymbolKind
        Get
            Return NativeSymbolKind.ProcedureSignature
        End Get
    End Property

    Public Sub New()
        Me.Name = "Sig"
    End Sub

    Public Function CalculateSignature(Optional ByVal name As String = Nothing, Optional ByVal includeSal As Boolean = False) As String
        Dim builder As New StringBuilder()

        If includeSal AndAlso Not ReturnTypeSalAttribute.IsEmpty Then
            builder.Append(ReturnTypeSalAttribute.DisplayName)
            builder.Append(" ")
        End If

        If ReturnType IsNot Nothing Then
            builder.Append(ReturnType.DisplayName)
            builder.Append(" ")
        End If

        If Not String.IsNullOrEmpty(name) Then
            builder.Append(name)
        End If

        builder.Append("(")

        For i As Integer = 0 To m_paramList.Count - 1
            If i > 0 Then
                builder.Append(", ")
            End If

            Dim cur As NativeParameter = m_paramList(i)
            If includeSal AndAlso Not cur.SalAttribute.IsEmpty Then
                builder.Append(cur.SalAttribute.DisplayName)
                builder.Append(" ")
            End If

            If String.IsNullOrEmpty(cur.Name) Then
                builder.Append(cur.NativeType.DisplayName)
            Else
                builder.AppendFormat("{0} {1}", cur.NativeType.DisplayName, cur.Name)
            End If

        Next

        builder.Append(")")
        Return builder.ToString()
    End Function

    Public Overrides Function GetChildren() As System.Collections.Generic.IEnumerable(Of NativeSymbol)
        Dim list As New List(Of NativeSymbol)

        If m_returnType IsNot Nothing Then
            list.Add(m_returnType)
        End If

        If m_returnTypeSalAttribute IsNot Nothing Then
            list.Add(m_returnTypeSalAttribute)
        End If

        For Each param As NativeParameter In m_paramList
            list.Add(param)
        Next

        Return list
    End Function

    Public Overrides Sub ReplaceChild(ByVal oldChild As NativeSymbol, ByVal newChild As NativeSymbol)
        If Object.ReferenceEquals(oldChild, m_returnType) Then
            ReplaceChildSingle(oldChild, newChild, m_returnType)
        ElseIf Object.ReferenceEquals(oldChild, m_returnTypeSalAttribute) Then
            ReplaceChildSingle(oldChild, newChild, m_returnTypeSalAttribute)
        Else
            ReplaceChildInList(oldChild, newChild, m_paramList)
        End If
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

#End Region

#End Region

