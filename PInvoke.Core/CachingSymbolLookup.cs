using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public class CachingSymbolLookup : INativeSymbolLookup
    {
        private readonly BasicSymbolStorage _storage = new BasicSymbolStorage();
        private readonly INativeSymbolImporter _importer;

        public IEnumerable<NativeEnum> NativeEnums
        {
            get { throw new NotImplementedException(); }
        }

        public CachingSymbolLookup(INativeSymbolImporter importer)
        {
            _importer = importer;
        }

        public bool TryFindConstant(string name, out NativeConstant nConst)
        {
            if (_storage.TryFindConstant(name, out nConst))
            {
                return true;
            }

            if (_importer.TryImportConstant(name, out nConst))
            {
                _storage.AddConstant(nConst);
                return true;
            }

            return false;
        }

        public bool TryFindDefined(string name, out NativeDefinedType nt)
        {
            if (_storage.TryFindDefined(name ,out nt))
            {
                return true;
            }

            if (_importer.TryImportDefined(name, out nt))
            {
                _storage.AddDefinedType(nt);
                return true;
            }

            return false;
        }

        public bool TryFindProcedure(string name, out NativeProcedure proc)
        {
            if (_storage.TryFindProcedure(name, out proc))
            {
                return true;
            }

            if (_importer.TryImportProcedure(name, out proc))
            {
                _storage.AddProcedure(proc);
                return true;
            }

            return false;
        }

        public bool TryFindTypeDef(string name, out NativeTypeDef nt)
        {
            if (_storage.TryFindTypeDef(name, out nt))
            {
                return true;
            }

            if (_importer.TryImportTypedef(name, out nt))
            {
                _storage.AddTypeDef(nt);
                return true;
            }

            return false;
        }

        public bool TryFindEnumValue(string name, out NativeEnum enumeration, out NativeEnumValue value)
        {
            if (_storage.TryFindEnumValue(name, out enumeration, out value))
            {
                return true;
            }

            if (_importer.TryImportEnumValue(name, out enumeration, out value))
            {
                _storage.AddDefinedType(enumeration);
                return true;
            }

            return false;
        }
    }
}
