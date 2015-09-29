
using System;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using PInvoke;
using PInvoke.Controls;

namespace WindowsTool
{
    public partial class MainForm : Form
    {
        private enum TabMode
        {
            ReversePInvoke,
            PInvokeSearch,
            PInvokeSnippet
        }

        #region ImageIndex

        private enum ImageIndex
        {
            Field = 0,
            Class = 1,
            Interface = 2,
            Structure = 3,
            InstanceMethod = 4,
            StaticMethod = 5,
            Namespace = 6
        }

        #endregion

        #region NodeSorter

        private class NodeSorter : System.Collections.IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                TreeNode tx = (TreeNode)x;
                TreeNode ty = (TreeNode)y;

                ImageIndex ix = (ImageIndex)tx.ImageIndex;
                ImageIndex iy = (ImageIndex)ty.ImageIndex;

                // we want to display all namespaces before all types
                if (ix == ImageIndex.Namespace && iy != ImageIndex.Namespace) return -1;
                if (iy == ImageIndex.Namespace && ix != ImageIndex.Namespace) return 1;

                // we want to display all nested types before all methods
                if (IsType(ix) && !IsType(iy)) return -1;
                if (IsType(iy) && !IsType(ix)) return 1;

                return String.Compare(tx.Text, ty.Text);
            }

            #endregion

            private static bool IsType(ImageIndex ii)
            {
                return (ii == ImageIndex.Class || ii == ImageIndex.Interface || ii == ImageIndex.Structure);
            }
        }

        #endregion

        #region Fields and Properties

        private Properties.Settings _userSettings = Properties.Settings.Default;

        private bool _firstTimeActivated;
        private bool _printing;
        private bool _printPending;

        private string _lastTreePath;
        private string _lastAssemblyPath;

        private bool _nativeStorageSet;
        private NativeStorage _nativeStorage;

        private string _signatureString;

        public string SignatureString
        {
            get
            { return _signatureString; }
        }

        private TabMode Mode
        {
            get
            {
                if (Object.ReferenceEquals(tabControl1.SelectedTab, reversePInvokeTabPage))
                {
                    return TabMode.ReversePInvoke;
                }
                else if (object.ReferenceEquals(tabControl1.SelectedTab, pinvokeSearchTabPage))
                {
                    return TabMode.PInvokeSearch;
                }
                else if (object.ReferenceEquals(tabControl1.SelectedTab, pinvokeSnippetTabPage))
                {
                    return TabMode.PInvokeSnippet;
                }
                else
                {
                    Debug.Fail("Invalid Mode");
                    return TabMode.ReversePInvoke;
                }
            }
        }

        private ISignatureImportControl PInvokeControl
        {
            get
            {
                switch (Mode)
                {
                    case TabMode.PInvokeSnippet:
                        return snippetDisplay;
                    case TabMode.PInvokeSearch:
                        return symbolDisplay;
                    default:
                        return null;
                }
            }
        }

        //private static string applicationVersionString;
        internal static string ApplicationVersionString
        {
            get
            {
                //                if (applicationVersionString == null)
                //                {
                //#if RAZZLE_BUILD
                //                    applicationVersionString = Text.Replace("n.n.nnnn.nnnnn", ThisAssembly.InformationalVersion);
                //#else // RAZZLE_BUILD
                //                AssemblyName lib_assembly_name = typeof(SignatureGenerator.NativeSignature).Assembly.GetName();
                //                applicationVersionString = lib_assembly_name.Version.ToString();
                //#endif // RAZZLE_BUILD
                //                }
                //                Debug.Assert(applicationVersionString != null);
                //                return applicationVersionString;

                // hard-coding 1.0 for the MSDN release
                return "1.0";
            }
        }

        #endregion

        #region Construction

        public MainForm()
            : this(null, false)
        { }

        public MainForm(string assemblyPath)
            : this(assemblyPath, false)
        { }

        public MainForm(string assemblyPath, bool asDialog)
        {
            InitializeComponent();

            if (asDialog)
            {
                buttonCancel.Enabled = true;

                // make it a dialog
                this.StartPosition = FormStartPosition.CenterParent;
                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.ShowInTaskbar = false;

                // remove the Help menu
                menuStrip.Items.RemoveAt(menuStrip.Items.Count - 1);

                // remove the Exit menu item
                ToolStripMenuItem fileDropDown = (ToolStripMenuItem)menuStrip.Items[0];
                fileDropDown.DropDownItems.RemoveAt(fileDropDown.DropDownItems.Count - 1);
                fileDropDown.DropDownItems.RemoveAt(fileDropDown.DropDownItems.Count - 1);
            }
            else
            {
                // Remove the Ok/Cancel panel
                dialogFlowLayoutPanel.Visible = false;
            }

            treeView.PathSeparator = "\0";
            treeView.TreeViewNodeSorter = new NodeSorter();
        }

        #endregion

        #region Dispose

        private void DisposeHelper(bool disposing)
        {
            if (disposing)
            {
                _userSettings.Mode = (int)Mode;
                _userSettings.PInvokeSnippetAutoGenerate = snippetDisplay.AutoGenerate;
                _userSettings.PInvokeSearchAutoGenerate = symbolDisplay.AutoGenerate;
                _userSettings.PInvokeLanguageType = symbolDisplay.LanguageType.ToString();
                _userSettings.PInvokeSearchKind = symbolDisplay.SearchKind.ToString();
                _userSettings.PInvokeShowAll = symbolDisplay.ShowAll;
                _userSettings.Save();
            }
        }

        #endregion

        #region Misc Handlers

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (buttonCancel.Enabled)
            {
                // if we are a dialog, use alternative window caption
                Text = (string)Tag;
            }
            else
            {
                // put the version to window title
                Text = Text.Replace("n.n.nnnn.nnnnn", ApplicationVersionString);
            }

            // Start building the native storage 
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.LoadNativeStorage), this);

            // Load up the correct tab page
            switch ((TabMode)(_userSettings.Mode))
            {
                case TabMode.ReversePInvoke:
                    tabControl1.SelectedTab = reversePInvokeTabPage;
                    break;
                case TabMode.PInvokeSearch:
                    tabControl1.SelectedTab = pinvokeSearchTabPage;
                    break;
                case TabMode.PInvokeSnippet:
                    tabControl1.SelectedTab = pinvokeSnippetTabPage;
                    break;
                default:
                    tabControl1.SelectedTab = reversePInvokeTabPage;
                    break;
            }

            // Show all setting
            showAllToolStripMenuItem.Checked = _userSettings.PInvokeShowAll;
            symbolDisplay.ShowAll = _userSettings.PInvokeShowAll;

            // Disable wrapper method generation
            PInvoke.Transform.TransformKindFlags transformFlags = PInvoke.Transform.TransformKindFlags.All;
            transformFlags &= ~PInvoke.Transform.TransformKindFlags.WrapperMethods;
            symbolDisplay.TransformKindFlags = transformFlags;
            snippetDisplay.TransformKindFlags = transformFlags;

            // Load settings
            symbolDisplay.AutoGenerate = _userSettings.PInvokeSearchAutoGenerate;
            snippetDisplay.AutoGenerate = _userSettings.PInvokeSnippetAutoGenerate;
            symbolDisplay.SearchKind = ParseOrDefault(_userSettings.PInvokeSearchKind, SearchKind.All);
            symbolDisplay.LanguageType = ParseOrDefault(_userSettings.PInvokeLanguageType, PInvoke.Transform.LanguageType.CSharp);
            snippetDisplay.LanguageType = ParseOrDefault(_userSettings.PInvokeLanguageType, PInvoke.Transform.LanguageType.CSharp);
            snippetDisplay.LanguageTypeChanged += new EventHandler(OnLanguageTypeChanged);
            symbolDisplay.LanguageTypeChanged += new EventHandler(OnLanguageTypeChanged);
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Print(e.Node);
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (!_firstTimeActivated)
            {
                _firstTimeActivated = true;
                OnTabPageChanged(this, EventArgs.Empty);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _signatureString = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // stringize the signature and comments
            TreeNode node = treeView.SelectedNode;

            /*

            MethodDescriptor method_descr;
            if (node != null && (method_descr = node.Tag as MethodDescriptor) != null)
            {
                System.IO.StringWriter writer = new System.IO.StringWriter();
                TextWriterCodePrinter code_printer = new TextWriterCodePrinter(writer);

                StringBuilder log_builder = new StringBuilder();
                LogCallbackPrinter log_printer = new LogCallbackPrinter(delegate(string entry)
                {
                    log_builder.AppendFormat("// {0}{1}", entry, writer.NewLine);
                });

                PrintMethod(method_descr, code_printer, log_printer, true, true);
                writer.Flush();

                // The resulting signature string consists of:
                // 1. [log as a series of single-line comments]
                // 3. [definition of types used by the function]
                // 2. function signature
                if (log_builder.Length > 0) log_builder.Append(writer.NewLine);
                this.signatureString = log_builder.ToString() + writer.GetStringBuilder().ToString();
            }

            */

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*
            // remember the assembly and the path in the tree
            TreeNode node = treeView.SelectedNode;
            lastTreePath = (node == null ? null : node.FullPath);

            Reflector current = Reflector.CurrentReflector;
            if (current != null)
            {
                lastAssemblyPath = current.AssemblyPath;

                // free the reflector (and hence release the assembly)
                Reflector.CurrentReflector = null;
            }
            else lastAssemblyPath = null;
            */
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // restore the original state when the dialog is shown again
            /*
            if (lastAssemblyPath != null)
            {
                LoadAssembly(lastAssemblyPath);
            }

            if (lastTreePath != null)
            {
                TrySelectNode(lastTreePath);
            }
            */
        }

        private void OnTabPageChanged(object sender, EventArgs e)
        {
            bool sigexp = (TabMode.ReversePInvoke == Mode);

            // Force all of the menus to update now so that the hotkeys will
            // be disabled
            // editToolStripMenuItem_DropDownOpening(this, EventArgs.Empty);
            openToolStripMenuItem.Enabled = sigexp;

            optionsToolStripMenuItemExp.Available = sigexp;
            optionsToolStripMenuItemImp.Available = !sigexp;

            fileToolStripMenuItemExp.Available = sigexp;
            fileToolStripMenuItemImp.Available = !sigexp;

            if (TabMode.ReversePInvoke == Mode)
            {
                OnReversePInvokeTabDisplay();
            }
            else
            {
                OnPInvokeTabDisplay();
            }
        }

        private void OnPInvokeTabDisplay()
        {
            if (_nativeStorage == null)
            {
                Cursor = Cursors.WaitCursor;
                symbolDisplay.Enabled = false;
                snippetDisplay.Enabled = false;

                statusStripLabel.Text = "Loading Database ...";
                statusStripProgressBar.Visible = true;
                statusStripProgressBar.Style = ProgressBarStyle.Marquee;
                return;
            }

            Cursor = Cursors.Default;
            symbolDisplay.Enabled = true;
            snippetDisplay.Enabled = true;
            statusStripProgressBar.Style = ProgressBarStyle.Blocks;
            statusStripProgressBar.Visible = false;
            statusStripLabel.Text = String.Empty;
            if (!_nativeStorageSet && _nativeStorage != null)
            {
                statusStrip.Text = String.Empty;
                symbolDisplay.NativeStorage = _nativeStorage;
                snippetDisplay.NativeStorage = _nativeStorage;
                _nativeStorageSet = true;
            }
        }

        private void OnReversePInvokeTabDisplay()
        {
            // Ensure that the cursor is set to default.  If the user loads up the SigImp tab
            // first, the cursor will be busy for a time.  During that time the user can switch
            // the tab and we should reset the cursor
            Cursor = Cursors.Default;
            statusStripLabel.Text = String.Empty;
            statusStripProgressBar.Visible = false;
            statusStripProgressBar.Style = ProgressBarStyle.Blocks;
        }

        private void OnLanguageTypeChanged(object sender, EventArgs e)
        {
            ISignatureImportControl source = sender as ISignatureImportControl;
            if (source != null)
            {
                PInvoke.Transform.LanguageType cur = source.LanguageType;

                symbolDisplay.LanguageType = cur;
                snippetDisplay.LanguageType = cur;
            }
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            /*
            string[] files = (string[])e.Data.GetData("FileDrop");
            if (files != null && files.Length == 1)
            {
                LoadAssembly(files[0]);
            }
            */
        }

        private void treeView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileDrop"))
            {
                string[] files = (string[])e.Data.GetData("FileDrop");
                if (files != null && files.Length == 1)
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        #endregion

        #region Menu Handlers

        /*

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!printing)
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadAssembly(openFileDialog.FileName);
                }
            }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!printing)
            {
                TreeNode node = treeView.SelectedNode;
                string path = (node == null ? null : node.FullPath);

                Reflector current = Reflector.CurrentReflector;
                if (current != null)
                {
                    LoadAssembly(current.AssemblyPath);
                }

                if (path != null)
                {
                    TrySelectNode(path);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private Control GetActiveControl()
        {
            Control cur = ActiveControl;
            do
            {
                ContainerControl container = cur as ContainerControl;
                if (container == null || container.ActiveControl == null)
                {
                    return cur;
                }

                cur = container.ActiveControl;
            }
            while (true);
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            Control cur = GetActiveControl();

            bool copyEnabled = false;
            bool selectAllEnabled = false;          
            if (cur != null)
            {
                TextBoxBase text_base = cur as TextBoxBase;
                if (text_base != null)
                {
                    selectAllEnabled = true;
                    copyEnabled = (text_base.SelectionLength > 0);
                }
                else
                {
                    DataGridView grid_view = cur as DataGridView;
                    if (grid_view != null)
                    {
                        copyEnabled = true;
                        selectAllEnabled = true;
                    }
                }
            }

            copyToolStripMenuItem.Enabled = copyEnabled;
            selectAllToolStripMenuItem.Enabled = selectAllEnabled;
        }

        // "Radio" menu item handlers:

        private void bit32TargetPlatformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bit32TargetPlatformToolStripMenuItem.CheckState = CheckState.Indeterminate;
            bit64TargetPlatformToolStripMenuItem.CheckState = CheckState.Unchecked;
            Print();
        }

        private void bit64TargetPlatformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bit32TargetPlatformToolStripMenuItem.CheckState = CheckState.Unchecked;
            bit64TargetPlatformToolStripMenuItem.CheckState = CheckState.Indeterminate;
            Print();
        }

        private void unicodeTargetPlatformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            unicodeTargetPlatformToolStripMenuItem.CheckState = CheckState.Indeterminate;
            aNSITargetPlatformToolStripMenuItem.CheckState = CheckState.Unchecked;
            Print();
        }

        private void aNSITargetPlatformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            unicodeTargetPlatformToolStripMenuItem.CheckState = CheckState.Unchecked;
            aNSITargetPlatformToolStripMenuItem.CheckState = CheckState.Indeterminate;
            Print();
        }

        private void windowsTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            windowsTypesToolStripMenuItem.CheckState = CheckState.Indeterminate;
            plainCTypesToolStripMenuItem.CheckState = CheckState.Unchecked;
            Print();
        }

        private void plainCTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            windowsTypesToolStripMenuItem.CheckState = CheckState.Unchecked;
            plainCTypesToolStripMenuItem.CheckState = CheckState.Indeterminate;
            Print();
        }

        private void marshalDirectionAnnotationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Print();
        }

        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "siggen.chm"));
        }

        private void showAllToolStripMenuItem_CheckChanged(object sender, EventArgs e)
        {
            symbolDisplay.ShowAll = showAllToolStripMenuItem.Checked;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog();
        }

        private void optionsToolStripMenuItemImp_DropDownOpening(object sender, EventArgs e)
        {
            showAllToolStripMenuItem.Enabled = (TabMode.PInvokeSearch == Mode);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // seems lame but is there a better way?
            SendKeys.Send("^c");
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // seems lame but is there a better way?
            SendKeys.Send("^a");
        }

        #endregion

        #region Tree Building and Signature Display

        private void LoadAssembly(string assemblyPath)
        {
            Cursor prev_cursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Reflector new_reflector;
                try
                {
                    new_reflector = new Reflector(assemblyPath);
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Could not load assembly '{0}'.\n\n{1}",
                        assemblyPath, GetExceptionMessage(e)),
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (RebuildTree(new_reflector))
                {
                    Reflector.CurrentReflector = new_reflector;
                    Print();
                }
                else
                {
                    new_reflector.Dispose();
                }
            }
            finally
            {
                Cursor.Current = prev_cursor;
            }

            reloadToolStripMenuItem.Enabled = true;
        }

        private static TreeNode CreateTreeNode(string text, ImageIndex imageIndex)
        {
            TreeNode node = new TreeNode(text, (int)imageIndex, (int)imageIndex);
            node.Name = text;
            return node;
        }

        /// <summary>
        /// Unwraps target invocation exceptions and concatenates all <see cref="LoaderException"/>s.
        /// </summary>
        private static string GetExceptionMessage(Exception e)
        {
            while (e is TargetInvocationException)
            {
                e = e.InnerException;
            }

            // extract LoaderExceptions from ReflectionTypeLoadException
            ReflectionTypeLoadException rtle = e as ReflectionTypeLoadException;
            if (rtle != null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < rtle.LoaderExceptions.Length; i++)
                {
                    if (sb.Length > 0) sb.Append('\n');
                    sb.Append(rtle.LoaderExceptions[i].Message);
                }

                return sb.ToString();
            }

            // extract LoaderMessages from UnableToGetTypesException
            UnableToGetTypesException utgte = e as UnableToGetTypesException;
            if (utgte != null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < utgte.LoaderMessages.Length; i++)
                {
                    if (sb.Length > 0) sb.Append('\n');
                    sb.Append(utgte.LoaderMessages[i]);
                }

                return sb.ToString();
            }

            return e.Message;
        }

        private bool RebuildTree(Reflector reflector)
        {
            List<TypeDescriptor> type_list;
            List<MethodDescriptor> method_list;
            try
            {
                reflector.GetInteropTypesAndMethods(out type_list, out method_list);
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Could not reflect assembly '{0}'.\n\n{1}",
                    reflector.AssemblyPath, GetExceptionMessage(e)),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            if (type_list.Count == 0 && method_list.Count == 0)
            {
                MessageBox.Show(
                    String.Format("Assembly '{0}' contains no delegates and no interop methods.", reflector.AssemblyPath),
                    "Assembly not loaded",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
                return false;
            }

            treeView.BeginUpdate();
            try
            {
                treeView.Nodes.Clear();

                TreeNode root_node = CreateTreeNode(reflector.AssemblyPath, ImageIndex.Field);

                treeView.Nodes.Add(root_node);

                // convert the lists to a tree view hierarchy
                Dictionary<string, TreeNode> node_map = new Dictionary<string, TreeNode>();

                foreach (TypeDescriptor descriptor in type_list)
                {
                    // just pass it on to GetTypeNode
                    GetTypeNode(node_map, root_node, descriptor);
                }
                foreach (MethodDescriptor descriptor in method_list)
                {
                    TreeNode parent = GetTypeNode(node_map, root_node, descriptor.DeclaringType);

                    ImageIndex image_index =
                        ((descriptor.MethodAttributes & MethodAttributes.Static) == MethodAttributes.Static) ?
                        ImageIndex.StaticMethod :
                        ImageIndex.InstanceMethod;

                    TreeNode method_node = CreateTreeNode(descriptor.MethodName + " : " + descriptor.MethodSigString, image_index);
                    method_node.Tag = descriptor;

                    parent.Nodes.Add(method_node);
                }

                treeView.Sort();

                // expand
                while (true)
                {
                    root_node.Expand();
                    if (root_node.Nodes.Count == 1)
                    {
                        root_node = root_node.Nodes[0];
                    }
                    else break;
                }
            }
            finally
            {
                treeView.EndUpdate();
            }

            return true;
        }

        /// <summary>
        /// Creates and returns or just returns the node representing the specified type.
        /// </summary>
        private TreeNode GetTypeNode(Dictionary<string, TreeNode> nodeMap, TreeNode rootNode, TypeDescriptor descriptor)
        {
            TreeNode node;
            if (!nodeMap.TryGetValue(descriptor.TypeName, out node))
            {
                string type_name;
                int index;

                // check for nested types
                if (descriptor.DeclaringType != null)
                {
                    node = GetTypeNode(nodeMap, rootNode, descriptor.DeclaringType);

                    index = descriptor.TypeName.LastIndexOf('+');
                    if (index > 0)
                    {
                        type_name = descriptor.TypeName.Substring(index + 1);
                    }
                    else if (descriptor.TypeName.StartsWith(descriptor.DeclaringType.TypeName))
                    {
                        type_name = descriptor.TypeName.Substring(descriptor.DeclaringType.TypeName.Length);
                    }
                    else
                    {
                        type_name = descriptor.TypeName;
                    }
                }
                else
                {
                    // find or create the namespace of the type
                    index = descriptor.TypeName.LastIndexOf('.');
                    if (index > 0)
                    {
                        type_name = descriptor.TypeName.Substring(index + 1);
                        string ns_name = descriptor.TypeName.Substring(0, index);

                        if (!nodeMap.TryGetValue(ns_name, out node))
                        {
                            // we do not have the namespace in the tree view
                            node = CreateTreeNode(ns_name, ImageIndex.Namespace);
                            rootNode.Nodes.Add(node);

                            nodeMap.Add(ns_name, node);
                        }
                    }
                    else
                    {
                        type_name = descriptor.TypeName;
                        node = rootNode;
                    }
                }

                ImageIndex image_index;

                if ((descriptor.TypeAttributes & TypeAttributes.Interface) == TypeAttributes.Interface)
                    image_index = ImageIndex.Interface;
                else if (descriptor.IsValueType)
                    image_index = ImageIndex.Structure;
                else
                    image_index = ImageIndex.Class;

                TreeNode type_node = CreateTreeNode(type_name, image_index);
                type_node.Tag = descriptor;

                node.Nodes.Add(type_node);

                nodeMap.Add(descriptor.TypeName, type_node);
                node = type_node;
            }

            return node;
        }

        /// <summary>
        /// Called after reloading an assembly - we want to restore the treeview node selection.
        /// </summary>
        private void TrySelectNode(string fullPath)
        {
            // try the select a node with the specified full path
            TreeNode node = null, final_node = null;

            foreach (string component in
                fullPath.Split(new string[] { treeView.PathSeparator }, StringSplitOptions.None))
            {
                if (node == null)
                {
                    node = treeView.Nodes[component];
                }
                else
                {
                    node = node.Nodes[component];
                }

                if (node != null)
                {
                    final_node = node;
                }
                else break;
            }

            if (final_node != null)
            {
                treeView.SelectedNode = final_node;
            }
        }

        private void Print()
        {
            Print(treeView.SelectedNode);
        }

        private void Print(TreeNode node)
        {
            // prevent reentrancy
            if (printing)
            {
                printPending = true;
                return;
            }

            printing = true;
            try
            {
                richTextBoxCode.Clear();
                richTextBoxMessages.Clear();

                if (node != null && node.Tag != null)
                {
                    MethodDescriptor method_descr = node.Tag as MethodDescriptor;

                    if (method_descr != null)
                    {
                        buttonOK.Enabled = true;

                        // this is a node representing a method - prints its signature including definitions 
                        PrintMethod(method_descr, codePrinter, logPrinter, true, false);
                    }
                    else
                    {
                        TypeDescriptor type_descr = (TypeDescriptor)node.Tag;
                        if (type_descr.IsDelegate)
                        {
                            buttonOK.Enabled = true;

                            // this is a node representing a delegate - prints its signature including definitions 
                            PrintDelegate(type_descr, codePrinter, logPrinter, true, false);
                        }
                        else
                        {
                            buttonOK.Enabled = false;

                            // this is a node representing a non-delegate type - print preview of its methods
                            PrintMethodPreview(node, codePrinter, logPrinter, false, false);
                        }
                    }
                }
                else
                {
                    buttonOK.Enabled = false;
                }

                richTextBoxCode.Refresh();
                richTextBoxMessages.Refresh();
            }
            finally
            {
                printing = false;

                if (printPending)
                {
                    printPending = false;
                    Print();
                }
            }
        }

        /// <summary>
        /// Prints a native signature to the provided printers according to the current settings.
        /// </summary>
        private void PrintSignature(SignatureGenerator.NativeSignature nativeSig, ICodePrinter printer, ILogPrinter logPrinter,
            bool printDefs, bool defsFirst)
        {
            PrintFlags p_flags = PrintFlags.None;

            if (plainCTypesToolStripMenuItem.Checked) p_flags |= PrintFlags.UsePlainC;
            if (marshalDirectionAnnotationsToolStripMenuItem.Checked) p_flags |= PrintFlags.PrintMarshalDirection;

            if (!defsFirst)
                nativeSig.PrintTo(printer, logPrinter, p_flags);

            if (printDefs)
            {
                foreach (NativeTypeDefinition def in nativeSig.GetDefinitions())
                {
                    if (defsFirst)
                        def.PrintTo(printer, logPrinter, p_flags);

                    printer.PrintLn();
                    printer.PrintLn();

                    if (!defsFirst)
                        def.PrintTo(printer, logPrinter, p_flags);
                }
            }

            if (defsFirst)
                nativeSig.PrintTo(printer, logPrinter, p_flags);
        }

        /// <summary>
        /// Prints one method to the provided printers according to the current settings.
        /// </summary>
        private void PrintMethod(MethodDescriptor descriptor, ICodePrinter printer, ILogPrinter logPrinter, bool printDefs, bool defsFirst)
        {
            // this is a node representing a method - get the signature (cached by the descriptor)
            SignatureGenerator.NativeSignature native_sig = descriptor.GetNativeSignature(
                aNSITargetPlatformToolStripMenuItem.Checked,
                bit64TargetPlatformToolStripMenuItem.Checked);

            PrintSignature(native_sig, printer, logPrinter, printDefs, defsFirst);
        }

        /// <summary>
        /// Prints one delegate to the provided printers according to the current settings.
        /// </summary>
        private void PrintDelegate(TypeDescriptor descriptor, ICodePrinter printer, ILogPrinter logPrinter, bool printDefs, bool defsFirst)
        {
            Debug.Assert(descriptor.IsDelegate);

            // this is a node representing a delegate - get the signature (cached by the descriptor)
            SignatureGenerator.NativeSignature native_sig = descriptor.GetNativeSignature(
                aNSITargetPlatformToolStripMenuItem.Checked,
                bit64TargetPlatformToolStripMenuItem.Checked);

            PrintSignature(native_sig, printer, logPrinter, printDefs, defsFirst);
        }

        /// <summary>
        /// Prints preview of all methods under the specified node to the provided printers according to the current settings.
        /// </summary>
        private void PrintMethodPreview(TreeNode node, ICodePrinter printer, ILogPrinter logPrinter, bool printDefs, bool defsFirst)
        {
            // this can take some time - try to not appear hung
            int tick_count1 = Environment.TickCount;
            int tick_count2 = unchecked(tick_count1 + 1);
            Cursor original_cursor = Cursor.Current;

            // this is a node representing a type - get signatures of all its methods
            ILogPrinter mem_log = new LogMemoryPrinter();
            bool first = true;

            try
            {
                foreach (TreeNode child in node.Nodes)
                {
                    MethodDescriptor method_descr = child.Tag as MethodDescriptor;
                    if (method_descr != null)
                    {
                        if (!first)
                        {
                            codePrinter.PrintLn();
                            codePrinter.PrintLn();
                        }
                        else first = false;

                        // print no messages and no definitions
                        PrintMethod(method_descr, codePrinter, mem_log, printDefs, defsFirst);

                        int ticks = Environment.TickCount;
                        if (ticks != tick_count1 && ticks != tick_count2)
                        {
                            // it's taking too long
                            tick_count1 = Environment.TickCount;
                            tick_count2 = unchecked(tick_count1 + 1);

                            Application.DoEvents();

                            // re-set the selection on rich text box controls because DoEvents may have
                            // included some undesired user input
                            richTextBoxCode.Select(richTextBoxCode.TextLength, 0);
                            richTextBoxMessages.Select(richTextBoxMessages.TextLength, 0);

                            Cursor.Current = Cursors.WaitCursor;
                        }
                    }
                }
            }
            finally
            {
                Cursor.Current = original_cursor;
            }

            logPrinter.PrintEntry(
                Severity.Info, 0,
                "Please select an individual method in the tree to view additional information.");
        }

    */

        #endregion

        #region NativeStorageLoading 

        private void LoadNativeStorage(object invokeUntyped)
        {
            try
            {
                _nativeStorage = NativeStorage.LoadFromAssemblyPath();
            }
            catch (Exception ex)
            {
                // Need to swallow this unfortunately because we are on a backrground thread 
                Debug.Fail(ex.Message);
            }
            finally
            {
                if (_nativeStorage == null)
                {
                    _nativeStorage = new NativeStorage();
                }
            }

            // Ping the UI to let in know the Database is now loaded
            ISynchronizeInvoke invoke = (ISynchronizeInvoke)invokeUntyped;
            try
            {
                WaitCallback del = delegate (object notused) { this.OnTabPageChanged(this, EventArgs.Empty); };
                invoke.Invoke(del, new object[] { null });
            }
            catch (Exception)
            {
                // No need to assert.  We get this if the user closes the application before
                // the database finishes loading
            }
        }

        private static T ParseOrDefault<T>(string str, T defaultValue)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), str);
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }

        #endregion
    }
}
