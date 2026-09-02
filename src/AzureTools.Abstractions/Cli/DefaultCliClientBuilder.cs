using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public class DefaultCliClientBuilder : ICliClientBuilder
    {


        public DefaultCliClientBuilder(string fileName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            FileName = fileName;
        }

        private string _arguments = string.Empty;
        private Encoding _encoding = System.Text.Encoding.UTF8;
        private Action<string> _stdOutHandler = _ => { };
        private Action<string> _stdErrHandler = _ => { };
        private bool _redirectStandardInput;

        public string WorkingDirectory { get; private set; }

        public string FileName { get; }


        #region 链式配置
        public DefaultCliClientBuilder SetArgs(string arguments)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(arguments);
            _arguments = arguments;
            return this;
        }

        internal DefaultCliClientBuilder AppendArg(string arg)
        {
            if (!string.IsNullOrWhiteSpace(_arguments))
                _arguments += " ";
            _arguments += arg;
            return this;
        }

        public DefaultCliClientBuilder SetWorkingDirectory(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            WorkingDirectory = path;
            return this;
        }

        public DefaultCliClientBuilder SetEncoding(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            return this;
        }

        public DefaultCliClientBuilder SetStdOut(Action<string> handler)
        {
            _stdOutHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        public DefaultCliClientBuilder SetStdErr(Action<string> handler)
        {
            _stdErrHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        public DefaultCliClientBuilder EnableStandardInput()
        {
            _redirectStandardInput = true;
            return this;
        }
        #endregion

        public async Task<CliResult> ExecuteAsync(CancellationToken ct = default)
        {
            ProcessStartInfo psi = BuildPsi();
            using Process proc = new Process { StartInfo = psi };

            StringBuilder stdoutSb = new StringBuilder();
            StringBuilder stderrSb = new StringBuilder();
            proc.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    stdoutSb.AppendLine(e.Data);
                    _stdOutHandler?.Invoke(e.Data);
                }
            };
            proc.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    stderrSb.AppendLine(e.Data);
                    _stdErrHandler?.Invoke(e.Data);
                }
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            await proc.WaitForExitAsync(ct);
            return new CliResult(stdoutSb.ToString().TrimEnd(), stderrSb.ToString().TrimEnd(), proc.ExitCode);
        }

        public ICliStreamSession StartStream(CancellationToken externalCt = default)
        {
            if (OperatingSystem.IsWindows()) return PtyCliStreamSession.Create(FileName, _arguments, WorkingDirectory, _encoding, externalCt);

            ProcessStartInfo psi = BuildPsi();
            return new CliStreamSession(psi, externalCt);
        }

        private System.Diagnostics.ProcessStartInfo BuildPsi()
        {
            ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = FileName,
                Arguments = _arguments,
                WorkingDirectory = WorkingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = _redirectStandardInput,
                StandardOutputEncoding = _encoding,
                StandardErrorEncoding = _encoding
            };
            return psi;
        }



        ICliClientBuilder ICliClientBuilder.Args(string arguments)
        {
            return SetArgs(arguments);
        }

        ICliClientBuilder ICliClientBuilder.EnableStandardInput()
        {
            return EnableStandardInput();
        }

        ICliClientBuilder ICliClientBuilder.Encoding(Encoding encoding)
        {
            return SetEncoding(encoding);
        }

        ICliClientBuilder ICliClientBuilder.OnStdErr(Action<string> handler)
        {
            return SetStdErr(handler);
        }

        ICliClientBuilder ICliClientBuilder.OnStdOut(Action<string> handler)
        {
            return SetStdOut(handler);
        }

        ICliClientBuilder ICliClientBuilder.WorkingDirectory(string path)
        {
            return SetWorkingDirectory(path);
        }
    }
}
