/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SignatureGenerator
{
    #region NativeTypeDesc

    [Flags]
    public enum MarshalFlags
    {
        None = 0,

        MarshalIn       = 1,
        MarshalOut      = 2,
        
        ComInterop      = 4,  // not set for P/Invoke
        
        AnsiStrings     = 8,  // \ if none set, it's auto
        UnicodeStrings  = 16, // /
        
        AnsiPlatform    = 32, // not set for Unicode platform
        
        ByRefParam      = 64,
        RetValParam     = 128,
        CallbackParam   = 256,
        
        StructField     = 512,
        ArrayElement    = 1024,
        SysArrayElement = 2048, // an element of the System.Array untyped array

        Platform64Bit   = 4096, // not set for 32-bit platform


        TypeDefKeyFlags = ComInterop | AnsiStrings | UnicodeStrings | AnsiPlatform | Platform64Bit
    }
    
    public class NativeTypeDesc : ICloneable
    {
        public Type Type;

        /// <summary>
        /// Non-<B>null</B> if the instance describes a parameter;
        /// </summary>
        /// <remarks>
        /// We need this to resolve references to parameters using <see cref="MarshalAsAttribute.SizeParamIndex"/>.
        /// </remarks>
        public ParameterInfo ParameterInfo;
        
        public MarshalFlags Flags;
        
        public MarshalAsAttribute MarshalAs;

        public int PointerIndirections;

        #region Properties

        public bool MarshalsIn
        {
            get
            { return ((Flags & MarshalFlags.MarshalIn) == MarshalFlags.MarshalIn); }
        }

        public bool MarshalsOut
        {
            get
            { return ((Flags & MarshalFlags.MarshalOut) == MarshalFlags.MarshalOut); }
        }

        public bool AnsiStrings
        {
            get
            { return ((Flags & MarshalFlags.AnsiStrings) == MarshalFlags.AnsiStrings); }
        }

        public bool UnicodeStrings
        {
            get
            { return ((Flags & MarshalFlags.UnicodeStrings) == MarshalFlags.UnicodeStrings); }
        }

        public bool AnsiPlatform
        {
            get
            { return ((Flags & MarshalFlags.AnsiPlatform) == MarshalFlags.AnsiPlatform); }
        }

        public bool IsComInterop
        {
            get
            { return ((Flags & MarshalFlags.ComInterop) == MarshalFlags.ComInterop); }
        }

        public bool IsByRefParam
        {
            get
            { return ((Flags & MarshalFlags.ByRefParam) == MarshalFlags.ByRefParam); }
        }

        public bool IsRetValParam
        {
            get
            { return ((Flags & MarshalFlags.RetValParam) == MarshalFlags.RetValParam); }
        }

        public bool IsCallbackParam
        {
            get
            { return ((Flags & MarshalFlags.CallbackParam) == MarshalFlags.CallbackParam); }
        }

        public bool IsStructField
        {
            get
            { return ((Flags & MarshalFlags.StructField) == MarshalFlags.StructField); }
        }

        public bool IsArrayElement
        {
            get
            { return ((Flags & MarshalFlags.ArrayElement) == MarshalFlags.ArrayElement); }
        }

        public bool IsSysArrayElement
        {
            get
            { return ((Flags & MarshalFlags.SysArrayElement) == MarshalFlags.SysArrayElement); }
        }

        public bool IsPlatform64Bit
        {
            get
            { return ((Flags & MarshalFlags.Platform64Bit) == MarshalFlags.Platform64Bit); }
        }

        public bool ShouldMarshalStringsAnsi
        {
            get
            {
                if (AnsiStrings) return true;
                if (UnicodeStrings) return false;
                return AnsiPlatform;
            }
        }

        public bool HasNonDefaultMarshaling
        {
            get
            { return (MarshalAs != null); }
        }

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new <see cref="NativeTypeDesc"/> out of a <see cref="ParameterInfo"/>.
        /// </summary>
        public NativeTypeDesc(ParameterInfo pi, MarshalFlags flags)
        {
            Debug.Assert(pi != null);

            SetupTypeAndFlags(pi.ParameterType, flags);

            this.ParameterInfo = pi;

            if ((pi.Attributes & ParameterAttributes.In) == ParameterAttributes.In) this.Flags |= MarshalFlags.MarshalIn;
            if ((pi.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out) this.Flags |= MarshalFlags.MarshalOut;
            if ((pi.Attributes & ParameterAttributes.Retval) == ParameterAttributes.Retval) this.Flags |= MarshalFlags.RetValParam; 
            
            if ((pi.Attributes & ParameterAttributes.HasFieldMarshal) == ParameterAttributes.HasFieldMarshal)
            {
                object[] custom_attrs = pi.GetCustomAttributes(typeof(MarshalAsAttribute), false);

                if (custom_attrs != null && custom_attrs.Length == 1)
                {
                    this.MarshalAs = (MarshalAsAttribute)custom_attrs[0];
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="NativeTypeDesc"/> out of a <see cref="FieldInfo"/>.
        /// </summary>
        public NativeTypeDesc(FieldInfo fi, MarshalFlags flags)
        {
            Debug.Assert(fi != null && !fi.IsStatic);

            SetupTypeAndFlags(fi.FieldType, flags);

            if ((fi.Attributes & FieldAttributes.HasFieldMarshal) == FieldAttributes.HasFieldMarshal)
            {
                object[] custom_attrs = fi.GetCustomAttributes(typeof(MarshalAsAttribute), false);

                if (custom_attrs != null && custom_attrs.Length == 1)
                {
                    this.MarshalAs = (MarshalAsAttribute)custom_attrs[0];
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="NativeTypeDesc"/> out of a <see cref="Type"/>.
        /// </summary>
        public NativeTypeDesc(Type type, MarshalFlags flags)
        {
            Debug.Assert(type != null);

            SetupTypeAndFlags(type, flags);
        }

        /// <summary>
        /// Creates a new <see cref="NativeTypeDesc"/> out of a <see cref="Type"/> and a given <see cref="UnmanagedType"/>.
        /// </summary>
        public NativeTypeDesc(Type type, UnmanagedType unmngType, MarshalFlags flags)
        {
            Debug.Assert(type != null);

            SetupTypeAndFlags(type, flags);

            if (unmngType != (UnmanagedType)0)
            {
                // create a fake MarshalAs attribute instance
                this.MarshalAs = new MarshalAsAttribute(unmngType);
            }
        }

        private NativeTypeDesc()
        { }

        private void SetupTypeAndFlags(Type type, MarshalFlags flags)
        {
            this.Flags = flags;

            // strip &
            if (type.IsByRef)
            {
                this.Flags |= MarshalFlags.ByRefParam;
                type = type.GetElementType();
            }

            // strip *
            while (type.IsPointer)
            {
                this.PointerIndirections++;
                type = type.GetElementType();
            }

            this.Type = type;
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            NativeTypeDesc copy = new NativeTypeDesc();
            copy.Type = this.Type;
            copy.Flags = this.Flags;
            copy.MarshalAs = this.MarshalAs;
            copy.PointerIndirections = this.PointerIndirections;

            return copy;
        }

        #endregion
    }

    #endregion

    [Serializable]
    public class NativeSignature : ICodePrintable
    {
        #region Fields

        private Log _log;

        private bool isFunctionPointer;
        private bool isComInterop;

        private string name;
        private CallingConvention callingConvention;

        private NativeType returnType;
        private NativeParameter[] parameters;

        #endregion

        #region Properties

        internal Log Log
        {
            get
            {
                if (_log == null) _log = new Log();
                return _log;
            }
        }

        public string Name
        {
            get { return name; }
            set { name = value; } // needed for fcn pointers wrapped in structures
        }

        public NativeType ReturnType
        {
            get { return returnType; }
        }

        public NativeParameter[] Parameters
        {
            get { return parameters; }
        }

        public int ParameterCount
        {
            get { return parameters.Length; }
        }

        #endregion

        #region Construction

        private NativeSignature(bool isFunctionPointer, bool isComInterop)
        {
            this.isFunctionPointer = isFunctionPointer;
            this.isComInterop = isComInterop;
        }

        /// <summary>
        /// Sets the <see cref="name"/> and adds notices about possible alternative names.
        /// </summary>
        private void SetPInvokeName(DllImportAttribute dllImp)
        {
            bool have_ordinal = dllImp.EntryPoint.StartsWith("#");

            if (have_ordinal)
                this.name = "Ordinal_" + dllImp.EntryPoint.Substring(1);
            else
                this.name = dllImp.EntryPoint;

            // P/Invoke may also try to append A or W when finding the entrypoint
            if (!dllImp.ExactSpelling && !have_ordinal)
            {
                switch (dllImp.CharSet)
                {
                    case CharSet.None:
                    case CharSet.Ansi:
                    {
                        // the "A" variant is searched after the exact spelling
                        string name_a = this.name + "A";
                        Log.Add(Errors.INFO_PossibleAltNameLookup, name_a, this.name);
                        break;
                    }

                    case CharSet.Unicode:
                    {
                        // the "W" variant is searched before the exact spelling
                        string name_w = this.name + "W";
                        Log.Add(Errors.INFO_PossibleAltNameLookup, this.name, name_w);
                        this.name = name_w;
                        break;
                    }

                    case CharSet.Auto:
                    {
                        // the lookup logic is based on the target platform
                        string name_a = this.name + "A";
                        string name_w = this.name + "W";
                        Log.Add(Errors.INFO_PossibleAutoAltNameLookup, name_a, name_w);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="NativeSignature"/> from a supplied P/Invoke <see cref="MethodInfo"/>.
        /// </summary>
        public static NativeSignature FromPInvokeSignature(MethodInfo mi, bool ansiPlatform, bool platform64bit)
        {
            Debug.Assert(mi != null && (mi.Attributes & MethodAttributes.PinvokeImpl) == MethodAttributes.PinvokeImpl);

            NativeSignature sig = new NativeSignature(false, false);

            // P/Invokes must be static
            if (!mi.IsStatic)
                sig.Log.Add(Errors.ERROR_PInvokeIsNotStatic);

            // P/Invokes should be non-public
            if (mi.IsPublic)
                sig.Log.Add(Errors.WARN_PInvokeIsPublic);

            object[] attrs = mi.GetCustomAttributes(typeof(DllImportAttribute), false);
            Debug.Assert(attrs.Length == 1);

            DllImportAttribute dllimp = (DllImportAttribute)attrs[0];

            sig.SetPInvokeName(dllimp);
            sig.callingConvention = dllimp.CallingConvention;

            // set up marshal flags
            MarshalFlags flags = MarshalFlags.None;
            if (ansiPlatform) flags |= MarshalFlags.AnsiPlatform;
            if (platform64bit) flags |= MarshalFlags.Platform64Bit;

            flags |= Utility.GetCharSetMarshalFlag(dllimp.CharSet);
            
            ParameterInfo[] pi = mi.GetParameters();
            int par_count = pi.Length;

            // check for vararg
            if ((mi.CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
            {
                par_count++;

                if (dllimp.CallingConvention != CallingConvention.Cdecl)
                {
                    // vararg P/Invokes should use cdecl calling convention
                    sig.Log.Add(Errors.WARN_VarargIsNotCdecl, dllimp.CallingConvention.ToString());
                }
            }

            // check for implicit HRESULT return value
            if (!dllimp.PreserveSig && mi.ReturnType != typeof(void))
            {
                par_count++;
            }

            // set parameters
            sig.parameters = new NativeParameter[par_count];

            for (int i = 0; i < pi.Length; i++)
            {
                sig.parameters[i] = NativeParameter.FromClrParameter(pi[i], flags);
            }

            if ((mi.CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
            {
                sig.parameters[par_count - 1] = new NativeParameter("...", null);
            }
                
            // set return type
            if (!dllimp.PreserveSig)
            {
                // make it return HRESULT
                sig.returnType = new PrimitiveNativeType(TypeName.Error, platform64bit);

                if (mi.ReturnType != typeof(void))
                {
                    ParameterInfo ret_pi = Utility.MakeRetParameterInfo(mi.ReturnParameter, pi.Length);
                    sig.parameters[pi.Length] = NativeParameter.FromClrParameter(ret_pi, flags);
                }
            }
            else
            {
                sig.returnType = NativeType.FromClrParameter(mi.ReturnParameter, flags | MarshalFlags.RetValParam);
            }

            return sig;
        }

        /// <summary>
        /// Creates a new <see cref="NativeSignature"/> from a supplied COM Interop <see cref="MethodInfo"/>.
        /// </summary>
        public static NativeSignature FromComInteropSignature(MethodInfo mi, bool ansiPlatform, bool platform64bit)
        {
            Debug.Assert(mi != null);

            MethodImplAttributes mia = mi.GetMethodImplementationFlags();

            Debug.Assert(mi.DeclaringType != null && mi.DeclaringType.IsImport &&
                (mia & MethodImplAttributes.InternalCall) == MethodImplAttributes.InternalCall);

            NativeSignature sig = new NativeSignature(false, true);

            // COM always uses stdcall
            sig.name = Utility.MakeCIdentifier(mi.Name);
            sig.callingConvention = CallingConvention.StdCall;

            // set up marshal flags
            MarshalFlags flags = MarshalFlags.ComInterop;
            if (ansiPlatform) flags |= MarshalFlags.AnsiPlatform;
            if (platform64bit) flags |= MarshalFlags.Platform64Bit;

            if (mi.DeclaringType.IsAnsiClass) flags |= MarshalFlags.AnsiStrings;
            else if (mi.DeclaringType.IsUnicodeClass) flags |= MarshalFlags.UnicodeStrings;

            ParameterInfo[] pi = mi.GetParameters();
            int par_count = pi.Length;

            // check for implicit HRESULT return value
            if ((mia & MethodImplAttributes.PreserveSig) != MethodImplAttributes.PreserveSig &&
                mi.ReturnType != typeof(void))
            {
                par_count++;
            }

            // set parameters
            sig.parameters = new NativeParameter[par_count];

            for (int i = 0; i < pi.Length; i++)
            {
                sig.parameters[i] = NativeParameter.FromClrParameter(pi[i], flags);
            }

            // set return type
            if ((mia & MethodImplAttributes.PreserveSig) != MethodImplAttributes.PreserveSig)
            {
                // make it return HRESULT
                sig.returnType = new PrimitiveNativeType(TypeName.Error, platform64bit);

                if (mi.ReturnType != typeof(void))
                {
                    ParameterInfo ret_pi = Utility.MakeRetParameterInfo(mi.ReturnParameter, pi.Length);
                    sig.parameters[pi.Length] = NativeParameter.FromClrParameter(ret_pi, flags);
                }
            }
            else
            {
                sig.returnType = NativeType.FromClrParameter(mi.ReturnParameter, flags);
            }

            return sig;
        }

        /// <summary>
        /// Creates a new <see cref="NativeSignature"/> from a supplied delegate <see cref="Type"/>.
        /// </summary>
        public static NativeSignature FromDelegateType(Type delegateType, bool ansiPlatform, bool platform64bit)
        {
            Debug.Assert(delegateType != null && typeof(Delegate).IsAssignableFrom(delegateType));

            NativeSignature sig = new NativeSignature(true, false);

            // set up marshal flags
            MarshalFlags flags = MarshalFlags.CallbackParam;
            if (ansiPlatform) flags |= MarshalFlags.AnsiPlatform;
            if (platform64bit) flags |= MarshalFlags.Platform64Bit;

            // callbacks use stdcall by default
            sig.callingConvention = CallingConvention.Winapi;
            sig.name = Utility.GetNameOfType(delegateType);

            // look to see if the delegate is decorated by UnmanagedFunctionPointerAttribute
            object[] attrs = delegateType.GetCustomAttributes(typeof(UnmanagedFunctionPointerAttribute), false);
            if (attrs.Length > 0)
            {
                UnmanagedFunctionPointerAttribute fnptr = (UnmanagedFunctionPointerAttribute)attrs[0];

                sig.callingConvention = fnptr.CallingConvention;
                flags |= Utility.GetCharSetMarshalFlag(fnptr.CharSet);
            }
            else
            {
                // charset is Ansi by default
                flags |= MarshalFlags.AnsiStrings;
            }

            MethodInfo mi = delegateType.GetMethod("Invoke");
            if (mi == null)
            {
                sig.parameters = new NativeParameter[0];
                sig.returnType = new PrimitiveNativeType(TypeName.Void, platform64bit);

                sig.Log.Add(Errors.WARN_NonSpecificDelegateUsed, delegateType.Name);

                sig.parameters = new NativeParameter[0];
                sig.returnType = new PrimitiveNativeType(TypeName.I4, platform64bit);
            }
            else
            {
                ParameterInfo[] pi = mi.GetParameters();

                // set parameters
                sig.parameters = new NativeParameter[pi.Length];

                for (int i = 0; i < pi.Length; i++)
                {
                    sig.parameters[i] = NativeParameter.FromClrParameter(pi[i], flags);
                }

                sig.returnType = NativeType.FromClrParameter(mi.ReturnParameter, flags);
            }
            return sig;
        }

        #endregion

        #region ICodePrintable Members

        public void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            // print the signature log to the log printer
            if (_log != null)
            {
                string prefix = (isFunctionPointer ? Resources.FunctionPointer : Resources.Function);
                _log.Print(logPrinter, String.Format(prefix, name));
            }

            // print the return value log
            returnType.PrintLog(logPrinter, flags, Resources.ReturnValue);

            returnType.PrintTo(printer, logPrinter, flags);
            printer.Print(OutputType.Other, " ");

            if (isFunctionPointer) printer.Print(OutputType.Operator, "(");

            switch (callingConvention)
            {
                case CallingConvention.Cdecl:    printer.Print(OutputType.Keyword, "__cdecl");    break;
                case CallingConvention.FastCall: printer.Print(OutputType.Keyword, "__fastcall"); break;

                case CallingConvention.StdCall:
                {
                    if (isComInterop && (flags & PrintFlags.UsePlainC) != PrintFlags.UsePlainC)
                        printer.Print(OutputType.TypeName, "STDMETHODCALLTYPE");
                    else
                        printer.Print(OutputType.Keyword, "__stdcall");
                    break;
                }

                case CallingConvention.ThisCall: printer.Print(OutputType.Keyword, "__thiscall"); break;
                case CallingConvention.Winapi:
                {
                    if ((flags & PrintFlags.UsePlainC) == PrintFlags.UsePlainC) goto case CallingConvention.StdCall;
                    else
                    {
                        if (isFunctionPointer)
                            printer.Print(OutputType.Keyword, "CALLBACK");
                        else
                            printer.Print(OutputType.Keyword, "WINAPI");
                    }
                    break;
                }

                default:
                {
                    Debug.Fail(null);
                    break;
                }
            }
            printer.Print(OutputType.Other, " ");

            if (isFunctionPointer)
            {
                // function pointers print as "ret_type (call_conv *name)(params)"
                printer.Print(OutputType.Operator, "*");
                printer.Print(OutputType.Other, " ");
                printer.Print(OutputType.TypeName, name);
                printer.Print(OutputType.Operator, ")");
            }
            else
            {
                printer.Print(OutputType.Identifier, name);
            }

            printer.Print(OutputType.Operator, "(");
            if (parameters.Length == 0)
            {
                if ((flags & PrintFlags.UsePlainC) == PrintFlags.UsePlainC)
                {
                    // parameterless function should be declared as f(void) in C
                    printer.Print(OutputType.Keyword, TypeName.Void.PlainC);
                }
            }
            else
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i].PrintTo(printer, logPrinter, flags);

                    if (i < parameters.Length - 1)
                    {
                        printer.Print(OutputType.Operator, ",");
                        printer.Print(OutputType.Other, " ");
                    }
                }
            }
            printer.Print(OutputType.Operator, ");");
        }

        #endregion

        #region Definition Enumeration

        public IEnumerable<NativeTypeDefinition> GetDefinitions()
        {
            NativeTypeDefinitionSet set = new NativeTypeDefinitionSet();

            GetDefinitionsRecursive(set, null);

            return set;
        }

        internal void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
        {
            // enumerate return type and parameter types to find all related definitions avoiding duplicates
            returnType.GetDefinitionsRecursive(set, parentDef);

            for (int i = 0; i < parameters.Length; i++)
            {
                // Type may be null of special parameters like the ... ellipsis
                if (parameters[i].Type != null)
                {
                    parameters[i].Type.GetDefinitionsRecursive(set, parentDef);
                }
            }
        }

        #endregion

        #region Method Classification

        public static bool IsPInvoke(MethodInfo method)
        {
            return ((method.Attributes & MethodAttributes.PinvokeImpl) == MethodAttributes.PinvokeImpl);
        }

        public static bool IsRCWMethod(MethodInfo method)
        {
            return (method.DeclaringType.IsImport &&
                (method.GetMethodImplementationFlags() & MethodImplAttributes.InternalCall) == 
                MethodImplAttributes.InternalCall);
        }

        #endregion
    }

    [Serializable]
    public class NativeParameter : ICodePrintable
    {
        #region Fields

        private string name;
        private NativeType type;
        private object defaultValue;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
        }

        public NativeType Type
        {
            get { return type; }
        }

        public bool HasDefaultValue
        {
            get { return (defaultValue != this); }
        }

        public object DefaultValue
        {
            get
            {
                if (defaultValue == this) throw new InvalidOperationException();
                return defaultValue;
            }
        }

        #endregion

        #region Construction

        public NativeParameter(string name, NativeType nt, object defaultValue)
        {
            Debug.Assert(!String.IsNullOrEmpty(name));

            this.name = name;
            this.type = nt;
            this.defaultValue = defaultValue;
        }

        public NativeParameter(string name, NativeType nt)
            : this(name, nt, null)
        {
            this.defaultValue = this;
        }

        public static NativeParameter FromClrParameter(ParameterInfo pi, MarshalFlags flags)
        {
            Debug.Assert(pi != null);

            string name = Utility.MakeCIdentifier(pi.Name);
            if (String.IsNullOrEmpty(name))
            {
                name = "arg" + pi.Position;
            }

            NativeType nt = NativeType.FromClrParameter(pi, flags);

            if ((pi.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault)
            {
                return new NativeParameter(name, nt, pi.RawDefaultValue);
            }
            else
            {
                return new NativeParameter(name, nt);
            }
        }

        public static NativeParameter FromName(string name)
        {
            Debug.Assert(!String.IsNullOrEmpty(name));

            return new NativeParameter(name, null);
        }

        #endregion

        #region ICodePrintable Members

        public void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            if (type != null)
            {
                // print the log of the underlying type
                type.PrintLog(logPrinter, flags, String.Format(Resources.Parameter, name));
                
                // print marshal direction of this parameter
                if ((flags & PrintFlags.PrintMarshalDirection) == PrintFlags.PrintMarshalDirection)
                {
                    type.PrintMarshalDirection(printer, flags);
                }

                // print the underlying type
                type.PrintTo(printer, logPrinter, flags);
            }

            printer.Print(OutputType.Other, " ");
            printer.Print(OutputType.Identifier, name);

            if (type != null)
            {
                type.PrintPostIdentifierTo(printer, flags);
            }

            if (HasDefaultValue)
            {
                bool plain_c = (flags & PrintFlags.UsePlainC) == PrintFlags.UsePlainC;

                if (plain_c)
                {
                    // C does not support default parameter values - make it a comment
                    printer.Print(OutputType.Other, " ");
                    printer.Print(OutputType.Comment, "/* ");
                }

                printer.Print((plain_c ? OutputType.Comment : OutputType.Other), " ");
                printer.Print((plain_c ? OutputType.Comment : OutputType.Operator), "=");
                printer.Print((plain_c ? OutputType.Comment : OutputType.Other), " ");

                if (DefaultValue == null)
                {
                    if (plain_c)
                        printer.Print(OutputType.Comment, "0");
                    else
                        printer.Print(OutputType.Identifier, "NULL");
                }
                else
                {
                    string def_str = DefaultValue as string;
                    if (def_str != null)
                    {
                        def_str = Utility.StringToLiteral(def_str);
                    }
                    else
                    {
                        Debug.Assert(def_str.GetType().IsPrimitive);
                        def_str = DefaultValue.ToString();
                    }

                    printer.Print((plain_c ? OutputType.Comment : OutputType.Literal), def_str);
                }

                if (plain_c)
                {
                    // close the comment
                    printer.Print(OutputType.Comment, " */");
                    printer.Print(OutputType.Other, " ");
                }
            }
        }

        #endregion
    }

    [Serializable]
    public class NativeField : ICodePrintable
    {
        #region Fields

        private string name;
        private NativeType type;
        private bool containsManagedReference;
        private int? offset;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
        }

        public NativeType Type
        {
            get { return type; }
        }

        // used for ref type alignment and overlapping tests in structures with explicit layout
        public bool ContainsManagedReference
        {
            get { return containsManagedReference; }
        }

        public int? Offset
        {
            get { return offset; }
        }

        #endregion

        #region Construction

        public NativeField(string name, NativeType nt, bool containsManagedReference, int? offset)
        {
            Debug.Assert(!String.IsNullOrEmpty(name));

            this.name = name;
            this.type = nt;
            this.offset = offset;
            this.containsManagedReference = containsManagedReference;
        }

        public NativeField(string name, NativeType nt, bool containsManagedReference)
            : this(name, nt, containsManagedReference, null)
        { }

        public static NativeField FromClrField(FieldInfo fi, MarshalFlags flags)
        {
            Debug.Assert(fi != null);

            string name = Utility.MakeCIdentifier(fi.Name);
            if (String.IsNullOrEmpty(name))
            {
                name = String.Format("field{0:x}", fi.MetadataToken);
            }

            NativeType nt = NativeType.FromClrField(fi, flags);
            
            // determine whether the type of the field contains a managed reference
            bool contains_reference = false;
            if (fi.FieldType.IsValueType)
            {
                StructureNativeType snt = nt as StructureNativeType;
                if (snt != null)
                {
                    StructureDefinition definition = (StructureDefinition)snt.Definition;
                    for (int i = 0; i < definition.FieldCount; i++)
                    {
                        if (definition.GetField(i).ContainsManagedReference)
                        {
                            contains_reference = true;
                            break;
                        }
                    }
                }
            }
            else contains_reference = true;

            object[] custom_attrs = fi.GetCustomAttributes(typeof(FieldOffsetAttribute), false);

            Debug.Assert(custom_attrs.Length == 0 || custom_attrs.Length == 1);
            if (custom_attrs.Length > 0)
            {
                FieldOffsetAttribute foa = (FieldOffsetAttribute)custom_attrs[0];
                return new NativeField(name, nt, contains_reference, foa.Value);
            }
            else
            {
                return new NativeField(name, nt, contains_reference);
            }
        }

        internal void SetOffset(int offset)
        {
            this.offset = offset;
        }

        #endregion

        #region ICodePrintable Members

        public void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            // print the log of the underlying type
            type.PrintLog(logPrinter, flags, String.Format(Resources.Field, name));

            type.PrintTo(printer, logPrinter, flags);

            printer.Print(OutputType.Other, " ");
            printer.Print(OutputType.Identifier, name);

            type.PrintPostIdentifierTo(printer, flags);

            printer.Print(OutputType.Operator, ";");
        }

        #endregion
    }

    [Serializable]
    public abstract class NativeType : ICodePrintable
    {
        #region Fields

        internal Log _log;
        protected int indirections;

        protected bool isByrefParameter;
        protected bool descMarshalsIn;
        protected bool descMarshalsOut;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the size of the type including any padding.
        /// </summary>
        public abstract int TypeSize
        {
            get;
        }

        public abstract int AlignmentRequirement
        {
            get;
        }

        public virtual bool MarshalsAsPointerWithKnownDirection
        {
            get
            { return isByrefParameter; }
        }

        public abstract bool MarshalsIn
        {
            get;
        }

        public abstract bool MarshalsOut
        {
            get;
        }

        internal Log Log
        {
            get
            {
                if (_log == null) _log = new Log();
                return _log;
            }
        }

        public virtual bool IsInvalid
        {
            get
            { return false; }
        }

        #endregion

        #region Construction

        protected NativeType()
        { }

        protected NativeType(NativeTypeDesc desc)
        {
            this.isByrefParameter = desc.IsByRefParam;
            this.descMarshalsIn = desc.MarshalsIn;
            this.descMarshalsOut = desc.MarshalsOut;
        }  

        public static NativeType FromClrParameter(ParameterInfo pi, MarshalFlags flags)
        {
            if (pi == null)
                throw new ArgumentNullException("pi");

            NativeTypeDesc desc = new NativeTypeDesc(pi,
                flags & ~(MarshalFlags.ArrayElement | MarshalFlags.StructField));

            return CreateFromKey(desc);
        }

        public static NativeType FromClrField(FieldInfo fi, MarshalFlags flags)
        {
            if (fi == null)
                throw new ArgumentNullException("fi");

            NativeTypeDesc desc = new NativeTypeDesc(fi,
                (flags & ~MarshalFlags.ArrayElement) | MarshalFlags.StructField);

            return CreateFromKey(desc);
        }

        public static NativeType FromClrType(Type type, MarshalFlags flags)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            NativeTypeDesc desc = new NativeTypeDesc(type, flags);

            return CreateFromKey(desc);
        }

        public static NativeType FromClrArrayElement(Type arrayType, UnmanagedType elementUnmanagedType, MarshalFlags flags)
        {
            if (arrayType == null)
                throw new ArgumentNullException("arrayType");

            Type element_managed_type;
            if (arrayType == typeof(System.Array))
            {
                element_managed_type = GetTypeForUnmanagedType(elementUnmanagedType, flags);

                // keep the StructField flag
                flags |= MarshalFlags.ArrayElement;
                flags |= MarshalFlags.SysArrayElement;
            }
            else if (arrayType.IsArray)
            {
                element_managed_type = arrayType.GetElementType();

                // keep the StructField flag
                flags |= MarshalFlags.ArrayElement;
            }
            else
            {
                throw new ArgumentException("", "arrayType");
            }

            // we intentionally do not clear MarshalFlags.ByRefParam because we want the by-ref
            // property to be inherited by the element in some cases (see ArrayNativeType.PrintTo)
            flags &= ~(MarshalFlags.MarshalIn | MarshalFlags.MarshalOut);

            NativeTypeDesc desc = new NativeTypeDesc(
                element_managed_type,
                elementUnmanagedType,
                flags);

            return CreateFromKey(desc);
        }

        private static Type GetTypeForUnmanagedType(UnmanagedType unmngType, MarshalFlags flags)
        {
            // ignore desc.Type and decide according to unmanaged type
            switch (unmngType)
            {
                case UnmanagedType.AnsiBStr:
                case UnmanagedType.BStr:
                case UnmanagedType.TBStr:
                case UnmanagedType.ByValTStr:
                case UnmanagedType.VBByRefStr:
                case UnmanagedType.LPStr:
                case UnmanagedType.LPTStr:
                case UnmanagedType.LPWStr: return typeof(string);

                case UnmanagedType.Bool:
                case UnmanagedType.VariantBool: return typeof(bool);

                case 0:
                case UnmanagedType.AsAny:
                case UnmanagedType.ByValArray:
                case UnmanagedType.IDispatch:
                case UnmanagedType.Interface:
                case UnmanagedType.IUnknown:
                case UnmanagedType.Struct:
                case UnmanagedType.LPStruct:
                case UnmanagedType.CustomMarshaler: return typeof(System.Object);

                case UnmanagedType.FunctionPtr: return typeof(System.Delegate);

                case UnmanagedType.I1: return typeof(sbyte);
                case UnmanagedType.I2: return typeof(short);
                case UnmanagedType.Error:
                case UnmanagedType.I4: return typeof(int);
                case UnmanagedType.I8: return typeof(long);
                case UnmanagedType.U1: return typeof(byte);
                case UnmanagedType.U2: return typeof(ushort);
                case UnmanagedType.U4: return typeof(uint);
                case UnmanagedType.U8: return typeof(ulong);

                case UnmanagedType.R4: return typeof(float);
                case UnmanagedType.R8: return typeof(double);
                case UnmanagedType.Currency: return typeof(decimal);

                case UnmanagedType.SysInt: return typeof(System.IntPtr);
                case UnmanagedType.SysUInt: return typeof(System.UIntPtr);

                case UnmanagedType.LPArray:
                case UnmanagedType.SafeArray: return typeof(System.Array);

                default:
                {
                    Debug.Fail(null);
                    return null;
                }
            }
        }

        protected UnmanagedType ValidateUnmanagedType(NativeTypeDesc desc, UnmanagedType[] allowedUnmanagedTypes)
        {
            if (allowedUnmanagedTypes.Length == 0)
            {
                if (desc.MarshalAs != null)
                {
                    Log.Add(Errors.ERROR_InvalidManagedUnmanagedCombo,
                        desc.Type.FullName, desc.MarshalAs.Value.ToString());
                }
                return UnmanagedType.AsAny;
            }
            else
            {
                return ValidateUnmanagedType(desc, allowedUnmanagedTypes, allowedUnmanagedTypes[0]);
            }
        }

        protected UnmanagedType ValidateUnmanagedType(NativeTypeDesc desc, UnmanagedType[] allowedUnmanagedTypes,
            UnmanagedType defaultUnmanagedType)
        {
            if (desc.MarshalAs != null)
            {
                UnmanagedType ut = desc.MarshalAs.Value;

                // verify that this managed/unmanaged type combination is allowed
                for (int i = 0; i < allowedUnmanagedTypes.Length; i++)
                {
                    if (allowedUnmanagedTypes[i] == ut) return ut;
                }

                // report error and use the default
                Log.Add(Errors.ERROR_InvalidManagedUnmanagedCombo, desc.Type.FullName, ut.ToString());
            }

            return defaultUnmanagedType;
        }

        protected void CheckPointersToReferenceType(NativeTypeDesc desc)
        {
            Debug.Assert(!desc.Type.IsValueType);

            if (desc.PointerIndirections > 0)
            {
                Log.Add(Errors.ERROR_UnmanagedPointersToRefType, desc.Type.FullName);
            }
        }

        private static NativeType CreateFromKey(NativeTypeDesc desc)
        {
            Type type = desc.Type;

            // check for custom marshalers
            if (desc.MarshalAs != null && desc.MarshalAs.Value == UnmanagedType.CustomMarshaler)
            {
                return new CustomMarshaledNativeType(desc);
            }

            // enums must be checked prior to the switch because they have a primitive type code
            if (type.IsEnum)
            {
                return new EnumNativeType(desc);
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:  return new PrimitiveNativeType(desc, (desc.IsComInterop ? MarshalType.BooleanC : MarshalType.BooleanP));
                case TypeCode.Byte:     return new PrimitiveNativeType(desc, MarshalType.Byte);
                case TypeCode.Double:   return new PrimitiveNativeType(desc, MarshalType.Double);
                case TypeCode.Int16:    return new PrimitiveNativeType(desc, MarshalType.Int16);
                case TypeCode.Int32:    return new PrimitiveNativeType(desc, MarshalType.Int32);
                case TypeCode.Int64:    return new PrimitiveNativeType(desc, MarshalType.Int64);
                case TypeCode.SByte:    return new PrimitiveNativeType(desc, MarshalType.SByte);
                case TypeCode.Single:   return new PrimitiveNativeType(desc, MarshalType.Single);
                case TypeCode.UInt16:   return new PrimitiveNativeType(desc, MarshalType.UInt16);
                case TypeCode.UInt32:   return new PrimitiveNativeType(desc, MarshalType.UInt32);
                case TypeCode.UInt64:   return new PrimitiveNativeType(desc, MarshalType.UInt64);

                case TypeCode.Char:     return new CharNativeType(desc);
                case TypeCode.String:   return new StringNativeType(desc);

                case TypeCode.DateTime: return new DateNativeType(desc);
                case TypeCode.Decimal:  return new DecimalNativeType(desc);

                case TypeCode.DBNull:

                case TypeCode.Object:
                {
                    if (type.IsGenericType)
                    {
                        // generic types cannot be marshaled
                        NativeType res = new UnknownPointerNativeType(desc.IsPlatform64Bit);
                        res.Log.Add(Errors.ERROR_GenericTypesNotAllowed);
                        return res;
                    }

                    if (desc.MarshalAs != null && desc.MarshalAs.Value == UnmanagedType.Interface)
                    {
                        new InterfaceNativeType(desc);
                    }

                    if (type.IsValueType)
                    {
                        // void
                        if (type == typeof(void)) return new PrimitiveNativeType(TypeName.Void, desc.PointerIndirections, desc.IsPlatform64Bit);

                        // IntPtr/UIntPtr
                        if (type == typeof(IntPtr) || type == typeof(UIntPtr)) return new PrimitiveNativeType(desc, MarshalType.IntPtr);

                        // Guid
                        if (type == typeof(Guid)) return new GuidNativeType(desc);

                        if (!desc.IsStructField)
                        {
                            // ArrayWithOffset
                            if (type == typeof(ArrayWithOffset)) return new UnknownPointerNativeType(desc, MarshalType.Empty);

                            // HandleRef
                            if (type == typeof(HandleRef)) return new HandleNativeType(desc);
                        }

                        // Color (only as param)
                        if (type == typeof(System.Drawing.Color) && !desc.IsStructField) return new ColorNativeType(desc);

                        // System.ArgIterator
                        if (type == typeof(System.ArgIterator)) return new VariableArgumentListNativeType(desc);

                        // a non-generic structure (with layout)
                        return new StructureNativeType(desc);
                    }
                    else
                    {
                        // Delegate
                        if (Utility.IsDelegate(type)) return new CallbackNativeType(desc);

                        // StringBuilder
                        if (type == typeof(StringBuilder)) return new StringNativeType(desc);

                        if (!desc.IsArrayElement && Utility.HasLayout(type))
                        {
                            // classes with layout will be embedded in structures or passed with an additional
                            // pointer indirection and in-only default marshaling direction
                            return new StructureNativeType(desc);
                        }

                        // System.Object
                        if (type == typeof(System.Object)) return new InterfaceNativeType(desc);

                        // System.Array
                        if (type == typeof(System.Array))
                        {
                            if (desc.MarshalAs != null &&
                                Array.IndexOf(desc.IsStructField ? MarshalType.ArrayClassField : MarshalType.ArrayClass,
                                desc.MarshalAs.Value) > 0)
                            {
                                // only use ArrayNativeType when it's explicitly SafeArray or ByValArray
                                return new ArrayNativeType(desc);
                            }
                            else
                            {
                                return new InterfaceNativeType(desc);
                            }
                        }

                        // System.Collections.IEnumerator
                        if (type == typeof(System.Collections.IEnumerator) && !desc.HasNonDefaultMarshaling)
                        {
                            return new InterfaceNativeType(desc, TypeName.IEnumVARIANT);
                        }

                        // System.Runtime.InteropServices.SafeHandle & System.Runtime.InteropServices.CriticalHandle
                        if (typeof(SafeHandle).IsAssignableFrom(type) || typeof(CriticalHandle).IsAssignableFrom(type))
                        {
                            return new HandleNativeType(desc);
                        }

                        // an interface
                        if (type.IsInterface) return new InterfaceNativeType(desc);

                        // an array
                        if (type.IsArray) return new ArrayNativeType(desc);

                        // everything else is converted into a COM interface pointer
                        return new InterfaceNativeType(desc);
                    }
                }

                default:
                {
                    Debug.Fail(null);
                    break;
                }
            }

            return null;
        }

        #endregion

        #region Hint Logging

        protected void VerifyMarshalDirection(NativeTypeDesc desc)
        {
            // process only top-level items
            if (desc.IsArrayElement || desc.IsStructField) return;

            if (desc.PointerIndirections == 0 && !desc.IsByRefParam)
            {
                // value types with UnmanagedType.LPStruct are actually indirected
                if (desc.Type.IsValueType &&
                    desc.MarshalAs != null &&
                    desc.MarshalAs.Value == UnmanagedType.LPStruct)
                {
                    return;
                }

                // System.Object with UnmanagedType.AsAny can be anything
                if (desc.Type == typeof(object) &&
                    desc.MarshalAs != null &&
                    desc.MarshalAs.Value == UnmanagedType.AsAny)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (desc.Type.IsValueType)
            {
                if (desc.MarshalsOut)
                {
                    // by-value value type cannot go out
                    Log.Add(Errors.WARN_ByValValueTypeMarkedOut);
                    descMarshalsOut = false;
                }
            }
            else
            {
                if (desc.Type.IsArray || (Utility.HasLayout(desc.Type) &&
                    (desc.MarshalAs == null || desc.MarshalAs.Value != UnmanagedType.Interface)))
                {
                    if (!desc.MarshalsIn && !desc.MarshalsOut)
                    {
                        // arrays and formatted ref types go in only by default
                        Log.Add(Errors.INFO_DefaultArrayAndRefTypeMarshaling);
                    }
                }
                else if (desc.Type == typeof(string) || desc.Type == typeof(StringBuilder))
                {
                    // handled by StringNativeType
                }
                else
                {
                    if (desc.MarshalsOut)
                    {
                        // any other reference type cannot go out
                        Log.Add(Errors.WARN_ByValRefTypeMarkedOut, desc.Type.FullName);
                        descMarshalsOut = false;
                    }
                }
            }
        }

        /// <summary>
        /// Explains what's going to happen regarding buffer allocation and release.
        /// </summary>
        protected void ExplainMemoryManagement(NativeTypeDesc desc, string subject)
        {
            if (desc.IsCallbackParam)
            {
                if (desc.IsByRefParam)
                {
                    if (desc.MarshalsIn == desc.MarshalsOut)
                    {
                        // by-ref inout parameter
                        Log.Add(Errors.INFO_BufferCallbackInOut, subject);
                    }
                    else if (desc.MarshalsIn)
                    {
                        // by-ref in parameter
                        Log.Add(Errors.INFO_BufferCallbackIn, subject);
                    }
                    else
                    {
                        // by-ref out parameter
                        Log.Add(Errors.INFO_BufferCallbackOut, subject);
                    }
                }
                else if (desc.IsRetValParam)
                {
                    // by-value return
                    Log.Add(Errors.INFO_BufferCallbackOut, subject);
                }
                else
                {
                    // by-value in parameter
                    Log.Add(Errors.INFO_BufferCallbackIn, subject);
                }
            }
            else
            {
                if (desc.IsByRefParam)
                {
                    if (desc.MarshalsIn == desc.MarshalsOut)
                    {
                        // by-ref inout parameter
                        Log.Add(Errors.INFO_BufferInOut, subject);
                    }
                    else if (desc.MarshalsIn)
                    {
                        // by-ref in parameter
                        Log.Add(Errors.INFO_BufferTemporaryIn, subject);
                    }
                    else
                    {
                        // by-ref out parameter
                        Log.Add(Errors.INFO_BufferOut, subject);
                    }
                }
                else if (desc.IsRetValParam)
                {
                    // by-value return
                    Log.Add(Errors.INFO_BufferOut, subject);
                }
            }
        }

        #endregion

        #region ICodePrintable Members

        public virtual void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            // The log is printed by the PrintLog called by NativeParameter/NativeField (we want the log to be
            // associated with the parameter/field as otherwise it would be confusing for the user).
        }

        public void PrintMarshalDirection(ICodePrinter printer, PrintFlags flags)
        {
            // print /*[in]*/, /*[out]*/, or /*[in,out]*/ for indirected parameter types
            if (MarshalsAsPointerWithKnownDirection)
            {
                if (MarshalsOut)
                {
                    if (MarshalsIn)
                        printer.Print(OutputType.Comment, "/*[in,out]*/");
                    else
                        printer.Print(OutputType.Comment, "/*[out]*/");
                }
                else
                {
                    printer.Print(OutputType.Comment, "/*[in]*/");
                }

                printer.Print(OutputType.Other, " ");
            }
        }

        public virtual void PrintPostIdentifierTo(ICodePrinter printer, PrintFlags flags)
        {
            // used arrays and fixed length embedded strings
        }

        public virtual void PrintLog(ILogPrinter logPrinter, PrintFlags flags, string messagePrefix)
        {
            if (_log != null)
            {
                _log.Print(logPrinter, messagePrefix);
            }
        }

        #endregion

        #region Definition Enumeration

        internal virtual void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
        {
            // overriden by types that have (DefinedNativeType) or may have (ArrayNativeType) a definition
        }

        #endregion
    }

    [Serializable]
    class UnknownPointerNativeType : PrimitiveNativeType
    {
        #region Construction

        public UnknownPointerNativeType(bool platform64bit)
            : base(TypeName.Void, 1, platform64bit)
        { }

        public UnknownPointerNativeType(NativeTypeDesc desc, UnmanagedType[] allowedUnmanagedTypes)
            : base(TypeName.Void, 1, desc.IsPlatform64Bit)
        {
            ValidateUnmanagedType(desc, allowedUnmanagedTypes);
        }

        #endregion
    }

    /// <summary>
    /// Represents a non-primitive native type that requires separate definition.
    /// </summary>
    [Serializable]
    public abstract class DefinedNativeType : NativeType
    {
        protected string name;
        protected string nameModifier;
        protected bool isConstPointer;
        protected NativeTypeDefinition typeDefinition;

        protected const string StructModifier = "struct";
        protected const string ClassModifier = "class";
        protected const string UnionModifier = "union";
        protected const string EnumModifier = "enum";

        #region Properties

        public NativeTypeDefinition Definition
        {
            get { return typeDefinition; }
        }

        public override int TypeSize
        {
            get
            {
                if (this.indirections > 0) return TypeName.GetPointerSize(typeDefinition.IsPlatform64Bit);
                else return typeDefinition.Size;
            }
        }

        public override int AlignmentRequirement
        {
            get
            {
                if (this.indirections > 0) return TypeName.GetPointerSize(typeDefinition.IsPlatform64Bit);
                return typeDefinition.Alignment;
            }
        }

        public override bool MarshalsIn
        {
            get
            { return (descMarshalsIn || !descMarshalsOut); }
        }

        public override bool MarshalsOut
        {
            get
            { return (descMarshalsOut || !descMarshalsIn); }
        }

        #endregion

        #region Construction

        protected DefinedNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            this.name = Utility.GetNameOfType(desc.Type);

            // count the total level of indirections
            this.indirections = desc.PointerIndirections;
            if (desc.IsByRefParam) this.indirections++;

            VerifyMarshalDirection(desc);
        }

        #endregion

        #region ICodePrintable Members

        public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            base.PrintTo(printer, logPrinter, flags);

            if (isConstPointer && indirections > 0)
            {
                printer.Print(OutputType.Keyword, "const");
                printer.Print(OutputType.Other, " ");
            }

            if (!String.IsNullOrEmpty(nameModifier))
            {
                printer.Print(OutputType.Keyword, nameModifier);
                printer.Print(OutputType.Other, " ");
            }

            printer.Print(OutputType.TypeName, name);

            if (indirections > 0)
            {
                printer.Print(OutputType.Other, " ");
                for (int i = 0; i < indirections; i++)
                {
                    printer.Print(OutputType.Operator, "*");
                }
            }
        }

        #endregion

        #region Definition Enumeration

        internal override void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
        {
            if (!set.Contains(typeDefinition))
            {
                set.Add(typeDefinition);
                typeDefinition.GetDefinitionsRecursive(set, typeDefinition);
            }
            if (parentDef != null) set.AddDependency(parentDef, typeDefinition);
        }

        #endregion
    }

    [Serializable]
    public abstract class NativeTypeDefinition : ICodePrintable
    {
        #region Fields

        internal Log _log;
        protected bool platform64bit;

        /// <summary>
        /// Type definitions are shared by all referring parameters/fields.
        /// </summary>
        private static Dictionary<TypeDefKey, NativeTypeDefinition> cache = new Dictionary<TypeDefKey, NativeTypeDefinition>();

        #endregion

        #region TypeDefKey

        protected class TypeDefKey
        {
            public readonly Type Type;
            public readonly MarshalFlags Flags;

            public TypeDefKey(Type type, MarshalFlags flags)
            {
                Debug.Assert(type != null);

                this.Type = type;
                this.Flags = flags;
            }

            public override bool Equals(object obj)
            {
                TypeDefKey other = obj as TypeDefKey;
                return (
                    other != null &&
                    this.Type.Equals(other.Type) &&
                    this.Flags == other.Flags);
            }

            public override int GetHashCode()
            {
                return (
                    Type.GetHashCode() ^
                    Flags.GetHashCode());
            }
        }

        #endregion

        #region Construction

        protected static T Get<T>(TypeDefKey key)
            where T : NativeTypeDefinition, new()
        {
            Debug.Assert(key != null);

            NativeTypeDefinition definition;
            T specific_definition;

            lock (cache)
            {
                if (cache.TryGetValue(key, out definition)) return (T)definition;

                specific_definition = new T();
                cache.Add(key, specific_definition);
            }

            specific_definition.Initialize(key);
            return specific_definition;
        }

        protected virtual void Initialize(TypeDefKey key)
        {
            this.platform64bit = ((key.Flags & MarshalFlags.Platform64Bit) == MarshalFlags.Platform64Bit);
        }

        #endregion

        #region Properties

        internal Log Log
        {
            get
            {
                if (_log == null) _log = new Log();
                return _log;
            }
        }

        public bool IsPlatform64Bit
        {
            get
            { return platform64bit; }
        }

        protected abstract string MessageLogPrefix
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Returns size of the type including any padding.
        /// </summary>
        public abstract int Size
        {
            get;
        }

        /// <summary>
        /// Returns the alignment requirement of the type.
        /// </summary>
        public virtual int Alignment
        {
            get
            { return this.Size; }
        }

        #endregion

        #region ICodePrintable Members

        public virtual void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            if (_log != null)
            {
                _log.Print(logPrinter, MessageLogPrefix);
            }
        }

        #endregion

        #region Definition Enumeration

        public virtual NativeTypeDefinition GetForwardDeclaration()
        {
            return null;
        }

        public abstract void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef);

        #endregion
    }

    public class NativeTypeDefinitionSet : Set<NativeTypeDefinition>
    {
        public NativeTypeDefinitionSet()
            : base(new Transformer<NativeTypeDefinition>(GetForwardDeclaration))
        { }

        static bool GetForwardDeclaration(ref NativeTypeDefinition definition)
        {
            // we are called because the a cycle was encountered during the set
            // enumeration - we should produce something like a forward declaration

            definition = definition.GetForwardDeclaration();
            return (definition != null);
        }
    }
}
