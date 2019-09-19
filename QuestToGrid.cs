using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Drawing;

namespace QuestStudio
{
    public partial class TreeForm : Form
    {
        void ShowTriggerInGrids(QuestTrigger trigger, TreeNode triggerNode)
        {
          //  Console.WriteLine("ShowTriggerInGrids, Selected node");
            m_SelectedTriggerNode = triggerNode;

            gridCondition.ColumnCount = 1;
            gridCondition.Columns[0].HeaderText = "Conditions";
            gridCondition.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            gridCondition.Rows.Clear();

            gridReward.ColumnCount = 1;
            gridReward.Columns[0].HeaderText = "Rewards";
            gridReward.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            gridReward.Rows.Clear();

            if (trigger == null)
                toolStripStatusLabel1.Text = "";
            else
            {
                toolStripStatusLabel1.Text = "Active trigger: " +  ((QuestTrigger)triggerNode.Tag).Name;

                locked_edit = true;
                {
                    for (int row = 0; row < triggerNode.Nodes[0].Nodes.Count; row++)
                    {
                        gridCondition.Rows.Add();
                        int col = 0;
                        ObjectToGrid(gridCondition, ref col, row, triggerNode.Nodes[0].Nodes[row].Tag);
                    }

                    for (int row = 0; row < triggerNode.Nodes[1].Nodes.Count; row++)
                    {
                        gridReward.Rows.Add();
                        int col = 0;
                        ObjectToGrid(gridReward, ref col, row, triggerNode.Nodes[1].Nodes[row].Tag);
                    }
                }
                locked_edit = false;
            }

            for (int col = 0; col < gridCondition.Columns.Count; col++)
                gridCondition.Columns[col].SortMode = DataGridViewColumnSortMode.NotSortable;

            for (int col = 0; col < gridReward.Columns.Count; col++)
                gridReward.Columns[col].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

#region Defintions

        public object pr(Expression<Func<object>> expression)
        {
            PropertyInfo propertyInfo;

            if (expression.Body is UnaryExpression)
                propertyInfo = ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member as PropertyInfo;
            else
                propertyInfo = ((MemberExpression)expression.Body).Member as PropertyInfo;

            if (propertyInfo == null)
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");

            return propertyInfo;
        }

        object[] GetObjectCellOrder(object o)
        {
            if (o is Condition)
            {
                Condition c = (Condition)o;
                switch (c.getConditionType())
                {
                    case ConditionType.COND000:
                        return new object[] { "Quest ID:", pr(() => ((Cond000)c).questid) };
                    case ConditionType.COND001:
                        return new object[] { "Quest Checks:", pr(() => ((Cond001)c).data) };
                    case ConditionType.COND002:
                        return new object[] { "Variable Checks:", pr(() => ((Cond002)c).data) };
                    case ConditionType.COND003:
                        return new object[] { "Ability Checks:", pr(() => ((Cond003)c).data) };
                    case ConditionType.COND004:
                        return new object[] { "Item Checks:", pr(() => ((Cond004)c).data) };
                    case ConditionType.COND005:
                        return new object[] { "Minimum party level:", pr(()=>((Cond005)c).MinimumLevel), null, 
                                          "Party leader", pr(()=>((Cond005)c).IsLeader) };
                    case ConditionType.COND006:
                        return new object[] { "Zone:", pr(()=>((Cond006)c).ZoneNo), null, 
                                          "X:", pr(()=>((Cond006)c).Y), null, 
                                          "Y:", pr(()=>((Cond006)c).Y), null, 
                                          "Range:", pr(()=>((Cond006)c).Radius) };
                    case ConditionType.COND007: // unused
                        return new object[] { };
                    case ConditionType.COND008:
                        return new object[] { pr(() => ((Cond008)c).Operator), pr(() => ((Cond008)c).Time) };
                    case ConditionType.COND009:
                        return new object[] { "Skill between:", pr(()=>((Cond009)c).SkillNo1), "and", pr(()=>((Cond009)c).SkillNo2), null, 
                                          "Has skill?", pr(()=>((Cond009)c).Operator) };
                    case ConditionType.COND010:
                        return new object[] { "Random percent between:", pr(() => ((Cond010)c).MinimumPercent), "and", pr(() => ((Cond010)c).MaximumPercent) };
                    case ConditionType.COND011:
                        return new object[] { "Type:", pr(() => ((Cond011)c).Who), null, 
                                          "Var:", pr(() => ((Cond011)c).VarNo), pr(() => ((Cond011)c).Operator), pr(() => ((Cond011)c).Value) };
                    case ConditionType.COND012:
                        return new object[] { "Zone:", pr(()=>((Cond012)c).Zone), null, 
                                          "X:", pr(()=>((Cond012)c).X), null,
                                          "Y:", pr(()=>((Cond012)c).Y), null,
                                          "Event ID:", pr(()=>((Cond012)c).EventID) };
                    case ConditionType.COND013:
                        return new object[] { "NPC:", pr(() => ((Cond013)c).NpcNo) };
                    case ConditionType.COND014:
                        return new object[] { "Switch:", pr(() => ((Cond014)c).SwitchNo), pr(() => ((Cond014)c).Operator) };
                    case ConditionType.COND015:
                        return new object[] { "Member count from:", pr(() => ((Cond015)c).Number1), "to", pr(() => ((Cond015)c).Number2) };
                    case ConditionType.COND016:
                        return new object[] { "Type:", pr(() => ((Cond016)c).Who), null,
                                          "Start time:", pr(() => ((Cond016)c).TimeStart), null,
                                          "End time:", pr(() => ((Cond016)c).TimeEnd) };
                    case ConditionType.COND017:
                        return new object[] { "NPC:", pr(() => ((Cond017)c).npcno1), 
                                          "Var:", pr(() => ((Cond017)c).varno1), 
                                          pr(() => ((Cond017)c).Operator), 
                                          "NPC:", pr(() => ((Cond017)c).npcno2), 
                                          "Var:", pr(() => ((Cond017)c).varno2) };
                    case ConditionType.COND018:
                        return new object[] { "Day:", pr(() => ((Cond018)c).DayOfMonth), null, 
                                          "Time:", pr(() => ((Cond018)c).HourLow), ":", pr(() => ((Cond018)c).MinuteLow), 
                                          "-", pr(() => ((Cond018)c).HourHigh), ":",  pr(() => ((Cond018)c).MinuteHigh) };
                    case ConditionType.COND019: // unused
                        return new object[] { };
                    case ConditionType.COND020: // unused
                        return new object[] { };
                    case ConditionType.COND021:
                        return new object[] { "Maximum distance to selected", pr(() => ((Cond021)c).ObjectType), pr(() => ((Cond021)c).Radius) };
                    case ConditionType.COND022:
                        return new object[] { "Channel is between", pr(() => ((Cond022)c).Min), "and", pr(() => ((Cond022)c).Max) };
                    case ConditionType.COND023:
                        return new object[] { "User has a clan?", pr(() => ((Cond023)c).Operator) };
                    case ConditionType.COND024:
                        return new object[] { "Rank in clan", pr(() => ((Cond024)c).Operator), pr(() => ((Cond024)c).ClanPos) };
                    case ConditionType.COND025:
                        return new object[] { "Clan contribution:", pr(() => ((Cond025)c).Operator), pr(() => ((Cond025)c).ClanContribute) };
                    case ConditionType.COND026:
                        return new object[] { "Clan grade:", pr(() => ((Cond026)c).Operator), pr(() => ((Cond026)c).ClanGrade) };
                    case ConditionType.COND027:
                        return new object[] { "Clan points:", pr(() => ((Cond027)c).Operator), pr(() => ((Cond027)c).ClanPoint) };
                    case ConditionType.COND028:
                        return new object[] { "Clan money:", pr(() => ((Cond028)c).Operator), pr(() => ((Cond028)c).ClanMoney) };
                    case ConditionType.COND029:
                        return new object[] { "Clan member count:", pr(() => ((Cond029)c).Operator), pr(() => ((Cond029)c).MemberCount) };
                    case ConditionType.COND030:
                        return new object[] { "Clan skill between:", pr(() => ((Cond030)c).Skill1), "and", pr(() => ((Cond030)c).Skill2), null, 
                                          "Clan has skill?", pr(() => ((Cond030)c).Operator) };

                    case ConditionType.COND050:
                        return new object[] { "Quest between", pr(() => ((Cond050)c).Quest1), "and", pr(() => ((Cond050)c).Quest2) };
                    case ConditionType.COND051:
                        return new object[] { "Arena group:", pr(() => ((Cond051)c).Has) };
                    case ConditionType.COND052:
                        return new object[] { "Arena status", pr(() => ((Cond052)c).Operator), "NPC event value" };
                    case ConditionType.COND053:
                        return new object[] { "Switch", pr(() => ((Cond053)c).On) };
                    case ConditionType.COND054:
                        return new object[] { "Arena game:", pr(() => ((Cond054)c).IdOrType), pr(() => ((Cond054)c).Game) };
                    default:
                        break;
                }
            }
            else if (o is Reward)
            {
                Reward r = (Reward)o;
                switch (r.getRewardType())
                {
                    case RewardType.REWD000:
                        return new object[] { pr(() => ((Rewd000)r).Operator), pr(() => ((Rewd000)r).questid) };
                    case RewardType.REWD001:
                        return new object[] { pr(() => ((Rewd001)r).Action), pr(() => ((Rewd001)r).Amount), "of item ID", pr(() => ((Rewd001)r).ItemSN) };
                    case RewardType.REWD002:
                        return new object[] { "Quest variables:", pr(() => ((Rewd002)r).data) };
                    case RewardType.REWD003:
                        return new object[] { "Abilities:", pr(() => ((Rewd003)r).data) };
                    case RewardType.REWD004:
                        return new object[] { "User variables:", pr(() => ((Rewd004)r).data) };
                    case RewardType.REWD005:
                        return new object[] { "Give", pr(()=>((Rewd005)r).Value), pr(()=>((Rewd005)r).Target), pr(()=>((Rewd005)r).ItemSN), 
                                          "stat:", pr(()=>((Rewd005)r).ItemOpt), "equation:",  pr(()=>((Rewd005)r).Equation) };
                    case RewardType.REWD006:
                        return new object[] { "HP%", pr(() => ((Rewd006)r).PercentOfHP), 
                                          "MP%", pr(() => ((Rewd006)r).PercentOfMP) };
                    case RewardType.REWD007:
                        return new object[] { pr(() => ((Rewd007)r).PartyOpt),  "Zone:", pr(() => ((Rewd007)r).ZoneNo), 
                                                                            "X:", pr(() => ((Rewd007)r).X), 
                                                                            "Y:", pr(() => ((Rewd007)r).Y) };
                    case RewardType.REWD008:
                        return new object[] { "Spawn:", pr(() => ((Rewd008)r).HowMany), 
                                          "of mob ID", pr(() => ((Rewd008)r).MonsterID), 
                                          "at", pr(() => ((Rewd008)r).Where) , 
                                          "range:", pr(() => ((Rewd008)r).Range), 
                                          "team:", pr(() => ((Rewd008)r).TeamNo), 
                                          null,
                                          "Zone:", pr(() => ((Rewd008)r).ZoneNo), "X:", pr(() => ((Rewd008)r).X) , "Y:", pr(() => ((Rewd008)r).Y) };
                    case RewardType.REWD009:
                        return new object[] { "Trigger:", pr(() => ((Rewd009)r).NextTriggerSN) };
                    case RewardType.REWD010:
                        return new object[] { "Reset player stats." };
                    case RewardType.REWD011:
                        return new object[] { "Type:", pr(() => ((Rewd011)r).Who), null,
                                          "Var:", pr(() => ((Rewd011)r).VarNo), pr(() => ((Rewd011)r).Operator), pr(() => ((Rewd011)r).Value)};
                    case RewardType.REWD012:
                        return new object[] { pr(() => ((Rewd012)r).MsgType), "String ID:", pr(() => ((Rewd012)r).StringID) };
                    case RewardType.REWD013:
                        return new object[] { "Selected", pr(() => ((Rewd013)r).Who), 
                                          "Trigger:", pr(() => ((Rewd013)r).Trigger), "after", pr(() => ((Rewd013)r).Sec), "seconds" };
                    case RewardType.REWD014:
                        return new object[] { pr(() => ((Rewd014)r).Operator), pr(() => ((Rewd014)r).SkillNo) };
                    case RewardType.REWD015:
                        return new object[] { "Switch:", pr(() => ((Rewd015)r).QuestSwitch), pr(() => ((Rewd015)r).OnOff) };
                    case RewardType.REWD016:
                        return new object[] { "Switch group:", pr(() => ((Rewd016)r).QuestSwitchGroup) };
                    case RewardType.REWD017:
                        return new object[] { "Reset player switches." };
                    case RewardType.REWD018:
                        return new object[] { "BUGGED!", pr(() => ((Rewd018)r).StrID) };
                    case RewardType.REWD019:
                        return new object[] { "Zone:", pr(() => ((Rewd019)r).ZoneNo), 
                                          "Team:", pr(() => ((Rewd019)r).TeamNo) , 
                                          "Trigger:", pr(() => ((Rewd019)r).TriggerName) };

                    case RewardType.REWD020:
                        return new object[] { "Team:", pr(() => ((Rewd020)r).TeamType) };
                    case RewardType.REWD021:
                        return new object[] { "X:", pr(() => ((Rewd021)r).X),
                                          "Y:", pr(() => ((Rewd021)r).Y)};
                    case RewardType.REWD022:
                        return new object[] { "Zone:", pr(() => ((Rewd022)r).ZoneNo), "Mob regeneration:", pr(() => ((Rewd022)r).Operator) };
                    case RewardType.REWD023:
                        return new object[] { "Clan level up!" };
                    case RewardType.REWD024:
                        return new object[] { "Clan money:", pr(() => ((Rewd024)r).Operator), pr(() => ((Rewd024)r).ClanMoney) };
                    case RewardType.REWD025:
                        return new object[] { "Clan points:", pr(() => ((Rewd025)r).Operator), pr(() => ((Rewd025)r).ClanPoints) };
                    case RewardType.REWD026:
                        return new object[] { "Clan skill:", pr(() => ((Rewd026)r).Operator), "skill", pr(() => ((Rewd026)r).ClanSkillNo) };
                    case RewardType.REWD027:
                        return new object[] { "Clan contribution:", pr(() => ((Rewd027)r).Operator), pr(() => ((Rewd027)r).ClanContribute) };
                    case RewardType.REWD028:
                        return new object[] { "Zone:", pr(() => ((Rewd028)r).ZoneNo), 
                                          "X:", pr(() => ((Rewd028)r).X),
                                          "Y:", pr(() => ((Rewd028)r).Y),
                                          "Range:", pr(() => ((Rewd028)r).Range) };
                    case RewardType.REWD029:
                        return new object[] { "Lua function:", pr(() => ((Rewd029)r).ScriptName) };
                    case RewardType.REWD030:
                        return new object[] { "Reset player skills." };
                    case RewardType.REWD031:
                        return new object[] { "BUGGED!", pr(() => ((Rewd031)r).Var) };
                    case RewardType.REWD032:
                        return new object[] { "BUGGED!", pr(() => ((Rewd032)r).ItemSN), pr(() => ((Rewd032)r).PartyOpt_Bugged) };
                    case RewardType.REWD033:
                        return new object[] { "BUGGED!", pr(() => ((Rewd033)r).NextRewardSplitter) };
                    case RewardType.REWD034:
                        return new object[] { pr(() => ((Rewd034)r).HideShowToggle) };

                    case RewardType.REWD050:
                        return new object[] { "BreakId:", pr(() => ((Rewd050)r).BreakID) };
                    case RewardType.REWD051:
                        return new object[] { "Type:", pr(() => ((Rewd051)r).MsgType), "Msg:", pr(() => ((Rewd051)r).Message) };
                    case RewardType.REWD052:
                        return new object[] { "Switch", pr(() => ((Rewd052)r).On) };
                    default:
                        break;
                }
            }
            else if (o is ITEM_DATA_CONDITION)
            {
                ITEM_DATA_CONDITION d = (ITEM_DATA_CONDITION)o;
                return new object[] { "Item", pr(() => d.itemsn), pr(() => d.op), pr(() => d.cnt), "slot", pr(() => d.equipslot) };
            }
            else if (o is QUEST_DATA_CONDITION)
            {
                QUEST_DATA_CONDITION d = (QUEST_DATA_CONDITION)o;
                return new object[] { pr(() => d.VarType), pr(() => d.VarNo), pr(() => d.op), pr(() => d.Value) };
            }
            else if (o is ABIL_DATA_CONDITION)
            {
                ABIL_DATA_CONDITION d = (ABIL_DATA_CONDITION)o;
                return new object[] { pr(() => d.type), pr(() => d.op), pr(() => d.value) };
            }
            else if (o is QUEST_DATA_REWARD)
            {
                QUEST_DATA_REWARD d = (QUEST_DATA_REWARD)o;
                return new object[] { pr(() => d.VarType), pr(() => d.VarNo), pr(() => d.op), pr(() => d.Value) };
            }
            else if (o is ABIL_DATA_REWARD)
            {
                ABIL_DATA_REWARD d = (ABIL_DATA_REWARD)o;
                return new object[] { pr(() => d.type), pr(() => d.op), pr(() => d.value) };
            }
            return new object[] { o };
        }

#endregion

#region Object to Grid
        void ObjectToGrid(DataGridView grid, ref int col, int row, object obj)
        {
            if (obj is Condition)
            {
                AddObjectToRow(grid, obj, ref col, row, ((Condition)obj).getConditionType(), true);
                AddObjectToRow(grid, obj, ref col, row, null, true);
            }
            else if (obj is Reward)
            {
                AddObjectToRow(grid, obj, ref col, row, ((Reward)obj).getRewardType(), true);
                AddObjectToRow(grid, obj, ref col, row, null, true);
            }

            foreach (object o in GetObjectCellOrder(obj))
                AddObjectToRow(grid, obj, ref col, row, o, true);

            for (int c = 0; c < grid.ColumnCount; c++)
                for (int r = 0; r < grid.RowCount; r++)
                    if (grid[c, r].ValueType == typeof(object))
                    {
                        grid[c, r].ReadOnly = true;
                        grid[c, r].Style.BackColor = SystemColors.InactiveBorder;
                    }
        }

        public void AddObjectToRow(DataGridView grid, object baseObj, ref int col, int row, object Obj, bool ReadOnly)
        {
            if (col + 3 > grid.Columns.Count)
                grid.Columns.Add("", "");

            if (Obj == null) // spacer
            {
                col++;
            }
            else if (Obj is PropertyInfo)
            {
                PropertyInfo propInfo = (PropertyInfo)Obj;
                AddObjectToRow(grid, baseObj, ref col, row, propInfo.GetValue(baseObj, null), false);
            }
            else if (Obj is string)
            {
                grid[col, row] = new DataGridViewTextBoxCell();
                grid[col, row].Value = Obj;

                if (!ReadOnly)
                    grid[col, row].ValueType = Obj.GetType();
                
                col++;
            }
            else if (Obj is sbyte || Obj is short || Obj is int || Obj is long || Obj is byte || Obj is ushort || Obj is uint || Obj is ulong)
            {
                DataGridViewTextBoxCell TextBoxCell = new DataGridViewTextBoxCell();
                grid[col, row] = TextBoxCell;
                grid[col, row].ValueType = Obj.GetType();
                grid[col, row].Value = Obj;
                col++;
            }
            else if (Obj is bool)
            {
                DataGridViewCheckBoxCell CheckBoxCell = new DataGridViewCheckBoxCell();
                grid[col, row] = CheckBoxCell;
                grid[col, row].ValueType = Obj.GetType();
                grid[col, row].Value = Obj;
                col++;
            }
            else if (Obj.GetType().IsEnum)
            {
                DataGridViewComboBoxCell ComboCell = new DataGridViewComboBoxCell();
                ComboCell.DataSource = Enum.GetValues(Obj.GetType());
                ComboCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                ComboCell.DropDownWidth = 130;

                grid[col, row] = ComboCell;
                grid[col, row].ValueType = Obj.GetType();
                grid[col, row].Value = Obj;
                col++;
            }
            else if (Obj.GetType().IsArray)
            {
                Array dataArray = (Array)Obj;
                AddObjectToRow(grid, baseObj, ref col, row, dataArray.Length, false);

                foreach (object o in dataArray)
                {
                    AddObjectToRow(grid, baseObj, ref col, row, null, true);
                    AddObjectToRow(grid, baseObj, ref col, row, o, false);
                }
            }
            else if (Obj is ITEM_DATA_CONDITION || Obj is QUEST_DATA_CONDITION || Obj is ABIL_DATA_CONDITION || Obj is QUEST_DATA_REWARD || Obj is ABIL_DATA_REWARD)
            {
                ObjectToGrid(grid, ref col, row, Obj);
            }
            else
            {
                throw new FooException("AddObjectToRow, Unknown object type" + Obj.GetType());
            }
        }
#endregion

#region Grid to Object
        void RowToObject(DataGridView grid, int row, object cell, ref int col, ref object BaseObj, bool noGrid)
        {
            if (cell == null || cell is string) // hard-coded strings or spacer cells
            {
                col++;
            }
            else if (cell is PropertyInfo)
            {
                PropertyInfo propInfo = (PropertyInfo)cell;
                Type dt = propInfo.PropertyType; // PropertyType = byte[], ElementType = byte

                if (dt.IsArray)
                {
                    int dataCount = 0;

                    // If there aren't enough columns or the row type has changed, make an array size 0
                    if (!noGrid && grid.Columns.Count > col && grid.Rows.Count > row)
                        dataCount = (int)grid[col, row].Value;

                    col++; // dataCount cell

                    Array arr = Array.CreateInstance(dt.GetElementType(), dataCount);
                    for (int i = 0; i < dataCount; i++)
                    {
                        col++; // spacer cell

                        // Insert cells for each array element property
                        object arrInst = Activator.CreateInstance(dt.GetElementType());
                        foreach (object o in GetObjectCellOrder(arrInst))
                            RowToObject(grid, row, o, ref col, ref arrInst, noGrid);

                        arr.SetValue(arrInst, i);
                    }
                    propInfo.SetValue(BaseObj, arr, null);
                }
                else if (dt.IsEnum)
                {
                    // If there aren't enough columns or the row type has changed, keep default values
                    // If the value cannot be converted to enum, keep the default value
                    if (!noGrid && grid.Columns.Count > col && grid.Rows.Count > row && grid[col, row].Value != null)
                    {
                        FieldInfo fieldInfo = dt.GetField(grid[col, row].Value.ToString());
                        if (fieldInfo == null)
                            Console.WriteLine("RowToObject::Cannot convert value " + grid[col, row].Value.ToString() + " to " + dt);
                        else
                            propInfo.SetValue(BaseObj, grid[col, row].Value, null);
                    }
                    col++;
                }
                else // strings, ints, etc
                {
                    // If there aren't enough columns or the row type has changed, keep default values
                    if (!noGrid && grid.Columns.Count > col && grid.Rows.Count > row && grid[col, row].Value != null)
                        propInfo.SetValue(BaseObj, grid[col, row].Value, null);

                    col++;
                }
            }
            else
                throw new FooException("??");
        }
#endregion

    }
}
