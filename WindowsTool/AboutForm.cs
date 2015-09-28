/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace WindowsTool
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            label.Text = label.Text.Replace("n.n.nnnn.nnnnn", MainForm.ApplicationVersionString);
        }
    }
}
