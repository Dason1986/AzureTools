using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Managers;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;
using Toolkit.Modules.SSH.Stores;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public partial class SshHostManagementViewModel : ManageViewModel<SshHostProfile>
    {
        private readonly ISshHostStore _store;
        private readonly ISshConnectionManager _connectionManager;

        [ObservableProperty]
        private ObservableCollection<SshFeatureMenuItem> _featureMenus = new ObservableCollection<SshFeatureMenuItem>();

        [ObservableProperty]
        private bool _hasNoFeatureMenu = true;

        public SshHostManagementViewModel(ISshHostStore store, ISshConnectionManager connectionManager, IDialogService dialogService) : base(dialogService)
        {
            _store = store;
            _connectionManager = connectionManager;
            LoadHosts();
        }

        private void LoadHosts()
        {
            SshHostProfile[] hosts = _store.GetHosts().ToArray();
            Items = new ObservableCollection<SshHostProfile>(hosts);
            SelectedItem = hosts.FirstOrDefault();
        }

        protected override async Task AddAsync()
        {
            SshHostProfile host = await EditOptionAsync(null);
            if (host == null) return;
            AddOrUpdate(host);
        }

        protected override async Task EditAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(SSHResource.SelectItemToEdit);
                return;
            }
            SshHostProfile host = await EditOptionAsync(SelectedItem);
            if (host == null) return;
            AddOrUpdate(host);
        }

        protected override async Task<SshHostProfile> EditOptionAsync(SshHostProfile host)
        {
            SshHostEditorViewModel editViewModel = await ShowDialog<SshHostEditorViewModel>(new SshHostEditorOption { Option = host, IsEditMode = host != null });
            return editViewModel.DialogResult == true ? editViewModel.EditedHost : null;
        }

        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(SSHResource.SelectItemToDelete);
                return;
            }
            bool confirm = await DialogService.ShowConfirmAsync(string.Format("是否删除主机[{0}]?", SelectedItem.Name));//SSHResources.MsgConfirmDeleteSshHost
            if (!confirm) return;
            _connectionManager.Disconnect(SelectedItem.Id);
            _store.Delete(SelectedItem);
            _store.Save();
            LoadHosts();
        }

        private Task OpenFileBrowserAsync()
        {
            SshContainerFileProvider provider = new SshContainerFileProvider(_connectionManager, SelectedItem);
            ShowKeyed("FileBrowser", new ViewParameters()
                .Set("Provider", provider)
                .Set("Title", SelectedItem.Name));
            return Task.CompletedTask;
        }

        private Task OpenFeatureAsync(string code)
        {
            return code switch
            {
                SshHostFeature.FileBrowser => OpenFileBrowserAsync(),
                SshHostFeature.Terminal => OpenTerminalAsync(),
                SshHostFeature.Package => OpenPackageAsync(),
                SshHostFeature.Firewall => OpenFirewallAsync(),
                SshHostFeature.Docker => OpenDockerAsync(),
                SshHostFeature.Podman => OpenPodmanAsync(),
                _ => Task.CompletedTask
            };
        }

        private Task OpenTerminalAsync()
        {
            SshShellCliSession session = new SshShellCliSession(_connectionManager.GetClient(SelectedItem));
            ShowKeyed(SshViewKeys.TerminalKeys.Key, new ViewParameters()
                .Set(SshViewKeys.TerminalKeys.Session, session)
                .Set(SshViewKeys.TerminalKeys.Title, SelectedItem.Name)
                .Set(SshViewKeys.TerminalKeys.IsInteractive, true));
            return Task.CompletedTask;
        }

        private Task OpenDockerAsync()
        {
            ShowKeyed(SshViewKeys.DockerBrowser, new ViewParameters()
                .Set(SshViewKeys.DockerBrowserParams.Host, SelectedItem));
            return Task.CompletedTask;
        }

        private Task OpenPodmanAsync()
        {
            ShowKeyed(SshViewKeys.PodmanBrowser, new ViewParameters()
                .Set(SshViewKeys.DockerBrowserParams.Host, SelectedItem));
            return Task.CompletedTask;
        }

        private Task OpenPackageAsync()
        {
            ShowKeyed(SshViewKeys.PackageManager, new ViewParameters()
                .Set(SshViewKeys.PackageManagerKeys.Host, SelectedItem));
            return Task.CompletedTask;
        }

        private Task OpenFirewallAsync()
        {
            ShowKeyed(SshViewKeys.FirewallManager, new ViewParameters()
                .Set(SshViewKeys.FirewallManagerKeys.Host, SelectedItem));
            return Task.CompletedTask;
        }

        private void RebuildFeatureMenus()
        {
            HashSet<string> enabled = SelectedItem?.Features?.Keys.ToHashSet();
            SshFeatureMenuItem[] menus = SshHostFeature.All
                .Where(f => enabled != null && enabled.Contains(f.Code))
                .Select(f => new SshFeatureMenuItem(f, OpenFeatureAsync))
                .ToArray();
            FeatureMenus = new ObservableCollection<SshFeatureMenuItem>(menus);
            HasNoFeatureMenu = menus.Length == 0;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(SelectedItem)) RebuildFeatureMenus();
        }

        protected override Task RefreshAsync()
        {
            LoadHosts();
            return Task.CompletedTask;
        }

        private void AddOrUpdate(SshHostProfile host)
        {
            if (host == null) return;
            _store.AddOrUpdate(host);
            _store.Save();
            _connectionManager.Disconnect(host.Id);
            LoadHosts();
        }

        protected override Task SaveAsync() => Task.CompletedTask;
        protected override Task DiscardChangesAsync() => Task.CompletedTask;
        protected override Task<bool> HandleCloseAsync() => Task.FromResult(true);
    }
}
