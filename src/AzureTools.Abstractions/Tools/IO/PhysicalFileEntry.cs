using System;
using System.IO;

namespace AzureTools.Tools.IO
{

    public class PhysicalFileEntry : IFileEntry
    {
        public PhysicalFileEntry(string fullPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath);
        }
        #region Fields & Properties
        public string Name { get; }
        public string FullPath { get; }

        bool IItemEntry.IsDirectory => false;

        bool IItemEntry.IsFile => true;
        #endregion
        public byte[] GetBuffer()
        {
            return File.ReadAllBytes(FullPath);
        }

        public Stream GetRead()
        {
            return new FileStream(FullPath, FileMode.Open, FileAccess.Read);
        }

        public Stream GetWrite()
        {
            return new FileStream(FullPath, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public void SaveAs(string path)
        {
            File.Copy(FullPath, path, true);
        }

        public void SaveAs(Stream stream)
        {
            using Stream fileStream = GetWrite();
            fileStream.CopyTo(stream);
        }


    }
}
