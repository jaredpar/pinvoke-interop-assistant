/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SignatureGenerator
{
    [Serializable]
    public class CallbackNativeType : DefinedNativeType
    {
        #region Construction

        public CallbackNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            Debug.Assert(typeof(Delegate).IsAssignableFrom(desc.Type));

            UnmanagedType[] allowed_unmanaged_types;
            if (desc.IsStructField)
            {
                // fields of delegate type can only marshal as function pointers
                allowed_unmanaged_types = MarshalType.DelegField;
            }
            else
            {
                // parameters of delegate type
                allowed_unmanaged_types = (desc.IsComInterop ? MarshalType.DelegC : MarshalType.DelegP);
            }

            switch (ValidateUnmanagedType(desc, allowed_unmanaged_types))
            {
                case UnmanagedType.FunctionPtr:
                {
                    if (!desc.IsCallbackParam)
                    {
                        // the "number one" mistake when marshaling delegate to function ptr
                        Log.Add(Errors.INFO_BewarePrematureDelegateRelease);
                    }

                    this.typeDefinition = FunctionPtrDefinition.Get(desc);
                    break;
                }

                case UnmanagedType.Interface:
                {
                    this.name = DelegateInterfaceDefinition.InterfaceName;
                    this.nameModifier = StructModifier;

                    this.indirections++;

                    Log.Add(Errors.INFO_SeeMscorlibTlbForInterface, this.name);

                    this.typeDefinition = DelegateInterfaceDefinition.Get(desc);
                    break;
                }

                default:
                {
                    Debug.Fail(null);
                    goto case UnmanagedType.FunctionPtr;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents the definition of a function pointer type (<c>typedef</c>ed).
    /// </summary>
    [Serializable]
    public class FunctionPtrDefinition : NativeTypeDefinition
    {
        #region ForwardDeclaration

        /// <summary>
        /// If a there is a circular dependency involving a function pointer, the pointer is wrapped in a
        /// structure and a forward declaration of the structure is generated.
        /// </summary>
        [Serializable]
        class ForwardDeclaration : NativeTypeDefinition
        {
            private FunctionPtrDefinition definition;

            public ForwardDeclaration(FunctionPtrDefinition definition)
            {
                Debug.Assert(definition != null);
                this.definition = definition;
            }

            protected override string MessageLogPrefix
            {
                get { return definition.MessageLogPrefix; }
            }

            public override string Name
            {
                get { return definition.Name; }
            }

            public override int Size
            {
                get { return definition.Size; }
            }

            public override void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
            {
                // empty
            }

            public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
            {
                base.PrintTo(printer, logPrinter, flags);

                printer.Print(OutputType.Keyword, "struct");
                printer.Print(OutputType.Other, " ");
                printer.Print(OutputType.Identifier, Name);
                printer.Print(OutputType.Operator, ";");
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The signature of the function pointer.
        /// </summary>
        private NativeSignature signature;
        
        /// <summary>
        /// Is different from the signature name if wrapped in structure.
        /// </summary>
        private string name;

        private bool hasForwardDeclaration;

        #endregion

        #region Properties

        protected override string MessageLogPrefix
        {
            get { return String.Format(Resources.FunctionPointer, Name); }
        }

        public override string Name
        {
            get { return name; }
        }

        public override int Size
        {
	        get { return TypeName.GetPointerSize(platform64bit); }
        }

        #endregion

        #region Construction

        public static NativeTypeDefinition Get(NativeTypeDesc desc)
        {
            // when a delegate is marshaled as an unmanaged function pointer, its
            // parameters and return type have P/Invoke's default marshaling behavior
            // regardless of whether the function pointer is passed to a static entry
            // point or to a COM member

            return NativeTypeDefinition.Get<FunctionPtrDefinition>(
                new TypeDefKey(desc.Type, (desc.Flags & MarshalFlags.TypeDefKeyFlags) & ~MarshalFlags.ComInterop));
        }

        public FunctionPtrDefinition()
        { }

        protected override void Initialize(TypeDefKey key)
        {
            Debug.Assert(typeof(Delegate).IsAssignableFrom(key.Type));

            base.Initialize(key);

            // create the native signature of the delegate
            signature = NativeSignature.FromDelegateType(
                key.Type,
                (key.Flags & MarshalFlags.AnsiPlatform) == MarshalFlags.AnsiPlatform,
                platform64bit);

            name = signature.Name;
        }

        #endregion

        #region ICodePrintable Members

        public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null && signature != null);

            if (hasForwardDeclaration)
            {
                printer.Print(OutputType.Keyword, "struct");
                printer.Print(OutputType.Other, " ");
                printer.PrintLn(OutputType.Identifier, Name);

                printer.Indent();
                printer.PrintLn(OutputType.Operator, "{");

                // will print "ret_type (call_conv *ptr)(params)"
                signature.PrintTo(printer, logPrinter, flags);

                printer.Unindent();
                printer.PrintLn();
                printer.Print(OutputType.Operator, "};");
            }
            else
            {
                printer.Print(OutputType.Keyword, "typedef");
                printer.Print(OutputType.Other, " ");

                // will print "ret_type (call_conv *name)(params)"
                signature.PrintTo(printer, logPrinter, flags);
            }
        }

        #endregion

        #region Definition Enumeration

        public override void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
        {
            signature.GetDefinitionsRecursive(set, parentDef);
        }

        public override NativeTypeDefinition GetForwardDeclaration()
        {
            // wrap the fcn ptr into a structure and return a forward decl of the structure
            this.hasForwardDeclaration = true;
            this.signature.Name = "ptr";

            return new ForwardDeclaration(this);
        }

        #endregion
    }

    [Serializable]
    public class DelegateInterfaceDefinition : NativeTypeDefinition
    {
        #region Properties

        internal const string InterfaceName = "_Delegate";

        protected override string MessageLogPrefix
        {
            get { return String.Format(Resources.Interface, InterfaceName); }
        }

        public override string Name
        {
            get { return InterfaceName; }
        }

        public override int Size
        {
            get { return TypeName.GetPointerSize(platform64bit); }
        }

        #endregion

        #region Construction

        public static NativeTypeDefinition Get(NativeTypeDesc desc)
        {
            // delegates are marshaled as _Delegate COM interface
            // UUID = FB6AB00F-5096-3AF8-A33D-D7885A5FA829

            return NativeTypeDefinition.Get<DelegateInterfaceDefinition>(
                new TypeDefKey(typeof(Delegate), (desc.Flags & MarshalFlags.TypeDefKeyFlags) | MarshalFlags.ComInterop));
        }

        protected override void Initialize(TypeDefKey key)
        {
            Debug.Assert(typeof(Delegate).IsAssignableFrom(key.Type));

            base.Initialize(key);
        }

        #endregion

        #region Properties

        #endregion

        #region ICodePrintable Members

        public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            printer.Print(OutputType.Identifier, "MIDL_INTERFACE");
            printer.Print(OutputType.Operator, "(");
            printer.Print(OutputType.Literal, Utility.StringToLiteral("FB6AB00F-5096-3AF8-A33D-D7885A5FA829"));
            printer.PrintLn(OutputType.Operator, ")");
            
            printer.Print(OutputType.TypeName, InterfaceName);
            printer.Print(OutputType.Other, " ");
            printer.Print(OutputType.Operator, ":");
            printer.Print(OutputType.Other, " ");
            printer.Print(OutputType.Keyword, "public");
            printer.Print(OutputType.Other, " ");
            printer.PrintLn(OutputType.TypeName, "IDispatch");

            printer.PrintLn(OutputType.Operator, "{");
            
            printer.Print(OutputType.Keyword, "public");
            printer.PrintLn(OutputType.Operator, ":");

            printer.Print(OutputType.Other, "    ");
            printer.PrintLn(OutputType.Comment, "// methods omitted");

            printer.Print(OutputType.Other, "    ");
            printer.Print(OutputType.Keyword, "virtual");
            printer.Print(OutputType.Other, " ");

            new PrimitiveNativeType(TypeName.Error, platform64bit).PrintTo(printer, logPrinter, flags);
            printer.Print(OutputType.Other, " ");
            
            if ((flags & PrintFlags.UsePlainC) == PrintFlags.UsePlainC)
                printer.Print(OutputType.Keyword, "__stdcall");
            else
                printer.Print(OutputType.TypeName, "STDMETHODCALLTYPE");

            printer.Print(OutputType.Other, " ");
            printer.Print(OutputType.Identifier, "DynamicInvoke");
            printer.Print(OutputType.Operator, "(");

            new PrimitiveNativeType(TypeName.SafeArray, 1, platform64bit).PrintTo(printer, logPrinter, flags);
            printer.Print(OutputType.Other, " ");
            printer.Print(OutputType.Identifier, "args");
            printer.Print(OutputType.Operator, ",");
            printer.Print(OutputType.Other, " ");

            new PrimitiveNativeType(TypeName.Variant, 1, platform64bit).PrintTo(printer, logPrinter, flags);
            printer.Print(OutputType.Other, " ");
            printer.Print(OutputType.Identifier, "pRetVal");
            printer.Print(OutputType.Operator, ")");

            printer.Print(OutputType.Other, " ");
            printer.Print(OutputType.Operator, "=");
            printer.Print(OutputType.Other, " ");

            printer.Print(OutputType.Literal, "0");
            printer.PrintLn(OutputType.Operator, ";");

            printer.Print(OutputType.Operator, "};");
        }

        #endregion

        #region Definition Enumeration

        public override void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
        {
            // no defined types here
        }

        #endregion
    }
}
