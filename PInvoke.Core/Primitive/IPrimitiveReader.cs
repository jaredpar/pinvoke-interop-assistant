using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public interface IPrimitiveReader
    {
        IEnumerable<NativeTypeId> ReadTypeIds();
        IEnumerable<NativeMemberData> ReadMembers(NativeTypeId typeId);
        IEnumerable<NativeEnumValueData> ReadEnumValues(NativeTypeId typeId);
        IEnumerable<NativeSalEntryData> ReadSalEntries(NativeSimpleId salId);
        IEnumerable<NativeParameterData> ReadParameters(NativeSimpleId signatureId);
        NativeSignatureData ReadSignatureData(NativeSimpleId signatureId);
        NativeFunctionPointerData ReadFuntionPointerData(NativeTypeId id);
    }
}
