using System;
using System.Collections.Generic;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace DragonLib.XML
{
    public class DragonMLSettings
    {
        /// <summary>
        /// Use ids in dragon:ref tags.
        /// </summary>
        public bool UseRefId { get; set; } = true;
        
        /// <summary>
        /// Writes the ?xml header
        /// </summary>
        public bool WriteXmlHeader { get; set; } = true;

        /// <summary>
        /// Custom Type Serializers
        /// </summary>
        public Dictionary<Type, IDragonMLSerializer> TypeSerializers = new();

        /// <summary>
        /// Prefix namespace for system tags
        /// </summary>
        public string Namespace { get; set; } = "dragon";
        
        /// <summary>
        /// Dictionary of XML namespaces
        /// </summary>
        public Dictionary<string, string> Namespaces { get; set; } = new()
        {
            {
                "dragon", "https://yretenai.com/dragonml/v1"
            },
        };
        
        public static DragonMLSettings Default => new();

        public static DragonMLSettings Slim => new()
        {
            UseRefId = false,
        };
    }
}
