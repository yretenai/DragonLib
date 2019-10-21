using System;

namespace DragonLib
{
    public class FileExtensionAttribute : Attribute
    {
        public FileExtensionAttribute(string extension) => Extension = extension;
        public string Extension { get; set; }
    }
}
