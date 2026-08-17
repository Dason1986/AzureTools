using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AzureTools.Tools.IO
{

    public class ZipFileProviderBuilder
    {
        #region Fields & Properties

        private string path;
        private string password;

        #endregion

        public ZipFileProviderBuilder WithPath(string path)
        {
            this.path = path;
            return this;
        }

        public ZipFileProviderBuilder WithPassword(string password)
        {
            this.password = password;
            return this;
        }

        public IStoreFileProvider Build()
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            return new ZipStoreProvider(path, password);
        }

        public static ZipFileProviderBuilder Create(string path)
        {
            return new ZipFileProviderBuilder().WithPath(path);
        }
    }

    internal class ZipStoreProvider : IStoreFileProvider
    {
        #region Constructors

        public ZipStoreProvider(string path, string password)
        {
            _path = path;
            _password = password;

            if (!File.Exists(path))
            {
                using FileStream stream = File.Create(path);
                using ZipOutputStream zipStream = new ZipOutputStream(stream);
                zipStream.Finish();
            }
            ctx = OpenZip();
        }

        #endregion

        #region Fields & Properties

        private readonly string _path;
        private readonly string _password;
        private readonly object _syncRoot = new();
        private readonly ZipContext ctx;

        #endregion

        public void Dispose()
        {
            ctx.Dispose();
        }

        private ZipContext OpenZip()
        {
            FileStream stream = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            ZipFile zip = new ZipFile(stream, leaveOpen: false);
            if (!string.IsNullOrEmpty(_password)) zip.Password = _password;
            return new ZipContext(zip);
        }

        public byte[] GetBytes(string path)
        {
            lock (_syncRoot)
            {
                string normalizedPath = ZipFileUtils.NormalizePath(path);
                int index = ctx.Zip.FindEntry(normalizedPath, false);
                if (index < 0) return Array.Empty<byte>();

                using Stream entryStream = ctx.Zip.GetInputStream(index);
                using MemoryStream ms = new MemoryStream();
                entryStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public void WriteFile(string path, byte[] data)
        {
            lock (_syncRoot)
            {
                string normalizedPath = ZipFileUtils.NormalizePath(path);


                ctx.Zip.BeginUpdate(new MemoryArchiveStorage());
                if (ctx.Zip.FindEntry(normalizedPath, false) >= 0)
                    ctx.Zip.Delete(normalizedPath);

                ctx.Zip.Add(new MemoryDataSource(data), normalizedPath);
                ctx.Zip.CommitUpdate();
            }
        }

        public void WriteFiles(IDictionary<string, byte[]> files)
        {
            lock (_syncRoot)
            {
                if (files == null || files.Count == 0) return;

                ctx.Zip.BeginUpdate(new MemoryArchiveStorage());

                foreach ((string path, byte[] data) in files)
                {
                    string normalizedPath = ZipFileUtils.NormalizePath(path);


                    if (ctx.Zip.FindEntry(normalizedPath, false) >= 0)
                        ctx.Zip.Delete(normalizedPath);
                    ctx.Zip.Add(new MemoryDataSource(data), normalizedPath);
                }
                ctx.Zip.CommitUpdate();
            }
        }

        public void DeleteFile(string path)
        {
            lock (_syncRoot)
            {
                string normalizedPath = ZipFileUtils.NormalizePath(path);

                if (ctx.Zip.FindEntry(normalizedPath, false) < 0)
                    return;

                ctx.Zip.BeginUpdate(new MemoryArchiveStorage());
                ctx.Zip.Delete(normalizedPath);
                ctx.Zip.CommitUpdate();
            }
        }

        public IEnumerable<string> GetFileNames()
        {
            lock (_syncRoot)
            {
                return ctx.Zip
                    .Cast<ZipEntry>()
                    .Select(e => ZipFileUtils.NormalizePath(e.Name))
                    .ToList();
            }
        }
    }

    internal sealed class ZipContext : IDisposable
    {
        public ZipFile Zip { get; }

        public ZipContext(ZipFile zip)
        {
            Zip = zip;
        }

        public void Dispose()
        {
            Zip.Close();
        }
    }

    internal class MemoryDataSource : IStaticDataSource
    {
        private readonly byte[] _data;

        public MemoryDataSource(byte[] data)
        {
            _data = data;
        }

        public Stream GetSource()
        {
            return new MemoryStream(_data);
        }
    }

    internal static class ZipFileUtils
    {
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/";
            string normalized = path.Replace('\\', '/');
            if (!normalized.StartsWith("/"))
                normalized = "/" + normalized;
            return normalized.TrimEnd('/').TrimStart('/');
        }
    }
}
