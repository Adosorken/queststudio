using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace QuestEditor
{
    public partial class GridForm : Form
    {
        public GridForm()
        {
            InitializeComponent();
        }

        public void Populate(QuestFile file)
        {
            gridBlocks.DataSource = file.Blocks;
            gridBlocks.Refresh();

            SelectBlock(0);
        }

        public void SelectBlock(int blockno)
        {
            gridTriggers.DataSource = new List<QuestTrigger>();

            if (gridBlocks.DataSource == null)
                return;

            Console.WriteLine("SelectBlock :: reload to trigger grid");

            List<QuestBlock> blockList = (List<QuestBlock>)gridBlocks.DataSource;
            if (blockList.Count > blockno)
            {
                gridTriggers.DataSource = blockList[blockno].Triggers;
                gridTriggers.Refresh();
                SelectTrigger(0);
            }
        }

        public void SelectTrigger(int triggerno)
        {
            if (gridTriggers.DataSource == null)
                return;

            Console.WriteLine("SelectTrigger :: reload to trigger grid");

            List<QuestTrigger> triggerList = (List<QuestTrigger>)gridTriggers.DataSource;
            if (triggerList.Count > triggerno)
            {
                QuestTrigger trigger = triggerList[triggerno];

                gridConditions.ColumnCount = 1;
                gridConditions.Columns[0].HeaderText = "Conditions";
                gridConditions.Rows.Clear();

                for (int row = 0; row < trigger.Conditions.Count; row++)
                {
                    gridConditions.Rows.Add();
                    Condition c = trigger.Conditions[row];

                    AddConditionToGrid(c, row);
                }

                gridConditions.Refresh();
            }
        }

        private void gridBlocks_SelectionChanged(object sender, EventArgs e)
        {
            Console.WriteLine("gridBlocks_SelectionChanged");
            SelectBlock(gridBlocks.CurrentRow.Index);
        }

        private void gridTriggers_SelectionChanged(object sender, EventArgs e)
        {
            Console.WriteLine("gridTriggers_SelectionChanged");
            SelectTrigger(gridTriggers.CurrentRow.Index);
        }

        bool AddConditionToGrid(Condition c, int r)
        {
            int col = 0;
            ConditionType cType = c.getConditionType();

            switch (cType)
            {
                case ConditionType.COND000:
                    return AddObjectList(ref gridConditions, ref col, r, cType, "", "Quest ID:", ((Cond000)c).questid);
                case ConditionType.COND001:
                    return AddObjectList(ref gridConditions, ref col, r, cType, "", "Quest Checks:", ((Cond001)c).data.Length, "", ((Cond001)c).data);
                case ConditionType.COND002:
                    return AddObjectList(ref gridConditions, ref col, r, cType, "", "Variable Checks:", ((Cond002)c).data.Length, "", ((Cond002)c).data);
                case ConditionType.COND003:
                    return AddObjectList(ref gridConditions, ref col, r, cType, "", "Ability Checks:", ((Cond003)c).data.Length, "", ((Cond003)c).data);
                case ConditionType.COND004:
                    return AddObjectList(ref gridConditions, ref col, r, cType, "", "Item Checks:", ((Cond004)c).data.Length, "", ((Cond004)c).data);
                
                case ConditionType.COND013:
                    return AddObjectList(ref gridConditions, ref col, r, cType, "", "NPC:", "", ((Cond013)c).NpcNo);
                  
                default:
                    return AddObjectList(ref gridConditions, ref col, r, cType, "", (object)c);
            }
        }

        public bool AddObjectList(ref DataGridView grid, ref int col, int row, params object[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                grid.Columns.Add("","");

                object Obj = list[i];
                Type t = Obj.GetType();

                if (t == typeof(sbyte) ||
                    t == typeof(short) ||
                    t == typeof(int) ||
                    t == typeof(long) ||
                    t == typeof(byte) ||
                    t == typeof(ushort) ||
                    t == typeof(uint) ||
                    t == typeof(ulong))
                {
                    DataGridViewTextBoxCell TextBoxCell = new DataGridViewTextBoxCell();
                    grid[col, row] = TextBoxCell;
                    grid[col, row].ValueType = Obj.GetType();
                    grid[col, row].Value = Obj;
                }
                else if (t.IsArray)
                {
                    Array dataArray = (Array)Obj;
                    foreach (object o in dataArray)
                        AddObjectList(ref grid, ref col, row, o);
                }
                else if (t.IsEnum)
                {
                    DataGridViewComboBoxCell ComboCell = new DataGridViewComboBoxCell();
                    ComboCell.DataSource = Enum.GetValues(t);
                    grid[col, row] = ComboCell;
                    grid[col, row].ValueType = t;
                    grid[col, row].Value = Obj;
                }
                else if (t == typeof(string))
                {
                    DataGridViewTextBoxCell TextBoxCel = new DataGridViewTextBoxCell();
                    grid[col, row] = TextBoxCel;
                    grid[col, row].Value = Obj;
                    grid[col, row].ReadOnly = true;
                }
                else if (t == typeof(ITEM_DATA))
                {
                    ITEM_DATA itemData = (ITEM_DATA)Obj;
                    AddObjectList(ref grid, ref col, row, "Item", itemData.itemsn, itemData.op, itemData.cnt, "slot", itemData.equipslot);
                }
                else if (t == typeof(QUEST_DATA))
                {
                    QUEST_DATA questData = (QUEST_DATA)Obj;
                    AddObjectList(ref grid, ref col, row, questData.VarType, questData.VarNo, questData.op, questData.Value);
                }
                else if (t == typeof(ABIL_DATA))
                {
                    ABIL_DATA abilData = (ABIL_DATA)Obj;
                    AddObjectList(ref grid, ref col, row, abilData.type, abilData.op, abilData.value);
                }
                else
                {
                    Console.WriteLine("Unknown Column Type: " + t);
                    AddPropertiesToGrid(ref grid, ref col, row, (object)Obj);
                }
                col++;
            }
            return true;
        }

        void AddPropertiesToGrid(ref DataGridView grid, ref int col, int row, object Obj)
        {
            PropertyInfo[] myFieldInfo = Obj.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo f in myFieldInfo)
            {
                grid.Columns.Add("", "");
                Console.WriteLine("[{0},{1}] Name: {4}, FieldType: {2}, Value: {3}", row, col, f.PropertyType, f.GetValue(Obj, null), f.Name);

                AddObjectList(ref grid, ref col, row, f.Name, f.GetValue(Obj, null));
                col++;
            }
        }

        private void gridConditions_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void gridConditions_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void grid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            for (int i = 0; i < grid.Rows.Count; i++)
                grid.Rows[i].HeaderCell.Value = i.ToString();
        }
    }
}
