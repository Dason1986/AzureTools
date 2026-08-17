using AzureTools.Extensions;
using AzureTools.Models;
using AzureTools.Properties;
using AzureTools.SDK;
using AzureTools.Services;
using AzureTools.Stores;
using AzureTools.Views.ContainerRegistries;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace AzureTools.ViewModels.ContainerRegistries
{
    public partial class ContainerRegistryEnvManageViewModel : ManageViewModel<ContainerRegistryEnv>
    {
        #region Constructors

        public ContainerRegistryEnvManageViewModel(IContainerRegistryStore containerRegistryStore, IAzureToolsStore azureToolsStore, IDialogService dialogService, IAzureResourceFactory azureResourceFactory) : base(dialogService)
        {
            _containerRegistryStore = containerRegistryStore;
            this.azureToolsStore = azureToolsStore;
            _azureResourceFactory = azureResourceFactory;
            CanClose = HandleCloseAsync;

            LoadItems();
        }

        #endregion

        #region Fields & Properties

        private readonly IContainerRegistryStore _containerRegistryStore;
        private readonly IAzureToolsStore azureToolsStore;
        private readonly IAzureResourceFactory _azureResourceFactory;

      

        #endregion


        private void LoadItems()
        {
            Items = new ObservableCollection<ContainerRegistryEnv>(_containerRegistryStore.GetOptions());
            HasChanges = false;
        }
         
        protected override  async Task RefreshAsync()
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
                    _containerRegistryStore.DiscardChanges();
                }
            }
            LoadItems();
        }


        protected override async Task AddAsync()
        {
            ContainerRegistryEnv item = await EditOptionAsync(null);
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

            _containerRegistryStore.AddOption(item);
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

            ContainerRegistryEnv item = await EditOptionAsync(SelectedItem);
            if (item == null) return;

            _containerRegistryStore.UpdateOption(item);
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

            _containerRegistryStore.RemoveOption(SelectedItem);
            LoadItems();
            HasChanges = true;
        }

        
        protected override async Task SaveAsync()
        {
            _containerRegistryStore.Save();
            await DialogService.ShowInfoAsync(MessageResources.SaveSuccess);
            HasChanges = false;
        }


        protected override Task DiscardChangesAsync()
        {
            _containerRegistryStore.DiscardChanges();
            LoadItems();
            return Task.CompletedTask;

        }

        [RelayCommand]
        private async Task ImportAsync()
        {
            try
            {
                TenantProfile[] tenants = azureToolsStore.GetTenants();
                if (tenants.Length == 0) return;

                TenantProfile tenant = await SelectTenantAsync(tenants);
                if (tenant == null)
                {
                    await DialogService.ShowInfoAsync(MessageResources.NoTenantSelected);
                    return;
                }

                SDK.Models.ContainerRegistryAzureEnv[] sdkRegistries = await _azureResourceFactory.GetAllContainerRegistriesAsync(tenant.ToSdk());
                ContainerRegistryEnv[] registries = sdkRegistries.Select(r => r.ToDomain()).ToArray();
                if (registries.Length == 0)
                {
                    await DialogService.ShowInfoAsync(MessageResources.NoContainerRegistryFound);
                    return;
                }

                IList<ContainerRegistryEnv> selectedRegistries = await SelectRegistriesAsync(registries);
                if (selectedRegistries == null || selectedRegistries.Count == 0) return;

                int importedCount = 0;
                int updatedCount = 0;
                foreach (ContainerRegistryEnv registry in selectedRegistries)
                {
                    registry.TenantId = tenant.TenantId;
                    ContainerRegistryEnv existing = Items.FirstOrDefault(x => x.Registry.Equals(registry.Registry, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.Registry = registry.Registry;
                        existing.RegistryUrl = registry.RegistryUrl;
                        existing.CreatedOn = registry.CreatedOn;
                        existing.IsAdminUserEnabled = registry.IsAdminUserEnabled;
                        existing.IsAnonymousPullEnabled = registry.IsAnonymousPullEnabled;
                        existing.SubscriptionName = registry.SubscriptionName;
                        existing.SubscriptionId = registry.SubscriptionId;
                        existing.ResourceGroupName = registry.ResourceGroupName;
                        _containerRegistryStore.UpdateOption(existing);
                        updatedCount++;
                    }
                    else
                    {
                        _containerRegistryStore.AddOption(registry);
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

        private async Task<TenantProfile> SelectTenantAsync(TenantProfile[] tenants)
        {
            if (tenants.Length == 1) return tenants[0];

            IList<TenantProfile> selected = await DialogService.SelectItemsAsync<TenantProfile>(
                tenants, false, MessageResources.SelectTenantToImportEnv,
                new Dictionary<string, string> { { "Name", ModelResources.TenantColumnName }, { "Code", ModelResources.TenantCodeName } });
            return selected?.FirstOrDefault();
        }

        private async Task<IList<ContainerRegistryEnv>> SelectRegistriesAsync(ContainerRegistryEnv[] registries)
        {
            if (registries.Length == 1) return registries.ToList();

            Dictionary<string, string> propertyMappings = new Dictionary<string, string>
            {
                ["Registry"] = FormResources.LblRegistry,
                ["State"] = FormResources.LblState,
                ["SubscriptionId"] = FormResources.LblSubscriptionId
            };

            return await DialogService.SelectItemsAsync(
                registries.ToList(), true,
                MessageResources.SelectContainerRegistryToImport,
                propertyMappings);
        }

        protected override async Task<ContainerRegistryEnv> EditOptionAsync(ContainerRegistryEnv item)
        {
            ContainerRegistryEnvEditorViewModel editViewModel = await ShowDialog<ContainerRegistryEnvEditorView, ContainerRegistryEnvEditorViewModel>(new ContainerRegistryEnvEditorOption { Option = item, IsEditMode = item != null });
            return editViewModel.DialogResult == true ? editViewModel.GetItem() : null;
        }

        protected override async Task<bool> HandleCloseAsync()
        {
            if (!HasChanges) return true;
            bool save = await DialogService.ShowConfirmAsync(MessageResources.ConfirmSaveUnsavedChanges, MessageResources.TitleUnsavedChanges);
            if (save)
            {
                _containerRegistryStore.Save();
            }
            else
            {
                _containerRegistryStore.DiscardChanges();
            }
            return true;
        }
    }
}
