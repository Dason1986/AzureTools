using Avalonia.Platform.Storage;
using AzureTools.Tools.IO;
using System.IO;

namespace AzureTools.Services
{
    class AvaloniaFileEntry : IFileEntry
    {

        public AvaloniaFileEntry(IStorageFile storageFile)
        {

            FullPath = storageFile.Path.AbsolutePath;
            Name = Path.GetFileName(FullPath);
            this.storageFile = storageFile;
        }
        #region Fields & Properties

        private readonly IStorageFile storageFile;

        public string Name { get; }
        public string FullPath { get; }
        bool IItemEntry.IsDirectory => false;

        bool IItemEntry.IsFile => true;
        #endregion
        public byte[] GetBuffer()
        {
            using Stream stream = GetRead();
            if (stream is MemoryStream ms)
            {
                return ms.ToArray();
            }

            using MemoryStream msBuffer = new MemoryStream();
            stream.CopyTo(msBuffer);
            return msBuffer.ToArray();
        }
        public Stream GetRead()
        {
            return storageFile.OpenReadAsync().GetAwaiter().GetResult();
        }
        public Stream GetWrite()
        {
            return storageFile.OpenWriteAsync().GetAwaiter().GetResult();
        }



        public void SaveAs(Stream stream)
        {
            using Stream sourcestream = GetRead();
            sourcestream.CopyTo(stream);
            stream.Flush();
            stream.Dispose();
        }


    }
}