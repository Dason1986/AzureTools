using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace AzureTools.Tests.IO
{
    internal static class ZipFileCreator
    {
        internal static void CreateEmptyZip(string path)
        {
            using FileStream stream = File.Create(path);
            using ZipOutputStream zipStream = new ZipOutputStream(stream);
            zipStream.Finish();
        }

        internal static void CreateEncryptedZip(string path, string password)
        {
            using FileStream stream = File.Create(path);
            using ZipOutputStream zipStream = new ZipOutputStream(stream);
            zipStream.Password = password;
            zipStream.Finish();
        }
    }
}
