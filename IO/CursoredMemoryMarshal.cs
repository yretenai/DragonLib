using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DragonLib.IO {
    public class CursoredMemoryMarshal {
        public Memory<byte> Buffer;
        public int Cursor;

        public CursoredMemoryMarshal(Memory<byte> buffer, int cursor = 0) {
            Buffer = buffer;
            Cursor = cursor;
        }

        private void Align(int n) {
            if (Cursor < n) {
                Cursor = n;
                return;
            }

            if (Cursor % n == 0) return;

            Cursor += n - Cursor % n;
        }

        public T Read<T>(int alignment = 0) where T : struct {
            var value = MemoryMarshal.Read<T>(Buffer[Cursor..].Span);
            Cursor += Unsafe.SizeOf<T>();
            if (alignment > 0) Align(alignment);

            return value;
        }

        public ReadOnlySpan<T> Cast<T>(int count, int alignment = 0) where T : struct {
            var size = Unsafe.SizeOf<T>() * count;
            var value = MemoryMarshal.Cast<byte, T>(Buffer[Cursor..(Cursor + size)].Span);
            Cursor += size;
            if (alignment > 0) Align(alignment);

            return value;
        }

        public ReadOnlySpan<T> CastInter<T>(int count, int alignment = 0) where T : struct {
            Span<T> array = new T[count];
            for (var i = 0; i < count; ++i) array[i] = Read<T>(alignment);

            return array;
        }

        public void Paste(Memory<byte> buffer) {
            SpanHelper.EnsureSpace(ref Buffer, Cursor + buffer.Length);
            buffer.CopyTo(Buffer[Cursor..]);
            Cursor += buffer.Length;
        }

        public void Paste(Span<byte> buffer) {
            SpanHelper.EnsureSpace(ref Buffer, Cursor + buffer.Length);
            buffer.CopyTo(Buffer[Cursor..].Span);
            Cursor += buffer.Length;
        }

        public Memory<byte> Copy(int size) {
            var slice = Buffer.Slice(Cursor, size);
            Cursor += size;
            return slice;
        }

        public T Peek<T>() where T : struct {
            return MemoryMarshal.Read<T>(Buffer[Cursor..].Span);
        }

        public string ReadString(int size) {
            if (size <= 0) return string.Empty;

            var slice = Buffer.Slice(Cursor, size);
            Cursor += size;
            return Encoding.UTF8.GetString(slice.Span);
        }

        public string ReadString() {
            return ReadString(Read<int>());
        }

        // TODO: Map SpanHelper methods to CursoredMemoryMarshal.
    }
}
