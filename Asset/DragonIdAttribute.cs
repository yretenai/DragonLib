using System;

namespace DragonLib.Asset {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DragonIdAttribute : Attribute {
        public DragonIdAttribute() { }

        public DragonIdAttribute(DragonAssetSectionId id) {
            Id = id;
        }

        public DragonAssetSectionId Id { get; } = DragonAssetSectionId.Null;
        public override object TypeId => Id;

        public override bool Equals(object? obj) {
            return obj switch {
                DragonIdAttribute attr => attr.Id.Equals(Id),
                DragonAssetSectionId id => id.Equals(Id),
                ulong number => number.Equals((ulong)Id),
                _ => base.Equals(obj)
            };
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public override bool Match(object? obj) {
            return Equals(obj);
        }

        public override string ToString() {
            return $"[DragonAssetSection {Id:G}]";
        }

        public override bool IsDefaultAttribute() {
            return Id == DragonAssetSectionId.Null;
        }
    }
}
