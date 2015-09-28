' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System
Imports System.Text
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports PInvokeTest.PInvokeTestLib

''' <summary>
''' Test the PInvokeTestLib DLL
''' 
''' These tests are designed to test the ability to marshal bits back and forth.  Some extra 
''' parser testing is done here but mainly this concentrates on verifying the types and
''' signatures we generate can properly marshal data accross the native managed boundary
''' </summary>
''' <remarks></remarks>
<TestClass()> _
Public Class TestPInvokeTestLib

    ''' <summary>
    ''' Call the reverse string API
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub ReverseString1()
        Dim result As String = Nothing
        Assert.IsTrue(NativeMethods.ReverseString("foo", result))
        Assert.AreEqual("oof", result)
    End Sub

    ''' <summary>
    ''' Call reverse string with bad parameters
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub ReverseString2()
        Dim builder As New StringBuilder()
        builder.Capacity = 5
        Assert.IsFalse(NativeMethods.ReverseString("longlonglonglonglongstring", builder, builder.Capacity))
    End Sub

    ''' <summary>
    ''' Simple bitvector test
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub BitVector1()
        Dim bt As New BitVector1
        Assert.IsTrue(NativeMethods.UpdateBitVector1Data(bt))
        Assert.AreEqual(CUInt(5), bt.m1)
        Assert.AreEqual(CUInt(42), bt.m2)
    End Sub

    ''' <summary>
    ''' Data going both ways in the bitvector
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub BitVector2()
        Dim bt As New BitVector1()
        bt.m1 = 5
        bt.m2 = 3
        Assert.IsTrue(NativeMethods.IsM1GreaterThanM2(bt))
        bt.m2 = 7
        Assert.IsFalse(NativeMethods.IsM1GreaterThanM2(bt))
    End Sub

    <TestMethod()> _
    Public Sub CalculateStringLength1()
        Dim len As Integer = 0
        Assert.IsTrue(NativeMethods.CalculateStringLength("foo", len))
        Assert.AreEqual(3, len)
    End Sub

    <TestMethod()> _
    Public Sub S1FakeConstructor_1()
        Dim s1 As New s1()
        Assert.IsTrue(NativeMethods.s1FakeConstructor(42, 3.5, s1))
        Assert.AreEqual(42, s1.m1)
        Assert.AreEqual(3.5, s1.m2)
    End Sub

    <TestMethod()> _
    Public Sub S1FakeConstructor2_1()
        Dim s1 As s1 = NativeMethods.s1FakeConstructor2(42, 3.5)
        Assert.AreEqual(42, s1.m1)
        Assert.AreEqual(3.5, s1.m2)
    End Sub

    <TestMethod()> _
    Public Sub S2FakeConstructor()
        Dim s2 As New s2()
        Assert.IsTrue(NativeMethods.s2FakeConstructor(5, "foo", s2))
        Assert.AreEqual(5, s2.m1)
        Assert.AreEqual("foo", s2.m2)
    End Sub

    <TestMethod()> _
    Public Sub Enume1Values()
        Assert.AreEqual(0, CInt(e1.v1))
        Assert.AreEqual(NativeConstants.VALUE_CONSTANT_1, CInt(e1.v2))
    End Sub

    <TestMethod()> _
    Public Sub CopyM1ToM2()
        Dim s3 As New s3()
        s3.m1 = New Integer() {1, 2, 3, 4}
        s3.m2 = New Double(4) {}
        Assert.IsTrue(NativeMethods.CopyM1ToM2(s3))
        Assert.AreEqual(CDbl(1), s3.m2(0))
        Assert.AreEqual(CDbl(2), s3.m2(1))
    End Sub

    Public Structure TempStruct
        Public m1 As UInteger
        <System.Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.U4)> _
        Public m2 As Boolean
        Public m3 As UInteger
    End Structure

    ''' <summary>
    ''' Use a struct with multiple bitvectors that are not directly beside 
    ''' each other
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub MultiBitVector()
        'Dim b As TempStruct = NativeMethods.CreateBitVector2(1, True, 2)
        'Assert.AreEqual(CUInt(1), b.m1)
        'Assert.IsTrue(b.m2)
        'Assert.AreEqual(CUInt(2), b.m2)
    End Sub

    <TestMethod()> _
    Public Sub SumArray()
        Dim arr(3) As Integer
        arr(0) = 1
        arr(1) = 2
        arr(2) = 3
        arr(3) = 15
        Dim sum As Integer = 0
        Assert.IsTrue(NativeMethods.SumArray(arr, sum))
        Assert.AreEqual(21, sum)
    End Sub

    <TestMethod()> _
    Public Sub SumArray2()
        Dim arr(3) As Integer
        arr(0) = 1
        arr(1) = 2
        arr(2) = 3
        arr(3) = 15
        Dim sum As Integer = 0
        Assert.IsTrue(NativeMethods.SumArray2(arr, arr.Length, sum))
        Assert.AreEqual(21, sum)
    End Sub

    <TestMethod()> _
    Public Sub S4Add()
        Dim s As New s4
        Dim d(4) As Byte
        s.m1 = d
        s.m1(0) = 1
        s.m1(1) = 2
        s.m1(2) = 3
        s.m1(3) = 4
        Assert.AreEqual(10, NativeMethods.s4Add(s))

    End Sub

    <TestMethod()> _
    Public Sub GetVeryLongString()
        Dim b As String = Nothing
        NativeMethods.GetVeryLongString(b)
        Assert.IsTrue(b.StartsWith("012012"))
    End Sub

    <TestMethod()> _
       Public Sub GetVeryLongString2()
        Dim b As String = Nothing
        NativeMethods.GetVeryLongString2(b)
        Assert.IsTrue(b.StartsWith("012012"))
    End Sub

    <TestMethod()> _
    Public Sub GetPointerPointerToChar()
        Dim p As IntPtr = IntPtr.Zero
        Assert.IsTrue(NativeMethods.GetPointerPointerToChar("f"c, p))
        Dim o As Object = Marshal.PtrToStructure(p, GetType(Char))
        Dim c As Char = DirectCast(o, Char)
        Assert.AreEqual("f"c, c)
    End Sub

    <TestMethod()> _
    Public Sub CopyDecimalToPointer()
        Dim p1 As New Decimal(42)
        Dim p2 As New Decimal(0)
        Assert.AreNotEqual(p1, p2)
        Assert.IsTrue(NativeMethods.CopyDecimalToPoiner(p1, p2))
        Assert.AreEqual(p1, p2)
    End Sub

    <TestMethod()> _
    Public Sub CopyDecimalToReturn()
        Dim d1 As New Decimal(42)
        Dim d2 As Decimal = NativeMethods.CopyDecimalToReturn(d1)
        Assert.AreEqual(d1, d2)
    End Sub

    <TestMethod()> _
    Public Sub CopyDecimalPointerToPointer()
        Dim d1 As New Decimal(42)
        Dim d2 As New Decimal(5)
        Assert.AreNotEqual(d1, d2)
        Assert.IsTrue(NativeMethods.CopyDecimalPointerToPointer(d1, d2))
        Assert.AreEqual(d1, d2)
    End Sub

    <TestMethod()> _
    Public Sub CopyCurrencyToPointer()
        Dim d1 As New Decimal(42)
        Dim d2 As New Decimal(5)
        Assert.AreNotEqual(d1, d2)
        Assert.IsTrue(NativeMethods.CopyCurrencyToPointer(d1, d2))
        Assert.AreEqual(d1, d2)
    End Sub

    <TestMethod()> _
    Public Sub CopyBstrToNormalStr()
        Dim result As String = Nothing
        Assert.IsTrue(NativeMethods.CopyBstrToNoramlStr("foo", result))
        Assert.AreEqual("foo", result)
    End Sub

    <TestMethod()> _
    Public Sub CopyToBstr()
        Dim result As String = Nothing
        Assert.IsTrue(NativeMethods.CopyToBstr("bar", result))
        Assert.AreEqual("bar", result)
    End Sub

    <TestMethod()> _
    Public Sub CopyBothToBstr()
        Dim result As String = Nothing
        Assert.IsTrue(NativeMethods.CopyBothToBstr("foo", "bar", result))
        Assert.AreEqual("foobar", result)
    End Sub

    <TestMethod()> _
    Public Sub CopyBstrToBstr()
        Dim result As String = Nothing
        Assert.IsTrue(NativeMethods.CopyBstrToBstr("foo", result))
        Assert.AreEqual("foo", result)
    End Sub

    <TestMethod()> _
    Public Sub CopyNormalStrToBstrRet()
        Dim result As String = NativeMethods.CopyNormalStrToBstrRet("str5")
        Assert.AreEqual("str5", result)
    End Sub

    <TestMethod()> _
    Public Sub CreateBasicOpaque()
        Dim p As IntPtr = NativeMethods.CreateBasicOpaque()
        Assert.IsTrue(NativeMethods.VerifyBasicOpaque(p))
    End Sub

    <TestMethod()> _
    Public Sub VerifyBasicOpaque()
        Assert.IsFalse(NativeMethods.VerifyBasicOpaque(IntPtr.Zero))
    End Sub

    <TestMethod()> _
    Public Sub GetFunctionPointerReturningInt()
        Dim p As pFunctionPointerReturningInt = NativeMethods.GetFunctionPointerReturningInt()
        Assert.AreEqual(42, p())
    End Sub

    Public Function AreResultAndValueEqualImpl() As Integer
        Return 56
    End Function

    <TestMethod()> _
    Public Sub AreResultAndValueEqual()
        Dim p As pFunctionPointerReturningInt = AddressOf AreResultAndValueEqualImpl
        Assert.IsTrue(NativeMethods.AreResultAndValueEqual(p, 56))
        Assert.IsFalse(NativeMethods.AreResultAndValueEqual(p, 42))
    End Sub

    <TestMethod()> _
    Public Sub GetAStructWithASimpleFunctionPointer()
        Dim s As New structWithFunctionPointer
        NativeMethods.GetAStructWithASimpleFunctionPointer(3, s)
        Assert.AreEqual(3, s.m1)
        Assert.AreEqual(5, s.AnonymousMember1(2, 3))
    End Sub

    <TestMethod()> _
    Public Sub MultiplyWithCDecl()
        Assert.AreEqual(30, NativeMethods.MultiplyWithCDecl(5, 6))
    End Sub

    <TestMethod()> _
    Public Sub SimpleClass()
        Dim c As New simpleClass()
        c.m1 = 42
        c.m2 = 54
        Assert.AreEqual(42, NativeMethods.GetSimpleClassM1(c))
        Assert.AreEqual(54, NativeMethods.GetSimpleClassM2(c))
    End Sub

    <TestMethod()> _
    Public Sub StringInStruct()
        Dim s As New stringInStruct
        s.m1 = "foo"
        Assert.IsTrue(NativeMethods.VerifyStringInStructM1(s, "foo"))
        Assert.IsFalse(NativeMethods.VerifyStringInStructM1(s, "false"))
    End Sub

    <TestMethod()> _
    Public Sub StringDiffTypeInStruct()
        Dim s As New structWithDiffStringTypes
        NativeMethods.PopulateStructWithDiffStringTypes(s, "foo", "bar")
        Assert.AreEqual("foo", s.m1)
        Assert.AreEqual("bar", s.m2)
    End Sub

End Class
