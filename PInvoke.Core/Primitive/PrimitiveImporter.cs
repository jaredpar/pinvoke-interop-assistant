using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
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

            switch (id.Kind)
            {
                case NativeSymbolKind.StructType:
                    nt = new NativeStruct(name);
                    break;
                case NativeSymbolKind.UnionType:
                    nt = new NativeUnion(name);
                    break;
                case NativeSymbolKind.EnumType:
                    nt = new NativeEnum(name);
                    break;
                case NativeSymbolKind.FunctionPointer:
                    return TryLoadFunctionPointer(id, out nt);
                default:
                    Contract.ThrowInvalidEnumValue(id.Kind);
                    nt = null;
                    return false;
            }

            foreach (var memberId in _reader.ReadMembers(id))
            {
                var member = new NativeMember(memberId.Name, new NativeNamedType(memberId.Name));
                nt.Members.Add(member);
            }

            return true;
        }

        private bool TryLoadFunctionPointer(NativeTypeId id, out NativeDefinedType nt)
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
