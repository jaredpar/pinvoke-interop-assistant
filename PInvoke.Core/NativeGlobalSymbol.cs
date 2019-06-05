using PInvoke.NativeTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public struct NativeGlobalSymbol
    {
        public NativeName Name { get; }
        public NativeSymbol Symbol { get; }

        public NativeNameKind Kind => Name.Kind;

        public NativeGlobalSymbol(NativeName name, NativeSymbol symbol)
        {
            Contract.Requires(name.SymbolKind == symbol.Kind);
            Contract.Requires(name.Name == symbol.Name);
            Contract.Requires(name == NativeNameUtil.GetName(symbol));
            Name = name;
            Symbol = symbol;
        }

        public NativeGlobalSymbol(NativeDefinedType definedType) : this(definedType.NativeName, definedType) { }
        public NativeGlobalSymbol(NativeTypeDef typeDef) : this(typeDef.NativeName, typeDef) { }
        public NativeGlobalSymbol(NativeConstant constant) : this(constant.NativeName, constant) { }
        public NativeGlobalSymbol(NativeProcedure procedure) : this(procedure.NativeName, procedure) { }
        public NativeGlobalSymbol(NativeEnumValue value) : this(value.NativeName, value) { }
    }
}
