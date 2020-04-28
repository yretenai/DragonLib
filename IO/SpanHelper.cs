using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace DragonLib.IO
{
    [PublicAPI]
    public static class SpanHelper
    {
        public static byte ReadByte(Span<byte> buffer, ref int cursor) => buffer[cursor++];

        public static sbyte ReadSByte(Span<byte> buffer, ref int cursor) => (sbyte) buffer[cursor++];

        public static short ReadLittleShort(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadInt16LittleEndian(buffer.Slice(cursor));
            cursor += sizeof(short);
            return value;
        }

        public static short ReadBigShort(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadInt16BigEndian(buffer.Slice(cursor));
            cursor += sizeof(short);
            return value;
        }

        public static ushort ReadLittleUShort(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(cursor));
            cursor += sizeof(ushort);
            return value;
        }

        public static ushort ReadBigUShort(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(cursor));
            cursor += sizeof(ushort);
            return value;
        }

        public static int ReadLittleInt(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(cursor));
            cursor += sizeof(int);
            return value;
        }

        public static int ReadBigInt(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(cursor));
            cursor += sizeof(int);
            return value;
        }

        public static uint ReadLittleUInt(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(cursor));
            cursor += sizeof(uint);
            return value;
        }

        public static uint ReadBigUInt(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(cursor));
            cursor += sizeof(uint);
            return value;
        }

        public static long ReadLittleLong(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadInt64LittleEndian(buffer.Slice(cursor));
            cursor += sizeof(long);
            return value;
        }

        public static long ReadBigLong(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadInt64BigEndian(buffer.Slice(cursor));
            cursor += sizeof(long);
            return value;
        }

        public static ulong ReadLittleULong(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadUInt64LittleEndian(buffer.Slice(cursor));
            cursor += sizeof(ulong);
            return value;
        }

        public static ulong ReadBigULong(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadUInt64BigEndian(buffer.Slice(cursor));
            cursor += sizeof(ulong);
            return value;
        }

        public static float ReadLittleSingle(Span<byte> buffer, ref int cursor)
        {
            var value = buffer.Slice(cursor, sizeof(float));
            cursor += sizeof(float);
            return BitConverter.ToSingle(value);
        }

        public static float ReadBigSingle(Span<byte> buffer, ref int cursor) => throw new NotImplementedException();

        public static double ReadLittleDouble(Span<byte> buffer, ref int cursor)
        {
            var value = buffer.Slice(cursor, sizeof(double));
            cursor += sizeof(double);
            return BitConverter.ToSingle(value);
        }

        public static double ReadBigDouble(Span<byte> buffer, ref int cursor) => throw new NotImplementedException();

        public static T[] ReadStructArray<T>(Span<byte> buffer, int count, ref int cursor) where T : struct
        {
            if (count <= 0) return Array.Empty<T>();
            var size = count * SizeHelper.SizeOf<T>();
            var value = MemoryMarshal.Cast<byte, T>(buffer.Slice(cursor, size));
            cursor += size;
            return value.ToArray();
        }

        public static T ReadStruct<T>(Span<byte> buffer, ref int cursor) where T : struct
        {
            var value = MemoryMarshal.Read<T>(buffer.Slice(cursor));
            cursor += SizeHelper.SizeOf<T>();
            return value;
        }

        public static void WriteByte(Span<byte> buffer, byte value, ref int cursor) => buffer[cursor++] = value;

        public static void WriteSByte(Span<byte> buffer, sbyte value, ref int cursor) => buffer[cursor++] = (byte) value;

        public static void WriteLittleShort(Span<byte> buffer, short value, ref int cursor)
        {
            BinaryPrimitives.WriteInt16LittleEndian(buffer.Slice(cursor), value);
            cursor += sizeof(short);
        }

        public static void WriteBigShort(Span<byte> buffer, short value, ref int cursor)
        {
            BinaryPrimitives.WriteInt16BigEndian(buffer.Slice(cursor), value);
            cursor += sizeof(short);
        }


        public static void WriteLittleUShort(Span<byte> buffer, ushort value, ref int cursor)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(cursor), value);
            cursor += sizeof(short);
        }

        public static void WriteBigUShort(Span<byte> buffer, ushort value, ref int cursor)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(cursor), value);
            cursor += sizeof(short);
        }

        public static void WriteLittleInt(Span<byte> buffer, int value, ref int cursor)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(cursor), value);
            cursor += sizeof(int);
        }

        public static void WriteBigInt(Span<byte> buffer, int value, ref int cursor)
        {
            BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(cursor), value);
            cursor += sizeof(int);
        }

        public static void WriteLittleUInt(Span<byte> buffer, uint value, ref int cursor)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(cursor), value);
            cursor += sizeof(int);
        }

        public static void WriteBigUInt(Span<byte> buffer, uint value, ref int cursor)
        {
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(cursor), value);
            cursor += sizeof(int);
        }

        public static void WriteLittleLong(Span<byte> buffer, long value, ref int cursor)
        {
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(cursor), value);
            cursor += sizeof(long);
        }

        public static void WriteBigLong(Span<byte> buffer, long value, ref int cursor)
        {
            BinaryPrimitives.WriteInt64BigEndian(buffer.Slice(cursor), value);
            cursor += sizeof(long);
        }


        public static void WriteLittleULong(Span<byte> buffer, ulong value, ref int cursor)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer.Slice(cursor), value);
            cursor += sizeof(long);
        }

        public static void WriteBigULong(Span<byte> buffer, ulong value, ref int cursor)
        {
            BinaryPrimitives.WriteUInt64BigEndian(buffer.Slice(cursor), value);
            cursor += sizeof(long);
        }

        public static void WriteLittleSingle(Span<byte> buffer, float value, ref int cursor)
        {
            var bytes = BitConverter.GetBytes(value);
            bytes.CopyTo(buffer.Slice(cursor));
            cursor += sizeof(float);
        }

        public static void WriteBigSingle(Span<byte> buffer, float value, ref int cursor) => throw new NotImplementedException();

        public static void WriteLittleDouble(Span<byte> buffer, double value, ref int cursor)
        {
            var bytes = BitConverter.GetBytes(value);
            bytes.CopyTo(buffer.Slice(cursor));
            cursor += sizeof(float);
        }

        public static void WriteBigDouble(Span<byte> buffer, double value, ref int cursor) => throw new NotImplementedException();

        public static void WriteStructArray<T>(Span<byte> buffer, T[] value, ref int cursor) where T : struct
        {
            if (value.Length <= 0) return;
            var data = MemoryMarshal.Cast<T, byte>(value);
            data.CopyTo(buffer.Slice(cursor));
            cursor += data.Length;
        }

        public static void WriteStruct<T>(Span<byte> buffer, T value, ref int cursor) where T : struct
        {
            MemoryMarshal.Write(buffer, ref value);
            cursor += SizeHelper.SizeOf<T>();
        }
    }
}
