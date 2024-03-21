using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DragonLib.IO;

public sealed class JsonLStream<T> : IEnumerable<T>, IAsyncEnumerable<T>, IDisposable {
	public JsonLStream(Stream stream, JsonSerializerOptions? options = null, Encoding? encoding = null, bool leaveOpen = false) {
		Options = options ?? JsonSerializerOptions.Default;
		if (stream.CanRead) {
			Reader = new StreamReader(stream, encoding ?? Encoding.UTF8, false, 4096, leaveOpen);
		}

		if (stream.CanWrite) {
			Writer = new StreamWriter(stream, encoding ?? Encoding.UTF8, 4096, leaveOpen);
		}
	}

	public TextReader? Reader { get; set; }
	public TextWriter? Writer { get; set; }
	public JsonSerializerOptions Options { get; set; }

	public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new()) {
		while (true) {
			var item = await ReadItemAsync(cancellationToken).ConfigureAwait(false);
			if (item == null) {
				yield break;
			}

			yield return item;
		}
	}

	public void Dispose() {
		Reader?.Dispose();
		Writer?.Dispose();
		Reader = null;
		Writer = null;
	}

	public IEnumerator<T> GetEnumerator() {
		while (true) {
			var item = ReadItem();
			if (item == null) {
				yield break;
			}

			yield return item;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public async Task WriteItemAsync(T item, CancellationToken? cancellationToken = null) {
		if (Writer == null) {
			throw new InvalidOperationException("Stream is not writable");
		}

		await Writer.WriteLineAsync(JsonSerializer.Serialize(item, Options).AsMemory(), cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
	}

	public async Task WriteItemsAsync(IEnumerable<T> items, CancellationToken? cancellationToken = null) {
		foreach (var item in items) {
			await WriteItemAsync(item, cancellationToken).ConfigureAwait(false);
		}
	}

	public async Task WriteItemsAsync(IEnumerator<T> items, CancellationToken? cancellationToken = null) {
		while (items.MoveNext()) {
			await WriteItemAsync(items.Current, cancellationToken).ConfigureAwait(false);
		}
	}

	public async Task WriteItemsAsync(IAsyncEnumerable<T> items, CancellationToken? cancellationToken = null) {
		await foreach (var item in items.WithCancellation(cancellationToken ?? CancellationToken.None)) {
			await WriteItemAsync(item, cancellationToken).ConfigureAwait(false);
		}
	}

	public async Task WriteItemsAsync(IAsyncEnumerator<T> items, CancellationToken? cancellationToken = null) {
		while (await items.MoveNextAsync().ConfigureAwait(false)) {
			await WriteItemAsync(items.Current, cancellationToken).ConfigureAwait(false);
		}
	}

	public async Task<T?> ReadItemAsync(CancellationToken cancellationToken = new()) {
		if (Reader == null) {
			throw new InvalidOperationException("Stream is not readable");
		}

		var line = await Reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
		return string.IsNullOrEmpty(line) ? default : JsonSerializer.Deserialize<T>(line);
	}

	public async Task<IEnumerable<T>> ReadItemsAsync(int count, CancellationToken cancellationToken = new()) {
		var items = new List<T>();
		for (var i = 0; i < count; i++) {
			var item = await ReadItemAsync(cancellationToken).ConfigureAwait(false);
			if (item == null) {
				break;
			}

			items.Add(item);
		}

		return items;
	}

	public void WriteItem(T item) {
		if (Writer == null) {
			throw new InvalidOperationException("Stream is not writable");
		}

		Writer.WriteLine(JsonSerializer.Serialize(item, Options));
	}

	public void WriteItems(IEnumerable<T> items) {
		foreach (var item in items) {
			WriteItem(item);
		}
	}

	public void WriteItems(IEnumerator<T> items) {
		while (items.MoveNext()) {
			WriteItem(items.Current);
		}
	}

	public T? ReadItem() {
		if (Reader == null) {
			throw new InvalidOperationException("Stream is not readable");
		}

		var line = Reader.ReadLine();
		return string.IsNullOrEmpty(line) ? default : JsonSerializer.Deserialize<T>(line);
	}

	public IEnumerable<T> ReadItems(int count) {
		var items = new List<T>();
		for (var i = 0; i < count; i++) {
			var item = ReadItem();
			if (item == null) {
				break;
			}

			items.Add(item);
		}

		return items;
	}

	public IEnumerable<T> ReadItems() {
		var items = new List<T>();
		while (true) {
			var item = ReadItem();
			if (item == null) {
				break;
			}

			items.Add(item);
		}

		return items;
	}
}
