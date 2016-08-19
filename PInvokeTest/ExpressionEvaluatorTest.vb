' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System
Imports System.Text
Imports System.Collections.Generic
Imports PInvoke.Parser
Imports Xunit

Public Class ExpressionEvaluatorTest

#Region "Additional test attributes"
    '
    ' You can use the following additional attributes as you write your tests:
    '
    ' Use ClassInitialize to run code before running the first test in the class
    ' <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    ' End Sub
    '
    ' Use ClassCleanup to run code after all tests in a class have run
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub
    '
    ' Use TestInitialize to run code before running each test
    ' <TestInitialize()> Public Sub MyTestInitialize()
    ' End Sub
    '
    ' Use TestCleanup to run code after each test has run
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region

    Private Sub AssertEval(ByVal expr As String, ByVal result As Object)
        Dim ee As New ExpressionEvaluator()
        Dim actual As ExpressionValue = Nothing
        Assert.True(ee.TryEvaluate(expr, actual))
        Assert.Equal(result, actual.Value)
    End Sub

    <Fact>
    Public Sub Leaf1()
        AssertEval("1", 1)
        AssertEval("0xf", 15)
    End Sub

    <Fact>
    Public Sub Plus1()
        AssertEval("1+2", 3)
        AssertEval("540+50+50", 640)
    End Sub

    <Fact>
    Public Sub Plus2()
        AssertEval("0x1 + 0x2", 3)
    End Sub

    <Fact>
    Public Sub Minus1()
        AssertEval("10-2", 8)
        AssertEval("(20-5)-5", 10)
    End Sub

    <Fact>
    Public Sub Minus2()
        AssertEval("1-2", -1)
    End Sub

    <Fact>
    Public Sub Divide1()
        AssertEval("10/2", 5)
    End Sub

    <Fact>
    Public Sub Modulus1()
        AssertEval("5 % 2 ", 1)
        AssertEval("10 % 3", 1)
        AssertEval("15 % 8", 7)
    End Sub

    <Fact>
    Public Sub ShiftLeft1()
        AssertEval("2 << 1", 4)
    End Sub

    <Fact>
    Public Sub ShiftRight1()
        AssertEval("4 >> 1", 2)
    End Sub

    <Fact>
    Public Sub Negative1()
        AssertEval("-1", -1)
        AssertEval("-(2+4)", -6)
    End Sub

    <Fact>
    Public Sub Negative2()
        AssertEval("-0.1F", -0.1F)
        AssertEval("-3.2F", -3.2F)
    End Sub

    <Fact>
    Public Sub Boolean1()
        AssertEval("true", 1)
        AssertEval("false", 0)
    End Sub

    <Fact>
    Public Sub OpAnd1()
        AssertEval("true && true", 1)
        AssertEval("true && false", 0)
    End Sub

    <Fact>
    Public Sub OpOr1()
        AssertEval("true || true", 1)
        AssertEval("false || true", 1)
        AssertEval("false || false", 0)
    End Sub

    <Fact>
    Public Sub OpAssign()
        AssertEval("1=2", 2)
    End Sub

    <Fact>
    Public Sub Char1()
        AssertEval("'c'", "c"c)
    End Sub

    <Fact>
    Public Sub OpEquals1()
        AssertEval("1==1", 1)
        AssertEval("1==2", 0)
    End Sub


End Class
