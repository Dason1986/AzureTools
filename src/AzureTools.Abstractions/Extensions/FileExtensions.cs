using AzureTools.Tools.IO;
using System.IO;
using System.Text;

namespace AzureTools.Extensions
{
    public static class FileExtensions
    {

        public static string GetAllText(this IFileEntry fileEntry, Encoding encoding = null)
        {
            if (fileEntry == null) return string.Empty;
            byte[] buffer = fileEntry.GetBuffer();
            Encoding currnet = encoding ?? Encoding.UTF8;
            return currnet.GetString(buffer);
        }
        public static void WriteAllText(this IFileEntry fileEntry, string content, Encoding encoding = null)
        {
            using Stream stream = fileEntry.GetWrite();
            if (stream.CanWrite)
            {
                stream.SetLength(0);
                stream.Position = 0;
                Encoding currnet = encoding ?? Encoding.UTF8;
                byte[] buffer = currnet.GetBytes(content);
                stream.Write(buffer);
                stream.Flush();

            }
        }
    }
}
