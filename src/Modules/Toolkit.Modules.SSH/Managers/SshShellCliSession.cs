using AzureTools.Cli;
using Renci.SshNet;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Toolkit.Modules.SSH.Managers
{
    public sealed class SshShellCliSession : ICliStreamSession
    {
        private readonly SshClient _client;
        private readonly ShellStream _shell;
        private readonly CancellationTokenSource _cts = new();
        private bool _stopped;

        public SshShellCliSession(SshClient client, string terminal = "xterm-256color")
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _shell = client.CreateShellStream(terminal, 80, 24, 800, 600, 4096);
            RunTask = Task.Run(() => ReadLoop(_cts.Token));
        }

        public Task RunTask { get; }

        public bool IsRunning => !_stopped && _client.IsConnected;

#pragma warning disable CS0067
        public event CliStreamOutputEventHandler ErrorReceived;
#pragma warning restore CS0067

        public event CliStreamOutputEventHandler OutputReceived;

        public async Task WriteAsync(string text, CancellationToken ct = default)
        {
            if (_stopped) throw new InvalidOperationException("会话已关闭");
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            await _shell.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            await _shell.FlushAsync().ConfigureAwait(false);
        }

        public async Task WriteLineAsync(string text, CancellationToken ct = default)
        {
            if (_stopped) throw new InvalidOperationException("会话已关闭");
            byte[] bytes = Encoding.UTF8.GetBytes(text + Environment.NewLine);
            await _shell.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            await _shell.FlushAsync().ConfigureAwait(false);
        }

        public void Stop()
        {
            if (_stopped) return;
            _stopped = true;
            _cts.Cancel();
            try { _shell.Close(); } catch { }
        }

        private void ReadLoop(CancellationToken ct)
        {
            byte[] buffer = new byte[4096];
            char[] chars = new char[4096];
            Decoder decoder = Encoding.UTF8.GetDecoder();
            StringBuilder lineBuffer = new StringBuilder();
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    int read = _shell.Read(buffer, 0, buffer.Length);
                    if (read == 0) break;
                    int count = decoder.GetChars(buffer, 0, read, chars, 0);
                    for (int i = 0; i < count; i++)
                    {
                        char c = chars[i];
                        if (c == '\n') RaiseLine(lineBuffer);
                        else if (c != '\r') lineBuffer.Append(c);
                    }
                }
                if (lineBuffer.Length > 0) RaiseLine(lineBuffer);
            }
            catch (OperationCanceledException) { }
            catch (IOException) { }
            catch { }
        }

        private void RaiseLine(StringBuilder lineBuffer)
        {
            string text = lineBuffer.ToString();
            lineBuffer.Clear();
            string clean = AnsiEscapeCleaner.Strip(text);
            if (clean.Length == 0) return;
            OutputReceived?.Invoke(this, new CliStreamOutputEventArgs(clean));
        }
    }
}
