using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace QuestStudio
{
    class StructWriter
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

        ///<summary>
        ///Calculate the necessary padding for a datatype at a certain index
        ///</summary>
        private static int GetPadding(int Offset, Type datatype)
        {
            int SizeOfType = GetTypeSize(datatype);
            
            if (Offset % SizeOfType != 0)
                return (SizeOfType - (Offset % SizeOfType));
            else
                return 0;
        }


        // 0xCD     Clean Memory    Allocated memory via malloc or new but never written by the application. 
        public static byte[] CreateSpecialByteArray(int length)
        {
            var arr = new byte[length];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = 0xCD;

            return arr;
        }

        ///<summary>
        ///Concat two byte arrays, adding sufficient padding based on the length and supplied datatype
        ///</summary>
        public static void PrePadding(ref byte[] allData, Type dt, byte[] fieldData)
        {
            int Padding = GetPadding(allData.Length, dt);
            if (Padding > 0)
            {
                byte[] padding = CreateSpecialByteArray(Padding);
                byte[] combined = padding.Concat(fieldData).ToArray();
                allData = allData.Concat(combined).ToArray();
            }
            else
            {
                allData = allData.Concat(fieldData).ToArray();
            }
        }

        ///<summary>
        ///Convert an object into bytes and add them to the byte array with sufficient padding
        ///</summary>
        public static void ObjectToBytes(object Obj, ref byte[] data)
        {
            Type dt = Obj.GetType();
            if (dt == typeof(bool))
                PrePadding(ref data, typeof(bool), BitConverter.GetBytes((bool)Obj));
            else if (dt == typeof(sbyte))
                PrePadding(ref data, typeof(byte), new byte[1] { (byte)Obj });//Can't use BitConverter, returns byte[2] for byte
            else if (dt == typeof(byte))
                PrePadding(ref data, typeof(byte), new byte[1] { (byte)Obj });//Can't use BitConverter, returns byte[2] for byte
            else if (dt == typeof(short))
                PrePadding(ref data, typeof(short), BitConverter.GetBytes((short)Obj));
            else if (dt == typeof(int))
                PrePadding(ref data, typeof(int), BitConverter.GetBytes((int)Obj));
            else if (dt == typeof(long))
                PrePadding(ref data, typeof(long), BitConverter.GetBytes((long)Obj));
            else if (dt == typeof(ushort))
                PrePadding(ref data, typeof(ushort), BitConverter.GetBytes((ushort)Obj));
            else if (dt == typeof(uint))
                PrePadding(ref data, typeof(uint), BitConverter.GetBytes((uint)Obj));
            else if (dt == typeof(ulong))
                PrePadding(ref data, typeof(ulong), BitConverter.GetBytes((ulong)Obj));

            else if (dt == typeof(string))
            {
                byte[] stringbytes = Encoding.GetEncoding("EUC-KR").GetBytes((string)Obj).Concat(new byte[1] { 0x00 }).ToArray().Concat(new byte[1] {0x00}).ToArray();

                PrePadding(ref data, typeof(short), BitConverter.GetBytes((short)stringbytes.Length));

                data = data.Concat(stringbytes).ToArray();
            }
            else if (dt.IsArray)
            {
                Array arr = (Array)Obj;
                PrePadding(ref data, typeof(int), BitConverter.GetBytes((int)arr.Length));
                foreach (object d in arr)
                    GetFields(d, ref data);
            }
            else if (dt.IsEnum)
            {
                object value = System.Convert.ChangeType(Obj, Enum.GetUnderlyingType(dt));
                ObjectToBytes(value, ref data);
            }
            else
            {
                throw new FooException("Writing Unknown DataType " + dt);
            }
        }

        ///<summary>
        ///Loop over an object's properties and add them to the reference bytelist.
        ///</summary>
        public static void GetFields(object Obj, ref byte[] byteList)
        {
            //Console.WriteLine("###" + Obj.GetType().ToString() + "###");
            FieldInfo[] myFieldInfo = Obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo f in myFieldInfo)
            {
                object value = f.GetValue(Obj);
                //Console.WriteLine("[{3}] Name: {0}, FieldType: {1}, Value: {2}", f.Name, f.FieldType, value, byteList.Length);
                ObjectToBytes(value, ref byteList);
            }

            // structure is padded so the total length is a multiple of the largest member size
            if (byteList.Length % 4 != 0)
            {
                byte[] padding = CreateSpecialByteArray(4 - (byteList.Length % 4));
                byteList = byteList.Concat(padding).ToArray();
            }
        }

        public static byte[] Convert(object Obj)
        {
            byte[] byteList = new byte[0] { };
            GetFields(Obj, ref byteList);
            return byteList;
        }
    }
}