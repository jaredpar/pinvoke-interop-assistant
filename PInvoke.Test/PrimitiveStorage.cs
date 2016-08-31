using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PInvoke.Primitive;

namespace PInvoke.Test
{
    public sealed class PrimitiveStorage : IPrimitiveReader, IPrimitiveWriter
    {
        private readonly List<NativeSymbolId> _symbolIdList = new List<NativeSymbolId>();
        private readonly List<NativeMemberData> _memberList = new List<NativeMemberData>();
        private readonly List<NativeEnumValueData> _enumValueList = new List<NativeEnumValueData>();
        private readonly List<NativeSalEntryData> _salDataList = new List<NativeSalEntryData>();
        private readonly List<NativeParameterData> _paramDataList = new List<NativeParameterData>();
        private readonly List<NativeFunctionPointerData> _funcPtrDataList = new List<NativeFunctionPointerData>();
        private readonly List<NativeSignatureData> _sigDataList = new List<NativeSignatureData>();
        private readonly List<NativeProcedureData> _procDataList = new List<NativeProcedureData>();

        public IEnumerable<NativeMemberData> ReadMembers(NativeSymbolId typeId)
        {
            return _memberList.Where(x => x.ContainingTypeId == typeId);
        }

        public IEnumerable<NativeSymbolId> ReadSymbolIds()
        {
            return _symbolIdList;
        }

        public IEnumerable<NativeEnumValueData> ReadEnumValues(NativeSymbolId typeId)
        {
            return _enumValueList.Where(x => x.ContainingTypeId == typeId);
        }

        public IEnumerable<NativeSalEntryData> ReadSalEntries(NativeSimpleId salId)
        {
            return _salDataList.Where(x => x.SalId == salId);
        }

        public IEnumerable<NativeParameterData> ReadParameters(NativeSimpleId signatureId)
        {
            return _paramDataList.Where(x => x.SignatureId == signatureId);
        }

        public NativeSignatureData ReadSignatureData(NativeSimpleId signatureId)
        {
            return _sigDataList.Single(x => x.SignatureId == signatureId);
        }

        public NativeFunctionPointerData ReadFuntionPointerData(NativeSymbolId id)
        {
            return _funcPtrDataList.Single(x => x.ContainingTypeId == id);
        }

        public NativeProcedureData ReadProcedureData(NativeSymbolId id)
        {
            return _procDataList.Single(x => x.ProcedureId == id);
        }

        public void Write(NativeMemberData member)
        {
            _memberList.Add(member);
        }

        public void Write(NativeSymbolId typeId)
        {
            _symbolIdList.Add(typeId);
        }

        public void Write(NativeEnumValueData data)
        {
            _enumValueList.Add(data);
        }

        public void Write(NativeSalEntryData data)
        {
            _salDataList.Add(data);
        }

        public void Write(NativeParameterData data)
        {
            _paramDataList.Add(data);
        }

        public void Write(NativeFunctionPointerData data)
        {
            _funcPtrDataList.Add(data);
        }

        public void Write(NativeSignatureData data)
        {
            _sigDataList.Add(data);
        }

        public void Write(NativeProcedureData data)
        {
            _procDataList.Add(data);
        }
    }
}
