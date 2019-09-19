using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Drawing.Design;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.ComponentModel.Design;
using System.Resources;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;

namespace QuestStudio
{
    public class QuestBlock
    {
        public string Name { get; set; }
        public List<QuestTrigger> Triggers = new List<QuestTrigger>();

        // Clone is only to be used when the Block has been generated from a tree. Otherwise edits will be missing
        public QuestBlock Clone()
        {
            QuestBlock block = new QuestBlock();
            block.Name = this.Name;

            foreach (QuestTrigger t in this.Triggers)
                block.Triggers.Add(t.Clone());

            return block;
        }

        public String GetNodeText(int iIndex)
        {
           // return "«" + iIndex + "» " + Name;
            return "‹" + iIndex + "› " + Name;
        }
    };

    public class QuestTrigger
    {
        public string Name { get; set; }
        public bool CheckNext { get; set; }
        public List<Condition> Conditions = new List<Condition>();
        public List<Reward> Rewards = new List<Reward>();

        public String GetNodeText()
        {
            return CheckNext ? ("☑ " + Name + " ↷") : "☐ " + Name;
        }

        // Clone is only to be used when the Trigger has been generated from a tree. Otherwise edits will be missing
        public QuestTrigger Clone() 
        {
            QuestTrigger trigger = new QuestTrigger();
            trigger.Name = this.Name;
            trigger.CheckNext = this.CheckNext;

            foreach (Condition c in this.Conditions)
                trigger.Conditions.Add(c.Clone());

            foreach (Reward r in this.Rewards)
                trigger.Rewards.Add(r.Clone());

            return trigger;
        }
    };

    public class QuestFile
    {
        public string Title { get; set; }
        public int Version { get; set; }
        public List<QuestBlock> Blocks = new List<QuestBlock>();
        
        public string FileName { get; set; }
        public bool Dirty { get; set; }

        public QuestFile()
        {
        }

        public QuestFile(string Filename)
        {
            Load(Filename);
        }

        public void Load(string Filename)
        {
            this.FileName = Filename;

            FileHandler fh = new FileHandler(Filename, FileHandler.FileOpenMode.Reading, Encoding.GetEncoding("EUC-KR"));

            Version = fh.Read<int>();
            int blockCount = fh.Read<int>();
            Title = fh.Read<string>(fh.Read<short>()).Trim('\0');

            for (int i = 0; i < blockCount; i++)
            {
                QuestBlock block = new QuestBlock();

                int triggersCount = fh.Read<int>();
                block.Name = fh.Read<string>(fh.Read<short>()).Trim('\0');

                for (int j = 0; j < triggersCount; j++)
                {
                    QuestTrigger trigger = new QuestTrigger();
                    trigger.CheckNext = (fh.Read<byte>() != 0);
                    int ConditionsCount = fh.Read<int>();
                    int RewardsCount = fh.Read<int>();
                    trigger.Name = fh.Read<string>(fh.Read<short>()).Trim('\0');

                    for (int c = 0; c < ConditionsCount; c++)
                    {
                        int size = fh.Read<int>();
                        int type = fh.Read<int>();
                        byte[] data = fh.Read<byte[]>(size - 8);

                        try
                        {
                            object obj = CreateObject((ConditionType)type);
                            StructReader.Convert(data, ref obj);

                            Condition condition = (Condition)obj;
                            condition.setOriginalData(data);
                            trigger.Conditions.Add(condition);
                        }
                        catch
                        {
                            object obj = CreateObject(ConditionType.COND000);
                            Condition condition = (Condition)obj;
                            condition.setOriginalData(data);
                            trigger.Conditions.Add(condition);
                        }
                    }

                    for (int c = 0; c < RewardsCount; c++)
                    {
                        int size = fh.Read<int>();
                        int type = fh.Read<int>() & 0x0ffff;
                        byte[] data = fh.Read<byte[]>(size - 8);

                        try
                        {
                            object obj = CreateObject((RewardType)type);
                            StructReader.Convert(data, ref obj);
                            Reward reward = (Reward)obj;
                            reward.setOriginalData(data);
                            trigger.Rewards.Add(reward);
                        }
                        catch
                        {
                            object obj = CreateObject(RewardType.REWD000);
                            Reward reward = (Reward)obj;
                            reward.setOriginalData(data);
                            trigger.Rewards.Add(reward);
                        }
                    }
                    block.Triggers.Add(trigger);
                }
                this.Blocks.Add(block);
            }
            fh.Close();
        }

        public bool Save(string Filename)
        {
            try
            {
                FileHandler fh = new FileHandler(Filename, FileHandler.FileOpenMode.Writing, Encoding.GetEncoding("EUC-KR"));

                fh.Write<int>(Version);
                fh.Write<int>(Blocks.Count);
                fh.Write<short>((short)Encoding.GetEncoding("EUC-KR").GetBytes(Title.Trim('\0') + '\0').Length);
                fh.Write<string>(Title.Trim('\0') + '\0');

                foreach (QuestBlock block in Blocks)
                {
                    fh.Write<int>(block.Triggers.Count);
                    fh.Write<short>((short)Encoding.GetEncoding("EUC-KR").GetBytes(block.Name.Trim('\0') + '\0').Length);
                    fh.Write<string>(block.Name.Trim('\0') + '\0');

                    foreach (QuestTrigger trigger in block.Triggers)
                    {
                        fh.Write<byte>(trigger.CheckNext ? (byte)1 : (byte)0);
                        fh.Write<int>(trigger.Conditions.Count);
                        fh.Write<int>(trigger.Rewards.Count);
                        fh.Write<short>((short)Encoding.GetEncoding("EUC-KR").GetBytes(trigger.Name.Trim('\0') + '\0').Length);
                        fh.Write<string>(trigger.Name.Trim('\0') + '\0');

                        foreach (Condition c in trigger.Conditions)
                        {
                            byte[] bytes = StructWriter.Convert(c);
                            fh.Write<int>(bytes.Length + 8);
                            fh.Write<int>((int)c.getConditionType());
                            fh.Write<byte[]>(bytes);
                        }

                        foreach (Reward r in trigger.Rewards)
                        {
                            byte[] bytes = StructWriter.Convert(r);
                            fh.Write<int>(bytes.Length + 8);
                            fh.Write<int>((int)r.getRewardType() | 0x01000000);
                            fh.Write<byte[]>(bytes);
                        }
                    }
                }
                fh.Close();

                this.FileName = Filename;
            }
            catch (Exception)
            {
                MessageBox.Show("Error:\nCannot open " + FileName + " for writing.");
                return false;
            }
            return true;
        }

        public static Condition CreateObject(ConditionType t)
        {
            switch (t)
            {
                case ConditionType.COND000: { Condition c = new Cond000(); c.setConditionType(t); return c; }
                case ConditionType.COND001: { Condition c = new Cond001(); c.setConditionType(t); return c; }
                case ConditionType.COND002: { Condition c = new Cond002(); c.setConditionType(t); return c; }
                case ConditionType.COND003: { Condition c = new Cond003(); c.setConditionType(t); return c; }
                case ConditionType.COND004: { Condition c = new Cond004(); c.setConditionType(t); return c; }
                case ConditionType.COND005: { Condition c = new Cond005(); c.setConditionType(t); return c; }
                case ConditionType.COND006: { Condition c = new Cond006(); c.setConditionType(t); return c; }
                case ConditionType.COND007: { Condition c = new Cond007(); c.setConditionType(t); return c; }
                case ConditionType.COND008: { Condition c = new Cond008(); c.setConditionType(t); return c; }
                case ConditionType.COND009: { Condition c = new Cond009(); c.setConditionType(t); return c; }
                case ConditionType.COND010: { Condition c = new Cond010(); c.setConditionType(t); return c; }
                case ConditionType.COND011: { Condition c = new Cond011(); c.setConditionType(t); return c; }
                case ConditionType.COND012: { Condition c = new Cond012(); c.setConditionType(t); return c; }
                case ConditionType.COND013: { Condition c = new Cond013(); c.setConditionType(t); return c; }
                case ConditionType.COND014: { Condition c = new Cond014(); c.setConditionType(t); return c; }
                case ConditionType.COND015: { Condition c = new Cond015(); c.setConditionType(t); return c; }
                case ConditionType.COND016: { Condition c = new Cond016(); c.setConditionType(t); return c; }
                case ConditionType.COND017: { Condition c = new Cond017(); c.setConditionType(t); return c; }
                case ConditionType.COND018: { Condition c = new Cond018(); c.setConditionType(t); return c; }
                case ConditionType.COND019: { Condition c = new Cond019(); c.setConditionType(t); return c; }
                case ConditionType.COND020: { Condition c = new Cond020(); c.setConditionType(t); return c; }
                case ConditionType.COND021: { Condition c = new Cond021(); c.setConditionType(t); return c; }
                case ConditionType.COND022: { Condition c = new Cond022(); c.setConditionType(t); return c; }
                case ConditionType.COND023: { Condition c = new Cond023(); c.setConditionType(t); return c; }
                case ConditionType.COND024: { Condition c = new Cond024(); c.setConditionType(t); return c; }
                case ConditionType.COND025: { Condition c = new Cond025(); c.setConditionType(t); return c; }
                case ConditionType.COND026: { Condition c = new Cond026(); c.setConditionType(t); return c; }
                case ConditionType.COND027: { Condition c = new Cond027(); c.setConditionType(t); return c; }
                case ConditionType.COND028: { Condition c = new Cond028(); c.setConditionType(t); return c; }
                case ConditionType.COND029: { Condition c = new Cond029(); c.setConditionType(t); return c; }
                case ConditionType.COND030: { Condition c = new Cond030(); c.setConditionType(t); return c; }

                /* Custom Conditions */
                case ConditionType.COND050: { Condition c = new Cond050(); c.setConditionType(t); return c; }
                case ConditionType.COND051: { Condition c = new Cond051(); c.setConditionType(t); return c; }
                case ConditionType.COND052: { Condition c = new Cond052(); c.setConditionType(t); return c; }
                case ConditionType.COND053: { Condition c = new Cond053(); c.setConditionType(t); return c; }
                case ConditionType.COND054: { Condition c = new Cond054(); c.setConditionType(t); return c; }
                default:
                    throw new FooException("GetObject::Unknown ConditionType(" + ((int)t).ToString() + ")");
            }
        }

        public static Reward CreateObject(RewardType t)
        {
            switch (t)
            {
                case RewardType.REWD000: { Reward r = new Rewd000(); r.setRewardType(t); return r; }
                case RewardType.REWD001: { Reward r = new Rewd001(); r.setRewardType(t); return r; }
                case RewardType.REWD002: { Reward r = new Rewd002(); r.setRewardType(t); return r; }
                case RewardType.REWD003: { Reward r = new Rewd003(); r.setRewardType(t); return r; }
                case RewardType.REWD004: { Reward r = new Rewd004(); r.setRewardType(t); return r; }
                case RewardType.REWD005: { Reward r = new Rewd005(); r.setRewardType(t); return r; }
                case RewardType.REWD006: { Reward r = new Rewd006(); r.setRewardType(t); return r; }
                case RewardType.REWD007: { Reward r = new Rewd007(); r.setRewardType(t); return r; }
                case RewardType.REWD008: { Reward r = new Rewd008(); r.setRewardType(t); return r; }
                case RewardType.REWD009: { Reward r = new Rewd009(); r.setRewardType(t); return r; }
                case RewardType.REWD010: { Reward r = new Rewd010(); r.setRewardType(t); return r; }
                case RewardType.REWD011: { Reward r = new Rewd011(); r.setRewardType(t); return r; }
                case RewardType.REWD012: { Reward r = new Rewd012(); r.setRewardType(t); return r; }
                case RewardType.REWD013: { Reward r = new Rewd013(); r.setRewardType(t); return r; }
                case RewardType.REWD014: { Reward r = new Rewd014(); r.setRewardType(t); return r; }
                case RewardType.REWD015: { Reward r = new Rewd015(); r.setRewardType(t); return r; }
                case RewardType.REWD016: { Reward r = new Rewd016(); r.setRewardType(t); return r; }
                case RewardType.REWD017: { Reward r = new Rewd017(); r.setRewardType(t); return r; }
                case RewardType.REWD018: { Reward r = new Rewd018(); r.setRewardType(t); return r; }
                case RewardType.REWD019: { Reward r = new Rewd019(); r.setRewardType(t); return r; }
                case RewardType.REWD020: { Reward r = new Rewd020(); r.setRewardType(t); return r; }
                case RewardType.REWD021: { Reward r = new Rewd021(); r.setRewardType(t); return r; }
                case RewardType.REWD022: { Reward r = new Rewd022(); r.setRewardType(t); return r; }
                case RewardType.REWD023: { Reward r = new Rewd023(); r.setRewardType(t); return r; }
                case RewardType.REWD024: { Reward r = new Rewd024(); r.setRewardType(t); return r; }
                case RewardType.REWD025: { Reward r = new Rewd025(); r.setRewardType(t); return r; }
                case RewardType.REWD026: { Reward r = new Rewd026(); r.setRewardType(t); return r; }
                case RewardType.REWD027: { Reward r = new Rewd027(); r.setRewardType(t); return r; }
                case RewardType.REWD028: { Reward r = new Rewd028(); r.setRewardType(t); return r; }
                case RewardType.REWD029: { Reward r = new Rewd029(); r.setRewardType(t); return r; }
                case RewardType.REWD030: { Reward r = new Rewd030(); r.setRewardType(t); return r; }
                case RewardType.REWD031: { Reward r = new Rewd031(); r.setRewardType(t); return r; }
                case RewardType.REWD032: { Reward r = new Rewd032(); r.setRewardType(t); return r; }
                case RewardType.REWD033: { Reward r = new Rewd033(); r.setRewardType(t); return r; }
                case RewardType.REWD034: { Reward r = new Rewd034(); r.setRewardType(t); return r; }

                /* Custom Rewards */
                case RewardType.REWD050: { Reward r = new Rewd050(); r.setRewardType(t); return r; }
                case RewardType.REWD051: { Reward r = new Rewd051(); r.setRewardType(t); return r; }
                case RewardType.REWD052: { Reward r = new Rewd052(); r.setRewardType(t); return r; }
                default:
                    throw new FooException("GetObject::Unknown RewardType(" + ((int)t).ToString() + ")");
            }
        }
    };
}