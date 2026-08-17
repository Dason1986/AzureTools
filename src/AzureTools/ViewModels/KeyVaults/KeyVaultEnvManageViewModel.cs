using AzureTools.Extensions;
using AzureTools.Models;
using AzureTools.Properties;
using AzureTools.SDK;
using AzureTools.Services;
using AzureTools.Stores;
using AzureTools.Views.KeyVaults;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace AzureTools.ViewModels.KeyVaults
{
    public partial class KeyVaultEnvManageViewModel : ManageViewModel<KeyVaultEnv>
    {
        #region Constructors

        public KeyVaultEnvManageViewModel(IKeyVaultStore keyVaultStore, IAzureToolsStore azureToolsStore, IDialogService dialogService, IAzureResourceFactory azureResourceFactory):base(dialogService)
        {
            _keyVaultStore = keyVaultStore;
            this.azureToolsStore = azureToolsStore;
            _azureResourceFactory = azureResourceFactory;
            CanClose = HandleCloseAsync;
            LoadItems();
        }

        #endregion

        #region Fields & Properties

        private readonly IKeyVaultStore _keyVaultStore;
        private readonly IAzureToolsStore azureToolsStore;
        private readonly IAzureResourceFactory _azureResourceFactory;

      

        #endregion

        private void LoadItems()
        {
            Items = new ObservableCollection<KeyVaultEnv>(_keyVaultStore.GetOptions());
            HasChanges = false;
        }


        protected override async Task RefreshAsync()
        {
            if (HasChanges)
            {
                bool result = await DialogService.ShowConfirmAsync(MessageResources.ConfirmSaveUnsavedChanges, MessageResources.TitleUnsavedChanges);
                if (result)
                {
                    await SaveAsync();
                }
                else
                {
                    _keyVaultStore.DiscardChanges();
                }
            }
            LoadItems();
        }


        protected override async Task AddAsync()
        {
            KeyVaultEnv item = await EditOptionAsync(null);
            if (item == null) return;

            if (string.IsNullOrWhiteSpace(item.Name))
            {
                await DialogService.ShowInfoAsync(MessageResources.EnvNameCannotBeEmpty);
                return;
            }

            if (Items.Any(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                await DialogService.ShowInfoAsync(string.Format(MessageResources.EnvAlreadyExists, item.Name));
                return;
            }

            _keyVaultStore.AddOption(item);
            LoadItems();
            HasChanges = true;
        }


        protected override async Task EditAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(MessageResources.SelectItemToEdit);
                return;
            }

            KeyVaultEnv item = await EditOptionAsync(SelectedItem);
            if (item == null) return;

            _keyVaultStore.UpdateOption(item);
            LoadItems();
            HasChanges = true;
        }


        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(MessageResources.SelectItemToDelete);
                return;
            }

            bool confirm = await DialogService.ShowConfirmAsync(string.Format(MessageResources.ConfirmDeleteEnv, SelectedItem.Name));
            if (!confirm) return;

            _keyVaultStore.RemoveOption(SelectedItem);
            LoadItems();
            HasChanges = true;
        }


        protected override async Task SaveAsync()
        {
            _keyVaultStore.Save();
            await DialogService.ShowInfoAsync(MessageResources.SaveSuccess);
            HasChanges = false;
        }


        protected override Task DiscardChangesAsync()
        {
            _keyVaultStore.DiscardChanges();
            LoadItems();
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ImportAsync()
        {
            try
            {
                TenantProfile[] tenants = azureToolsStore.GetTenants();
                TenantProfile tenant = null;
                if (tenants.Length > 1)
                {
                    IList<TenantProfile> selected = await DialogService.SelectItemsAsync<TenantProfile>(tenants, false, MessageResources.SelectTenantToImportEnv, new Dictionary<string, string>() { { "Name", ModelResources.TenantColumnName }, { "Code", ModelResources.TenantCodeName } });
                    tenant = selected?.FirstOrDefault();
                }
                else
                {
                    tenant = tenants[0];
                }
                if (tenant == null)
                {
                    await DialogService.ShowInfoAsync(MessageResources.NoTenantSelected);
                    return;
                }
                SDK.Models.KeyVaultAzureEnv[] sdkVaults = await _azureResourceFactory.GetAllKeyVaultAsync(tenant.ToSdk());
                KeyVaultEnv[] vaults = sdkVaults.Select(v => v.ToDomain()).ToArray();
                if (vaults.Length == 0)
                {
                    await DialogService.ShowInfoAsync(MessageResources.NoKeyVaultFound);
                    return;
                }

                IList<KeyVaultEnv> selectedVaults = await SelectRegistriesAsync(vaults);
                if (selectedVaults == null || selectedVaults.Count == 0) return;

                int importedCount = 0;
                int updatedCount = 0;
                foreach (KeyVaultEnv vault in selectedVaults)
                {
                    vault.TenantId = tenant.TenantId;
                    KeyVaultEnv existing = Items.FirstOrDefault(x => x.KeyVault.Equals(vault.KeyVault, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.KeyVault = vault.KeyVault;
                        existing.KeyVaultUrl = vault.KeyVaultUrl;
                        existing.CreatedOn = vault.CreatedOn;
                        existing.EnableSoftDelete = vault.EnableSoftDelete;
                        existing.SubscriptionName = vault.SubscriptionName;
                        existing.SubscriptionId = vault.SubscriptionId;
                        existing.ResourceGroupName = vault.ResourceGroupName;
                        _keyVaultStore.UpdateOption(existing);
                        updatedCount++;
                    }
                    else
                    {
                        vault.Name = vault.KeyVault;
                        _keyVaultStore.AddOption(vault);
                        importedCount++;
                    }
                }

                LoadItems();
                HasChanges = true;
                await DialogService.ShowInfoAsync(string.Format(MessageResources.ImportSuccess, importedCount, updatedCount));
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(string.Format(MessageResources.ImportFailed, ex.Message));
            }
        }


        protected override async Task<KeyVaultEnv> EditOptionAsync(KeyVaultEnv item )
        {
            KeyVaultEnvEditorViewModel editViewModel = await ShowDialog<KeyVaultEnvEditorView, KeyVaultEnvEditorViewModel>(new KeyVaultEnvEditorOption { Option = item, IsEditMode = item!=null });
            return editViewModel.DialogResult == true ? editViewModel.GetItem() : null;
        }

        protected override  async Task<bool> HandleCloseAsync()
        {
            if (!HasChanges) return true;
            bool save = await DialogService.ShowConfirmAsync(MessageResources.ConfirmSaveUnsavedChanges, MessageResources.TitleUnsavedChanges);
            if (save)
            {
                _keyVaultStore.Save();
            }
            else
            {
                _keyVaultStore.DiscardChanges();
            }
            return true;
        }

        private async Task<IList<KeyVaultEnv>> SelectRegistriesAsync(KeyVaultEnv[] vaults)
        {
            if (vaults.Length <= 1) return Array.Empty<KeyVaultEnv>();

            Dictionary<string, string> propertyMappings = new Dictionary<string, string>
            {
                ["KeyVault"] = ModelResources.KeyVaultColumnName,
                ["KeyVaultUrl"] = ModelResources.KeyVaultUrlName,
                ["SubscriptionId"] = ModelResources.SubscriptionIdName
            };
            return await DialogService.SelectItemsAsync(
                   vaults,
                   true,
                   MessageResources.SelectKeyVaultToImport,
                   propertyMappings);
        }
    }
}
