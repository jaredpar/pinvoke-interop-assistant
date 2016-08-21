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

namespace PInvoke.Controls
{
    public partial class SelectSymbolDialog
    {
        private NativeStorage _ns;
        private SearchDataGrid _searchGrid;

        private NativeSymbolBag _bag;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NativeStorage NativeStorage
        {
            get { return _ns; }
            set
            {
                _ns = value;
                _searchGrid.NativeStorage = value;
            }
        }

        public SearchKind SearchKind
        {
            get { return _searchGrid.SearchKind; }
            set
            {
                _searchGrid.SearchKind = value;
                switch (value)
                {
                    case PInvoke.Controls.SearchKind.Constant:
                        this.Name = "Select a Constant";
                        this.Label1.Text = "Constant Name";
                        break;
                    case PInvoke.Controls.SearchKind.Procedure:
                        this.Name = "Select a Procedure";
                        this.Label1.Text = "Procedure Name";
                        break;
                    case PInvoke.Controls.SearchKind.Type:
                        this.Name = "Select a Type";
                        this.Label1.Text = "Type Name";
                        break;
                    case PInvoke.Controls.SearchKind.All:
                        this.Name = "Select a Symbol";
                        this.Label1.Text = "Name";
                        break;
                    case PInvoke.Controls.SearchKind.None:
                        break;
                        // Do nothing
                }
            }
        }

        public NativeSymbolBag SelectedSymbolBag
        {
            get { return _bag; }
        }

        public SelectSymbolDialog() : this(SearchKind.Constant, NativeStorage.DefaultInstance)
        {
        }

        public SelectSymbolDialog(SearchKind kind, NativeStorage ns)
        {
            InitializeComponent();
            _ns = ns;
            _searchGrid = new SearchDataGrid();
            _searchGrid.Dock = DockStyle.Fill;
            TableLayoutPanel1.Controls.Add(_searchGrid, 1, 1);

            SearchKind = kind;
        }

        private void OnNameChanged(System.Object sender, System.EventArgs e)
        {
            if (_searchGrid != null)
            {
                _searchGrid.SearchText = m_nameTb.Text;
            }
        }

        private void m_okBtn_Click(System.Object sender, System.EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            if (_searchGrid != null)
            {
                this._bag = _searchGrid.SelectedSymbolBag;
            }
            this.Close();
        }
    }

}
