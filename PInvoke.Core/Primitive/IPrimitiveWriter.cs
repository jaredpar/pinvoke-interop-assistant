using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public interface IPrimitiveWriter
    {
        void Write(NativeTypeId typeId);
        void Write(NativeMemberData memberId);
        void Write(NativeEnumValueData enumValue);
        void Write(NativeSalEntryData data);
        void Write(NativeSignatureData data);
        void Write(NativeParameterData data);
        void Write(NativeFunctionPointerData data);
    }
}
