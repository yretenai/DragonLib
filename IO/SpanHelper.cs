using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Half = DragonLib.Numerics.Half;

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

        public static Half ReadLittleHalf(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(cursor));
            cursor += sizeof(ushort);
            return Half.ToHalf(value);
        }

        public static Half ReadBigHalf(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(cursor));
            cursor += sizeof(ushort);
            return Half.ToHalf(value);
        }

        public static float ReadLittleSingle(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(cursor));
            cursor += sizeof(float);
            return value;
        }

        public static float ReadBigSingle(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadSingleBigEndian(buffer.Slice(cursor));
            cursor += sizeof(float);
            return value;
        }

        public static double ReadLittleDouble(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadDoubleLittleEndian(buffer.Slice(cursor));
            cursor += sizeof(double);
            return value;
        }

        public static double ReadBigDouble(Span<byte> buffer, ref int cursor)
        {
            var value = BinaryPrimitives.ReadDoubleBigEndian(buffer.Slice(cursor));
            cursor += sizeof(double);
            return value;
        }

        public static decimal ReadLittleDecimal(Span<byte> buffer, ref int cursor)
        {
            var a = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(cursor));
            var b = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(cursor + sizeof(int)));
            var c = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(cursor + sizeof(int) * 2));
            var d = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(cursor + sizeof(int) * 3));
            cursor += sizeof(int) * 4;
            return new decimal(new [] { a, b, c, d });
        }

        public static decimal ReadBigDecimal(Span<byte> buffer, ref int cursor)
        {
            var a = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(cursor));
            var b = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(cursor + sizeof(int)));
            var c = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(cursor + sizeof(int) * 2));
            var d = BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(cursor + sizeof(int) * 3));
            cursor += sizeof(int) * 4;
            return new decimal(new [] { a, b, c, d });
        }

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

        public static unsafe Array ReadStructArray(Span<byte> buffer, Type T, int count, ref int cursor)
        {
            if (count <= 0) return Array.CreateInstance(T, 0);
            var size = SizeHelper.SizeOf(T);
            var arraySize = count * size;
            var refptr = 0;
            var array = Array.CreateInstance(T, count);
            for (var i = 0; i < count; ++i)
            {
                fixed (byte* pin = &buffer.Slice(cursor + refptr).GetPinnableReference())
                {
                    array.SetValue(Marshal.PtrToStructure(new IntPtr(pin), T), i);
                    refptr += size;
                }
            }

            cursor += arraySize;
            return array;
        }

        public static unsafe object? ReadStruct(Span<byte> buffer, Type T, ref int cursor)
        {
            object? value;
            fixed (byte* pin = &buffer.Slice(cursor).GetPinnableReference()) value = Marshal.PtrToStructure(new IntPtr(pin), T);

            cursor += SizeHelper.SizeOf(T);
            return value;
        }

        public static void WriteByte(ref Memory<byte> buffer, byte value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(byte));
            buffer.Span[cursor++] = value;
        }

        public static void WriteSByte(ref Memory<byte> buffer, sbyte value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(sbyte));
            buffer.Span[cursor++] = (byte) value;
        }

        public static void WriteLittleShort(ref Memory<byte> buffer, short value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(short));
            BinaryPrimitives.WriteInt16LittleEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(short);
        }

        public static void WriteBigShort(ref Memory<byte> buffer, short value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(short));
            BinaryPrimitives.WriteInt16BigEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(short);
        }

        public static void WriteLittleUShort(ref Memory<byte> buffer, ushort value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(ushort));
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(short);
        }

        public static void WriteBigUShort(ref Memory<byte> buffer, ushort value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(ushort));
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(short);
        }

        public static void WriteLittleInt(ref Memory<byte> buffer, int value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(int));
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(int);
        }

        public static void WriteBigInt(ref Memory<byte> buffer, int value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(int));
            BinaryPrimitives.WriteInt32BigEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(int);
        }

        public static void WriteLittleUInt(ref Memory<byte> buffer, uint value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(uint));
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(int);
        }

        public static void WriteBigUInt(ref Memory<byte> buffer, uint value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(uint));
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(int);
        }

        public static void WriteLittleLong(ref Memory<byte> buffer, long value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(long));
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(long);
        }

        public static void WriteBigLong(ref Memory<byte> buffer, long value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(long));
            BinaryPrimitives.WriteInt64BigEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(long);
        }


        public static void WriteLittleULong(ref Memory<byte> buffer, ulong value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(ulong));
            BinaryPrimitives.WriteUInt64LittleEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(long);
        }

        public static void WriteBigULong(ref Memory<byte> buffer, ulong value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(ulong));
            BinaryPrimitives.WriteUInt64BigEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(long);
        }

        public static void WriteLittleHalf(ref Memory<byte> buffer, Half value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(short));
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Span.Slice(cursor), value.ToValue());
            cursor += sizeof(short);
        }

        public static void WriteBigSingle(ref Memory<byte> buffer, Half value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(short));
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Span.Slice(cursor), value.ToValue());
            cursor += sizeof(short);
        }

        public static void WriteLittleSingle(ref Memory<byte> buffer, float value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(float));
            BinaryPrimitives.WriteSingleLittleEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(float);
        }

        public static void WriteBigSingle(ref Memory<byte> buffer, float value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(float));
            BinaryPrimitives.WriteSingleBigEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(float);
        }
        
        public static void WriteLittleDouble(ref Memory<byte> buffer, double value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(double));
            BinaryPrimitives.WriteDoubleLittleEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(double);
        }

        public static void WriteBigDouble(ref Memory<byte> buffer, double value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(double));
            BinaryPrimitives.WriteDoubleBigEndian(buffer.Span.Slice(cursor), value);
            cursor += sizeof(double);
        }

        public static void WriteLittleDecimal(ref Memory<byte> buffer, decimal value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(int) * 4);
            var bits = Decimal.GetBits(value);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Span.Slice(cursor), bits[0]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Span.Slice(cursor + sizeof(int)), bits[1]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Span.Slice(cursor + sizeof(int) * 2), bits[2]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Span.Slice(cursor + sizeof(int) * 3), bits[3]);
            cursor += sizeof(int) * 4;
        }

        public static void WriteBigDecimal(ref Memory<byte> buffer, decimal value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + sizeof(int) * 4);
            var bits = Decimal.GetBits(value);
            BinaryPrimitives.WriteInt32BigEndian(buffer.Span.Slice(cursor), bits[0]);
            BinaryPrimitives.WriteInt32BigEndian(buffer.Span.Slice(cursor + sizeof(int)), bits[1]);
            BinaryPrimitives.WriteInt32BigEndian(buffer.Span.Slice(cursor + sizeof(int) * 2), bits[2]);
            BinaryPrimitives.WriteInt32BigEndian(buffer.Span.Slice(cursor + sizeof(int) * 3), bits[3]);
            cursor += sizeof(int) * 4;
        }

        public static void WriteStructArray<T>(ref Memory<byte> buffer, T[] value, ref int cursor) where T : struct
        {
            if (value.Length <= 0) return;
            EnsureSpace(ref buffer, cursor + SizeHelper.SizeOf<T>() * value.Length);
            var data = MemoryMarshal.Cast<T, byte>(value);
            data.CopyTo(buffer.Span.Slice(cursor));
            cursor += data.Length;
        }

        public static void WriteStruct<T>(ref Memory<byte> buffer, T value, ref int cursor) where T : struct
        {
            EnsureSpace(ref buffer, cursor + SizeHelper.SizeOf<T>());
            MemoryMarshal.Write(buffer.Span, ref value);
            cursor += SizeHelper.SizeOf<T>();
        }

        [DllImport("kernel32.dll")]
        private static extern unsafe void RtlCopyMemory([Out] void* dest, [In] void* src, [In] int len);

        public static unsafe void WriteStructArray(ref Memory<byte> buffer, Type T, Array value, ref int cursor)
        {
            if (value.Length <= 0) return;
            EnsureSpace(ref buffer, cursor + SizeHelper.SizeOf(T) * value.Length);
            var size = SizeHelper.SizeOf(T);
            var ptr = IntPtr.Zero;
            for (var i = 0; i < value.Length; ++i)
            {
                var entry = value.GetValue(i);
                if (entry == null) continue;
                Marshal.StructureToPtr(entry, ptr, false);
                using var destPtr = buffer.Slice(cursor + size * i).Pin();
                RtlCopyMemory(destPtr.Pointer, ptr.ToPointer(), size);
            }

            cursor += size * value.Length;
        }

        public static unsafe void WriteStruct(ref Memory<byte> buffer, Type T, object value, ref int cursor)
        {
            EnsureSpace(ref buffer, cursor + SizeHelper.SizeOf(T));
            using var destPtr = buffer.Slice(cursor).Pin();
            var ptr = IntPtr.Zero;
            Marshal.StructureToPtr(value, ptr, false);
            RtlCopyMemory(destPtr.Pointer, ptr.ToPointer(), SizeHelper.SizeOf(T));
            cursor += SizeHelper.SizeOf(T);
        }

        public static void EnsureSpace(ref Memory<byte> buffer, int size)
        {
            if (buffer.Length < size)
            {
                var tmp = new Memory<byte>(new byte[size]);
                buffer.CopyTo(tmp);
                buffer = tmp;
            }
        }
    }
}
