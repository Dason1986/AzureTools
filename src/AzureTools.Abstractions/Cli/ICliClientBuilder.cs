using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public interface ICliClientBuilder
    {

        ICliClientBuilder Args(string arguments);
        ICliClientBuilder EnableStandardInput();
        ICliClientBuilder Encoding(Encoding encoding);
        Task<CliResult> ExecuteAsync(CancellationToken ct = default);
        ICliClientBuilder OnStdErr(Action<string> handler);
        ICliClientBuilder OnStdOut(Action<string> handler);
        ICliStreamSession StartStream(CancellationToken externalCt = default);
        ICliClientBuilder WorkingDirectory(string path);
    }
}
