using System;

namespace DragonLib
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BitFieldAttribute : Attribute
    {
        public BitFieldAttribute(int length) => Length = length;

        public int Length { get; }
    }
}
