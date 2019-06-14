using PInvoke.Enums;
using PInvoke.NativeTypes;
using PInvoke.NativeTypes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{

    public static class NativeNameUtil
    {
        public static NativeSymbolKind GetNativeSymbolKind(NativeNameKind kind)
        {
            switch (kind)
            {
                case NativeNameKind.Struct: return NativeSymbolKind.StructType;
                case NativeNameKind.Union: return NativeSymbolKind.UnionType;
                case NativeNameKind.FunctionPointer: return NativeSymbolKind.FunctionPointer;
                case NativeNameKind.Procedure: return NativeSymbolKind.Procedure;
                case NativeNameKind.TypeDef: return NativeSymbolKind.TypeDefType;
                case NativeNameKind.Constant: return NativeSymbolKind.Constant;
                case NativeNameKind.Enum: return NativeSymbolKind.EnumType;
                case NativeNameKind.EnumValue: return NativeSymbolKind.EnumNameValue;
                default: throw Contract.CreateInvalidEnumValueException(kind);
            }
        }

        public static bool TryGetNativeNameKind(NativeSymbolKind symbolKind, out NativeNameKind nameKind)
        {
            switch (symbolKind)
            {
                case NativeSymbolKind.StructType:
                    nameKind = NativeNameKind.Struct;
                    return true;
                case NativeSymbolKind.EnumType:
                    nameKind = NativeNameKind.Enum;
                    return true;
                case NativeSymbolKind.UnionType:
                    nameKind = NativeNameKind.Union;
                    return true;
                case NativeSymbolKind.TypeDefType:
                    nameKind = NativeNameKind.TypeDef;
                    return true;
                case NativeSymbolKind.Procedure:
                    nameKind = NativeNameKind.Procedure;
                    return true;
                case NativeSymbolKind.FunctionPointer:
                    nameKind = NativeNameKind.FunctionPointer;
                    return true;
                case NativeSymbolKind.EnumNameValue:
                    nameKind = NativeNameKind.EnumValue;
                    return true;
                case NativeSymbolKind.Constant:
                    nameKind = NativeNameKind.Constant;
                    return true;
                case NativeSymbolKind.ArrayType:
                case NativeSymbolKind.PointerType:
                case NativeSymbolKind.BuiltinType:
                case NativeSymbolKind.BitVectorType:
                case NativeSymbolKind.NamedType:
                case NativeSymbolKind.ProcedureSignature:
                case NativeSymbolKind.Parameter:
                case NativeSymbolKind.Member:
                case NativeSymbolKind.SalEntry:
                case NativeSymbolKind.SalAttribute:
                case NativeSymbolKind.ValueExpression:
                case NativeSymbolKind.Value:
                case NativeSymbolKind.OpaqueType:
                    nameKind = (NativeNameKind)0;
                    return false;
                default:
                    Contract.ThrowInvalidEnumValue(symbolKind);
                    nameKind = (NativeNameKind)0;
                    return false;
            }
        }

        public static bool TryGetName(NativeSymbol symbol, out NativeName name)
        {
            if (!TryGetNativeNameKind(symbol.Kind, out NativeNameKind kind))
            {
                name = NativeName.Nil;
                return false;
            }

            name = new NativeName(symbol.Name, kind);
            return true;
        }

        public static NativeName GetName(NativeSymbol symbol)
        {
            if (!TryGetName(symbol, out NativeName name))
            {
                throw new Exception($"Unable to create name for {symbol.Name} {symbol.Kind}");
            }

            return name;
        }

        public static IEnumerable<NativeName> GetNames(IEnumerable<NativeSymbol> symbols)
        {
            foreach (var symbol in symbols)
            {
                if (TryGetName(symbol, out NativeName name))
                {
                    yield return name;
                }
            }
        }

        public static IEnumerable<NativeName> GetNames(params IEnumerable<NativeSymbol>[] col)
        {
            foreach (var item in col)
            {
                foreach (var name in GetNames(item))
                {
                    yield return name;
                }
            }
        }

        public static bool IsAnyType(NativeNameKind kind)
        {
            switch (kind)
            {
                case NativeNameKind.Struct:
                case NativeNameKind.Union:
                case NativeNameKind.FunctionPointer:
                case NativeNameKind.TypeDef:
                    return true;
                case NativeNameKind.Procedure:
                case NativeNameKind.Constant:
                case NativeNameKind.Enum:
                case NativeNameKind.EnumValue:
                    return false;
                default:
                    Contract.ThrowInvalidEnumValue(kind);
                    return false;
            }
        }
    }

    /// <summary>
    /// Unique name for global symbols.
    /// </summary>
    public struct NativeName : IEquatable<NativeName>
    {
        public static NativeName Nil => default(NativeName);

        public string Name { get; }
        public NativeNameKind Kind { get; }

        public bool IsNil => this == Nil;
        public NativeSymbolKind SymbolKind => NativeNameUtil.GetNativeSymbolKind(Kind);

        public NativeName(string name, NativeNameKind kind)
        {
            Name = name;
            Kind = kind;
        }

        public static bool operator ==(NativeName left, NativeName right) => left.Name == right.Name && left.Kind == right.Kind;
        public static bool operator !=(NativeName left, NativeName right) => !(left == right);
        public bool Equals(NativeName other) => this == other;
        public override bool Equals(object obj) => obj is NativeName && Equals((NativeName)obj);
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
        public override string ToString() => $"{Name} - {Kind}";
    }
}
