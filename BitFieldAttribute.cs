using System;
using JetBrains.Annotations;

namespace DragonLib
{
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public class BitFieldAttribute : Attribute
    {
        public BitFieldAttribute(int length)
        {
            Length = length;
        }

        public int Length { get; }
    }
}
