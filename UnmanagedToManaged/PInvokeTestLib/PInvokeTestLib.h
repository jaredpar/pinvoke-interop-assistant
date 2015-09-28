/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the PINVOKETESTLIB_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// PINVOKETESTLIB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef PINVOKETESTLIB_EXPORTS
#define PINVOKETESTLIB_API __declspec(dllexport)
#else
#define PINVOKETESTLIB_API __declspec(dllimport)
#endif

#define _countof(x) (sizeof(x) / sizeof(x[0]))

#define foo "bar"
#define foo2 "bar2"

#include <sal.h>
#include "objbase.h"

extern "C"
{

/* Start: BitVector Tests */
struct BitVector1
{
	DWORD m1 : 5;
	DWORD m2 : 6;
};

PINVOKETESTLIB_API
BOOL
UpdateBitVector1Data(__in BitVector1 *data);

PINVOKETESTLIB_API
BOOL
IsM1GreaterThanM2(__in BitVector1 *data);

// two bitvectors around a non bitvector
struct BitVector2
{
	DWORD m1 : 2;
	BOOL m2;
	DWORD m3: 2;
};

PINVOKETESTLIB_API
BitVector2
CreateBitVector2(DWORD m1, BOOL m2, DWORD m3);

/* End: BitVector Tests */

/* Start: String Tests */

PINVOKETESTLIB_API 
BOOL
ReverseString(
		  __in LPCWSTR orig, 
		  __out_ecount(size) LPWSTR buffer, 
		  int size);

PINVOKETESTLIB_API
bool
CalculateStringLength(
		  __in LPCWSTR orig,
		  __out_ecount(1) int *size);

PINVOKETESTLIB_API
int
GetVeryLongString(
		  __out_ecount_part(size,return+1) LPWSTR buffer,
		  int size);

// Same but with unsigned types and a straight return
PINVOKETESTLIB_API
unsigned
GetVeryLongString2(
		  __out_ecount_part(size,return) LPWSTR buffer,
		  unsigned size);

struct stringInStruct
{
	wchar_t *m1;
};

PINVOKETESTLIB_API
bool
VerifyStringInStructM1(stringInStruct s1, const wchar_t* comp);

struct structWithDiffStringTypes
{
	char *m1;
	wchar_t *m2;
};

PINVOKETESTLIB_API
void 
PopulateStructWithDiffStringTypes(structWithDiffStringTypes *s1, const char *m1, const wchar_t *m2);

/* End: String Tests */

/* Start: Struct Tests */

struct s1
{
	int m1;
	double m2;
};

PINVOKETESTLIB_API
bool
s1FakeConstructor(int i, double d, __out_ecount(1) s1 *s);

PINVOKETESTLIB_API
s1
s1FakeConstructor2(int i, double d);

/* End: Struct Tests */


/* Start: Inline Arrays within structs */
struct s2
{
	int m1;
	wchar_t m2[250];
};

PINVOKETESTLIB_API
bool
s2FakeConstructor(int i, LPCWSTR data, __out_ecount(1) s2 *s);

struct s3
{
	int m1[4];
	double m2[4];
};

// Make sure that we don't convert the BYTE[] into a String
struct s4
{
	BYTE m1[4];
};
PINVOKETESTLIB_API
int
s4Add(s4 s);

PINVOKETESTLIB_API
bool
CopyM1ToM2(s3* s);

/* End: Inline Arrays within structs */

/* Start: Union Values */

/* End : Union Values */

/* Start: Enum Values */
#define VALUE_CONSTANT_1  5

enum e1
{
	v1,
	v2 = VALUE_CONSTANT_1
};


/* End: Enum Values */

/* Start: Array Param */

PINVOKETESTLIB_API
bool
SumArray(__in_ecount(4) int* p, __out int *sum);

PINVOKETESTLIB_API
bool
SumArray2(__in_ecount(size) int* p, int size, __out int *sum);

/* End: Array Param */

/* Start: Pointer Pointer */

PINVOKETESTLIB_API
bool
GetPointerPointerToChar(WCHAR toRet, WCHAR** c);

/* End: Pointer Pointer */

/* Start: Better managed types */

PINVOKETESTLIB_API
bool 
CopyDecimalToPoiner(DECIMAL dec, __out LPDECIMAL pDec);

PINVOKETESTLIB_API
DECIMAL
CopyDecimalToReturn(DECIMAL dec);

PINVOKETESTLIB_API
bool
CopyDecimalPointerToPointer(__in LPDECIMAL pDec1, __out LPDECIMAL pDec2);

PINVOKETESTLIB_API
bool
CopyCurrencyToPointer(CURRENCY c, __out CURRENCY *pCur);

/* End: Better managed types */

/* Start: BSTR */

PINVOKETESTLIB_API
bool
CopyBstrToNoramlStr(BSTR src, __out_ecount_part(*size,*size+1) LPWSTR dest, unsigned *size);

PINVOKETESTLIB_API
bool
CopyToBstr(LPCWSTR src, __out BSTR* dest);

PINVOKETESTLIB_API
bool
CopyBothToBstr(LPCWSTR src1, LPCWSTR src2, BSTR* dest); 

PINVOKETESTLIB_API
bool
CopyBstrToBstr(BSTR src, BSTR* dest);

PINVOKETESTLIB_API
BSTR
CopyNormalStrToBstrRet(LPCWSTR src);

/* End: BSTR */

/* Start: Opaque type support */

typedef struct opaque1 *POPAQUE1;

PINVOKETESTLIB_API
POPAQUE1
CreateBasicOpaque();

PINVOKETESTLIB_API
bool 
VerifyBasicOpaque(POPAQUE1 p1);

/* End: Opaque type support */

/* Start: Function Pointer support */

typedef int (*pFunctionPointerReturningInt)();
PINVOKETESTLIB_API
pFunctionPointerReturningInt
GetFunctionPointerReturningInt();

PINVOKETESTLIB_API
bool
AreResultAndValueEqual(pFunctionPointerReturningInt pFPtr, int value);

struct structWithFunctionPointer
{
	int m1;
	int (*pAddTheValues)(int,int);
};

PINVOKETESTLIB_API
void
GetAStructWithASimpleFunctionPointer(int, __inout structWithFunctionPointer *ps);

/* End: Function Pointer support */

/* Start: Calling Conventions */
PINVOKETESTLIB_API
int
__cdecl
MultiplyWithCDecl(int x, int y);
/* End: Calling Conventions */

/* Start: Class */

class simpleClass 
{
public:
	int m1;
	int m2;
};

PINVOKETESTLIB_API
int GetSimpleClassM1(simpleClass c1);

PINVOKETESTLIB_API
int GetSimpleClassM2(simpleClass c1);

/* End: Class */

}