namespace DragonLib;

public static class Singleton<T> where T : class, new() {
	public static T Instance {
		get {
			if ((T?) field is null) {
				field = new T();
			}

			return field;
		}
		set;
	} = null!;
}
