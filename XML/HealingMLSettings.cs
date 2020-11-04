using System;
using System.Collections.Generic;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace DragonLib.XML
{
    public class HealingMLSettings
    {
        /// <summary>
        /// Use ids in dragon:ref tags.
        /// </summary>
        public bool UseRefId { get; set; } = true;

        /// <summary>
        /// Custom Type Serializers
        /// </summary>
        public Dictionary<Type, IHMLSerializer> TypeSerializers = new Dictionary<Type, IHMLSerializer>();

        /// <summary>
        /// Prefix namespace for system tags
        /// </summary>
        public string Namespace { get; set; } = "dragon";

        public static HealingMLSettings Default => new HealingMLSettings();

        public static HealingMLSettings Slim => new HealingMLSettings
        {
            UseRefId = false
        };
    }
}
