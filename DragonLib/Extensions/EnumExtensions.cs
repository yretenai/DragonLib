using System.Numerics;

namespace DragonLib.Extensions;

public static class EnumExtensions {
	extension<T>(T value) where T : Enum {
		public T[] Flags {
			get {
				var v = Convert.ToUInt64(value);
				if (v == 0) {
					return [];
				}

				var t = new T[BitOperations.PopCount(v)];

				var i = 0;
				for (var j = 0; j < 64; ++j) {
					if ((v & (1UL << j)) != 0) {
						t[i++] = (T) Enum.ToObject(typeof(T), 1UL << j);
					}
				}

				return t;
			}
		}
	}
}
