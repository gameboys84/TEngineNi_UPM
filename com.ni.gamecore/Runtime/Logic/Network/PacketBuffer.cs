using System;
using System.IO;
using System.Text;

namespace GameCore
{
    public sealed class PacketWriter
    {
        private readonly MemoryStream stream = new MemoryStream();
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        public void WriteByte(byte value) => stream.WriteByte(value);
        public void WriteBoolean(bool value) => WriteByte(value ? (byte)1 : (byte)0);
        public void WriteUInt16(ushort value) { WriteByte((byte)(value >> 8)); WriteByte((byte)value); }
        public void WriteInt16(short value) => WriteUInt16(unchecked((ushort)value));
        public void WriteUInt32(uint value) { WriteUInt16((ushort)(value >> 16)); WriteUInt16((ushort)value); }
        public void WriteInt32(int value) => WriteUInt32(unchecked((uint)value));
        public void WriteUInt64(ulong value) { WriteUInt32((uint)(value >> 32)); WriteUInt32((uint)value); }
        public void WriteInt64(long value) => WriteUInt64(unchecked((ulong)value));
        public void WriteRaw(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            stream.Write(value, 0, value.Length);
        }
        public void WriteBytes16(byte[] value)
        {
            if (value == null || value.Length > ushort.MaxValue) throw new ArgumentException("Byte field exceeds its length limit.");
            WriteUInt16((ushort)value.Length); WriteRaw(value);
        }
        public void WriteBytes32(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            WriteUInt32((uint)value.Length); WriteRaw(value);
        }
        public void WriteString(string value) => WriteBytes16(Utf8.GetBytes(value ?? throw new ArgumentNullException(nameof(value))));
        public byte[] ToArray() => stream.ToArray();
    }
    public sealed class PacketReader
    {
        private readonly byte[] data;
        private int position;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        public PacketReader(byte[] data) { this.data = data ?? throw new ArgumentNullException(nameof(data)); }
        public int Remaining => data.Length - position;
        private void Require(int count) { if (count < 0 || count > Remaining) throw new InvalidDataException("Truncated packet field."); }
        public byte ReadByte() { Require(1); return data[position++]; }
        public bool ReadBoolean()
        {
            byte value = ReadByte();
            if (value > 1) throw new InvalidDataException("Invalid boolean field.");
            return value == 1;
        }
        public ushort ReadUInt16() { Require(2); return (ushort)((ReadByte() << 8) | ReadByte()); }
        public short ReadInt16() => unchecked((short)ReadUInt16());
        public uint ReadUInt32() { Require(4); return ((uint)ReadUInt16() << 16) | ReadUInt16(); }
        public int ReadInt32() => unchecked((int)ReadUInt32());
        public ulong ReadUInt64() { Require(8); return ((ulong)ReadUInt32() << 32) | ReadUInt32(); }
        public long ReadInt64() => unchecked((long)ReadUInt64());
        public byte[] ReadRaw(int count)
        {
            Require(count); var result = new byte[count]; Buffer.BlockCopy(data, position, result, 0, count); position += count; return result;
        }
        public byte[] ReadBytes16() => ReadRaw(ReadUInt16());
        public byte[] ReadBytes32()
        {
            uint count = ReadUInt32();
            if (count > int.MaxValue) throw new InvalidDataException("Byte field exceeds its length limit.");
            return ReadRaw((int)count);
        }
        public string ReadString() => Utf8.GetString(ReadBytes16());
        public void RequireEnd() { if (Remaining != 0) throw new InvalidDataException("Unexpected trailing packet fields."); }
    }
}
