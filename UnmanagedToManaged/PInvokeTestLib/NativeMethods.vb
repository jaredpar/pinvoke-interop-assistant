' Copyright (c) Microsoft Corporation.  All rights reserved.

Partial Public Class NativeConstants
    
    '''PINVOKETESTLIB_API -> __declspec(dllimport)
    '''Error generating expression: Error generating function call.  Operation not implemented
    Public Const PINVOKETESTLIB_API As String = "__declspec(dllimport)"
    
    '''foo -> "bar"
    Public Const foo As String = "bar"
    
    '''foo2 -> "bar2"
    Public Const foo2 As String = "bar2"
    
    '''VALUE_CONSTANT_1 -> 5
    Public Const VALUE_CONSTANT_1 As Integer = 5
End Class

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure BitVector1
    
    '''m1 : 5
    '''m2 : 6
    Public bitvector1 As UInteger
    
    Public Property m1() As UInteger
        Get
            Return CType((Me.bitvector1 And 31UI),UInteger)
        End Get
        Set
            Me.bitvector1 = CType((value Or Me.bitvector1),UInteger)
        End Set
    End Property
    
    Public Property m2() As UInteger
        Get
            Return CType(((Me.bitvector1 And 2016UI)  _
                        / 32),UInteger)
        End Get
        Set
            Me.bitvector1 = CType(((value * 32)  _
                        Or Me.bitvector1),UInteger)
        End Set
    End Property
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure BitVector2
    
    '''m1 : 2
    Public bitvector1 As UInteger
    
    '''BOOL->int
    <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)>  _
    Public m2 As Boolean
    
    '''m3 : 2
    Public bitvector2 As UInteger
    
    Public Property m1() As UInteger
        Get
            Return CType((Me.bitvector1 And 3UI),UInteger)
        End Get
        Set
            Me.bitvector1 = CType((value Or Me.bitvector1),UInteger)
        End Set
    End Property
    
    Public Property m3() As UInteger
        Get
            Return CType((Me.bitvector2 And 3UI),UInteger)
        End Get
        Set
            Me.bitvector2 = CType((value Or Me.bitvector2),UInteger)
        End Set
    End Property
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure s1
    
    '''int
    Public m1 As Integer
    
    '''double
    Public m2 As Double
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet:=System.Runtime.InteropServices.CharSet.[Unicode])>  _
Public Structure s2
    
    '''int
    Public m1 As Integer
    
    '''wchar_t[250]
    <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst:=250)>  _
    Public m2 As String
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure s3
    
    '''int[4]
    <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=4, ArraySubType:=System.Runtime.InteropServices.UnmanagedType.I4)>  _
    Public m1() As Integer
    
    '''double[4]
    <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=4, ArraySubType:=System.Runtime.InteropServices.UnmanagedType.R8)>  _
    Public m2() As Double
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure s4
    
    '''BYTE[4]
    <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=4, ArraySubType:=System.Runtime.InteropServices.UnmanagedType.I1)>  _
    Public m1() As Byte
End Structure

Public Enum e1
    
    v1
    
    '''v2 -> 5
    v2 = 5
End Enum

'''Return Type: int
Public Delegate Function pFunctionPointerReturningInt() As Integer

'''Return Type: int
'''param0: int
'''param1: int
Public Delegate Function structWithFunctionPointer_pAddTheValues(ByVal param0 As Integer, ByVal param1 As Integer) As Integer

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure structWithFunctionPointer
    
    '''int
    Public m1 As Integer
    
    '''structWithFunctionPointer_pAddTheValues
    Public AnonymousMember1 As structWithFunctionPointer_pAddTheValues
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure tagDEC
    
    '''USHORT->unsigned short
    Public wReserved As UShort
    
    '''Anonymous_ff27e256_6b78_4650_bf76_0913cf467816
    Public Union1 As Anonymous_ff27e256_6b78_4650_bf76_0913cf467816
    
    '''ULONG->unsigned int
    Public Hi32 As UInteger
    
    '''Anonymous_76f02cb6_61b6_426b_a1fe_2f838699901e
    Public Union2 As Anonymous_76f02cb6_61b6_426b_a1fe_2f838699901e
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)>  _
Public Structure tagCY
    
    '''Anonymous_b0d8d0e5_ef5b_497e_acdd_98c7f555ccf4
    <System.Runtime.InteropServices.FieldOffsetAttribute(0)>  _
    Public Struct1 As Anonymous_b0d8d0e5_ef5b_497e_acdd_98c7f555ccf4
    
    '''LONGLONG->double
    <System.Runtime.InteropServices.FieldOffsetAttribute(0)>  _
    Public int64 As Double
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)>  _
Public Structure Anonymous_ff27e256_6b78_4650_bf76_0913cf467816
    
    '''Anonymous_9b707827_f6f8_4ad6_896e_baaa772386ce
    <System.Runtime.InteropServices.FieldOffsetAttribute(0)>  _
    Public Struct1 As Anonymous_9b707827_f6f8_4ad6_896e_baaa772386ce
    
    '''USHORT->unsigned short
    <System.Runtime.InteropServices.FieldOffsetAttribute(0)>  _
    Public signscale As UShort
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)>  _
Public Structure Anonymous_76f02cb6_61b6_426b_a1fe_2f838699901e
    
    '''Anonymous_dc68cfca_15cd_49b9_a790_da4c9eed8374
    <System.Runtime.InteropServices.FieldOffsetAttribute(0)>  _
    Public Struct1 As Anonymous_dc68cfca_15cd_49b9_a790_da4c9eed8374
    
    '''ULONGLONG->double
    <System.Runtime.InteropServices.FieldOffsetAttribute(0)>  _
    Public Lo64 As Double
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure Anonymous_b0d8d0e5_ef5b_497e_acdd_98c7f555ccf4
    
    '''unsigned int
    Public Lo As UInteger
    
    '''int
    Public Hi As Integer
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure Anonymous_9b707827_f6f8_4ad6_896e_baaa772386ce
    
    '''BYTE->unsigned char
    Public scale As Byte
    
    '''BYTE->unsigned char
    Public sign As Byte
End Structure

<System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)>  _
Public Structure Anonymous_dc68cfca_15cd_49b9_a790_da4c9eed8374
    
    '''ULONG->unsigned int
    Public Lo32 As UInteger
    
    '''ULONG->unsigned int
    Public Mid32 As UInteger
End Structure

Partial Public Class NativeMethods
    
    '''Return Type: BOOL->int
    '''data: BitVector1*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="UpdateBitVector1Data")>  _
    Public Shared Function UpdateBitVector1Data(<System.Runtime.InteropServices.InAttribute()> ByRef data As BitVector1) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
    End Function
    
    '''Return Type: BOOL->int
    '''data: BitVector1*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="IsM1GreaterThanM2")>  _
    Public Shared Function IsM1GreaterThanM2(<System.Runtime.InteropServices.InAttribute()> ByRef data As BitVector1) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
    End Function
    
    '''Return Type: BitVector2
    '''m1: DWORD->unsigned int
    '''m2: BOOL->int
    '''m3: DWORD->unsigned int
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CreateBitVector2")>  _
    Public Shared Function CreateBitVector2(ByVal m1 As UInteger, <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> ByVal m2 As Boolean, ByVal m3 As UInteger) As BitVector2
    End Function
    
    '''Return Type: BOOL->int
    '''orig: LPCWSTR->WCHAR*
    '''buffer: LPWSTR->WCHAR*
    '''size: int
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="ReverseString")>  _
    Public Overloads Shared Function ReverseString(<System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal orig As String, <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal buffer As System.Text.StringBuilder, ByVal size As Integer) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)> Boolean
    End Function
    
    '''Return Type: boolean
    '''orig: LPCWSTR->WCHAR*
    '''size: int*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CalculateStringLength")>  _
    Public Shared Function CalculateStringLength(<System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal orig As String, <System.Runtime.InteropServices.OutAttribute()> ByRef size As Integer) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: int
    '''buffer: LPWSTR->WCHAR*
    '''size: int
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="GetVeryLongString")>  _
    Public Overloads Shared Function GetVeryLongString(<System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal buffer As System.Text.StringBuilder, ByVal size As Integer) As Integer
    End Function
    
    '''Return Type: unsigned int
    '''buffer: LPWSTR->WCHAR*
    '''size: unsigned int
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="GetVeryLongString2")>  _
    Public Overloads Shared Function GetVeryLongString2(<System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal buffer As System.Text.StringBuilder, ByVal size As UInteger) As UInteger
    End Function
    
    '''Return Type: boolean
    '''i: int
    '''d: double
    '''s: s1*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="s1FakeConstructor")>  _
    Public Shared Function s1FakeConstructor(ByVal i As Integer, ByVal d As Double, <System.Runtime.InteropServices.OutAttribute()> ByRef s As s1) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: s1
    '''i: int
    '''d: double
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="s1FakeConstructor2")>  _
    Public Shared Function s1FakeConstructor2(ByVal i As Integer, ByVal d As Double) As s1
    End Function
    
    '''Return Type: boolean
    '''i: int
    '''data: LPCWSTR->WCHAR*
    '''s: s2*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="s2FakeConstructor")>  _
    Public Shared Function s2FakeConstructor(ByVal i As Integer, <System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal data As String, <System.Runtime.InteropServices.OutAttribute()> ByRef s As s2) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: int
    '''s: s4
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="s4Add")>  _
    Public Shared Function s4Add(ByVal s As s4) As Integer
    End Function
    
    '''Return Type: boolean
    '''s: s3*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyM1ToM2")>  _
    Public Shared Function CopyM1ToM2(ByRef s As s3) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: boolean
    '''p: int*
    '''sum: int*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="SumArray")>  _
    Public Shared Function SumArray(<System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPArray, ArraySubType:=System.Runtime.InteropServices.UnmanagedType.I4, SizeConst:=4)> ByVal p() As Integer, <System.Runtime.InteropServices.OutAttribute()> ByRef sum As Integer) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: boolean
    '''p: int*
    '''size: int
    '''sum: int*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="SumArray2")>  _
    Public Shared Function SumArray2(<System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPArray, ArraySubType:=System.Runtime.InteropServices.UnmanagedType.I4, SizeParamIndex:=1)> ByVal p() As Integer, ByVal size As Integer, <System.Runtime.InteropServices.OutAttribute()> ByRef sum As Integer) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: boolean
    '''toRet: WCHAR->wchar_t->unsigned short
    '''c: WCHAR**
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="GetPointerPointerToChar")>  _
    Public Shared Function GetPointerPointerToChar(ByVal toRet As Char, ByRef c As System.IntPtr) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: boolean
    '''dec: DECIMAL->tagDEC
    '''pDec: LPDECIMAL->DECIMAL*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyDecimalToPoiner")>  _
    Public Shared Function CopyDecimalToPoiner(ByVal dec As Decimal, ByRef pDec As Decimal) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: DECIMAL->tagDEC
    '''dec: DECIMAL->tagDEC
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyDecimalToReturn")>  _
    Public Shared Function CopyDecimalToReturn(ByVal dec As Decimal) As Decimal
    End Function
    
    '''Return Type: boolean
    '''pDec1: LPDECIMAL->DECIMAL*
    '''pDec2: LPDECIMAL->DECIMAL*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyDecimalPointerToPointer")>  _
    Public Shared Function CopyDecimalPointerToPointer(<System.Runtime.InteropServices.InAttribute()> ByRef pDec1 As Decimal, ByRef pDec2 As Decimal) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: boolean
    '''c: CURRENCY->CY->tagCY
    '''pCur: CURRENCY*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyCurrencyToPointer")>  _
    Public Shared Function CopyCurrencyToPointer(<System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Currency)> ByVal c As Decimal, <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Currency)> ByRef pCur As Decimal) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: boolean
    '''src: BSTR->OLECHAR*
    '''dest: LPWSTR->WCHAR*
    '''size: unsigned int*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyBstrToNoramlStr")>  _
    Public Overloads Shared Function CopyBstrToNoramlStr(<System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)> ByVal src As String, <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal dest As System.Text.StringBuilder, ByRef size As UInteger) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: boolean
    '''src: LPCWSTR->WCHAR*
    '''dest: BSTR*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyToBstr")>  _
    Public Shared Function CopyToBstr(<System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal src As String, <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr), System.Runtime.InteropServices.OutAttribute()> ByRef dest As String) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: boolean
    '''src1: LPCWSTR->WCHAR*
    '''src2: LPCWSTR->WCHAR*
    '''dest: BSTR*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyBothToBstr")>  _
    Public Shared Function CopyBothToBstr(<System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal src1 As String, <System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal src2 As String, <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)> ByRef dest As String) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: boolean
    '''src: BSTR->OLECHAR*
    '''dest: BSTR*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyBstrToBstr")>  _
    Public Shared Function CopyBstrToBstr(<System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)> ByVal src As String, <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)> ByRef dest As String) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: BSTR->OLECHAR*
    '''src: LPCWSTR->WCHAR*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CopyNormalStrToBstrRet")>  _
    Public Shared Function CopyNormalStrToBstrRet(<System.Runtime.InteropServices.InAttribute(), System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal src As String) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.BStr)> String
    End Function
    
    '''Return Type: POPAQUE1->opaque1*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="CreateBasicOpaque")>  _
    Public Shared Function CreateBasicOpaque() As System.IntPtr
    End Function
    
    '''Return Type: boolean
    '''p1: POPAQUE1->opaque1*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="VerifyBasicOpaque")>  _
    Public Shared Function VerifyBasicOpaque(ByVal p1 As System.IntPtr) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: pFunctionPointerReturningInt
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="GetFunctionPointerReturningInt")>  _
    Public Shared Function GetFunctionPointerReturningInt() As pFunctionPointerReturningInt
    End Function
    
    '''Return Type: boolean
    '''pFPtr: pFunctionPointerReturningInt
    '''value: int
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="AreResultAndValueEqual")>  _
    Public Shared Function AreResultAndValueEqual(ByVal pFPtr As pFunctionPointerReturningInt, ByVal value As Integer) As <System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)> Boolean
    End Function
    
    '''Return Type: void
    '''param0: int
    '''ps: structWithFunctionPointer*
    <System.Runtime.InteropServices.DllImportAttribute("PInvokeTestLib.dll", EntryPoint:="GetAStructWithASimpleFunctionPointer")>  _
    Public Shared Sub GetAStructWithASimpleFunctionPointer(ByVal param0 As Integer, ByRef ps As structWithFunctionPointer)
    End Sub
    
    <System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.CodeDom.Compiler.GeneratedCodeAttribute("InteropSignatureToolkit", "0.9 Beta1")>  _
    Public Overloads Shared Function ReverseString(ByVal orig As String, ByRef buffer As String) As Boolean
        Dim varbuffer As System.Text.StringBuilder = New System.Text.StringBuilder(1024)
        Dim methodRetVar As Boolean
        methodRetVar = NativeMethods.ReverseString(orig, varbuffer, 1024)
        buffer = varbuffer.ToString
        Return methodRetVar
    End Function
    
    <System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.CodeDom.Compiler.GeneratedCodeAttribute("InteropSignatureToolkit", "0.9 Beta1")>  _
    Public Overloads Shared Function GetVeryLongString(ByRef buffer As String) As Integer
        Dim varbuffer As System.Text.StringBuilder
        Dim retVar_ As Integer
        Dim sizeVar As Integer = 2056
    PerformCall:
        varbuffer = New System.Text.StringBuilder(sizeVar)
        retVar_ = NativeMethods.GetVeryLongString(varbuffer, sizeVar)
        If (retVar_ >= sizeVar) Then
            sizeVar = (retVar_ + 1)
            goto PerformCall
        End If
        buffer = varbuffer.ToString
        Return retVar_
    End Function
    
    <System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.CodeDom.Compiler.GeneratedCodeAttribute("InteropSignatureToolkit", "0.9 Beta1")>  _
    Public Overloads Shared Function GetVeryLongString2(ByRef buffer As String) As UInteger
        Dim varbuffer As System.Text.StringBuilder
        Dim retVar_ As UInteger
        Dim sizeVar As UInteger = 2056
    PerformCall:
        varbuffer = New System.Text.StringBuilder(CType(sizeVar,Integer))
        retVar_ = NativeMethods.GetVeryLongString2(varbuffer, sizeVar)
        If (retVar_ >= sizeVar) Then
            sizeVar = retVar_
            goto PerformCall
        End If
        buffer = varbuffer.ToString
        Return retVar_
    End Function
    
    <System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.CodeDom.Compiler.GeneratedCodeAttribute("InteropSignatureToolkit", "0.9 Beta1")>  _
    Public Overloads Shared Function CopyBstrToNoramlStr(ByVal src As String, ByRef dest As String) As Boolean
        Dim vardest As System.Text.StringBuilder
        Dim retVar_ As Boolean
        Dim sizeVar As UInteger = 2056
        Dim oldSizeVar_ As UInteger
    PerformCall:
        oldSizeVar_ = sizeVar
        vardest = New System.Text.StringBuilder(CType(sizeVar,Integer))
        retVar_ = NativeMethods.CopyBstrToNoramlStr(src, vardest, sizeVar)
        If (oldSizeVar_ <= sizeVar) Then
            sizeVar = (sizeVar * CType(2,UInteger))
            goto PerformCall
        End If
        dest = vardest.ToString
        Return retVar_
    End Function
End Class
