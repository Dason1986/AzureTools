using AzureTools.Cli;
using AzureTools.Tools.IO;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.Managers
{
    /// <summary>
    /// 基于统一文件管理接口 IContainerFileProvider 的 SSH 远程文件访问实现
    /// </summary>
    public sealed class SshContainerFileProvider : IContainerFileProvider
    {
        private readonly ISshConnectionManager connectionManager;
        private readonly SshHostProfile host;

        public SshContainerFileProvider(ISshConnectionManager connectionManager, SshHostProfile host)
        {
            this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            this.host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public async Task<IReadOnlyList<ContainerItemInfo>> GetDirectoryContentsAsync(string subpath, CancellationToken ct = default)
        {
            return await Task.Run(() =>
            {
                SftpClient client = connectionManager.GetSftpClient(host);
                lock (client)
                {
                    string path = NormalizePath(subpath);
                    List<ContainerItemInfo> items = new List<ContainerItemInfo>();
                    foreach (ISftpFile entry in client.ListDirectory(path))
                    {
                        if (entry.Name == "." || entry.Name == "..") continue;
                        items.Add(new ContainerItemInfo(entry.Name, entry.FullName, entry.Length, new DateTimeOffset(entry.LastWriteTime), entry.IsDirectory));
                    }
                    return (IReadOnlyList<ContainerItemInfo>)items;
                }
            }, ct).ConfigureAwait(false);
        }

        public Task<string> GetWorkingDirectoryAsync(CancellationToken ct = default)
        {
            return Task.Run(() => connectionManager.GetSftpClient(host).WorkingDirectory, ct);
        }

        public async Task<ContainerItemInfo> GetFileInfoAsync(string subpath, CancellationToken ct = default)
        {
            return await Task.Run(() =>
            {
                SftpClient client = connectionManager.GetSftpClient(host);
                lock (client)
                {
                    string path = NormalizePath(subpath);
                    if (!client.Exists(path)) return NotFoundInfo(path);
                    ISftpFile entry = client.Get(path);
                    return new ContainerItemInfo(entry.Name, entry.FullName, entry.Length, new DateTimeOffset(entry.LastWriteTime), entry.IsDirectory);
                }
            }, ct).ConfigureAwait(false);
        }

        public Task CreateDirectoryAsync(string path, CancellationToken ct = default)
        {
            return RunAsync(client => client.CreateDirectory(NormalizePath(path)), ct);
        }

        public Task DeleteAsync(string path, CancellationToken ct = default)
        {
            return RunAsync(client => DeletePath(client, NormalizePath(path)), ct);
        }

        public async Task<string> ReadTextFileAsync(string path, CancellationToken ct = default)
        {
            MemoryStream buffer = new MemoryStream();
            await DownloadCoreAsync(path, buffer, ct).ConfigureAwait(false);
            buffer.Position = 0;
            using StreamReader reader = new StreamReader(buffer, Encoding.UTF8);
            return await reader.ReadToEndAsync(ct).ConfigureAwait(false);
        }

        public Task WriteTextFileAsync(string path, string content, CancellationToken ct = default)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
            return UploadCoreAsync(new MemoryStream(bytes), path, ct);
        }

        public async Task UploadFileAsync(IFileEntry source, string remotePath, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            using Stream stream = source.GetRead();
            await UploadCoreAsync(stream, remotePath, ct).ConfigureAwait(false);
        }

        public async Task DownloadFileAsync(string remotePath, IFileEntry destination, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(destination);
            using Stream stream = destination.GetWrite();
            await DownloadCoreAsync(remotePath, stream, ct).ConfigureAwait(false);
        }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        private Task RunAsync(Action<SftpClient> action, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                SftpClient client = connectionManager.GetSftpClient(host);
                lock (client) action(client);
            }, ct);
        }

        private Task UploadCoreAsync(Stream source, string remotePath, CancellationToken ct)
        {
            return RunAsync(client => client.UploadFile(source, NormalizePath(remotePath), true), ct);
        }

        private Task DownloadCoreAsync(string remotePath, Stream destination, CancellationToken ct)
        {
            return RunAsync(client => client.DownloadFile(NormalizePath(remotePath), destination), ct);
        }

        private static void DeletePath(SftpClient client, string path)
        {
            if (!client.Get(path).IsDirectory)
            {
                client.DeleteFile(path);
                return;
            }
            foreach (ISftpFile child in client.ListDirectory(path))
            {
                if (child.Name == "." || child.Name == "..") continue;
                DeletePath(client, child.FullName);
            }
            client.DeleteDirectory(path);
        }

        private static ContainerItemInfo NotFoundInfo(string path)
        {
            return new ContainerItemInfo(GetFileName(path), path, 0, DateTimeOffset.MinValue, false);
        }

        private static string GetFileName(string path)
        {
            string normalized = NormalizePath(path);
            int index = normalized.LastIndexOf('/');
            return index < 0 ? normalized : normalized[(index + 1)..];
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == ".") return "/";
            path = path.Replace("\\", "/");
            if (!path.StartsWith("/")) path = "/" + path;
            if (path.Length > 1) path = path.TrimEnd('/');
            return path.Length == 0 ? "/" : path;
        }
    }
}
