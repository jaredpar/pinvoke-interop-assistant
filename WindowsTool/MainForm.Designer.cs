namespace WindowsTool
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

            this.DisposeHelper(disposing);
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
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("<no assembly is open>");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainerOuter = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.splitContainerInner = new System.Windows.Forms.SplitContainer();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.richTextBoxCode = new System.Windows.Forms.RichTextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelAdditionalInfo = new System.Windows.Forms.Label();
            this.richTextBoxMessages = new System.Windows.Forms.RichTextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItemExp = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItemImp = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItemExp = new System.Windows.Forms.ToolStripMenuItem();
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
            this.optionsToolStripMenuItemImp = new System.Windows.Forms.ToolStripMenuItem();
            this.showAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.dialogFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.reversePInvokeTabPage = new System.Windows.Forms.TabPage();
            this.pinvokeSearchTabPage = new System.Windows.Forms.TabPage();
            this.pinvokeSnippetTabPage = new System.Windows.Forms.TabPage();
            this.symbolDisplay = new PInvoke.Controls.SymbolDisplayControl();
            this.snippetDisplay = new PInvoke.Controls.TranslateSnippetControl();
            this.statusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.splitContainerOuter.Panel1.SuspendLayout();
            this.splitContainerOuter.Panel2.SuspendLayout();
            this.splitContainerOuter.SuspendLayout();
            this.splitContainerInner.Panel1.SuspendLayout();
            this.splitContainerInner.Panel2.SuspendLayout();
            this.splitContainerInner.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.dialogFlowLayoutPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.reversePInvokeTabPage.SuspendLayout();
            this.pinvokeSearchTabPage.SuspendLayout();
            this.pinvokeSnippetTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerOuter
            // 
            this.splitContainerOuter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainerOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerOuter.Location = new System.Drawing.Point(3, 3);
            this.splitContainerOuter.MinimumSize = new System.Drawing.Size(50, 50);
            this.splitContainerOuter.Name = "splitContainerOuter";
            // 
            // splitContainerOuter.Panel1
            // 
            this.splitContainerOuter.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainerOuter.Panel2
            // 
            this.splitContainerOuter.Panel2.Controls.Add(this.splitContainerInner);
            this.splitContainerOuter.Size = new System.Drawing.Size(747, 350);
            this.splitContainerOuter.SplitterDistance = 278;
            this.splitContainerOuter.TabIndex = 3;
            // 
            // treeView
            // 
            this.treeView.AllowDrop = true;
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.imageList;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            treeNode2.Name = "Node0";
            treeNode2.Text = "<no assembly is open>";
            this.treeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2});
            this.treeView.SelectedImageIndex = 0;
            this.treeView.Size = new System.Drawing.Size(274, 346);
            this.treeView.TabIndex = 2;
            this.treeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_DragDrop);
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_DragEnter);
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
            // splitContainerInner
            // 
            this.splitContainerInner.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainerInner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerInner.Location = new System.Drawing.Point(0, 0);
            this.splitContainerInner.MinimumSize = new System.Drawing.Size(50, 50);
            this.splitContainerInner.Name = "splitContainerInner";
            this.splitContainerInner.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerInner.Panel1
            // 
            this.splitContainerInner.Panel1.Controls.Add(this.pictureBox2);
            this.splitContainerInner.Panel1.Controls.Add(this.label1);
            this.splitContainerInner.Panel1.Controls.Add(this.richTextBoxCode);
            // 
            // splitContainerInner.Panel2
            // 
            this.splitContainerInner.Panel2.Controls.Add(this.pictureBox1);
            this.splitContainerInner.Panel2.Controls.Add(this.labelAdditionalInfo);
            this.splitContainerInner.Panel2.Controls.Add(this.richTextBoxMessages);
            this.splitContainerInner.Size = new System.Drawing.Size(465, 350);
            this.splitContainerInner.SplitterDistance = 164;
            this.splitContainerInner.TabIndex = 6;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Location = new System.Drawing.Point(0, 20);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(462, 1);
            this.pictureBox2.TabIndex = 3;
            this.pictureBox2.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Unmanaged &signature:";
            // 
            // richTextBoxCode
            // 
            this.richTextBoxCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxCode.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBoxCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxCode.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxCode.Location = new System.Drawing.Point(0, 21);
            this.richTextBoxCode.Name = "richTextBoxCode";
            this.richTextBoxCode.ReadOnly = true;
            this.richTextBoxCode.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBoxCode.Size = new System.Drawing.Size(461, 139);
            this.richTextBoxCode.TabIndex = 5;
            this.richTextBoxCode.Text = "";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(0, 20);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(462, 1);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // labelAdditionalInfo
            // 
            this.labelAdditionalInfo.AutoSize = true;
            this.labelAdditionalInfo.Location = new System.Drawing.Point(4, 4);
            this.labelAdditionalInfo.Name = "labelAdditionalInfo";
            this.labelAdditionalInfo.Size = new System.Drawing.Size(110, 13);
            this.labelAdditionalInfo.TabIndex = 7;
            this.labelAdditionalInfo.Text = "Additional &information:";
            // 
            // richTextBoxMessages
            // 
            this.richTextBoxMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxMessages.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBoxMessages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxMessages.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxMessages.Location = new System.Drawing.Point(0, 21);
            this.richTextBoxMessages.Name = "richTextBoxMessages";
            this.richTextBoxMessages.ReadOnly = true;
            this.richTextBoxMessages.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBoxMessages.Size = new System.Drawing.Size(461, 157);
            this.richTextBoxMessages.TabIndex = 8;
            this.richTextBoxMessages.Text = "";
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
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
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
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItemExp,
            this.fileToolStripMenuItemImp,
            this.editToolStripMenuItem,
            this.optionsToolStripMenuItemExp,
            this.optionsToolStripMenuItemImp,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(767, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItemExp
            // 
            this.fileToolStripMenuItemExp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.reloadToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItemExp.Name = "fileToolStripMenuItemExp";
            this.fileToolStripMenuItemExp.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItemExp.Text = "&File";

            /*
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::WindowsTool.Properties.Resources.openHS;
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+O";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // reloadToolStripMenuItem
            // 
            this.reloadToolStripMenuItem.Enabled = false;
            this.reloadToolStripMenuItem.Image = global::WindowsTool.Properties.Resources.refreshDocViewHS;
            this.reloadToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            this.reloadToolStripMenuItem.ShortcutKeyDisplayString = "F5";
            this.reloadToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.reloadToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.reloadToolStripMenuItem.Text = "&Reload";
            this.reloadToolStripMenuItem.Click += new System.EventHandler(this.reloadToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(160, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // fileToolStripMenuItemImp
            // 
            this.fileToolStripMenuItemImp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem1});
            this.fileToolStripMenuItemImp.Name = "fileToolStripMenuItemImp";
            this.fileToolStripMenuItemImp.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItemImp.Text = "&File";
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            this.exitToolStripMenuItem1.Text = "E&xit";
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.toolStripMenuItem2,
            this.selectAllToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            this.editToolStripMenuItem.DropDownOpening += new System.EventHandler(this.editToolStripMenuItem_DropDownOpening);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Image = global::WindowsTool.Properties.Resources.copyHS;
            this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.copyToolStripMenuItem.Text = "&Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(164, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+A";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItemExp
            // 
            this.optionsToolStripMenuItemExp.Checked = true;
            this.optionsToolStripMenuItemExp.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.optionsToolStripMenuItemExp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bit32TargetPlatformToolStripMenuItem,
            this.bit64TargetPlatformToolStripMenuItem,
            this.toolStripMenuItem3,
            this.unicodeTargetPlatformToolStripMenuItem,
            this.aNSITargetPlatformToolStripMenuItem,
            this.toolStripMenuItem4,
            this.windowsTypesToolStripMenuItem,
            this.plainCTypesToolStripMenuItem,
            this.toolStripMenuItem5,
            this.marshalDirectionAnnotationsToolStripMenuItem});
            this.optionsToolStripMenuItemExp.Name = "optionsToolStripMenuItemExp";
            this.optionsToolStripMenuItemExp.Size = new System.Drawing.Size(56, 20);
            this.optionsToolStripMenuItemExp.Text = "&Options";
            // 
            // bit32TargetPlatformToolStripMenuItem
            // 
            this.bit32TargetPlatformToolStripMenuItem.Checked = true;
            this.bit32TargetPlatformToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.bit32TargetPlatformToolStripMenuItem.Name = "bit32TargetPlatformToolStripMenuItem";
            this.bit32TargetPlatformToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.bit32TargetPlatformToolStripMenuItem.Text = "&32-bit Target Platform";
            this.bit32TargetPlatformToolStripMenuItem.Click += new System.EventHandler(this.bit32TargetPlatformToolStripMenuItem_Click);
            // 
            // bit64TargetPlatformToolStripMenuItem
            // 
            this.bit64TargetPlatformToolStripMenuItem.Name = "bit64TargetPlatformToolStripMenuItem";
            this.bit64TargetPlatformToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.bit64TargetPlatformToolStripMenuItem.Text = "&64-bit Target Platform";
            this.bit64TargetPlatformToolStripMenuItem.Click += new System.EventHandler(this.bit64TargetPlatformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(225, 6);
            // 
            // unicodeTargetPlatformToolStripMenuItem
            // 
            this.unicodeTargetPlatformToolStripMenuItem.Checked = true;
            this.unicodeTargetPlatformToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.unicodeTargetPlatformToolStripMenuItem.Name = "unicodeTargetPlatformToolStripMenuItem";
            this.unicodeTargetPlatformToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.unicodeTargetPlatformToolStripMenuItem.Text = "&Unicode Target Platform";
            this.unicodeTargetPlatformToolStripMenuItem.Click += new System.EventHandler(this.unicodeTargetPlatformToolStripMenuItem_Click);
            // 
            // aNSITargetPlatformToolStripMenuItem
            // 
            this.aNSITargetPlatformToolStripMenuItem.Name = "aNSITargetPlatformToolStripMenuItem";
            this.aNSITargetPlatformToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.aNSITargetPlatformToolStripMenuItem.Text = "&ANSI Target Platform - Win9x";
            this.aNSITargetPlatformToolStripMenuItem.Click += new System.EventHandler(this.aNSITargetPlatformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(225, 6);
            // 
            // windowsTypesToolStripMenuItem
            // 
            this.windowsTypesToolStripMenuItem.Checked = true;
            this.windowsTypesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.windowsTypesToolStripMenuItem.Name = "windowsTypesToolStripMenuItem";
            this.windowsTypesToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.windowsTypesToolStripMenuItem.Text = "&Windows Types";
            this.windowsTypesToolStripMenuItem.Click += new System.EventHandler(this.windowsTypesToolStripMenuItem_Click);
            // 
            // plainCTypesToolStripMenuItem
            // 
            this.plainCTypesToolStripMenuItem.Name = "plainCTypesToolStripMenuItem";
            this.plainCTypesToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.plainCTypesToolStripMenuItem.Text = "Plain &C++ Types";
            this.plainCTypesToolStripMenuItem.Click += new System.EventHandler(this.plainCTypesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(225, 6);
            // 
            // marshalDirectionAnnotationsToolStripMenuItem
            // 
            this.marshalDirectionAnnotationsToolStripMenuItem.Checked = true;
            this.marshalDirectionAnnotationsToolStripMenuItem.CheckOnClick = true;
            this.marshalDirectionAnnotationsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.marshalDirectionAnnotationsToolStripMenuItem.Name = "marshalDirectionAnnotationsToolStripMenuItem";
            this.marshalDirectionAnnotationsToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.marshalDirectionAnnotationsToolStripMenuItem.Text = "Marshal &Direction Annotations";
            this.marshalDirectionAnnotationsToolStripMenuItem.Click += new System.EventHandler(this.marshalDirectionAnnotationsToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItemImp
            // 
            this.optionsToolStripMenuItemImp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showAllToolStripMenuItem});
            this.optionsToolStripMenuItemImp.Name = "optionsToolStripMenuItemImp";
            this.optionsToolStripMenuItemImp.Size = new System.Drawing.Size(56, 20);
            this.optionsToolStripMenuItemImp.Text = "&Options";
            this.optionsToolStripMenuItemImp.DropDownOpening += new System.EventHandler(this.optionsToolStripMenuItemImp_DropDownOpening);
            // 
            // showAllToolStripMenuItem
            // 
            this.showAllToolStripMenuItem.CheckOnClick = true;
            this.showAllToolStripMenuItem.Name = "showAllToolStripMenuItem";
            this.showAllToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.showAllToolStripMenuItem.Text = "Show &All";
            this.showAllToolStripMenuItem.Click += new System.EventHandler(this.showAllToolStripMenuItem_CheckChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem,
            this.toolStripSeparator1,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            this.contentsToolStripMenuItem.ShortcutKeyDisplayString = "F1";
            this.contentsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.contentsToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.contentsToolStripMenuItem.Text = "&Help";
            this.contentsToolStripMenuItem.Click += new System.EventHandler(this.contentsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(123, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            */
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "*.dll";
            this.openFileDialog.Filter = "Assembly files (*.dll; *.exe)|*.dll;*.exe|Class libraries (*.dll)|*.dll|Executabl" +
                "es (*.exe)|*.exe|All files (*.*)|*.*";
            this.openFileDialog.Title = "Open assembly";
            // 
            // dialogFlowLayoutPanel
            // 
            this.dialogFlowLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.dialogFlowLayoutPanel.AutoSize = true;
            this.dialogFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.dialogFlowLayoutPanel.Controls.Add(this.buttonCancel);
            this.dialogFlowLayoutPanel.Controls.Add(this.buttonOK);
            this.dialogFlowLayoutPanel.Location = new System.Drawing.Point(605, 388);
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(767, 417);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.reversePInvokeTabPage);
            this.tabControl1.Controls.Add(this.pinvokeSearchTabPage);
            this.tabControl1.Controls.Add(this.pinvokeSnippetTabPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(761, 382);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.OnTabPageChanged);
            // 
            // reversePInvokeTabPage
            // 
            this.reversePInvokeTabPage.Controls.Add(this.splitContainerOuter);
            this.reversePInvokeTabPage.Location = new System.Drawing.Point(4, 22);
            this.reversePInvokeTabPage.Name = "reversePInvokeTabPage";
            this.reversePInvokeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.reversePInvokeTabPage.Size = new System.Drawing.Size(753, 356);
            this.reversePInvokeTabPage.TabIndex = 0;
            this.reversePInvokeTabPage.Text = "SigExp";
            this.reversePInvokeTabPage.UseVisualStyleBackColor = true;
            // 
            // pinvokeSearchTabPage
            // 
            this.pinvokeSearchTabPage.Controls.Add(this.symbolDisplay);
            this.pinvokeSearchTabPage.Location = new System.Drawing.Point(4, 22);
            this.pinvokeSearchTabPage.Name = "pinvokeSearchTabPage";
            this.pinvokeSearchTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.pinvokeSearchTabPage.Size = new System.Drawing.Size(753, 356);
            this.pinvokeSearchTabPage.TabIndex = 1;
            this.pinvokeSearchTabPage.Text = "SigImp Search";
            this.pinvokeSearchTabPage.UseVisualStyleBackColor = true;
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
            // symbolDisplay
            // 
            this.symbolDisplay.AutoGenerate = false;
            this.symbolDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.symbolDisplay.LanguageType = PInvoke.Transform.LanguageType.VisualBasic;
            this.symbolDisplay.Location = new System.Drawing.Point(3, 3);
            this.symbolDisplay.Name = "symbolDisplay";
            this.symbolDisplay.Size = new System.Drawing.Size(747, 350);
            this.symbolDisplay.TabIndex = 0;
            // 
            // snippetDisplay
            // 
            this.snippetDisplay.AutoGenerate = false;
            this.snippetDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.snippetDisplay.LanguageType = PInvoke.Transform.LanguageType.VisualBasic;
            this.snippetDisplay.Location = new System.Drawing.Point(3, 3);
            this.snippetDisplay.Name = "snippetDisplay";
            this.snippetDisplay.Size = new System.Drawing.Size(747, 350);
            this.snippetDisplay.TabIndex = 0;
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(767, 463);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(250, 200);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Tag = "Select a managed method...";
            this.Text = "P/Invoke Interop Assistant version n.n.nnnn.nnnnn";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.splitContainerOuter.Panel1.ResumeLayout(false);
            this.splitContainerOuter.Panel2.ResumeLayout(false);
            this.splitContainerOuter.ResumeLayout(false);
            this.splitContainerInner.Panel1.ResumeLayout(false);
            this.splitContainerInner.Panel1.PerformLayout();
            this.splitContainerInner.Panel2.ResumeLayout(false);
            this.splitContainerInner.Panel2.PerformLayout();
            this.splitContainerInner.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.dialogFlowLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.reversePInvokeTabPage.ResumeLayout(false);
            this.pinvokeSearchTabPage.ResumeLayout(false);
            this.pinvokeSnippetTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerOuter;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.SplitContainer splitContainerInner;
        private System.Windows.Forms.RichTextBox richTextBoxCode;
        private System.Windows.Forms.RichTextBox richTextBoxMessages;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItemExp;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelAdditionalInfo;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItemExp;
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
        private System.Windows.Forms.ToolStripMenuItem reloadToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.FlowLayoutPanel dialogFlowLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage reversePInvokeTabPage;
        private System.Windows.Forms.TabPage pinvokeSearchTabPage;
        private PInvoke.Controls.SymbolDisplayControl symbolDisplay;
        private PInvoke.Controls.TranslateSnippetControl snippetDisplay;
        private System.Windows.Forms.TabPage pinvokeSnippetTabPage;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItemImp;
        private System.Windows.Forms.ToolStripMenuItem showAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItemImp;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripStatusLabel statusStripLabel;
        private System.Windows.Forms.ToolStripProgressBar statusStripProgressBar;
    }
}

