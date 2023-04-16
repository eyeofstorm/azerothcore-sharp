/*
 * This file is part of the AzerothCore Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Affero General Public License as published by the
 * Free Software Foundation; either version 3 of the License, or (at your
 * option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Numerics;
using System.Text;
using AzerothCore.Cryptography;

namespace AzerothCore.Utilities;

public sealed class ByteBuffer : IDisposable
{
    private byte            _bitPosition = 8;
    private byte            _bitValue;
    private BinaryWriter?   _writeStream;
    private BinaryReader?   _readStream;

    public ByteBuffer()
    {
        _writeStream = new BinaryWriter(new MemoryStream());
    }

    public ByteBuffer(byte[] data)
    {
        _readStream = new BinaryReader(new MemoryStream(data));
    }

    public void Dispose()
    {
        if (_writeStream != null)
        {
            _writeStream.Dispose();
        }

        if (_readStream != null)
        {
            _readStream.Dispose();
        }
    }

    #region Read Methods
    public sbyte ReadInt8()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadSByte();
    }

    public short ReadInt16()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadInt16();
    }

    public int ReadInt32()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadInt32();
    }

    public long ReadInt64()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadInt64();
    }

    public byte ReadUInt8()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadByte();
    }

    public ushort ReadUInt16()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadUInt16();
    }

    public uint ReadUInt32()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadUInt32();
    }

    public ulong ReadUInt64()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadUInt64();
    }

    public float ReadFloat()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadSingle();
    }

    public double ReadDouble()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadDouble();
    }

    public string ReadCString()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        StringBuilder tmpString = new();

        char tmpChar = _readStream.ReadChar();
        char tmpEndChar = Convert.ToChar(Encoding.UTF8.GetString(new byte[] { 0 }));

        while (tmpChar != tmpEndChar)
        {
            tmpString.Append(tmpChar);
            tmpChar = _readStream.ReadChar();
        }

        return tmpString.ToString();
    }

    public string ReadString(uint length)
    {
        if (length == 0)
        {
            return "";
        }

        ResetBitPos();

        return Encoding.UTF8.GetString(ReadBytes(length));
    }

    public bool ReadBool()
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadBoolean();
    }

    public byte[] ReadBytes(uint count)
    {
        if (_readStream == null)
        {
            throw new NullReferenceException(nameof(_readStream));
        }

        ResetBitPos();

        return _readStream.ReadBytes((int)count);
    }

    public void Skip(int count)
    {
        ResetBitPos();

        if (_readStream != null)
        {
            _readStream.BaseStream.Position += count;
        }
    }

    public uint ReadPackedTime()
    {
        return (uint)Time.GetUnixTimeFromPackedTime(ReadUInt32());
    }

    public Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    }

    // BitPacking
    public byte ReadBit()
    {
        if (_bitPosition == 8)
        {
            _bitValue = ReadUInt8();
            _bitPosition = 0;
        }

        int? returnValue = _bitValue;
        _bitValue = (byte)(2 * returnValue);
        ++_bitPosition;

        return (byte)(returnValue >> 7);
    }

    public bool HasBit()
    {
        if (_bitPosition == 8)
        {
            _bitValue = ReadUInt8();
            _bitPosition = 0;
        }

        int? returnValue = _bitValue;
        _bitValue = (byte)(2 * returnValue);
        ++_bitPosition;

        return Convert.ToBoolean(returnValue >> 7);
    }

    public T ReadBits<T>(int bitCount)
    {
        int value = 0;

        for (var i = bitCount - 1; i >= 0; --i)
        {
            if (HasBit())
            {
                value |= (1 << i);
            }
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }
    #endregion

    #region Write Methods
    public void WriteInt8(sbyte data)
    {
        FlushBits();

        _writeStream?.Write(data);
    }

    public void WriteInt16(short data)
    {
        FlushBits();

        _writeStream?.Write(data);
    }

    public void WriteInt32(int data)
    {
        FlushBits();

        _writeStream?.Write(data);
    }

    public void WriteInt64(long data)
    {
        FlushBits();

        _writeStream?.Write(data);
    }

    public void WriteUInt8(byte data)
    {
        FlushBits();

        _writeStream?.Write(data);
    }

    public void WriteUInt16(ushort data)
    {
        FlushBits();

        _writeStream?.Write(data);
    }

    public void WriteUInt32(uint data)
    {
        FlushBits();

        _writeStream?.Write(data);
    }

    public void WriteUInt64(ulong data)
    {
        FlushBits();

        _writeStream?.Write(data);
    }

    public void WriteFloat(float data)
    {
        FlushBits();

        _writeStream?.Write(data);
    }

    public void WriteDouble(double data)
    {
        FlushBits();
        _writeStream?.Write(data);
    }

    /// <summary>
    /// Writes a string to the packet with a null terminated (0)
    /// </summary>
    /// <param name="str"></param>
    public void WriteCString(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            WriteUInt8(0);
            return;
        }

        WriteString(str);
        WriteUInt8(0);
    }

    public void WriteString(string str)
    {
        if (str.IsEmpty())
        {
            return;
        }

        byte[] sBytes = Encoding.UTF8.GetBytes(str);
        WriteBytes(sBytes);
    }

    public void WriteBytes(byte[] data)
    {
        FlushBits();

        _writeStream?.Write(data, 0, data.Length);
    }

    public void WriteBytes(byte[] data, uint count)
    {
        FlushBits();

        _writeStream?.Write(data, 0, (int)count);
    }

    public void WriteBytes(ByteBuffer buffer)
    {
        WriteBytes(buffer.GetData());
    }

    public void WriteSrpInteger(SrpInteger srpInteger)
    {
        WriteBytes(srpInteger.ToByteArray().Reverse().ToArray());
    }

    public void WriteVector4(Vector4 pos)
    {
        WriteFloat(pos.X);
        WriteFloat(pos.Y);
        WriteFloat(pos.Z);
        WriteFloat(pos.W);
    }

    public void WriteVector3(Vector3 pos)
    {
        WriteFloat(pos.X);
        WriteFloat(pos.Y);
        WriteFloat(pos.Z);
    }

    public void WriteVector2(Vector2 pos)
    {
        WriteFloat(pos.X);
        WriteFloat(pos.Y);
    }

    public void WritePackXYZ(Vector3 pos)
    {
        uint packed = 0;
        packed |= ((uint)(pos.X / 0.25f) & 0x7FF);
        packed |= ((uint)(pos.Y / 0.25f) & 0x7FF) << 11;
        packed |= ((uint)(pos.Z / 0.25f) & 0x3FF) << 22;
        WriteUInt32(packed);
    }

    public bool WriteBit(bool bit)
    {
        --_bitPosition;

        if (bit)
        {
            _bitValue |= (byte)(1 << _bitPosition);
        }

        if (_bitPosition == 0)
        {
            _writeStream?.Write(_bitValue);

            _bitPosition = 8;
            _bitValue = 0;
        }

        return bit;
    }

    public void WriteBits(object bit, int count)
    {
        for (int i = count - 1; i >= 0; --i)
        {
            WriteBit(((Convert.ToUInt32(bit) >> i) & 1) != 0);
        }
    }

    public void WritePackedTime(long time)
    {
        WriteUInt32(Time.GetPackedTimeFromUnixTime(time));
    }

    public void WritePackedTime()
    {
        WriteUInt32(Time.GetPackedTimeFromDateTime(DateTime.Now));
    }
    #endregion

    public bool HasUnfinishedBitPack()
    {
        return _bitPosition != 8;
    }

    public void FlushBits()
    {
        if (_bitPosition == 8)
        {
            return;
        }

        _writeStream?.Write(_bitValue);

        _bitValue = 0;
        _bitPosition = 8;
    }

    public void ResetBitPos()
    {
        if (_bitPosition > 7)
        {
            return;
        }

        _bitPosition = 8;
        _bitValue = 0;
    }

    public byte[] GetData()
    {
        Stream stream = GetCurrentStream();

        var data = new byte[stream.Length];

        long pos = stream.Position;
        stream.Seek(0, SeekOrigin.Begin);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)stream.ReadByte();
        }

        stream.Seek(pos, SeekOrigin.Begin);

        return data;
    }

    public uint GetSize()
    {
        return (uint)GetCurrentStream().Length;
    }

    public Stream GetCurrentStream()
    {
        if (_writeStream != null)
        {
            return _writeStream.BaseStream;
        }
        else if (_readStream != null)
        {
            return _readStream.BaseStream;
        }
        else
        {
            throw new NullReferenceException(nameof(_writeStream) + " and " + nameof(_readStream));
        }
    }

    public void Clear()
    {
        _bitPosition = 8;
        _bitValue = 0;

        _writeStream = new BinaryWriter(new MemoryStream());
    }
}
