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
using System.Runtime.Serialization;
using SignatureGenerator;

namespace WindowsTool
{
    #region UnableToGetTypesException

    [Serializable]
    class UnableToGetTypesException : ApplicationException
    {
        private string[] loaderMessages;

        public string[] LoaderMessages
        {
            get { return loaderMessages; }
        }
        
        public UnableToGetTypesException(ReflectionTypeLoadException rtle)
            : base(rtle.Message)
        {
            loaderMessages = new string[rtle.LoaderExceptions.Length];

            for (int i = 0; i < rtle.LoaderExceptions.Length; i++)
            {
                loaderMessages[i] = rtle.LoaderExceptions[i].Message;
            }
        }

        protected UnableToGetTypesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            loaderMessages = (string[])info.GetValue("_loaderMessages", typeof(string[]));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_loaderMessages", loaderMessages);
        }
    }

    #endregion

    class Reflector : IDisposable
    {
        #region Static Members

        private static Reflector currentReflector;

        public static Reflector CurrentReflector
        {
            get
            { return currentReflector; }

            set
            {
                if (currentReflector != null)
                {
                    currentReflector.Dispose();
                }
                currentReflector = value;
            }
        }

        #endregion

        #region Fields and Properties

        private AppDomain reflectingDomain;
        private string assemblyPath;
        private RemoteReflector remoteReflector;

        public string AssemblyPath
        {
            get
            { return assemblyPath; }
        }

        #endregion

        #region Construction

        static Reflector()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveHandler);
        }

        public Reflector(string assemblyPath)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            this.assemblyPath = assemblyPath;
            this.reflectingDomain = AppDomain.CreateDomain(assemblyPath + " reflecting domain", null, setup);

            try
            {
                object remote_object = this.reflectingDomain.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName,
                    typeof(RemoteReflector).FullName,
                    false,
                    BindingFlags.Default,
                    null,
                    new object[] { assemblyPath },
                    null,
                    null,
                    null);

                this.remoteReflector = (RemoteReflector)remote_object;
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        private static Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
        {
            // work around the problem with VS loading the add-ins to the LoadFrom context
            if (args.Name == Assembly.GetExecutingAssembly().FullName) return Assembly.GetExecutingAssembly();
            if (args.Name == typeof(NativeType).Assembly.FullName) return typeof(NativeType).Assembly;
            return null;
        }

        #endregion

        #region GetInteropTypesAndMethods

        public void GetInteropTypesAndMethods(out List<TypeDescriptor> typeDescs, out List<MethodDescriptor> methodDescs)
        {
            remoteReflector.GetInteropTypesAndMethods(out typeDescs, out methodDescs);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            AppDomain.Unload(reflectingDomain);
        }

        #endregion
    }

    class RemoteReflector : MarshalByRefObject
    {
        #region Fields

        private const BindingFlags bindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private Assembly assembly;
        private Dictionary<Guid, MethodInfo> methodMap;
        private Dictionary<Guid, Type> typeMap;

        #endregion

        #region Construction

        public RemoteReflector(string assemblyPath)
        {
            Debug.Assert(!AppDomain.CurrentDomain.IsDefaultAppDomain());

            this.assembly = Assembly.LoadFrom(assemblyPath);

            this.methodMap = new Dictionary<Guid, MethodInfo>();
            this.typeMap = new Dictionary<Guid, Type>();
        }

        public override object InitializeLifetimeService()
        {
            // prevent this MBRO from expiring
            return null;
        }

        #endregion

        #region GetInteropTypesAndMethods, GetNativeSignature

        /// <summary>
        /// Returns a list of interop (P/Invoke & RCW) method descriptors and list of delegates.
        /// </summary>
        public void GetInteropTypesAndMethods(out List<TypeDescriptor> typeDescs, out List<MethodDescriptor> methodDescs)
        {
            methodDescs = new List<MethodDescriptor>();
            typeDescs = new List<TypeDescriptor>();

            // let's keep track of the types for which we have descriptors
            Dictionary<Type, TypeDescriptor> map = new Dictionary<Type, TypeDescriptor>();

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                // ReflectionTypeLoadException changes to FileNotFoundException when crossing the appdomain boundary
                // because it contain Type references to the target assembly -> convert to pure strings now
                throw new UnableToGetTypesException(e);
            }

            foreach (Type type in types)
            {
                TypeDescriptor type_desc = null;

                if (typeof(Delegate).IsAssignableFrom(type))
                {
                    type_desc = GetDescriptorForType(map, type);
                    typeDescs.Add(type_desc);
                }
                else
                {
                    foreach (MethodInfo info in type.GetMethods(bindingFlags))
                    {
                        if (NativeSignature.IsPInvoke(info) || NativeSignature.IsRCWMethod(info))
                        {
                            if (type_desc == null)
                            {
                                type_desc = GetDescriptorForType(map, type);
                            }

                            Guid id = Guid.NewGuid();
                            MethodDescriptor method_desc = new MethodDescriptor(this, info, type_desc, id);

                            methodDescs.Add(method_desc);
                            methodMap.Add(id, info);
                        }
                    }
                }
            }
        }

        private TypeDescriptor GetDescriptorForType(Dictionary<Type, TypeDescriptor> map, Type type)
        {
            TypeDescriptor type_desc;
            if (!map.TryGetValue(type, out type_desc))
            {
                Guid id = Guid.NewGuid();

                if (type.IsNested)
                {
                    TypeDescriptor declaring_type_desc = GetDescriptorForType(map, type.DeclaringType);
                    type_desc = new TypeDescriptor(this, declaring_type_desc, type, id);
                }
                else
                {
                    type_desc = new TypeDescriptor(this, type, id);
                }

                typeMap.Add(id, type);
            }
            return type_desc;
        }

        public NativeSignature GetNativeSignature(Guid id, bool ansiPlatform, bool platform64bit)
        {
            MethodInfo info;
            if (methodMap.TryGetValue(id, out info))
            {
                // requesting signature for a method
                if (NativeSignature.IsPInvoke(info))
                {
                    return NativeSignature.FromPInvokeSignature(info, ansiPlatform, platform64bit);
                }
                else
                {
                    Debug.Assert(NativeSignature.IsRCWMethod(info));
                    return NativeSignature.FromComInteropSignature(info, ansiPlatform, platform64bit);
                }
            }
            else
            {
                // requesting signature for a delegate
                Type type = typeMap[id];

                Debug.Assert(typeof(Delegate).IsAssignableFrom(type));
                return NativeSignature.FromDelegateType(type, ansiPlatform, platform64bit);
            }
        }

        #endregion
    }

    [Serializable]
    class ItemDescriptor
    {
        #region Fields and Properties

        protected readonly RemoteReflector reflector;
        protected readonly Guid id;

        private NativeSignature signature;
        private bool signatureAnsi;
        private bool signature64bit;

        #endregion

        #region Construction

        public ItemDescriptor(RemoteReflector reflector, Guid id)
        {
            this.reflector = reflector;
            this.id = id;
        }

        #endregion

        #region GetNativeSignature

        public NativeSignature GetNativeSignature(bool ansiPlatform, bool platform64bit)
        {
            if (signature == null ||
                signatureAnsi != ansiPlatform ||
                signature64bit != platform64bit)
            {
                // cache the signature
                signature = reflector.GetNativeSignature(id, ansiPlatform, platform64bit);
                signatureAnsi = ansiPlatform;
                signature64bit = platform64bit;
            }
            return signature;
        }

        #endregion
    }

    /// <summary>
    /// Represents one type that declares interop methods.
    /// </summary>
    /// <remarks>Passed accross domain boundary by value.</remarks>
    [Serializable]
    class TypeDescriptor : ItemDescriptor
    {
        #region Fields

        public readonly string TypeName;
        public readonly bool IsValueType;
        public readonly bool IsDelegate;
        public readonly TypeAttributes TypeAttributes;

        public readonly TypeDescriptor DeclaringType; // non-null for nested types only

        #endregion

        #region Construction

        public TypeDescriptor(RemoteReflector reflector, TypeDescriptor declaringType, Type type, Guid id)
            : base(reflector, id)
        {
            this.DeclaringType = declaringType;
            this.TypeName = type.FullName;
            this.IsValueType = type.IsValueType;
            this.IsDelegate = typeof(Delegate).IsAssignableFrom(type);
            this.TypeAttributes = type.Attributes;
        }

        public TypeDescriptor(RemoteReflector reflector, Type type, Guid id)
            : this(reflector, null, type, id)
        { }

        #endregion
    }

    /// <summary>
    /// Represents one interop method.
    /// </summary>
    /// <remarks>Passed accross domain boundary by value. Native signature of the method is retrieved lazily.</remarks>
    [Serializable]
    class MethodDescriptor : ItemDescriptor
    {
        #region Fields and Properties

        public readonly string MethodName;
        public readonly string MethodSigString; // return type : (parameter types)
        public readonly MethodAttributes MethodAttributes;

        public readonly TypeDescriptor DeclaringType;

        #endregion

        #region Construction

        public MethodDescriptor(RemoteReflector reflector, MethodInfo info, TypeDescriptor declaringType, Guid id)
            : base(reflector, id)
        {
            this.DeclaringType = declaringType;
            this.MethodName = info.Name;
            this.MethodAttributes = info.Attributes;

            // signature is null - will be created when first needed

            StringBuilder sb = new StringBuilder();
            
            TypeToString(sb, info.ReturnType);

            sb.Append('(');
            ParameterInfo[] parameters = info.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0) sb.Append(',');
                TypeToString(sb, parameters[i].ParameterType);
            }
            sb.Append(')');

            this.MethodSigString = sb.ToString();
        }

        private static void TypeToString(StringBuilder sb, Type type)
        {
            bool byref = false;
            int ptr_count = 0;

            if (type.IsByRef)
            {
                byref = true;
                type = type.GetElementType();
            }

            while (type.IsPointer)
            {
                ptr_count++;
                type = type.GetElementType();
            }

            if ((Type.GetTypeCode(type) != TypeCode.Object && !type.IsEnum) ||
                type == typeof(void) ||
                type == typeof(object))
            {
                sb.Append(type.Name.ToLower());
            }
            else if (type == typeof(IntPtr))
            {
                sb.Append("native int");
            }
            else if (type == typeof(UIntPtr))
            {
                sb.Append("native uint");
            }
            else
            {
                sb.Append(type.FullName);
            }

            // add indirections
            while (ptr_count-- > 0) sb.Append('*');
            if (byref) sb.Append('&');
        }

        #endregion
    }
}
