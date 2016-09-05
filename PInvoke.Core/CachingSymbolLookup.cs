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

        public IEnumerable<NativeName> NativeNames => _storage.NativeNames;

        public CachingSymbolLookup(INativeSymbolImporter importer)
        {
            _importer = importer;
        }

        public bool TryGetGlobalSymbol(string name, out NativeGlobalSymbol symbol)
        {
            if (_storage.TryGetGlobalSymbol(name, out symbol))
            {
                return true;
            }

            if (_importer.TryImport(name, out symbol))
            {
                _storage.Add(symbol);
                return true;
            }

            return false;
        }

        public bool TryGetGlobalSymbol(NativeName name, out NativeGlobalSymbol symbol)
        {
            if (_storage.TryGetGlobalSymbol(name, out symbol))
            {
                return true;
            }

            if (_importer.TryImport(name, out symbol))
            {
                _storage.Add(symbol);
                return true;
            }

            return false;
        }
    }
}
