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
        /// Custom Type Serializers
        /// </summary>
        public Dictionary<Type, IDragonMLSerializer> TypeSerializers = new Dictionary<Type, IDragonMLSerializer>();

        /// <summary>
        /// Prefix namespace for system tags
        /// </summary>
        public string Namespace { get; set; } = "dragon";

        public static DragonMLSettings Default => new DragonMLSettings();

        public static DragonMLSettings Slim => new DragonMLSettings
        {
            UseRefId = false
        };
    }
}
