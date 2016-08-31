using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Primitive
{
    // CTODO: Get rid of try.  JUst load the values here.  Either you can or can't load. 
    public sealed class PrimitiveImporter : INativeSymbolImporter
    {
        private readonly IPrimitiveReader _reader;
        private readonly HashSet<NativeSymbolId> _symbolSet = new HashSet<NativeSymbolId>();
        private readonly Dictionary<string, NativeSymbolId> _symbolIdMap = new Dictionary<string, NativeSymbolId>();

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

        private NativeSymbol Import(NativeSymbolId id)
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

        public bool TryImport(string name, out NativeSymbol symbol)
        {
            NativeSymbolId id;
            if (!_symbolIdMap.TryGetValue(name ,out id))
            {
                symbol = null;
                return false;
            }

            return TryImport(id, out symbol);
        }

        public bool TryImport(NativeSymbolId id, out NativeSymbol symbol)
        {
            if (!_symbolSet.Contains(id))
            {
                symbol = null;
                return false;
            }

            symbol = Import(id);
            return true;
        }

        private bool TryLoadCore<T>(string name, out T value) where T : NativeSymbol
        {
            value = default(T);

            NativeSymbolId id;
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
            return TryLoadCore(name, out nt);
        }

        public bool TryImportTypedef(string name, out NativeTypeDef nt)
        {
            return TryLoadCore(name, out nt);
        }

        public bool TryImportProcedure(string name, out NativeProcedure proc)
        {
            return TryLoadCore(name, out proc);
        }

        public bool TryImportConstant(string name, out NativeConstant nConst)
        {
            return TryLoadCore(name, out nConst);
        }
    }
}
