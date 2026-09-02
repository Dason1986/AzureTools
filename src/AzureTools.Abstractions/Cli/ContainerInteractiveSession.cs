using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    internal class ContainerInteractiveSession
    {
        private const string SentinelPrompt = "===CMD_END_MARKER===>";
        private readonly ICliStreamSession _session;

        private readonly List<string> _outputLines = new();
        private readonly StringBuilder _lineBuffer = new();

        private readonly object _lock = new();
        private TaskCompletionSource<ShellCommandResult>? _pendingCmdTcs;

        public ContainerInteractiveSession(ICliStreamSession session)
        {
            _session = session;
            _session.RunTask.ContinueWith(_ => OnSessionExit(), TaskScheduler.Default);
        }

        public void FeedRawText(string rawText)
        {
            lock (_lock)
            {
                if (_pendingCmdTcs is null) return;

                _lineBuffer.Append(rawText);
                ProcessBufferedLines();
            }
        }

        private void ProcessBufferedLines()
        {
            string allText = _lineBuffer.ToString();
            _lineBuffer.Clear();

            string[] lines = allText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == lines.Length - 1 && !allText.EndsWith("\n") && !allText.EndsWith("\r"))
                {
                    _lineBuffer.Append(lines[i]);
                    break;
                }

                string line = lines[i];
                if (line.Contains(SentinelPrompt))
                {
                    CompleteCommand(line);
                    continue;
                }

                _outputLines.Add(line);
            }
        }

        private void CompleteCommand(string terminalLine)
        {
            ShellCommandResult result = new ShellCommandResult
            {
                OutputLines = new List<string>(_outputLines),
                RawOutput = string.Join(Environment.NewLine, _outputLines),
                Success = true
            };

            _outputLines.Clear();
            _pendingCmdTcs!.TrySetResult(result);
            _pendingCmdTcs = null;
        }

        public async Task InitializePromptAsync(CancellationToken ct = default)
        {
            await SendRawCommandAsync($"export PS1='{SentinelPrompt}'", ct);
        }

        public async Task<ShellCommandResult> SendCommandAsync(string command, CancellationToken ct = default)
        {
            lock (_lock)
            {
                if (_pendingCmdTcs != null)
                    throw new InvalidOperationException("同一会话不支持并发执行命令");

                _outputLines.Clear();
                _lineBuffer.Clear();
                _pendingCmdTcs = new TaskCompletionSource<ShellCommandResult>();
            }

            await _session.WriteLineAsync(command, ct);
            await _session.WriteLineAsync(string.Empty, ct);

            using CancellationTokenRegistration reg = ct.Register(() =>
            {
                lock (_lock)
                {
                    _pendingCmdTcs?.TrySetCanceled();
                    _pendingCmdTcs = null;
                }
            });
            return await _pendingCmdTcs.Task;
        }

        public void Close() => _session.Stop();

        private void OnSessionExit()
        {
            lock (_lock)
            {
                _pendingCmdTcs?.TrySetException(new InvalidOperationException("交互式会话已断开"));
                _pendingCmdTcs = null;
            }
        }

        private async Task SendRawCommandAsync(string cmd, CancellationToken ct)
        {
            await _session.WriteLineAsync(cmd, ct);
        }
    }
}
