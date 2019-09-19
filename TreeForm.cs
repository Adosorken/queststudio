using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.Resources;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using QuestStudio.Tree_Extensions;
using System.IO;

namespace QuestStudio
{
    public partial class TreeForm : Form
    {
        public TreeForm()
        {
            InitializeComponent();
        }
        public TreeForm(string loadFile)
        {
            FileOnLoad = loadFile;
            InitializeComponent();
        }

        #region Members

        private string FileOnLoad = null;

        private int[] ConditionUsed = new int[200];
        private int[] ActionUsed = new int[200];
        private bool locked_edit = false; // grid is locked while populating, prevent cell value changed events
        private UnDoRedo UndoManager;

        private TreeNode m_SelectedTriggerNode = null; // Node that is clicked and can be edited in the grids
        private TreeNode m_MenuNode = null; // Node that was rightclicked to display a contextmenu
        private QuestTrigger m_CopyTrigger = null;

        #endregion

        #region Form Events

        private void TreeForm_Load(object sender, EventArgs e)
        {
            if (!(Properties.Settings.Default.WindowWidth == 0 &&
                 Properties.Settings.Default.WindowHeight == 0 &&
                 Properties.Settings.Default.LocationX == 0 &&
                 Properties.Settings.Default.LocationY == 0))
            {
                this.Location = new Point(Properties.Settings.Default.LocationX, Properties.Settings.Default.LocationY);
                this.Width = Properties.Settings.Default.WindowWidth;
                this.Height = Properties.Settings.Default.WindowHeight;
            }

            this.treeView1.ValidateLabelEdit += new TreeLE.ValidateLabelEditEventHandler(Tree1_ValidateLabelEdit);
            this.treeView1.BeforeLabelEdit += Tree1_BeforeLabelEdit;
            this.treeView1.AfterLabelEdit += Tree1_AfterLabelEdit;
            this.treeView1.KeyDown += Tree1_KeyDown;

            UndoManager = new UnDoRedo();
            UndoManager.form = this;
            UndoManager.tree = treeView1;

            SelectNode(null);

            if (FileOnLoad != null)
            {
                TreeNode fileNode = this.AddToTree(new QuestFile(FileOnLoad));
                fileNode.EnsureVisible();
                fileNode.Expand();
                SelectNode(fileNode);
            }

            // Assert 
            //Ability c = new Ability();
            //Console.WriteLine(c.getText());
            //Console.WriteLine(TypeDescriptor.GetConverter(typeof(Ability)));
        }

        private void TreeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.LocationX = this.Location.X;
                Properties.Settings.Default.LocationY = this.Location.Y;
                Properties.Settings.Default.WindowWidth = this.Width;
                Properties.Settings.Default.WindowHeight = this.Height;
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        #region Add Nodes

        public TreeNode AddToTree(Condition condition, TreeNode parent, int index)
        {
            treeView1.BeginUpdate();

            TreeNode child = new TreeNode(Condition.getNodeText(condition));
            child.Tag = condition;

            if (index == -1)
                parent.Nodes.Add(child);
            else {
                parent.Nodes.Insert(index, child);
                parent.Text = "Conditions (" + parent.Nodes.Count + ")";
            }

            ConditionUsed[(int)condition.getConditionType()] += 1;

            treeView1.EndUpdate();
            return child;
        }

        public TreeNode AddToTree(Reward reward, TreeNode parent, int index)
        {
            treeView1.BeginUpdate();

            TreeNode child = new TreeNode(Reward.getNodeText(reward));
            child.Tag = reward;

            if (index == -1)
                parent.Nodes.Add(child);
            else {
                parent.Nodes.Insert(index, child);
                parent.Text = "Rewards (" + parent.Nodes.Count + ")";
            }

            if (reward.getRewardType() == RewardType.REWD027 ||
                reward.getRewardType() == RewardType.REWD031 ||
                reward.getRewardType() == RewardType.REWD032 ||
                reward.getRewardType() == RewardType.REWD033)
            {
                throw new FooException("Using bugged reward: " + reward);
            }

            ActionUsed[(int)reward.getRewardType()] += 1;

            treeView1.EndUpdate();
            return child;
        }

        public TreeNode AddToTree(QuestTrigger trigger, TreeNode parent, int index)
        {
            treeView1.BeginUpdate();

            TreeNode child;
            if (index == -1)
                child = parent.Nodes.Add(trigger.GetNodeText());
            else
                child = parent.Nodes.Insert(index, trigger.GetNodeText());

            child.Tag = trigger;
            child.Checked = trigger.CheckNext;

            TreeNode node4 = new TreeNode("Conditions (" + trigger.Conditions.Count + ")");
            child.Nodes.Add(node4);
            for (int c = 0; c < trigger.Conditions.Count; c++)
                AddToTree(trigger.Conditions.ElementAt(c), node4, -1);

            node4 = new TreeNode("Rewards (" + trigger.Rewards.Count + ")");
            child.Nodes.Add(node4);
            for (int c = 0; c < trigger.Rewards.Count; c++)
                AddToTree(trigger.Rewards.ElementAt(c), node4, -1);

            treeView1.EndUpdate();
            return child;
        }

        public TreeNode AddToTree(QuestBlock block, TreeNode parent, int index)
        {
            treeView1.BeginUpdate();

            TreeNode child = new TreeNode(block.GetNodeText(parent.Nodes.Count));
            child.Tag = block;

            if (index == -1)
                parent.Nodes.Add(child);
            else
                parent.Nodes.Insert(index, child);

            for (int r = 0; r < block.Triggers.Count; r++)
                AddToTree(block.Triggers.ElementAt(r), child, -1);

            if (index != -1)
            {
                foreach (TreeNode BlockNode in child.Parent.Nodes)
                    if (BlockNode.Index >= child.Index)
                        BlockNode.Text = block.GetNodeText(BlockNode.Index);
            }

            treeView1.EndUpdate();
            return child;
        }

        TreeNode AddToTree(QuestFile file)
        {
            treeView1.BeginUpdate();

            TreeNode child = new TreeNode(System.IO.Path.GetFileName(file.FileName) + " : " + file.Title);
            child.Tag = file;
            child.ToolTipText = file.FileName;
            treeView1.Nodes.Add(child);

            for (int i = 0; i < file.Blocks.Count; i++)
                AddToTree( file.Blocks.ElementAt(i), child, -1);

            treeView1.EndUpdate();
            return child;
        }

        #endregion

        #region Remove Nodes

        public void RemoveBlock(TreeNode parent, int blockid)
        {
            treeView1.BeginUpdate();

            parent.Nodes.RemoveAt(blockid);

            foreach (TreeNode BlockNode in parent.Nodes)
                if (BlockNode.Index >= blockid)
                    BlockNode.Text = ((QuestBlock)BlockNode.Tag).GetNodeText(BlockNode.Index);

            treeView1.EndUpdate();
        }

        public void RemoveTrigger(TreeNode parent, int triggerid)
        {
            treeView1.BeginUpdate();

            parent.Nodes.RemoveAt(triggerid);

            treeView1.EndUpdate();
        }

        public void RemoveCondition(TreeNode parent, int conditionid)
        {
            treeView1.BeginUpdate();

            if (parent.Nodes.Count > 0)
                parent.Nodes.RemoveAt(conditionid);

            parent.Text = "Conditions (" + parent.Nodes.Count + ")";

            treeView1.EndUpdate();
        }

        public void RemoveReward(TreeNode parent, int rewardid)
        {
            treeView1.BeginUpdate();

            if (parent.Nodes.Count > 0)
                parent.Nodes.RemoveAt(rewardid);

            parent.Text = "Rewards (" + parent.Nodes.Count + ")";

            treeView1.EndUpdate();
        }

        #endregion

        #region TreeNode To Object

        private QuestTrigger CreateTriggerFromTree(TreeNode triggerNode)
        {
            QuestTrigger trigger = new QuestTrigger();
            trigger.Name = ((QuestTrigger)triggerNode.Tag).Name;
            trigger.CheckNext = ((QuestTrigger)triggerNode.Tag).CheckNext;

            foreach (TreeNode conditionNode in triggerNode.Nodes[0].Nodes)
                trigger.Conditions.Add((Condition)conditionNode.Tag);

            foreach (TreeNode rewardNode in triggerNode.Nodes[1].Nodes)
                trigger.Rewards.Add((Reward)rewardNode.Tag);

            return trigger;
        }

        private QuestBlock CreateBlockFromTree(TreeNode blockNode)
        {
            QuestBlock block = new QuestBlock();
            block.Name = ((QuestBlock)blockNode.Tag).Name;

            foreach (TreeNode triggerNode in blockNode.Nodes)
                block.Triggers.Add(CreateTriggerFromTree(triggerNode));

            return block;
        }

        // Nodes get added, removed or moved in the tree, so rebuild the object from the tree
        private QuestFile CreateFileFromTree(TreeNode fileNode)
        {
            QuestFile file = new QuestFile();
            file.Title = ((QuestFile)fileNode.Tag).Title;
            file.Version = ((QuestFile)fileNode.Tag).Version;
            file.FileName = ((QuestFile)fileNode.Tag).FileName;

            foreach (TreeNode blockNode in fileNode.Nodes)
                file.Blocks.Add(CreateBlockFromTree(blockNode));
            
            return file;
        }

        #endregion

        #region Find Nodes

        TreeNode FindNode(TreeNode Node, int Level, string Text, bool Exact, bool Expand)
        {
            TreeNode Found = null;
            foreach (TreeNode tn in Node.Nodes)
            {
                if (tn.Level < Level)
                {
                    TreeNode result = FindNode(tn, Level, Text, Exact, Expand);
                    if (result != null)
                        Found = result;
                }
                else if (tn.Level == Level)
                {
                    if ((Exact && tn.Text.ToUpper().Trim('\0') == Text) || (!Exact && tn.Text.ToUpper().Contains(Text)))
                    {
                        if (Expand)
                        {
                            tn.ExpandAll();
                            tn.EnsureVisible();
                        }

                        Found = tn;
                    }
                }
            }
            return Found;
        }

        public TreeNode FindNode( int Level, string Text, bool Exact, bool Expand )
        {
            Console.WriteLine("{0}, {1}, {2}, {2}", Level, Text.ToUpper(), Exact, Expand);

            foreach (TreeNode node in treeView1.Nodes)
            {
                TreeNode result = FindNode(node, Level, Text.ToUpper().Trim('\0'), Exact, Expand);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion

        #region TreeView Events

        TreeNode GetParent(int Level, TreeNode node)
        {
            while (node != null && node.Level > Level)
                node = node.Parent;

            return node;
        }

        public void SelectNode(TreeNode node)
        {
            if ( node == null ) {
                treeView1.SelectedNode = null;
                toolStripStatusLabel1.Text = "";
                toolStripStatusLabel2.Text = "";
            } else {
                treeView1.SelectedNode = null; // force reselect event
                treeView1.SelectedNode = node;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode Node = e.Node;

            QuestFile file = (QuestFile)GetParent(0, Node).Tag;
            if (file.Dirty)
                toolStripStatusLabel2.Text = file.FileName + " (unsaved)";
            else
                toolStripStatusLabel2.Text = file.FileName + " (saved)";
            
            toolStripStatusLabel1.Text = "";

            switch (Node.Level)
            {
                case 0:
                case 1:
                    ShowTriggerInGrids(null, null);
                    break;
                case 2:
                {
                    QuestTrigger trigger = (QuestTrigger)Node.Tag;
                    ShowTriggerInGrids(trigger, Node);

                    uint hash = Util.StrToHashKey(Node.Text);
                    Console.WriteLine("Input: " + Node.Text);
                    Console.WriteLine("UINT: " + hash);
                    Console.WriteLine("INT: " + (int)hash + " (database)");
                    Console.WriteLine("HEX: " + hash.ToString("X"));
                    break;
                }
                case 3:
                {   
                    QuestTrigger trigger = (QuestTrigger)Node.Parent.Tag;
                    ShowTriggerInGrids(trigger, Node.Parent);
                    break;
                }
                case 4:
                {
                    QuestTrigger trigger = (QuestTrigger)Node.Parent.Parent.Tag;
                    ShowTriggerInGrids(trigger, Node.Parent.Parent);

                    // Select first cell of the row
                    if (Node.Parent.Index == 0)
                        gridCondition.CurrentCell = gridCondition[0, Node.Index];
                    else if (Node.Parent.Index == 1)
                        gridReward.CurrentCell = gridReward[0, Node.Index];

                    if (Node.Tag != null)
                    {
                        if (Node.Tag.GetType().IsSubclassOf(typeof(Condition)))
                        {
                            Condition c = (Condition)Node.Tag;
                            Console.WriteLine("IN:\t\t" + BitConverter.ToString(c.getOriginalData()) + " L:" + c.getOriginalData().Length);
                        }
                        else if (Node.Tag.GetType().IsSubclassOf(typeof(Reward)))
                        {
                            Reward c = (Reward)Node.Tag;
                            Console.WriteLine("IN:\t\t" + BitConverter.ToString(c.getOriginalData()) + " L:" + c.getOriginalData().Length);
                        }

                        byte[] bytes = StructWriter.Convert(Node.Tag);
                        Console.WriteLine("OUT:\t" + BitConverter.ToString(bytes) + " L:" + bytes.Length);
                    }
                    break;
                }
                default:
                    break;
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 2)
            {
                e.Node.ExpandAll();
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Should already be the selected node but lets reselect.
                SelectNode(e.Node);

                m_MenuNode = e.Node;

                if (e.Node.Level == 0)
                {
                    mnuFile.Show(treeView1, e.X, e.Y);
                }
                else if (e.Node.Level == 1)
                {
                    mnuBlock.Show(treeView1, e.X, e.Y);
                }
                else if (e.Node.Level == 2)
                {
                    checkNextToolStripMenuItem.Checked = ((QuestTrigger)e.Node.Tag).CheckNext;
                    mnuTrigger.Show(treeView1, e.X, e.Y);
                }
            }
        }

        private void Tree1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (this.treeView1.SelectedNode != null)
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        this.treeView1.BeginEdit();
                        break;
                    case Keys.F2:
                        this.treeView1.BeginEdit();
                        break;
                    case Keys.Space:
                        this.treeView1.SelectedNode.Toggle();
                        break;
                    default:
                        break;
                }
        }

            #region CheckBox

            private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
            {
                if (e.Node.Tag != null & e.Node.Tag is QuestTrigger)
                {
                    ((QuestTrigger)e.Node.Tag).CheckNext = e.Node.Checked;
                }
            }

            private void treeView1_BeforeCheck(object sender, TreeViewCancelEventArgs e)
            {
                if (e.Action != TreeViewAction.Unknown)
                    e.Cancel = true;
            }

            #endregion

            #region Label

        private void treeView1_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Level > 2)
            {
                Console.WriteLine("canceledit treeView1_BeforeLabelEdit");
                e.CancelEdit = true;
            }
        }

        private void Tree1_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Level > 2)
            {
                Console.WriteLine("canceledit Tree1_BeforeLabelEdit");
                e.CancelEdit = true;
                return;
            }

            // --- Here we can customize label for editing ---
            TreeNode tn = treeView1.SelectedNode;
            switch (tn.Level)
            {
                case 0:
                    tn.Text = ((QuestFile)tn.Tag).Title;
                    break;
                case 1:
                    tn.Text = ((QuestBlock)tn.Tag).Name;
                    break;
                case 2:
                    tn.Text = ((QuestTrigger)tn.Tag).Name;
                    break;
            }
        }

        private void Tree1_AfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
        {
            // --- Here we can transform edited label back to its original format ---
            TreeNode tn = treeView1.SelectedNode;
            switch (tn.Level)
            {
                case 0:
                    {
                        string oldTitle = ((QuestFile)tn.Tag).Title;

                        ((QuestFile)tn.Tag).Title = e.Label;
                        tn.Text = System.IO.Path.GetFileName(((QuestFile)tn.Tag).FileName) + " : " + ((QuestFile)tn.Tag).Title;

                        AddAction(UnDoRedo.ActionType.Rename_File, tn, oldTitle, e.Label);
                        break;
                    }
                case 1:
                    {
                        string oldName = ((QuestBlock)tn.Tag).Name;

                        ((QuestBlock)tn.Tag).Name = e.Label;
                        tn.Text = ((QuestBlock)tn.Tag).GetNodeText(tn.Index);

                        AddAction(UnDoRedo.ActionType.Rename_Block, tn, oldName, e.Label);
                        break;
                    }
                case 2:
                    {
                        string oldName = ((QuestTrigger)tn.Tag).Name;

                        ((QuestTrigger)tn.Tag).Name = e.Label;
                        tn.Text = ((QuestTrigger)tn.Tag).GetNodeText();

                        AddAction(UnDoRedo.ActionType.Rename_Trigger, tn, oldName, e.Label);
                        break;
                    }
            }
        }

        private void Tree1_ValidateLabelEdit(object sender, ValidateLabelEditEventArgs e)
        {
            switch (e.Node.Level)
            {
                case 0: // file
                    break;

                case 1: // block;
                    {
                        if (e.Label.Trim() == "")
                        {
                            MessageBox.Show("The tree node label cannot be empty",
                                "Label Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                            return;
                        }
                        break;
                    }

                case 2: // trigger
                    {
                        if (e.Label.Trim() == "")
                        {
                            MessageBox.Show("The tree node label cannot be empty", "Label Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                            return;
                        }

                        TreeNode node = FindNode(2, e.Label, true, false);
                        if (node != null && node != e.Node)
                        {
                            MessageBox.Show("A trigger with this name already exists in the file", "Label Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                            return;
                        }
                        break;
                    }
            };
        }

            #endregion

        #endregion
        
        #region Grid Events

        private void grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (locked_edit)
                return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridView grid = (DataGridView)sender;

            if (grid[e.ColumnIndex, e.RowIndex].ValueType == typeof(object))
            {
                //Console.WriteLine("skip readonly edit");
                return;
            }

            //Console.WriteLine("grid_CellValueChanged, Row {0} Col {1}", e.RowIndex, e.ColumnIndex);

            int col = 2;
            if (grid[0, e.RowIndex].ValueType == typeof(ConditionType))
            {
                Condition new_cond_obj = QuestFile.CreateObject((ConditionType)grid[0, e.RowIndex].Value);
                object obj = (object)new_cond_obj;

                // populate obj from grid, ignore values that are not acceptable for each property
                foreach (object o in GetObjectCellOrder(obj))
                    RowToObject(grid, e.RowIndex, o, ref col, ref obj, e.ColumnIndex == 0);

                // update the row from the new obj
                locked_edit = true;
                {
                    grid.Rows.RemoveAt(e.RowIndex); // remove so all columns are cleared
                    grid.Rows.Insert(e.RowIndex, 1); // add only columns that are used now

                    col = 0;
                    ObjectToGrid(grid, ref col, e.RowIndex, obj);
                }
                locked_edit = false;

                if (m_SelectedTriggerNode != null)
                {
                    TreeNode condition_node = m_SelectedTriggerNode.Nodes[0].Nodes[e.RowIndex];

                    Condition old_cond_obj = (Condition)condition_node.Tag;
                    condition_node.Text = Condition.getNodeText(new_cond_obj);
                    condition_node.Tag = new_cond_obj;

                    AddAction(UnDoRedo.ActionType.Edit_Condition, condition_node, old_cond_obj.Clone(), new_cond_obj.Clone());
                }
            }
            else if (grid[0, e.RowIndex].ValueType == typeof(RewardType))
            {
                Reward new_rewd_obj = QuestFile.CreateObject((RewardType)grid[0, e.RowIndex].Value);
                object obj = (object)new_rewd_obj;

                foreach (object cellInfo in GetObjectCellOrder(obj))
                    RowToObject(grid, e.RowIndex, cellInfo, ref col, ref obj, e.ColumnIndex == 0);

                locked_edit = true;
                {
                    grid.Rows.RemoveAt(e.RowIndex);
                    grid.Rows.Insert(e.RowIndex, 1);

                    col = 0;
                    ObjectToGrid(grid, ref col, e.RowIndex, obj);
                }
                locked_edit = false;

                if ( m_SelectedTriggerNode != null)
                {
                    TreeNode reward_node = m_SelectedTriggerNode.Nodes[1].Nodes[e.RowIndex];

                    Reward old_rewd_obj = (Reward)reward_node.Tag;
                    reward_node.Text = Reward.getNodeText(new_rewd_obj);
                    reward_node.Tag = new_rewd_obj;

                    AddAction(UnDoRedo.ActionType.Edit_Reward, reward_node, old_rewd_obj.Clone(), new_rewd_obj.Clone());
                }
            }
        }

        private void grid_KeyDown(object sender, KeyEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            if (m_SelectedTriggerNode == null)
                return;

            if (e.Control && e.KeyCode == Keys.Add)
            {
                if (grid.Name == "gridCondition")
                {
                    Condition condition = QuestFile.CreateObject(ConditionType.COND000);
                    TreeNode condition_node = AddToTree(condition, m_SelectedTriggerNode.Nodes[0], -1);
                    this.SelectNode(m_SelectedTriggerNode);
                    AddAction(UnDoRedo.ActionType.Add_Condition, condition_node, condition.Clone());
                }
                else if (grid.Name == "gridReward")
                {
                    Reward reward = QuestFile.CreateObject(RewardType.REWD000);
                    TreeNode reward_node = AddToTree(reward, m_SelectedTriggerNode.Nodes[1], -1);
                    this.SelectNode(m_SelectedTriggerNode);
                    AddAction(UnDoRedo.ActionType.Add_Reward, reward_node, reward.Clone());
                }
            }
            else if (e.Control && e.KeyCode == Keys.Subtract)
            {
                if (grid.Name == "gridCondition")
                {
                    if (m_SelectedTriggerNode.Nodes[0].Nodes.Count == 0)
                        return;

                    int condition_id = m_SelectedTriggerNode.Nodes[0].Nodes.Count - 1;
                    TreeNode condition_node = m_SelectedTriggerNode.Nodes[0].Nodes[condition_id];
                    Condition condition = (Condition)condition_node.Tag;
                    AddAction(UnDoRedo.ActionType.Del_Condition, condition_node, condition.Clone());

                    RemoveCondition(m_SelectedTriggerNode.Nodes[0], m_SelectedTriggerNode.Nodes[0].Nodes.Count - 1);
                    this.SelectNode(m_SelectedTriggerNode);
                }
                else if (grid.Name == "gridReward")
                {
                    if (m_SelectedTriggerNode.Nodes[1].Nodes.Count == 0)
                        return;

                    int reward_id = m_SelectedTriggerNode.Nodes[1].Nodes.Count - 1;
                    TreeNode reward_node = m_SelectedTriggerNode.Nodes[1].Nodes[reward_id];
                    Reward reward = (Reward)reward_node.Tag;
                    AddAction(UnDoRedo.ActionType.Del_Reward, reward_node, reward.Clone());

                    RemoveReward(m_SelectedTriggerNode.Nodes[1], m_SelectedTriggerNode.Nodes[1].Nodes.Count - 1);
                    this.SelectNode(m_SelectedTriggerNode);
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                Point[] cellList = new Point[grid.SelectedCells.Count];
                int cellid = 0;
                foreach (DataGridViewCell cell in grid.SelectedCells)
                    cellList[cellid++] = new Point(cell.RowIndex, cell.ColumnIndex);

                UndoManager.Lock();

                QuestTrigger old_trigger = CreateTriggerFromTree(m_SelectedTriggerNode);

                treeView1.BeginUpdate();

                foreach (Point cell in cellList)
                {
                    int row = cell.X;
                    int col = cell.Y;
                    if (col == 0)
                        continue;

                    if (row >= 0 && col >= 0 && grid.Columns.Count > col && grid.Rows.Count > row && grid[col, row] != null)
                    {
                        if (grid[col, row].ValueType == typeof(object)) //spacer cell
                            continue;
                        else if (grid[col, row].ValueType == typeof(string)) // empty string
                            grid[col, row].Value = "";
                        else
                            grid[col, row].Value = Activator.CreateInstance(grid[col, row].ValueType);
                    }
                }

                treeView1.EndUpdate();

                QuestTrigger new_trigger = CreateTriggerFromTree(m_SelectedTriggerNode);
                UndoManager.Unlock();
                AddAction(UnDoRedo.ActionType.Paste, m_SelectedTriggerNode, old_trigger, new_trigger);

                // Reselect the same area as before

                grid.ClearSelection();
                foreach (Point cell in cellList)
                    if (cell.X >= 0 && cell.Y >= 0 && grid.Columns.Count > cell.Y && grid.Rows.Count > cell.X && grid[cell.Y, cell.X] != null)
                        grid[cell.Y, cell.X].Selected = true;
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                DataObject d = grid.GetClipboardContent();
                Clipboard.SetDataObject(d);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                Console.WriteLine("Paste start");

                Point[] cellList = new Point[grid.SelectedCells.Count];
                int cellid = 0;
                foreach (DataGridViewCell cell in grid.SelectedCells)
                    cellList[cellid++] = new Point(cell.RowIndex, cell.ColumnIndex);

                UndoManager.Lock();

                QuestTrigger old_trigger = CreateTriggerFromTree(m_SelectedTriggerNode);

                treeView1.BeginUpdate();

                string CopiedContent = Clipboard.GetText();
                string[] Lines = CopiedContent.Split('\n');
                int StartingRow = grid.CurrentCell.RowIndex;
                int StartingColumn = grid.CurrentCell.ColumnIndex;
                foreach (var line in Lines)
                {
                    if (StartingRow <= (grid.Rows.Count - 1))
                    {
                        string[] cells = line.Split('\t');
                        int ColumnIndex = StartingColumn;
                        for (int i = 0; i < cells.Length && ColumnIndex <= (grid.Columns.Count - 1); i++)
                        {
                            Type dt = grid[ColumnIndex, StartingRow].ValueType;
                            if (dt == typeof(object))
                            {
                                //Console.WriteLine("Skip paste in readonly cell");
                            }
                            else if (dt.IsEnum)
                            {
                                try {
                                    grid[ColumnIndex, StartingRow].Value = EnumExtensions.PasteConvertStringToEnum(dt, cells[i]);
                                } catch (Exception) {
                                    Console.WriteLine("Exception! Enum paste failed, change nothing.");
                                }
                            }
                            else
                            {
                                try {
                                    grid[ColumnIndex, StartingRow].Value = Convert.ChangeType(cells[i], dt);
                                } catch (Exception) {
                                    grid[ColumnIndex, StartingRow].Value = Activator.CreateInstance(dt);
                                }
                            }
                            ColumnIndex++;
                        }
                        StartingRow++;
                    }
                }
                Console.WriteLine("Paste end");

                treeView1.EndUpdate();

                QuestTrigger new_trigger = CreateTriggerFromTree(m_SelectedTriggerNode);
                UndoManager.Unlock();
                AddAction(UnDoRedo.ActionType.Paste, m_SelectedTriggerNode, old_trigger, new_trigger);

                // Reselect the same area as before

                grid.ClearSelection();
                foreach (Point cell in cellList)
                    if (cell.X >= 0 && cell.Y >= 0 && grid.Columns.Count > cell.Y && grid.Rows.Count > cell.X && grid[cell.Y, cell.X] != null)
                        grid[cell.Y, cell.X].Selected = true;
            }
        }

        private void grid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            for (int i = 0; i < grid.Rows.Count; i++)
                grid.Rows[i].HeaderCell.Value = i.ToString();
        }

        private void grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            e.ThrowException = false;
            e.Cancel = true;
            grid.CancelEdit();
        }

        private void grid_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            if (grid.CurrentCell == null)
                return;
            grid.BeginEdit(true);
        }

            #region Cell Selection Styling

        void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            if (grid.CurrentCell != null)
            {
                grid[e.ColumnIndex, e.RowIndex].Style.SelectionBackColor = SystemColors.Highlight;
                grid[e.ColumnIndex, e.RowIndex].Style.SelectionForeColor = Color.White;
            }
            grid.Invalidate();
        }

        void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            if (grid.CurrentCell != null)
            {
                if (grid.SelectedCells.Count == 1)
                {
                    grid[e.ColumnIndex, e.RowIndex].Style.SelectionBackColor = Color.White;
                    grid[e.ColumnIndex, e.RowIndex].Style.SelectionForeColor = Color.Black;
                }
                else
                {
                    grid[e.ColumnIndex, e.RowIndex].Style.SelectionBackColor = SystemColors.Highlight;
                    grid[e.ColumnIndex, e.RowIndex].Style.SelectionForeColor = Color.White;
                }
            }
            grid.Invalidate();
        }

        void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            if (grid.CurrentCell == null)
                return;

            if (e.ColumnIndex == grid.CurrentCell.ColumnIndex && e.RowIndex == grid.CurrentCell.RowIndex)
            {
                if (grid.SelectedCells.Count == 1)
                {
                    grid.CurrentCell.Style.SelectionBackColor = Color.White;
                    grid.CurrentCell.Style.SelectionForeColor = Color.Black;
                }
                else
                {
                    grid.CurrentCell.Style.SelectionBackColor = SystemColors.Highlight;
                    grid.CurrentCell.Style.SelectionForeColor = Color.White;
                }

                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);
                using (Pen p = new Pen(Color.Black, 2))
                {
                    Rectangle rect = e.CellBounds;
                    rect.X += 1;
                    rect.Y += 1;
                    rect.Width -= 3;
                    rect.Height -= 3;
                    e.Graphics.DrawRectangle(p, rect);
                }
                e.Handled = true;
            }
        }

        #endregion

        #endregion

        #region Menu Events

            #region File Menu

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!RemoveAllFiles())
                return;

            QuestFile newFile = new QuestFile();
            newFile.Title = "New QuestFile | iRosePH";
            newFile.Version = 12;
            newFile.Dirty = true;

            TreeNode fileNode = this.AddToTree(newFile);
            SelectNode(fileNode);
        }

        bool RemoveAllFiles()
        {
            foreach (TreeNode fileNode in treeView1.Nodes)
            {
                QuestFile file = (QuestFile)fileNode.Tag;
                if (file.Dirty)
                {
                    string text = "";
                    if (file.FileName == null)
                        text = "Save changes to " + file.Title + "?";
                    else
                        text = "Save changes to " + file.FileName + "?";

                    DialogResult result = MessageBox.Show(text, "Close", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                        SaveFile(file, false);
                    else if (result == DialogResult.Cancel)
                        return false;
                }
            }

            treeView1.Nodes.Clear();
            treeView1.SelectedNode = null;
            ShowTriggerInGrids(null, null);
            return true;
        }

        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (!RemoveAllFiles())
                return;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "QSD Files (.qsd)|*.qsd";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = true;

            DialogResult userClickedOK = openFileDialog1.ShowDialog();
            if (userClickedOK == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;

                treeView1.BeginUpdate();
                Console.WriteLine("Begin loading");
                TreeNode fileNode = null;
                int i = 0;
                foreach (string File in openFileDialog1.FileNames)
                {
                    if (openFileDialog1.FileNames.Length > 1 && File.Contains('#'))
                        continue;

                    QuestFile quest_file = new QuestFile(File);
                    fileNode = this.AddToTree(quest_file);
                    i++;
                }
                Console.WriteLine("All files loaded");
                treeView1.EndUpdate();
                Cursor.Current = Cursors.Default;

                if (openFileDialog1.FileNames.Length == 1 && fileNode != null)
                {
                    fileNode.EnsureVisible();
                    fileNode.Expand();
                    SelectNode(fileNode);
                }

            }

            for (int i = 0; i < ConditionUsed.Length; i++)
            {
                Console.WriteLine(Enum.Parse(typeof(ConditionType), i.ToString()) + ": " + ConditionUsed[i].ToString());
                ConditionUsed[i] = 0;
            }

            for (int i = 0; i < ActionUsed.Length; i++)
            {
                Console.WriteLine(Enum.Parse(typeof(RewardType), i.ToString()) + ": " + ActionUsed[i].ToString());
                ActionUsed[i] = 0;
            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Matt 2014");
        }

        private bool SaveFile(QuestFile newFile, bool SaveAs)
        {
            if (newFile.FileName == null || SaveAs)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "QSD|*.QSD";
                save.FileName = newFile.FileName;

                if (save.ShowDialog() != DialogResult.OK)
                    return false;

                if (!newFile.Save(save.FileName))
                    return SaveFile(newFile, true);
            } 
            else 
            {
                if ( !newFile.Save(newFile.FileName) )
                    return SaveFile(newFile, true);
            }
            newFile.Dirty = false;
            toolStripStatusLabel2.Text = newFile.FileName + " (saved)";
            return true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;

            TreeNode fileNode = GetParent(0, treeView1.SelectedNode);
            if (fileNode.Tag.GetType() == typeof(QuestFile))
            {
                QuestFile newFile = CreateFileFromTree(fileNode);
                SaveFile(newFile, false);
                fileNode.Tag = newFile;
                fileNode.Text = System.IO.Path.GetFileName(newFile.FileName) + " : " + newFile.Title;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;

            TreeNode fileNode = GetParent(0, treeView1.SelectedNode);
            if (fileNode.Tag.GetType() == typeof(QuestFile))
            {
                QuestFile newFile = CreateFileFromTree(fileNode);
                SaveFile(newFile, true);
                fileNode.Tag = newFile;
                fileNode.Text = System.IO.Path.GetFileName(newFile.FileName) + " : " + newFile.Title;
            }
        }

        private void searchTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchForm search = new SearchForm(this);
            search.StartPosition = FormStartPosition.CenterParent;
            search.ShowDialog(this);

            if (search.foundNode != null)
            {
                treeView1.SelectedNode = null;
                treeView1.SelectedNode = search.foundNode;
            }

            search.Dispose();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!RemoveAllFiles())
                return;

            Application.Exit();
        }

            #endregion

            #region Edit Menu

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoManager.Undo(1);
            undoToolStripMenuItem.Enabled = UndoManager.IsUndoPossible();
            redoToolStripMenuItem.Enabled = UndoManager.IsRedoPossible();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoManager.Redo(1);
            undoToolStripMenuItem.Enabled = UndoManager.IsUndoPossible();
            redoToolStripMenuItem.Enabled = UndoManager.IsRedoPossible();
        }

            #endregion

            #region Tool Menu

        private void searchDuplicateTriggersToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("searchDuplicateTriggers");
            Dictionary<string, TreeNode> triggerNames = new Dictionary<string, TreeNode>();
            Dictionary<uint, string> triggerHashes = new Dictionary<uint, string>();
            foreach (TreeNode f in treeView1.Nodes)
            {
                foreach (TreeNode b in f.Nodes)
                {
                    foreach (TreeNode t in b.Nodes)
                    {
                        if (triggerNames.ContainsKey(t.Text.ToUpper()))
                        {
                            TreeNode found = triggerNames[t.Text.ToUpper()];
                            Console.WriteLine("Found duplicate trigger '{0}' at File[{1},{2}] and [{3},{4}]", t.Text, f.Text, b.Text, found.Parent.Parent.Text, found.Parent.Text);
                            t.EnsureVisible();
                            t.ExpandAll();
                            found.EnsureVisible();
                            found.ExpandAll();
                        }
                        else
                        {
                            triggerNames.Add(t.Text.ToUpper(), t);
                        }

                        uint hash = Util.StrToHashKey(t.Text);
                        if (triggerHashes.ContainsKey(hash))
                        {
                            string foundTrigger = triggerHashes[hash];
                            Console.WriteLine("Found duplicate hash '{0}' at {1} and {2}", hash, t.Text, foundTrigger);
                            MessageBox.Show(String.Format("Found duplicate hash '{0}' at {1} and {2}", hash, t.Text, foundTrigger));
                        }
                        else
                        {
                            triggerHashes.Add(hash, t.Text);
                        }
                    }
                }
            }
            Console.WriteLine("searchDuplicateTriggers done");
            MessageBox.Show("done");
        }

        private void convertStringToHashToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter a trigger name", "StringToHashKey", "", -1, -1);
            if (input.Length > 0)
            {
                uint hash = Util.StrToHashKey(input);
                Console.WriteLine("Input: " + input);
                Console.WriteLine("UINT: " + hash);
                Console.WriteLine("INT: " + (int)hash + " (database)");
                Console.WriteLine("HEX: " + hash.ToString("X"));
                MessageBox.Show("trigger: " + input + "\n" +
                                "UINT: " + hash + "\n" +
                                "INT: " + (int)hash + " (database)" + "\n" +
                                "HEX: " + hash.ToString("X"), "StringToHashKey");
            }
        }

        private void dumpHashesToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "txt|*.txt";

            if (save.ShowDialog() == DialogResult.OK)
            {
                FileHandler fh = new FileHandler(save.FileName, FileHandler.FileOpenMode.Writing, Encoding.GetEncoding("EUC-KR"));

                foreach (TreeNode f in treeView1.Nodes)
                {
                    fh.Write<string>(String.Format("{0}\n", f.Text));
                    foreach (TreeNode b in f.Nodes)
                    {
                        fh.Write<string>(String.Format("\t{0}\n", b.Text));
                        foreach (TreeNode t in b.Nodes)
                        {
                            uint hash = Util.StrToHashKey(t.Text);
                            fh.Write<string>(String.Format("\t\t{0}\t{1}\t{2}\t{3}\n", t.Text, hash, (int)hash, hash.ToString("X")));
                        }
                    }
                }
                fh.Close();
            }
        }

        #endregion

        #endregion

        #region Node Menus

            #region File Node Menu

        private void opt_mnuFile_AddBlock_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestFile))
                return;

            QuestBlock block = new QuestBlock();
            block.Name = "New Block";

            TreeNode node = new TreeNode(block.GetNodeText(m_MenuNode.Nodes.Count));
            node.Tag = block;
            m_MenuNode.Nodes.Add(node);
            this.SelectNode(node);

            // Clone is OK because the collections are empty
            AddAction(UnDoRedo.ActionType.Add_Block, node, block.Clone());
        }

            #endregion

            #region Block Node Menu

        private void copyBlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestBlock))
                return;

            QuestBlock copyBlock = CreateBlockFromTree(m_MenuNode);
            QuestBlock block = copyBlock.Clone();

            block.Name = copyBlock.Name + " Copy";
            TreeNode addedNode = AddToTree(block, m_MenuNode.Parent, -1);

            // trigger Clone is OK because the Collections are generated at copy
            AddAction(UnDoRedo.ActionType.Add_Block, addedNode, block.Clone());
        }

        private void deleteBlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestBlock))
                return;
            
            AddAction(UnDoRedo.ActionType.Del_Block, m_MenuNode, CreateBlockFromTree(m_MenuNode) );

            TreeNode parent = m_MenuNode.Parent;
            RemoveBlock(parent, m_MenuNode.Index);
            this.SelectNode(parent);
        }

        private void addTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestBlock))
                return;

            QuestTrigger trigger = new QuestTrigger();
            trigger.Name = "trigger-" + DateTime.UtcNow.Ticks.ToString("X").GetLast(8);
            trigger.CheckNext = false;

            TreeNode node = m_MenuNode.Nodes.Add(trigger.GetNodeText());
            node.Checked = trigger.CheckNext;
            node.Tag = trigger;

            node.Nodes.Add(new TreeNode("Conditions (0)"));
            node.Nodes.Add(new TreeNode("Rewards (0)"));

            Console.WriteLine("Added new trigger");
            this.SelectNode(node);

            // trigger Clone is OK because the Collections are empty
            AddAction(UnDoRedo.ActionType.Add_Trigger, node, trigger.Clone() );
        }

        private void pasteTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestBlock))
                return;
            if (m_CopyTrigger == null)
                return;

            QuestTrigger trigger = m_CopyTrigger.Clone();
            trigger.Name = "trigger-" + DateTime.UtcNow.Ticks.ToString("X").GetLast(8);
            TreeNode newTrigger = AddToTree(trigger, m_MenuNode, -1);

            this.SelectNode(newTrigger);

            // trigger Clone is OK because the Collections are generated at copy
            AddAction(UnDoRedo.ActionType.Add_Trigger, newTrigger, trigger.Clone() );
        }

            #endregion

            #region Trigger Node Menu

        private void opt_mnuTrigger_AddCondition_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestTrigger))
                return;

            Condition condition = QuestFile.CreateObject(ConditionType.COND000);

            TreeNode condition_node = AddToTree(condition, m_MenuNode.Nodes[0], -1);

            Console.WriteLine("Added condition");
            this.SelectNode(m_MenuNode);

            AddAction(UnDoRedo.ActionType.Add_Condition, condition_node, condition.Clone() );
        }

        private void opt_mnuTrigger_AddReward_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestTrigger))
                return;

            Reward reward = QuestFile.CreateObject(RewardType.REWD000);

            TreeNode reward_node = AddToTree(reward, m_MenuNode.Nodes[1], -1);

            Console.WriteLine("Added action");
            this.SelectNode(m_MenuNode);

            AddAction(UnDoRedo.ActionType.Add_Reward, reward_node, reward.Clone() );
        }

        private void opt_mnuTrigger_up_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestTrigger))
                return;

            TreeNode parent = m_MenuNode.Parent;
            int index = m_MenuNode.Index;

            if (index > 0)
            {
                parent.Nodes.RemoveAt(index);
                parent.Nodes.Insert(index - 1, m_MenuNode);
                this.SelectNode(m_MenuNode);

                AddAction(UnDoRedo.ActionType.Move_Up, m_MenuNode, m_MenuNode.Tag);
            }
        }

        private void opt_mnuTrigger_down_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestTrigger))
                return;

            TreeNode parent = m_MenuNode.Parent;
            int index = m_MenuNode.Index;

            if (index < parent.Nodes.Count - 1)
            {
                parent.Nodes.RemoveAt(index);
                parent.Nodes.Insert(index + 1, m_MenuNode);
                this.SelectNode(m_MenuNode);

                AddAction(UnDoRedo.ActionType.Move_Down, m_MenuNode, m_MenuNode.Tag);
            }
        }

        private void deleteTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestTrigger))
                return;

            AddAction(UnDoRedo.ActionType.Del_Trigger, m_MenuNode, CreateTriggerFromTree(m_MenuNode) );

            TreeNode parent = m_MenuNode.Parent;
            RemoveTrigger(parent, m_MenuNode.Index);
            this.SelectNode(parent);
        }

        private void copyTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestTrigger))
                return;

            m_CopyTrigger = CreateTriggerFromTree(m_MenuNode);
            pasteTriggerToolStripMenuItem.Text = "Paste " + m_CopyTrigger.Name;
        }

        private void checkNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_MenuNode == null || m_MenuNode.Tag.GetType() != typeof(QuestTrigger))
                return;

            QuestTrigger trigger_old = CreateTriggerFromTree(m_MenuNode);

            ((QuestTrigger)m_MenuNode.Tag).CheckNext = checkNextToolStripMenuItem.Checked;
            m_MenuNode.Checked = checkNextToolStripMenuItem.Checked;

            QuestTrigger trigger_new = CreateTriggerFromTree(m_MenuNode);

            m_MenuNode.Text = ((QuestTrigger)m_MenuNode.Tag).GetNodeText();

            AddAction(UnDoRedo.ActionType.Paste, m_MenuNode, trigger_old, trigger_new);
        }

            #endregion

        #endregion

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            if (m_SelectedTriggerNode != null)
            {
                m_SelectedTriggerNode.EnsureVisible();
                SelectNode(m_SelectedTriggerNode);
            }
        }

        public void AddAction(QuestStudio.UnDoRedo.ActionType action, TreeNode node, Object obj)
        {
            this.AddAction(action, node, obj, obj);
        }
        public void AddAction(QuestStudio.UnDoRedo.ActionType action, TreeNode node, Object object_old, Object object_new)
        {
            if (this.UndoManager.IsLocked())
                return;

            this.UndoManager.AddAction(action, node, object_old, object_new);

            TreeNode fileNode = GetParent(0, node);
            QuestFile file = (QuestFile)fileNode.Tag;
            file.Dirty = true;
            toolStripStatusLabel2.Text = file.FileName + " (unsaved)";
        }

        private void goToLineToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void insertMultipleRowsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox1 = new AboutBox1();
            aboutBox1.ShowDialog();
        }
    }
}