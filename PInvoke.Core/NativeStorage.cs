
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Threading;
using System.Text;
using Microsoft.Win32;
using static PInvoke.Contract;
using PInvoke;
using System.IO;

namespace PInvoke
{
    public partial class NativeStorage
    {
        [DebuggerDisplay("Id={Id} Kind={Kind}")]
        public class TypeReference
        {
            public int Id;

            public NativeSymbolKind Kind;
            public TypeReference(int id, NativeSymbolKind kind)
            {
                this.Id = id;
                this.Kind = kind;
            }
        }

        [ThreadStatic()]

        private static NativeStorage t_default;
        /// <summary>
        /// Default Instance to use if not explicitly given one
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static NativeStorage DefaultInstance
        {
            get
            {
                if (t_default == null)
                {
                    t_default = new NativeStorage();
                }
                return t_default;
            }
            set { t_default = value; }
        }

        public partial class DefinedTypeDataTable
        {
            private void DefinedTypeDataTable_ColumnChanging(System.Object sender, System.Data.DataColumnChangeEventArgs e)
            {
                if ((e.Column.ColumnName == this.IdColumn.ColumnName))
                {
                    //Add user code here
                }

            }
        }

        #region "DefinedType Table"

        public partial class DefinedTypeDataTable
        {


            private Dictionary<string, DefinedTypeRow> _cacheMap;
            public bool CacheLookup
            {
                get { return _cacheMap != null; }
                set
                {
                    if (value)
                    {

                        if (_cacheMap == null)
                        {
                            _cacheMap = new Dictionary<string, DefinedTypeRow>(StringComparer.Ordinal);
                            foreach (DefinedTypeRow row in this.Rows)
                            {
                                _cacheMap[row.Name] = row;
                            }
                        }
                    }
                    else
                    {
                        _cacheMap = null;
                    }
                }
            }


            public DefinedTypeRow Add(NativeSymbolKind kind, string name)
            {
                DefinedTypeRow row = this.NewDefinedTypeRow();
                row.Kind = kind;
                row.Name = name;
                this.AddDefinedTypeRow(row);

                if (_cacheMap != null)
                {
                    _cacheMap[name] = row;
                }

                return row;
            }

            /// <summary>
            /// Try and find a defined type by it's name
            /// </summary>
            /// <param name="name"></param>
            /// <param name="drow"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public bool TryFindByName(string name, out DefinedTypeRow dRow)
            {

                // Use the map if we are caching lookups
                if (_cacheMap != null)
                {
                    return _cacheMap.TryGetValue(name, out dRow);
                }

                dRow = null;
                DataRow[] rows = this.Select(string.Format("{0}='{1}'", NameColumn.ColumnName, name));
                if (rows.Length == 0)
                {
                    return false;
                }

                dRow = (DefinedTypeRow)rows[0];
                return true;
            }

            public bool TryFindById(int id, out DefinedTypeRow drow)
            {
                DataRow[] rows = this.Select(string.Format("{0}={1}", IdColumn.ColumnName, id));
                if (rows.Length == 0)
                {
                    drow = null;
                    return false;
                }

                drow = (DefinedTypeRow)rows[0];
                return true;
            }

            public List<DefinedTypeRow> FindByNamePattern(string pattern)
            {
                List<DefinedTypeRow> list = new List<DefinedTypeRow>();
                string filter = string.Format("{0} LIKE '{1}'", NameColumn.ColumnName, pattern);
                DataRow[] rows = this.Select(filter);
                foreach (DefinedTypeRow dtRow in rows)
                {
                    list.Add(dtRow);
                }

                return list;
            }

        }

        public partial class DefinedTypeRow
        {
            public NativeSymbolKind Kind
            {
                get { return (NativeSymbolKind)this.KindRaw; }
                set { KindRaw = Convert.ToInt32(value); }
            }

            public NativeCallingConvention CallingConvention
            {
                get { return (NativeCallingConvention)this.ConventionRaw; }
                set { this.ConventionRaw = (Int32)value; }
            }
        }

        #endregion

        #region "Member Table"
        public partial class MemberDataTable
        {

            internal MemberRow Add(DefinedTypeRow dtRow, string name, TypeReference typeRef)
            {
                MemberRow row = this.NewMemberRow();
                row.DefinedTypeRow = dtRow;
                row.Name = name;
                row.TypeId = typeRef.Id;
                row.TypeKind = typeRef.Kind;
                this.AddMemberRow(row);

                return row;
            }

            public bool TryFindById(int id, out List<MemberRow> erows)
            {

                DataRow[] rows = this.Select(string.Format("{0}={1}", this.DefinedTypeIdColumn.ColumnName, id));
                if (rows.Length == 0)
                {
                    erows = null;
                    return false;
                }

                erows = new List<MemberRow>();
                foreach (DataRow row in rows)
                {
                    erows.Add((MemberRow)row);
                }
                return true;
            }
        }

        public partial class MemberRow
        {
            public NativeSymbolKind TypeKind
            {
                get { return (NativeSymbolKind)TypeKindRaw; }
                set { TypeKindRaw = Convert.ToInt32(value); }
            }
        }

        #endregion

        #region "EnumValue Table"

        public partial class EnumValueDataTable
        {

            public EnumValueRow Add(DefinedTypeRow dtRow, string name, string value)
            {
                EnumValueRow row = this.NewEnumValueRow();
                row.DefinedTypeRow = dtRow;
                row.Name = name;
                row.Value = value;
                this.AddEnumValueRow(row);
                return row;
            }

            public bool TryFindById(int id, out List<EnumValueRow> erows)
            {

                DataRow[] rows = this.Select(string.Format("{0}={1}", DefinedTypeIdColumn.ColumnName, id));
                if (rows.Length == 0)
                {
                    erows = null;
                    return false;
                }

                erows = new List<EnumValueRow>();
                foreach (DataRow row in rows)
                {
                    erows.Add((EnumValueRow)row);
                }
                return true;
            }

            public bool TryFindByValueName(string valName, out List<EnumValueRow> erows)
            {
                DataRow[] rows = this.Select(string.Format("{0}='{1}'", NameColumn.ColumnName, valName));
                if (rows.Length == 0)
                {
                    erows = null;
                    return false;
                }

                erows = new List<EnumValueRow>();
                foreach (DataRow row in rows)
                {
                    erows.Add((EnumValueRow)row);
                }
                return true;
            }

        }

        #endregion

        #region "TypedefType Table"

        public partial class TypedefTypeDataTable
        {


            private Dictionary<string, TypedefTypeRow> _cacheMap;
            public bool CacheLookup
            {
                get { return _cacheMap != null; }
                set
                {
                    if (value)
                    {
                        if (_cacheMap != null)
                        {
                            _cacheMap = new Dictionary<string, TypedefTypeRow>(StringComparer.Ordinal);
                            foreach (TypedefTypeRow cur in Rows)
                            {
                                _cacheMap[cur.Name] = cur;
                            }
                        }
                    }
                    else
                    {
                        _cacheMap = null;
                    }
                }
            }

            public TypedefTypeRow Add(string name, TypeReference typeRef)
            {
                TypedefTypeRow row = this.NewTypedefTypeRow();
                row.Name = name;
                row.RealTypeId = typeRef.Id;
                row.RealTypeKind = typeRef.Kind;
                this.AddTypedefTypeRow(row);

                if (_cacheMap != null)
                {
                    _cacheMap[name] = row;
                }

                return row;
            }

            public bool TryFindByName(string name, out TypedefTypeRow row)
            {
                if (_cacheMap != null)
                {
                    return _cacheMap.TryGetValue(name, out row);
                }

                string filter = string.Format("{0}='{1}'", NameColumn.ColumnName, name);
                DataRow[] rows = this.Select(filter);
                if (rows.Length == 0)
                {
                    row = null;
                    return false;
                }

                row = (TypedefTypeRow)rows[0];
                return true;
            }

            public List<TypedefTypeRow> FindByNamePattern(string pattern)
            {
                List<TypedefTypeRow> list = new List<TypedefTypeRow>();
                string filter = string.Format("{0} LIKE '{1}'", NameColumn.ColumnName, pattern);
                DataRow[] rows = this.Select(filter);
                foreach (TypedefTypeRow dtRow in rows)
                {
                    list.Add(dtRow);
                }

                return list;
            }

            public bool TryFindById(int id, out TypedefTypeRow drow)
            {
                DataRow[] rows = this.Select(string.Format("{0}={1}", IdColumn.ColumnName, id));
                if (rows.Length == 0)
                {
                    drow = null;
                    return false;
                }

                drow = (TypedefTypeRow)rows[0];
                return true;
            }

            public List<TypedefTypeRow> FindByTarget(TypeReference typeRef)
            {
                List<TypedefTypeRow> list = new List<TypedefTypeRow>();
                string filter = string.Format("{0}={1} AND {2}={3}", RealTypeIdColumn.ColumnName, typeRef.Id, RealTypeKindRawColumn, Convert.ToInt32(typeRef.Kind));
                foreach (TypedefTypeRow trow in this.Select(filter))
                {
                    list.Add(trow);
                }

                return list;
            }
        }

        public partial class TypedefTypeRow
        {
            public NativeSymbolKind RealTypeKind
            {
                get { return (NativeSymbolKind)RealTypeKindRaw; }
                set { RealTypeKindRaw = Convert.ToInt32(value); }
            }
        }
        #endregion

        #region "NamedType Table"

        public partial class NamedTypeDataTable
        {

            private Dictionary<string, NamedTypeRow> _cacheMap;
            private static string CreateMoniker(string qual, string name, bool isConst)
            {
                return qual + "#" + name + "#" + isConst.ToString();
            }

            public bool CacheLookup
            {
                get { return _cacheMap != null; }
                set
                {
                    if (value)
                    {
                        if (_cacheMap == null)
                        {
                            _cacheMap = new Dictionary<string, NamedTypeRow>();
                            foreach (NamedTypeRow row in Rows)
                            {
                                _cacheMap.Add(CreateMoniker(row.Qualification, row.Name, row.IsConst), row);
                            }
                        }
                    }
                    else
                    {
                        _cacheMap = null;
                    }
                }
            }


            public NamedTypeRow Add(string qual, string name, bool isConst)
            {

                NamedTypeRow row = this.NewNamedTypeRow();
                row.Name = name;
                row.Qualification = qual;
                row.IsConst = isConst;
                this.AddNamedTypeRow(row);

                if (_cacheMap != null)
                {
                    _cacheMap.Add(CreateMoniker(qual, name, isConst), row);
                }

                return row;
            }

            public bool TryFindByName(string qual, string name, bool isConst, out NamedTypeRow row)
            {
                row = null;

                if (_cacheMap != null)
                {
                    string moniker = CreateMoniker(qual, name, isConst);
                    return _cacheMap.TryGetValue(moniker, out row);
                }

                DataRow[] rows = null;
                string filter = string.Format("{0}='{1}' And {2}='{3}' AND {4}={5}", NameColumn.ColumnName, name, QualificationColumn.ColumnName, qual, IsConstColumn.ColumnName, isConst);
                rows = this.Select(filter);

                if (rows.Length == 0)
                {
                    return false;
                }

                row = (NamedTypeRow)rows[0];
                return true;
            }

            public bool TryFindById(int id, out NamedTypeRow drow)
            {
                DataRow[] rows = this.Select(string.Format("{0}={1}", IdColumn.ColumnName, id));
                if (rows.Length == 0)
                {
                    drow = null;
                    return false;
                }

                drow = (NamedTypeRow)rows[0];
                return true;
            }
        }

        #endregion

        #region "PointerType Table"

        public partial class PointerTypeDataTable
        {

            public PointerTypeRow Add(TypeReference typeRef)
            {
                ThrowIfNull(typeRef);

                PointerTypeRow row = this.NewPointerTypeRow();
                row.RealTypeId = typeRef.Id;
                row.RealTypeKind = typeRef.Kind;
                this.AddPointerTypeRow(row);
                return row;
            }

            public bool TryFindById(int id, out PointerTypeRow row)
            {
                string filter = string.Format("{0}={1}", IdColumn.ColumnName, id);
                DataRow[] rows = this.Select(filter);
                if (rows.Length == 0)
                {
                    row = null;
                    return false;
                }

                row = (PointerTypeRow)rows[0];
                return true;
            }

            public bool TryFindByTarget(TypeReference typeRef, out PointerTypeRow row)
            {
                string filter = string.Format("{0}={1} AND {2}={3}", RealTypeIdColumn.ColumnName, typeRef.Id, RealTypeKindRawColumn.ColumnName, Convert.ToInt32(typeRef.Kind));
                DataRow[] rows = this.Select(filter);
                if (rows.Length == 0)
                {
                    row = null;
                    return false;
                }

                row = (PointerTypeRow)rows[0];
                return true;
            }

        }

        public partial class PointerTypeRow
        {
            public NativeSymbolKind RealTypeKind
            {
                get { return (NativeSymbolKind)RealTypeKindRaw; }
                set { RealTypeKindRaw = Convert.ToInt32(value); }
            }
        }

        #endregion

        #region "ArrayType Table"

        public partial class ArrayTypeDataTable
        {

            public ArrayTypeRow Add(int count, TypeReference typeRef)
            {
                ThrowIfNull(typeRef);

                ArrayTypeRow row = this.NewArrayTypeRow();
                row.RealTypeId = typeRef.Id;
                row.RealTypeKind = typeRef.Kind;
                row.ElementCountt = count;
                this.AddArrayTypeRow(row);
                return row;
            }

            public bool TryFindById(int id, out ArrayTypeRow row)
            {
                string filter = string.Format("{0}={1}", IdColumn.ColumnName, id);
                DataRow[] rows = this.Select(filter);
                if (rows.Length == 0)
                {
                    row = null;
                    return false;
                }

                row = (ArrayTypeRow)rows[0];
                return true;
            }

            public bool TryFindByTarget(TypeReference typeRef, out ArrayTypeRow row)
            {
                string filter = string.Format("{0}={1} AND {2}={3}", RealTypeIdColumn.ColumnName, typeRef.Id, RealTypeKindRawColumn.ColumnName, Convert.ToInt32(typeRef.Kind));
                DataRow[] rows = this.Select(filter);
                if (rows.Length == 0)
                {
                    row = null;
                    return false;
                }

                row = (ArrayTypeRow)rows[0];
                return true;
            }
        }

        public partial class ArrayTypeRow
        {
            public NativeSymbolKind RealTypeKind
            {
                get { return (NativeSymbolKind)RealTypeKindRaw; }
                set { RealTypeKindRaw = Convert.ToInt32(value); }
            }
        }

        #endregion

        #region "Specialized Table"
        public partial class SpecializedTypeDataTable
        {

            public bool TryFindById(int id, out SpecializedTypeRow srow)
            {
                srow = null;
                DataRow[] rows = this.Select(string.Format("{0}={1}", this.IdColumn.ColumnName, id));
                if (rows.Length == 0)
                {
                    return false;
                }
                else
                {
                    srow = (SpecializedTypeRow)rows[0];
                    return true;
                }
            }

            public bool TryFindBuiltin(BuiltinType bt, bool isUnsigned, out SpecializedTypeRow srow)
            {
                DataRow[] rows = this.Select(string.Format("{0}={1} AND {2}={3}", this.BuiltinTypeRawColumn, Convert.ToInt32(bt), this.IsUnsignedColumn.ColumnName, isUnsigned));
                if (rows.Length == 0)
                {
                    srow = null;
                    return false;
                }
                else
                {
                    srow = (SpecializedTypeRow)rows[0];
                    return true;
                }
            }

            public bool TryFindBitVector(int size, out SpecializedTypeRow srow)
            {
                DataRow[] rows = this.Select(string.Format("{0}={1}", this.BitVectorSizeColumn, size));
                if (rows.Length == 0)
                {
                    srow = null;
                    return false;
                }
                else
                {
                    srow = (SpecializedTypeRow)rows[0];
                    return true;
                }
            }
        }

        public partial class SpecializedTypeRow
        {
            public NativeSymbolKind Kind
            {
                get { return (NativeSymbolKind)KindRaw; }
                set { KindRaw = Convert.ToInt32(value); }
            }

            public BuiltinType BuiltinType
            {
                get { return (BuiltinType)BuiltinTypeRaw; }
                set { BuiltinTypeRaw = Convert.ToInt32(value); }
            }
        }

        #endregion

        #region "Constants Table"

        public partial class ConstantDataTable
        {

            public List<ConstantRow> FindByNamePattern(string pattern)
            {
                List<ConstantRow> list = new List<ConstantRow>();
                string filter = string.Format("{0} LIKE '{1}'", NameColumn.ColumnName, pattern);
                DataRow[] rows = this.Select(filter);
                foreach (ConstantRow dtRow in rows)
                {
                    list.Add(dtRow);
                }

                return list;
            }

            public bool TryFindByName(string name, out ConstantRow row)
            {
                List<ConstantRow> list = new List<ConstantRow>();
                string filter = string.Format("{0}='{1}'", NameColumn.ColumnName, name);
                DataRow[] rows = this.Select(filter);
                if (rows.Length == 0)
                {
                    row = null;
                    return false;
                }

                row = (ConstantRow)rows[0];
                return true;
            }

            /// <summary>
            /// Convert all of the stored constants back into Macro instances
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            internal List<PInvoke.Parser.Macro> LoadAllMacros()
            {
                List<PInvoke.Parser.Macro> list = new List<PInvoke.Parser.Macro>();
                foreach (ConstantRow nRow in this.Rows)
                {
                    switch (nRow.Kind)
                    {
                        case ConstantKind.MacroMethod:
                            PInvoke.Parser.MethodMacro method = null;
                            if (PInvoke.Parser.MethodMacro.TryCreateFromDeclaration(nRow.Name, nRow.Value, out method))
                            {
                                list.Add(method);
                            }
                            break;
                        case ConstantKind.Macro:
                            list.Add(new PInvoke.Parser.Macro(nRow.Name, nRow.Value));
                            break;
                        default:
                            ThrowInvalidEnumValue(nRow.Kind);
                            break;
                    }
                }

                return list;
            }

        }

        public partial class ConstantRow
        {
            public ConstantKind Kind
            {
                get { return (ConstantKind)KindRaw; }
                set { KindRaw = (Int32)value; }
            }
        }

        #endregion

        #region "Procedure Table"
        public partial class ProcedureDataTable
        {

            public ProcedureRow Add(string name, string dllName, NativeCallingConvention conv, int sigId)
            {
                ProcedureRow row = this.NewProcedureRow();
                row.Name = name;
                row.DllName = dllName;
                row.CallingConvention = conv;
                row.SignatureId = sigId;
                this.AddProcedureRow(row);
                return row;
            }

            public bool TryLoadByName(string name, out ProcedureRow procRow)
            {
                DataRow[] rows = this.Select(string.Format("{0}='{1}'", this.NameColumn.ColumnName, name));
                if (rows.Length == 0)
                {
                    procRow = null;
                    return false;
                }
                else
                {
                    procRow = (ProcedureRow)rows[0];
                    return true;
                }

            }

            public List<ProcedureRow> FindByNamePattern(string pattern)
            {
                List<ProcedureRow> list = new List<ProcedureRow>();
                string filter = string.Format("{0} LIKE '{1}'", NameColumn.ColumnName, pattern);
                DataRow[] rows = this.Select(filter);
                foreach (ProcedureRow dtRow in rows)
                {
                    list.Add(dtRow);
                }

                return list;
            }

        }

        public partial class ProcedureRow
        {
            public NativeCallingConvention CallingConvention
            {
                get { return (NativeCallingConvention)ConventionRaw; }
                set { ConventionRaw = (Int32)value; }
            }
        }
        #endregion

        #region "Signature Table"
        public partial class SignatureDataTable
        {

            public bool TryLoadById(Int32 id, out SignatureRow sigRow)
            {
                DataRow[] rows = this.Select(string.Format("{0}={1}", this.IdColumn.ColumnName, id));
                if (rows.Length == 0)
                {
                    sigRow = null;
                    return false;
                }

                sigRow = (SignatureRow)rows[0];
                return true;
            }
        }

        public partial class SignatureRow
        {
            public NativeSymbolKind ReturnTypeKind
            {
                get { return (NativeSymbolKind)ReturnTypeKindRaw; }
                set { ReturnTypeKindRaw = Convert.ToInt32(value); }
            }
        }
        #endregion

        #region "Parameter Table"
        public partial class ParameterDataTable
        {
            public ParameterRow Add(SignatureRow sig, string name, TypeReference typeRef, string salId)
            {
                ParameterRow row = this.NewParameterRow();
                row.Name = name;
                row.SignatureId = sig.Id;
                row.TypeId = typeRef.Id;
                row.TypeKind = typeRef.Kind;
                row.SalId = salId;
                this.AddParameterRow(row);
                return row;
            }
        }

        public partial class ParameterRow
        {
            public NativeSymbolKind TypeKind
            {
                get { return (NativeSymbolKind)TypeKindRaw; }
                set { TypeKindRaw = Convert.ToInt32(value); }
            }
        }
        #endregion

        #region "SalEntry Table"

        public partial class SalEntryDataTable
        {

            public SalEntryRow Add(SalEntryType type, string text)
            {
                SalEntryRow row = this.NewSalEntryRow();
                row.Type = type;
                row.Text = text;
                this.AddSalEntryRow(row);
                return row;
            }

            public bool TryFindNoText(SalEntryType type, out SalEntryRow row)
            {
                row = null;

                string filter = string.Format("{0}={1}", TypeRawColumn.ColumnName, Convert.ToInt32(type));
                DataRow[] rows = this.Select(filter);
                foreach (SalEntryRow cur in rows)
                {
                    if (cur.IsTextNull())
                    {
                        row = cur;
                        return true;
                    }
                }

                return false;
            }

            public bool TryFind(SalEntryType type, string text, out SalEntryRow row)
            {
                string filter = string.Format("{0}={1} AND {2}='{3}'", TypeRawColumn.ColumnName, Convert.ToInt32(type), TextColumn.ColumnName, text);
                DataRow[] rows = this.Select(filter);
                if (rows.Length == 0)
                {
                    row = null;
                    return false;
                }

                row = (SalEntryRow)rows[0];
                return true;
            }

            public bool TryFindById(int id, out SalEntryRow row)
            {
                string filter = string.Format("{0}={1}", IdColumn.ColumnName, id);
                DataRow[] rows = this.Select(filter);
                if (rows.Length == 0)
                {
                    row = null;
                    return false;
                }

                row = (SalEntryRow)rows[0];
                return true;
            }

        }

        public partial class SalEntryRow
        {
            public SalEntryType Type
            {
                get { return (SalEntryType)TypeRaw; }
                set { TypeRaw = Convert.ToInt32(value); }
            }
        }

        #endregion

        public bool CacheLookup
        {
            get { return DefinedType.CacheLookup; }
            set
            {
                DefinedType.CacheLookup = value;
                TypedefType.CacheLookup = value;
                NamedType.CacheLookup = value;
            }
        }

        public void AddConstant(NativeConstant nConst)
        {
            if (nConst == null)
            {
                throw new ArgumentNullException("nConst");
            }

            ConstantRow constRow = Constant.NewConstantRow();
            constRow.Name = nConst.Name;
            constRow.Kind = nConst.ConstantKind;

            switch (nConst.ConstantKind)
            {
                case ConstantKind.MacroMethod:
                    // Macro Methods wrap the value in "" so that it will be a valid expression.  Strip them
                    // here
                    constRow.Value = nConst.Value.Expression.Substring(1, nConst.Value.Expression.Length - 2);
                    break;
                case ConstantKind.Macro:
                    // Save the value
                    constRow.Value = nConst.Value.Expression;
                    break;
                default:
                    ThrowInvalidEnumValue(nConst.ConstantKind);
                    break;
            }

            Constant.AddConstantRow(constRow);
        }

        /// <summary>
        /// Add a defined type to the table
        /// </summary>
        /// <param name="nt"></param>
        /// <remarks></remarks>
        public void AddDefinedType(NativeDefinedType nt)
        {
            if (nt == null)
            {
                throw new ArgumentNullException("nt");
            }

            // Add the core type information first.  That we when doing recursive member
            // adds, we can query the table to see if a type has already been added
            DefinedTypeRow dtRow = this.DefinedType.Add(nt.Kind, nt.Name);

            // Add the members
            foreach (NativeMember member in nt.Members)
            {
                TypeReference typeRef = CreateTypeReference(member.NativeType);
                this.Member.Add(dtRow, member.Name, typeRef);
            }

            if (nt.Kind == NativeSymbolKind.EnumType)
            {
                // If this is an enum then add it to the list
                NativeEnum ntEnum = (NativeEnum)nt;
                foreach (NativeEnumValue enumVal in ntEnum.Values)
                {
                    this.EnumValue.Add(dtRow, enumVal.Name, enumVal.Value.Expression);
                }
            }
            else if (nt.Kind == NativeSymbolKind.FunctionPointer)
            {
                // if this is a function pointer then make sure to add the reference to the 
                // signature
                NativeFunctionPointer fPtr = (NativeFunctionPointer)nt;
                SignatureRow sigRow = this.AddSignature(fPtr.Signature);
                dtRow.SignatureId = sigRow.Id;
                dtRow.CallingConvention = fPtr.CallingConvention;
            }
        }

        /// <summary>
        /// Add the typedef into the dataset
        /// </summary>
        /// <param name="nt"></param>
        /// <remarks></remarks>
        public void AddTypedef(NativeTypeDef nt)
        {
            if (nt == null)
            {
                throw new ArgumentNullException("nt");
            }

            if (nt.RealType == null)
            {
                string msg = string.Format("NativeTypedef does not point to a real type");
                throw new InvalidOperationException(msg);
            }

            // First look for an existing entry
            TypedefTypeRow trow = null;
            if (TypedefType.TryFindByName(nt.Name, out trow))
            {
                return;
            }

            TypeReference typeRef = CreateTypeReference(nt.RealType);
            TypedefType.Add(nt.Name, typeRef);
        }

        public bool TryLoadTypedef(string name, out NativeTypeDef typedefNt)
        {
            typedefNt = null;

            TypedefTypeRow trow = null;
            if (!TypedefType.TryFindByName(name, out trow))
            {
                return false;
            }

            NativeType nt = null;
            if (!TryLoadType(new TypeReference(trow.RealTypeId, trow.RealTypeKind), out nt))
            {
                return false;
            }

            typedefNt = new NativeTypeDef(trow.Name, nt);
            return true;
        }

        /// <summary>
        /// Add a procedure into the table
        /// </summary>
        /// <param name="proc"></param>
        /// <remarks></remarks>
        public void AddProcedure(NativeProcedure proc)
        {
            if (proc == null)
            {
                throw new ArgumentNullException("proc");
            }

            // Store the procedure row
            int sigId = AddSignature(proc.Signature).Id;
            Procedure.Add(proc.Name, proc.DllName, proc.CallingConvention, sigId);
        }

        private SignatureRow AddSignature(NativeSignature sig)
        {
            ThrowIfNull(sig);

            // Create the row
            SignatureRow sigRow = this.Signature.NewSignatureRow();
            if (sig.ReturnType != null)
            {
                TypeReference typeref = CreateTypeReference(sig.ReturnType);
                sigRow.ReturnTypeId = typeref.Id;
                sigRow.ReturnTypeKind = typeref.Kind;
            }

            sigRow.ReturnTypeSalId = AddSalAttribute(sig.ReturnTypeSalAttribute);
            this.Signature.AddSignatureRow(sigRow);

            // Store each of the parameters
            foreach (NativeParameter param in sig.Parameters)
            {
                Parameter.Add(sigRow, param.Name, CreateTypeReference(param.NativeType), AddSalAttribute(param.SalAttribute));
            }

            return sigRow;
        }

        /// <summary>
        /// Save the sal attribute.  Return a comma separed list of Id's
        /// </summary>
        /// <param name="attr"></param>
        /// <remarks></remarks>
        private string AddSalAttribute(NativeSalAttribute attr)
        {
            if (attr.IsEmpty)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            foreach (NativeSalEntry entry in attr.SalEntryList)
            {
                SalEntryRow row = null;

                // Only try and cache when there is no text
                if (string.IsNullOrEmpty(entry.Text))
                {
                    if (!SalEntry.TryFindNoText(entry.SalEntryType, out row))
                    {
                        row = SalEntry.Add(entry.SalEntryType, null);
                    }
                }
                else
                {
                    if (!SalEntry.TryFind(entry.SalEntryType, entry.Text, out row))
                    {
                        row = SalEntry.Add(entry.SalEntryType, entry.Text);
                    }
                }

                if (builder.Length > 0)
                {
                    builder.Append(',');
                }
                builder.Append(row.Id);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Attributes are stored as a comma delimeted list of the attributes
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool TryLoadSalAttribute(string str, out NativeSalAttribute attr)
        {
            string[] arr = str.Split(',');
            attr = new NativeSalAttribute();

            foreach (string cur in arr)
            {
                int id = 0;
                SalEntryRow row = null;
                if (!Int32.TryParse(cur, out id) || !SalEntry.TryFindById(id, out row))
                {
                    return false;
                }

                NativeSalEntry entry = new NativeSalEntry();
                entry.SalEntryType = row.Type;
                if (!row.IsTextNull())
                {
                    entry.Text = row.Text;
                }

                attr.SalEntryList.Add(entry);
            }

            return true;
        }

        /// <summary>
        /// Search for a defined type with the specified name pattern
        /// </summary>
        /// <param name="namePattern"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<NativeDefinedType> SearchForDefinedType(string namePattern)
        {
            List<NativeDefinedType> list = new List<NativeDefinedType>();
            foreach (DefinedTypeRow nRow in DefinedType.FindByNamePattern(namePattern))
            {
                NativeDefinedType definedNt = null;
                if (TryLoadDefined(nRow.Name, out definedNt))
                {
                    list.Add(definedNt);
                }
            }

            return list;
        }

        public List<NativeTypeDef> SearchForTypedef(string namePattern)
        {
            List<NativeTypeDef> list = new List<NativeTypeDef>();
            foreach (TypedefTypeRow nRow in TypedefType.FindByNamePattern(namePattern))
            {
                NativeTypeDef typeDef = null;
                if (TryLoadTypedef(nRow.Name, out typeDef))
                {
                    list.Add(typeDef);
                }
            }

            return list;
        }

        public List<NativeProcedure> SearchForProcedure(string namePattern)
        {
            List<NativeProcedure> list = new List<NativeProcedure>();
            foreach (ProcedureRow nRow in Procedure.FindByNamePattern(namePattern))
            {
                NativeProcedure proc = null;
                if (TryLoadProcedure(nRow.Name, out proc))
                {
                    list.Add(proc);
                }
            }

            return list;
        }

        public List<NativeConstant> SearchForConstant(string namePattern)
        {
            List<NativeConstant> list = new List<NativeConstant>();
            foreach (ConstantRow nRow in this.Constant.FindByNamePattern(namePattern))
            {
                NativeConstant c = null;
                if (TryLoadConstant(nRow.Name, out c))
                {
                    list.Add(c);
                }
            }

            return list;
        }

        public bool TryLoadDefined(string name, out NativeDefinedType definedNt)
        {
            definedNt = null;

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            DefinedTypeRow dtRow = null;
            if (!DefinedType.TryFindByName(name, out dtRow))
            {
                return false;
            }

            NativeType nt = null;
            if (!TryLoadDefinedType(new TypeReference(dtRow.Id, dtRow.Kind), out nt))
            {
                return false;
            }

            definedNt = (NativeDefinedType)nt;
            return true;
        }

        /// <summary>
        /// Try and load a type by it's name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool TryLoadByName(string name, out NativeType nt)
        {
            NativeDefinedType definedNt = null;
            if (TryLoadDefined(name, out definedNt))
            {
                nt = definedNt;
                return true;
            }

            NativeTypeDef typedef = null;
            if (TryLoadTypedef(name, out typedef))
            {
                nt = typedef;
                return true;
            }

            // Lastly try and load the Builtin types
            NativeBuiltinType bt = null;
            if (NativeBuiltinType.TryConvertToBuiltinType(name, out bt))
            {
                nt = bt;
                return true;
            }

            nt = null;
            return false;
        }

        public bool TryLoadConstant(string name, out NativeConstant nConst)
        {
            ConstantRow constRow = null;
            if (!Constant.TryFindByName(name, out constRow))
            {
                nConst = null;
                return false;
            }

            nConst = new NativeConstant(constRow.Name, constRow.Value, constRow.Kind);
            return true;
        }

        /// <summary>
        /// Try and load a procedure by it's name
        /// </summary>
        /// <param name="retProc"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool TryLoadProcedure(string name, out NativeProcedure retProc)
        {
            retProc = null;

            NativeProcedure proc = null;
            ProcedureRow procRow = null;
            if (!Procedure.TryLoadByName(name, out procRow))
            {
                return false;
            }

            // Load the procedure
            proc = new NativeProcedure();
            proc.Name = procRow.Name;
            proc.CallingConvention = procRow.CallingConvention;
            if (!procRow.IsDllNameNull())
            {
                proc.DllName = procRow.DllName;
            }

            // Try and load the signature
            if (!TryLoadSignature(procRow.SignatureId, out proc.Signature))
            {
                return false;
            }

            retProc = proc;
            return true;
        }

        public List<PInvoke.Parser.Macro> LoadAllMacros()
        {
            return Constant.LoadAllMacros();
        }

        private bool TryLoadSignature(Int32 id, out NativeSignature retSig)
        {
            retSig = null;

            SignatureRow sigRow = null;
            if (!Signature.TryLoadById(id, out sigRow))
            {
                return false;
            }

            NativeSignature sig = new NativeSignature();

            if (!TryLoadType(new TypeReference(sigRow.ReturnTypeId, sigRow.ReturnTypeKind), out sig.ReturnType))
            {
                return false;
            }

            // Load the sal attribute on the return value
            if (!sigRow.IsReturnTypeSalIdNull() && !TryLoadSalAttribute(sigRow.ReturnTypeSalId, out sig.ReturnTypeSalAttribute))
            {
                return false;
            }

            // Load the parameters
            foreach (ParameterRow paramRow in sigRow.GetParameterRows())
            {
                NativeParameter param = new NativeParameter();

                // When this is a function pointer, the name can be null
                param.Name = string.Empty;
                if (!paramRow.IsNameNull())
                {
                    param.Name = paramRow.Name;
                }

                if (!TryLoadType(new TypeReference(paramRow.TypeId, paramRow.TypeKind), out param.NativeType))
                {
                    return false;
                }

                if (!paramRow.IsSalIdNull() && !TryLoadSalAttribute(paramRow.SalId, out param.SalAttribute))
                {
                    return false;
                }

                sig.Parameters.Add(param);
            }

            retSig = sig;
            return true;
        }

        #region "Private Methods"

        #region "Add Types"

        /// <summary>
        /// Create a reference to a native type.  This will add the appropriate entries into the table
        /// to reference this type
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal TypeReference CreateTypeReference(NativeType nt)
        {
            ThrowIfNull(nt);

            switch (nt.Category)
            {
                case NativeSymbolCategory.Defined:
                    return CreateTypeReferenceToName(nt);
                case NativeSymbolCategory.Proxy:
                    return CreateTypeReferenceToProxy((NativeProxyType)nt);
                case NativeSymbolCategory.Specialized:
                    return CreateTypeReferenceToSpecialized((NativeSpecializedType)nt);
                default:
                    ThrowInvalidEnumValue(nt.Category);
                    // Will throw
                    return null;
            }
        }

        /// <summary>
        /// Create a type reference to a name.  
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private TypeReference CreateTypeReferenceToName(NativeType nt)
        {
            ThrowIfNull(nt);

            // Create a NativeNamedType to make the referenc to
            NativeNamedType namedNt = new NativeNamedType(nt.Name);
            return CreateTypeReferenceToProxy(namedNt);
        }

        private TypeReference CreateTypeReferenceToNamedType(NativeNamedType nt)
        {
            NamedTypeRow nRow = null;
            if (!NamedType.TryFindByName(nt.Qualification, nt.Name, nt.IsConst, out nRow))
            {
                nRow = NamedType.Add(nt.Qualification, nt.Name, nt.IsConst);
            }

            return new TypeReference(nRow.Id, NativeSymbolKind.NamedType);
        }

        private TypeReference CreateTypeReferenceToTypedef(NativeTypeDef nt)
        {
            TypedefTypeRow nRow = null;
            if (!TypedefType.TryFindByName(nt.Name, out nRow))
            {
                nRow = TypedefType.Add(nt.Name, CreateTypeReference(nt.RealType));
            }

            return new TypeReference(nRow.Id, NativeSymbolKind.TypedefType);
        }

        private TypeReference CreateTypeReferenceToArray(NativeArray nt)
        {
            TypeReference typeref = CreateTypeReference(nt.RealType);
            ArrayTypeRow row = ArrayType.Add(nt.ElementCount, typeref);
            return new TypeReference(row.Id, NativeSymbolKind.ArrayType);
        }

        private TypeReference CreateTypeReferenceToPointer(NativePointer nt)
        {
            TypeReference typeref = CreateTypeReference(nt.RealType);
            PointerTypeRow row = null;
            if (!PointerType.TryFindByTarget(typeref, out row))
            {
                row = PointerType.Add(typeref);
            }

            return new TypeReference(row.Id, NativeSymbolKind.PointerType);
        }

        /// <summary>
        /// Create a type reference to the proxy.
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private TypeReference CreateTypeReferenceToProxy(NativeProxyType nt)
        {
            ThrowIfNull(nt);

            // See what kind of type reference we're adding and special case then optimized ones
            switch (nt.Kind)
            {
                case NativeSymbolKind.NamedType:
                    return CreateTypeReferenceToNamedType((NativeNamedType)nt);
                case NativeSymbolKind.TypedefType:
                    return CreateTypeReferenceToTypedef((NativeTypeDef)nt);
                case NativeSymbolKind.ArrayType:
                    return CreateTypeReferenceToArray((NativeArray)nt);
                case NativeSymbolKind.PointerType:
                    return CreateTypeReferenceToPointer((NativePointer)nt);
                default:
                    throw new Exception("Invalid enum value");
            }
        }

        /// <summary>
        /// Create a type reference to the specialized type.  
        /// </summary>
        /// <param name="nt"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private TypeReference CreateTypeReferenceToSpecialized(NativeSpecializedType nt)
        {
            ThrowIfNull(nt);

            // Optimization.  See if there is an entry we can reuse here
            SpecializedTypeRow existingRow = null;
            switch (nt.Kind)
            {
                case NativeSymbolKind.BuiltinType:
                    NativeBuiltinType nativeBt = (NativeBuiltinType)nt;
                    this.SpecializedType.TryFindBuiltin(nativeBt.BuiltinType, nativeBt.IsUnsigned, out existingRow);
                    break;
                case NativeSymbolKind.BitVectorType:
                    this.SpecializedType.TryFindBitVector(((NativeBitVector)nt).Size, out existingRow);
                    break;
                case NativeSymbolKind.OpaqueType:
                    return new TypeReference(0, NativeSymbolKind.OpaqueType);
            }

            if (existingRow != null)
            {
                return new TypeReference(existingRow.Id, nt.Kind);
            }

            // Get the id
            SpecializedTypeRow row = this.SpecializedType.NewSpecializedTypeRow();
            row.Kind = nt.Kind;
            switch (nt.Kind)
            {
                case NativeSymbolKind.BitVectorType:
                    NativeBitVector bitNt = (NativeBitVector)nt;
                    row.BitVectorSize = bitNt.Size;
                    break;
                case NativeSymbolKind.BuiltinType:
                    NativeBuiltinType builtinNt = (NativeBuiltinType)nt;
                    row.BuiltinType = builtinNt.BuiltinType;
                    row.IsUnsigned = builtinNt.IsUnsigned;
                    break;
            }
            this.SpecializedType.AddSpecializedTypeRow(row);
            return new TypeReference(row.Id, nt.Kind);
        }

        #endregion

        #region "LoadTypes"

        public bool TryLoadType(TypeReference typeRef, out NativeType nt)
        {
            ThrowIfNull(typeRef);

            switch (typeRef.Kind)
            {
                case NativeSymbolKind.StructType:
                case NativeSymbolKind.UnionType:
                case NativeSymbolKind.EnumNameValue:
                case NativeSymbolKind.FunctionPointer:
                    return TryLoadDefinedType(typeRef, out nt);
                case NativeSymbolKind.TypedefType:
                case NativeSymbolKind.NamedType:
                case NativeSymbolKind.ArrayType:
                case NativeSymbolKind.PointerType:
                    return TryLoadProxyType(typeRef, out nt);
                case NativeSymbolKind.BuiltinType:
                case NativeSymbolKind.BitVectorType:
                    return TryLoadSpecialized(typeRef, out nt);
                default:
                    ThrowInvalidEnumValue(typeRef.Kind);
                    nt = null;
                    return false;
            }

        }

        private bool TryLoadDefinedType(TypeReference typeRef, out NativeType nt)
        {

            int id = typeRef.Id;
            nt = null;
            DefinedTypeRow drow = null;
            if (!DefinedType.TryFindById(id, out drow))
            {
                return false;
            }

            NativeDefinedType dt = null;
            switch (drow.Kind)
            {
                case NativeSymbolKind.StructType:
                    dt = new NativeStruct();
                    break;
                case NativeSymbolKind.EnumType:
                    // Load the enum values
                    NativeEnum et = new NativeEnum();
                    List<EnumValueRow> erows = null;
                    if (!this.EnumValue.TryFindById(drow.Id, out erows))
                    {
                        return false;
                    }

                    foreach (EnumValueRow row in erows)
                    {
                        et.Values.Add(new NativeEnumValue(row.Name, row.Value));
                    }


                    dt = et;
                    break;
                case NativeSymbolKind.UnionType:
                    dt = new NativeUnion();
                    break;
                case NativeSymbolKind.FunctionPointer:
                    NativeFunctionPointer fptr = new NativeFunctionPointer();
                    fptr.CallingConvention = drow.CallingConvention;

                    if (!this.TryLoadSignature(drow.SignatureId, out fptr.Signature))
                    {
                        return false;
                    }
                    dt = fptr;
                    break;
                default:
                    ThrowInvalidEnumValue(drow.Kind);
                    break;
            }

            // Set the common properties
            dt.Name = drow.Name;
            List<MemberRow> memberRows = null;

            if (Member.TryFindById(drow.Id, out memberRows))
            {
                foreach (MemberRow memberRow in memberRows)
                {
                    NativeMember member = new NativeMember();
                    member.Name = memberRow.Name;

                    if (!TryLoadType(new TypeReference(memberRow.TypeId, memberRow.TypeKind), out member.NativeType))
                    {
                        return false;
                    }
                    dt.Members.Add(member);
                }
            }

            nt = dt;
            return true;
        }

        private bool TryLoadProxyType(TypeReference typeRef, out NativeType nt)
        {
            nt = null;

            int id = typeRef.Id;

            if (NativeSymbolKind.TypedefType == typeRef.Kind)
            {
                TypedefTypeRow trow = null;
                if (!TypedefType.TryFindById(typeRef.Id, out trow))
                {
                    return false;
                }

                NativeType realNt = null;
                if (!TryLoadType(new TypeReference(trow.RealTypeId, trow.RealTypeKind), out realNt))
                {
                    return false;
                }

                nt = new NativeTypeDef(trow.Name, realNt);
                return true;
            }
            else if (NativeSymbolKind.NamedType == typeRef.Kind)
            {
                NamedTypeRow nrow = null;
                if (!NamedType.TryFindById(typeRef.Id, out nrow))
                {
                    return false;
                }

                nt = new NativeNamedType(nrow.Qualification, nrow.Name, nrow.IsConst);
                return true;
            }
            else if (NativeSymbolKind.ArrayType == typeRef.Kind)
            {
                ArrayTypeRow arow = null;
                if (!ArrayType.TryFindById(typeRef.Id, out arow))
                {
                    return false;
                }

                NativeType realNt = null;
                if (!TryLoadType(new TypeReference(arow.RealTypeId, arow.RealTypeKind), out realNt))
                {
                    return false;
                }

                nt = new NativeArray(realNt, arow.ElementCountt);
                return true;
            }
            else if (NativeSymbolKind.PointerType == typeRef.Kind)
            {
                PointerTypeRow prow = null;
                if (!PointerType.TryFindById(typeRef.Id, out prow))
                {
                    return false;
                }

                NativeType realNt = null;
                if (!TryLoadType(new TypeReference(prow.RealTypeId, prow.RealTypeKind), out realNt))
                {
                    return false;
                }

                nt = new NativePointer(realNt);
                return true;
            }
            else
            {
                ThrowInvalidEnumValue(typeRef.Kind);
                return false;
            }
        }

        private bool TryLoadSpecialized(TypeReference typeRef, out NativeType nt)
        {
            nt = null;

            if (typeRef.Kind == NativeSymbolKind.OpaqueType)
            {
                nt = new NativeOpaqueType();
                return true;
            }

            SpecializedTypeRow srow = null;
            if (!SpecializedType.TryFindById(typeRef.Id, out srow))
            {
                nt = null;
                return false;
            }
            else
            {
                switch (typeRef.Kind)
                {
                    case NativeSymbolKind.BitVectorType:
                        NativeBitVector bt = new NativeBitVector();
                        bt.Size = srow.BitVectorSize;
                        nt = bt;
                        break;
                    case NativeSymbolKind.BuiltinType:
                        nt = new NativeBuiltinType(srow.BuiltinType, srow.IsUnsigned);
                        break;
                    default:
                        ThrowInvalidEnumValue(typeRef.Kind);
                        return false;
                }

                return true;
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Look for the windows.xml file in the following location
        ///  - AssemblyPath\Data\windows.xml
        ///  - AssemblyPath\windows.xml
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static NativeStorage LoadFromAssemblyPath()
        {
            string loc = typeof(NativeStorage).Assembly.Location;
            string assemblyDirectory = Path.GetDirectoryName(loc);

            string target = Path.Combine(assemblyDirectory, "Data\\windows.xml");
            if (File.Exists(target))
            {
                return LoadFromPath(target);
            }

            target = Path.Combine(assemblyDirectory, "windows.xml");
            return LoadFromPath(target);
        }

        public static NativeStorage LoadFromPath(string target)
        {
            try
            {
                NativeStorage ns = new NativeStorage();
                ns.ReadXml(target);
                return ns;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                return new NativeStorage();
            }
        }
    }
}
