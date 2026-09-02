using AzureTools.Properties;

using AzureTools.Services;
using AzureTools.Services.KeyVaults;
using AzureTools.Tools.IO;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.Module.Azure.ViewModels.KeyVaults
{
    public partial class KeyVaultSecretCompareViewModel : ViewModel
    {
        #region Constructors

        public KeyVaultSecretCompareViewModel(KeyVaultSecretCompareOption option, IDialogService dialogService, IKeyVaultExportFiles exportFiles)
        {
            _envNames = option.EnvNames;
            source = option.Results;
            _results = new ObservableCollection<SecretCompareReport>(source);
            _dialogService = dialogService;
            _exportFiles = exportFiles;
            UpdateSummary();
        }

        #endregion

        #region Fields & Properties

        private readonly IDialogService _dialogService;
        private readonly IKeyVaultExportFiles _exportFiles;
        private readonly IList<SecretCompareReport> source;

        [ObservableProperty]
        private List<string> _envNames;

        [ObservableProperty]
        private ObservableCollection<SecretCompareReport> _results;

        [ObservableProperty]
        private SecretCompareReport _selectedCompareResult;

        [ObservableProperty]
        private string _summaryText;

        #endregion

        [RelayCommand]
        private void ShowAll()
        {
            Results = new ObservableCollection<SecretCompareReport>(source);
        }

        [RelayCommand]
        private void ShowDiff()
        {
            Results = new ObservableCollection<SecretCompareReport>(source.Where(x => !x.GetIsSame()));
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            string envName = string.Join("_vs_", EnvNames);
            string defaultFileName = $"Compare_{envName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            IFileEntry fileEntry = await _dialogService.SaveFileAsync(ModelResources.ExcelFileFilter, defaultFileName);
            if (fileEntry == null) return;

            using Stream stream = fileEntry.GetWrite();
            _exportFiles.CompareToXlsx(Results, stream, fileEntry.FullPath, EnvNames);
            await _dialogService.OpenWithExplorerAsync(fileEntry.FullPath);
        }

        private void UpdateSummary()
        {
            int allExistCount = Results.Count(r => r.Status == CompareStatus.AllExist);
            int partialCount = Results.Count(r => r.Status == CompareStatus.Partial);
            SummaryText = string.Format(MessageResources.CompareSummaryFormat, Results.Count, allExistCount, partialCount);
        }
    }
}
