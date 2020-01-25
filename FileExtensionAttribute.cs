using System;
using JetBrains.Annotations;

namespace DragonLib
{
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Field)]
    public class FileExtensionAttribute : Attribute
    {
        public FileExtensionAttribute(string extension) => Extension = extension;
        public string Extension { get; set; }
    }
}
