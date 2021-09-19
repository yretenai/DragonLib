using System.Collections.Generic;
using System.IO;

namespace DragonLib.IO {
    public static class DirectoryHelper {
        public static IEnumerable<string> FindFiles(params string[] filesOrDirectories) {
            return FindFilesWithPattern("*", filesOrDirectories);
        }

        public static IEnumerable<string> FindFilesWithPattern(string pattern, params string[] filesOrDirectories) {
            foreach (var path in filesOrDirectories)
                if (Directory.Exists(path))
                    foreach (var dirPath in Directory.EnumerateDirectories(path, pattern, SearchOption.AllDirectories))
                        yield return dirPath;
                else if (File.Exists(path)) yield return path;
        }
    }
}
