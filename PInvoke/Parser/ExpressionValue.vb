' Copyright (c) Microsoft Corporation.  All rights reserved.
Option Strict Off
Namespace Parser

    <DebuggerDisplay("Value={Value}")> _
    Public Class ExpressionValue
        Private _value As Object

        Public Property Value() As Object
            Get
                Return _value
            End Get
            Set(ByVal value As Object)
                _value = value
            End Set
        End Property

        Public Sub New(ByVal value As Object)
            Contract.ThrowIfNull(value)
            _value = value
        End Sub

        Public Sub New(ByVal value As Boolean)
            If value Then
                _value = 1
            Else
                _value = 0
            End If
        End Sub

        Public Shared Operator +(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As ExpressionValue
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return New ExpressionValue(left.Value + right.Value)
        End Operator

        Public Shared Operator -(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As ExpressionValue
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return New ExpressionValue(left.Value - right.Value)
        End Operator

        Public Shared Operator -(ByVal left As ExpressionValue) As ExpressionValue
            Contract.ThrowIfNull(left)
            Return New ExpressionValue(-(left.Value))
        End Operator

        Public Shared Operator /(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As ExpressionValue
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return New ExpressionValue(left.Value / right.Value)
        End Operator

        Public Shared Operator \(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As ExpressionValue
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return New ExpressionValue(left.Value \ right.Value)
        End Operator

        Public Shared Operator >(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As Boolean
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return left.Value > right.Value
        End Operator

        Public Shared Operator <(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As Boolean
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return left.Value < right.Value
        End Operator

        Public Shared Operator >=(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As Boolean
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return left.Value >= right.Value
        End Operator

        Public Shared Operator <=(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As Boolean
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return left.Value <= right.Value
        End Operator

        Public Shared Operator <>(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As Boolean
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return left.Value <> right.Value
        End Operator

        Public Shared Operator =(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As Boolean
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return left.Value = right.Value
        End Operator

        Public Shared Operator *(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As ExpressionValue
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return New ExpressionValue(left.Value * right.Value)
        End Operator

        Public Shared Operator <<(ByVal left As ExpressionValue, ByVal count As Int32) As ExpressionValue
            Contract.ThrowIfNull(left)
            Return New ExpressionValue(CInt(left.Value) << count)
        End Operator

        Public Shared Operator >>(ByVal left As ExpressionValue, ByVal count As Int32) As ExpressionValue
            Contract.ThrowIfNull(left)
            Return New ExpressionValue(CInt(left.Value) >> count)
        End Operator

        Public Shared Operator IsTrue(ByVal expr As ExpressionValue) As Boolean
            Contract.ThrowIfNull(expr)
            Return CBool(expr.Value)
        End Operator

        Public Shared Operator IsFalse(ByVal expr As ExpressionValue) As Boolean
            Contract.ThrowIfNull(expr)
            Return Not CBool(expr.Value)
        End Operator

        Public Shared Operator Not(ByVal expr As ExpressionValue) As Boolean
            Contract.ThrowIfNull(expr)
            Return Not CBool(expr.Value)
        End Operator

        Public Shared Operator And(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As ExpressionValue
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return New ExpressionValue(left.Value And right.Value)
        End Operator

        Public Shared Operator Or(ByVal left As ExpressionValue, ByVal right As ExpressionValue) As ExpressionValue
            Contract.ThrowIfNull(left)
            Contract.ThrowIfNull(right)
            Return New ExpressionValue(left.Value Or right.Value)
        End Operator

        Public Shared Widening Operator CType(ByVal value As Int32) As ExpressionValue
            Return New ExpressionValue(value)
        End Operator

        Public Shared Widening Operator CType(ByVal value As Boolean) As ExpressionValue
            Return New ExpressionValue(value)
        End Operator

    End Class
End Namespace
