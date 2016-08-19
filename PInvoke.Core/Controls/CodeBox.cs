// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PInvoke.Controls
{
    public partial class CodeBox
    {
        public RichTextBox RichTextBox
        {
            get { return m_box; }
        }

        public string Code
        {
            get { return m_box.Text; }
            set
            {
                m_box.Text = value;
                m_box.ScrollToCaret();
            }
        }


        public CodeBox()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            Color color = m_box.BackColor;
            m_box.ReadOnly = true;
            m_box.BackColor = color;
        }

        public void SelectAll()
        {
            m_box.SelectAll();
        }

        public void Copy()
        {
            m_box.Copy();
        }

        private void OnCopyClick(System.Object sender, System.EventArgs e)
        {
            m_box.Copy();
        }

        private void OnSelectAllClick(System.Object sender, System.EventArgs e)
        {
            m_box.SelectAll();
        }
    }
}
