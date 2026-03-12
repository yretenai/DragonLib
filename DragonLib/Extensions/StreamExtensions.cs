namespace DragonLib.Extensions;

public static class StreamExtensions {
	extension(Stream stream) {
		public void Align(long n) {
			if (stream.CanWrite && stream.Position.Align(n) >= stream.Length) {
				stream.SetLength(stream.Length.Align(n));
				stream.Position = stream.Length;
			} else {
				stream.Position = stream.Position.Align(n);
			}
		}
	}
}
