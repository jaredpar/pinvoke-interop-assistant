// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace PInvoke.NativeTypes.Enums
{
    /// <summary>
    /// The kind of the native type.  Makes it easy to do switching
    /// </summary>
    /// <remarks></remarks>
    public enum NativeSymbolKind
    {
        StructType,
        EnumType,
        UnionType,
        ArrayType,
        PointerType,
        BuiltinType,
        TypeDefType,
        BitVectorType,
        NamedType,
        Procedure,
        ProcedureSignature,
        FunctionPointer,
        Parameter,
        Member,
        EnumNameValue,
        Constant,
        SalEntry,
        SalAttribute,
        ValueExpression,
        Value,
        OpaqueType
    }
}
