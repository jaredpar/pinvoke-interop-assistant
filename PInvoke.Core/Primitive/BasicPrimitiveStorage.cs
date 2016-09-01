using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PInvoke.Primitive;

namespace PInvoke.Primitive
{
    public sealed class BasicPrimitiveStorage : IPrimitiveReader, IPrimitiveWriter
    {
        private readonly List<PrimitiveSymbolId> _symbolIdList = new List<PrimitiveSymbolId>();
        private readonly List<PrimitiveMemberData> _memberList = new List<PrimitiveMemberData>();
        private readonly List<PrimitiveEnumValueData> _enumValueList = new List<PrimitiveEnumValueData>();
        private readonly List<PrimitiveSalEntryData> _salDataList = new List<PrimitiveSalEntryData>();
        private readonly List<PrimitiveParameterData> _paramDataList = new List<PrimitiveParameterData>();
        private readonly List<PrimitiveFunctionPointerData> _funcPtrDataList = new List<PrimitiveFunctionPointerData>();
        private readonly List<PrimitiveSignatureData> _sigDataList = new List<PrimitiveSignatureData>();
        private readonly List<PrimitiveProcedureData> _procDataList = new List<PrimitiveProcedureData>();
        private readonly List<PrimitiveTypeData> _typeDataList = new List<PrimitiveTypeData>();
        private readonly List<PrimitiveTypeDefData> _typeDefDataList = new List<PrimitiveTypeDefData>();
        private readonly List<PrimitiveConstantData> _constDataList = new List<PrimitiveConstantData>();

        public IEnumerable<PrimitiveMemberData> ReadMembers(PrimitiveSymbolId typeId)
        {
            return _memberList.Where(x => x.ContainingTypeId == typeId);
        }

        public IEnumerable<PrimitiveSymbolId> ReadSymbolIds()
        {
            return _symbolIdList;
        }

        public IEnumerable<PrimitiveEnumValueData> ReadEnumValues(PrimitiveSymbolId typeId)
        {
            return _enumValueList.Where(x => x.ContainingTypeId == typeId);
        }

        public IEnumerable<PrimitiveSalEntryData> ReadSalEntries(PrimitiveSimpleId salId)
        {
            return _salDataList.Where(x => x.SalId == salId);
        }

        public IEnumerable<PrimitiveParameterData> ReadParameters(PrimitiveSimpleId signatureId)
        {
            return _paramDataList.Where(x => x.SignatureId == signatureId);
        }

        public PrimitiveSignatureData ReadSignatureData(PrimitiveSimpleId signatureId)
        {
            return _sigDataList.Single(x => x.SignatureId == signatureId);
        }

        public PrimitiveFunctionPointerData ReadFuntionPointerData(PrimitiveSymbolId id)
        {
            return _funcPtrDataList.Single(x => x.ContainingTypeId == id);
        }

        public PrimitiveProcedureData ReadProcedureData(PrimitiveSymbolId id)
        {
            return _procDataList.Single(x => x.ProcedureId == id);
        }

        public PrimitiveTypeData ReadTypeData(PrimitiveSimpleId id)
        {
            return _typeDataList.Single(x => x.Id == id);
        }

        public PrimitiveTypeDefData ReadTypeDefData(PrimitiveSymbolId id)
        {
            return _typeDefDataList.Single(x => x.SourceTypeId == id);
        }

        public PrimitiveConstantData ReadConstantData(PrimitiveSymbolId id)
        {
            return _constDataList.Single(x => x.Id == id);
        }

        public PrimitiveEnumValueData? ReadEnumValueData(string valueName)
        {
            return _enumValueList.SingleOrDefault(x => x.Name == valueName);
        }

        public void Write(PrimitiveMemberData member)
        {
            _memberList.Add(member);
        }

        public void Write(PrimitiveSymbolId typeId)
        {
            _symbolIdList.Add(typeId);
        }

        public void Write(PrimitiveEnumValueData data)
        {
            _enumValueList.Add(data);
        }

        public void Write(PrimitiveSalEntryData data)
        {
            _salDataList.Add(data);
        }

        public void Write(PrimitiveParameterData data)
        {
            _paramDataList.Add(data);
        }

        public void Write(PrimitiveFunctionPointerData data)
        {
            _funcPtrDataList.Add(data);
        }

        public void Write(PrimitiveSignatureData data)
        {
            _sigDataList.Add(data);
        }

        public void Write(PrimitiveProcedureData data)
        {
            _procDataList.Add(data);
        }

        public void Write(PrimitiveTypeData data)
        {
            _typeDataList.Add(data);
        }

        public void Write(PrimitiveTypeDefData data)
        {
            _typeDefDataList.Add(data);
        }

        public void Write(PrimitiveConstantData data)
        {
            _constDataList.Add(data);
        }
    }
}
