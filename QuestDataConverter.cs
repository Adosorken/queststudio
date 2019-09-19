using System;
using System.ComponentModel;

namespace QuestStudio
{
    class QuestDataConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context,
                                 System.Globalization.CultureInfo culture,
                                 object value, Type destType)
        {
            if (destType == typeof(string) && value is QUEST_DATA_CONDITION)
            {
                QUEST_DATA_CONDITION d = (QUEST_DATA_CONDITION)value;
                return d.VarType + "(" + d.VarNo + ") " + d.op.getText() + " " + d.Value;
            }
            else if (destType == typeof(string) && value is ABIL_DATA_CONDITION)
            {
                ABIL_DATA_CONDITION d = (ABIL_DATA_CONDITION)value;
                return d.type + " " + d.op.getText() + " " + d.value;
            }
            else if (destType == typeof(string) && value is ITEM_DATA_CONDITION)
            {
                ITEM_DATA_CONDITION d = (ITEM_DATA_CONDITION)value;
                return d.itemsn + " @ " + d.equipslot.getText() + " " + d.op.getText() + " " + d.cnt;
            }
            else if (destType == typeof(string) && value is QUEST_DATA_REWARD)
            {
                QUEST_DATA_REWARD d = (QUEST_DATA_REWARD)value;
                return d.VarType + "(" + d.VarNo + ") " + d.op.getText() + " " + d.Value;
            }
            else if (destType == typeof(string) && value is ABIL_DATA_REWARD)
            {
                ABIL_DATA_REWARD d = (ABIL_DATA_REWARD)value;
                return d.type + " " + d.op.getText() + " " + d.value;
            }
            return base.ConvertTo(context, culture, value, destType);
        }
    }
}
