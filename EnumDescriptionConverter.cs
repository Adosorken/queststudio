using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace QuestStudio
{
    ///<summary>
    ///Uses the Description Attribute to show a different text for Enum values in the PropertyGrid
    ///</summary>
    class EnumDescriptionConverter : EnumConverter
    {
        private Type _enumType;
        public EnumDescriptionConverter(Type type) : base(type)
        {
            _enumType = type;

            //foreach (FieldInfo fi1 in _enumType.GetFields())
            //{
            //    foreach (FieldInfo fi2 in _enumType.GetFields())
            //    {
            //        DescriptionAttribute dna1 = (DescriptionAttribute)Attribute.GetCustomAttribute(fi1, typeof(DescriptionAttribute));
            //        DescriptionAttribute dna2 = (DescriptionAttribute)Attribute.GetCustomAttribute(fi2, typeof(DescriptionAttribute));
            //        if ((dna1 != null) && (dna2 != null) && (dna1.Description == dna2.Description) && (fi1.Name != fi2.Name) )
            //        {
            //            MessageBox.Show("Duplicate Enum Description, " + dna1.Description);
            //           // throw new FooException("Duplicate Enum Description");
            //        }
            //    }
            //}
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
            DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
            if (dna != null)
                return dna.Description;
            else
                return value.ToString();
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            foreach (FieldInfo fi in _enumType.GetFields())
            {
                DescriptionAttribute dna =
                (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
                if ((dna != null) && ((string)value == dna.Description))
                    return Enum.Parse(_enumType, fi.Name);
            }
            return Enum.Parse(_enumType, (string)value);
        }
    }

    ///<summary>
    ///Extends Enums to get the Description Attribute.
    ///</summary>
    public static class EnumExtensions
    {
        public static string getText(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
            {
               MessageBox.Show("enum.getText::Cannot convert value " + value.ToString() + " to " + value.GetType());
               Console.WriteLine("enum.getText::Cannot convert value " + value.ToString() + " to " + value.GetType());
                return value.ToString();
            }

            DescriptionAttribute attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static object PasteConvertStringToEnum(Type dt, object value)
        {
            foreach (FieldInfo fi in dt.GetFields())
            {
                DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
                if ((dna != null) && ((string)value == dna.Description))
                    return Enum.Parse(dt, fi.Name);
            }
            return Enum.Parse(dt, (string)value);
        }
    };

    public static class StringExtension
    {
        public static string GetLast(this string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }
    }
}
