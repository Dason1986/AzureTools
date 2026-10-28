using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    [SupportedOSPlatform("windows")]
    internal sealed class PtyCliStreamSession : ICliStreamSession, IDisposable
    {
        private readonly StreamReader _stdoutReader;
        private readonly StreamWriter _stdinWriter;
        private readonly SafePseudoConsoleHandle _pty;
        private readonly SafeWaitHandle _procHandle;
        private readonly IntPtr _attrList;
        private readonly CancellationTokenSource _cts;
        private readonly Task _readTask;
        private readonly ConcurrentQueue<string> _outputBuffer = new();
        private CliStreamOutputEventHandler _outputReceived;
        private readonly object _lock = new();
        private int _disposed;

        public Task RunTask { get; }

        public bool IsRunning
        {
            get
            {
                if (_procHandle.IsInvalid) return false;
                return PtyInterop.WaitForSingleObject(_procHandle, 0) == PtyInterop.WAIT_TIMEOUT;
            }
        }

        public event CliStreamOutputEventHandler OutputReceived
        {
            add => Subscribe(value);
            remove => Unsubscribe(value);
        }

        public event CliStreamOutputEventHandler ErrorReceived;

        private PtyCliStreamSession(
            StreamReader stdoutReader, StreamWriter stdinWriter,
            SafePseudoConsoleHandle pty, SafeWaitHandle procHandle, IntPtr attrList,
            Task runTask, CancellationTokenSource cts, string diagnosticLine)
        {
            _stdoutReader = stdoutReader;
            _stdinWriter = stdinWriter;
            _pty = pty;
            _procHandle = procHandle;
            _attrList = attrList;
            _cts = cts;
            RunTask = runTask;
            RaiseOutput(diagnosticLine);
            _readTask = ReadLoopAsync(cts.Token);
            _ = runTask.ContinueWith(_ => OnProcessExitedAsync(), TaskScheduler.Default);
        }

        private async Task OnProcessExitedAsync()
        {
            try { await _readTask; } catch { }
            try { PtyInterop.GetExitCodeProcess(_procHandle, out uint exitCode); RaiseOutput($"[PTY] 进程已退出 (ExitCode={exitCode})"); }
            catch { RaiseOutput("[PTY] 进程已退出"); }
            Cleanup();
        }

        public static PtyCliStreamSession Create(string fileName, string arguments, string workingDirectory, Encoding encoding, CancellationToken externalCt)
        {
            PtyInterop.CreatePipe(out SafePipeHandle ptyInputRead, out SafePipeHandle ptyInputWrite, IntPtr.Zero, 0);
            PtyInterop.CreatePipe(out SafePipeHandle ptyOutputRead, out SafePipeHandle ptyOutputWrite, IntPtr.Zero, 0);

            int hr = PtyInterop.CreatePseudoConsole(new PtyInterop.COORD(120, 30),
                ptyInputRead, ptyOutputWrite, 0, out SafePseudoConsoleHandle pty);
            ptyInputRead.Dispose();
            ptyOutputWrite.Dispose();
            if (hr != 0)
            {
                ptyInputWrite.Dispose();
                ptyOutputRead.Dispose();
                throw new InvalidOperationException($"CreatePseudoConsole failed: 0x{hr:X}");
            }

            AnonymousPipeClientStream stdinClient = new AnonymousPipeClientStream(PipeDirection.Out, ptyInputWrite);
            StreamWriter stdinWriter = new StreamWriter(stdinClient, encoding) { AutoFlush = true };
            AnonymousPipeClientStream stdoutClient = new AnonymousPipeClientStream(PipeDirection.In, ptyOutputRead);
            StreamReader stdoutReader = new StreamReader(stdoutClient, Encoding.UTF8);

            IntPtr size = IntPtr.Zero;
            PtyInterop.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref size);
            IntPtr attrList = Marshal.AllocHGlobal(size);
            if (!PtyInterop.InitializeProcThreadAttributeList(attrList, 1, 0, ref size))
                throw new InvalidOperationException("InitializeProcThreadAttributeList failed");
            if (!PtyInterop.UpdateProcThreadAttribute(attrList, 0, PtyInterop.PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE, pty.DangerousGetHandle(), nint.Size, IntPtr.Zero, IntPtr.Zero))
                throw new InvalidOperationException("UpdateProcThreadAttribute failed");

            PtyInterop.STARTUPINFOEX si = new PtyInterop.STARTUPINFOEX();
            si.StartupInfo.cb = Marshal.SizeOf<PtyInterop.STARTUPINFOEX>();
            si.lpAttributeList = attrList;
            string cmdLine = BuildCommandLine(fileName, arguments);
            uint flags = PtyInterop.EXTENDED_STARTUPINFO_PRESENT | PtyInterop.CREATE_UNICODE_ENVIRONMENT;
            if (!PtyInterop.CreateProcessW(null, cmdLine, IntPtr.Zero, IntPtr.Zero, false, flags, IntPtr.Zero, workingDirectory, ref si, out PtyInterop.PROCESS_INFORMATION pi))
                throw new InvalidOperationException("CreateProcessW failed: " + Marshal.GetLastWin32Error());
            PtyInterop.CloseHandle(pi.hThread);

            SafeWaitHandle procHandle = new SafeWaitHandle(pi.hProcess, true);
            bool stillRunning = PtyInterop.WaitForSingleObject(procHandle, 0) == PtyInterop.WAIT_TIMEOUT;
            string diagnostic = $"[PTY] 已启动 (PID={pi.dwProcessId}, Running={stillRunning}): {cmdLine}";

            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            Task runTask = Task.Run(() =>
            {
                try { PtyInterop.WaitForSingleObject(procHandle, PtyInterop.WAIT_INFINITE); }
                catch (ObjectDisposedException) { }
            }, cts.Token);

            return new PtyCliStreamSession(stdoutReader, stdinWriter, pty, procHandle, attrList, runTask, cts, diagnostic);
        }

        private static string BuildCommandLine(string fileName, string arguments)
        {
            bool isBatch = fileName.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase)
                        || fileName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase);
            string quoted = "\"" + fileName + "\"";
            string tail = string.IsNullOrEmpty(arguments) ? string.Empty : " " + arguments;
            return isBatch ? "cmd.exe /q /c " + quoted + tail : quoted + tail;
        }

        private async Task ReadLoopAsync(CancellationToken ct)
        {
            char[] chars = new char[4096];
            StringBuilder lineBuffer = new StringBuilder();
            try
            {
                while (true)
                {
                    int n = await _stdoutReader.ReadAsync(chars, 0, chars.Length);
                    if (n == 0) break;
                    for (int i = 0; i < n; i++)
                    {
                        char c = chars[i];
                        if (c == '\n')
                        {
                            RaiseCleanLine(lineBuffer);
                            lineBuffer.Clear();
                        }
                        else if (c != '\r')
                        {
                            lineBuffer.Append(c);
                        }
                    }
                }
                if (lineBuffer.Length > 0)
                    RaiseCleanLine(lineBuffer);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                RaiseOutput($"[PTY] 读取异常: {ex.Message}");
            }
        }

        private void RaiseCleanLine(StringBuilder raw)
        {
            string text = raw.ToString();
            string clean = AnsiEscapeCleaner.Strip(text);
            if (clean.Length == 0 && text.Length > 0) return;
            RaiseOutput(clean);
        }

        private void RaiseOutput(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            CliStreamOutputEventHandler handlers;
            lock (_lock)
            {
                if (_outputReceived is null)
                {
                    _outputBuffer.Enqueue(line);
                    return;
                }
                handlers = _outputReceived;
            }
            handlers(this, new CliStreamOutputEventArgs(line));
        }

        private void Subscribe(CliStreamOutputEventHandler handler)
        {
            List<string> replay = new List<string>();
            lock (_lock)
            {
                while (_outputBuffer.TryDequeue(out string line))
                    replay.Add(line);
                _outputReceived += handler;
            }
            foreach (string line in replay)
                handler(this, new CliStreamOutputEventArgs(line));
        }

        private void Unsubscribe(CliStreamOutputEventHandler handler)
        {
            lock (_lock)
                _outputReceived -= handler;
        }

        public async Task WriteAsync(string text, CancellationToken ct = default)
        {
            await _stdinWriter.WriteAsync(text.AsMemory(), ct);
        }

        public async Task WriteLineAsync(string text, CancellationToken ct = default)
        {
            await _stdinWriter.WriteLineAsync(text.AsMemory(), ct);
        }

        public void Stop()
        {
            try { _pty.Dispose(); } catch { }
            try
            {
                if (IsRunning)
                    PtyInterop.TerminateProcess(_procHandle, 1);
            }
            catch { }
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                Cleanup();
            });
        }

        public void Dispose()
        {
            Stop();
        }

        private void Cleanup()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            try { _cts.Dispose(); } catch { }
            try
            {
                if (_attrList != IntPtr.Zero)
                {
                    PtyInterop.DeleteProcThreadAttributeList(_attrList);
                    Marshal.FreeHGlobal(_attrList);
                }
            }
            catch { }
            try { _stdinWriter.Dispose(); } catch { }
            try { _stdoutReader.Dispose(); } catch { }
            try { _pty.Dispose(); } catch { }
            try { _procHandle.Dispose(); } catch { }
        }
    }
}
