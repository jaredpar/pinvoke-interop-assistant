/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

// PInvokeTestLib.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "PInvokeTestLib.h"
#include "objbase.h"
#include <atlbase.h>
#include <strsafe.h>

#ifdef _MANAGED
#pragma managed(push, off)
#endif

#define IfFailGo(x) hr = (x); if ( FAILED(hr) ) { goto Error; }
#define IfFalseGo(expr,HR) if ( !(expr) ) { hr = HR; goto Error; }

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif

void WaitForInput()
{
	Sleep(100000);
}

PINVOKETESTLIB_API
BOOL
UpdateBitVector1Data(__in BitVector1 *data)
{
	if ( !data )
	{
		return FALSE;
	}

	data->m1 = 5;
	data->m2 = 42;
	return TRUE;
}

PINVOKETESTLIB_API
BOOL
IsM1GreaterThanM2(__in BitVector1 *data)
{
	if ( !data )
	{
		return FALSE;
	}

	if ( data->m1 > data->m2 ) 
	{
		return TRUE;
	}

	return FALSE;
}

PINVOKETESTLIB_API 
BOOL
ReverseString(
		  __in LPCWSTR orig, 
		  __out_ecount(size) LPWSTR buffer, 
		  int size)
{
	if ( !orig || !buffer )
	{
		return FALSE;
	}

	HRESULT hr;
	size_t len = 0;
	IfFailGo(StringCchLengthW(orig, 255, &len));
	IfFalseGo(len + 1 < (size_t)size, E_FAIL);
	for ( size_t cur = 0; cur < len; ++cur )
	{
		buffer[len-cur-1] = orig[cur];
	}

	buffer[len] = L'\0';
	return TRUE;

Error:
	buffer[0] = L'\0';
	return FALSE;
}

PINVOKETESTLIB_API
bool
CalculateStringLength(
		  __in LPCWSTR orig,
		  __out_ecount(1) int *size)
{
	if ( !orig || !size)
	{
		return false;
	}

	*size = 0;
	size_t tempSize;

	if ( FAILED(StringCchLengthW(orig, 255, &tempSize)))
	{
		return false;
	}

	*size = (int)tempSize;
	return TRUE;
}

PINVOKETESTLIB_API
bool
s1FakeConstructor(int i, double d, __out_ecount(1) s1 *s)
{
	if (!s)
	{
		return false;
	}

	s->m1 = i;
	s->m2 = d;
	return true;
}

PINVOKETESTLIB_API
s1
s1FakeConstructor2(int i, double d)
{
	s1 s;
	s.m1 = i;
	s.m2 = d;
	return s;
}

PINVOKETESTLIB_API
bool
s2FakeConstructor(int i, LPCWSTR data, __out_ecount(1) s2 *s)
{
	if ( !s || !data)
	{
		return false;
	}

	StringCchCopy(s->m2, _countof(s->m2), data);
	s->m1 =i;
	return true;
}


PINVOKETESTLIB_API
bool
CopyM1ToM2(s3* s)
{
	if ( !s )
	{
		return false;
	}

	for ( int i = 0; i < _countof(s->m1); ++i )
	{
		s->m2[i] = s->m1[i];
	}

	return true;
}

PINVOKETESTLIB_API
BitVector2
CreateBitVector2(DWORD m1, BOOL m2, DWORD m3)
{
	BitVector2 b;
	b.m1 = m1;
	b.m2 = m2;
	b.m3 = m3;
	return b;
}


PINVOKETESTLIB_API
bool
SumArray(__in_ecount(4) int* p, __out int *sum)
{
	 if ( !p || !sum )
	 {
		return false;
	 }

	 *sum = 0;
	 for (int i = 0; i < 4; ++i )
	 {
		 *sum = *sum + p[i];
	 }

	 return true;
}

PINVOKETESTLIB_API
bool
SumArray2(__in_ecount(size) int* p, int size, __out int *sum)
{
	 if ( !p || !sum )
	 {
		return false;
	 }

	 *sum = 0;
	 for (int i = 0; i < size; ++i )
	 {
		 *sum = *sum + p[i];
	 }

	 return true;
}

PINVOKETESTLIB_API
int
s4Add(s4 s)
{
	int total = 0;
	for ( int i = 0; i < 4; ++i )
	{
		total += s.m1[i];
	}

	return total;
}

PINVOKETESTLIB_API
int
GetVeryLongString(
		  __out_ecount_part(size,return+1) LPWSTR buffer,
		  int size)
{
	if ( size < 4000) 
	{
		return 4000;
	}

	for ( int i = 0; i < 100; ++i)
	{
		int cur = i % 3;
		switch ( cur )
		{
		case 1:
			buffer[i] = L'1';
			break;
		case 2:
			buffer[i] = L'2';
			break;
		case 0:
			buffer[i] = L'0';
			break;
		}

	}

	buffer[100] = L'\0';
	return 99;
}

PINVOKETESTLIB_API
unsigned 
GetVeryLongString2(
		  __out_ecount_part(size,return) LPWSTR buffer,
		  unsigned size)
{
	if ( size < 4000) 
	{
		return 4000;
	}

	for ( unsigned i = 0; i < 100; ++i)
	{
		int cur = i % 3;
		switch ( cur )
		{
		case 1:
			buffer[i] = L'1';
			break;
		case 2:
			buffer[i] = L'2';
			break;
		case 0:
			buffer[i] = L'0';
			break;
		}

	}

	buffer[100] = L'\0';
	return 99;
}

PINVOKETESTLIB_API
bool
VerifyStringInStructM1(stringInStruct s1, const wchar_t* comp)
{
	return 0 == wcscmp(s1.m1, comp); 
}

PINVOKETESTLIB_API
void 
PopulateStructWithDiffStringTypes(structWithDiffStringTypes *s1, const char *m1, const wchar_t *m2)
{
	s1->m1 = strdup(m1);
	s1->m2 = wcsdup(m2);
}

PINVOKETESTLIB_API
bool
GetPointerPointerToChar(WCHAR toRet, WCHAR** c) 
{
	if ( !c )
	{
		return false;
	}

	*c = (WCHAR*)::CoTaskMemAlloc(sizeof(WCHAR));
	if (!*c)
	{
		return false;
	}

	**c = toRet; 
	return true;
}

PINVOKETESTLIB_API
bool 
CopyDecimalToPoiner(DECIMAL dec, __out LPDECIMAL pDec)
{
	if ( !pDec )
	{
		return false;
	}

	*pDec = dec;
	return true;
}

PINVOKETESTLIB_API
DECIMAL
CopyDecimalToReturn(DECIMAL dec)
{
	return dec;
}

PINVOKETESTLIB_API
bool
CopyDecimalPointerToPointer(__in LPDECIMAL pDec1, __out LPDECIMAL pDec2)
{
	if ( !pDec1 || !pDec2 )
	{
		return false;
	}

	*pDec2 = *pDec1;
	return true;
}

PINVOKETESTLIB_API
bool
CopyCurrencyToPointer(CURRENCY c, __out CURRENCY *pCur)
{
	if ( !pCur )
	{
		return false;
	}

	*pCur = c;
	return true;
}

PINVOKETESTLIB_API
bool
CopyBstrToNoramlStr(BSTR src, __out_ecount_part(*size,*size+1) LPWSTR dest, unsigned *size)
{
	if ( !src || !dest || !size )
	{
		return false;
	}

	CComBSTR d = src;
	if ( *size < d.Length() )
	{
		*size = d.Length();
		return false;
	}


	::StringCchCopy(dest, *size, d);
	*size = d.Length();
	return true;
}

PINVOKETESTLIB_API
bool
CopyToBstr(LPCWSTR src, __out BSTR* dest)
{
	if ( !src || !dest )
	{
		return false;
	}

	CComBSTR d;
	d = src;
	*dest = d.Detach();
	return true;
}

PINVOKETESTLIB_API
bool
CopyBothToBstr(LPCWSTR src1, LPCWSTR src2, BSTR* dest) 
{
	if ( !src1 || !src2 || !dest )
	{
		return false;
	}

	CComBSTR d(L"");
	HRESULT hr;
	IfFailGo(d.Append(src1));
	IfFailGo(d.Append(src2));
	*dest = d.Detach();
	return true;

Error:
	return false;
}

PINVOKETESTLIB_API
bool
CopyBstrToBstr(BSTR src, BSTR* dest)
{
	if ( !src || !dest )
	{
		return false;
	}

	*dest = ::SysAllocString(src);
	return *dest != NULL; 
}

PINVOKETESTLIB_API
BSTR
CopyNormalStrToBstrRet(LPCWSTR src)
{
	return ::SysAllocString(src);
}

struct opaque1
{
	bool isValid;
};

PINVOKETESTLIB_API
POPAQUE1
CreateBasicOpaque()
{
	opaque1 *p = new opaque1();
	p->isValid = true;
	return p;
}

PINVOKETESTLIB_API
bool 
VerifyBasicOpaque(POPAQUE1 p1)
{
	if ( !p1 || !p1->isValid )
	{
		return false;
	}

	return true;
}

int GetFunctionPointerReturningIntImpl()
{
	return 42;
}

PINVOKETESTLIB_API
pFunctionPointerReturningInt
GetFunctionPointerReturningInt()
{
	return GetFunctionPointerReturningIntImpl;
}

PINVOKETESTLIB_API
bool
AreResultAndValueEqual(pFunctionPointerReturningInt pFPtr, int value)
{
	if ( !pFPtr)
	{
		return false;
	}

	return pFPtr() == value;
}

int SumTheValuesImpl(int left, int right)
{
	return left + right;
}

PINVOKETESTLIB_API
void
GetAStructWithASimpleFunctionPointer(int value, __inout structWithFunctionPointer *ps)
{
	ps->m1 = value;
	ps->pAddTheValues = SumTheValuesImpl;
}

PINVOKETESTLIB_API
int
__cdecl
MultiplyWithCDecl(int x, int y)
{
	return x* y;
}

PINVOKETESTLIB_API
int GetSimpleClassM1(simpleClass c1)
{
	return c1.m1;
}

PINVOKETESTLIB_API
int GetSimpleClassM2(simpleClass c1)
{
	return c1.m2;
}