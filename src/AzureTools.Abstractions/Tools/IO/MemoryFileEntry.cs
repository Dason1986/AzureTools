using System;
using System.IO;

namespace AzureTools.Tools.IO
{
    public class MemoryFileEntry : IFileEntry
    {
        #region Constructors

        public MemoryFileEntry(string name, byte[] buffer)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(buffer);
            Name = name;
            FullPath = name;
            this.buffer = buffer;
        }

        public MemoryFileEntry(string name, string content) : this(name, System.Text.Encoding.UTF8.GetBytes(content))
        {
        }

        #endregion

        #region Fields & Properties

        public string Name { get; private set; }
        public string FullPath { get; private set; }

        readonly private byte[] buffer;
        bool IItemEntry.IsDirectory => false;

        bool IItemEntry.IsFile => true;
        #endregion

        public byte[] GetBuffer()
        {
            return buffer;
        }

        public Stream GetWrite()
        {
            return new MemoryStream(buffer);
        }



        public void SaveAs(Stream stream)
        {
            stream.Write(buffer);
            stream.Flush();
            stream.Dispose();
        }

        public Stream GetRead()
        {
            return new MemoryStream(buffer);
        }


    }
}
