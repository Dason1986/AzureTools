using AzureTools.SDK.KeyVaults;

using System.Collections.Generic;
using System.IO;

namespace AzureTools.Services.KeyVaults
{
    public interface IKeyVaultExportFiles
    {
        void CompareToXlsx(IEnumerable<SecretCompareReport> dataSource, Stream stream, string filePath, List<string> envNames);
        void ToXlsx(IEnumerable<SecretItem> dataSource, Stream stream, string filePath);
        void HistoriesToXlsx(IEnumerable<SecretItem> dataSource, Stream stream, string filePath);
    }
}
