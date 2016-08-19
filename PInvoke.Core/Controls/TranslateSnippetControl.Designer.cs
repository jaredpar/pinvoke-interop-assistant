
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace PInvoke.Controls
{
    partial class TranslateSnippetControl : System.Windows.Forms.UserControl, ISignatureImportControl
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
            this.TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.m_autoGenerateBtn = new System.Windows.Forms.CheckBox();
            this.m_generateBtn = new System.Windows.Forms.Button();
            this.m_errorsTb = new System.Windows.Forms.RichTextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.m_langTypeCb = new System.Windows.Forms.ComboBox();
            this.m_nativeCodeTb = new System.Windows.Forms.TextBox();
            this.SplitContainer1 = new System.Windows.Forms.SplitContainer();
            this.m_bgWorker = new System.ComponentModel.BackgroundWorker();
            this.m_managedCodeBox = new PInvoke.Controls.CodeBox();
            this.TableLayoutPanel1.SuspendLayout();
            this.FlowLayoutPanel1.SuspendLayout();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            this.SuspendLayout();
            //
            //TableLayoutPanel1
            //
            this.TableLayoutPanel1.ColumnCount = 2;
            this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 62f));
            this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
            this.TableLayoutPanel1.Controls.Add(this.FlowLayoutPanel1, 0, 5);
            this.TableLayoutPanel1.Controls.Add(this.m_errorsTb, 0, 4);
            this.TableLayoutPanel1.Controls.Add(this.Label1, 0, 1);
            this.TableLayoutPanel1.Controls.Add(this.Label2, 0, 3);
            this.TableLayoutPanel1.Controls.Add(this.Label3, 0, 0);
            this.TableLayoutPanel1.Controls.Add(this.m_langTypeCb, 1, 0);
            this.TableLayoutPanel1.Controls.Add(this.m_nativeCodeTb, 0, 2);
            this.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.TableLayoutPanel1.Name = "TableLayoutPanel1";
            this.TableLayoutPanel1.RowCount = 6;
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableLayoutPanel1.Size = new System.Drawing.Size(318, 450);
            this.TableLayoutPanel1.TabIndex = 0;
            //
            //FlowLayoutPanel1
            //
            this.FlowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.FlowLayoutPanel1.AutoSize = true;
            this.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableLayoutPanel1.SetColumnSpan(this.FlowLayoutPanel1, 2);
            this.FlowLayoutPanel1.Controls.Add(this.m_autoGenerateBtn);
            this.FlowLayoutPanel1.Controls.Add(this.m_generateBtn);
            this.FlowLayoutPanel1.Location = new System.Drawing.Point(136, 421);
            this.FlowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            this.FlowLayoutPanel1.Size = new System.Drawing.Size(182, 29);
            this.FlowLayoutPanel1.TabIndex = 7;
            //
            //m_autoGenerateBtn
            //
            this.m_autoGenerateBtn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_autoGenerateBtn.AutoSize = true;
            this.m_autoGenerateBtn.Location = new System.Drawing.Point(3, 6);
            this.m_autoGenerateBtn.Name = "m_autoGenerateBtn";
            this.m_autoGenerateBtn.Size = new System.Drawing.Size(95, 17);
            this.m_autoGenerateBtn.TabIndex = 8;
            this.m_autoGenerateBtn.Text = "&Auto Generate";
            this.m_autoGenerateBtn.UseVisualStyleBackColor = true;
            //
            //m_generateBtn
            //
            this.m_generateBtn.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_generateBtn.Location = new System.Drawing.Point(104, 3);
            this.m_generateBtn.Name = "m_generateBtn";
            this.m_generateBtn.Size = new System.Drawing.Size(75, 23);
            this.m_generateBtn.TabIndex = 9;
            this.m_generateBtn.Text = "&Generate";
            this.m_generateBtn.UseVisualStyleBackColor = true;
            //
            //m_errorsTb
            //
            this.TableLayoutPanel1.SetColumnSpan(this.m_errorsTb, 2);
            this.m_errorsTb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_errorsTb.Location = new System.Drawing.Point(3, 240);
            this.m_errorsTb.Name = "m_errorsTb";
            this.m_errorsTb.ReadOnly = true;
            this.m_errorsTb.Size = new System.Drawing.Size(312, 178);
            this.m_errorsTb.TabIndex = 6;
            this.m_errorsTb.Text = "";
            //
            //Label1
            //
            this.Label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Label1.AutoSize = true;
            this.TableLayoutPanel1.SetColumnSpan(this.Label1, 2);
            this.Label1.Location = new System.Drawing.Point(3, 27);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(105, 13);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "&Native Code Snippet";
            //
            //Label2
            //
            this.Label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Label2.AutoSize = true;
            this.TableLayoutPanel1.SetColumnSpan(this.Label2, 2);
            this.Label2.Location = new System.Drawing.Point(3, 224);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(34, 13);
            this.Label2.TabIndex = 5;
            this.Label2.Text = "&Errors";
            //
            //Label3
            //
            this.Label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(3, 7);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(55, 13);
            this.Label3.TabIndex = 1;
            this.Label3.Text = "&Language";
            //
            //m_langTypeCb
            //
            this.m_langTypeCb.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right);
            this.m_langTypeCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_langTypeCb.FormattingEnabled = true;
            this.m_langTypeCb.Location = new System.Drawing.Point(65, 3);
            this.m_langTypeCb.Name = "m_langTypeCb";
            this.m_langTypeCb.Size = new System.Drawing.Size(250, 21);
            this.m_langTypeCb.TabIndex = 2;
            //
            //m_nativeCodeTb
            //
            this.TableLayoutPanel1.SetColumnSpan(this.m_nativeCodeTb, 2);
            this.m_nativeCodeTb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_nativeCodeTb.Location = new System.Drawing.Point(3, 43);
            this.m_nativeCodeTb.Multiline = true;
            this.m_nativeCodeTb.Name = "m_nativeCodeTb";
            this.m_nativeCodeTb.Size = new System.Drawing.Size(312, 178);
            this.m_nativeCodeTb.TabIndex = 4;
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
            this.SplitContainer1.Panel2.Controls.Add(this.m_managedCodeBox);
            this.SplitContainer1.Size = new System.Drawing.Size(673, 450);
            this.SplitContainer1.SplitterDistance = 318;
            this.SplitContainer1.TabIndex = 10;
            //
            //m_bgWorker
            //
            //
            //m_managedCodeBox
            //
            this.m_managedCodeBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_managedCodeBox.Location = new System.Drawing.Point(0, 0);
            this.m_managedCodeBox.Name = "m_managedCodeBox";
            this.m_managedCodeBox.Size = new System.Drawing.Size(351, 450);
            this.m_managedCodeBox.TabIndex = 11;
            //
            //TranslateSnippetControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SplitContainer1);
            this.Name = "TranslateSnippetControl";
            this.Size = new System.Drawing.Size(673, 450);
            this.TableLayoutPanel1.ResumeLayout(false);
            this.TableLayoutPanel1.PerformLayout();
            this.FlowLayoutPanel1.ResumeLayout(false);
            this.FlowLayoutPanel1.PerformLayout();
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel2.ResumeLayout(false);
            this.SplitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel1;
        internal System.Windows.Forms.RichTextBox m_errorsTb;
        internal System.Windows.Forms.Button m_generateBtn;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.SplitContainer SplitContainer1;
        internal PInvoke.Controls.CodeBox m_managedCodeBox;
        internal System.ComponentModel.BackgroundWorker m_bgWorker;
        internal System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
        internal System.Windows.Forms.CheckBox m_autoGenerateBtn;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.ComboBox m_langTypeCb;

        internal System.Windows.Forms.TextBox m_nativeCodeTb;
    }

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
