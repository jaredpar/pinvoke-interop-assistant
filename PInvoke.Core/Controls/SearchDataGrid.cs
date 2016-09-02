// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;

namespace PInvoke.Controls
{
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
        private Timer _timerRaw = new Timer();

        private DataGridViewColumn _nameColumn;
        private DataGridViewColumn _valueColumn;
        private INativeSymbolStorage _storage;
        private IncrementalSearch _search;
        private string _searchText;
        private bool _showInvalidData;
        private List<NativeName> _list = new List<NativeName>();
        private SearchKind _kind;
        private SearchDataGridInfo _info;
        private bool _handleCreated;
        private NativeSymbolBag _selectionBag;

        private List<string> _selectionList = new List<string>();

        private Timer Timer
        {
            get { return _timerRaw; }
            set
            {
                if (_timerRaw != null)
                {
                    _timerRaw.Tick -= OnTimerInterval;
                }
                _timerRaw = value;
                if (_timerRaw != null)
                {
                    _timerRaw.Tick += OnTimerInterval;
                }
            }
        }

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
                    var name = _list[row.Index];
                    NativeSymbol symbol;
                    if (Storage.TryGetGlobalSymbol(name, out symbol))
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
                switch (_kind)
                {
                    case SearchKind.Constant:
                        _info = new SearchDataGridInfo.ConstantsInfo(Storage);
                        break;
                    case SearchKind.Procedure:
                        _info = new SearchDataGridInfo.ProcedureInfo(Storage);
                        break;
                    case SearchKind.Type:
                        _info = new SearchDataGridInfo.TypeInfo(Storage);
                        break;
                    case SearchKind.None:
                        _info = new SearchDataGridInfo.EmptyInfo();
                        break;
                    case SearchKind.All:
                        _info = new SearchDataGridInfo.AllInfo(Storage);
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
            Timer.Enabled = false;
            Timer.Interval = 500;

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
                Timer.Dispose();
            }
        }

        protected override void OnCellValueNeeded(System.Windows.Forms.DataGridViewCellValueEventArgs e)
        {
            base.OnCellValueNeeded(e);
            if (e.RowIndex >= _list.Count)
            {
                return;
            }

            var cur = _list[e.RowIndex];
            if (e.ColumnIndex == _nameColumn.Index)
            {
                e.Value = cur.Name;
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

        private void StartSearch()
        {
            // Don't start searching until we are actually displayed.  
            if (!_handleCreated)
            {
                return;
            }

            _list.Clear();
            Timer.Enabled = true;
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
                Timer.Enabled = false;
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
        private int SearchSort(NativeName left, NativeName right)
        {
            string target = SearchText;
            string leftName = left.Name;
            string rightName = right.Name;

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

        public bool ShouldAllow(NativeName name)
        {
            return ShouldAllowCore(name);
        }

        public abstract IEnumerable<NativeName> GetInitialData();

        public string GetValue(NativeName name)
        {
            NativeGlobalSymbol symbol;
            if (!Storage.TryGetGlobalSymbol(name, out symbol))
            {
                return string.Empty;
            }

            return GetValue(symbol);
        }

        public string GetValue(NativeGlobalSymbol symbol)
        {
            switch (symbol.Kind)
            {
                case NativeNameKind.Struct:
                    return $"struct {symbol.Symbol.Name}";
                case NativeNameKind.Union:
                    return $"union {symbol.Symbol.Name}";
                case NativeNameKind.FunctionPointer:
                    return $"function pointer {symbol.Symbol.Name}";
                case NativeNameKind.Procedure:
                    return ((NativeProcedure)symbol.Symbol).Signature.DisplayName;
                case NativeNameKind.TypeDef:
                    return $"typedef {symbol.Symbol.Name}";
                case NativeNameKind.Constant:
                    return ((NativeConstant)symbol.Symbol).Value.Expression;
                case NativeNameKind.Enum:
                    return $"enum {symbol.Symbol.Name}";
                case NativeNameKind.EnumValue:
                    return ((NativeEnumValue)symbol.Symbol).Value.Expression;
                default:
                    Contract.ThrowInvalidEnumValue(symbol.Kind);
                    return string.Empty;
            }
        }

        protected virtual bool ShouldAllowCore(NativeName name)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return true;
            }

            string rawName = name.Name;
            return rawName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #region "EmptyInfo"

        public class EmptyInfo : SearchDataGridInfo
        {
            public EmptyInfo() : base(new BasicSymbolStorage())
            {

            }

            public override IEnumerable<NativeName> GetInitialData() => new NativeName[] { };
        }

        #endregion

        #region "ConstantsInfo"
        public class ConstantsInfo : SearchDataGridInfo
        {
            public ConstantsInfo(INativeSymbolStorage storage) :base(storage)
            {

            }

            public override IEnumerable<NativeName> GetInitialData() => Storage.NativeNames.Where(x => x.Kind == NativeNameKind.Constant);

            protected override bool ShouldAllowCore(NativeName name)
            {
                NativeConstant constant;
                if (!Storage.TryGetGlobalSymbol(name, out constant) ||
                    !IsValidConstant(constant))
                {
                    return false;
                }

                return base.ShouldAllowCore(name);
            }

            public static bool IsValidConstant(NativeConstant constant)
            {
                if (constant.ConstantKind == ConstantKind.MacroMethod)
                {
                    return false;
                }

                Parser.ExpressionParser p = new Parser.ExpressionParser();
                Parser.ExpressionNode node = null;
                if (!p.TryParse(constant.Value.Expression, out node))
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
            public ProcedureInfo(INativeSymbolStorage storage) : base(storage)
            {

            }

            public override IEnumerable<NativeName> GetInitialData() => Storage.NativeNames.Where(x => x.Kind == NativeNameKind.Procedure);
        }

        #endregion

        #region "TypeInfo"

        public class TypeInfo : SearchDataGridInfo
        {
            public TypeInfo(INativeSymbolStorage storage) : base(storage)
            {

            }

            public override IEnumerable<NativeName> GetInitialData() => Storage.NativeNames.Where(x => NativeNameUtil.IsAnyType(x.Kind));
        }

        #endregion

        #region "AllInfo"

        public class AllInfo : SearchDataGridInfo
        {
            public AllInfo(INativeSymbolStorage storage) : base(storage)
            {
            }

            public override IEnumerable<NativeName> GetInitialData() => Storage.NativeNames;

            protected override bool ShouldAllowCore(NativeName name)
            {
                if (!ShowInvalidData && name.Kind == NativeNameKind.Constant)
                {
                    NativeConstant constant;
                    if (Storage.TryGetGlobalSymbol(name, out constant) && !ConstantsInfo.IsValidConstant(constant))
                    {
                        return false;
                    }
                }

                return base.ShouldAllowCore(name);
            }
        }

        #endregion

    }
}
