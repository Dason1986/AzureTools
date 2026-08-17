using System.IO;

namespace AzureTools.Tools.IO
{
    public interface IFileEntry : IItemEntry
    {
        byte[] GetBuffer();
        Stream GetWrite();
        Stream GetRead();



        void SaveAs(Stream stream);
    }
}
