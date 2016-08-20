
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
using PInvoke;

internal class TypePair<K, V>
{
	public K Key;
	public V Value;
}

/// <summary>
/// Used to compare NativeType trees for equality
/// </summary>
/// <remarks></remarks>
public class NativeTypeEqualityComparer : IEqualityComparer<NativeType>
{

	/// <summary>
	/// Type of comparer
	/// </summary>
	/// <remarks></remarks>
	private enum ComparerType
	{
		TopLevel,
		Recursive
	}

	public static NativeTypeEqualityComparer TopLevel {
		get { return new NativeTypeEqualityComparer(ComparerType.TopLevel); }
	}

	public static NativeTypeEqualityComparer Recursive {
		get { return new NativeTypeEqualityComparer(ComparerType.Recursive); }
	}


	private ComparerType _type;
	private NativeTypeEqualityComparer(ComparerType type)
	{
		_type = type;
	}

	#region "Shared Methods"

	public static bool AreEqualRecursive(NativeType left, NativeType right)
	{
		return Recursive.Equals1(left, right);
	}

	public static bool AreEqualTopLevel(NativeType left, NativeType right)
	{
		return TopLevel.Equals1(left, right);
	}

	#endregion

	#region "IEqualityComparer"

	public bool Equals1(NativeType x, NativeType y)
	{

		// Standard null checks
		if (x == null || y == null) {
			return x == null && y == null;
		}

		switch (_type) {
			case ComparerType.Recursive:
				return EqualsRecursive(x, y);
			case ComparerType.TopLevel:
				return EqualsTopLevel(x, y);
			default:
				throw new Exception("invalid enum");
		}

	}
	bool System.Collections.Generic.IEqualityComparer<NativeType>.Equals(NativeType x, NativeType y)
	{
		return Equals1(x, y);
	}

	public int GetHashCode1(NativeType obj)
	{
		return obj.GetHashCode();
	}
	int System.Collections.Generic.IEqualityComparer<NativeType>.GetHashCode(NativeType obj)
	{
		return GetHashCode1(obj);
	}

	#endregion

	#region "Private Methods"

	private bool EqualsRecursive(NativeType left, NativeType right)
	{

		// Quick sanity check
		if (!EqualsTopLevel(left, right)) {
			return false;
		}

		NativeSymbolIterator it = new NativeSymbolIterator();

		Dictionary<string, NativeDefinedType> leftMap = new Dictionary<string, NativeDefinedType>();
		Dictionary<string, NativeDefinedType> rightMap = new Dictionary<string, NativeDefinedType>();

		foreach (NativeDefinedType nt in it.FindAllReachableDefinedTypes(left)) {
			if (!leftMap.ContainsKey(nt.Name)) {
				leftMap.Add(nt.Name, nt);
			}
		}

		foreach (NativeDefinedType nt in it.FindAllReachableDefinedTypes(right)) {
			if (!rightMap.ContainsKey(nt.Name)) {
				rightMap.Add(nt.Name, nt);
			}
		}

		if (leftMap.Count != rightMap.Count) {
			return false;
		}

		foreach (NativeDefinedType leftDefined in leftMap.Values) {
			NativeDefinedType rightDefined = null;
			if (!rightMap.TryGetValue(leftDefined.Name, rightDefined) || !EqualsTopLevel(leftDefined, rightDefined)) {
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Top level takes a very shallow look at the type.  When it encounters a nested defined type, it will only compare the 
	/// name's.  In that case NativeNamedType instances and NativeDefinedType instances will compare true if they match on name 
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <returns></returns>
	/// <remarks></remarks>
	private bool EqualsTopLevel(NativeType left, NativeType right)
	{

		left = DigThroughNamedType(left);
		right = DigThroughNamedType(right);

		if (left.Kind != right.Kind) {
			return false;
		}

		if (!EqualsCore(left, right)) {
			return false;
		}

		// If this is a defined type then make sure the members compare
		if (left.Category == NativeSymbolCategory.Defined) {
			NativeDefinedType leftDefined = (NativeDefinedType)left;
			NativeDefinedType rightDefined = (NativeDefinedType)right;

			if (leftDefined.Members.Count != rightDefined.Members.Count) {
				return false;
			}

			for (int i = 0; i <= leftDefined.Members.Count - 1; i++) {
				NativeMember leftMember = leftDefined.Members(i);
				NativeMember rightMember = rightDefined.Members(i);

				if (0 != string.CompareOrdinal(leftMember.Name, rightMember.Name) || !EqualsCore(leftMember.NativeType, rightMember.NativeType)) {
					return false;
				}
			}
		}

		return true;
	}

	private NativeType DigThroughNamedType(NativeType nt)
	{
		while ((NativeSymbolKind.NamedType == nt.Kind)) {
			NativeNamedType namedtype = (NativeNamedType)nt;
			if (namedtype.RealType == null) {
				break; // TODO: might not be correct. Was : Exit While
			}

			nt = namedtype.RealType;
		}

		return nt;
	}

	private bool EqualsCore(NativeType left, NativeType right)
	{
		left = DigThroughNamedType(left);
		right = DigThroughNamedType(right);

		if (left.Kind != right.Kind) {

			if ((left.Kind == NativeSymbolKind.NamedType && right.Category == NativeSymbolCategory.Defined) || (left.Category == NativeSymbolCategory.Defined && right.Kind == NativeSymbolKind.NamedType)) {
				return 0 == string.CompareOrdinal(left.Name, right.Name);
			}

			return false;
		}

		switch (left.Category) {
			case NativeSymbolCategory.Defined:
				return EqualsDefinedCore((NativeDefinedType)left, (NativeDefinedType)right);
			case NativeSymbolCategory.Proxy:
				return EqualsProxyCore((NativeProxyType)left, (NativeProxyType)right);
			case NativeSymbolCategory.Specialized:
				return EqualsSpecializedCore((NativeSpecializedType)left, (NativeSpecializedType)right);
			default:
				throw new Exception("error");
		}
	}

	private bool EqualsDefinedCore(NativeDefinedType left, NativeDefinedType right)
	{

		if (left.IsAnonymous && right.IsAnonymous) {
			// don't compare names when both types are anonymous
		} else if (0 != string.CompareOrdinal(left.Name, right.Name)) {
			return false;
		}

		// If this is an enum, compare the values
		if (left.Kind == NativeSymbolKind.EnumType) {
			NativeEnum leftEnum = (NativeEnum)left;
			NativeEnum rightEnum = (NativeEnum)right;

			if (rightEnum.Values.Count != leftEnum.Values.Count) {
				return false;
			}

			for (int i = 0; i <= leftEnum.Values.Count - 1; i++) {
				NativeEnumValue e1 = leftEnum.Values(i);
				NativeEnumValue e2 = rightEnum.Values(i);

				if (0 != string.CompareOrdinal(e1.Name, e2.Name) || 0 != string.CompareOrdinal(e1.Value.Expression, e2.Value.Expression)) {
					return false;
				}
			}
		}

		return true;
	}

	private bool EqualsProxyCore(NativeProxyType left, NativeProxyType right)
	{
		bool ret = false;
		switch (left.Kind) {
			case NativeSymbolKind.ArrayType:
				NativeArray a1 = (NativeArray)left;
				NativeArray a2 = (NativeArray)right;

				ret = a1.ElementCount == a2.ElementCount;
				break;
			case NativeSymbolKind.NamedType:
				ret = (0 == string.CompareOrdinal(((NativeNamedType)left).Name, ((NativeNamedType)right).Name));
				break;
			case NativeSymbolKind.TypedefType:
				ret = (0 == string.CompareOrdinal(((NativeTypeDef)left).Name, ((NativeTypeDef)right).Name));
				break;
			case NativeSymbolKind.PointerType:
				ret = true;
				break;
			default:
				ret = false;
				break;
		}

		if (!ret) {
			return false;
		}

		if (left.RealType == null && right.RealType == null) {
			return ret;
		}

		if (left.RealType == null || right.RealType == null) {
			return false;
		}

		return EqualsCore(left.RealType, right.RealType);
	}

	private bool EqualsSpecializedCore(NativeSpecializedType left, NativeSpecializedType right)
	{
		switch (left.Kind) {
			case NativeSymbolKind.BitVectorType:
				NativeBitVector bt1 = (NativeBitVector)left;
				NativeBitVector bt2 = (NativeBitVector)right;
				return bt1.Size == bt2.Size;
			case NativeSymbolKind.BuiltinType:
				NativeBuiltinType b1 = (NativeBuiltinType)left;
				NativeBuiltinType b2 = (NativeBuiltinType)right;
				return b1.BuiltinType == b2.BuiltinType;
			default:
				return false;
		}
	}

	#endregion

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
