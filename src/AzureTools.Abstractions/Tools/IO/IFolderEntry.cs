using System.Collections.Generic;

namespace AzureTools.Tools.IO
{
    public interface IFolderEntry : IItemEntry
    {
        IEnumerable<IItemEntry> GetItems();

        IFileEntry GetFile(string name);
        IFileEntry CreateFile(string name);
        IFolderEntry GetFolder(string name);
        IFolderEntry CreateFolder(string name);
    }
}
