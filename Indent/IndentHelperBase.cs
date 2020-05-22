using System.Linq;
using JetBrains.Annotations;

namespace DragonLib.Indent
{
    [PublicAPI]
    public class IndentHelperBase
    {
        private string CachedTabs = "";
        protected int TabSize { get; set; }

        protected virtual string TabCharacter { get; } = "";

        public override string ToString() => CachedTabs;

        public static IndentHelperBase operator +(IndentHelperBase a, int b)
        {
            var c = a.Clone();
            c.TabSize += b;
            c.CachedTabs = c.Compile();
            return c;
        }

        public static IndentHelperBase operator -(IndentHelperBase a, int b)
        {
            var c = a.Clone();
            c.TabSize -= b;
            if (c.TabSize < 0) c.TabSize = 0;
            c.CachedTabs = c.Compile();
            return c;
        }

        protected virtual IndentHelperBase Clone() =>
            new IndentHelperBase
            {
                TabSize = TabSize
            };

        private string Compile() => string.Join(string.Empty, Enumerable.Repeat(TabCharacter, TabSize));
    }
}
