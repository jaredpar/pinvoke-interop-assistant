using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public class CachingSymbolLookup : INativeSymbolLookup
    {
        private readonly BasicSymbolStorage _storage;
        private readonly INativeSymbolImporter _loader;

        public IEnumerable<NativeEnum> NativeEnums
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool TryFindConstant(string name, out NativeConstant nConst)
        {
            if (_storage.TryFindConstant(name, out nConst))
            {
                return true;
            }

            if (_loader.TryImportConstant(name, out nConst))
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

            if (_loader.TryImportDefined(name, out nt))
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

            if (_loader.TryImportProcedure(name, out proc))
            {
                _storage.AddProcedure(proc);
                return true;
            }

            return false;
        }

        public bool TryFindTypedef(string name, out NativeTypeDef nt)
        {
            if (_storage.TryFindTypedef(name, out nt))
            {
                return true;
            }

            if (_loader.TryImportTypedef(name, out nt))
            {
                _storage.AddTypedef(nt);
                return true;
            }

            return false;
        }
    }
}
