using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive.Bulk
{
    public static partial class BulkUtil
    {
        private sealed class Writer : IPrimitiveWriter
        {
            private readonly IBulkWriter _writer;

            internal Writer(IBulkWriter writer)
            {
                _writer = writer;
            }

            private void WriteItemStart(ItemKind kind)
            {
                _writer.WriteItemStart();
                _writer.WriteInt32((int)kind);
            }

            private void WriteSymbolId(PrimitiveSymbolId id)
            {
                _writer.WriteString(id.Name);
                _writer.WriteInt32((int)id.Kind);
            }

            private void WriteSimpleId(PrimitiveSimpleId id)
            {
                _writer.WriteInt32(id.Id);
            }

            private void WriteTypeId(PrimitiveTypeId id)
            {
                WriteSymbolId(id.SymbolId);
                WriteSimpleId(id.SimpleId);
            }

            public void Write(PrimitiveEnumValueData enumValue)
            {
                WriteItemStart(ItemKind.EnumValueData);
                _writer.WriteString(enumValue.Name);
                _writer.WriteString(enumValue.Value);
                WriteSymbolId(enumValue.ContainingTypeId);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveSignatureData data)
            {
                WriteItemStart(ItemKind.SignatureData);
                WriteSimpleId(data.SignatureId);
                WriteTypeId(data.ReturnTypeId);
                WriteSimpleId(data.ReturnTypeSalId);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveFunctionPointerData data)
            {
                WriteItemStart(ItemKind.FunctionPointerData);
                WriteSymbolId(data.ContainingTypeId);
                _writer.WriteInt32((int)data.CallingConvention);
                WriteSimpleId(data.SignatureId);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveTypeData data)
            {
                WriteItemStart(ItemKind.TypeData);
                WriteSimpleId(data.Id);
                _writer.WriteInt32((int)data.Kind);
                _writer.WriteInt32(data.ElementCount);
                WriteTypeId(data.ElementTypeId);
                _writer.WriteInt32((int)data.BuiltinType);
                _writer.WriteString(data.Name);
                _writer.WriteString(data.Qualification);
                _writer.WriteBoolean(data.IsConst);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveConstantData data)
            {
                WriteItemStart(ItemKind.ConstantData);
                WriteSymbolId(data.Id);
                _writer.WriteString(data.Value);
                _writer.WriteInt32((int)data.Kind);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveTypeDefData data)
            {
                WriteItemStart(ItemKind.TypeDefData);
                WriteSymbolId(data.SourceTypeId);
                WriteTypeId(data.TargetTypeId);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveProcedureData data)
            {
                WriteItemStart(ItemKind.ProcedureData);
                WriteSymbolId(data.ProcedureId);
                _writer.WriteInt32((int)data.CallingConvention);
                WriteSimpleId(data.SignatureId);
                _writer.WriteString(data.DllName);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveParameterData data)
            {
                WriteItemStart(ItemKind.ParameterData);
                WriteSimpleId(data.SignatureId);
                _writer.WriteInt32(data.Index);
                _writer.WriteString(data.Name);
                WriteTypeId(data.TypeId);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveSalEntryData data)
            {
                WriteItemStart(ItemKind.SalEntryData);
                WriteSimpleId(data.SalId);
                _writer.WriteInt32(data.Index);
                _writer.WriteInt32((int)data.SalEntryType);
                _writer.WriteString(data.Text);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveMemberData memberId)
            {
                WriteItemStart(ItemKind.MemberData);
                _writer.WriteString(memberId.Name);
                WriteTypeId(memberId.MemberTypeId);
                WriteSymbolId(memberId.ContainingTypeId);
                _writer.WriteItemEnd();
            }

            public void Write(PrimitiveSymbolId symbolId)
            {
                WriteItemStart(ItemKind.SymbolId);
                WriteSymbolId(symbolId);
                _writer.WriteItemEnd();
            }
        }
    }
}
