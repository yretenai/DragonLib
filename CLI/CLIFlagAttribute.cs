using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace DragonLib.CLI {
    [AttributeUsage(AttributeTargets.Property)]
    public class CLIFlagAttribute : Attribute {
        public CLIFlagAttribute(string flag) {
            Flag = flag;
        }

        public string Flag { get; set; }
        public string? Help { get; set; }
        public string? Category { get; set; }
        public string? Visitor { get; set; }
        public bool Hidden { get; set; }
        public Assembly? VisitorAssembly { get; set; }
        public bool IsRequired { get; set; }
        public int Positional { get; set; } = -1;
        public object? Default { get; set; }
        public string[]? ValidValues { get; set; } = Array.Empty<string>();
        public string[]? Aliases { get; set; } = Array.Empty<string>();
        public object? Extra { get; set; }

        public string[] Flags => Aliases?.Concat(new[] { Flag }).Distinct().Reverse().ToArray() ?? new[] { Flag };

        public override object TypeId => Flag;

        public override bool Equals(object? obj) {
            if (obj is CLIFlagAttribute attribute) return Equals(attribute);

            return base.Equals(obj);
        }

        protected bool Equals(CLIFlagAttribute other) {
            return base.Equals(other) && Flag == other.Flag && Help == other.Help && Category == other.Category && Visitor == other.Visitor && Hidden == other.Hidden && VisitorAssembly == other.VisitorAssembly && IsRequired == other.IsRequired && Positional == other.Positional && Default?.Equals(other.Default) == true && ValidValues?.Equals(other.ValidValues) == true && Aliases?.Equals(other.Aliases) == true;
        }

        public override string ToString() {
            return $"-{(Flag.Length > 1 ? "-" : string.Empty)}{Flag}: {Help}";
        }

        public override bool Match(object? obj) {
            if (obj is not CLIFlagAttribute attribute) return false;

            var otherFlags = attribute.Flags;
            return Flags.Any(flag => otherFlags.Contains(flag));
        }

        public override bool IsDefaultAttribute() {
            return string.IsNullOrWhiteSpace(Flag);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Attribute properties tend to not get updated")]
        public override int GetHashCode() {
            return HashCode.Combine(base.GetHashCode(), Flag, HashCode.Combine(Help, Category, Visitor, VisitorAssembly?.GetHashCode() ?? 0, Hidden), IsRequired, Positional, Default, ValidValues, Aliases);
        }
    }
}
