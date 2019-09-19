using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace QuestStudio
{
    public class Reward
    {
        private byte[] originalData = null;
        public void setOriginalData(byte[] d) { originalData = d; }
        public byte[] getOriginalData()
        {
            return (originalData == null) ? (new byte[] { }) : originalData;
        }

        private RewardType reward_type;
        public void setRewardType(RewardType t) { reward_type = t; }
        public RewardType getRewardType() { return reward_type; }

        public Reward Clone()
        {
            byte[] data = StructWriter.Convert(this);
            object obj = QuestFile.CreateObject(this.getRewardType());
            StructReader.Convert(data, ref obj);
            return (Reward)obj;
        }

        public static string getNodeText(Reward r)
        {
            switch (r.getRewardType())
            {
                case RewardType.REWD000:
                    return string.Format("{0} Quest {1}", ((Rewd000)r).Operator.getText(), ((Rewd000)r).questid);
                case RewardType.REWD001:
                    return string.Format("{0} Item {1} x {2}", ((Rewd001)r).Action.getText(), ((Rewd001)r).Amount, ((Rewd001)r).ItemSN);
                case RewardType.REWD002:
                    {
                        if (((Rewd002)r).data.Length == 1)
                            return string.Format("Quest: {0}({1}) {2} {3}", ((Rewd002)r).data[0].VarType.getText(), ((Rewd002)r).data[0].VarNo, ((Rewd002)r).data[0].op.getText(), ((Rewd002)r).data[0].Value);
                        else
                            return string.Format("QuestVars: {0}", ((Rewd002)r).data.Length);
                    }
                case RewardType.REWD003:
                    {
                        if (((Rewd003)r).data.Length == 1)
                            return string.Format("Ability: {0} {1} {2}", ((Rewd003)r).data[0].type.getText(), ((Rewd003)r).data[0].op.getText(), ((Rewd003)r).data[0].value);
                        else
                            return string.Format("Abilities: {0}", ((Rewd003)r).data.Length);
                    }
                case RewardType.REWD004:
                    {
                        if (((Rewd004)r).data.Length == 1)
                            return string.Format("User: {0}({1}) {2} {3}", ((Rewd004)r).data[0].VarType.getText(), ((Rewd004)r).data[0].VarNo, ((Rewd004)r).data[0].op.getText(), ((Rewd004)r).data[0].Value);
                        else
                            return string.Format("UserVars: {0}", ((Rewd004)r).data.Length);
                    }
                case RewardType.REWD005:
                    {
                        if (((Rewd005)r).Target == ItemRewardType.Experience)
                            return string.Format("Reward Experience: {0}", ((Rewd005)r).Value);
                        else if (((Rewd005)r).Target == ItemRewardType.Money)
                            return string.Format("Reward Zulie: {0}", ((Rewd005)r).Value);
                        else 
                            return string.Format("Reward Item: {0} x {1}", ((Rewd005)r).Value, ((Rewd005)r).ItemSN);
                    }
                case RewardType.REWD006:
                    return string.Format("Set HP({0}%) and MP({1}%)", ((Rewd006)r).PercentOfHP, ((Rewd006)r).PercentOfMP);
                case RewardType.REWD007:
                    return string.Format("Warp to Zone {0} ({1},{2}) Party: {3}", ((Rewd007)r).ZoneNo, ((Rewd007)r).X, ((Rewd007)r).Y, ((Rewd007)r).PartyOpt);
                case RewardType.REWD008:
                    return string.Format("Spawn {1} of mob ID {2} at {0}", ((Rewd008)r).Where.getText(), ((Rewd008)r).HowMany, ((Rewd008)r).MonsterID);
                case RewardType.REWD009:
                    return string.Format("Set Next Trigger: {0}", ((Rewd009)r).NextTriggerSN);
                case RewardType.REWD010:
                    return string.Format("Reset Stats");
                case RewardType.REWD011:
                    return string.Format("Set {0} Var {1} {2} {3}", ((Rewd011)r).Who.getText(), ((Rewd011)r).VarNo, ((Rewd011)r).Operator.getText(), ((Rewd011)r).Value);
                case RewardType.REWD012:
                    return string.Format("NPC {0} Message {1}", ((Rewd012)r).MsgType.getText(), ((Rewd012)r).StringID);
                case RewardType.REWD013:
                    return string.Format("Set Next {0} Trigger: {1}", ((Rewd013)r).Who.getText(), ((Rewd013)r).Trigger);
                case RewardType.REWD014:
                    return string.Format("{0} Skill {1}", ((Rewd014)r).Operator.getText(), ((Rewd014)r).SkillNo);
                case RewardType.REWD015:
                    return string.Format("Set Switch {0} {1}", ((Rewd015)r).QuestSwitch, ((Rewd015)r).OnOff.getText());
                case RewardType.REWD016:
                    return string.Format("Reset SwitchGroup {0}", ((Rewd016)r).QuestSwitchGroup);
                case RewardType.REWD017:
                    return string.Format("Reset All Switches");
                case RewardType.REWD018:
                    return string.Format("NPC Announce String {0}", ((Rewd018)r).StrID);
                case RewardType.REWD019:
                    return string.Format("Zone {0} Do Trigger: {1}, Team {2}", ((Rewd019)r).ZoneNo, ((Rewd019)r).TriggerName, ((Rewd019)r).TeamNo);
                case RewardType.REWD020:
                    return string.Format("Set Team by {0}", ((Rewd020)r).TeamType.getText());
                case RewardType.REWD021:
                    return string.Format("Set Revive Position: {0}, {1}", ((Rewd021)r).X, ((Rewd021)r).Y);
                case RewardType.REWD022:
                    return string.Format("Zone {0} Regen Mode: {1}", ((Rewd022)r).ZoneNo, ((Rewd022)r).Operator.getText());
                case RewardType.REWD023:
                    return string.Format("Increase Clan Level");
                case RewardType.REWD024:
                    return string.Format("Clan Money {0} {1}", ((Rewd024)r).Operator.getText(), ((Rewd024)r).ClanMoney);
                case RewardType.REWD025:
                    return string.Format("Clan Points {0} {1}", ((Rewd025)r).Operator.getText(), ((Rewd025)r).ClanPoints);
                case RewardType.REWD026:
                    return string.Format("Clan Skill {0} {1}", ((Rewd026)r).Operator.getText(), ((Rewd026)r).ClanSkillNo);
                case RewardType.REWD027:
                    return string.Format("Clan Contribute (bugged) {0} {1}", ((Rewd027)r).Operator.getText(), ((Rewd027)r).ClanContribute);
                case RewardType.REWD028:
                    return string.Format("Warp ClanMembers in Range to {0} {1}, {2}", ((Rewd028)r).ZoneNo, ((Rewd028)r).X, ((Rewd028)r).Y);
                case RewardType.REWD029:
                    return string.Format("Run Lua Function {0}", ((Rewd029)r).ScriptName);
                case RewardType.REWD030:
                    return string.Format("Reset Skills");
                case RewardType.REWD031:
                    return string.Format("Set QuestVar {0} (???)", ((Rewd031)r).Var.VarNo);
                case RewardType.REWD032:
                    return string.Format("Reward Item {0}", ((Rewd032)r).ItemSN);
                case RewardType.REWD033:
                    return string.Format("/");
                case RewardType.REWD034:
                    return string.Format("NPC Visibility: {0}", ((Rewd034)r).HideShowToggle);

                /* Custom Rewards */
                case RewardType.REWD050:
                    return string.Format("Reward from Break: {0}", ((Rewd050)r).BreakID);
                case RewardType.REWD051:
                    return string.Format("Write Message: {0}> {1}", ((Rewd051)r).MsgType.getText(), ((Rewd051)r).Message);
                case RewardType.REWD052:
                    return string.Format("Set Quest Switch at NPC EventValue index: {0}", ((Rewd052)r).On.getText());
                default:
                    return "Unknown Reward " + (int)r.getRewardType();
            }
        }
    };

    class Rewd000 : Reward
    {
        public int questid { get; set; }
        public QuestActionOperator Operator { get; set; }
    };

    class Rewd001 : Reward
    {
        public Rewd001() { ItemSN = 8001; Amount = 1;  }
        public uint ItemSN { get; set; }
        public ItemRewardOperator Action { get; set; }
        public short Amount { get; set; }
        [Browsable(false)] public byte ToPartyBugged { get; set; }
    };

    class Rewd002 : Reward
    {
        public QUEST_DATA_REWARD[] data { get; set; }
    };

    class Rewd003 : Reward
    {
        public ABIL_DATA_REWARD[] data { get; set; }
        //[Browsable(false)] public byte ToPartyBugged { get; set; }
    };

    class Rewd004 : Reward
    {
        public QUEST_DATA_REWARD[] data { get; set; }
    };


    // Equationn:   iR = ( (S_REWARD+30) * ( pUSER->GetCur_CHARM()+10 ) * ( ::Get_WorldREWARD() ) * ( pUSER->GetCur_FAME()+20 ) / ( pUSER->GetCur_LEVEL()+70 ) / 30000 ) + S_REWARD;
    // Equation1:   iR = S_REWARD * ( pUSER->GetCur_LEVEL()+3 ) * ( pUSER->GetCur_LEVEL()+pUSER->GetCur_CHARM()/2+40 ) * ( ::Get_WorldREWARD() ) / 10000;
    // Equation2:   iR = S_REWARD * nDupCNT;
    // Equation3,5: iR = ( (S_REWARD+20) * ( pUSER->GetCur_CHARM()+10 ) * ( ::Get_WorldREWARD() ) * ( pUSER->GetCur_FAME()+20 ) / ( pUSER->GetCur_LEVEL()+70 ) / 30000 ) + S_REWARD;
    // Equation4:   iR = (S_REWARD+2) * ( pUSER->GetCur_LEVEL()+pUSER->GetCur_CHARM()+40 ) * ( pUSER->GetCur_FAME()+40 ) * ( ::Get_WorldREWARD() ) / 140000;
    // Equation6:   iR = ( (S_REWARD+20) * ( pUSER->GetCur_LEVEL()+pUSER->GetCur_CHARM() ) * ( pUSER->GetCur_FAME()+20 ) * ( ::Get_WorldREWARD() ) / 3000000 ) + S_REWARD;
    // Equation15:  iR = S_REWARD;
    class Rewd005 : Reward
    {
        public Rewd005() { Value = 1; ItemSN = 1000; }
        public ItemRewardType Target { get; set; }
        public ItemRewardEquation Equation { get; set; }
        public int Value { get; set; }
        public int ItemSN { get; set; }
        public byte PartyOpt { get; set; }
        public short ItemOpt { get; set; }
    };

    class Rewd006 : Reward
    {
        public Rewd006() { PercentOfHP = 100; PercentOfMP = 100; }
        public int PercentOfHP { get; set; }
        public int PercentOfMP { get; set; }
        [Browsable(false)] public byte PartyOpt_UNUSED { get; set; }
    };

    class Rewd007 : Reward
    {
        public Rewd007() { ZoneNo = 1; X = 500000; Y = 500000; }
        public int ZoneNo { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public PlayerOrParty PartyOpt { get; set; }
    };

    class Rewd008 : Reward
    {
        public Rewd008() { ZoneNo = 1; X = 500000; Y = 500000; Range = 100; TeamNo = 100; MonsterID = 1; HowMany = 1; }
        public int MonsterID { get; set; }
        public int HowMany { get; set; }
        public Who08 Where { get; set; }
        public int ZoneNo { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Range { get; set; }
        public int TeamNo { get; set; }
    };

    class Rewd009 : Reward
    {
        public Rewd009() { NextTriggerSN = "trig-ger-001"; }
        public string NextTriggerSN { get; set; }
    };

    class Rewd010 : Reward
    {
    };

    class Rewd011 : Reward
    {
        public Rewd011() { Operator = Operator_Act.OP_SET; }
        public NpcOrEvent Who { get; set; }
        public short VarNo { get; set; }
        public int Value { get; set; }
        public Operator_Act Operator { get; set; }
    };

    class Rewd012 : Reward
    {
        public MsgType MsgType { get; set; }
        public int StringID { get; set; }
    };

    class Rewd013 : Reward
    {
        public Rewd013() { Trigger = "trig-ger-001"; }
        public NpcOrEvent Who { get; set; }
        public int Sec { get; set; }
        public string Trigger { get; set; }
    };

    class Rewd014 : Reward
    {
        public AddRemoveOperator Operator { get; set; }
        public int SkillNo { get; set; }
    };

    class Rewd015 : Reward
    {
        public short QuestSwitch { get; set; }
        public OnOffOperator OnOff { get; set; }
    };

    class Rewd016 : Reward
    {
        public short QuestSwitchGroup { get; set; }
    };

    class Rewd017 : Reward
    {
    };

    class Rewd018 : Reward
    {
        public int StrID { get; set; }
    };

    /*
     * short			nTriggerLength;
     * char			    TriggerName[ 1 ];	// char Trigger[ nTriggerLength ], NULL Æ÷ÇÔ
     * t_HASHKEY		m_HashTrigger;		/// ·Îµù½Ã Æ®¸®°Å¸íÀ» Çì½¬°ªÀ¸·Î º¯°æ ½ÃÅ´...
     */
    class Rewd019 : Reward
    {
        public Rewd019() { ZoneNo = 1; TeamNo = 11; TriggerName = "trig-ger-001"; }
        public short ZoneNo { get; set; }
        public short TeamNo { get; set; }
        public string TriggerName { get; set; }
    };

    class Rewd020 : Reward
    {
        public TeamNoType TeamType { get; set; }
    };

    class Rewd021 : Reward
    {
        public Rewd021() { X = 500000; Y = 500000; }
        public int X { get; set; }
        public int Y { get; set; }
    };

    class Rewd022 : Reward
    {
        public short ZoneNo { get; set; }
        public OffOnToggle Operator { get; set; }
    };

    class Rewd023 : Reward
    {
    };

    class Rewd024 : Reward
    {
        public Rewd024() { Operator = AddSubOperator.OP_ADD; }

        public int ClanMoney { get; set; }
        public AddSubOperator Operator { get; set; }
    };

    class Rewd025 : Reward
    {
        public Rewd025() { Operator = AddSubOperator.OP_ADD; }

        public short ClanPoints { get; set; }
        public AddSubOperator Operator { get; set; }
    };

    class Rewd026 : Reward
    {
        public short ClanSkillNo { get; set; }
        public AddRemoveOperator Operator { get; set; }
    };
    
    // Never used and bugged, adds to Clan Points instead of Clan Contribution
    class Rewd027 : Reward
    {
        public Rewd027() { Operator = AddSubOperator.OP_ADD; }
        public short ClanContribute { get; set; }
        public AddSubOperator Operator { get; set; }
    };

    class Rewd028 : Reward
    {
        public Rewd028() { X = 500000; Y = 500000; }
        public int Range { get; set; }
        public short ZoneNo { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    };

    class Rewd029 : Reward
    {
        public Rewd029() { ScriptName = "scriptFunc"; }
        public string ScriptName { get; set; }
    };

    class Rewd030 : Reward
    {
    };

    // Never used
    class Rewd031 : Reward
    {
        [Browsable(false)] public int MonsterSN_Unused { get; set; }
        [Browsable(false)] public int CompareValue_Unused { get; set; }
        public QUEST_DATA_REWARD Var { get; set; }
    };

    // Never used
    class Rewd032 : Reward
    {
        public uint ItemSN { get; set; } 
        [Browsable(false)] public int CompareValue_Unused { get; set; } 
        [Browsable(false)] public byte PartyOpt_Bugged { get; set; }
    };

    // Never used
    class Rewd033 : Reward
    {
        [Browsable(false)] public short NextRewardSplitter { get; set; }
    };

    class Rewd034 : Reward
    {
        public NpcVisibility HideShowToggle { get; set; }
    };

    /* Custom Rewards */
    class Rewd050 : Reward
    {
        public int BreakID { get; set; }
    };

    class Rewd051 : Reward
    {
        public Rewd051() { Message = "Your message."; }
        public ChatType MsgType { get; set; }
        public string Message { get; set; }
    };

    class Rewd052 : Reward
    {
        public OnOffOperator On { get; set; }
    };
}
