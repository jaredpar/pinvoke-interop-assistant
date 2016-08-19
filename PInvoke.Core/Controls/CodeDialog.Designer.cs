
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace Controls
{

	[Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
	partial class CodeDialog : System.Windows.Forms.Form
	{

		//Form overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]
		protected override void Dispose(bool disposing)
		{
			try {
				if (disposing && components != null) {
					components.Dispose();
				}
			} finally {
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
			this.m_okBtn = new System.Windows.Forms.Button();
			this.m_cancelBtn = new System.Windows.Forms.Button();
			this.Label1 = new System.Windows.Forms.Label();
			this.m_codeTb = new System.Windows.Forms.TextBox();
			this.Label2 = new System.Windows.Forms.Label();
			this.m_errorsTb = new System.Windows.Forms.TextBox();
			this.m_bgWorker = new System.ComponentModel.BackgroundWorker();
			this.TableLayoutPanel1.SuspendLayout();
			this.FlowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			//
			//TableLayoutPanel1
			//
			this.TableLayoutPanel1.ColumnCount = 2;
			this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
			this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TableLayoutPanel1.Controls.Add(this.FlowLayoutPanel1, 1, 0);
			this.TableLayoutPanel1.Controls.Add(this.Label1, 0, 0);
			this.TableLayoutPanel1.Controls.Add(this.m_codeTb, 0, 1);
			this.TableLayoutPanel1.Controls.Add(this.Label2, 0, 2);
			this.TableLayoutPanel1.Controls.Add(this.m_errorsTb, 0, 3);
			this.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.TableLayoutPanel1.Name = "TableLayoutPanel1";
			this.TableLayoutPanel1.RowCount = 4;
			this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70f));
			this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30f));
			this.TableLayoutPanel1.Size = new System.Drawing.Size(644, 460);
			this.TableLayoutPanel1.TabIndex = 0;
			//
			//FlowLayoutPanel1
			//
			this.FlowLayoutPanel1.AutoSize = true;
			this.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.FlowLayoutPanel1.Controls.Add(this.m_okBtn);
			this.FlowLayoutPanel1.Controls.Add(this.m_cancelBtn);
			this.FlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.FlowLayoutPanel1.Location = new System.Drawing.Point(560, 3);
			this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
			this.TableLayoutPanel1.SetRowSpan(this.FlowLayoutPanel1, 2);
			this.FlowLayoutPanel1.Size = new System.Drawing.Size(81, 58);
			this.FlowLayoutPanel1.TabIndex = 1;
			//
			//m_okBtn
			//
			this.m_okBtn.Location = new System.Drawing.Point(3, 3);
			this.m_okBtn.Name = "m_okBtn";
			this.m_okBtn.Size = new System.Drawing.Size(75, 23);
			this.m_okBtn.TabIndex = 0;
			this.m_okBtn.Text = "OK";
			this.m_okBtn.UseVisualStyleBackColor = true;
			//
			//m_cancelBtn
			//
			this.m_cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_cancelBtn.Location = new System.Drawing.Point(3, 32);
			this.m_cancelBtn.Name = "m_cancelBtn";
			this.m_cancelBtn.Size = new System.Drawing.Size(75, 23);
			this.m_cancelBtn.TabIndex = 1;
			this.m_cancelBtn.Text = "Cancel";
			this.m_cancelBtn.UseVisualStyleBackColor = true;
			//
			//Label1
			//
			this.Label1.AutoSize = true;
			this.Label1.Location = new System.Drawing.Point(3, 0);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(60, 13);
			this.Label1.TabIndex = 0;
			this.Label1.Text = "Enter Code";
			//
			//m_codeTb
			//
			this.m_codeTb.AcceptsReturn = true;
			this.m_codeTb.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_codeTb.Location = new System.Drawing.Point(3, 16);
			this.m_codeTb.Multiline = true;
			this.m_codeTb.Name = "m_codeTb";
			this.m_codeTb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.m_codeTb.Size = new System.Drawing.Size(551, 297);
			this.m_codeTb.TabIndex = 1;
			//
			//Label2
			//
			this.Label2.AutoSize = true;
			this.Label2.Location = new System.Drawing.Point(3, 316);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(103, 13);
			this.Label2.TabIndex = 2;
			this.Label2.Text = "Errors and Warnings";
			//
			//m_errorsTb
			//
			this.m_errorsTb.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_errorsTb.Location = new System.Drawing.Point(3, 332);
			this.m_errorsTb.Multiline = true;
			this.m_errorsTb.Name = "m_errorsTb";
			this.m_errorsTb.ReadOnly = true;
			this.m_errorsTb.Size = new System.Drawing.Size(551, 125);
			this.m_errorsTb.TabIndex = 3;
			//
			//m_bgWorker
			//
			//
			//CodeDialog
			//
			this.AcceptButton = this.m_okBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_cancelBtn;
			this.ClientSize = new System.Drawing.Size(644, 460);
			this.Controls.Add(this.TableLayoutPanel1);
			this.Name = "CodeDialog";
			this.Text = "Code Dialog";
			this.TableLayoutPanel1.ResumeLayout(false);
			this.TableLayoutPanel1.PerformLayout();
			this.FlowLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel1;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.TextBox m_codeTb;
		internal System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
		internal System.Windows.Forms.Button m_okBtn;
		internal System.Windows.Forms.Button m_cancelBtn;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.TextBox m_errorsTb;
		internal System.ComponentModel.BackgroundWorker m_bgWorker;
	}


}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
