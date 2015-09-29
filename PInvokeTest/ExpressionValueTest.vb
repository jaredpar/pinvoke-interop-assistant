' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System
Imports System.Text
Imports System.Collections.Generic
Imports System.IO
Imports PInvoke.Parser
Imports Xunit

Public Class ExpressionValueTest

    Public Sub TestPlus(ByVal x As Object, ByVal y As Object, ByVal r As Object)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Dim result As ExpressionValue = left + right
        Assert.Equal(r, result.Value)
        Assert.Equal(r.GetType(), result.Value.GetType())
    End Sub


    Public Sub TestMinus(ByVal x As Object, ByVal y As Object, ByVal r As Object)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Dim result As ExpressionValue = left - right
        Assert.Equal(r, result.Value)
        Assert.Equal(r.GetType(), result.Value.GetType())
    End Sub

    Public Sub TestDivide(ByVal x As Object, ByVal y As Object, ByVal r As Object)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Dim result As ExpressionValue = left / right
        Assert.Equal(r, result.Value)
        Assert.Equal(r.GetType(), result.Value.GetType())
    End Sub

    Public Sub TestMultiply(ByVal x As Object, ByVal y As Object, ByVal r As Object)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Dim result As ExpressionValue = left * right
        Assert.Equal(r, result.Value)
        Assert.Equal(r.GetType(), result.Value.GetType())
    End Sub

    Public Sub TestShiftLeft(ByVal x As Object, ByVal y As Integer, ByVal r As Object)
        Dim left As New ExpressionValue(x)
        Dim result As ExpressionValue = left << y
        Assert.Equal(r, result.Value)
        Assert.Equal(r.GetType(), result.Value.GetType())
    End Sub

    Public Sub TestShiftRight(ByVal x As Object, ByVal y As Integer, ByVal r As Object)
        Dim left As New ExpressionValue(x)
        Dim result As ExpressionValue = left >> y
        Assert.Equal(r, result.Value)
        Assert.Equal(r.GetType(), result.Value.GetType())
    End Sub

    Public Sub TestGreaterThan(ByVal x As Object, ByVal y As Object, ByVal expected As Boolean)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Assert.Equal(expected, left > right)
    End Sub

    Public Sub TestGreaterThanOrEqualsTo(ByVal x As Object, ByVal y As Object, ByVal expected As Boolean)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Assert.Equal(expected, left >= right)
    End Sub

    Public Sub TestLessThan(ByVal x As Object, ByVal y As Object, ByVal expected As Boolean)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Assert.Equal(expected, left < right)
    End Sub

    Public Sub TestLessThanOrEqualsTo(ByVal x As Object, ByVal y As Object, ByVal expected As Boolean)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Assert.Equal(expected, left <= right)
    End Sub

    Public Sub TestNotEqualsTo(ByVal x As Object, ByVal y As Object, ByVal expected As Boolean)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Assert.Equal(expected, left <> right)
    End Sub

    Public Sub TestEqualsTo(ByVal x As Object, ByVal y As Object, ByVal expected As Boolean)
        Dim left As New ExpressionValue(x)
        Dim right As New ExpressionValue(y)
        Assert.Equal(expected, left = right)
    End Sub

    <Fact>
    Public Sub TestInt32()
        TestPlus(1, 2, 3)
        TestPlus(10, 15, 25)
        TestPlus(-1, 3, 2)
        TestMinus(15, 5, 10)
        TestDivide(15, 5, 3.0R)
        TestMultiply(3, 2, 6)
        TestShiftLeft(2, 1, 4)
        TestShiftRight(4, 1, 2)
        TestGreaterThan(1, 2, False)
        TestGreaterThanOrEqualsTo(2, 2, True)
        TestLessThan(1, 2, True)
        TestLessThanOrEqualsTo(2, 2, True)
        TestEqualsTo(1, 1, True)
        TestEqualsTo(1, 2, False)
        TestNotEqualsTo(1, 1, False)
        TestNotEqualsTo(1, 2, True)

    End Sub

    <Fact>
    Public Sub TestInt64()
        TestPlus(1L, 2L, 3L)
        TestPlus(10L, 15L, 25L)
        TestPlus(-1L, 3L, 2L)
        TestMinus(15L, 5L, 10L)
        TestDivide(15L, 5L, 3.0R)
        TestMultiply(3L, 2L, 6L)
        TestGreaterThan(1L, 2L, False)
        TestGreaterThanOrEqualsTo(2L, 2L, True)
        TestLessThan(1L, 2L, True)
        TestLessThanOrEqualsTo(2L, 2L, True)
        TestEqualsTo(1L, 1L, True)
        TestEqualsTo(1L, 2L, False)
        TestNotEqualsTo(1L, 1L, False)
        TestNotEqualsTo(1L, 2L, True)
    End Sub

    <Fact>
    Public Sub TestDouble()
        TestPlus(1.0R, 2.0R, 3.0R)
        TestPlus(10.0R, 15.0R, 25.0R)
        TestPlus(-1.0R, 3.0R, 2.0R)
        TestMinus(15.0R, 5.0R, 10.0R)
        TestDivide(15.0R, 5.0R, 3.0R)
        TestMultiply(3.0R, 2.0R, 6.0R)
        TestGreaterThan(1.0R, 2.0R, False)
        TestGreaterThanOrEqualsTo(2.0R, 2.0R, True)
        TestLessThan(1.0R, 2.0R, True)
        TestLessThanOrEqualsTo(2.0R, 2.0R, True)
        TestEqualsTo(1.0R, 1.0R, True)
        TestEqualsTo(1.0R, 2.0R, False)
        TestNotEqualsTo(1.0R, 1.0R, False)
        TestNotEqualsTo(1.0R, 2.0R, True)
    End Sub

    <Fact>
    Public Sub Conversion1()
        Dim left As ExpressionValue = 1
        Dim right As ExpressionValue = 5
        Assert.False(left = right)
        Assert.True(left = 1)
        Assert.True(left <> 5)
        Assert.True(right = 5)
    End Sub

    <Fact>
    Public Sub Conversion2()
        Dim left As ExpressionValue = True
        Dim right As ExpressionValue = False
        Assert.True(left = 1)
        Assert.True(right = 0)
    End Sub

End Class
