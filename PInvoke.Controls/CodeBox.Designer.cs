// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
namespace PInvoke.Controls
{
    partial class CodeBox : System.Windows.Forms.UserControl
    {
        //UserControl overrides dispose to clean up the component list.
        [System.Diagnostics.DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //Required by the Windows Form Designer

        private System.ComponentModel.IContainer components;
        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.  
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.m_box = new System.Windows.Forms.RichTextBox();
            this.m_menuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SelectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_menuStrip.SuspendLayout();
            this.SuspendLayout();
            //
            //m_box
            //
            this.m_box.ContextMenuStrip = this.m_menuStrip;
            this.m_box.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_box.Location = new System.Drawing.Point(0, 0);
            this.m_box.Name = "m_box";
            this.m_box.Size = new System.Drawing.Size(436, 365);
            this.m_box.TabIndex = 0;
            this.m_box.Text = "";
            //
            //m_menuStrip
            //
            this.m_menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.CopyToolStripMenuItem,
                this.SelectAllToolStripMenuItem
            });
            this.m_menuStrip.Name = "m_menuStrip";
            this.m_menuStrip.Size = new System.Drawing.Size(129, 48);
            //
            //CopyToolStripMenuItem
            //
            this.CopyToolStripMenuItem.Name = "CopyToolStripMenuItem";
            this.CopyToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.CopyToolStripMenuItem.Text = "Copy";
            //
            //SelectAllToolStripMenuItem
            //
            this.SelectAllToolStripMenuItem.Name = "SelectAllToolStripMenuItem";
            this.SelectAllToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.SelectAllToolStripMenuItem.Text = "Select All";
            //
            //CodeBox
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_box);
            this.Name = "CodeBox";
            this.Size = new System.Drawing.Size(436, 365);
            this.m_menuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        internal System.Windows.Forms.RichTextBox m_box;
        internal System.Windows.Forms.ContextMenuStrip m_menuStrip;
        internal System.Windows.Forms.ToolStripMenuItem CopyToolStripMenuItem;

        internal System.Windows.Forms.ToolStripMenuItem SelectAllToolStripMenuItem;
    }
}
