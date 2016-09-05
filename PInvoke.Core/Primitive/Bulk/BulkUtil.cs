using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive.Bulk
{
    public static partial class BulkUtil
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

        public static BasicPrimitiveStorage Read(IBulkReader bulkReader)
        {
            var reader = new Reader(bulkReader);
            return reader.Read();
        }

        public static IPrimitiveWriter CreateWriter(IBulkWriter bulkWriter)
        {
            return new Writer(bulkWriter);
        }
    }
}
