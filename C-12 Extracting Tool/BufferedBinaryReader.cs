using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public static class Utilities
{
    
}

public class BufferedBinaryReader : IDisposable
{
    public long Position { get { return bufferOffset; } }
    public long Length { get { return bufferSize; } }

    private readonly Stream stream;
    private byte[] buffer;
    private int bufferSize;
    private int bufferOffset;
    private int numBufferedBytes;

    public BufferedBinaryReader(Stream stream, int bufferSize)
    {
        this.stream = stream;
        this.bufferSize = bufferSize;
        buffer = new byte[bufferSize];
        bufferOffset = bufferSize;
    }

    public BufferedBinaryReader(byte[] buffer)
    {
        if (buffer != null)
        {
            bufferSize = buffer.Length;
            this.buffer = buffer;
        }
        else
        {
            bufferSize = 0;
            this.buffer = null;
        }
    }

    public int NumBytesAvailable { get { return Math.Max(0, numBufferedBytes - bufferOffset); } }

    public bool FillBuffer()
    {
        var numBytesUnread = bufferSize - bufferOffset;
        var numBytesToRead = bufferSize - numBytesUnread;
        bufferOffset = 0;
        numBufferedBytes = numBytesUnread;

        if (numBytesUnread > 0)
        {
            Buffer.BlockCopy(buffer, numBytesToRead, buffer, 0, numBytesUnread);
        }

        while (numBytesToRead > 0)
        {
            var numBytesRead = stream.Read(buffer, numBytesUnread, numBytesToRead);
            if (numBytesRead == 0)
            {
                return false;
            }
            numBufferedBytes += numBytesRead;
            numBytesToRead -= numBytesRead;
            numBytesUnread += numBytesRead;
        }

        return true;
    }

    public void ChangeBuffer(byte[] buffer)
    {
        bufferSize = buffer.Length;
        this.buffer = buffer;
        bufferOffset = 0;
    }

    public void ChangeBuffer(BufferedBinaryReader reader)
    {
        bufferSize = reader.bufferSize;
        buffer = reader.buffer;
        bufferOffset = reader.bufferOffset;
    }

    public byte[] GetBuffer()
    {
        return buffer;
    }

    public byte ReadByte()
    {
        byte val = (byte)(int)buffer[bufferOffset];
        bufferOffset++;

        return val;
    }

    public byte ReadByte(int offset)
    {
        byte val = (byte)(int)buffer[bufferOffset + offset];

        return val;
    }

    public sbyte ReadSByte()
    {
        sbyte val = (sbyte)(int)buffer[bufferOffset];
        bufferOffset++;

        return val;
    }

    public sbyte ReadSByte(int offset)
    {
        sbyte val = (sbyte)(int)buffer[bufferOffset + offset];

        return val;
    }

    public byte[] ReadBytes(int length)
    {
        byte[] bytes = new byte[length];

        for (int i = 0; i < length; i++)
            bytes[i] = ReadByte();

        return bytes;
    }

    public short ReadInt16()
    {
        var val = (short)((int)buffer[bufferOffset] | (int)buffer[bufferOffset + 1] << 8);
        bufferOffset += 2;

        return val;
    }

    public short ReadInt16(int offset)
    {
        var val = (short)((int)buffer[bufferOffset + offset] | (int)buffer[bufferOffset + offset + 1] << 8);

        return val;
    }

    public ushort ReadUInt16()
    {
        var val = (ushort)((int)buffer[bufferOffset] | (int)buffer[bufferOffset + 1] << 8);
        bufferOffset += 2;

        return val;
    }

    public ushort ReadUInt16(int offset)
    {
        var val = (ushort)((int)buffer[bufferOffset + offset] | (int)buffer[bufferOffset + offset + 1] << 8);

        return val;
    }

    public int ReadInt32()
    {
        var val = (int)((int)buffer[bufferOffset] | (int)buffer[bufferOffset + 1] << 8 |
                        (int)buffer[bufferOffset + 2] << 16 | (int)buffer[bufferOffset + 3] << 24);
        bufferOffset += 4;

        return val;
    }

    public int ReadInt32(int offset)
    {
        var val = (int)((int)buffer[bufferOffset + offset] | (int)buffer[bufferOffset + offset + 1] << 8 |
                        (int)buffer[bufferOffset + offset + 2] << 16 | (int)buffer[bufferOffset + offset + 3] << 24);
        return val;
    }

    public uint ReadUInt32()
    {
        var val = (uint)((int)buffer[bufferOffset] | (int)buffer[bufferOffset + 1] << 8 |
                        (int)buffer[bufferOffset + 2] << 16 | (int)buffer[bufferOffset + 3] << 24);
        bufferOffset += 4;

        return val;
    }

    public uint ReadUInt32(int offset)
    {
        var val = (uint)((int)buffer[bufferOffset + offset] | (int)buffer[bufferOffset + offset + 1] << 8 |
                        (int)buffer[bufferOffset + offset + 2] << 16 | (int)buffer[bufferOffset + offset + 3] << 24);
        return val;
    }

    public string ReadUTF8(int offset)
    {
        List<byte> strBytes = new List<byte>();
        int b;

        while ((b = ReadByte(offset)) != 0x00)
        {
            strBytes.Add((byte)b);
            offset++;
        }

        return Encoding.UTF8.GetString(strBytes.ToArray());
    }

    public void WriteByte(int offset, byte value)
    {
        buffer[bufferOffset + offset] = value;
    }

    public void WriteInt16(int offset, short value)
    {
        buffer[bufferOffset + offset] = (byte)value;
        buffer[bufferOffset + offset + 1] = (byte)(value >> 8);
    }

    public void WriteInt32(int offset, int value)
    {
        buffer[bufferOffset + offset] = (byte)value;
        buffer[bufferOffset + offset + 1] = (byte)(value >> 8);
        buffer[bufferOffset + offset + 2] = (byte)(value >> 16);
        buffer[bufferOffset + offset + 3] = (byte)(value >> 24);
    }

    public void Write(byte value)
    {
        buffer[bufferOffset] = value;
        bufferOffset++;
    }

    public void Write(ushort value)
    {
        buffer[bufferOffset] = (byte)value;
        buffer[bufferOffset + 1] = (byte)(value >> 8);
        bufferOffset += 2;
    }

    //ASM Names
    public byte LB(int offset)
    {
        byte val = (byte)(int)buffer[bufferOffset + offset];

        return val;
    }

    public short LH(int offset)
    {
        var val = (short)((int)buffer[bufferOffset + offset] | (int)buffer[bufferOffset + offset + 1] << 8);

        return val;
    }

    public ushort LHU(int offset)
    {
        var val = (ushort)((int)buffer[bufferOffset + offset] | (int)buffer[bufferOffset + offset + 1] << 8);

        return val;
    }

    public int LW(int offset)
    {
        var val = (int)((int)buffer[bufferOffset + offset] | (int)buffer[bufferOffset + offset + 1] << 8 |
                        (int)buffer[bufferOffset + offset + 2] << 16 | (int)buffer[bufferOffset + offset + 3] << 24);
        return val;
    }

    public void SB(int offset, byte value)
    {
        buffer[bufferOffset + offset] = value;
    }

    public void SH(int offset, short value)
    {
        buffer[bufferOffset + offset] = (byte)value;
        buffer[bufferOffset + offset + 1] = (byte)(value >> 8);
    }

    public void SW(int offset, int value)
    {
        buffer[bufferOffset + offset] = (byte)value;
        buffer[bufferOffset + offset + 1] = (byte)(value >> 8);
        buffer[bufferOffset + offset + 2] = (byte)(value >> 16);
        buffer[bufferOffset + offset + 3] = (byte)(value >> 24);
    }
    //

    public void Dispose()
    {
        if (stream != null)
            stream.Close();
    }

    public void Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                bufferOffset = (int)offset;
                break;
            case SeekOrigin.Current:
                bufferOffset += (int)offset;
                break;
            case SeekOrigin.End:
                bufferOffset = bufferSize - (int)offset;
                break;
        }
    }
}
