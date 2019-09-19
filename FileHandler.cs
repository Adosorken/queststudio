#region using
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
#endregion

#region New Value Types

/// <summary>
/// Class for the read/write value "NString"
/// </summary>
public class NString
{
    #region member declarations
    private string stringData = string.Empty;
    #endregion

    #region constructor
    /// <summary>
    /// Create an empty NString
    /// </summary>
    public NString()
    {

    }

    /// <summary>
    /// Create an NString using a previous string value
    /// </summary>
    /// <param name="value">String value</param>
    public NString(string value)
    {
        stringData = value;
    }
    #endregion

    /// <summary>
    /// Gets the length of the string
    /// </summary>
    public int Length
    {
        get { return stringData.Length; }
    }

    /// <summary>
    /// Gets the character from the index of the string
    /// </summary>
    /// <param name="index">Character position</param>
    /// <returns>Selected character</returns>
    public char this[int index]
    {
        get { return stringData[index]; }
    }

    /// <summary>
    /// Converts string to NString
    /// </summary>
    /// <param name="value">String value</param>
    /// <returns>NString of the string value</returns>
    public static implicit operator NString(string value)
    {
        return new NString(value);
    }

    /// <summary>
    /// Converts NString to string
    /// </summary>
    /// <param name="value">NString value</param>
    /// <returns>String of the NString value</returns>
    public static implicit operator string(NString value)
    {
        return value.stringData;
    }
}

#endregion

public class FileHandler
{
    /// <summary>
    /// File mode
    /// </summary>
    public enum FileOpenMode
    {
        Reading,
        Writing,
    }

    #region Member Declarations

    private FileStream fileStream;
    private BinaryReader binaryReader;
    private BinaryWriter binaryWriter;
    private FileOpenMode fileOpenMode;
    private Encoding encoding;

    #endregion

    #region Constructor

    /// <summary>
    /// Opens a file for reading/saving
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="mode">Whether to open the file for reading or writing</param>
    /// <param name="encodeType">Encoding type to use for string based values</param>
    public FileHandler(string path, FileOpenMode mode, Encoding encodeType)
    {
        encoding = encodeType;
        fileOpenMode = mode;

        if (fileOpenMode == FileOpenMode.Reading)
        {
            fileStream = File.OpenRead(path);
            binaryReader = new BinaryReader(fileStream);
        }
        else if (fileOpenMode == FileOpenMode.Writing)
        {
            fileStream = File.OpenWrite(path);
            binaryWriter = new BinaryWriter(fileStream);
        }
    }

    #endregion

    /// <summary>
    /// Seek to a specified position in the file
    /// </summary>
    /// <param name="offset">File offset</param>
    /// <param name="origin">Origin to seek from</param>
    public void Seek(int offset, SeekOrigin origin)
    {
        fileStream.Seek(offset, origin);
    }

    /// <summary>
    /// Gets the current position of the file stream
    /// </summary>
    /// <returns>File position</returns>
    public int Tell()
    {
        return (int)fileStream.Position;
    }

    #region Reading

    /// <summary>
    /// Reads a value from the file stream using BinaryReader
    /// </summary>
    /// <typeparam name="T">Structure type</typeparam>
    /// <param name="length">Length of the value</param>
    /// <returns>T value</returns>
    public T Read<T>(int length)
    {
        if (typeof(T) == typeof(byte[]))
            return (T)((object)binaryReader.ReadBytes(length));

        if (typeof(T) == typeof(string))
            return (T)((object)encoding.GetString(binaryReader.ReadBytes(length)));

        if (typeof(T) == typeof(NString))
        {
            List<byte> dynamicBuffer = new List<byte>(length);
            byte currentByte = 0;

            for (int i = 0; i < length; i++)
            {
                if ((currentByte = binaryReader.ReadByte()) != 0 && currentByte != 0xCD)
                    dynamicBuffer.Add(currentByte);
            }

            return (T)((object)new NString(encoding.GetString(dynamicBuffer.ToArray())));
        }

        throw new Exception("Invalid data type");
    }

    /// <summary>
    /// Reads a value using BinaryReader from the file stream
    /// </summary>
    /// <typeparam name="T">Structure type</typeparam>
    /// <returns>T value</returns>
    public T Read<T>()
    {
        if (typeof(T) == typeof(byte))
            return (T)((object)binaryReader.ReadByte());

        if (typeof(T) == typeof(sbyte))
            return (T)((object)binaryReader.ReadSByte());

        if (typeof(T) == typeof(char))
            return (T)((object)binaryReader.ReadChar());

        if (typeof(T) == typeof(short))
            return (T)((object)binaryReader.ReadInt16());

        if (typeof(T) == typeof(ushort))
            return (T)((object)binaryReader.ReadUInt16());

        if (typeof(T) == typeof(int))
            return (T)((object)binaryReader.ReadInt32());

        if (typeof(T) == typeof(uint))
            return (T)((object)binaryReader.ReadUInt32());

        if (typeof(T) == typeof(long))
            return (T)((object)binaryReader.ReadInt64());

        if (typeof(T) == typeof(ulong))
            return (T)((object)binaryReader.ReadUInt64());

        if (typeof(T) == typeof(float))
            return (T)((object)binaryReader.ReadSingle());

        if (typeof(T) == typeof(double))
            return (T)((object)binaryReader.ReadDouble());

        if (typeof(T) == typeof(decimal))
            return (T)((object)binaryReader.ReadDecimal());

        GCHandle handle = GCHandle.Alloc(binaryReader.ReadBytes(Marshal.SizeOf(typeof(T))), GCHandleType.Pinned);
        T value;

        try
        {
            value = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free();
        }

        return value;
    }

    #endregion

    #region Writing

    /// <summary>
    /// Writes a value to the file stream using BinaryWriter
    /// </summary>
    /// <typeparam name="T">Structure type</typeparam>
    /// <param name="value">Value to write</param>
    /// <param name="length">Length of the value</param>
    public void Write<T>(T value, int length)
    {
        if (typeof(T) == typeof(NString))
        {
            NString nString = (NString)((object)value);

            byte[] stringData = encoding.GetBytes(nString);
            byte[] data = new byte[length];

            for (int i = 0; i < stringData.Length; i++)
                data[i] = stringData[i];

            binaryWriter.Write(data);
        }
    }

    /// <summary>
    /// Writes a value to the file stream using BinaryWriter
    /// </summary>
    /// <typeparam name="T">Structure type</typeparam>
    /// <param name="value">Value to write</param>
    public void Write<T>(T value)
    {
        if (typeof(T) == typeof(string))
            binaryWriter.Write(encoding.GetBytes((string)((object)value)));
        else if (typeof(T) == typeof(byte))
            binaryWriter.Write((byte)((object)value));
        else if (typeof(T) == typeof(byte[]))
            binaryWriter.Write((byte[])((object)value));
        else if (typeof(T) == typeof(sbyte))
            binaryWriter.Write((sbyte)((object)value));
        else if (typeof(T) == typeof(char))
            binaryWriter.Write((char)((object)value));
        else if (typeof(T) == typeof(short))
            binaryWriter.Write((short)((object)value));
        else if (typeof(T) == typeof(ushort))
            binaryWriter.Write((ushort)((object)value));
        else if (typeof(T) == typeof(int))
            binaryWriter.Write((int)((object)value));
        else if (typeof(T) == typeof(uint))
            binaryWriter.Write((uint)((object)value));
        else if (typeof(T) == typeof(long))
            binaryWriter.Write((long)((object)value));
        else if (typeof(T) == typeof(ulong))
            binaryWriter.Write((ulong)((object)value));
        else if (typeof(T) == typeof(float))
            binaryWriter.Write((float)((object)value));
        else if (typeof(T) == typeof(double))
            binaryWriter.Write((double)((object)value));
        else if (typeof(T) == typeof(decimal))
            binaryWriter.Write((decimal)((object)value));
        else
        {
            int unmanagedSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[unmanagedSize];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
                Marshal.Copy(handle.AddrOfPinnedObject(), buffer, 0, unmanagedSize);
            }
            finally
            {
                handle.Free();
            }

            binaryWriter.Write(buffer);
        }
    }

    #endregion

    /// <summary>
    /// Closes and releases the file stream and any child resources
    /// </summary>
    public void Close()
    {
        fileStream.Close();
    }
}