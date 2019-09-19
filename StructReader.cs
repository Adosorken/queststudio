using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace QuestStudio
{
    public class StructReader
    {
        private static int GetTypeSize(Type T)
        {
            if (T == typeof(byte) || T == typeof(sbyte) || T == typeof(bool))
                return 1;
            else if (T == typeof(ushort) || T == typeof(short))
                return 2;
            else if (T == typeof(uint) || T == typeof(int))
                return 4;
            else if (T == typeof(ulong) || T == typeof(long))
                return 8;

            throw new FooException("GetTypeSize::Unknown Type");
        }

        private static int FixAlignment(ref int Offset, Type datatype)
        {
            int SizeOfType = GetTypeSize(datatype);

            //Console.WriteLine("Reading {0} at Offset {1} (size={2})", datatype, Offset, SizeOfType);

            int PaddedOffset = Offset;
            if (Offset % SizeOfType != 0) // misaligned
                PaddedOffset += (SizeOfType - (Offset % SizeOfType));

            Offset = PaddedOffset + SizeOfType; // update the ref parameter

            //Console.WriteLine("Read from: {0}, Next Object at {1}", PaddedOffset, Offset);
            return PaddedOffset;
        }
        
        T CreateType<T>() where T : new()
        {
            return new T();
        }

        public static object BytesToObject(byte[] data, ref int offs, Type dt)
        {
            if (dt == typeof(bool))
                return (bool)(data[FixAlignment(ref offs, dt)] != 0);
            else if (dt == typeof(sbyte))
                return (sbyte)data[FixAlignment(ref offs, dt)];
            else if (dt == typeof(byte))
                return (byte)data[FixAlignment(ref offs, dt)];
            else if (dt == typeof(short))
                return BitConverter.ToInt16(data, FixAlignment(ref offs, dt));
            else if (dt == typeof(int))
                return BitConverter.ToInt32(data, FixAlignment(ref offs, dt));
            else if (dt == typeof(long))
                return BitConverter.ToInt64(data, FixAlignment(ref offs, dt));
            else if (dt == typeof(ushort))
                return BitConverter.ToUInt16(data, FixAlignment(ref offs, dt));
            else if (dt == typeof(uint))
                return BitConverter.ToUInt32(data, FixAlignment(ref offs, dt));
            else if (dt == typeof(ulong))
                return BitConverter.ToUInt64(data, FixAlignment(ref offs, dt));

            else if (dt == typeof(string))
            {
                BitConverter.ToInt16(data, FixAlignment(ref offs, typeof(short))); // unused

                int length = 0;
                while (data[offs + length] != '\0')
                    length++;

                string str = Encoding.GetEncoding("EUC-KR").GetString(data, offs, length);
                offs += length + 1;
                return str;
            }
            else if (dt.IsArray)
            {
                int dataCount = BitConverter.ToInt32(data, FixAlignment(ref offs, typeof(int)));
                if (dataCount > 1 && data.Length <= 20)
                    dataCount = 1; // one file has a bugged action

                Array arr = Array.CreateInstance(dt.GetElementType(), dataCount);
                for (int i = 0; i < dataCount; i++)
                {
                    object o = Activator.CreateInstance(dt.GetElementType());
                    StructReader.GetFields(data, ref offs, ref o);
                    arr.SetValue(o, i);
                }
                return arr;
            }
            else if (dt.IsEnum)
            {
                object value = BytesToObject(data, ref offs, Enum.GetUnderlyingType(dt));
                return Enum.Parse(dt, value.ToString());
            }
            else
            {
                throw new FooException("Reading Unknown DataType " + dt);
            }
        }

        public static void GetFields(byte[] data, ref int index, ref object Obj)
        {
            //Console.WriteLine("###" + Obj.GetType().ToString() + "###");
            FieldInfo[] myFieldInfo = Obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo f in myFieldInfo)
            {
                Object value = BytesToObject(data, ref index, f.FieldType);
                f.SetValue(Obj, value);
                //Console.WriteLine("[{3}] Name: {0}, FieldType: {1}, Value: {2}", f.Name, f.FieldType, value, index);
            }

            // structure is padded so the total length is a multiple of the largest member size
            if (index % 4 != 0)
            {
                index += (4 - (index % 4));
            }
        }

        public static void Convert(byte[] data, ref object Obj)
        {
            int index = 0;
            GetFields(data, ref index, ref Obj);
        }
    }
}
