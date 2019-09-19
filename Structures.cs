using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;

namespace QuestStudio
{
    [TypeConverter(typeof(QuestDataConverter))]
    public class QUEST_DATA_CONDITION
    {
        public ushort VarNo { get; set; }
        public QuestVarType VarType { get; set; }
        public short Value { get; set; }
        public Operator_Cond op { get; set; }
    };

    [TypeConverter(typeof(QuestDataConverter))]
    public class QUEST_DATA_REWARD
    {
        public QUEST_DATA_REWARD() { op = Operator_Act.OP_SET; }
        public ushort VarNo { get; set; }
        public QuestVarType VarType { get; set; }
        public short Value { get; set; }
        public Operator_Act op { get; set; }
    };

    [TypeConverter(typeof(QuestDataConverter))]
    public class ABIL_DATA_CONDITION 
    {
        public Ability type { get; set; }
        public int value { get; set; }
        public Operator_Cond op { get; set; }
    };

    [TypeConverter(typeof(QuestDataConverter))]
    public class ABIL_DATA_REWARD
    {
        public ABIL_DATA_REWARD() { op = SetAddSubOperator.OP_SET; }
        public Ability type { get; set; }
        public int value { get; set; }
        public SetAddSubOperator op { get; set; } // Ability doesn't need Set ON/OFF
    };

    [TypeConverter(typeof(QuestDataConverter))]
    public class ITEM_DATA_CONDITION
    {
        public uint itemsn { get; set; }
        public EquipIndex equipslot { get; set; }
        public int cnt { get; set; }
        public Operator_Cond op { get; set; }
    };

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum ConditionType
    {
        [Description("Select quest")]
        COND000 = 0,
        [Description("Quest variable check")]
        COND001 = 1,
        [Description("User variable check")]
        COND002 = 2,
        [Description("Ability check")]
        COND003 = 3,
        [Description("Item check")]
        COND004 = 4,
        [Description("Party check")]
        COND005 = 5,
        [Description("Position check")]
        COND006 = 6,
        [Description("World time check [unused]")]
        COND007 = 7,
        [Description("Remaining quest time")]
        COND008 = 8,
        [Description("Skill check")]
        COND009 = 9,
        [Description("Random percent")]
        COND010 = 10,
        [Description("Check NPC/eventobj variable")]
        COND011 = 11,
        [Description("Select eventobj")]
        COND012 = 12,
        [Description("Select NPC")]
        COND013 = 13,
        [Description("Quest switch check")]
        COND014 = 14,
        [Description("Party member count")]
        COND015 = 15,
        [Description("Zone time check")]
        COND016 = 16,
        [Description("Compare NPC variables")]
        COND017 = 17,
        [Description("Time of day (month)")]
        COND018 = 18,
        [Description("Time of day (week) [unused]")]
        COND019 = 19,
        [Description("Team check [unused]")]
        COND020 = 20,
        [Description("Distance to NPC/eventobj")]
        COND021 = 21,
        [Description("Channel check")]
        COND022 = 22,
        [Description("In Clan")]
        COND023 = 23,
        [Description("Clan rank")]
        COND024 = 24,
        [Description("Clan contribution [unused]")]
        COND025 = 25,
        [Description("Clan grade")]
        COND026 = 26,
        [Description("Clan points")]
        COND027 = 27,
        [Description("Clan money")]
        COND028 = 28,
        [Description("Clan member count [unused]")]
        COND029 = 29,
        [Description("Clan skill")]
        COND030 = 30,


        /* Custom Conditions */
        [Description("Does not have quest")]
        COND050 = 50,
        [Description("In ArenaGroup")]
        COND051 = 51,
        [Description("Compare arena status to eventvalue")]
        COND052 = 52,
        [Description("Check quest switch by eventvalue")]
        COND053 = 53,
        [Description("Arena game check")]
        COND054 = 54,
    };

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum RewardType
    {
        [Description("Quest action")]
        REWD000,
        [Description("Give/take item")]
        REWD001,
        [Description("Change quest variable")]
        REWD002,
        [Description("Change ability")]
        REWD003,
        [Description("Change user variable")]
        REWD004,
        [Description("Give item/exp/money")]
        REWD005,
        [Description("Set HP/MP %")]
        REWD006,
        [Description("Warp user/party")]
        REWD007,
        [Description("Spawn mob")]
        REWD008,
        [Description("Set next trigger")]
        REWD009,
        [Description("Reset stats")]
        REWD010,
        [Description("Set NPC/eventobj variable")]
        REWD011,
        [Description("NPC message")]
        REWD012,
        [Description("Set next NPC/eventobj trigger")]
        REWD013,
        [Description("Add/remove Skill")]
        REWD014,
        [Description("Set quest switch")]
        REWD015,
        [Description("Reset switch group [unused]")]
        REWD016,
        [Description("Reset all quest switches [unused]")]
        REWD017,
        [Description("NPC Announce [unused]")]
        REWD018,
        [Description("Do zone trigger")]
        REWD019,
        [Description("Set team")]
        REWD020,
        [Description("Set revive location")]
        REWD021,
        [Description("Set regen mode [unused]")]
        REWD022,
        [Description("Increase clan grade")]
        REWD023,
        [Description("Set clan money")]
        REWD024,
        [Description("Set clan points")]
        REWD025,
        [Description("Add/remove clan skill")]
        REWD026,
        [Description("[bug27] [unused]")] // Contribute bugged
        REWD027,
        [Description("Warp clan members in range")]
        REWD028,
        [Description("LUA function")]
        REWD029,
        [Description("Reset skills")]
        REWD030,
        [Description("[bug31] [unused]")]
        REWD031,
        [Description("Give item [unused]")]
        REWD032,
        [Description("[bug33] [unused]")]
        REWD033,
        [Description("Show/hide NPC")]
        REWD034,

        /* Custom Rewards */
        [Description("Reward from break")]
        REWD050 = 50,
        [Description("Write message")]
        REWD051 = 51,
        [Description("Set quest switch by eventvalue")]
        REWD052 = 52,
    };

    public enum QuestVarType : ushort
    {
        //[Description("Per-Quest Variables (0-9)")]
        QuestVar = 0x0000,

        //[Description("Per-Quest Switches (0-31)")]
        QuestSwitch = 0x0100,

        //[Description("Remaining quest time.")]
        QuestTime = 0x0200,

        //[Description("Used in the Episode Quest (0-4)")]
        EpisodeVar = 0x0300,

        //[Description("Used in Job Quests (0-2)")]
        JobVar = 0x0400,

        //[Description("Planet Variables (0-6)")]
        PlanetVar = 0x0500,

        //[Description("Used in Union Quests (0-9)")]
        UnionVar = 0x0600,
    };

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum Operator_Cond : byte
    {
        [Description("==")]
        OP_EQUALS = 0,
        [Description(">")]
        OP_G = 1,
        [Description(">=")]
        OP_GE = 2,
        [Description("<")]
        OP_L = 3,
        [Description("<=")]
        OP_LE = 4,
        [Description("!=")]
        OP_INEQ = 10,
    };

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum Operator_Act : byte
    {
        [Description("=")]
        OP_SET = 5,
        [Description("+=")]
        OP_ADD = 6,
        [Description("-=")]
        OP_SUB = 7,
        [Description("SET(0/OFF)")]
        OP_OFF = 8,
        [Description("SET(1/ON)")]
        OP_ON = 9,
    };

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum Operator : byte
    {
        [Description("==")]
        OP_EQUALS = 0,
        [Description(">")]
        OP_G = 1,
        [Description(">=")]
        OP_GE = 2,
        [Description("<")]
        OP_L = 3,
        [Description("<=")]
        OP_LE = 4,
        [Description("=")]
        OP_SET = 5,
        [Description("+=")]
        OP_ADD = 6,
        [Description("-=")]
        OP_SUB = 7,
        [Description("SET(0/OFF)")]
        OP_OFF = 8,
        [Description("SET(1/ON)")]
        OP_ON = 9,
        [Description("!=")]
        OP_INEQ = 10,
    };

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum AddSubOperator : byte
    {
        [Description("+=")]
        OP_ADD = 6,
        [Description("-=")]
        OP_SUB = 7,
    };

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum SetAddSubOperator : byte
    {
        [Description("=")]
        OP_SET = 5,
        [Description("+=")]
        OP_ADD = 6,
        [Description("-=")]
        OP_SUB = 7,
    };

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum QuestActionOperator : byte
    {
        Remove = 0,
        Append = 1,
        [Description("Set (keep timer)")]
        Set_Keep = 2,
        [Description("Set (reset timer)")]
        Set_Reset = 3,
        Select = 4,
    };

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum Ability : int
    {
        [Description("Ability 0")]
        ABILITY0 = 0,
        //[Description("Ability 1")]
        //ABILITY1 = 1,
        [Description("Gender")]
        ABILITY2 = 2,
        [Description("Birth Stone")]
        ABILITY3 = 3,
        [Description("Job")]
        ABILITY4 = 4,
        [Description("Union")]
        ABILITY5 = 5,
        [Description("Rank")]
        ABILITY6 = 6,
        [Description("Fame")]
        ABILITY7 = 7,
        [Description("Face")]
        ABILITY8 = 8,
        [Description("Hair")]
        ABILITY9 = 9,
        [Description("Strength")]
        ABILITY10 = 10,
        [Description("Dexterity")]
        ABILITY11 = 11,
        [Description("Intelligence")]
        ABILITY12 = 12,
        [Description("Concentration")]
        ABILITY13 = 13,
        [Description("Charm")]
        ABILITY14 = 14,
        [Description("Sensibility")]
        ABILITY15 = 15,

        // Stats recalculated frequently, not affected by quest rewards.
        //[Description("HP")]
        //ABILITY16 = 16,
        //[Description("MP")]
        //ABILITY17 = 17,
        //[Description("Attack Power")]
        //ABILITY18 = 18,
        //[Description("Defense")]
        //ABILITY19 = 19,
        //[Description("Hit Rate")]
        //ABILITY20 = 20,
        //[Description("Magic Defense")]
        //ABILITY21 = 21,
        //[Description("Dodge Rate")]
        //ABILITY22 = 22,
        //[Description("Moving Speed")]
        //ABILITY23 = 23,
        //[Description("Attack Speed")]
        //ABILITY24 = 24,
        //[Description("Weight")]
        //ABILITY25 = 25,
        //[Description("Critical")]
        //ABILITY26 = 26,
        //[Description("HP Recovery Rate")]
        //ABILITY27 = 27,
        //[Description("MP Recovery Rate")]
        //ABILITY28 = 28,
        //[Description("MP Decrease")]
        //ABILITY29 = 29,

        [Description("Experience")]
        ABILITY30 = 30,
        [Description("Level")]
        ABILITY31 = 31,
        [Description("Stat Points")]
        ABILITY32 = 32,
        [Description("PK Flag")]
        ABILITY33 = 33,
        [Description("Team")]
        ABILITY34 = 34,
        [Description("Head Size")]
        ABILITY35 = 35,
        [Description("Body Size")]
        ABILITY36 = 36,
        [Description("Skill Points")]
        ABILITY37 = 37,
        [Description("Max HP")]
        ABILITY38 = 38,
        [Description("Max MP")]
        ABILITY39 = 39,
        [Description("Money")]
        ABILITY40 = 40,


        //[Description("[Passive] Bare Hand Attack Power")]
        //ABILITY41 = 41,
        //[Description("[Passive] Attack Power of One-Hand Weapon")]
        //ABILITY42 = 42,
        //[Description("[Passive] Attack Power of Two-Hand Weapon")]
        //ABILITY43 = 43,
        //[Description("[Passive] Attack Power of Bow")]
        //ABILITY44 = 44,
        //[Description("[Passive] Attack Power of Gun")]
        //ABILITY45 = 45,
        //[Description("[Passive] Attack Power of Magick Weapon")]
        //ABILITY46 = 46,
        //[Description("[Passive] Attack Power of Bow gun")]
        //ABILITY47 = 47,
        //[Description("[Passive] Attack Power of Combat Weapon")]
        //ABILITY48 = 48,
        //[Description("[Passive] Attack Speed of Bow")]
        //ABILITY49 = 49,
        //[Description("[Passive] Attack Speed of Gun")]
        //ABILITY50 = 50,
        //[Description("[Passive] Attack Speed of Combat Weapon")]
        //ABILITY51 = 51,
        //[Description("[Passive] Moving Speed")]
        //ABILITY52 = 52,
        //[Description("[Passive] Defense Power")]
        //ABILITY53 = 53,
        //[Description("[Passive] Max HP")]
        //ABILITY54 = 54,
        //[Description("[Passive] Max MP")]
        //ABILITY55 = 55,
        //[Description("[Passive] HP Recovery Amount")]
        //ABILITY56 = 56,
        //[Description("[Passive] MP Recovery Rate")]
        //ABILITY57 = 57,
        //[Description("[Passive] Capacity of Bag Pack")]
        //ABILITY58 = 58,
        //[Description("[Passive] Discount in Purchase")]
        //ABILITY59 = 59,
        //[Description("[Passive] Sales Premium")]
        //ABILITY60 = 60,
        //[Description("[Passive] Decrease of required MP spending")]
        //ABILITY61 = 61,
        //[Description("[Passive] Expansion of Summoning Gauge")]
        //ABILITY62 = 62,
        //[Description("[Passive] Extra Drop Chance")]
        //ABILITY63 = 63,


        [Description("Pay Planet Tax")]
        ABILITY73 = 73,
        [Description("Current Planet")]
        ABILITY75 = 75,
        [Description("Stamina")]
        ABILITY76 = 76,
        //[Description("Fuel")]
        //ABILITY77 = 77,

        [Description("Union Points 1 [Junon Order]")]
        ABILITY81 = 81,
        //[Description("Union Points 2")]
        //ABILITY82 = 82,
        [Description("Union Points 3 [Crusaders]")]
        ABILITY83 = 83,
        [Description("Union Points 4 [Arumic]")]
        ABILITY84 = 84,
        [Description("Union Points 5 [Ferrell]")]
        ABILITY85 = 85,
        //[Description("Union Points 6")]
        //ABILITY86 = 86,
        [Description("Valor")]
        ABILITY87 = 87,
        [Description("Premium Points")]
        ABILITY88 = 88,
        //[Description("Union Points 9")]
        //ABILITY89 = 89,
        //[Description("Union Points 10")]
        //ABILITY90 = 90,

        // Clan
        [Description("Clan ID")]
        ABILITY91 = 91,
        [Description("Clan Points")]
        ABILITY92 = 92,
        [Description("Clan Position")]
        ABILITY93 = 93,

        // MaintainAbility coupons
        //[Description("Free warehouse")]
        //ABILITY94 = 94,
        //[Description("Warehouse Extension")]
        //ABILITY95 = 95,
        //[Description("Change private shop appearance")]
        //ABILITY96 = 96,
        //[Description("[Medal] Experience Rate")]
        //ABILITY97 = 97,

        [Description("Farming Permit (Junon)")]
        ABILITY105 = 105,
        [Description("Farming Permit (Luna)")]
        ABILITY106 = 106,
        [Description("Farming Permit (Eldeon)")]
        ABILITY107 = 107,
        [Description("Farming Permit (Oro)")]
        ABILITY108 = 108,

        [Description("Cartel ELO")]
        ABILITY115 = 115,

        //[Description("Gathering Double Speed")]
        //ABILITY110 = 110,
        //[Description("Gathering Double Drop")]
        //ABILITY111 = 111,
        //[Description("Gathering Speed")]
        //ABILITY112 = 112,

        //[Description("[Passive] Magic Resistance")]
        //ABILITY121 = 121,
        //[Description("[Passive] Hitting Rate")]
        //ABILITY122 = 122,
        //[Description("[Passive] Critical Rate")]
        //ABILITY123 = 123,
        //[Description("[Passive] Dodge Rate")]
        //ABILITY124 = 124,
        //[Description("[Passive] Shield Defense Power")]
        //ABILITY125 = 125,

        [Description("Soulmate Points")]
        ABILITY141 = 141,
        [Description("Soulmate Level")]
        ABILITY142 = 142,
        [Description("Soulmate Party Lvl ")]
        ABILITY143 = 143,
        [Description("Str, Dex, Int, Con, Cha, Sen")]
        ABILITY144 = 144,
        [Description("Current Union Points")]
        ABILITY146 = 146,

        //// Prestige stats
        //[Description("Posion Chance")]
        //ABILITY147 = 147,
        //[Description("Poison Rate")]
        //ABILITY148 = 148,
        //[Description("Stun Chance")]
        //ABILITY149 = 149,
        //[Description("Life Steal")]
        //ABILITY150 = 150,
        //[Description("Damage Reflect")]
        //ABILITY151 = 151,
        //[Description("Stun Deflect")]
        //ABILITY152 = 152,
        //[Description("Stealth Extend")]
        //ABILITY153 = 153,
        //[Description("Assist Survival")]
        //ABILITY154 = 154,
    };

    public enum ItemRewardOperator : byte
    {
        Take = 0,
        Give = 1
    };

    public enum OnOffOperator : byte
    {
        Off = 0,
        On = 1
    };

    public enum ArenaGameOperator : byte
    {
        GameID = 0,
        GameType = 1
    };

    public enum HasOperator : byte
    {
        HasNot = 0,
        Has = 1
    };

    public enum Who08 : byte
    {
        Player = 0,
        NPC = 1,
        EventObject = 2,
        Coordinate = 3,
    };

    public enum PlayerOrParty : byte
    {
        Player = 0,
        Party = 1,
    }

    public enum Who016 : byte
    {
        NPC = 0,
        EventObject = 1,
        Player = 2,
    };

    public enum EquipIndex : int
    {
        Inventory = 0,
        Faceitem = 1,
        Helmet = 2,
        Armor = 3,
        Backitem = 4,
        Arm = 5,
        Boots = 6,
        Weapon_R = 7,
        Weapon_L = 8,
        Necklace = 9,
        Ring = 10,
        Earring = 11,
        Inventory2 = 12,
        QuestItem = 13,
    };

    public enum NpcOrEvent : byte
    {
        NPC = 0,
        EventObject = 1
    };

    public enum MsgType : byte
    {
        Chat = 0,
        Shout = 1,
        Announce = 2,
    };

    public enum ChatType : byte
    {
        ALL,
        SHOUT,
        PARTY,
        WHISPER,
        NOTICE,
        SYSTEM,
        ERROR,
        QUEST,
        QUESTREWARD,
        CLAN,
        TRADE,
        ALLIED,
        FRIEND,
        GROUP,
        DEBUG,
        ANNOUNCE,
    };

    public enum AddRemoveOperator : byte
    {
        Remove = 0,
        Add = 1,
    };

    public enum TeamNoType : byte
    {
        Player = 0,
        Clan = 1,
        Party = 2,
    };

    public enum OffOnToggle : byte
    {
        Off = 0,
        On = 1,
        Toggle = 2,
    };

    public enum NpcVisibility : byte
    {
        Hide = 0,
        Show = 1,
        Toggle = 2,
    };

    public enum ItemRewardType : byte
    {
        Experience = 0,
        Money = 1,
        Item = 2,
    };
    
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum ItemRewardEquation : byte
    {
        [Description("0 (lower by lvl)")]
        Equation0 = 0,
        [Description("1 (exponential by lvl)")]
        Equation1 = 1,
        [Description("2 (zulie, value * questvar9)")]
        Equation2 = 2,
        [Description("3 (lower by lvl)")]
        Equation3 = 3,
        [Description("4 (higher by lvl & charm)")]
        Equation4 = 4,
        [Description("5 (lower by lvl)")]
        Equation5 = 5,
        [Description("6 (higher by lvl & charm)")]
        Equation6 = 6,
        [Description("15 (none, value is amount)")]
        Equation15 = 15,
    };

    public enum ClanPos : short
    {
        PenaltyRookie = 0,
        Rookie = 1,
        Veteran = 2,
        Captain = 3,
        Commander = 4,
        DeputyMaster = 5,
        Master = 6,
    };

    public enum DayOfWeek : byte
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
    };
}
