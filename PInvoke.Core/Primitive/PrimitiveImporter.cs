using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    // CTODO: Get rid of try.  JUst load the values here.  Either you can or can't load. 
    public sealed class PrimitiveImporter : INativeSymbolLoader
    {
        private readonly IPrimitiveReader _reader;
        private readonly Dictionary<string, NativeSymbolId> _symbolIdMap = new Dictionary<string, NativeSymbolId>(StringComparer.Ordinal);

        public PrimitiveImporter(IPrimitiveReader reader)
        {
            _reader = reader;
            BuildInitialMaps();
        }

        private void BuildInitialMaps()
        {
            foreach (var item in _reader.ReadSymbolIds())
            {
                _symbolIdMap[item.Name] = item;
            }
        }

        private NativeDefinedType ImportDefined(NativeSymbolId id)
        { 
            switch (id.Kind)
            {
                case NativeSymbolKind.StructType:
                case NativeSymbolKind.UnionType:
                    return ImportStructOrUnion(id);
                case NativeSymbolKind.EnumType:
                    return ImportEnum(id);
                case NativeSymbolKind.FunctionPointer:
                    return ImportFunctionPointer(id);
                default:
                    Contract.ThrowInvalidEnumValue(id.Kind);
                    return null;
            }
        }

        private NativeType ImportType(NativeSymbolId id)
        {
            if (id.Kind == NativeSymbolKind.BuiltinType)
            {
                var bt = (BuiltinType)Enum.Parse(typeof(BuiltinType), id.Name);
                return new NativeBuiltinType(bt);
            }

            return new NativeNamedType(id.Name);
        }

        private NativeDefinedType ImportStructOrUnion(NativeSymbolId id)
        {
            Contract.Requires(id.Kind == NativeSymbolKind.StructType || id.Kind == NativeSymbolKind.UnionType);
            var nt = id.Kind == NativeSymbolKind.StructType
                ? (NativeDefinedType)new NativeStruct(id.Name)
                : new NativeUnion(id.Name);

            foreach (var memberData in _reader.ReadMembers(id))
            {
                var memberType = ImportType(memberData.MemberTypeId);
                var member = new NativeMember(memberData.Name, memberType);
                nt.Members.Add(member);
            }

            return nt;
        }

        private NativeEnum ImportEnum(NativeSymbolId id)
        {
            var e = new NativeEnum(id.Name);

            foreach (var value in _reader.ReadEnumValues(id))
            {
                e.Values.Add(new NativeEnumValue(value.Name, value.Value));
            }

            return e;
        }

        private NativeSalAttribute ImportSalAttribute(NativeSimpleId id)
        {
            if (id.IsNil)
            {
                return new NativeSalAttribute();
            }

            var sal = new NativeSalAttribute();
            foreach (var data in _reader.ReadSalEntries(id).OrderBy(x => x.Index))
            {
                var entry = new NativeSalEntry(data.SalEntryType, data.Text);
                sal.SalEntryList.Add(entry);
            }
            return sal;
        }

        private NativeSignature ImportSignature(NativeSimpleId id)
        {
            var data = _reader.ReadSignatureData(id);
            var sig = new NativeSignature();
            sig.ReturnType = ImportType(data.ReturnTypeId);
            sig.ReturnTypeSalAttribute = ImportSalAttribute(data.ReturnTypeSalId);

            foreach (var pData in _reader.ReadParameters(id).OrderBy(x => x.Index))
            {
                var p = new NativeParameter(pData.Name, ImportType(pData.TypeId));
                sig.Parameters.Add(p);
            }

            return sig;
        }

        private NativeFunctionPointer ImportFunctionPointer(NativeSymbolId id)
        {
            var data = _reader.ReadFuntionPointerData(id);
            var ptr = new NativeFunctionPointer(id.Name);
            ptr.CallingConvention = data.CallingConvention;
            ptr.Signature = ImportSignature(data.SignatureId);
            return ptr;
        }

        private NativeProcedure ImportProcedure(NativeSymbolId id)
        {
            var data = _reader.ReadProcedureData(id);
            var proc = new NativeProcedure(id.Name);
            proc.DllName = data.DllName;
            proc.Signature = ImportSignature(data.SignatureId);
            proc.CallingConvention = data.CallingConvention;
            return proc;
        }

        public bool TryLoadConstant(string name, out NativeConstant nConst)
        {
            throw new NotImplementedException();
        }

        public bool TryLoadDefined(string name, out NativeDefinedType nt)
        {
            NativeSymbolId id;
            if (!_symbolIdMap.TryGetValue(name, out id))
            {
                nt = null;
                return false;
            }

            nt = ImportDefined(id);
            return true;
        }

        public bool TryLoadProcedure(string name, out NativeProcedure proc)
        {
            NativeSymbolId id;
            if (!_symbolIdMap.TryGetValue(name, out id))
            {
                proc = null;
                return false;
            }

            proc = ImportProcedure(id);
            return true;
        }

        public bool TryLoadTypedef(string name, out NativeTypeDef nt)
        {
            throw new NotImplementedException();
        }
    }
}
