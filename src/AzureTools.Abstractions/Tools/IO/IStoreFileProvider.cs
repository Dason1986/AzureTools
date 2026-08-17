using System;
using System.Collections.Generic;

namespace AzureTools.Tools.IO
{
    public interface IStoreFileProvider : IDisposable
    {
        byte[] GetBytes(string path);
        void WriteFile(string path, byte[] data);
        void WriteFiles(IDictionary<string, byte[]> files);
        void DeleteFile(string path);
        IEnumerable<string> GetFileNames();
    }
}
