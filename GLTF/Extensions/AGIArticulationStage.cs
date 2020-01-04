using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class AGIArticulationStage : GLTFProperty
    {
        public string? Name { get; set; }
        public AGIArticulationStageType Type { get; set; }
        public float MinimumValue { get; set; }
        public float MaximumValue { get; set; }
        public float InitialValue { get; set; }
    }
}
