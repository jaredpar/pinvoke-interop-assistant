// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using PInvoke;
using ConstantRow = PInvoke.NativeStorage.ConstantRow;
using ProcedureRow = PInvoke.NativeStorage.ProcedureRow;
using DefinedTypeRow = PInvoke.NativeStorage.DefinedTypeRow;
using TypedefTypeRow = PInvoke.NativeStorage.TypedefTypeRow;

namespace PInvoke.Controls
{
    #region "SearchDataGrid"

    public enum SearchKind
    {
        None,
        Constant,
        Procedure,
        Type,
        All
    }

    public class SearchDataGrid : DataGridView
    {

        private Timer withEventsField_m_timer = new Timer();
        private Timer m_timer
        {
            get { return withEventsField_m_timer; }
            set
            {
                if (withEventsField_m_timer != null)
                {
                    withEventsField_m_timer.Tick -= OnTimerInterval;
                }
                withEventsField_m_timer = value;
                if (withEventsField_m_timer != null)
                {
                    withEventsField_m_timer.Tick += OnTimerInterval;
                }
            }
        }
        private DataGridViewColumn _nameColumn;
        private DataGridViewColumn _valueColumn;
        private INativeSymbolStorage _storage;
        private IncrementalSearch _search;
        private string _searchText;
        private bool _showInvalidData;
        private List<object> _list = new List<object>();
        private SearchKind _kind;
        private SearchDataGridInfo _info;
        private bool _handleCreated;
        private NativeSymbolBag _selectionBag;

        private List<string> _selectionList = new List<string>();
        /// <summary>
        /// Have to do this to prevent the designer from serializing my columns
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object Columns
        {
            get { return base.Columns; }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                StartSearch();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public INativeSymbolStorage Storage
        {
            get { return _storage; }
            set
            {
                _storage = value;
                if (_info != null)
                {
                    _info.Storage = value;
                    StartSearch();
                }
            }
        }

        public List<NativeSymbol> SelectedSymbols
        {
            get
            {
                List<NativeSymbol> list = new List<NativeSymbol>();
                foreach (DataGridViewRow row in SelectedRows)
                {
                    NativeSymbol symbol = null;
                    if (_info.TryConvertToSymbol(_list[row.Index], ref symbol))
                    {
                        list.Add(symbol);
                    }
                }

                return list;
            }
        }

        public NativeSymbolBag SelectedSymbolBag
        {
            get
            {
                // Only calculate this once per selection because it can be very expensive
                if (_selectionBag != null)
                {
                    return _selectionBag;
                }

                NativeSymbolBag bag = new NativeSymbolBag(_storage);
                foreach (NativeSymbol cur in SelectedSymbols)
                {

                    try
                    {
                        if (cur.Category == NativeSymbolCategory.Defined)
                        {
                            bag.AddDefinedType((NativeDefinedType)cur);
                        }
                        else
                        {
                            switch (cur.Kind)
                            {
                                case NativeSymbolKind.Constant:
                                    bag.AddConstant((NativeConstant)cur);
                                    break;
                                case NativeSymbolKind.Procedure:
                                    bag.AddProcedure((NativeProcedure)cur);
                                    break;
                                case NativeSymbolKind.TypedefType:
                                    bag.AddTypeDef((NativeTypeDef)cur);
                                    break;
                                default:
                                    Contract.ThrowInvalidEnumValue(cur.Kind);
                                    break;
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Can happen when a symbol is included in the list twice
                    }
                }

                _selectionBag = bag;
                return bag;
            }
        }

        public SearchKind SearchKind
        {
            get { return _kind; }
            set
            {
                _kind = value;
                var ns = (Storage as NativeStorage) ?? new NativeStorage();
                switch (_kind)
                {
                    case PInvoke.Controls.SearchKind.Constant:
                        _info = new SearchDataGridInfo.ConstantsInfo(ns);
                        break;
                    case PInvoke.Controls.SearchKind.Procedure:
                        _info = new SearchDataGridInfo.ProcedureInfo(ns);
                        break;
                    case PInvoke.Controls.SearchKind.Type:
                        _info = new SearchDataGridInfo.TypeInfo(ns);
                        break;
                    case PInvoke.Controls.SearchKind.None:
                        _info = new SearchDataGridInfo.EmptyInfo();
                        break;
                    case PInvoke.Controls.SearchKind.All:
                        _info = new SearchDataGridInfo.AllInfo(ns);
                        break;
                    default:
                        _info = new SearchDataGridInfo.EmptyInfo();
                        Contract.ThrowInvalidEnumValue(_kind);
                        break;
                }

                _valueColumn.Name = _info.ValueColumnName;
                StartSearch();
            }
        }

        public bool ShowInvalidData
        {
            get { return _showInvalidData; }
            set
            {
                if (value != _showInvalidData)
                {
                    _showInvalidData = value;
                    StartSearch();
                }
            }
        }

        [Category("Action")]
        public event EventHandler SelectedSymbolsChanged;

        public SearchDataGrid() : this(new BasicSymbolStorage())
        {

        }

        public SearchDataGrid(INativeSymbolStorage storage)
        {
            _storage = storage;
            m_timer.Enabled = false;
            m_timer.Interval = 500;

            this.VirtualMode = true;
            this.ReadOnly = true;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.AllowUserToResizeRows = false;
            this.RowHeadersVisible = false;

            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            nameColumn.HeaderText = "Name";
            nameColumn.ReadOnly = true;
            nameColumn.Width = 180;
            _nameColumn = nameColumn;

            DataGridViewTextBoxColumn valueColumn = new DataGridViewTextBoxColumn();
            valueColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            valueColumn.HeaderText = "Value";
            valueColumn.ReadOnly = true;
            valueColumn.MinimumWidth = 100;
            _valueColumn = valueColumn;

            base.Columns.AddRange(new DataGridViewColumn[] {
                nameColumn,
                valueColumn
            });

            SearchKind = PInvoke.Controls.SearchKind.None;
        }

        #region "Overrides"

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                m_timer.Dispose();
            }
        }

        protected override void OnCellValueNeeded(System.Windows.Forms.DataGridViewCellValueEventArgs e)
        {
            base.OnCellValueNeeded(e);
            if (e.RowIndex >= _list.Count)
            {
                return;
            }

            object cur = _list[e.RowIndex];
            if (e.ColumnIndex == _nameColumn.Index)
            {
                e.Value = _info.GetName(cur);
            }
            else if (e.ColumnIndex == _valueColumn.Index)
            {
                e.Value = _info.GetValue(cur);
            }
            else
            {
                Debug.Fail("Unexpected");
                e.Value = string.Empty;
            }
        }

        protected override void OnSelectionChanged(System.EventArgs e)
        {
            base.OnSelectionChanged(e);
            CheckForSelectedSymbolChange();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            _handleCreated = true;
            StartSearch();
        }

        #endregion

        #region "Event Handlers"

        private void OnTimerInterval(object sender, EventArgs e)
        {
            SearchImpl();
        }

        #endregion

        #region "Helpers"


        private void StartSearch()
        {
            // Don't start searching until we are actually displayed.  
            if (!_handleCreated)
            {
                return;
            }

            _list.Clear();
            m_timer.Enabled = true;
            RowCount = 0;

            if (_search != null)
            {
                _search.Cancel();
            }

            _info.ShowInvalidData = _showInvalidData;
            _info.SearchText = SearchText;
            _info.Storage = _storage;
            _search = new IncrementalSearch(_info.GetInitialData(), _info.ShouldAllow);

            SearchImpl();
        }

        private void SearchImpl()
        {
            if (_search == null)
            {
                CheckForSelectedSymbolChange();
                m_timer.Enabled = false;
                return;
            }

            Result result = _search.Search();
            if (result.Completed)
            {
                _search = null;
            }

            _list.AddRange(result.IncrementalFound);
            _list.Sort(SearchSort);
            RowCount = _list.Count;
            Refresh();
        }

        /// <summary>
        /// Sort based on the following guidance.  For the examples assume we are searching for
        /// "INT"
        /// 
        ///  1) Starts with before not starts with.  "integral" before "aligned int"
        ///  2) Shorter matches over longer ones
        ///  3) Alphabetically for everything else
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private int SearchSort(object left, object right)
        {
            string target = SearchText;
            string leftName = _info.GetName(left);
            string rightName = _info.GetName(right);

            if (!string.IsNullOrEmpty(target))
            {

                bool leftStart = leftName.StartsWith(target, StringComparison.OrdinalIgnoreCase);
                bool rightStart = rightName.StartsWith(target, StringComparison.OrdinalIgnoreCase);
                if (leftStart != rightStart)
                {
                    if (leftStart)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }

                if (leftName.Length != rightName.Length)
                {
                    return leftName.Length - rightName.Length;
                }
            }

            return string.Compare(leftName, rightName, StringComparison.OrdinalIgnoreCase);
        }

        private string GetHashString(DataGridViewRow row)
        {
            string val = null;
            try
            {
                val = row.Cells[_nameColumn.Index].Value as string;
            }
            catch (Exception)
            {
                val = string.Empty;
            }

            // Ensure that it's unique so that refreshes will be forced
            if (string.IsNullOrEmpty(val))
            {
                Debug.Fail("Error getting column name");
                val = Guid.NewGuid().ToString();
            }

            return val;
        }

        private void CheckForSelectedSymbolChange()
        {
            bool changed = false;
            if (_selectionList.Count != this.SelectedRows.Count)
            {
                changed = true;
            }
            else
            {
                for (int i = 0; i <= _selectionList.Count - 1; i++)
                {
                    if (!StringComparer.Ordinal.Equals(_selectionList[i], GetHashString(SelectedRows[i])))
                    {
                        changed = true;
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
            }

            if (changed)
            {
                _selectionBag = null;
                // Eliminate the cache

                // Cache the current selection
                _selectionList.Clear();
                foreach (DataGridViewRow row in this.SelectedRows)
                {
                    _selectionList.Add(GetHashString(row));
                }

                if (SelectedSymbolsChanged != null)
                {
                    SelectedSymbolsChanged(this, EventArgs.Empty);
                }
            }
        }
        #endregion

    }

    public abstract class SearchDataGridInfo
    {
        // CTODO: should this be a lookup instead? 
        public INativeSymbolStorage Storage;
        public bool ShowInvalidData;

        public string SearchText;
        public virtual string ValueColumnName
        {
            get { return "Value"; }
        }

        protected SearchDataGridInfo(INativeSymbolStorage storage)
        {
            Storage = storage;
        }

        public bool ShouldAllow(object cur)
        {
            return ShouldAllowCore(cur);
        }

        public abstract IEnumerable GetInitialData();
        public abstract string GetName(object o);
        public abstract string GetValue(object o);
        public abstract bool TryConvertToSymbol(object o, ref NativeSymbol symbol);

        protected virtual bool ShouldAllowCore(object cur)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return true;
            }

            string name = GetName(cur);
            return name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #region "EmptyInfo"

        public class EmptyInfo : SearchDataGridInfo
        {
            public EmptyInfo() : base(new BasicSymbolStorage())
            {

            }

            public override IEnumerable GetInitialData()
            {
                return new List<object>();
            }

            public override string GetName(object o)
            {
                return string.Empty;
            }

            public override string GetValue(object o)
            {
                return string.Empty;
            }

            public override bool TryConvertToSymbol(object o, ref NativeSymbol symbol)
            {
                return false;
            }
        }

        #endregion

        #region "ConstantsInfo"
        public class ConstantsInfo : SearchDataGridInfo
        {
            public NativeStorage NativeStorage { get; }

            public ConstantsInfo(NativeStorage storage) : base(storage)
            {
                NativeStorage = storage;
            }

            public override IEnumerable GetInitialData()
            {
                return NativeStorage.Constant;
            }

            public override string GetName(object o)
            {
                ConstantRow row = (ConstantRow)o;
                return row.Name;
            }

            public override string GetValue(object o)
            {
                ConstantRow row = (ConstantRow)o;
                NativeConstant c = null;
                if (Storage.TryFindConstant(row.Name, out c))
                {
                    return c.Value.Expression;
                }
                else
                {
                    return string.Empty;
                }

            }

            public override bool TryConvertToSymbol(object o, ref NativeSymbol symbol)
            {
                ConstantRow row = (ConstantRow)o;
                NativeConstant cValue = null;
                if (Storage.TryFindConstant(row.Name, out cValue))
                {
                    symbol = cValue;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            protected override bool ShouldAllowCore(object cur)
            {
                if (!ShowInvalidData)
                {
                    ConstantRow row = (ConstantRow)cur;
                    if (!IsValidConstant(row))
                    {
                        return false;
                    }
                }

                return base.ShouldAllowCore(cur);
            }

            public static bool IsValidConstant(ConstantRow row)
            {
                if (row.Kind == ConstantKind.MacroMethod)
                {
                    return false;
                }

                Parser.ExpressionParser p = new Parser.ExpressionParser();
                Parser.ExpressionNode node = null;
                if (!p.TryParse(row.Value, out node))
                {
                    return false;
                }

                return !ContainsInvalidKind(node);
            }

            private static bool ContainsInvalidKind(Parser.ExpressionNode node)
            {
                if (node == null)
                {
                    return false;
                }

                if (node.Kind == Parser.ExpressionKind.Cast || node.Kind == Parser.ExpressionKind.FunctionCall)
                {
                    return true;
                }

                return ContainsInvalidKind(node.LeftNode) || ContainsInvalidKind(node.RightNode);
            }
        }

        #endregion

        #region "ProcedureInfo"
        public class ProcedureInfo : SearchDataGridInfo
        {
            public NativeStorage NativeStorage { get; }

            public ProcedureInfo(NativeStorage storage) : base(storage)
            {
                NativeStorage = storage;
            }

            public override System.Collections.IEnumerable GetInitialData()
            {
                return NativeStorage.Procedure.Rows;
            }

            public override string GetName(object o)
            {
                ProcedureRow row = (ProcedureRow)o;
                return row.Name;
            }

            public override string GetValue(object o)
            {
                ProcedureRow row = (ProcedureRow)o;
                NativeProcedure proc = null;
                if (Storage.TryFindProcedure(row.Name, out proc))
                {
                    return proc.Signature.DisplayName;
                }

                return string.Empty;
            }

            public override bool TryConvertToSymbol(object o, ref NativeSymbol symbol)
            {
                ProcedureRow row = (ProcedureRow)o;
                NativeProcedure proc = null;
                if (Storage.TryFindProcedure(row.Name, out proc))
                {
                    symbol = proc;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region "TypeInfo"
        public class TypeInfo : SearchDataGridInfo
        {
            public NativeStorage NativeStorage { get; }

            public TypeInfo(NativeStorage storage) : base(storage)
            {
                NativeStorage = storage;
            }

            public override System.Collections.IEnumerable GetInitialData()
            {
                ArrayList list = new ArrayList();
                list.AddRange(NativeStorage.DefinedType.Rows);
                list.AddRange(NativeStorage.TypedefType.Rows);
                return list;
            }

            public override string GetName(object o)
            {
                DefinedTypeRow definedRow = o as DefinedTypeRow;
                TypedefTypeRow typedefRow = o as TypedefTypeRow;
                if (definedRow != null)
                {
                    return definedRow.Name;
                }
                else if (typedefRow != null)
                {
                    return typedefRow.Name;
                }
                else
                {
                    return string.Empty;
                }
            }

            public override string GetValue(object o)
            {
                string name = GetName(o);
                NativeType type = null;
                if (Storage.TryFindByName(name, out type))
                {
                    switch (type.Kind)
                    {
                        case NativeSymbolKind.StructType:
                            return "struct";
                        case NativeSymbolKind.UnionType:
                            return "union";
                        case NativeSymbolKind.EnumNameValue:
                        case NativeSymbolKind.EnumType:
                            return "enum";
                        case NativeSymbolKind.FunctionPointer:
                            return "function pointer";
                        case NativeSymbolKind.TypedefType:
                            return "typedef";
                    }
                }

                return string.Empty;
            }

            public override bool TryConvertToSymbol(object o, ref NativeSymbol symbol)
            {
                string name = GetName(o);
                NativeType type = null;
                if (Storage.TryFindByName(name, out type))
                {
                    symbol = type;
                    return true;
                }

                return false;
            }
        }
        #endregion

        #region "AllInfo"
        public class AllInfo : SearchDataGridInfo
        {
            public NativeStorage NativeStorage { get; }

            public AllInfo(NativeStorage storage) : base(storage)
            {
                NativeStorage = storage;
            }

            public override System.Collections.IEnumerable GetInitialData()
            {
                ArrayList list = new ArrayList();
                list.AddRange(NativeStorage.Constant.Rows);
                list.AddRange(NativeStorage.Procedure.Rows);
                list.AddRange(NativeStorage.DefinedType.Rows);
                list.AddRange(NativeStorage.TypedefType.Rows);
                return list;
            }

            public override string GetName(object o)
            {
                ProcedureRow procRow = o as ProcedureRow;
                ConstantRow constRow = o as ConstantRow;
                DefinedTypeRow definedRow = o as DefinedTypeRow;
                TypedefTypeRow typedefRow = o as TypedefTypeRow;
                if (definedRow != null)
                {
                    return definedRow.Name;
                }
                else if (typedefRow != null)
                {
                    return typedefRow.Name;
                }
                else if (constRow != null)
                {
                    return constRow.Name;
                }
                else if (procRow != null)
                {
                    return procRow.Name;
                }
                else
                {
                    return string.Empty;
                }
            }

            public override string GetValue(object o)
            {
                NativeSymbol symbol = null;
                string value = null;
                if (TryGetSymbolAndValue(o, ref symbol, ref value))
                {
                    return value;
                }
                else
                {
                    return string.Empty;
                }
            }

            public override bool TryConvertToSymbol(object o, ref NativeSymbol symbol)
            {
                string value = null;
                return TryGetSymbolAndValue(o, ref symbol, ref value);
            }

            private bool TryGetSymbolAndValue(object o, ref NativeSymbol symbol, ref string value)
            {
                ProcedureRow procRow = o as ProcedureRow;
                ConstantRow constRow = o as ConstantRow;
                DefinedTypeRow definedRow = o as DefinedTypeRow;
                TypedefTypeRow typedefRow = o as TypedefTypeRow;
                string name = GetName(o);

                bool ret = true;
                if (definedRow != null || typedefRow != null)
                {
                    NativeType type = null;
                    if (Storage.TryFindByName(name, out type))
                    {
                        symbol = type;
                        switch (type.Kind)
                        {
                            case NativeSymbolKind.StructType:
                                value = "struct";
                                break;
                            case NativeSymbolKind.UnionType:
                                value = "union";
                                break;
                            case NativeSymbolKind.EnumType:
                                value = "enum";
                                break;
                            case NativeSymbolKind.EnumNameValue:
                                value = "enum";
                                break;
                            case NativeSymbolKind.FunctionPointer:
                                value = "function pointer";
                                break;
                            case NativeSymbolKind.TypedefType:
                                value = "typedef";
                                break;
                            default:
                                ret = false;
                                break;
                        }
                    }
                    else
                    {
                        ret = false;
                    }
                }
                else if (constRow != null)
                {
                    NativeConstant cValue = null;
                    if (Storage.TryFindConstant(name, out cValue))
                    {
                        symbol = cValue;
                        value = cValue.Value.Expression;
                    }
                    else
                    {
                        ret = false;
                    }
                }
                else if (procRow != null)
                {
                    NativeProcedure proc = null;
                    if (Storage.TryFindProcedure(name, out proc))
                    {
                        symbol = proc;
                        value = proc.Signature.DisplayName;
                    }
                    else
                    {
                        ret = false;
                    }
                }
                else
                {
                    ret = false;
                }

                Debug.Assert(ret, "Please file a bug: " + name);
                return ret;
            }

            protected override bool ShouldAllowCore(object cur)
            {
                if (!ShowInvalidData)
                {
                    ConstantRow constRow = cur as ConstantRow;
                    if (constRow != null && !ConstantsInfo.IsValidConstant(constRow))
                    {
                        return false;
                    }
                }

                return base.ShouldAllowCore(cur);
            }
        }

        #endregion

    }


    #endregion

}
