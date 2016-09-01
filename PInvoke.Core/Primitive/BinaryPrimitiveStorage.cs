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
        private enum ItemKind
        {
            EnumValueData,
            SignatureData,
            FunctionPointerData,
            TypeData,
            ConstantData,
            TypeDefData,
            ProcedureData,
            ParameterData,
            SalEntryData,
            MemberData,
            SymbolId
        }

        public static IPrimitiveWriter CreateWriter(Stream stream)
        {
            return new Writer(new BinaryWriter(stream));
        }

        public static IPrimitiveReader CreateReader(Stream stream)
        {
            var storage = new BasicPrimitiveStorage();
            var reader = new Reader(new BinaryReader(stream), storage);
            reader.Go();
            return storage;
        }
    }
}
