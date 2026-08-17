using Avalonia.Platform.Storage;
using AzureTools.Tools.IO;
using System.Collections.Generic;
using System.IO;

namespace AzureTools.Services
{
    class AvaloniaFolderEntry : IFolderEntry
    {
        public AvaloniaFolderEntry(IStorageFolder storageFolder)
        {

            FullPath = storageFolder.Path.AbsolutePath;
            Name = Path.GetFileName(FullPath);
            this.storageFolder = storageFolder;
        }
        #region Fields & Properties

        private readonly IStorageFolder storageFolder;

        public string Name { get; }
        public string FullPath { get; }
        bool IItemEntry.IsDirectory => true;

        bool IItemEntry.IsFile => false;

        public IEnumerable<IItemEntry> GetItems()
        {
            IAsyncEnumerator<IStorageItem> arr = storageFolder.GetItemsAsync().GetAsyncEnumerator();
            while (arr.MoveNextAsync().GetAwaiter().GetResult())
            {
                IStorageItem current = arr.Current;
                if (current is IStorageFile file) yield return new AvaloniaFileEntry(file);
                if (current is IStorageFolder folder) yield return new AvaloniaFolderEntry(folder);
            }

        }

        public IFileEntry GetFile(string name)
        {
            IStorageFile file = storageFolder.GetFileAsync(name).GetAwaiter().GetResult();
            if (file == null) throw new FileNotFoundException(name);
            return new AvaloniaFileEntry(file);
        }

        public IFileEntry CreateFile(string name)
        {
            IStorageFile file = storageFolder.CreateFileAsync(name).GetAwaiter().GetResult();
            if (file == null) throw new FileNotFoundException(name);
            return new AvaloniaFileEntry(file);
        }

        public IFolderEntry GetFolder(string name)
        {
            IStorageFolder folder = storageFolder.GetFolderAsync(name).GetAwaiter().GetResult();
            if (folder == null) throw new DirectoryNotFoundException(name);
            return new AvaloniaFolderEntry(folder);
        }

        public IFolderEntry CreateFolder(string name)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}