using AzureTools.SDK.KeyVaults;

using AzureTools.Tools.IO;
using System.Collections.Generic;
using System.IO;

namespace AzureTools.Services.KeyVaults
{
    public class KeyVaultExportFiles : IKeyVaultExportFiles
    {
        #region Constructors

        public KeyVaultExportFiles(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        #endregion

        #region Fields & Properties

        private readonly IDialogService _dialogService;

        #endregion

        public void CompareToXlsx(IEnumerable<SecretCompareReport> dataSource, Stream stream, string filePath, List<string> envNames)
        {
            List<string> headers = new List<string> { "Key名称" };
            foreach (string envName in envNames)
            {
                headers.Add($"{envName} 状态");
                headers.Add($"{envName} 值");
            }

            ExcelExporter.Export(stream, "比较结果", headers.ToArray(), dataSource, (result, header) =>
            {
                if (header.Equals("Key名称")) return result.KeyName;

                foreach (string envName in envNames)
                {
                    if (header.Equals($"{envName} 状态"))
                        return result.EnvStatusMap.TryGetValue(envName, out string status) ? status : "";
                    if (header.Equals($"{envName} 值"))
                        return result.EnvValueMap.TryGetValue(envName, out string value) ? value : "";
                }

                return string.Empty;
            });

            _dialogService.OpenWithExplorerAsync(filePath);
        }

        public void ToXlsx(IEnumerable<SecretItem> dataSource, Stream stream, string filePath)
        {
            string[] headers = new[]
            {
                nameof(SecretItem.Name),   nameof(SecretItem.Description), nameof(SecretItem.Tags),   nameof(SecretItem.Value),
                nameof(SecretItem.Version), nameof(SecretItem.Enabled), nameof(SecretItem.Required),
                nameof(SecretItem.Created), nameof(SecretItem.Updated), nameof(SecretItem.Expires)
            };

            ExcelExporter.Export(stream, "KeyVault配置", headers, dataSource, (item, header) =>
            {
                return header switch
                {
                    nameof(SecretItem.Name) => item.Name,
                    nameof(SecretItem.Description) => item.Description,
                    nameof(SecretItem.Tags) => string.Join(" ", item.Tags),
                    nameof(SecretItem.Value) => item.Value,
                    nameof(SecretItem.Version) => item.Version,
                    nameof(SecretItem.Enabled) => item.Enabled.ToString(),
                    nameof(SecretItem.Required) => item.Required.ToString(),
                    nameof(SecretItem.Created) => item.Created?.ToString("yyyy-MM-dd HH:mm:ss"),
                    nameof(SecretItem.Updated) => item.Updated?.ToString("yyyy-MM-dd HH:mm:ss"),
                    nameof(SecretItem.Expires) => item.Expires?.ToString("yyyy-MM-dd"),
                    _ => string.Empty
                };
            });

            _dialogService.OpenWithExplorerAsync(filePath);
        }

        public void HistoriesToXlsx(IEnumerable<SecretItem> dataSource, Stream stream, string filePath)
        {
            string[] headers = new[]
            {
                nameof(SecretItem.Name),   nameof(SecretItem.Value),
                nameof(SecretItem.Version), nameof(SecretItem.Created),
                nameof(SecretItem.Updated), nameof(SecretItem.Expires)
            };

            ExcelExporter.Export(stream, "KeyVault配置", headers, dataSource, (item, header) =>
            {
                return header switch
                {
                    nameof(SecretItem.Name) => item.Name,
                    nameof(SecretItem.Value) => item.Value,
                    nameof(SecretItem.Version) => item.Version,
                    nameof(SecretItem.Created) => item.Created?.ToString("yyyy-MM-dd HH:mm:ss"),
                    nameof(SecretItem.Updated) => item.Updated?.ToString("yyyy-MM-dd HH:mm:ss"),
                    nameof(SecretItem.Expires) => item.Expires?.ToString("yyyy-MM-dd"),
                    _ => string.Empty
                };
            });

            _dialogService.OpenWithExplorerAsync(filePath);
        }
    }
}
