using AzureTools.Tools.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public interface IContainerFileProvider : IAsyncDisposable, IDisposable
    {
        Task<IReadOnlyList<ContainerItemInfo>> GetDirectoryContentsAsync(string subpath, CancellationToken ct = default);

        Task<string> GetWorkingDirectoryAsync(CancellationToken ct = default);

        Task<ContainerItemInfo> GetFileInfoAsync(string subpath, CancellationToken ct = default);

        Task CreateDirectoryAsync(string path, CancellationToken ct = default);

        Task DeleteAsync(string path, CancellationToken ct = default);

        Task<string> ReadTextFileAsync(string path, CancellationToken ct = default);

        Task WriteTextFileAsync(string path, string content, CancellationToken ct = default);

        Task UploadFileAsync(IFileEntry source, string remotePath, CancellationToken ct = default);

        Task DownloadFileAsync(string remotePath, IFileEntry destination, CancellationToken ct = default);
    }
}
