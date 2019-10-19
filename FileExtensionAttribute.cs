using System;

namespace DragonLib
{
    public class FileExtensionAttribute : Attribute
    {
        public string Extension { get; set; }
        
        public FileExtensionAttribute(string extension)
        {
            Extension = extension;
        }
    }
}
