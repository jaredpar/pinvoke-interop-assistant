
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace PInvoke.Controls
{
    partial class SymbolDisplayControl : System.Windows.Forms.UserControl, ISignatureImportControl
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
            components = null;

            this.SplitContainer1 = new System.Windows.Forms.SplitContainer();
            this.TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Label1 = new System.Windows.Forms.Label();
            this.m_nameTb = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.m_searchKindCb = new System.Windows.Forms.ComboBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.m_languageCb = new System.Windows.Forms.ComboBox();
            this.FlowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.m_autoGenerateCBox = new System.Windows.Forms.CheckBox();
            this.m_generateBtn = new System.Windows.Forms.Button();
            this.DataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_searchGrid = new PInvoke.Controls.SearchDataGrid();
            this.DataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_codeBox = new PInvoke.Controls.CodeBox();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            this.TableLayoutPanel1.SuspendLayout();
            this.FlowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.m_searchGrid).BeginInit();
            this.SuspendLayout();
            //
            //SplitContainer1
            //
            this.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.SplitContainer1.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer1.Name = "SplitContainer1";
            //
            //SplitContainer1.Panel1
            //
            this.SplitContainer1.Panel1.Controls.Add(this.TableLayoutPanel1);
            this.SplitContainer1.Panel1MinSize = 125;
            //
            //SplitContainer1.Panel2
            //
            this.SplitContainer1.Panel2.Controls.Add(this.m_codeBox);
            this.SplitContainer1.Size = new System.Drawing.Size(887, 579);
            this.SplitContainer1.SplitterDistance = 404;
            this.SplitContainer1.TabIndex = 11;
            //
            //TableLayoutPanel1
            //
            this.TableLayoutPanel1.ColumnCount = 2;
            this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
            this.TableLayoutPanel1.Controls.Add(this.Label1, 0, 0);
            this.TableLayoutPanel1.Controls.Add(this.m_nameTb, 1, 0);
            this.TableLayoutPanel1.Controls.Add(this.Label2, 0, 1);
            this.TableLayoutPanel1.Controls.Add(this.m_searchKindCb, 1, 1);
            this.TableLayoutPanel1.Controls.Add(this.Label3, 0, 2);
            this.TableLayoutPanel1.Controls.Add(this.m_languageCb, 1, 2);
            this.TableLayoutPanel1.Controls.Add(this.m_searchGrid, 0, 4);
            this.TableLayoutPanel1.Controls.Add(this.FlowLayoutPanel2, 0, 4);
            this.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.TableLayoutPanel1.Name = "TableLayoutPanel1";
            this.TableLayoutPanel1.RowCount = 6;
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
            this.TableLayoutPanel1.Size = new System.Drawing.Size(404, 579);
            this.TableLayoutPanel1.TabIndex = 0;
            //
            //Label1
            //
            this.Label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(3, 6);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(35, 13);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "&Name";
            //
            //m_nameTb
            //
            this.m_nameTb.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right);
            this.m_nameTb.Location = new System.Drawing.Point(64, 3);
            this.m_nameTb.Name = "m_nameTb";
            this.m_nameTb.Size = new System.Drawing.Size(337, 20);
            this.m_nameTb.TabIndex = 2;
            //
            //Label2
            //
            this.Label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(3, 33);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(28, 13);
            this.Label2.TabIndex = 3;
            this.Label2.Text = "&Kind";
            //
            //m_searchKindCb
            //
            this.m_searchKindCb.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right);
            this.m_searchKindCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_searchKindCb.FormattingEnabled = true;
            this.m_searchKindCb.Location = new System.Drawing.Point(64, 29);
            this.m_searchKindCb.Name = "m_searchKindCb";
            this.m_searchKindCb.Size = new System.Drawing.Size(337, 21);
            this.m_searchKindCb.TabIndex = 4;
            //
            //Label3
            //
            this.Label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(3, 60);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(55, 13);
            this.Label3.TabIndex = 5;
            this.Label3.Text = "&Language";
            //
            //m_languageCb
            //
            this.m_languageCb.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right);
            this.m_languageCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_languageCb.FormattingEnabled = true;
            this.m_languageCb.Location = new System.Drawing.Point(64, 56);
            this.m_languageCb.Name = "m_languageCb";
            this.m_languageCb.Size = new System.Drawing.Size(337, 21);
            this.m_languageCb.TabIndex = 6;
            //
            //FlowLayoutPanel2
            //
            this.FlowLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.FlowLayoutPanel2.AutoSize = true;
            this.FlowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableLayoutPanel1.SetColumnSpan(this.FlowLayoutPanel2, 2);
            this.FlowLayoutPanel2.Controls.Add(this.m_autoGenerateCBox);
            this.FlowLayoutPanel2.Controls.Add(this.m_generateBtn);
            this.FlowLayoutPanel2.Location = new System.Drawing.Point(222, 80);
            this.FlowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.FlowLayoutPanel2.Name = "FlowLayoutPanel2";
            this.FlowLayoutPanel2.Size = new System.Drawing.Size(182, 29);
            this.FlowLayoutPanel2.TabIndex = 7;
            //
            //m_autoGenerateCBox
            //
            this.m_autoGenerateCBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_autoGenerateCBox.AutoSize = true;
            this.m_autoGenerateCBox.Location = new System.Drawing.Point(3, 6);
            this.m_autoGenerateCBox.Name = "m_autoGenerateCBox";
            this.m_autoGenerateCBox.Size = new System.Drawing.Size(95, 17);
            this.m_autoGenerateCBox.TabIndex = 8;
            this.m_autoGenerateCBox.Text = "&Auto Generate";
            this.m_autoGenerateCBox.UseVisualStyleBackColor = true;
            //
            //m_generateBtn
            //
            this.m_generateBtn.Location = new System.Drawing.Point(104, 3);
            this.m_generateBtn.Name = "m_generateBtn";
            this.m_generateBtn.Size = new System.Drawing.Size(75, 23);
            this.m_generateBtn.TabIndex = 9;
            this.m_generateBtn.Text = "&Generate";
            this.m_generateBtn.UseVisualStyleBackColor = true;
            //
            //DataGridViewTextBoxColumn3
            //
            this.DataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.DataGridViewTextBoxColumn3.HeaderText = "Name";
            this.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3";
            this.DataGridViewTextBoxColumn3.ReadOnly = true;
            this.DataGridViewTextBoxColumn3.Width = 180;
            //
            //DataGridViewTextBoxColumn4
            //
            this.DataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DataGridViewTextBoxColumn4.HeaderText = "Value";
            this.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4";
            this.DataGridViewTextBoxColumn4.ReadOnly = true;
            //
            //DataGridViewTextBoxColumn1
            //
            this.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.DataGridViewTextBoxColumn1.HeaderText = "Name";
            this.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1";
            this.DataGridViewTextBoxColumn1.ReadOnly = true;
            this.DataGridViewTextBoxColumn1.Width = 180;
            //
            //DataGridViewTextBoxColumn2
            //
            this.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DataGridViewTextBoxColumn2.HeaderText = "Value";
            this.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2";
            this.DataGridViewTextBoxColumn2.ReadOnly = true;
            //
            //DataGridViewTextBoxColumn5
            //
            this.DataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.DataGridViewTextBoxColumn5.HeaderText = "Name";
            this.DataGridViewTextBoxColumn5.Name = "DataGridViewTextBoxColumn5";
            this.DataGridViewTextBoxColumn5.ReadOnly = true;
            this.DataGridViewTextBoxColumn5.Width = 180;
            //
            //DataGridViewTextBoxColumn6
            //
            this.DataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DataGridViewTextBoxColumn6.HeaderText = "Value";
            this.DataGridViewTextBoxColumn6.Name = "DataGridViewTextBoxColumn6";
            this.DataGridViewTextBoxColumn6.ReadOnly = true;
            //
            //DataGridViewTextBoxColumn7
            //
            this.DataGridViewTextBoxColumn7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.DataGridViewTextBoxColumn7.HeaderText = "Name";
            this.DataGridViewTextBoxColumn7.Name = "DataGridViewTextBoxColumn7";
            this.DataGridViewTextBoxColumn7.ReadOnly = true;
            this.DataGridViewTextBoxColumn7.Width = 180;
            //
            //DataGridViewTextBoxColumn8
            //
            this.DataGridViewTextBoxColumn8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DataGridViewTextBoxColumn8.HeaderText = "Value";
            this.DataGridViewTextBoxColumn8.MinimumWidth = 100;
            this.DataGridViewTextBoxColumn8.Name = "DataGridViewTextBoxColumn8";
            this.DataGridViewTextBoxColumn8.ReadOnly = true;
            //
            //m_searchGrid
            //
            this.m_searchGrid.AllowUserToAddRows = false;
            this.m_searchGrid.AllowUserToDeleteRows = false;
            this.m_searchGrid.AllowUserToResizeRows = false;
            this.m_searchGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TableLayoutPanel1.SetColumnSpan(this.m_searchGrid, 2);
            this.m_searchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_searchGrid.Location = new System.Drawing.Point(3, 112);
            this.m_searchGrid.Name = "m_searchGrid";
            this.m_searchGrid.ReadOnly = true;
            this.m_searchGrid.RowHeadersVisible = false;
            this.m_searchGrid.SearchKind = PInvoke.Controls.SearchKind.None;
            this.m_searchGrid.SearchText = null;
            this.m_searchGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.m_searchGrid.ShowInvalidData = false;
            this.m_searchGrid.Size = new System.Drawing.Size(398, 464);
            this.m_searchGrid.StandardTab = true;
            this.m_searchGrid.TabIndex = 10;
            this.m_searchGrid.VirtualMode = true;
            //
            //DataGridViewTextBoxColumn9
            //
            this.DataGridViewTextBoxColumn9.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.DataGridViewTextBoxColumn9.HeaderText = "Name";
            this.DataGridViewTextBoxColumn9.Name = "DataGridViewTextBoxColumn9";
            this.DataGridViewTextBoxColumn9.ReadOnly = true;
            this.DataGridViewTextBoxColumn9.Width = 180;
            //
            //DataGridViewTextBoxColumn10
            //
            this.DataGridViewTextBoxColumn10.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DataGridViewTextBoxColumn10.HeaderText = "Value";
            this.DataGridViewTextBoxColumn10.Name = "DataGridViewTextBoxColumn10";
            this.DataGridViewTextBoxColumn10.ReadOnly = true;
            //
            //m_codeBox
            //
            this.m_codeBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_codeBox.Location = new System.Drawing.Point(0, 0);
            this.m_codeBox.Name = "m_codeBox";
            this.m_codeBox.Size = new System.Drawing.Size(479, 579);
            this.m_codeBox.TabIndex = 12;
            //
            //SymbolDisplayControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SplitContainer1);
            this.Name = "SymbolDisplayControl";
            this.Size = new System.Drawing.Size(887, 579);
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel2.ResumeLayout(false);
            this.SplitContainer1.ResumeLayout(false);
            this.TableLayoutPanel1.ResumeLayout(false);
            this.TableLayoutPanel1.PerformLayout();
            this.FlowLayoutPanel2.ResumeLayout(false);
            this.FlowLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.m_searchGrid).EndInit();
            this.ResumeLayout(false);

        }
        internal System.Windows.Forms.SplitContainer SplitContainer1;
        internal PInvoke.Controls.CodeBox m_codeBox;
        internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel1;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.TextBox m_nameTb;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.ComboBox m_searchKindCb;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.ComboBox m_languageCb;
        internal PInvoke.Controls.SearchDataGrid m_searchGrid;
        internal System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel2;
        internal System.Windows.Forms.CheckBox m_autoGenerateCBox;
        internal System.Windows.Forms.Button m_generateBtn;
        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn1;
        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn2;
        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn3;
        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn4;
        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn5;
        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn6;
        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn7;
        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn8;
        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn9;

        internal System.Windows.Forms.DataGridViewTextBoxColumn DataGridViewTextBoxColumn10;
    }
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
