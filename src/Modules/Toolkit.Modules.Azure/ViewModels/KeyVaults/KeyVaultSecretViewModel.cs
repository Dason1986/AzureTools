using AzureTools.SDK.KeyVaults;
using AzureTools.Services;
using AzureTools.Tools.IO;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Azure.KeyVaults;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Azure.Models.KeyVaults;
using Toolkit.Modules.Azure.Properties;
using Toolkit.Modules.Azure.Stores;
using Toolkit.Modules.Models;

namespace Toolkit.Modules.Azure.ViewModels.KeyVaults
{
    public partial class KeyVaultSecretViewModel : ViewModel
    {
        #region Constructors

        public KeyVaultSecretViewModel(KeyVaultEnv envOption, ProjectInfo projectInfo, IKeyVaultManagement keyVault, IKeyVaultStore keyVaultStore, IDialogService dialogService, IKeyVaultExportFiles exportFiles, ILogger<KeyVaultSecretViewModel> logger)
        {
            this._envOption = envOption;
            this.projectInfo = projectInfo;
            KeyVault = keyVault;
            _keyVaultStore = keyVaultStore;
            _dialogService = dialogService;
            _exportFiles = exportFiles;
            _logger = logger;
            WindowTitle = string.Format(AzureResource.TitleKeyVaultWindow, envOption.Name);
            SelectedSecretItem = new SecretItemModel();
        }

        #endregion

        #region Fields & Properties

        private readonly KeyVaultEnv _envOption;
        private readonly ProjectInfo projectInfo;
        private readonly IKeyVaultStore _keyVaultStore;
        private readonly IDialogService _dialogService;
        private readonly IKeyVaultExportFiles _exportFiles;
        private readonly ILogger<KeyVaultSecretViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<SecretItemModel> _keyVaults = new();

        [ObservableProperty]
        private ObservableCollection<SecretItemModel> _filteredKeyVaults = new();

        [ObservableProperty]
        private SecretItemModel _selectedSecretItem;

        [ObservableProperty]
        private string _currentValue;

        [ObservableProperty]
        private DateTime? _expirationDate;

        [ObservableProperty]
        private string _filterText;

        public string EnvironmentName => _envOption.Name;
        public IKeyVaultManagement KeyVault { get; }



        #endregion
        [RelayCommand]
        private async Task CerFileAsync()
        {
            await ShowDialogKeyed("Cer");
        }
        [RelayCommand]
        private async Task RsaFileAsync()
        {
            await ShowDialogKeyed("Rsa");
        }
        [RelayCommand]
        private async Task GetAllAsync()
        {
            SecretKeyOptionItem[] req = _keyVaultStore.GetRequiredKeys(projectInfo.Id);
            List<SecretItemModel> keys = (await KeyVault.GetKeys()).Select(x => SecretItemModel.FromModel(x)).ToList();
            Task[] tasks = keys.Select(async (item) =>
            {
                string val = await KeyVault.Get(item.Name);
                SecretKeyOptionItem matchedKey = req.FirstOrDefault(x => x.Key == item.Name);

                if (matchedKey != null)
                {
                    item.Required = matchedKey.Required;
                    item.Description = matchedKey.Description;
                    item.Tags = matchedKey.Tags;
                    item.Validation = matchedKey.Option;
                }
                else
                {
                    item.Tags = Array.Empty<string>();
                }
                item.Value = val;
            }).ToArray();

            await Task.WhenAll(tasks);

            keys.AddRange(from item in req
                          where !keys.Any(x => x.Name == item.Key)
                          select new SecretItemModel
                          {
                              Name = item.Key,
                              Required = item.Required,
                              Description = item.Description,
                              Tags = item.Tags,
                              Validation = item.Option
                          });

            KeyVaults = new ObservableCollection<SecretItemModel>(keys.OrderBy(x => x.Name));
            ApplyFilter();
        }

        partial void OnFilterTextChanged(string value) => ApplyFilter();

        private void ApplyFilter()
        {
            IEnumerable<SecretItemModel> source = KeyVaults;
            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                string filter = FilterText.Trim();
                source = KeyVaults.Where(x =>
                    (x.Name != null && x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Value != null && x.Value.Contains(filter, StringComparison.OrdinalIgnoreCase)));
            }
            FilteredKeyVaults = new ObservableCollection<SecretItemModel>(source);
            SelectedSecretItem = FilteredKeyVaults.FirstOrDefault();
        }

        [RelayCommand]
        private async Task UpdateAsync()
        {
            if (SelectedSecretItem == null)
            {
                await _dialogService.ShowInfoAsync(AzureResource.NoRecordSelected);
                return;
            }
            if (SelectedSecretItem.Value == CurrentValue)
            {
                await _dialogService.ShowInfoAsync(AzureResource.NoModification);
                return;
            }
            if (!ValidateForUpdate()) return;

            string key = SelectedSecretItem.Name;
            if (string.IsNullOrEmpty(key)) key = SelectedSecretItem.Name;

            try
            {
                await KeyVault.Set(key, CurrentValue);
                SelectedSecretItem.Value = CurrentValue;
                await _dialogService.ShowInfoAsync(AzureResource.UpdateSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新密钥失败，Key：{Key}，环境：{Environment}", key, _envOption.Name);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
            finally
            {
                SelectedSecretItem = null;
                CurrentValue = string.Empty;
            }
        }

        [RelayCommand]
        private async Task ValidatorAsync()
        {
            if (KeyVaults == null || KeyVaults.Count == 0)
            {
                await GetAllAsync();
            }

            try
            {
                List<string> errorKeys = new List<string>();
                SecretKeyOptionItem[] req = _keyVaultStore.GetRequiredKeys(projectInfo.Id);

                foreach (SecretKeyOptionItem keyname in req)
                {
                    if (KeyVaults.Any(x => x.Name == keyname.Key && string.IsNullOrEmpty(x.Value) && x.Required))
                        errorKeys.Add(keyname.Key);
                }

                foreach (SecretItemModel item in KeyVaults.Where(x => x.Validation != null && !string.IsNullOrEmpty(x.Value)))
                {
                    try
                    {
                        bool valid = KeyValidatorProvider.Default.Validate(item.Validation, item.Value, KeyVaults.ToList());
                        if (!valid)
                            errorKeys.Add($"{item.Name}: {KeyValidatorProvider.Default.GetErrorMessage(item.Validation)}");
                    }
                    catch (NotSupportedException ex)
                    {
                        errorKeys.Add($"{item.Name}: {ex.Message}");
                    }
                }

                if (errorKeys.Count > 0)
                    throw new KeyVaultMissException(errorKeys);

                await _dialogService.ShowInfoAsync(AzureResource.NoMissingConfig, AzureResource.TitleValidationResult);
            }
            catch (KeyVaultMissException ex)
            {
                string miss = string.Join("\r\n", ex.MissKeys);
                _logger.LogWarning("校验发现缺失项，环境：{Environment}，缺失：{MissKeys}", _envOption.Name, miss);
                await _dialogService.ShowInfoAsync(miss, AzureResource.TitleMissingItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "校验失败，环境：{Environment}", _envOption.Name);
                await _dialogService.ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand]
        private async Task HistoryAsync()
        {
            if (SelectedSecretItem == null)
            {
                await _dialogService.ShowInfoAsync(AzureResource.NoRecordSelected);
                return;
            }

            try
            {
                List<SecretItem> histories = await KeyVault.GetKeyVersion(SelectedSecretItem);
                if (histories.Count > 1)
                {
                    KeyVaultSecretHistoriesOption historyOption = new KeyVaultSecretHistoriesOption { Secrets = histories, EnvironmentName = _envOption.Name };
                    await ShowDialog<KeyVaultSecretHistoriesViewModel>(historyOption);
                }
                else
                {
                    await _dialogService.ShowInfoAsync(AzureResource.NoMultipleVersions);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取历史版本失败，Key：{Key}，环境：{Environment}", SelectedSecretItem?.Name, _envOption.Name);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [RelayCommand]
        private async Task SetAllExpirationAsync()
        {
            if (KeyVaults == null || KeyVaults.Count == 0 || !ExpirationDate.HasValue) return;

            try
            {
                foreach (SecretItem item in KeyVaults)
                {
                    if (item.Created == null) continue;
                    await KeyVault.SetExpiration(item.Name, ExpirationDate.Value);
                }
                await _dialogService.ShowInfoAsync(AzureResource.BatchSetExpirationSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量设置过期时间失败，环境：{Environment}", _envOption.Name);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [RelayCommand]
        private async Task SetOneExpirationAsync()
        {
            if (SelectedSecretItem == null)
            {
                await _dialogService.ShowInfoAsync(AzureResource.NoRecordSelected);
                return;
            }

            if (SelectedSecretItem.Created == null)
            {
                await _dialogService.ShowInfoAsync(AzureResource.RecordNotInKeyVault);
                return;
            }
            if (!ExpirationDate.HasValue)
            {
                await _dialogService.ShowInfoAsync(AzureResource.RecordNotInKeyVault);
                return;
            }

            try
            {
                await KeyVault.SetExpiration(SelectedSecretItem.Name, ExpirationDate.Value);
                await _dialogService.ShowInfoAsync(AzureResource.UpdateSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置单个过期时间失败，Key：{Key}，环境：{Environment}", SelectedSecretItem?.Name, _envOption.Name);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            if (KeyVaults == null || KeyVaults.Count == 0)
            {
                await _dialogService.ShowInfoAsync(AzureResource.GetConfigDataFirst);
                return;
            }

            string defaultFileName = $"KeyVault_{_envOption.Name}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            IFileEntry fileEntry = await _dialogService.SaveFileAsync(AzureResource.ExcelFileFilter, defaultFileName);
            if (fileEntry == null) return;

            using System.IO.Stream stream = fileEntry.GetWrite();
            _exportFiles.ToXlsx(KeyVaults, stream, fileEntry.FullPath);
        }

        [RelayCommand]
        private async Task RecycleAsync()
        {
            KeyVaultSecretRecycleOption recycleOption = new KeyVaultSecretRecycleOption { KeyVault = KeyVault, EnvironmentName = _envOption.Name };
            await ShowDialog<KeyVaultSecretRecycleViewModel>(recycleOption);
        }

        private bool ValidateForUpdate()
        {
            if (SelectedSecretItem?.Validation == null) return true;

            try
            {
                bool valid = KeyValidatorProvider.Default.Validate(
                    SelectedSecretItem.Validation,
                    CurrentValue,
                    KeyVaults.ToList());

                if (valid) return true;
                _ = _dialogService.ShowErrorAsync(KeyValidatorProvider.Default.GetErrorMessage(SelectedSecretItem.Validation), AzureResource.TitleValidationFailed);
                return false;
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "校验不支持的验证类型，Key：{Key}，环境：{Environment}", SelectedSecretItem?.Name, _envOption.Name);
                _ = _dialogService.ShowErrorAsync(ex.Message);
                return false;
            }
        }

        partial void OnSelectedSecretItemChanged(SecretItemModel value)
        {
            CurrentValue = value?.Value ?? string.Empty;
        }
    }
}
