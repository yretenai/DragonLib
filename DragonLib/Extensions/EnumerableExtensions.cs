namespace DragonLib.Extensions;

public static class EnumerableExtensions {
	public static void EnsureDirectoriesExists(this IEnumerable<string> path) {
		var paths = path.Select(Path.GetFullPath).Select(Path.GetDirectoryName).Distinct().ToArray();
		foreach (var directory in paths) {
			if (string.IsNullOrEmpty(directory)) {
				continue;
			}

			Directory.CreateDirectory(directory);
		}
	}

	public static T? SafeDequeue<T>(this Queue<T> queue, T? fallback = default) => queue.Count == 0 ? fallback : queue.Dequeue();
}
