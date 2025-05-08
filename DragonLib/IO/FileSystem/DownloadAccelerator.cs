// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.ObjectModel;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using DragonLib.IO.Binary;

namespace DragonLib.IO.FileSystem;

public record struct RequestInfo(Uri Uri, bool Exists, bool SupportsThreading, long Size);

public sealed class DownloadAccelerator : IDisposable {
	public DownloadAccelerator() {
		Handler = new HttpClientHandler();
		Handler.AutomaticDecompression = DecompressionMethods.All;
		Handler.CheckCertificateRevocationList = true;
		Client = new HttpClient(Handler, true);
	}

	public HttpClient Client { get; }
	public HttpClientHandler Handler { get; }

	public Uri? BaseAddress { get; set; }

	public int ThreadCount { get; set; } = -1;
	public int MinimumSizePerThread { get; set; } = 0x1000000; // 16MB
	public int Retries { get; set; } = 3;

	public void Dispose() => Client.Dispose();

	public Uri CombineUri(string text, Uri? baseUri = null) {
		if (string.IsNullOrEmpty(text)) {
			return baseUri ?? throw new InvalidOperationException("Base URI is null");
		}

		var uri = text[0] == '/' || !text.Contains("://", StringComparison.Ordinal) ? new Uri(text, UriKind.Relative) : new Uri(text, UriKind.RelativeOrAbsolute);

		return CombineUri(uri, baseUri);
	}

	public Uri CombineUri(Uri uri, Uri? baseUri = null) {
		if (uri.IsAbsoluteUri && !string.IsNullOrEmpty(uri.Host)) {
			return uri;
		}

		baseUri ??= BaseAddress;

		if (baseUri == null || !baseUri.IsAbsoluteUri) {
			throw new InvalidOperationException("Relative URI cannot be resolved without a base address");
		}

		return new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/" + uri.OriginalString.TrimStart('/'));
	}

	private bool ShouldFallback(int threads, bool supportsThreading, long length) => !supportsThreading || threads == 1 || length < MinimumSizePerThread;

	private static Task[] CalculateDownloadRanges(int threads, long length, out (long start, long end)[] ranges) {
		var tasks = new Task[threads];
		ranges = new (long start, long end)[threads];
		var blockSize = length / threads;
		for (var i = 0; i < threads; i++) {
			ranges[i] = (i * blockSize, (i + 1) * blockSize - 1);
		}

		ranges[^1] = (ranges[^1].start, length);
		return tasks;
	}

	public Task DownloadFileThreaded(string url, string path, int threads = -1) => DownloadFileThreaded(CombineUri(url), path, threads);

	public async Task<RequestInfo> GetInfo(string url) => await GetInfo(CombineUri(url));

	public async Task<RequestInfo> GetInfo(Uri uri) {
		uri = CombineUri(uri);

		using var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
		var response = await Client.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead);
		if (!response.IsSuccessStatusCode) {
			return new RequestInfo(uri, false, false, 0);
		}

		var length = response.Content.Headers.ContentLength.GetValueOrDefault();
		var supportsRange = response.Headers.AcceptRanges.Contains("bytes");

		return new RequestInfo(uri, true, supportsRange, length);
	}

	public async Task DownloadFileThreaded(Uri uri, string path, int threads = -1) => await DownloadFileThreaded(await GetInfo(uri).ConfigureAwait(false), path, threads);

	public async Task DownloadFileThreaded(RequestInfo info, string path, int threads = -1) {
		var (uri, exists, supportsThreading, length) = info;
		uri = CombineUri(uri);

		ValidateFileExists(exists);

		await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

		if (ShouldFallback(threads, supportsThreading, length)) {
			await using var stream = await Client.GetStreamAsync(uri);
			await stream.CopyToAsync(fileStream);
			return;
		}

		if (threads == -1) {
			if (ThreadCount > 0) {
				threads = ThreadCount;
			} else {
				threads = (int) Math.Min(length / MinimumSizePerThread + 1, Environment.ProcessorCount);
			}
		}

		var tasks = CalculateDownloadRanges(threads, length, out var ranges);

		fileStream.SetLength(length);
		using var mmap = MemoryMappedFile.CreateFromFile(fileStream, path, length, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);

		for (var i = 0; i < threads; i++) {
			var range = ranges[i];
			tasks[i] = DownloadFileThread(uri, mmap, range.start, range.end);
		}

		try {
			await Task.WhenAll(tasks);
		} catch {
			// fallback to single thread.
			await DownloadFileThreaded(uri, path, 1);
		}
	}

	private static void ValidateFileExists(bool exists) {
		if (!exists) {
			throw new WebException("File does not exist", new FileNotFoundException(), WebExceptionStatus.ReceiveFailure, null);
		}
	}

	private async Task DownloadFileThread(Uri uri, MemoryMappedFile mmap, long rangeStart, long rangeEnd) {
		await using var view = mmap.CreateViewStream(rangeStart, rangeEnd);

		for (var i = 0; i < Retries; ++i) {
			try {
				using var request = new HttpRequestMessage(HttpMethod.Get, uri);
				request.Headers.Range = new RangeHeaderValue(rangeStart, rangeEnd);
				var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
				await using var stream = await response.Content.ReadAsStreamAsync();
				await stream.CopyToAsync(view);
				return;
			} catch (Exception) {
				if (i == Retries - 1) {
					throw;
				}
				// ignored
			}
		}
	}

	public async Task<RentedArray<byte>> FetchFileThreaded(Uri uri, string path, int threads = -1) => await FetchFileThreaded(await GetInfo(uri).ConfigureAwait(false), path, threads);

	public async Task<RentedArray<byte>> FetchFileThreaded(RequestInfo info, string path, int threads = -1) {
		var (uri, exists, supportsThreading, length) = info;
		uri = CombineUri(uri);

		ValidateFileExists(exists);

		if (ShouldFallback(threads, supportsThreading, length)) {
			await using var stream = await Client.GetStreamAsync(uri);
			var buffer = new RentedArray<byte>((int) stream.Length);
			await stream.ReadExactlyAsync(buffer.Array, 0, buffer.Length);
			return buffer;
		}

		if (threads == -1) {
			if (ThreadCount > 0) {
				threads = ThreadCount;
			} else {
				threads = (int) Math.Min(length / MinimumSizePerThread + 1, Environment.ProcessorCount);
			}
		}

		var tasks = CalculateDownloadRanges(threads, length, out var ranges);

		{
			var buffer = new RentedArray<byte>((int) length);

			for (var i = 0; i < threads; i++) {
				var range = ranges[i];
				tasks[i] = FetchFileThread(uri, buffer, range.start, range.end);
			}

			try {
				await Task.WhenAll(tasks);
			} catch {
				buffer.Dispose();
				// fallback to single thread.
				return await FetchFileThreaded(uri, path, 1);
			}

			return buffer;
		}
	}

	private async Task FetchFileThread(Uri uri, RentedArray<byte> buffer, long rangeStart, long rangeEnd) {
		for (var i = 0; i < Retries; ++i) {
			try {
				using var request = new HttpRequestMessage(HttpMethod.Get, uri);
				request.Headers.Range = new RangeHeaderValue(rangeStart, rangeEnd);
				var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
				await using var stream = await response.Content.ReadAsStreamAsync();
				await stream.ReadExactlyAsync(buffer.Array, (int) rangeStart, (int) (rangeEnd - rangeStart));
				return;
			} catch (Exception) {
				if (i == Retries - 1) {
					throw;
				}
				// ignored
			}
		}
	}

	public async Task<string?> FetchString(HttpMethod method, string url, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null, Encoding? encoding = null) => await FetchString(method, CombineUri(url), body, headers, encoding);

	public async Task<string?> FetchString(HttpMethod method, Uri uri, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null, Encoding? encoding = null) {
		uri = CombineUri(uri);

		var data = await FetchBytes(method, uri, body, headers);
		if (data == null) {
			return null;
		}

		return encoding?.GetString(data) ?? Encoding.UTF8.GetString(data);
	}

	public async Task<T?> FetchJson<T>(HttpMethod method, string url, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null, JsonSerializerOptions? options = null) => await FetchJson<T>(method, CombineUri(url), body, headers, options);

	public async Task<T?> FetchJson<T>(HttpMethod method, Uri uri, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null, JsonSerializerOptions? options = null) {
		uri = CombineUri(uri);
		var data = await FetchBytes(method, uri, body, headers);
		return data == null ? default : JsonSerializer.Deserialize<T>(data, options ?? JsonSerializerOptions.Default);
	}

	public async Task<byte[]?> FetchBytes(HttpMethod method, string url, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null) => await FetchBytes(method, CombineUri(url), body, headers);

	public async Task<byte[]?> FetchBytes(HttpMethod method, Uri uri, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null) {
		uri = CombineUri(uri);
		using var request = new HttpRequestMessage(method, uri);
		request.Version = HttpVersion.Version11;
		request.VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

		if (body != null) {
			request.Content = new ByteArrayContent(body);
		}

		if (headers != null) {
			foreach (var (key, value) in headers) {
				try {
					request.Headers.Add(key, value);
				} catch {
					// ignored
				}

				try {
					request.Content?.Headers.Add(key, value);
				} catch {
					// ignored
				}
			}
		}

		var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

		if (!response.IsSuccessStatusCode) {
			return default;
		}

		return await response.Content.ReadAsByteArrayAsync();
	}
}
