
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.ComponentModel;
using System.Windows.Forms;
using PInvoke;
using PInvoke.Transform;

namespace PInvoke.Controls
{
    public partial class SymbolDisplayControl
    {

        private NativeStorage _ns;

        private BasicConverter _conv;
        /// <summary>
        /// Kind of search being performed
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SearchKind SearchKind
        {
            get { return m_searchGrid.SearchKind; }
            set
            {
                m_searchGrid.SearchKind = value;
                if (m_searchKindCb.SelectedItem != null)
                {
                    m_searchKindCb.SelectedItem = value;
                }
            }
        }

        public bool AutoGenerate
        {
            get { return m_autoGenerateCBox.Checked; }
            set { m_autoGenerateCBox.Checked = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowAll
        {
            get { return m_searchGrid.ShowInvalidData; }
            set { m_searchGrid.ShowInvalidData = value; }
        }

        public event EventHandler SearchKindChanged;

        public SymbolDisplayControl() : this(new NativeStorage())
        {

        }

        public SymbolDisplayControl(NativeStorage storage)
        {
            _ns = storage;
            _conv = new BasicConverter(LanguageType.VisualBasic, storage);

            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Populate the combo boxes
            m_languageCb.Items.AddRange(EnumUtil.GetAllValuesObject<LanguageType>());
            m_languageCb.SelectedItem = LanguageType.VisualBasic;
            m_searchKindCb.Items.AddRange(PInvoke.EnumUtil.GetAllValuesObjectExcept(SearchKind.None));
            m_searchKindCb.SelectedItem = SearchKind.All;

            // Initialize the values
            OnSearchKindChanged(null, EventArgs.Empty);
            OnLanguageChanged(null, EventArgs.Empty);
        }

        #region "ISignatureImportControl"

        /// <summary>
        /// Language that we are displaying the generated values in
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public LanguageType LanguageType
        {
            get
            {
                if (m_languageCb.SelectedItem == null)
                {
                    return Transform.LanguageType.VisualBasic;
                }

                return (LanguageType)m_languageCb.SelectedItem;
            }
            set { m_languageCb.SelectedItem = value; }
        }

        /// <summary>
        /// NativeStorage instance to use
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NativeStorage NativeStorage
        {
            get { return _ns; }
            set
            {
                _ns = value;
                _conv.NativeStorage = value;
                m_searchGrid.Storage = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Transform.TransformKindFlags TransformKindFlags
        {
            get { return _conv.TransformKindFlags; }
            set { _conv.TransformKindFlags = value; }
        }

        public event EventHandler LanguageTypeChanged;

        /// <summary>
        /// Current displayed managed code
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ManagedCode
        {
            get { return m_codeBox.Text; }
        }

        #endregion

        #region "Event Handlers"

        /// <summary>
        /// When the search kind changes update the grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void OnSearchKindChanged(System.Object sender, System.EventArgs e)
        {
            if (m_searchKindCb.SelectedItem != null)
            {
                SearchKind kind = (SearchKind)m_searchKindCb.SelectedItem;
                if (m_searchGrid != null)
                {
                    m_searchGrid.SearchKind = kind;
                }

                if (SearchKindChanged != null)
                {
                    SearchKindChanged(this, EventArgs.Empty);
                }
            }

        }

        /// <summary>
        /// When the language changes make sure to rebuild the converter as it depends on the current language
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void OnLanguageChanged(System.Object sender, System.EventArgs e)
        {
            // During initialization this can be true
            _conv.LanguageType = this.LanguageType;
            if (LanguageTypeChanged != null)
            {
                LanguageTypeChanged(this, EventArgs.Empty);
            }

            AutoGenerateCode();
        }

        private void OnSearchGridSelectionChanged(System.Object sender, System.EventArgs e)
        {
            AutoGenerateCode();
        }

        private void OnNameChanged(System.Object sender, System.EventArgs e)
        {
            if (m_searchGrid != null)
            {
                m_searchGrid.SearchText = m_nameTb.Text;
            }
        }


        private void OnGenerateClick(System.Object sender, System.EventArgs e)
        {
            if (m_searchGrid.SelectedRows.Count > 400)
            {
                string title = "Generation";
#if DEBUG
				title += " (" + m_searchGrid.SelectedRows.Count + ")";
#endif
                DialogResult result = MessageBox.Show("Generating the output might take a lot of time. Do you want to proceed?", title, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            Cursor oldCursor = this.Cursor;
            try
            {
                Cursor = Cursors.WaitCursor;
                GenerateCode(true);
            }
            finally
            {
                this.Cursor = oldCursor;
            }

        }

        private void OnAutoGenerateClick(System.Object sender, System.EventArgs e)
        {
            AutoGenerateCode();
        }

        private void m_nameTb_KeyDown(System.Object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A & e.Modifiers == Keys.Control)
            {
                m_nameTb.SelectAll();
                e.Handled = true;
            }
        }

        #endregion

        #region "Private Helpers"

        /// <summary>
        /// Regenerate the code if AutoGenerate is checked
        /// </summary>
        /// <remarks></remarks>
        private void AutoGenerateCode()
        {
            if (this.AutoGenerate)
            {
                GenerateCode(false);
            }
        }

        private void GenerateCode(bool force)
        {
            m_codeBox.Text = string.Empty;

            string text = null;
            if (force || m_searchGrid.SelectedRows.Count <= 5)
            {
                text = _conv.ConvertToPInvokeCode(m_searchGrid.SelectedSymbolBag);
            }
            else
            {
                text = "More than 5 rows selected.  Will not autogenerate";
            }

            m_codeBox.Code = text;
        }

        #endregion

    }

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
