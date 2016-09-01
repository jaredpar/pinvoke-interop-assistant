using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public static partial class BinaryPrimitiveStorage
    {
        private sealed class Writer : IPrimitiveWriter
        {
            private readonly BinaryWriter _writer;

            internal Writer(BinaryWriter writer)
            {
                _writer = writer;
            }

            private void WriteCore(ItemKind kind)
            {
                _writer.Write((int)kind);
            }

            private void WriteCore(PrimitiveSymbolId id)
            {
                WriteCore(id.Name);
                _writer.Write((int)id.Kind);
            }

            private void WriteCore(PrimitiveSimpleId id)
            {
                _writer.Write(id.Id);
            }

            private void WriteCore(PrimitiveTypeId id)
            {
                WriteCore(id.SymbolId);
                WriteCore(id.SimpleId);
            }

            private void WriteCore(string str)
            {
                if (str != null)
                {
                    _writer.Write(true);
                    _writer.Write(str);
                }
                else
                {
                    _writer.Write(false);
                }
            }

            public void Write(PrimitiveEnumValueData enumValue)
            {
                WriteCore(ItemKind.EnumValueData);
                WriteCore(enumValue.Name);
                WriteCore(enumValue.Value);
                WriteCore(enumValue.ContainingTypeId);
            }

            public void Write(PrimitiveSignatureData data)
            {
                WriteCore(ItemKind.SignatureData);
                WriteCore(data.SignatureId);
                WriteCore(data.ReturnTypeId);
                WriteCore(data.ReturnTypeSalId);
            }

            public void Write(PrimitiveFunctionPointerData data)
            {
                WriteCore(ItemKind.FunctionPointerData);
                WriteCore(data.ContainingTypeId);
                _writer.Write((int)data.CallingConvention);
                WriteCore(data.SignatureId);
            }

            public void Write(PrimitiveTypeData data)
            {
                WriteCore(ItemKind.TypeData);
                WriteCore(data.Id);
                _writer.Write((int)data.Kind);
                _writer.Write(data.ElementCount);
                WriteCore(data.ElementTypeId);
                _writer.Write((int)data.BuiltinType);
                WriteCore(data.Name);
                WriteCore(data.Qualification);
                _writer.Write(data.IsConst);
            }

            public void Write(PrimitiveConstantData data)
            {
                WriteCore(ItemKind.ConstantData);
                WriteCore(data.Id);
                WriteCore(data.Value);
                _writer.Write((int)data.Kind);
            }

            public void Write(PrimitiveTypeDefData data)
            {
                WriteCore(ItemKind.TypeDefData);
                WriteCore(data.SourceTypeId);
                WriteCore(data.TargetTypeId);
            }

            public void Write(PrimitiveProcedureData data)
            {
                WriteCore(ItemKind.ProcedureData);
                WriteCore(data.ProcedureId);
                _writer.Write((int)data.CallingConvention);
                WriteCore(data.SignatureId);
                WriteCore(data.DllName);
            }

            public void Write(PrimitiveParameterData data)
            {
                WriteCore(ItemKind.ParameterData);
                WriteCore(data.SignatureId);
                _writer.Write(data.Index);
                WriteCore(data.Name);
                WriteCore(data.TypeId);
            }

            public void Write(PrimitiveSalEntryData data)
            {
                WriteCore(ItemKind.SalEntryData);
                WriteCore(data.SalId);
                _writer.Write(data.Index);
                _writer.Write((int)data.SalEntryType);
                WriteCore(data.Text);
            }

            public void Write(PrimitiveMemberData memberId)
            {
                WriteCore(ItemKind.MemberData);
                WriteCore(memberId.Name);
                WriteCore(memberId.MemberTypeId);
                WriteCore(memberId.ContainingTypeId);
            }

            public void Write(PrimitiveSymbolId symbolId)
            {
                WriteCore(ItemKind.SymbolId);
                WriteCore(symbolId);
            }
        }
    }
}
