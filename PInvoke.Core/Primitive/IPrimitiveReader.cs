using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public interface IPrimitiveReader
    {
        IEnumerable<NativeSymbolId> ReadSymbolIds();
        IEnumerable<NativeMemberData> ReadMembers(NativeSymbolId typeId);
        IEnumerable<NativeEnumValueData> ReadEnumValues(NativeSymbolId typeId);
        IEnumerable<NativeSalEntryData> ReadSalEntries(NativeSimpleId salId);
        IEnumerable<NativeParameterData> ReadParameters(NativeSimpleId signatureId);
        NativeSignatureData ReadSignatureData(NativeSimpleId signatureId);
        NativeFunctionPointerData ReadFuntionPointerData(NativeSymbolId id);
        NativeProcedureData ReadProcedureData(NativeSymbolId id);
        NativeTypeData ReadTypeData(NativeSimpleId id);
        NativeTypeDefData ReadTypeDefData(NativeSymbolId id);
        NativeConstantData ReadConstantData(NativeSymbolId data);
        NativeEnumValueData? ReadEnumValueData(string valueName);
    }
}
