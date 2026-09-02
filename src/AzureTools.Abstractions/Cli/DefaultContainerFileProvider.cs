using AzureTools.Tools.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public class DefaultContainerFileProvider : IContainerFileProvider
    {
        private static readonly Regex EntryRegex = new(
            @"^F=(?<type>[df])\|S=(?<size>\d+)\|T=(?<time>\d+)\|N=(?<name>.*)$",
            RegexOptions.Compiled);

        private static readonly Regex AnsiRegex = new(
            @"\x1b\[[0-9;]*[a-zA-Z]|\x1b\][^\x07]*\x07|\x1b[()][0-9A-Za-z]|\x1b[=>]",
            RegexOptions.Compiled);

        private const int CommandTimeoutSeconds = 30;
        private const int ShellReadyDelayMs = 600;
        private const int DrainDelayMs = 120;
        private const string MarkerPrefix = "<<";
        private const string MarkerSuffix = ">>";

        private readonly ICliStreamSession _session;
        private readonly object _contextLock = new();
        private readonly List<string> _pendingBuffer = new();
        private CommandContext _current;
        private bool _shellReady;
        private bool _disposed;

        public DefaultContainerFileProvider(ICliStreamSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _session.OutputReceived += OnOutput;
            EnsureShellReady();
        }

        public async Task<IReadOnlyList<ContainerItemInfo>> GetDirectoryContentsAsync(string subpath, CancellationToken ct = default)
        {
            string path = NormalizePath(subpath);
            string script = BuildListScript(path);
            string[] lines = await RunCommandAsync(script, ct).ConfigureAwait(false);
            List<ContainerItemInfo> entries = new List<ContainerItemInfo>();
            foreach (string line in lines)
            {
                ContainerItemInfo info = ParseEntry(line, path);
                if (info != null) entries.Add(info);
            }
            return entries;
        }

        public async Task<string> GetWorkingDirectoryAsync(CancellationToken ct = default)
        {
            string[] lines = await RunCommandAsync("pwd", ct).ConfigureAwait(false);
            foreach (string line in lines)
            {
                string t = line.Trim();
                if (t.Length == 0 || t[0] != '/') continue;
                int space = t.IndexOf(' ');
                if (space >= 0) continue;
                return t;
            }
            return "/";
        }

        public async Task<ContainerItemInfo> GetFileInfoAsync(string subpath, CancellationToken ct = default)
        {
            string path = NormalizePath(subpath);
            string name = Path.GetFileName(path);
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/") ?? "/";
            string script = BuildSingleInfoScript(path);
            string[] lines = await RunCommandAsync(script, ct).ConfigureAwait(false);
            if (lines.Length == 0)
                return new ContainerItemInfo(name, path, 0, DateTimeOffset.MinValue, false);
            return ParseEntry(lines[0], parent) ?? new ContainerItemInfo(name, path, 0, DateTimeOffset.MinValue, false);
        }

        public Task CreateDirectoryAsync(string path, CancellationToken ct = default)
        {
            return RunCommandAsync($"mkdir -p {Quote(NormalizePath(path))}", ct);
        }

        public Task DeleteAsync(string path, CancellationToken ct = default)
        {
            return RunCommandAsync($"rm -rf {Quote(NormalizePath(path))}", ct);
        }

        public async Task<string> ReadTextFileAsync(string path, CancellationToken ct = default)
        {
            string[] lines = await RunCommandAsync($"cat {Quote(NormalizePath(path))} 2>/dev/null", ct).ConfigureAwait(false);
            return string.Join(Environment.NewLine, lines);
        }

        public async Task WriteTextFileAsync(string path, string content, CancellationToken ct = default)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
            string remote = NormalizePath(path);
            await RunCommandAsync($": > {Quote(remote)}", ct).ConfigureAwait(false);
            const int chunkSize = 4096;
            for (int i = 0; i < bytes.Length; i += chunkSize)
            {
                int len = Math.Min(chunkSize, bytes.Length - i);
                string b64 = Convert.ToBase64String(bytes, i, len);
                await RunCommandAsync($"printf '%s' {Quote(b64)} | base64 -d >> {Quote(remote)}", ct).ConfigureAwait(false);
            }
        }

        public async Task UploadFileAsync(IFileEntry source, string remotePath, CancellationToken ct = default)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            string remote = NormalizePath(remotePath);
            await RunCommandAsync($": > {Quote(remote)}", ct).ConfigureAwait(false);
            using Stream stream = source.GetRead();
            const int chunkSize = 4096;
            byte[] buffer = new byte[chunkSize];
            int read;
            while ((read = await stream.ReadAsync(buffer, 0, chunkSize, ct).ConfigureAwait(false)) > 0)
            {
                string b64 = Convert.ToBase64String(buffer, 0, read);
                await RunCommandAsync($"printf '%s' {Quote(b64)} | base64 -d >> {Quote(remote)}", ct).ConfigureAwait(false);
            }
        }

        public async Task DownloadFileAsync(string remotePath, IFileEntry destination, CancellationToken ct = default)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            string[] lines = await RunCommandAsync($"base64 {Quote(NormalizePath(remotePath))} 2>/dev/null", ct).ConfigureAwait(false);
            byte[] bytes = Convert.FromBase64String(string.Join(string.Empty, lines));
            using Stream stream = destination.GetWrite();
            await stream.WriteAsync(bytes, 0, bytes.Length, ct).ConfigureAwait(false);
            await stream.FlushAsync(ct).ConfigureAwait(false);
        }

        private static string BuildListScript(string path)
        {
            string q = Quote(path);
            StringBuilder sb = new StringBuilder();
            sb.Append("for f in $(ls -1A ");
            sb.Append(q);
            sb.Append(" 2>/dev/null); do ");
            sb.Append("p=");
            sb.Append(q);
            sb.Append("/$f; ");
            sb.Append("if [ -d \"$p\" ]; then t=d; else t=f; fi; ");
            sb.Append("s=$(stat -c%s \"$p\" 2>/dev/null || stat -f%z \"$p\" 2>/dev/null || echo 0); ");
            sb.Append("ts=$(stat -c%Y \"$p\" 2>/dev/null || stat -f%m \"$p\" 2>/dev/null || echo 0); ");
            sb.Append("echo \"F=$t|S=$s|T=$ts|N=$f\"; done");
            return sb.ToString();
        }

        private static string BuildSingleInfoScript(string path)
        {
            string q = Quote(path);
            StringBuilder sb = new StringBuilder();
            sb.Append("if [ -e ");
            sb.Append(q);
            sb.Append(" ]; then ");
            sb.Append("if [ -d ");
            sb.Append(q);
            sb.Append(" ]; then t=d; else t=f; fi; ");
            sb.Append("s=$(stat -c%s ");
            sb.Append(q);
            sb.Append(" 2>/dev/null || stat -f%z ");
            sb.Append(q);
            sb.Append(" 2>/dev/null || echo 0); ");
            sb.Append("ts=$(stat -c%Y ");
            sb.Append(q);
            sb.Append(" 2>/dev/null || stat -f%m ");
            sb.Append(q);
            sb.Append(" 2>/dev/null || echo 0); ");
            sb.Append("n=$(basename ");
            sb.Append(q);
            sb.Append("); ");
            sb.Append("echo \"F=$t|S=$s|T=$ts|N=$n\"; fi");
            return sb.ToString();
        }

        private void EnsureShellReady()
        {
            if (_shellReady) return;
            SpinWait.SpinUntil(() => _session.IsRunning, 1500);
            if (!_session.IsRunning)
                throw new InvalidOperationException("容器 exec 会话未能成功启动");
            Thread.Sleep(ShellReadyDelayMs);
            _shellReady = true;
        }

        private async Task<string[]> RunCommandAsync(string command, CancellationToken ct)
        {
            ThrowIfDisposed();
            EnsureShellReady();

            string tag = Guid.NewGuid().ToString("N");
            string fullMarker = MarkerPrefix + tag + MarkerSuffix;

            CommandContext ctx = new CommandContext(fullMarker);
            lock (_contextLock)
            {
                _pendingBuffer.Clear();
                _current = ctx;
            }

            string wrapped = command + " ; printf '%s\\n' " + Quote(fullMarker);
            Debug.WriteLine("[ContainerFileProvider] SEND: " + wrapped);
            using CancellationTokenSource combined = CancellationTokenSource.CreateLinkedTokenSource(ct);
            combined.CancelAfter(TimeSpan.FromSeconds(CommandTimeoutSeconds));
            CancellationToken token = combined.Token;
            bool success = false;
            try
            {
                await _session.WriteLineAsync(wrapped, token).ConfigureAwait(false);
                string[] result = await ctx.Task.WaitAsync(token).ConfigureAwait(false);
                success = true;
                Debug.WriteLine($"[ContainerFileProvider] RECV: {result.Length} lines");
                return result;
            }
            catch (OperationCanceledException ex) when (combined.IsCancellationRequested && !_disposed && !ct.IsCancellationRequested)
            {
                throw new TimeoutException("命令执行超时: " + command, ex);
            }
            catch (OperationCanceledException) when (_disposed)
            {
                throw new ObjectDisposedException(nameof(DefaultContainerFileProvider));
            }
            finally
            {
                lock (_contextLock)
                {
                    if (ReferenceEquals(_current, ctx))
                        _current = null;
                }
                if (!success)
                {
                    ctx.TryCancel();
                    try { combined.Cancel(false); } catch { }
                }
            }
        }

        private void OnOutput(object sender, CliStreamOutputEventArgs e)
        {
            if (e.Line == null) return;
            Debug.WriteLine("[ContainerFileProvider] LINE: " + e.Line);
            CommandContext ctx;
            lock (_contextLock)
            {
                ctx = _current;
                if (ctx == null)
                {
                    _pendingBuffer.Add(e.Line);
                    return;
                }
            }
            ctx.AcceptLine(e.Line);
        }

        private static ContainerItemInfo ParseEntry(string line, string parentPath)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;
            Match m = EntryRegex.Match(line.Trim());
            if (!m.Success) return null;
            char type = m.Groups["type"].Value[0];
            long size = long.Parse(m.Groups["size"].Value);
            long ts = long.Parse(m.Groups["time"].Value);
            string name = m.Groups["name"].Value;
            bool isDir = type == 'd';
            string normalizedParent = parentPath == "/" ? string.Empty : parentPath.TrimEnd('/');
            string physical = normalizedParent + "/" + name;
            return new ContainerItemInfo(name, physical, size, DateTimeOffset.FromUnixTimeSeconds(ts), isDir);
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == ".") return "/";
            path = path.Replace("\\", "/");
            if (!path.StartsWith("/")) path = "/" + path;
            if (path.Length > 1) path = path.TrimEnd('/');
            return path.Length == 0 ? "/" : path;
        }

        private static string Quote(string path) => "'" + path.Replace("'", "'\\''") + "'";

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DefaultContainerFileProvider));
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            try { _session.OutputReceived -= OnOutput; } catch { }
            try { _session.Stop(); } catch { }
            lock (_contextLock)
            {
                _current?.TryCancel();
                _current = null;
            }
            await default(ValueTask);
        }

        private sealed class CommandContext
        {
            private readonly string _marker;
            private readonly TaskCompletionSource<string[]> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly object _gate = new();
            private bool _markerSeen;

            public CommandContext(string marker)
            {
                _marker = marker;
                Buffer = new List<string>();
            }

            public List<string> Buffer { get; }

            public Task<string[]> Task => _tcs.Task;

            public void AcceptLine(string line)
            {
                lock (_gate)
                {
                    if (_tcs.Task.IsCompleted)
                    {
                        Debug.WriteLine("[ContainerFileProvider] AcceptLine SKIP: task already completed, line=" + line);
                        return;
                    }
                    if (!_markerSeen && IsMarkerLine(line))
                    {
                        _markerSeen = true;
                        Debug.WriteLine($"[ContainerFileProvider] MARKER HIT: setting result with {Buffer.Count} lines");
                        _tcs.TrySetResult(Buffer.ToArray());
                        return;
                    }
                    if (_markerSeen)
                    {
                        Debug.WriteLine("[ContainerFileProvider] AcceptLine SKIP (after marker): " + line);
                        return;
                    }
                    if (line.Contains(_marker) && !IsMarkerLine(line))
                    {
                        Debug.WriteLine("[ContainerFileProvider] AcceptLine SKIP (echo): " + line);
                        return;
                    }
                    Buffer.Add(line);
                }
            }

            private bool IsMarkerLine(string line)
            {
                if (string.IsNullOrEmpty(line)) return false;
                string t = AnsiRegex.Replace(line, string.Empty).Trim();
                if (t.Length < _marker.Length) return false;
                return t.EndsWith(_marker, StringComparison.Ordinal);
            }

            public void TryCancel()
            {
                lock (_gate)
                {
                    _tcs.TrySetCanceled();
                }
            }
        }
    }
}
