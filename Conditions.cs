using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace QuestStudio
{
    public class Condition
    {
        private byte[] originalData = null;
        public void setOriginalData(byte[] d) { originalData = d; }
        public byte[] getOriginalData() 
        {
            return (originalData == null) ? (new byte[] { }) : originalData;
        }

        private ConditionType condition_type;
        public void setConditionType(ConditionType t) { condition_type = t; }
        public ConditionType getConditionType() { return condition_type; }

        public Condition Clone()
        {
            byte[] data = StructWriter.Convert( this );
            object obj = QuestFile.CreateObject( this.getConditionType() );
            StructReader.Convert(data, ref obj);
            return (Condition)obj;
        }

        public static string getNodeText(Condition c)
        {
            switch (c.getConditionType())
            {
                case ConditionType.COND000:
                    return string.Format("Select Quest: {0}", ((Cond000)c).questid);
                case ConditionType.COND001:
                    {
                        if (((Cond001)c).data.Length == 1)
                            return string.Format("Quest: {0}({1}) {2} {3}", ((Cond001)c).data[0].VarType.getText(), ((Cond001)c).data[0].VarNo, ((Cond001)c).data[0].op.getText(), ((Cond001)c).data[0].Value);
                        else
                            return string.Format("Quest Variable Checks: {0}", ((Cond001)c).data.Length);
                    }
                case ConditionType.COND002:
                    {
                        if (((Cond002)c).data.Length == 1)
                            return string.Format("User: {0}({1}) {2} {3}", ((Cond002)c).data[0].VarType.getText(), ((Cond002)c).data[0].VarNo, ((Cond002)c).data[0].op.getText(), ((Cond002)c).data[0].Value);
                        else
                            return string.Format("User Variable Checks: {0}", ((Cond002)c).data.Length);
                    }
                case ConditionType.COND003:
                    {
                        if (((Cond003)c).data.Length == 1)
                            return string.Format("Ability: {0} {1} {2}", ((Cond003)c).data[0].type.getText(), ((Cond003)c).data[0].op.getText(), ((Cond003)c).data[0].value);
                        else
                            return string.Format("Ability Checks: {0}", ((Cond003)c).data.Length);
                    }
                case ConditionType.COND004:
                    {
                        if (((Cond004)c).data.Length == 1)
                            return string.Format("Item: {0} {1} {2} ({3})", ((Cond004)c).data[0].itemsn, ((Cond004)c).data[0].op.getText(), ((Cond004)c).data[0].cnt, ((Cond004)c).data[0].equipslot.getText());
                        else
                            return string.Format("Item Checks: {0}", ((Cond004)c).data.Length);
                    }
                case ConditionType.COND005:
                    return string.Format("Party Level >= {0}", ((Cond005)c).MinimumLevel);
                case ConditionType.COND006:
                    return string.Format("Position: Zone {0} ({1},{2})", ((Cond006)c).ZoneNo, ((Cond006)c).X, ((Cond006)c).Y);
                case ConditionType.COND007:
                    return string.Format("WorldTime In [{0} ~ {1}]", ((Cond007)c).Start, ((Cond007)c).End);
                case ConditionType.COND008:
                    return string.Format("Remaing QuestTime {0} {1}", ((Cond008)c).Operator.getText(), ((Cond008)c).Time);
                case ConditionType.COND009:
                    return string.Format("Has Skill In [{0} ~ {1}] : {2}", ((Cond009)c).SkillNo1, ((Cond009)c).SkillNo2, ((Cond009)c).Operator);
                case ConditionType.COND010:
                    return string.Format("Random Chance [{0} - {1}]", ((Cond010)c).MinimumPercent, ((Cond010)c).MaximumPercent);
                case ConditionType.COND011:
                    return string.Format("{0} Var {1} {2} {3}", ((Cond011)c).Who.getText(), ((Cond011)c).VarNo, ((Cond011)c).Operator.getText(), ((Cond011)c).Value);
                case ConditionType.COND012:
                    return string.Format("Select EventObj @ {0} {1},{2}", ((Cond012)c).Zone, ((Cond012)c).X, ((Cond012)c).Y);
                case ConditionType.COND013:
                    return string.Format("Select NPC {0}", ((Cond013)c).NpcNo);
                case ConditionType.COND014:
                    return string.Format("Switch {0} is {1}", ((Cond014)c).SwitchNo, ((Cond014)c).Operator.getText());
                case ConditionType.COND015:
                    return string.Format("Party MemberCount [{0} ~ {1}]", ((Cond015)c).Number1, ((Cond015)c).Number2);
                case ConditionType.COND016:
                    return string.Format("ZoneTime (Who: {0}) [{1} ~ {2}]", ((Cond016)c).Who.getText(), ((Cond016)c).TimeStart, ((Cond016)c).TimeEnd);
                case ConditionType.COND017:
                    return string.Format("NPC({0}) Var {1} {2} NPC({3}) Var {4}", ((Cond017)c).npcno1, ((Cond017)c).varno1, ((Cond017)c).Operator.getText(), ((Cond017)c).npcno2, ((Cond017)c).varno2);
                    //return string.Format("NPC({0}) Var {1} {2} NPC({3}) Var {4}", ((Cond017)c).npc1.npcno, ((Cond017)c).npc1.varno, ((Cond017)c).Operator.getText(), ((Cond017)c).npc2.npcno, ((Cond017)c).npc2.varno);
                case ConditionType.COND018:
                    return string.Format("MonthDay {0} Time [{1}:{2} ~ {3}:{4}]", ((Cond018)c).DayOfMonth, ((Cond018)c).HourLow, ((Cond018)c).MinuteLow, ((Cond018)c).HourHigh, ((Cond018)c).MinuteHigh);
                case ConditionType.COND019:
                    return string.Format("WeekDay {0} Time [{1}:{2} ~ {3}:{4}]", ((Cond019)c).DayOfWeek.getText(), ((Cond019)c).HourLow, ((Cond019)c).MinuteLow, ((Cond019)c).HourHigh, ((Cond019)c).MinuteHigh);
                case ConditionType.COND020:
                    return string.Format("Team [{0} ~ {1}]", ((Cond020)c).Number1, ((Cond020)c).Number2);
                case ConditionType.COND021:
                    return string.Format("Distance From {0} <= {1}", ((Cond021)c).ObjectType, ((Cond021)c).Radius);
                case ConditionType.COND022:
                    return string.Format("ChannelNo [{0} ~ {1}]", ((Cond022)c).Min, ((Cond022)c).Max);
                case ConditionType.COND023:
                    return string.Format("In Clan? {0}", ((Cond023)c).Operator.getText());
                case ConditionType.COND024:
                    return string.Format("Clan Position {0} {1}", ((Cond024)c).Operator.getText(), ((Cond024)c).ClanPos);
                case ConditionType.COND025:
                    return string.Format("Clan Contribution {0} {1}", ((Cond025)c).Operator.getText(), ((Cond025)c).ClanContribute);
                case ConditionType.COND026:
                    return string.Format("Clan Grade {0} {1}", ((Cond026)c).Operator.getText(), ((Cond026)c).ClanGrade);
                case ConditionType.COND027:
                    return string.Format("Clan Points {0} {1}", ((Cond027)c).Operator.getText(), ((Cond027)c).ClanPoint);
                case ConditionType.COND028:
                    return string.Format("Clan Money {0} {1}", ((Cond028)c).Operator.getText(), ((Cond028)c).ClanMoney);
                case ConditionType.COND029:
                    return string.Format("Clan MemberCnt {0} {1}", ((Cond029)c).Operator.getText(), ((Cond029)c).MemberCount);
                case ConditionType.COND030:
                    return string.Format("Clan Skill [{0} ~ {1}] : {2}", ((Cond030)c).Skill1, ((Cond030)c).Skill2, ((Cond030)c).Operator.getText());

                /* Custom Conditions */
                case ConditionType.COND050:
                    return string.Format("Does Not Have Quest [{0} ~ {1}]", ((Cond050)c).Quest1, ((Cond050)c).Quest2);
                case ConditionType.COND051:
                    return string.Format("In Arena Group? {0}", ((Cond051)c).Has.getText());
                case ConditionType.COND052:
                    return string.Format("Arena Status {0} NPC EventValue", ((Cond052)c).Operator.getText());
                case ConditionType.COND053:
                    return string.Format("Check Quest Switch at NPC EventValue index: {0}", ((Cond053)c).On.getText());
                case ConditionType.COND054:
                    return string.Format("Arena Game with {0} {1}", ((Cond054)c).IdOrType.getText(), ((Cond054)c).Game );
                default:
                    return "Unknown Condition " + (int)c.getConditionType();
            }
        }
    };

    class Cond000 : Condition
    {
        public int questid { get; set; }
    };

    class Cond001 : Condition
    {
        public QUEST_DATA_CONDITION[] data { get; set; }
    };

    class Cond002 : Condition
    {
        public QUEST_DATA_CONDITION[] data { get; set; }
    };

    class Cond003 : Condition
    {
        public ABIL_DATA_CONDITION[] data { get; set; }
    };

    class Cond004 : Condition
    {
        public ITEM_DATA_CONDITION[] data { get; set; }
    };

    class Cond005 : Condition
    {
        public Cond005() { IsLeader = true; MinimumLevel = 1; }
        public bool IsLeader { get; set; }
        public int MinimumLevel { get; set; }
        [Browsable(false)] public int UNUSED { get; set; }
    };

    class Cond006 : Condition
    {
        public Cond006() { ZoneNo = 1; X = 500000; Y = 500000; Radius = 100; }
        public int ZoneNo { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        [Browsable(false)] public int ZUNUSED { get; set; }
        public int Radius { get; set; }
    };

    class Cond007 : Condition
    {
        public uint Start { get; set; }
        public uint End { get; set; }
    };

    class Cond008 : Condition
    {
        public uint Time { get; set; }
        public Operator_Cond Operator { get; set; }
    };

    class Cond009 : Condition
    {
        public Cond009() { Operator = HasOperator.Has; }
        public int SkillNo1 { get; set; }
        public int SkillNo2 { get; set; }
        public HasOperator Operator { get; set; }
    };

    class Cond010 : Condition
    {
        public Cond010() { MaximumPercent = 100; }
        public byte MinimumPercent { get; set; }
        public byte MaximumPercent { get; set; }
    };

    class Cond011 : Condition
    {
        public NpcOrEvent Who { get; set; }
        public short VarNo { get; set; }
        public int Value { get; set; }
        public Operator_Cond Operator { get; set; }
    };

    class Cond012 : Condition
    {
        public Cond012() { Zone = 1; X = 500000; Y = 500000; }
        public short Zone { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int EventID { get; set; }
    };

    class Cond013 : Condition
    {
        public int NpcNo { get; set; }
    };

    class Cond014 : Condition
    {
        public short SwitchNo { get; set; }
        public OnOffOperator Operator { get; set; }
    };

    class Cond015 : Condition
    {
        public short Number1 { get; set; }
        public short Number2 { get; set; }
    };

    class Cond016 : Condition
    {
        public Who016 Who { get; set; }
        public uint TimeStart { get; set; }
        public uint TimeEnd { get; set; }
    };

    [TypeConverter(typeof(QuestDataConverter))]
    struct NPCVAR {
        public int npcno { get; set; }                                                 //4     +4
        public short varno { get; set; }                                               //8     +2
    };

    /// <summary>
    ///  npcno and varno is wrapped in a struct in source, it changes padding. manual fix :(
    ///  
    /// structure is padded so the total length is a multiple of the largest member size
    /// int 
    /// short
    /// ^ total struct = 8
    /// </summary>
    class Cond017 : Condition
    {
        //public NPCVAR npc1 { get; set; }
        //public NPCVAR npc2 { get; set; }

        public int npcno1 { get; set; }                                                 //4     +4
        public short varno1 { get; set; }                                               //8     +2
        [Browsable(false)]
        public short padfix1 { get; set; }                                              //10    +2

        public int npcno2 { get; set; }                                                 //12    +4
        public short varno2 { get; set; }                                               //16    +2
        [Browsable(false)]
        public short padfix2 { get; set; }                                              //18    +2

        public Operator_Cond Operator { get; set; }                                     //20    +1
    };

    class Cond018 : Condition
    {
        public byte DayOfMonth { get; set; }
        public byte HourLow { get; set; }
        public byte MinuteLow { get; set; }
        public byte HourHigh { get; set; }
        public byte MinuteHigh { get; set; }
    };

    class Cond019 : Condition
    {
        public DayOfWeek DayOfWeek { get; set; }
        public byte HourLow { get; set; }
        public byte MinuteLow { get; set; }
        public byte HourHigh { get; set; }
        public byte MinuteHigh { get; set; }
    };

    class Cond020 : Condition
    {
        public int Number1 { get; set; }
        public int Number2 { get; set; }
    };

    class Cond021 : Condition
    {
        public Cond021() { Radius = 100; }
        public NpcOrEvent ObjectType { get; set; }
        public int Radius { get; set; }
    };

    class Cond022 : Condition
    {
        public ushort Min { get; set; }
        public ushort Max { get; set; }
    };

    class Cond023 : Condition
    {
        public Cond023() { Operator = HasOperator.Has; }
        public HasOperator Operator { get; set; }
    };

    class Cond024 : Condition
    {
        public Cond024() { ClanPos = ClanPos.Master; }
        public ClanPos ClanPos { get; set; }
        public Operator_Cond Operator { get; set; }
    };

    class Cond025 : Condition
    {
        public short ClanContribute { get; set; }
        public Operator_Cond Operator { get; set; }
    };

    class Cond026 : Condition
    {
        public short ClanGrade { get; set; }
        public Operator_Cond Operator { get; set; }
    };

    class Cond027 : Condition
    {
        public short ClanPoint { get; set; }
        public Operator_Cond Operator { get; set; }
    };

    class Cond028 : Condition
    {
        public int ClanMoney { get; set; }
        public Operator_Cond Operator { get; set; }
    };

    class Cond029 : Condition
    {
        public short MemberCount { get; set; }
        public Operator_Cond Operator { get; set; }
    };

    class Cond030 : Condition
    {
        public Cond030() { Operator = HasOperator.Has; }
        public short Skill1 { get; set; }
        public short Skill2 { get; set; }
        public HasOperator Operator { get; set; }
    };


    /* Custom Conditions */
    class Cond050 : Condition
    {
        public Cond050() { }
        public short Quest1 { get; set; }
        public short Quest2 { get; set; }
    };

    class Cond051 : Condition // Has Group
    {
        public HasOperator Has { get; set; }
    };

    class Cond052 : Condition // Compare Group Status with NPC Event Value
    {
        public Operator_Cond Operator { get; set; }
    };

    class Cond053 : Condition // Check switch in selected quest by npc eventvalue
    {
        public OnOffOperator On { get; set; }
    };

    class Cond054 : Condition // Arena Game Check
    {
        public int Game { get; set; }
        public ArenaGameOperator IdOrType { get; set; }
    };
}
