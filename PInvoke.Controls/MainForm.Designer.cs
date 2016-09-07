namespace PInvoke.Controls
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            PInvoke.BasicSymbolStorage basicSymbolStorage1 = new PInvoke.BasicSymbolStorage();
            PInvoke.BasicSymbolStorage basicSymbolStorage2 = new PInvoke.BasicSymbolStorage();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bit32TargetPlatformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bit64TargetPlatformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.unicodeTargetPlatformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aNSITargetPlatformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.windowsTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plainCTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.marshalDirectionAnnotationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dialogFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pinvokeSearchTabPage = new System.Windows.Forms.TabPage();
            this.symbolDisplay = new PInvoke.Controls.SymbolDisplayControl();
            this.pinvokeSnippetTabPage = new System.Windows.Forms.TabPage();
            this.snippetDisplay = new PInvoke.Controls.TranslateSnippetControl();
            this.statusStrip.SuspendLayout();
            this.dialogFlowLayoutPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.pinvokeSearchTabPage.SuspendLayout();
            this.pinvokeSnippetTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.White;
            this.imageList.Images.SetKeyName(0, "field.bmp");
            this.imageList.Images.SetKeyName(1, "classa.bmp");
            this.imageList.Images.SetKeyName(2, "classi.bmp");
            this.imageList.Images.SetKeyName(3, "classv.bmp");
            this.imageList.Images.SetKeyName(4, "method.bmp");
            this.imageList.Images.SetKeyName(5, "staticmethod.bmp");
            this.imageList.Images.SetKeyName(6, "namespace.bmp");
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Enabled = false;
            this.buttonCancel.Location = new System.Drawing.Point(3, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(84, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.AutoSize = false;
            this.statusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusStripLabel,
            this.statusStripProgressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 441);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(767, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusStripLabel
            // 
            this.statusStripLabel.Name = "statusStripLabel";
            this.statusStripLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // statusStripProgressBar
            // 
            this.statusStripProgressBar.Name = "statusStripProgressBar";
            this.statusStripProgressBar.Size = new System.Drawing.Size(100, 16);
            this.statusStripProgressBar.Visible = false;
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(6, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // bit32TargetPlatformToolStripMenuItem
            // 
            this.bit32TargetPlatformToolStripMenuItem.Name = "bit32TargetPlatformToolStripMenuItem";
            this.bit32TargetPlatformToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // bit64TargetPlatformToolStripMenuItem
            // 
            this.bit64TargetPlatformToolStripMenuItem.Name = "bit64TargetPlatformToolStripMenuItem";
            this.bit64TargetPlatformToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(6, 6);
            // 
            // unicodeTargetPlatformToolStripMenuItem
            // 
            this.unicodeTargetPlatformToolStripMenuItem.Name = "unicodeTargetPlatformToolStripMenuItem";
            this.unicodeTargetPlatformToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // aNSITargetPlatformToolStripMenuItem
            // 
            this.aNSITargetPlatformToolStripMenuItem.Name = "aNSITargetPlatformToolStripMenuItem";
            this.aNSITargetPlatformToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(6, 6);
            // 
            // windowsTypesToolStripMenuItem
            // 
            this.windowsTypesToolStripMenuItem.Name = "windowsTypesToolStripMenuItem";
            this.windowsTypesToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // plainCTypesToolStripMenuItem
            // 
            this.plainCTypesToolStripMenuItem.Name = "plainCTypesToolStripMenuItem";
            this.plainCTypesToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(6, 6);
            // 
            // marshalDirectionAnnotationsToolStripMenuItem
            // 
            this.marshalDirectionAnnotationsToolStripMenuItem.Name = "marshalDirectionAnnotationsToolStripMenuItem";
            this.marshalDirectionAnnotationsToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // showAllToolStripMenuItem
            // 
            this.showAllToolStripMenuItem.Name = "showAllToolStripMenuItem";
            this.showAllToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            this.contentsToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // dialogFlowLayoutPanel
            // 
            this.dialogFlowLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.dialogFlowLayoutPanel.AutoSize = true;
            this.dialogFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.dialogFlowLayoutPanel.Controls.Add(this.buttonCancel);
            this.dialogFlowLayoutPanel.Controls.Add(this.buttonOK);
            this.dialogFlowLayoutPanel.Location = new System.Drawing.Point(605, 412);
            this.dialogFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.dialogFlowLayoutPanel.Name = "dialogFlowLayoutPanel";
            this.dialogFlowLayoutPanel.Size = new System.Drawing.Size(162, 29);
            this.dialogFlowLayoutPanel.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dialogFlowLayoutPanel, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(767, 441);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pinvokeSearchTabPage);
            this.tabControl1.Controls.Add(this.pinvokeSnippetTabPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(761, 406);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.OnTabPageChanged);
            // 
            // pinvokeSearchTabPage
            // 
            this.pinvokeSearchTabPage.Controls.Add(this.symbolDisplay);
            this.pinvokeSearchTabPage.Location = new System.Drawing.Point(4, 22);
            this.pinvokeSearchTabPage.Name = "pinvokeSearchTabPage";
            this.pinvokeSearchTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.pinvokeSearchTabPage.Size = new System.Drawing.Size(753, 380);
            this.pinvokeSearchTabPage.TabIndex = 1;
            this.pinvokeSearchTabPage.Text = "SigImp Search";
            this.pinvokeSearchTabPage.UseVisualStyleBackColor = true;
            // 
            // symbolDisplay
            // 
            this.symbolDisplay.AutoGenerate = false;
            this.symbolDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.symbolDisplay.LanguageType = PInvoke.Transform.LanguageType.VisualBasic;
            this.symbolDisplay.Location = new System.Drawing.Point(3, 3);
            this.symbolDisplay.Name = "symbolDisplay";
            this.symbolDisplay.Size = new System.Drawing.Size(747, 374);
            this.symbolDisplay.Storage = basicSymbolStorage1;
            this.symbolDisplay.TabIndex = 0;
            // 
            // pinvokeSnippetTabPage
            // 
            this.pinvokeSnippetTabPage.Controls.Add(this.snippetDisplay);
            this.pinvokeSnippetTabPage.Location = new System.Drawing.Point(4, 22);
            this.pinvokeSnippetTabPage.Name = "pinvokeSnippetTabPage";
            this.pinvokeSnippetTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.pinvokeSnippetTabPage.Size = new System.Drawing.Size(753, 356);
            this.pinvokeSnippetTabPage.TabIndex = 2;
            this.pinvokeSnippetTabPage.Text = "SigImp Translate Snippet";
            this.pinvokeSnippetTabPage.UseVisualStyleBackColor = true;
            // 
            // snippetDisplay
            // 
            this.snippetDisplay.AutoGenerate = false;
            this.snippetDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.snippetDisplay.LanguageType = PInvoke.Transform.LanguageType.VisualBasic;
            this.snippetDisplay.Location = new System.Drawing.Point(3, 3);
            this.snippetDisplay.Name = "snippetDisplay";
            this.snippetDisplay.Size = new System.Drawing.Size(747, 350);
            this.snippetDisplay.Storage = basicSymbolStorage2;
            this.snippetDisplay.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(767, 463);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(250, 200);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Tag = "Select a managed method...";
            this.Text = "P/Invoke Interop Assistant version n.n.nnnn.nnnnn";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.dialogFlowLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.pinvokeSearchTabPage.ResumeLayout(false);
            this.pinvokeSnippetTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ToolStripMenuItem bit32TargetPlatformToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bit64TargetPlatformToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem unicodeTargetPlatformToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aNSITargetPlatformToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem windowsTypesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem plainCTypesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem marshalDirectionAnnotationsToolStripMenuItem;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.FlowLayoutPanel dialogFlowLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pinvokeSearchTabPage;
        private PInvoke.Controls.SymbolDisplayControl symbolDisplay;
        private PInvoke.Controls.TranslateSnippetControl snippetDisplay;
        private System.Windows.Forms.TabPage pinvokeSnippetTabPage;
        private System.Windows.Forms.ToolStripMenuItem showAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripStatusLabel statusStripLabel;
        private System.Windows.Forms.ToolStripProgressBar statusStripProgressBar;
    }
}

