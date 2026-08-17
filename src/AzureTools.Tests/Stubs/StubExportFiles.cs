using AzureTools.SDK.KeyVaults;
using AzureTools.Services.KeyVaults;
using System.Collections.Generic;
using System.IO;

namespace AzureTools.Tests.Stubs
{
    public class StubExportFiles : IKeyVaultExportFiles
    {
        public bool ToXlsxCalled { get; private set; }
        public bool HistoriesToXlsxCalled { get; private set; }
        public bool CompareToXlsxCalled { get; private set; }
        public string LastFilePath { get; private set; }

        public void CompareToXlsx(IEnumerable<SecretCompareReport> dataSource, Stream stream, string filePath, List<string> envNames)
        {
            CompareToXlsxCalled = true;
            LastFilePath = filePath;
        }

        public void ToXlsx(IEnumerable<SecretItem> dataSource, Stream stream, string filePath)
        {
            ToXlsxCalled = true;
            LastFilePath = filePath;
        }

        public void HistoriesToXlsx(IEnumerable<SecretItem> dataSource, Stream stream, string filePath)
        {
            HistoriesToXlsxCalled = true;
            LastFilePath = filePath;
        }
    }
}
