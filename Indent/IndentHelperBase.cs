using System;
using System.Linq;

namespace DragonLib.Indent {
    public class IndentHelperBase {
        internal string? CachedTabs;
        protected int TabSize { get; set; }

        protected virtual string TabCharacter => "";

        protected bool Equals(IndentHelperBase other) {
            return TabSize == other.TabSize && TabCharacter == other.TabCharacter;
        }

        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj)) return false;

            if (ReferenceEquals(this, obj)) return true;

            return obj.GetType() == GetType() && Equals((IndentHelperBase)obj);
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() {
            return HashCode.Combine(TabSize, TabCharacter);
        }

        public override string ToString() {
            return CachedTabs ?? "";
        }

        public static IndentHelperBase operator +(IndentHelperBase a, int b) {
            var c = a.Clone();
            c.TabSize += b;
            c.CachedTabs = c.Compile();
            return c;
        }

        public static IndentHelperBase operator -(IndentHelperBase a, int b) {
            var c = a.Clone();
            c.TabSize -= b;
            if (c.TabSize < 0) c.TabSize = 0;

            c.CachedTabs = c.Compile();
            return c;
        }

        public static bool operator ==(IndentHelperBase a, int c) {
            return a.TabSize == c;
        }

        public static bool operator !=(IndentHelperBase a, int c) {
            return a.TabSize != c;
        }

        protected virtual IndentHelperBase Clone() {
            return new IndentHelperBase {
                TabSize = TabSize
            };
        }

        public string Compile() {
            return string.Join(string.Empty, Enumerable.Repeat(TabCharacter, TabSize));
        }
    }
}
