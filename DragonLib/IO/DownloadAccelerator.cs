using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DragonLib.IO;

public record struct RequestInfo(Uri Uri, bool Exists, bool SupportsThreading, long Size);

public sealed class DownloadAccelerator : IDisposable {
    public DownloadAccelerator() => Client = new HttpClient();

    public HttpClient Client { get; }
    public int ThreadCount { get; set; } = -1;
    public int MinimumSizePerThread { get; set; } = 0x1000000; // 16MB
    public int Retries { get; set; } = 3;

    public void Dispose() {
        Client.Dispose();
    }

    public Task DownloadFileThreaded(string url, string path, int threads = -1) => DownloadFileThreaded(new Uri((Client.BaseAddress?.AbsoluteUri ?? "") + url), path, threads);

    public async Task<RequestInfo> GetInfo(string url) => await GetInfo(new Uri((Client.BaseAddress?.AbsoluteUri ?? "") + url)).ConfigureAwait(false);

    public async Task<RequestInfo> GetInfo(Uri uri) {
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
                threads = (int)Math.Min(length / MinimumSizePerThread + 1, Environment.ProcessorCount);
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
}
