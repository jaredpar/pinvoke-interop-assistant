using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public interface IPrimitiveWriter
    {
        void Write(PrimitiveSymbolId typeId);
        void Write(PrimitiveMemberData memberId);
        void Write(PrimitiveEnumValueData enumValue);
        void Write(PrimitiveSalEntryData data);
        void Write(PrimitiveSignatureData data);
        void Write(PrimitiveParameterData data);
        void Write(PrimitiveFunctionPointerData data);
        void Write(PrimitiveProcedureData data);
        void Write(PrimitiveTypeData data);
        void Write(PrimitiveTypeDefData data);
        void Write(PrimitiveConstantData data);
    }
}
