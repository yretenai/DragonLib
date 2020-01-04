using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace DragonLib.Numerics
{
    [PublicAPI]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct Vector4Short
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public short W { get; set; }
    }
}
