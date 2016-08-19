' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System
Imports System.Text
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports PInvokeTest.PInvokeTestLib
Imports Xunit

''' <summary>
''' Test the PInvokeTestLib DLL
''' 
''' These tests are designed to test the ability to marshal bits back and forth.  Some extra 
''' parser testing is done here but mainly this concentrates on verifying the types and
''' signatures we generate can properly marshal data accross the native managed boundary
''' </summary>
''' <remarks></remarks>
Public Class TestPInvokeTestLib

    ''' <summary>
    ''' Call the reverse string API
    ''' </summary>
    ''' <remarks></remarks>
    <Fact(Skip:="Lib not building")>
    Public Sub ReverseString1()
        Dim result As String = Nothing
        Assert.True(NativeMethods.ReverseString("foo", result))
        Assert.Equal("oof", result)
    End Sub

    ''' <summary>
    ''' Call reverse string with bad parameters
    ''' </summary>
    ''' <remarks></remarks>
    <Fact(Skip:="Lib not building")>
    Public Sub ReverseString2()
        Dim builder As New StringBuilder()
        builder.Capacity = 5
        Assert.False(NativeMethods.ReverseString("longlonglonglonglongstring", builder, builder.Capacity))
    End Sub

    ''' <summary>
    ''' Simple bitvector test
    ''' </summary>
    ''' <remarks></remarks>
    <Fact(Skip:="Lib not building")>
    Public Sub BitVector1()
        Dim bt As New BitVector1
        Assert.True(NativeMethods.UpdateBitVector1Data(bt))
        Assert.Equal(CUInt(5), bt.m1)
        Assert.Equal(CUInt(42), bt.m2)
    End Sub

    ''' <summary>
    ''' Data going both ways in the bitvector
    ''' </summary>
    ''' <remarks></remarks>
    <Fact(Skip:="Lib not building")>
    Public Sub BitVector2()
        Dim bt As New BitVector1()
        bt.m1 = 5
        bt.m2 = 3
        Assert.True(NativeMethods.IsM1GreaterThanM2(bt))
        bt.m2 = 7
        Assert.False(NativeMethods.IsM1GreaterThanM2(bt))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CalculateStringLength1()
        Dim len As Integer = 0
        Assert.True(NativeMethods.CalculateStringLength("foo", len))
        Assert.Equal(3, len)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub S1FakeConstructor_1()
        Dim s1 As New s1()
        Assert.True(NativeMethods.s1FakeConstructor(42, 3.5, s1))
        Assert.Equal(42, s1.m1)
        Assert.Equal(3.5, s1.m2)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub S1FakeConstructor2_1()
        Dim s1 As s1 = NativeMethods.s1FakeConstructor2(42, 3.5)
        Assert.Equal(42, s1.m1)
        Assert.Equal(3.5, s1.m2)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub S2FakeConstructor()
        Dim s2 As New s2()
        Assert.True(NativeMethods.s2FakeConstructor(5, "foo", s2))
        Assert.Equal(5, s2.m1)
        Assert.Equal("foo", s2.m2)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub Enume1Values()
        Assert.Equal(0, CInt(e1.v1))
        Assert.Equal(NativeConstants.VALUE_CONSTANT_1, CInt(e1.v2))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyM1ToM2()
        Dim s3 As New s3()
        s3.m1 = New Integer() {1, 2, 3, 4}
        s3.m2 = New Double(4) {}
        Assert.True(NativeMethods.CopyM1ToM2(s3))
        Assert.Equal(CDbl(1), s3.m2(0))
        Assert.Equal(CDbl(2), s3.m2(1))
    End Sub

    Public Structure TempStruct
        Public m1 As UInteger
        <System.Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.U4)>
        Public m2 As Boolean
        Public m3 As UInteger
    End Structure

    ''' <summary>
    ''' Use a struct with multiple bitvectors that are not directly beside 
    ''' each other
    ''' </summary>
    ''' <remarks></remarks>
    <Fact(Skip:="Lib not building")>
    Public Sub MultiBitVector()
        'Dim b As TempStruct = NativeMethods.CreateBitVector2(1, True, 2)
        'Assert.Equal(CUInt(1), b.m1)
        'Assert.True(b.m2)
        'Assert.Equal(CUInt(2), b.m2)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub SumArray()
        Dim arr(3) As Integer
        arr(0) = 1
        arr(1) = 2
        arr(2) = 3
        arr(3) = 15
        Dim sum As Integer = 0
        Assert.True(NativeMethods.SumArray(arr, sum))
        Assert.Equal(21, sum)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub SumArray2()
        Dim arr(3) As Integer
        arr(0) = 1
        arr(1) = 2
        arr(2) = 3
        arr(3) = 15
        Dim sum As Integer = 0
        Assert.True(NativeMethods.SumArray2(arr, arr.Length, sum))
        Assert.Equal(21, sum)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub S4Add()
        Dim s As New s4
        Dim d(4) As Byte
        s.m1 = d
        s.m1(0) = 1
        s.m1(1) = 2
        s.m1(2) = 3
        s.m1(3) = 4
        Assert.Equal(10, NativeMethods.s4Add(s))

    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub GetVeryLongString()
        Dim b As String = Nothing
        NativeMethods.GetVeryLongString(b)
        Assert.True(b.StartsWith("012012"))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub GetVeryLongString2()
        Dim b As String = Nothing
        NativeMethods.GetVeryLongString2(b)
        Assert.True(b.StartsWith("012012"))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub GetPointerPointerToChar()
        Dim p As IntPtr = IntPtr.Zero
        Assert.True(NativeMethods.GetPointerPointerToChar("f"c, p))
        Dim o As Object = Marshal.PtrToStructure(p, GetType(Char))
        Dim c As Char = DirectCast(o, Char)
        Assert.Equal("f"c, c)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyDecimalToPointer()
        Dim p1 As New Decimal(42)
        Dim p2 As New Decimal(0)
        Assert.NotEqual(p1, p2)
        Assert.True(NativeMethods.CopyDecimalToPoiner(p1, p2))
        Assert.Equal(p1, p2)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyDecimalToReturn()
        Dim d1 As New Decimal(42)
        Dim d2 As Decimal = NativeMethods.CopyDecimalToReturn(d1)
        Assert.Equal(d1, d2)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyDecimalPointerToPointer()
        Dim d1 As New Decimal(42)
        Dim d2 As New Decimal(5)
        Assert.NotEqual(d1, d2)
        Assert.True(NativeMethods.CopyDecimalPointerToPointer(d1, d2))
        Assert.Equal(d1, d2)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyCurrencyToPointer()
        Dim d1 As New Decimal(42)
        Dim d2 As New Decimal(5)
        Assert.NotEqual(d1, d2)
        Assert.True(NativeMethods.CopyCurrencyToPointer(d1, d2))
        Assert.Equal(d1, d2)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyBstrToNormalStr()
        Dim result As String = Nothing
        Assert.True(NativeMethods.CopyBstrToNoramlStr("foo", result))
        Assert.Equal("foo", result)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyToBstr()
        Dim result As String = Nothing
        Assert.True(NativeMethods.CopyToBstr("bar", result))
        Assert.Equal("bar", result)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyBothToBstr()
        Dim result As String = Nothing
        Assert.True(NativeMethods.CopyBothToBstr("foo", "bar", result))
        Assert.Equal("foobar", result)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyBstrToBstr()
        Dim result As String = Nothing
        Assert.True(NativeMethods.CopyBstrToBstr("foo", result))
        Assert.Equal("foo", result)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CopyNormalStrToBstrRet()
        Dim result As String = NativeMethods.CopyNormalStrToBstrRet("str5")
        Assert.Equal("str5", result)
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub CreateBasicOpaque()
        Dim p As IntPtr = NativeMethods.CreateBasicOpaque()
        Assert.True(NativeMethods.VerifyBasicOpaque(p))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub VerifyBasicOpaque()
        Assert.False(NativeMethods.VerifyBasicOpaque(IntPtr.Zero))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub GetFunctionPointerReturningInt()
        Dim p As pFunctionPointerReturningInt = NativeMethods.GetFunctionPointerReturningInt()
        Assert.Equal(42, p())
    End Sub

    Public Function AreResultAndValueEqualImpl() As Integer
        Return 56
    End Function

    <Fact(Skip:="Lib not building")>
    Public Sub AreResultAndValueEqual()
        Dim p As pFunctionPointerReturningInt = AddressOf AreResultAndValueEqualImpl
        Assert.True(NativeMethods.AreResultAndValueEqual(p, 56))
        Assert.False(NativeMethods.AreResultAndValueEqual(p, 42))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub GetAStructWithASimpleFunctionPointer()
        Dim s As New structWithFunctionPointer
        NativeMethods.GetAStructWithASimpleFunctionPointer(3, s)
        Assert.Equal(3, s.m1)
        Assert.Equal(5, s.AnonymousMember1(2, 3))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub MultiplyWithCDecl()
        Assert.Equal(30, NativeMethods.MultiplyWithCDecl(5, 6))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub SimpleClass()
        Dim c As New simpleClass()
        c.m1 = 42
        c.m2 = 54
        Assert.Equal(42, NativeMethods.GetSimpleClassM1(c))
        Assert.Equal(54, NativeMethods.GetSimpleClassM2(c))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub StringInStruct()
        Dim s As New stringInStruct
        s.m1 = "foo"
        Assert.True(NativeMethods.VerifyStringInStructM1(s, "foo"))
        Assert.False(NativeMethods.VerifyStringInStructM1(s, "false"))
    End Sub

    <Fact(Skip:="Lib not building")>
    Public Sub StringDiffTypeInStruct()
        Dim s As New structWithDiffStringTypes
        NativeMethods.PopulateStructWithDiffStringTypes(s, "foo", "bar")
        Assert.Equal("foo", s.m1)
        Assert.Equal("bar", s.m2)
    End Sub

End Class
