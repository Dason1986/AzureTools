using AzureTools.Tools.IO;
using System.IO;

namespace AzureTools.Tests.Stubs
{
    public class StubFileEntry : IFileEntry
    {
        public StubFileEntry(string name = "test.xlsx", string fullPath = "C:\\test.xlsx")
        {
            Name = name;
            FullPath = fullPath;
        }
        #region Fields & Properties
        public string Name { get; }
        public string FullPath { get; }

        bool IItemEntry.IsDirectory => false;

        bool IItemEntry.IsFile => true;
        #endregion
        public byte[] GetBuffer() => System.Text.Encoding.UTF8.GetBytes(Name);

        public Stream GetWrite() => new MemoryStream();


        public void SaveAs(Stream stream) { }

        public Stream GetRead() => new MemoryStream();
    }
}
