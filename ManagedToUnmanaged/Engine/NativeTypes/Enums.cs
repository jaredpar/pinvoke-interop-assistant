/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SignatureGenerator
{
    [Serializable]
    public class EnumNativeType : DefinedNativeType
    {
        #region Construction

        public EnumNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(desc.Type.IsEnum);

            Type underlying_type = Enum.GetUnderlyingType(desc.Type);

            // verify managed/unmanaged combination (enums are normalized to their underlying type)
            UnmanagedType underlying_unmanaged_type;
            UnmanagedType[] allowed_unmanaged_types = Utility.GetAllowedUnmanagedTypesForEnum(underlying_type);
 
            if (allowed_unmanaged_types != null)
            {
                // we want the potential validation error to go this type's log
                underlying_unmanaged_type = ValidateUnmanagedType(desc, allowed_unmanaged_types);
            }
            else
            {
                Log.Add(Errors.ERROR_UnexpectedEnumUnderlyingType, underlying_type.FullName);
                underlying_unmanaged_type = UnmanagedType.I4;
            }

            // pass the underlying native type to the definition
            this.typeDefinition = EnumDefinition.Get(desc, underlying_unmanaged_type);
            this.nameModifier = String.Empty;
        }

        #endregion
    }

    [Serializable]
    public class EnumDefinition : NativeTypeDefinition
    {
        #region EnumField

        [Serializable]
        private struct EnumField
        {
            public readonly string Name;
            public readonly decimal Value;

            public EnumField(string name, object value)
            {
                Debug.Assert(name != null && value != null);

                this.Name = name;
                this.Value = Convert.ToDecimal(value);
            }

            public void PrintTo(ICodePrinter printer, PrintFlags flags, ref decimal nextValue)
            {
                if ((flags & PrintFlags.MangleEnumFields) == PrintFlags.MangleEnumFields)
                {
                    printer.Print(OutputType.Identifier, "_" + Guid.NewGuid().ToString("N") + "_");
                }
                printer.Print(OutputType.Identifier, Name);

                if (Value != nextValue)
                {
                    printer.Print(OutputType.Other, " ");
                    printer.Print(OutputType.Operator, "=");
                    printer.Print(OutputType.Other, " ");

                    printer.Print(OutputType.Literal, Value.ToString());

                    nextValue = Value;
                }
                unchecked
                {
                    nextValue++;
                }
            }
        }

        #endregion

        #region TypeDefKeyWithUnmanagedType

        protected class TypeDefKeyWithUnmanagedType : TypeDefKey
        {
            public readonly UnmanagedType UnmanagedType;

            public TypeDefKeyWithUnmanagedType(Type type, MarshalFlags flags, UnmanagedType unmanagedType)
                : base(type, flags)
            {
                this.UnmanagedType = unmanagedType;
            }
        }

        #endregion

        #region Fields

        private string name;
        private PrimitiveNativeType underlyingType;
        private EnumField[] fields;
        private bool isFlagsEnum;

        #endregion

        #region Properties

        protected override string MessageLogPrefix
        {
            get { return String.Format(Resources.Enum, name); }
        }

        public override string Name
        {
            get { return name; }
        }

        public override int Size
        {
            get { return underlyingType.TypeSize; }
        }

        #endregion

        #region Construction

        public static EnumDefinition Get(NativeTypeDesc desc, UnmanagedType underlyingType)
        {
            return NativeTypeDefinition.Get<EnumDefinition>(
                new TypeDefKeyWithUnmanagedType(desc.Type, (desc.Flags & MarshalFlags.TypeDefKeyFlags), underlyingType));
        }

        public EnumDefinition()
        { }

        protected override void Initialize(TypeDefKey key)
        {
            Debug.Assert(key.Type.IsEnum && key is TypeDefKeyWithUnmanagedType);

            Type type = key.Type;
            this.name = Utility.GetNameOfType(type);
            this.isFlagsEnum = type.IsDefined(typeof(FlagsAttribute), false);

            UnmanagedType underlying_unmng_type = ((TypeDefKeyWithUnmanagedType)key).UnmanagedType;
            TypeName type_name = TypeName.GetTypeNameForUnmanagedType(underlying_unmng_type);
            
            // The unmanaged type has been validated by EnumNativeType and is surely a primitive type, for
            // which there is a simple direct translation to TypeName.
            Debug.Assert(!type_name.IsEmpty);

            this.underlyingType = new PrimitiveNativeType(
                type_name,
                (key.Flags & MarshalFlags.Platform64Bit) == MarshalFlags.Platform64Bit);

            // now enumerate the enum fields and set up the fields array
            string[] names = Enum.GetNames(type);
            Array values = Enum.GetValues(type);

            Debug.Assert(names.Length == values.Length);

            this.fields = new EnumField[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                this.fields[i] = new EnumField(names[i], values.GetValue(i));
            }
        }

        #endregion

        #region ICodePrintable Members

        public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null && fields != null);

            printer.Print(OutputType.Keyword, "enum");
            printer.Print(OutputType.Other, " ");
            printer.Print(OutputType.TypeName, name);
            printer.Print(OutputType.Other, " ");
            
            // always explicitly print the underlying type
            printer.Print(OutputType.Operator, ":");
            printer.Print(OutputType.Other, " ");

            underlyingType.PrintTo(printer, logPrinter, flags);

            printer.PrintLn();

            printer.Indent();
            printer.Print(OutputType.Operator, "{");
            try
            {
                decimal next_value = 0;
                for (int i = 0; i < fields.Length; i++)
                {
                    // field name [ = field value ][,]
                    printer.PrintLn();

                    if (isFlagsEnum)
                    {
                        // explicitly list all field values for [Flags] enums
                        next_value = Decimal.MaxValue;
                    }
                    fields[i].PrintTo(printer, flags, ref next_value);

                    if (i < fields.Length - 1)
                    {
                        // comma after all but the last field
                        printer.Print(OutputType.Operator, ",");
                    }
                }
            }
            finally
            {
                printer.Unindent();

                printer.PrintLn();
                printer.Print(OutputType.Operator, "};");
            }
        }

        #endregion

        #region Definition Enumeration

        public override void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
        {
            // empty
        }

        #endregion
    }
}
