using System;

namespace DragonLib
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FileExtensionAttribute : Attribute
    {
        public FileExtensionAttribute(string extension) => Extension = extension;
        public string Extension { get; set; }
    }
}
