using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QuestStudio.Tree_Extensions;

namespace QuestStudio
{
    interface IUndoRedo
    {
        void Undo(int level);
        void Redo(int level);
        void InsertObjectforUndoRedo(ChangeRepresentationObject dataobject);
    }

    public partial class UnDoRedo : IUndoRedo
    {
        public enum ActionType
        {
            Add_Block,//ok
            Del_Block,//ok

            Add_Trigger,//ok
            Del_Trigger,//ok

            Add_Condition,//ok
            Del_Condition,//ok

            Add_Reward,//ok
            Del_Reward,//ok

            Rename_File,//ok
            Rename_Block,//ok
            Rename_Trigger,//ok

            Move_Up,//ok
            Move_Down,//ok

            Paste, //ok - replaces the entire trigger, also for CheckNext edit

            Edit_Condition,//ok
            Edit_Reward,//ok
        };

        private Stack<ChangeRepresentationObject> _UndoActionsCollection = new Stack<ChangeRepresentationObject>();
        private Stack<ChangeRepresentationObject> _RedoActionsCollection = new Stack<ChangeRepresentationObject>();
        public event EventHandler EnableDisableUndoRedoFeature;

        public TreeLE tree { get; set; }
        public TreeForm form { get; set; }
        
        private bool locked = false;
        public void Lock() { locked = true; }
        public void Unlock() { locked = false; }
        public bool IsLocked() { return locked; }

        private void AddBlock(int fileid, int blockid, QuestBlock block, bool Expand)
        {
            TreeNode newNode = form.AddToTree(block, tree.Nodes[fileid], blockid);
            form.SelectNode(newNode);

            if (Expand) newNode.Expand();
        }
        private void DelBlock(int fileid, int blockid)
        {
            form.RemoveBlock(tree.Nodes[fileid], blockid);
            form.SelectNode(tree.Nodes[fileid]);
        }
        private void AddTrigger(int fileid, int blockid, int triggerid, QuestTrigger trigger, bool Expand)
        {
            TreeNode newNode = form.AddToTree(trigger, tree.Nodes[fileid].Nodes[blockid], triggerid);
            form.SelectNode(newNode);

            if (Expand) newNode.ExpandAll();
        }
        private void DelTrigger(int fileid, int blockid, int triggerid)
        {
            form.RemoveTrigger(tree.Nodes[fileid].Nodes[blockid], triggerid);
            form.SelectNode(tree.Nodes[fileid].Nodes[blockid]);
        }
        private void AddCondition(int fileid, int blockid, int triggerid, int conditionid, Condition c, bool Expand)
        {
            TreeNode newNode = form.AddToTree(c, tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid].Nodes[0], conditionid);
            form.SelectNode(newNode.Parent.Parent);

            if (Expand) newNode.ExpandAll();
        }
        private void DelCondition(int fileid, int blockid, int triggerid, int conditionid)
        {
            form.RemoveCondition(tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid].Nodes[0], conditionid);
            form.SelectNode(tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid]);
        }
        private void AddReward(int fileid, int blockid, int triggerid, int rewardid, Reward r, bool Expand)
        {
            TreeNode newNode = form.AddToTree(r, tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid].Nodes[1], rewardid);
            form.SelectNode(newNode.Parent.Parent);

            if (Expand) newNode.ExpandAll();
        }
        private void DelReward(int fileid, int blockid, int triggerid, int rewardid)
        {
            form.RemoveReward(tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid].Nodes[1], rewardid);
            form.SelectNode(tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid]);
        }
        private void RenameFile(int fileid, string title)
        {
            TreeNode node = tree.Nodes[fileid];
            QuestFile file = (QuestFile)node.Tag;
            node.Text = System.IO.Path.GetFileName(file.FileName) + " : " + title;
            file.Title = title;
            form.SelectNode(node);
        }
        private void RenameBlock(int fileid, int blockid, string name)
        {
            TreeNode node = tree.Nodes[fileid].Nodes[blockid];
            QuestBlock block = (QuestBlock)node.Tag;
            block.Name = name;
            node.Text = block.GetNodeText(blockid);
            form.SelectNode(node);
        }
        private void RenameTrigger(int fileid, int blockid, int triggerid, string name)
        {
            TreeNode node = tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid];
            QuestTrigger trigger = (QuestTrigger)node.Tag;
            trigger.Name = name;
            node.Text = trigger.GetNodeText();
            form.SelectNode(node);
        }
        private void EditCondition(int fileid, int blockid, int triggerid, int conditionid, Condition condition)
        {
            TreeNode node = tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid].Nodes[0].Nodes[conditionid];
            node.Tag = condition.Clone();
            node.Text = Condition.getNodeText(condition);
            form.SelectNode(node.Parent.Parent);
        }
        private void EditReward(int fileid, int blockid, int triggerid, int rewardid, Reward reward)
        {
            TreeNode node = tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid].Nodes[1].Nodes[rewardid];
            node.Tag = reward.Clone();
            node.Text = Reward.getNodeText(reward);
            form.SelectNode(node.Parent.Parent);
        }
        private void EditTrigger(int fileid, int blockid, int triggerid, QuestTrigger trigger, bool Expand)
        {
            tree.BeginUpdate();
            DelTrigger(fileid, blockid, triggerid);
            AddTrigger(fileid, blockid, triggerid, trigger.Clone(), Expand);
            tree.EndUpdate();
        }

        private void MoveNodeUp(int fileid, int blockid, int triggerid, object o)
        {
            tree.BeginUpdate();

            TreeNode node = null;
            if (o is QuestTrigger)
                node = tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid];
            else
                throw new FooException("Move up/down not supported by other nodes");

            TreeNode parent = node.Parent;
            int index = node.Index;
            if (index > 0)
            {
                parent.Nodes.RemoveAt(index);
                parent.Nodes.Insert(index - 1, node);
                form.SelectNode(node);
            }

            tree.EndUpdate();
        }

        private void MoveNodeDown(int fileid, int blockid, int triggerid, object o)
        {
            tree.BeginUpdate();

            TreeNode node = null;
            if (o is QuestTrigger)
                node = tree.Nodes[fileid].Nodes[blockid].Nodes[triggerid];
            else
                throw new FooException("Move up/down not supported by other nodes");

            TreeNode parent = node.Parent;
            int index = node.Index;
            if (index < parent.Nodes.Count - 1)
            {
                parent.Nodes.RemoveAt(index);
                parent.Nodes.Insert(index + 1, node);
                form.SelectNode(node);
            }

            tree.EndUpdate();
        }

        public void Undo(int level)
        {
            for (int i = 1; i <= level; i++)
            {
                if (_UndoActionsCollection.Count == 0) return;

                ChangeRepresentationObject Undostruct = _UndoActionsCollection.Pop();
                Console.WriteLine("Undo {0} {1}", Undostruct.id, Undostruct.Action);

                if (Undostruct.Action == ActionType.Add_Block)
                {
                    DelBlock(Undostruct.file_idx, Undostruct.block_idx);
                }
                else if (Undostruct.Action == ActionType.Del_Block)
                {
                    AddBlock(Undostruct.file_idx, Undostruct.block_idx, (QuestBlock)Undostruct.obj_old, Undostruct.expanded);
                }
                else if (Undostruct.Action == ActionType.Add_Trigger)
                {
                    DelTrigger(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx);
                }
                else if (Undostruct.Action == ActionType.Del_Trigger)
                {
                    AddTrigger(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, (QuestTrigger)Undostruct.obj_old, Undostruct.expanded);
                }
                else if (Undostruct.Action == ActionType.Add_Condition)
                {
                    DelCondition(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, Undostruct.condition_idx);
                }
                else if (Undostruct.Action == ActionType.Del_Condition)
                {
                    AddCondition(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, Undostruct.condition_idx, (Condition)Undostruct.obj_old, Undostruct.expanded);
                }
                else if (Undostruct.Action == ActionType.Add_Reward)
                {
                    DelReward(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, Undostruct.reward_idx);
                }
                else if (Undostruct.Action == ActionType.Del_Reward)
                {
                    AddReward(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, Undostruct.reward_idx, (Reward)Undostruct.obj_old, Undostruct.expanded);
                }
                else if (Undostruct.Action == ActionType.Rename_File)
                {
                    RenameFile(Undostruct.file_idx, (string)Undostruct.obj_old);
                }
                else if (Undostruct.Action == ActionType.Rename_Block)
                {
                    RenameBlock(Undostruct.file_idx, Undostruct.block_idx, (string)Undostruct.obj_old);
                }
                else if (Undostruct.Action == ActionType.Rename_Trigger)
                {
                    RenameTrigger(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, (string)Undostruct.obj_old);
                }
                else if (Undostruct.Action == ActionType.Edit_Condition)
                {
                    EditCondition(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, Undostruct.condition_idx, (Condition)Undostruct.obj_old);
                }
                else if (Undostruct.Action == ActionType.Edit_Reward)
                {
                    EditReward(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, Undostruct.reward_idx, (Reward)Undostruct.obj_old);
                }
                else if (Undostruct.Action == ActionType.Paste)
                {
                    EditTrigger(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, (QuestTrigger)Undostruct.obj_old, Undostruct.expanded);
                }
                else if (Undostruct.Action == ActionType.Move_Up)
                {
                    MoveNodeDown(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, Undostruct.obj_old);
                    Undostruct.trigger_idx++;
                }
                else if (Undostruct.Action == ActionType.Move_Down)
                {
                    MoveNodeUp(Undostruct.file_idx, Undostruct.block_idx, Undostruct.trigger_idx, Undostruct.obj_old);
                    Undostruct.trigger_idx--;
                }
                _RedoActionsCollection.Push(Undostruct);
            }
        }

        public void Redo(int level)
        {
            for (int i = 1; i <= level; i++)
            {
                if (_RedoActionsCollection.Count == 0) return;

                ChangeRepresentationObject Redostruct = _RedoActionsCollection.Pop();
                Console.WriteLine("Redo {0} {1}", Redostruct.id, Redostruct.Action);

                if (Redostruct.Action == ActionType.Add_Block)
                {
                    AddBlock(Redostruct.file_idx, Redostruct.block_idx, (QuestBlock)Redostruct.obj_new, Redostruct.expanded);
                }
                else if (Redostruct.Action == ActionType.Del_Block)
                {
                    DelBlock(Redostruct.file_idx, Redostruct.block_idx);
                }
                else if (Redostruct.Action == ActionType.Add_Trigger)
                {
                    AddTrigger(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, (QuestTrigger)Redostruct.obj_new, Redostruct.expanded);
                }
                else if (Redostruct.Action == ActionType.Del_Trigger)
                {
                    DelTrigger(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx);
                }
                else if (Redostruct.Action == ActionType.Add_Condition)
                {
                    AddCondition(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, Redostruct.condition_idx, (Condition)Redostruct.obj_new, Redostruct.expanded);
                }
                else if (Redostruct.Action == ActionType.Del_Condition)
                {
                    DelCondition(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, Redostruct.condition_idx);
                }
                else if (Redostruct.Action == ActionType.Add_Reward)
                {
                    AddReward(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, Redostruct.reward_idx, (Reward)Redostruct.obj_new, Redostruct.expanded);
                }
                else if (Redostruct.Action == ActionType.Del_Reward)
                {
                    DelReward(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, Redostruct.reward_idx);
                }
                else if (Redostruct.Action == ActionType.Rename_File)
                {
                    RenameFile(Redostruct.file_idx, (string)Redostruct.obj_new);
                }
                else if (Redostruct.Action == ActionType.Rename_Block)
                {
                    RenameBlock(Redostruct.file_idx, Redostruct.block_idx, (string)Redostruct.obj_new);
                }
                else if (Redostruct.Action == ActionType.Rename_Trigger)
                {
                    RenameTrigger(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, (string)Redostruct.obj_new);
                }
                else if (Redostruct.Action == ActionType.Edit_Condition)
                {
                    EditCondition(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, Redostruct.condition_idx, (Condition)Redostruct.obj_new);
                }
                else if (Redostruct.Action == ActionType.Edit_Reward)
                {
                    EditReward(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, Redostruct.reward_idx, (Reward)Redostruct.obj_new);
                }
                else if (Redostruct.Action == ActionType.Paste)
                {
                    EditTrigger(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, (QuestTrigger)Redostruct.obj_new, Redostruct.expanded);
                }
                else if (Redostruct.Action == ActionType.Move_Up)
                {
                    MoveNodeUp(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, Redostruct.obj_old);
                    Redostruct.trigger_idx--;
                }
                else if (Redostruct.Action == ActionType.Move_Down)
                {
                    MoveNodeDown(Redostruct.file_idx, Redostruct.block_idx, Redostruct.trigger_idx, Redostruct.obj_old);
                    Redostruct.trigger_idx++;
                }
                _UndoActionsCollection.Push(Redostruct);
            }
        }

        public void AddAction(ActionType action, TreeNode node, Object object_old, Object object_new)
        {
            if (locked)
                return;

            ChangeRepresentationObject obj = new ChangeRepresentationObject();
            obj.Action = action;
            obj.id = (new Random()).Next();
            obj.expanded = node.IsExpanded;

            Console.WriteLine("[ADD] {0} {1} {2}", obj.id, obj.Action, node.Text);

            while (node != null)
            {
                if (node.Level == 0)
                    obj.file_idx = node.Index;
                else if (node.Level == 1)
                    obj.block_idx = node.Index;
                else if (node.Level == 2)
                    obj.trigger_idx = node.Index;
                else if (node.Level == 4)
                {
                    if (object_old is Condition && object_new is Condition)
                        obj.condition_idx = node.Index;
                    else if (object_old is Reward && object_new is Reward)
                        obj.reward_idx = node.Index;
                    else
                        throw new FooException("Invalid object type at level 4");
                }

                node = node.Parent;
            }

            obj.obj_new = object_new;
            obj.obj_old = object_old;

            _UndoActionsCollection.Push(obj);
            form.undoToolStripMenuItem.Enabled = true;
        }

        public void InsertObjectforUndoRedo(ChangeRepresentationObject dataobject)
        {
            _UndoActionsCollection.Push(dataobject);
            _RedoActionsCollection.Clear();
            if (EnableDisableUndoRedoFeature != null)
            {
                EnableDisableUndoRedoFeature(null, null);
            }
        }

        public bool IsUndoPossible()
        {
            if (_UndoActionsCollection.Count != 0)
                return true;
            else
                return false;
        }

        public bool IsRedoPossible()
        {
            if (_RedoActionsCollection.Count != 0)
                return true;
            else
                return false;
        }
    }

    public class ChangeRepresentationObject
    {
        public UnDoRedo.ActionType Action;
        public int file_idx;
        public int block_idx;
        public int trigger_idx;
        public int condition_idx;
        public int reward_idx;

        public object obj_new;
        public object obj_old;
        public bool expanded;

        public int id;
    };
}
