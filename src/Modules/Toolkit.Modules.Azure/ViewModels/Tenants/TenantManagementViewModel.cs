using AzureTools.SDK;
using AzureTools.Services;
using AzureTools.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Models;
using Toolkit.Modules.Properties;
using Toolkit.Modules.Stores;


namespace Toolkit.Modules.Azure.ViewModels.Tenants
{
    public partial class TenantManagementViewModel : ManageViewModel<TenantProfile>
    {
        #region Constructors

        public TenantManagementViewModel(IToolkitStore store, IAzureResourceFactory resourceFactory, IDialogService dialogService) : base(dialogService)
        {
            _store = store;
            _resourceFactory = resourceFactory;
            LoadTenants();
            AppProfile profile = _store.GetProfile();
            AddEnabled = profile != null && profile.TenantMode == TenantMode.Multiple;

        }

        #endregion

        #region Fields & Properties

        private readonly IToolkitStore _store;
        private readonly IAzureResourceFactory _resourceFactory;

        public bool AddEnabled { get; }

        #endregion

        private void LoadTenants()
        {
            TenantProfile[] tenants = _store.GetTenants();
            Items = new ObservableCollection<TenantProfile>(tenants);
            SelectedItem = tenants.FirstOrDefault();
        }


        protected override async Task AddAsync()
        {
            TenantProfile tenant = await EditOptionAsync(null);
            if (tenant == null) return;
            AddOrUpdateTenant(tenant);
        }

        protected override async Task EditAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(MessageResources.SelectItemToEdit);
                return;
            }
            TenantProfile tenant = await EditOptionAsync(SelectedItem);
            if (tenant == null) return;
            AddOrUpdateTenant(tenant);
        }

        protected override async Task<TenantProfile> EditOptionAsync(TenantProfile tenant)
        {
            TenantEditorViewModel editViewModel = await ShowDialog<TenantEditorViewModel>(new TenantEditorOption { Option = tenant, IsEditMode = tenant != null });
            return editViewModel.DialogResult == true ? editViewModel.EditedTenant : null;
        }

        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(MessageResources.SelectItemToDelete);
                return;
            }
            if (Items.Count == 1)
            {
                await DialogService.ShowErrorAsync(MessageResources.MsgCannotDeleteLastTenant);
                return;
            }
            bool confirm = await DialogService.ShowConfirmAsync(string.Format(FormResources.MsgConfirmDeleteTenant, SelectedItem.Name));
            if (!confirm) return;
            _store.DeleteTenant(SelectedItem);
            _store.Save();
            LoadTenants();
        }

        protected override Task RefreshAsync()
        {
            LoadTenants();
            return Task.CompletedTask;
        }

        private void AddOrUpdateTenant(TenantProfile tenant)
        {
            if (tenant == null) return;
            _store.AddOrUpdateTenant(tenant);
            _store.Save();
            _resourceFactory.ClearCredentialCache();
            LoadTenants();
        }

        protected override Task SaveAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override Task DiscardChangesAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<bool> HandleCloseAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
