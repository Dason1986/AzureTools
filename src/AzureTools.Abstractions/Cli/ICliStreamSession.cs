using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public interface ICliStreamSession
    {
        Task RunTask { get; }
        bool IsRunning { get; }
        void Stop();
        Task WriteAsync(string text, CancellationToken ct = default);
        Task WriteLineAsync(string text, CancellationToken ct = default);
        event CliStreamOutputEventHandler OutputReceived;
        event CliStreamOutputEventHandler ErrorReceived;
    }

    internal class CliStreamSession : ICliStreamSession
    {

        public CliStreamSession(ProcessStartInfo psi, CancellationToken externalCt = default)
        {
            Proc = new Process { StartInfo = psi };
            Proc.OutputDataReceived += OnOutput;
            Proc.ErrorDataReceived += OnError;
            Proc.Start();
            _stdinWriter = psi.RedirectStandardInput ? Proc.StandardInput : null;
            Proc.BeginOutputReadLine();
            Proc.BeginErrorReadLine();
            RunTask = Proc.WaitForExitAsync(externalCt);
        }
        private StreamWriter _stdinWriter;

        public Task RunTask { get; }
        public bool IsRunning => Proc is { HasExited: false };

        Process Proc;

        public event CliStreamOutputEventHandler OutputReceived;
        public event CliStreamOutputEventHandler ErrorReceived;

        private void OnOutput(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
                OutputReceived?.Invoke(this, new CliStreamOutputEventArgs(e.Data));
        }

        private void OnError(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
                ErrorReceived?.Invoke(this, new CliStreamOutputEventArgs(e.Data));
        }

        public void Stop()
        {
            try
            {
                if (IsRunning)
                    Proc.Kill();
            }
            catch
            {
            }
        }

        public async Task WriteAsync(string text, CancellationToken ct = default)
        {
            if (!IsRunning)
                throw new InvalidOperationException("进程已退出，无法写入标准输入");
            if (_stdinWriter == null)
                throw new NotSupportedException("未开启 RedirectStandardInput，不支持输入");

            await _stdinWriter.WriteAsync(text.AsMemory(), ct);
        }

        public async Task WriteLineAsync(string text, CancellationToken ct = default)
        {
            if (!IsRunning)
                throw new InvalidOperationException("进程已退出，无法写入标准输入");
            if (_stdinWriter == null)
                throw new NotSupportedException("未开启 RedirectStandardInput，不支持输入");

            await _stdinWriter.WriteLineAsync(text.AsMemory(), ct);
        }
    }
}
