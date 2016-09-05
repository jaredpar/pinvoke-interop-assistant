// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;

namespace PInvoke.Controls
{
    public partial class SelectSymbolDialog
    {
        private INativeSymbolStorage _storage;
        private SearchDataGrid _searchGrid;
        private NativeSymbolBag _bag;

        public INativeSymbolStorage Storage
        {
            get { return _storage; }
            set
            {
                _storage = value;
                _searchGrid.Storage = value;
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
                    case SearchKind.Constant:
                        this.Name = "Select a Constant";
                        this.Label1.Text = "Constant Name";
                        break;
                    case SearchKind.Procedure:
                        this.Name = "Select a Procedure";
                        this.Label1.Text = "Procedure Name";
                        break;
                    case SearchKind.Type:
                        this.Name = "Select a Type";
                        this.Label1.Text = "Type Name";
                        break;
                    case SearchKind.All:
                        this.Name = "Select a Symbol";
                        this.Label1.Text = "Name";
                        break;
                    case SearchKind.None:
                        break;
                        // Do nothing
                }
            }
        }

        public NativeSymbolBag SelectedSymbolBag
        {
            get { return _bag; }
        }

        public SelectSymbolDialog() : this(SearchKind.Constant, new BasicSymbolStorage())
        {

        }

        public SelectSymbolDialog(SearchKind kind, INativeSymbolStorage ns)
        {
            InitializeComponent();
            _storage = ns;
            _searchGrid = new SearchDataGrid();
            _searchGrid.Dock = DockStyle.Fill;
            TableLayoutPanel1.Controls.Add(_searchGrid, 1, 1);

            SearchKind = kind;
        }

        private void OnNameChanged(object sender, EventArgs e)
        {
            if (_searchGrid != null)
            {
                _searchGrid.SearchText = m_nameTb.Text;
            }
        }

        private void m_okBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            if (_searchGrid != null)
            {
                _bag = _searchGrid.SelectedSymbolBag;
            }
            Close();
        }
    }

}
