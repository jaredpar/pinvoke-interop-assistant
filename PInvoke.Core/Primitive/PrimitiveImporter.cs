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
        private readonly Dictionary<string, NativeTypeId> _typeIdMap = new Dictionary<string, NativeTypeId>(StringComparer.Ordinal);

        public PrimitiveImporter(IPrimitiveReader reader)
        {
            _reader = reader;
            BuildInitialMaps();
        }

        private void BuildInitialMaps()
        {
            foreach (var item in _reader.ReadTypeIds())
            {
                _typeIdMap[item.Name] = item;
            }
        }

        public bool TryLoadConstant(string name, out NativeConstant nConst)
        {
            throw new NotImplementedException();
        }

        public bool TryLoadDefined(string name, out NativeDefinedType nt)
        {
            NativeTypeId id;
            if (!_typeIdMap.TryGetValue(name, out id))
            {
                nt = null;
                return false;
            }

            nt = ImportDefined(id);
            return true;
        }

        private NativeDefinedType ImportDefined(NativeTypeId id)
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

        private NativeDefinedType ImportStructOrUnion(NativeTypeId id)
        {
            Contract.Requires(id.Kind == NativeSymbolKind.StructType || id.Kind == NativeSymbolKind.UnionType);
            var nt = id.Kind == NativeSymbolKind.StructType
                ? (NativeDefinedType)new NativeStruct(id.Name)
                : new NativeUnion(id.Name);

            foreach (var memberData in _reader.ReadMembers(id))
            {
                var memberType = new NativeNamedType(memberData.MemberTypeId.Name);
                var member = new NativeMember(memberData.Name, memberType);
                nt.Members.Add(member);
            }

            return nt;
        }

        private NativeEnum ImportEnum(NativeTypeId id)
        {
            var e = new NativeEnum(id.Name);

            foreach (var value in _reader.ReadEnumValues(id))
            {
                e.Values.Add(new NativeEnumValue(value.Name, value.Value));
            }

            return e;
        }

        private NativeFunctionPointer ImportFunctionPointer(NativeTypeId id)
        {
            throw new NotImplementedException();
        }

        public bool TryLoadProcedure(string name, out NativeProcedure proc)
        {
            throw new NotImplementedException();
        }

        public bool TryLoadTypedef(string name, out NativeTypeDef nt)
        {
            throw new NotImplementedException();
        }
    }
}
