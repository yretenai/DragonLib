using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace DragonLib.IO;

public record struct RequestInfo(Uri Uri, bool Exists, bool SupportsThreading, long Size);

public sealed class DownloadAccelerator : IDisposable {
    public DownloadAccelerator() {
        Handler = new HttpClientHandler();
        Handler.AutomaticDecompression = DecompressionMethods.All;
        Client = new HttpClient(Handler, true);
    }

    public HttpClient Client { get; }
    public HttpClientHandler Handler { get; }

    public Uri? BaseAddress { get; set; }

    public int ThreadCount { get; set; } = -1;
    public int MinimumSizePerThread { get; set; } = 0x1000000; // 16MB
    public int Retries { get; set; } = 3;

    public void Dispose() {
        Client.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Uri CombineUri(string text) => CombineUri(text[0] == '/' || !text.Contains("://", StringComparison.Ordinal) ? new Uri(text, UriKind.Relative) : new Uri(text, UriKind.RelativeOrAbsolute));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Uri CombineUri(Uri uri) {
        if (uri.IsAbsoluteUri && !string.IsNullOrEmpty(uri.Host)) {
            return uri;
        }

        if (BaseAddress == null) {
            throw new InvalidOperationException("Relative URI cannot be resolved without a base address");
        }

        return new Uri(BaseAddress, uri);
    }

    public Task DownloadFileThreaded(string url, string path, int threads = -1) => DownloadFileThreaded(CombineUri(url), path, threads);

    public async Task<RequestInfo> GetInfo(string url) => await GetInfo(CombineUri(url)).ConfigureAwait(false);

    public async Task<RequestInfo> GetInfo(Uri uri) {
        uri = CombineUri(uri);

        using var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
        var response = await Client.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) {
            return new RequestInfo(uri, false, false, 0);
        }

        var length = response.Content.Headers.ContentLength.GetValueOrDefault();
        var supportsRange = response.Headers.AcceptRanges.Contains("bytes");

        return new RequestInfo(uri, true, supportsRange, length);
    }

    public async Task DownloadFileThreaded(Uri uri, string path, int threads = -1) {
        await DownloadFileThreaded(await GetInfo(uri).ConfigureAwait(false), path, threads).ConfigureAwait(false);
    }

    public async Task DownloadFileThreaded(RequestInfo info, string path, int threads = -1) {
        var (uri, exists, supportsThreading, length) = info;
        uri = CombineUri(uri);

        if (!exists) {
            throw new WebException("File does not exist", new FileNotFoundException(), WebExceptionStatus.ReceiveFailure, null);
        }

        if (!supportsThreading || threads == 1 || length < MinimumSizePerThread) {
        #pragma warning disable CA2000 // Dispose objects before losing scope, buggy: https://github.com/dotnet/roslyn-analyzers/issues/5712
            var stream = await Client.GetStreamAsync(uri).ConfigureAwait(false);
            await using var _ = stream.ConfigureAwait(false);
            var fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            await using var __ = fileStream.ConfigureAwait(false);
        #pragma warning restore CA2000
            await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            return;
        }

        if (threads == -1) {
            if (ThreadCount > 0) {
                threads = ThreadCount;
            } else {
                threads = (int) Math.Min(length / MinimumSizePerThread + 1, Environment.ProcessorCount);
            }
        }

        var tasks = new Task[threads];
        var ranges = new (long start, long end)[threads];
        var blockSize = length / threads;
        for (var i = 0; i < threads; i++) {
            ranges[i] = (i * blockSize, (i + 1) * blockSize - 1);
        }

        ranges[^1] = (ranges[^1].start, length);

        {
        #pragma warning disable CA2000 // Dispose objects before losing scope, buggy: https://github.com/dotnet/roslyn-analyzers/issues/5712
            var fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            await using var _ = fileStream.ConfigureAwait(false);
        #pragma warning restore CA2000
            fileStream.SetLength(length);
        }

        for (var i = 0; i < threads; i++) {
            var range = ranges[i];
            tasks[i] = DownloadFileThread(uri, path, range.start, range.end);
        }

        try {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        } catch {
            // fallback to single thread.
            await DownloadFileThreaded(uri, path, 1).ConfigureAwait(false);
        }
    }

    private async Task DownloadFileThread(Uri uri, string path, long rangeStart, long rangeEnd) {
        for (var i = 0; i < Retries; ++i) {
            try {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Range = new RangeHeaderValue(rangeStart, rangeEnd);
                var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
#pragma warning disable CA2000 // Dispose objects before losing scope, buggy: https://github.com/dotnet/roslyn-analyzers/issues/5712
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                await using var _ = stream.ConfigureAwait(false);
                var fileStream = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                await using var __ = fileStream.ConfigureAwait(false);
#pragma warning restore CA2000
                fileStream.Seek(rangeStart, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
                return;
            } catch (Exception) {
                if (i == Retries - 1) {
                    throw;
                }
                // ignored
            }
        }
    }

    public async Task<string?> FetchString(HttpMethod method, string url, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null, Encoding? encoding = null) => await FetchString(method, CombineUri(url), body, headers, encoding).ConfigureAwait(false);
    public async Task<string?> FetchString(HttpMethod method, Uri uri, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null, Encoding? encoding = null) {
        uri = CombineUri(uri);

        var data = await FetchBytes(method, uri, body, headers).ConfigureAwait(false);
        if (data == null) {
            return null;
        }

        return encoding?.GetString(data) ?? Encoding.UTF8.GetString(data);
    }

    public async Task<T?> FetchJson<T>(HttpMethod method, string url, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null, JsonSerializerOptions? options = null) => await FetchJson<T>(method, CombineUri(url), body, headers, options).ConfigureAwait(false);
    public async Task<T?> FetchJson<T>(HttpMethod method, Uri uri, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null, JsonSerializerOptions? options = null) {
        uri = CombineUri(uri);
        var data = await FetchBytes(method, uri, body, headers).ConfigureAwait(false);
        return data == null ? default : JsonSerializer.Deserialize<T>(data, options ?? JsonSerializerOptions.Default);
    }

    public async Task<byte[]?> FetchBytes(HttpMethod method, string url, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null) => await FetchBytes(method, CombineUri(url), body, headers).ConfigureAwait(false);
    public async Task<byte[]?> FetchBytes(HttpMethod method, Uri uri, byte[]? body = null, ReadOnlyDictionary<string, string>? headers = null) {
        uri = CombineUri(uri);
        using var request = new HttpRequestMessage(method, uri) { Version = HttpVersion.Version11, VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher };

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

        var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode) {
            return default;
        }

        return await response.Content.ReadAsByteArrayAsync();
    }
}
