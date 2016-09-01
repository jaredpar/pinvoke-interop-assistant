using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public interface IPrimitiveReader
    {
        IEnumerable<PrimitiveSymbolId> ReadSymbolIds();
        IEnumerable<PrimitiveMemberData> ReadMembers(PrimitiveSymbolId typeId);
        IEnumerable<PrimitiveEnumValueData> ReadEnumValues(PrimitiveSymbolId typeId);
        IEnumerable<PrimitiveSalEntryData> ReadSalEntries(PrimitiveSimpleId salId);
        IEnumerable<PrimitiveParameterData> ReadParameters(PrimitiveSimpleId signatureId);
        PrimitiveSignatureData ReadSignatureData(PrimitiveSimpleId signatureId);
        PrimitiveFunctionPointerData ReadFuntionPointerData(PrimitiveSymbolId id);
        PrimitiveProcedureData ReadProcedureData(PrimitiveSymbolId id);
        PrimitiveTypeData ReadTypeData(PrimitiveSimpleId id);
        PrimitiveTypeDefData ReadTypeDefData(PrimitiveSymbolId id);
        PrimitiveConstantData ReadConstantData(PrimitiveSymbolId data);
        PrimitiveEnumValueData? ReadEnumValueData(string valueName);
    }
}
