using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    public sealed class PrimitiveImporter : INativeSymbolImporter
    {
        private readonly IPrimitiveReader _reader;
        private readonly HashSet<PrimitiveSymbolId> _symbolSet = new HashSet<PrimitiveSymbolId>();
        private readonly Dictionary<string, PrimitiveSymbolId> _symbolIdMap = new Dictionary<string, PrimitiveSymbolId>();

        public PrimitiveImporter(IPrimitiveReader reader)
        {
            _reader = reader;
            BuildInitialMaps();
        }

        private void BuildInitialMaps()
        {
            foreach (var item in _reader.ReadSymbolIds())
            {
                _symbolSet.Add(item);
                _symbolIdMap[item.Name] = item;
            }
        }

        private NativeSymbol Import(PrimitiveSymbolId id)
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
                case NativeSymbolKind.Procedure:
                    return ImportProcedure(id);
                case NativeSymbolKind.TypedefType:
                    return ImportTypeDef(id);
                case NativeSymbolKind.Constant:
                    return ImportConstant(id);
                default:
                    Contract.ThrowInvalidEnumValue(id.Kind);
                    return null;
            }
        }

        private NativeType ImportType(PrimitiveTypeId id)
        {
            // When importing symbols as types don't dig deep.  Just return as a named
            // type and let the higher layers do the resolution process.  It's not appropriate
            // at this layer.
            if (id.IsSymbolId)
            {
                return new NativeNamedType(id.SymbolId.Name);
            }

            return ImportType(id.SimpleId);
        }

        private NativeType ImportType(PrimitiveSimpleId id)
        {
            var data = _reader.ReadTypeData(id);
            switch (data.Kind)
            {
                case NativeSymbolKind.ArrayType:
                    return new NativeArray(ImportType(data.ElementTypeId), data.ElementCount);
                case NativeSymbolKind.PointerType:
                    return new NativePointer(ImportType(data.ElementTypeId));
                case NativeSymbolKind.BuiltinType:
                    return new NativeBuiltinType(data.BuiltinType);
                case NativeSymbolKind.BitVectorType:
                    return new NativeBitVector(data.ElementCount);
                case NativeSymbolKind.NamedType:
                    return new NativeNamedType(qualification: data.Qualification, name: data.Name, isConst: data.IsConst);
                case NativeSymbolKind.OpaqueType:
                    return new NativeOpaqueType();
                default:
                    Contract.ThrowInvalidEnumValue(data.Kind);
                    return null;
            }
        }

        private NativeTypeDef ImportTypeDef(PrimitiveSymbolId id)
        {
            var data = _reader.ReadTypeDefData(id);
            return new NativeTypeDef(id.Name, ImportType(data.TargetTypeId));
        }

        private NativeConstant ImportConstant(PrimitiveSymbolId id)
        {
            var data = _reader.ReadConstantData(id);
            return new NativeConstant(id.Name, data.Value, data.Kind);
        }

        private NativeDefinedType ImportStructOrUnion(PrimitiveSymbolId id)
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

        private NativeEnum ImportEnum(PrimitiveSymbolId id)
        {
            var e = new NativeEnum(id.Name);

            foreach (var value in _reader.ReadEnumValues(id))
            {
                e.Values.Add(new NativeEnumValue(value.Name, value.Value));
            }

            return e;
        }

        private NativeSalAttribute ImportSalAttribute(PrimitiveSimpleId id)
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

        private NativeSignature ImportSignature(PrimitiveSimpleId id)
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

        private NativeFunctionPointer ImportFunctionPointer(PrimitiveSymbolId id)
        {
            var data = _reader.ReadFuntionPointerData(id);
            var ptr = new NativeFunctionPointer(id.Name);
            ptr.CallingConvention = data.CallingConvention;
            ptr.Signature = ImportSignature(data.SignatureId);
            return ptr;
        }

        private NativeProcedure ImportProcedure(PrimitiveSymbolId id)
        {
            var data = _reader.ReadProcedureData(id);
            var proc = new NativeProcedure(id.Name);
            proc.DllName = data.DllName;
            proc.Signature = ImportSignature(data.SignatureId);
            proc.CallingConvention = data.CallingConvention;
            return proc;
        }

        public bool TryImport(string name, out NativeSymbol symbol)
        {
            PrimitiveSymbolId id;
            if (!_symbolIdMap.TryGetValue(name ,out id))
            {
                symbol = null;
                return false;
            }

            return TryImport(id, out symbol);
        }

        public bool TryImport(PrimitiveSymbolId id, out NativeSymbol symbol)
        {
            if (!_symbolSet.Contains(id))
            {
                symbol = null;
                return false;
            }

            symbol = Import(id);
            return true;
        }

        private bool TryImportCore<T>(string name, out T value) where T : NativeSymbol
        {
            value = default(T);

            PrimitiveSymbolId id;
            if (!_symbolIdMap.TryGetValue(name, out id))
            {
                return false;
            }

            NativeSymbol symbol;
            if (!TryImport(id, out symbol))
            {
                return false;
            }

            value = symbol as T;
            return value != null;
        }

        public bool TryImportDefined(string name, out NativeDefinedType nt)
        {
            return TryImportCore(name, out nt);
        }

        public bool TryImportTypedef(string name, out NativeTypeDef nt)
        {
            return TryImportCore(name, out nt);
        }

        public bool TryImportProcedure(string name, out NativeProcedure proc)
        {
            return TryImportCore(name, out proc);
        }

        public bool TryImportConstant(string name, out NativeConstant nConst)
        {
            return TryImportCore(name, out nConst);
        }

        public bool TryImportEnumValue(string name, out NativeEnum enumeration, out NativeEnumValue value)
        {
            // CTODO: Got to be a better way here.  Should this just be a global symbol.
            var data = _reader.ReadEnumValueData(name);
            if (data == null)
            {
                enumeration = null;
                value = null;
                return false;
            }

            NativeDefinedType nt;
            if (!TryImportDefined(data.Value.ContainingTypeId.Name, out nt) || nt.Kind != NativeSymbolKind.EnumType)
            {
                enumeration = null;
                value = null;
                return false;
            }

            enumeration = (NativeEnum)nt;
            value = enumeration.Values.Single(x => x.Name == name);
            return true;
        }
    }
}
