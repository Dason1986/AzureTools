using AzureTools.SDK;
using AzureTools.SDK.Models;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Azure.Properties;
using Toolkit.Modules.Azure.Stores;
using Toolkit.Modules.Models;
using Toolkit.Modules.Stores;


namespace Toolkit.Modules.Azure.ViewModels.AppContainers
{
    public partial class AppContainerEnvManageViewModel : ManageViewModel<AppContainerEnv>
    {
        #region Constructors

        public AppContainerEnvManageViewModel(IAppContainerStore appContainerStore, IToolkitStore azureToolsStore, IDialogService dialogService, IAzureResourceFactory azureResourceFactory) : base(dialogService)
        {
            _appContainerStore = appContainerStore;
            _azureToolsStore = azureToolsStore;

            _azureResourceFactory = azureResourceFactory;
            CanClose = HandleCloseAsync;
            LoadItems();
        }

        #endregion

        #region Fields & Properties

        private readonly IAppContainerStore _appContainerStore;
        private readonly IToolkitStore _azureToolsStore;

        private readonly IAzureResourceFactory _azureResourceFactory;



        #endregion

        private void LoadItems()
        {
            Items = new ObservableCollection<AppContainerEnv>(_appContainerStore.GetOptions());
            HasChanges = false;
        }

        protected override async Task RefreshAsync()
        {
            if (HasChanges)
            {
                bool result = await DialogService.ShowConfirmAsync(AzureResource.ConfirmSaveUnsavedChanges, AzureResource.TitleUnsavedChanges);
                if (result)
                {
                    await SaveAsync();
                }
                else
                {
                    _appContainerStore.DiscardChanges();
                }
            }
            LoadItems();
        }

        protected override async Task AddAsync()
        {
            AppContainerEnv item = await EditOptionAsync(null);
            if (item == null) return;

            if (string.IsNullOrWhiteSpace(item.EnvironmentName))
            {
                await DialogService.ShowInfoAsync(AzureResource.EnvNameCannotBeEmpty);
                return;
            }

            if (Items.Any(x => x.EnvironmentName.Equals(item.EnvironmentName, StringComparison.OrdinalIgnoreCase)))
            {
                await DialogService.ShowInfoAsync(string.Format(AzureResource.EnvAlreadyExists, item.EnvironmentName));
                return;
            }

            _appContainerStore.AddOption(item);
            LoadItems();
            HasChanges = true;
        }

        protected override async Task EditAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(AzureResource.SelectItemToEdit);
                return;
            }

            AppContainerEnv item = await EditOptionAsync(SelectedItem);
            if (item == null) return;

            _appContainerStore.UpdateOption(item);
            LoadItems();
            HasChanges = true;
        }

        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(AzureResource.SelectItemToDelete);
                return;
            }

            bool confirm = await DialogService.ShowConfirmAsync(string.Format(AzureResource.ConfirmDeleteEnv, SelectedItem.Name));
            if (!confirm) return;

            _appContainerStore.RemoveOption(SelectedItem);
            LoadItems();
            HasChanges = true;
        }

        protected override async Task SaveAsync()
        {
            _appContainerStore.Save();
            await DialogService.ShowInfoAsync(AzureResource.SaveSuccess);
            HasChanges = false;
        }

        protected override Task DiscardChangesAsync()
        {
            _appContainerStore.DiscardChanges();
            LoadItems();
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ImportAsync()
        {
            try
            {
                TenantProfile[] tenants = _azureToolsStore.GetTenants();
                if (tenants.Length == 0) return;

                TenantProfile tenant = await SelectTenantAsync(tenants);
                if (tenant == null)
                {
                    await DialogService.ShowInfoAsync(AzureResource.NoTenantSelected);
                    return;
                }

                AppContainerAzureEnv[] sdkEnvs = await _azureResourceFactory.GetAllAppContainersAsync(tenant.ToSdk());
                AppContainerEnv[] environments = sdkEnvs.Select(e => e.ToDomain()).ToArray();
                if (environments.Length == 0)
                {
                    await DialogService.ShowInfoAsync(AzureResource.NoContainerRegistryFound);
                    return;
                }

                IList<AppContainerEnv> selectedEnvironments = await SelectEnvironmentsAsync(environments);
                if (selectedEnvironments == null || selectedEnvironments.Count == 0) return;

                int importedCount = 0;
                int updatedCount = 0;
                foreach (AppContainerEnv env in selectedEnvironments)
                {
                    env.TenantId = tenant.TenantId;
                    AppContainerEnv existing = Items.FirstOrDefault(x => x.EnvironmentName.Equals(env.EnvironmentName, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.SubscriptionId = env.SubscriptionId;
                        existing.SubscriptionName = env.SubscriptionName;
                        existing.ResourceGroupName = env.ResourceGroupName;
                        existing.CreatedOn = env.CreatedOn;
                        _appContainerStore.UpdateOption(existing);
                        updatedCount++;
                    }
                    else
                    {
                        _appContainerStore.AddOption(env);
                        importedCount++;
                    }
                }

                LoadItems();
                HasChanges = true;
                await DialogService.ShowInfoAsync(string.Format(AzureResource.ImportSuccess, importedCount, updatedCount));
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(string.Format(AzureResource.ImportFailed, ex.Message));
            }
        }

        protected override async Task<AppContainerEnv> EditOptionAsync(AppContainerEnv item)
        {
            AppContainerEnvEditorViewModel editViewModel = await ShowDialog<AppContainerEnvEditorViewModel>(new AppContainerEnvEditorOption { Option = item, IsEditMode = item != null });
            return editViewModel.DialogResult == true ? editViewModel.GetItem() : null;
        }

        protected override async Task<bool> HandleCloseAsync()
        {
            if (!HasChanges) return true;
            bool save = await DialogService.ShowConfirmAsync(AzureResource.ConfirmSaveUnsavedChanges, AzureResource.TitleUnsavedChanges);
            if (save)
            {
                _appContainerStore.Save();
            }
            else
            {
                _appContainerStore.DiscardChanges();
            }
            return true;
        }

        private async Task<TenantProfile> SelectTenantAsync(TenantProfile[] tenants)
        {
            if (tenants.Length == 1) return tenants[0];

            IList<TenantProfile> selected = await DialogService.SelectItemsAsync<TenantProfile>(
                tenants, false, AzureResource.SelectTenantToImportEnv,
                new Dictionary<string, string> { { "Name", AzureResource.TenantColumnName }, { "Code", AzureResource.TenantCodeName } });
            return selected?.FirstOrDefault();
        }

        private async Task<IList<AppContainerEnv>> SelectEnvironmentsAsync(AppContainerEnv[] environments)
        {
            if (environments.Length == 1) return environments.ToList();

            Dictionary<string, string> propertyMappings = new Dictionary<string, string>
            {
                ["Name"] = AzureResource.LblName,
                ["SubscriptionId"] = AzureResource.LblSubscriptionId
            };

            return await DialogService.SelectItemsAsync(
                environments.ToList(), true,
                AzureResource.SelectContainerRegistryToImport,
                propertyMappings);
        }
    }
}
