using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DragonLib.OWM
{
    internal static class OWMHelper
    {
        internal static Span<byte> GetBytes<T>(params T[] values) where T : struct
        {
            return MemoryMarshal.Cast<T, byte>(new Span<T>(values));
        }

        internal static Span<byte> GetString(string value, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new Span<byte>(new byte[] { 0 });
            }
            
            var text = (encoding ?? Encoding.UTF8).GetBytes(value); 
            var length = text.Length;
            var buffer = new Span<byte>(new byte[length + (length > 128 ? 0 : 1)]);
            var ptr = 1;
            buffer[0] = (byte) (length % 128);
            if (length > 128)
            {
                buffer[1] = (byte) (length / 128);
                ptr = 2;
            }
            text.CopyTo(buffer.Slice(ptr));
            return buffer;
        }
    }
}
