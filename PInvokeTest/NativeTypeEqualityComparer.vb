' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.Collections.Generic
Imports PInvoke

Friend Class TypePair(Of K, V)
    Public Key As K
    Public Value As V
End Class

''' <summary>
''' Used to compare NativeType trees for equality
''' </summary>
''' <remarks></remarks>
Public Class NativeTypeEqualityComparer
    Implements IEqualityComparer(Of NativeType)

    ''' <summary>
    ''' Type of comparer
    ''' </summary>
    ''' <remarks></remarks>
    Private Enum ComparerType
        TopLevel
        Recursive
    End Enum

    Public Shared ReadOnly Property TopLevel() As NativeTypeEqualityComparer
        Get
            Return New NativeTypeEqualityComparer(ComparerType.TopLevel)
        End Get
    End Property

    Public Shared ReadOnly Property Recursive() As NativeTypeEqualityComparer
        Get
            Return New NativeTypeEqualityComparer(ComparerType.Recursive)
        End Get
    End Property

    Private _type As ComparerType

    Private Sub New(ByVal type As ComparerType)
        _type = type
    End Sub

#Region "Shared Methods"

    Public Shared Function AreEqualRecursive(ByVal left As NativeType, ByVal right As NativeType) As Boolean
        Return Recursive.Equals1(left, right)
    End Function

    Public Shared Function AreEqualTopLevel(ByVal left As NativeType, ByVal right As NativeType) As Boolean
        Return TopLevel.Equals1(left, right)
    End Function

#End Region

#Region "IEqualityComparer"

    Public Function Equals1(ByVal x As NativeType, ByVal y As NativeType) As Boolean Implements System.Collections.Generic.IEqualityComparer(Of NativeType).Equals

        ' Standard null checks
        If x Is Nothing OrElse y Is Nothing Then
            Return x Is Nothing AndAlso y Is Nothing
        End If

        Select Case _type
            Case ComparerType.Recursive
                Return EqualsRecursive(x, y)
            Case ComparerType.TopLevel
                Return EqualsTopLevel(x, y)
            Case Else
                Throw New Exception("invalid enum")
        End Select

    End Function

    Public Function GetHashCode1(ByVal obj As NativeType) As Integer Implements System.Collections.Generic.IEqualityComparer(Of NativeType).GetHashCode
        Return obj.GetHashCode()
    End Function

#End Region

#Region "Private Methods"

    Private Function EqualsRecursive(ByVal left As NativeType, ByVal right As NativeType) As Boolean

        ' Quick sanity check
        If Not EqualsTopLevel(left, right) Then
            Return False
        End If

        Dim it As New NativeSymbolIterator()

        Dim leftMap As New Dictionary(Of String, NativeDefinedType)()
        Dim rightMap As New Dictionary(Of String, NativeDefinedType)()

        For Each nt As NativeDefinedType In it.FindAllReachableDefinedTypes(left)
            If Not leftMap.ContainsKey(nt.Name) Then
                leftMap.Add(nt.Name, nt)
            End If
        Next

        For Each nt As NativeDefinedType In it.FindAllReachableDefinedTypes(right)
            If Not rightMap.ContainsKey(nt.Name) Then
                rightMap.Add(nt.Name, nt)
            End If
        Next

        If leftMap.Count <> rightMap.Count Then
            Return False
        End If

        For Each leftDefined As NativeDefinedType In leftMap.Values
            Dim rightDefined As NativeDefinedType = Nothing
            If Not rightMap.TryGetValue(leftDefined.Name, rightDefined) _
                OrElse Not EqualsTopLevel(leftDefined, rightDefined) Then
                Return False
            End If
        Next

        Return True
    End Function

    ''' <summary>
    ''' Top level takes a very shallow look at the type.  When it encounters a nested defined type, it will only compare the 
    ''' name's.  In that case NativeNamedType instances and NativeDefinedType instances will compare true if they match on name 
    ''' </summary>
    ''' <param name="left"></param>
    ''' <param name="right"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function EqualsTopLevel(ByVal left As NativeType, ByVal right As NativeType) As Boolean

        left = DigThroughNamedType(left)
        right = DigThroughNamedType(right)

        If left.Kind <> right.Kind Then
            Return False
        End If

        If Not EqualsCore(left, right) Then
            Return False
        End If

        ' If this is a defined type then make sure the members compare
        If left.Category = NativeSymbolCategory.Defined Then
            Dim leftDefined As NativeDefinedType = DirectCast(left, NativeDefinedType)
            Dim rightDefined As NativeDefinedType = DirectCast(right, NativeDefinedType)

            If leftDefined.Members.Count <> rightDefined.Members.Count Then
                Return False
            End If

            For i As Integer = 0 To leftDefined.Members.Count - 1
                Dim leftMember As NativeMember = leftDefined.Members(i)
                Dim rightMember As NativeMember = rightDefined.Members(i)

                If 0 <> String.CompareOrdinal(leftMember.Name, rightMember.Name) _
                    OrElse Not EqualsCore(leftMember.NativeType, rightMember.NativeType) Then
                    Return False
                End If
            Next
        End If

        Return True
    End Function

    Private Function DigThroughNamedType(ByVal nt As NativeType) As NativeType
        While (NativeSymbolKind.NamedType = nt.Kind)
            Dim namedtype As NativeNamedType = DirectCast(nt, NativeNamedType)
            If namedtype.RealType Is Nothing Then
                Exit While
            End If

            nt = namedtype.RealType
        End While

        Return nt
    End Function

    Private Function EqualsCore(ByVal left As NativeType, ByVal right As NativeType) As Boolean
        left = DigThroughNamedType(left)
        right = DigThroughNamedType(right)

        If left.Kind <> right.Kind Then
            If (left.Kind = NativeSymbolKind.NamedType AndAlso right.Category = NativeSymbolCategory.Defined) _
                OrElse (left.Category = NativeSymbolCategory.Defined AndAlso right.Kind = NativeSymbolKind.NamedType) Then

                Return 0 = String.CompareOrdinal(left.Name, right.Name)
            End If

            Return False
        End If

        Select Case left.Category
            Case NativeSymbolCategory.Defined
                Return EqualsDefinedCore(DirectCast(left, NativeDefinedType), DirectCast(right, NativeDefinedType))
            Case NativeSymbolCategory.Proxy
                Return EqualsProxyCore(DirectCast(left, NativeProxyType), DirectCast(right, NativeProxyType))
            Case NativeSymbolCategory.Specialized
                Return EqualsSpecializedCore(DirectCast(left, NativeSpecializedType), DirectCast(right, NativeSpecializedType))
            Case Else
                Throw New Exception("error")
        End Select
    End Function

    Private Function EqualsDefinedCore(ByVal left As NativeDefinedType, ByVal right As NativeDefinedType) As Boolean

        If left.IsAnonymous AndAlso right.IsAnonymous Then
            ' don't compare names when both types are anonymous
        ElseIf 0 <> String.CompareOrdinal(left.Name, right.Name) Then
            Return False
        End If

        ' If this is an enum, compare the values
        If left.Kind = NativeSymbolKind.EnumType Then
            Dim leftEnum As NativeEnum = DirectCast(left, NativeEnum)
            Dim rightEnum As NativeEnum = DirectCast(right, NativeEnum)

            If rightEnum.Values.Count <> leftEnum.Values.Count Then
                Return False
            End If

            For i As Integer = 0 To leftEnum.Values.Count - 1
                Dim e1 As NativeEnumValue = leftEnum.Values(i)
                Dim e2 As NativeEnumValue = rightEnum.Values(i)

                If 0 <> String.CompareOrdinal(e1.Name, e2.Name) _
                    OrElse 0 <> String.CompareOrdinal(e1.Value.Expression, e2.Value.Expression) Then
                    Return False
                End If
            Next
        End If

        Return True
    End Function

    Private Function EqualsProxyCore(ByVal left As NativeProxyType, ByVal right As NativeProxyType) As Boolean
        Dim ret As Boolean
        Select Case left.Kind
            Case NativeSymbolKind.ArrayType
                Dim a1 As NativeArray = DirectCast(left, NativeArray)
                Dim a2 As NativeArray = DirectCast(right, NativeArray)

                ret = a1.ElementCount = a2.ElementCount
            Case NativeSymbolKind.NamedType
                ret = (0 = String.CompareOrdinal( _
                    DirectCast(left, NativeNamedType).Name, _
                    DirectCast(right, NativeNamedType).Name))
            Case NativeSymbolKind.TypedefType
                ret = (0 = String.CompareOrdinal( _
                    DirectCast(left, NativeTypeDef).Name, _
                    DirectCast(right, NativeTypeDef).Name))
            Case NativeSymbolKind.PointerType
                ret = True
            Case Else
                ret = False
        End Select

        If Not ret Then
            Return False
        End If

        If left.RealType Is Nothing AndAlso right.RealType Is Nothing Then
            Return ret
        End If

        If left.RealType Is Nothing OrElse right.RealType Is Nothing Then
            Return False
        End If

        Return EqualsCore(left.RealType, right.RealType)
    End Function

    Private Function EqualsSpecializedCore(ByVal left As NativeSpecializedType, ByVal right As NativeSpecializedType) As Boolean
        Select Case left.Kind
            Case NativeSymbolKind.BitVectorType
                Dim bt1 As NativeBitVector = DirectCast(left, NativeBitVector)
                Dim bt2 As NativeBitVector = DirectCast(right, NativeBitVector)
                Return bt1.Size = bt2.Size
            Case NativeSymbolKind.BuiltinType
                Dim b1 As NativeBuiltinType = DirectCast(left, NativeBuiltinType)
                Dim b2 As NativeBuiltinType = DirectCast(right, NativeBuiltinType)
                Return b1.BuiltinType = b2.BuiltinType
            Case Else
                Return False
        End Select
    End Function

#End Region

End Class
