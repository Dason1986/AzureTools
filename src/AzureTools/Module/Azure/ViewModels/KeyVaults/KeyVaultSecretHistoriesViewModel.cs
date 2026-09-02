using AzureTools.Properties;
using AzureTools.SDK.KeyVaults;
using AzureTools.Services;
using AzureTools.Services.KeyVaults;
using AzureTools.Tools.IO;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.Module.Azure.ViewModels.KeyVaults
{
    public partial class KeyVaultSecretHistoriesViewModel : ViewModel
    {
        #region Constructors

        public KeyVaultSecretHistoriesViewModel(KeyVaultSecretHistoriesOption option, IDialogService dialogService, IKeyVaultExportFiles exportFiles)
        {
            _secrets = new ObservableCollection<SecretItem>(option.Secrets);
            _environmentName = option.EnvironmentName;
            _dialogService = dialogService;
            _exportFiles = exportFiles;
            string keyName = option.Secrets.FirstOrDefault()?.Name ?? "Unknown";
            _title = $"KeyVault [{option.EnvironmentName} - {keyName}] Histories";
        }

        #endregion

        #region Fields & Properties

        private readonly IDialogService _dialogService;
        private readonly string _environmentName;
        private readonly IKeyVaultExportFiles _exportFiles;

        [ObservableProperty]
        private ObservableCollection<SecretItem> _secrets;

        [ObservableProperty]
        private string _title;

        #endregion

        [RelayCommand]
        private async Task ExportAsync()
        {
            string defaultFileName = GetExportFileName();
            IFileEntry fileEntry = await _dialogService.SaveFileAsync(ModelResources.ExcelFileFilter, defaultFileName);
            if (fileEntry == null) return;

            using Stream stream = fileEntry.GetWrite();
            _exportFiles.HistoriesToXlsx(Secrets, stream, fileEntry.FullPath);
        }

        private string GetExportFileName()
        {
            string keyName = Secrets.FirstOrDefault()?.Name ?? "Unknown";
            return $"KeyVault_[{_environmentName}_{keyName}]_Histories_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        }
    }
}