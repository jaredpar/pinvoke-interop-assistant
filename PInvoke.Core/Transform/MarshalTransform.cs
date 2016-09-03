// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.CodeDom;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CodeParam = System.CodeDom.CodeParameterDeclarationExpression;
using CodeParamPair = System.Collections.Generic.KeyValuePair<System.CodeDom.CodeParameterDeclarationExpression, System.CodeDom.CodeParameterDeclarationExpression>;

namespace PInvoke.Transform
{

    /// <summary>
    /// Used to make several marshalling decisions about.  
    /// 
    /// Make sure to resolve typedefs and namedtypes before calling this function.  This
    /// code will not attempt to descend typedefs and named types  
    /// </summary>
    /// <remarks></remarks>

    public class MarshalTransform
    {
        private TransformKindFlags _kind;
        private CodeTransform _trans;

        private List<TransformPlugin> _list = new List<TransformPlugin>();
        internal TransformKindFlags Kind
        {
            get { return _kind; }
            set { _kind = value; }
        }

        public MarshalTransform(LanguageType lang, NativeSymbolBag bag, TransformKindFlags kind)
        {
            _trans = new CodeTransform(lang, bag);
            Kind = kind;

            // Method Parameters
            _list.Add(new BooleanTypesTransformPlugin());

            // Process BSTR types before any other string.  BSTR can techinally be used as other String types
            // such as LPWSTR and the other string matching code will flag them as such.  Therefore we will 
            // process them first since the reverse is not true
            _list.Add(new BstrTransformPlugin());
            _list.Add(new MutableStringBufferTransformPlugin());
            _list.Add(new ConstantStringTransformPlugin());
            _list.Add(new ArrayParameterTransformPlugin(_trans));
            _list.Add(new BetterManagedTypesTransformPlugin());
            _list.Add(new PointerToKnownTypeTransformPlugin(_trans));
            _list.Add(new SystemIntTransformPlugin());
            _list.Add(new RawStringTransformPlugin());

            // Very low on the list as it's a last ditch effort
            _list.Add(new DoublePointerOutTransformPlugin());
            _list.Add(new PointerPointerTransformPlugin());
            _list.Add(new DirectionalModifiersTransformPlugin());

            // Struct Member
            _list.Add(new StringBufferStructMemberTransformPlugin());
            _list.Add(new StringPointerStructMemberTransformPlugin());
            _list.Add(new BoolStructMemberTransformPlugin());

            // Union Members
            _list.Add(new BoolUnionMemberTransformPlugin());

            // Mainly wrapper generators
            _list.Add(new OneWayStringBufferTransformPlugin());
            _list.Add(new TwoWayStringBufferTransformPlugin());
            _list.Add(new TwoWayViaReturnStringBufferTransformPlugin());
            _list.Add(new PInvokePointerTransformPlugin());

            foreach (TransformPlugin cur in _list)
            {
                cur.LanguageType = lang;
            }
        }

        /// <summary>
        /// Run all of the marshaling hueristiscs on the type and it's members
        /// </summary>
        /// <param name="ctd"></param>
        /// <remarks></remarks>

        public void Process(CodeTypeDeclaration ctd)
        {
            // First check and see if it is a delegate type, if so run the delegate hueristics
            CodeTypeDelegate ctdDel = ctd as CodeTypeDelegate;
            if (ctdDel != null && ctdDel.UserData.Contains(TransformConstants.DefinedType))
            {
                ProcessDelegate(ctdDel);
                return;
            }

            // Now run the hueristics over the actual members of the type
            if (ctd.UserData.Contains(TransformConstants.DefinedType))
            {
                NativeDefinedType nt = ctd.UserData[TransformConstants.DefinedType] as NativeDefinedType;
                if (nt != null)
                {
                    switch (nt.Kind)
                    {
                        case NativeSymbolKind.StructType:
                            ProcessStruct(ctd);
                            break;
                        case NativeSymbolKind.UnionType:
                            RunPluginUnionMembers(ctd);
                            break;
                        case NativeSymbolKind.EnumType:
                            RunPluginEnumMembers(ctd);
                            break;
                    }
                }
            }

            // Now process the methods on the type.  First step is to convert all of them into 
            // best PInvoke signature.  Then create wrapper methods for them
            CodeTypeMemberCollection col = new CodeTypeMemberCollection(ctd.Members);
            List<CodeMemberMethod> list = new List<CodeMemberMethod>();

            foreach (CodeTypeMember mem in col)
            {
                // Look at procedures
                CodeMemberMethod codeProc = mem as CodeMemberMethod;
                if (codeProc != null && codeProc.UserData.Contains(TransformConstants.Procedure))
                {
                    list.Add(codeProc);
                }
            }

            foreach (CodeMemberMethod codeProc in list)
            {
                ProcessParameters(codeProc);
                ProcessReturnType(codeProc);
            }

            foreach (CodeMemberMethod codeProc in list)
            {
                ProcessWrapperMethods(ctd, codeProc);
            }
        }

        private void ProcessDelegate(CodeTypeDelegate del)
        {
            foreach (TransformPlugin plugin in _list)
            {
                if (0 != (plugin.TransformKind & TransformKindFlags.Signature))
                {
                    plugin.ProcessParameters(del);
                }
            }
        }

        private void ProcessStruct(CodeTypeDeclaration ctd)
        {
            ProcessStructMembers(ctd, TransformKindFlags.StructMembers);
        }

        private void RunPluginUnionMembers(CodeTypeDeclaration ctd)
        {
            if (TransformKindFlags.UnionMembers != (_kind & TransformKindFlags.UnionMembers))
            {
                return;
            }

            // Union fields are not processed as often.  After the initial conversion all union fields are
            // left in the raw form of IntPtr, Int and such.  Essentially all value types.  It's possible
            // to create an alignment issue if we try and refactor these out to better types.  For instance
            // we could create a string for an IntPtr and create an alignment issue
            foreach (TransformPlugin plugin in _list)
            {
                if (0 != (plugin.TransformKind & TransformKindFlags.UnionMembers))
                {
                    plugin.ProcessUnionMembers(ctd);
                }
            }
        }

        private void RunPluginEnumMembers(CodeTypeDeclaration ctd)
        {
            if (TransformKindFlags.EnumMembers != (_kind & TransformKindFlags.EnumMembers))
            {
                return;
            }

            // Enum fields are not processed
        }

        private void ProcessParameters(CodeMemberMethod codeProc)
        {
            if (TransformKindFlags.Signature != (_kind & TransformKindFlags.Signature))
            {
                return;
            }

            foreach (TransformPlugin plugin in _list)
            {
                if (0 != (plugin.TransformKind & TransformKindFlags.Signature))
                {
                    plugin.ProcessParameters(codeProc);
                }
            }
        }

        private void ProcessReturnType(CodeMemberMethod codeMethod)
        {
            if (TransformKindFlags.Signature != (_kind & TransformKindFlags.Signature))
            {
                return;
            }

            foreach (TransformPlugin plugin in _list)
            {
                if (0 != (plugin.TransformKind & TransformKindFlags.Signature))
                {
                    plugin.ProcessReturnType(codeMethod);
                }
            }
        }

        private void ProcessStructMembers(CodeTypeDeclaration ctd, TransformKindFlags kind)
        {
            if (TransformKindFlags.StructMembers != (_kind & TransformKindFlags.StructMembers))
            {
                return;
            }

            foreach (TransformPlugin plugin in _list)
            {
                if (0 != (plugin.TransformKind & kind))
                {
                    plugin.ProcessStructMembers(ctd);
                }
            }
        }

        private void ProcessWrapperMethods(CodeTypeDeclaration ctd, CodeMemberMethod codeMethod)
        {
            if (TransformKindFlags.WrapperMethods != (_kind & TransformKindFlags.WrapperMethods))
            {
                return;
            }

            List<CodeMemberMethod> list = new List<CodeMemberMethod>();
            foreach (TransformPlugin plugin in _list)
            {
                if (0 != (plugin.TransformKind & TransformKindFlags.WrapperMethods))
                {
                    list.AddRange(plugin.ProcessWrapperMethods(codeMethod));
                }
            }

            ctd.Members.AddRange(list.ToArray());
        }

    }

}
