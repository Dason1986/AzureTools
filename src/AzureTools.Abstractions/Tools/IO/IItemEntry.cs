namespace AzureTools.Tools.IO
{
    public interface IItemEntry
    {

        string Name { get; }
        string FullPath { get; }

        bool IsDirectory { get; }
        bool IsFile { get; }
    }
}
