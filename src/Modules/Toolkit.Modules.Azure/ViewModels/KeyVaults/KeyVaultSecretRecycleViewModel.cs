using AzureTools.SDK.KeyVaults;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Properties;

namespace Toolkit.Modules.Azure.ViewModels.KeyVaults
{
    public partial class KeyVaultSecretRecycleViewModel : ViewModel
    {
        #region Constructors

        public KeyVaultSecretRecycleViewModel(KeyVaultSecretRecycleOption option, IDialogService dialogService, ILogger<KeyVaultSecretRecycleViewModel> logger)
        {
            _keyVault = option.KeyVault;
            _dialogService = dialogService;
            _logger = logger;
            EnvironmentName = option.EnvironmentName;
        }

        #endregion

        #region Fields & Properties

        private readonly IKeyVaultManagement _keyVault;
        private readonly IDialogService _dialogService;
        private readonly ILogger<KeyVaultSecretRecycleViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<SecretItem> _deletedSecrets = new();

        [ObservableProperty]
        private SecretItem _selectedSecretItem;

        public string EnvironmentName { get; }

        public string WindowTitle => string.Format(FormResources.TitleKeyVaultRecycleForm, EnvironmentName);

        #endregion

        [RelayCommand]
        private async Task RefreshenAsync()
        {
            try
            {
                List<SecretItem> deletedKeys = await _keyVault.GetDeletedKeys();
                DeletedSecrets = new ObservableCollection<SecretItem>(deletedKeys.OrderBy(x => x.Name));
                SelectedSecretItem = DeletedSecrets.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取回收站密钥列表失败，环境：{Environment}", EnvironmentName);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (SelectedSecretItem == null)
            {
                await _dialogService.ShowInfoAsync(MessageResources.NoRecordSelected);
                return;
            }

            bool confirmed = await _dialogService.ShowConfirmAsync(
                string.Format(MessageResources.ConfirmDeleteKey, SelectedSecretItem.Name));

            if (!confirmed) return;

            try
            {
                await _keyVault.PurgeDeletedSecretAsync(SelectedSecretItem.Name);
                DeletedSecrets.Remove(SelectedSecretItem);
                SelectedSecretItem = DeletedSecrets.FirstOrDefault();
                await _dialogService.ShowInfoAsync(MessageResources.UpdateSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "永久删除密钥失败，Key：{Key}，环境：{Environment}", SelectedSecretItem?.Name, EnvironmentName);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [RelayCommand]
        private async Task RecoveryAsync()
        {
            if (SelectedSecretItem == null)
            {
                await _dialogService.ShowInfoAsync(MessageResources.NoRecordSelected);
                return;
            }

            try
            {
                await _keyVault.RecoverDeletedSecretAsync(SelectedSecretItem.Name);
                DeletedSecrets.Remove(SelectedSecretItem);
                SelectedSecretItem = DeletedSecrets.FirstOrDefault();
                await _dialogService.ShowInfoAsync(MessageResources.UpdateSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "还原密钥失败，Key：{Key}，环境：{Environment}", SelectedSecretItem?.Name, EnvironmentName);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
