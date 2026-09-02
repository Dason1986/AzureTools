using AzureTools.Cli;
using AzureTools.Module.Comm.ViewModels.CliStreamSession;
using AzureTools.Module.Comm.ViewModels.FileBrowser;
using AzureTools.Properties;
using AzureTools.SDK.AppContainers;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.Module.Azure.ViewModels.AppContainers
{
    public partial class ContainerAppReplicaDetailViewModel : ViewModel
    {
        #region Constructors

        public ContainerAppReplicaDetailViewModel(ContainerAppReplicaDetailOption option, IDialogService dialogService)
        {
            Replica = option.Replica;
            ContainerAppName = option.ContainerAppName;
            _management = option.Management;
            ResourceGroupName = option.EnvOption.ResourceGroupName;

            _dialogService = dialogService;
            Containers = Replica?.Containers == null
                ? new ObservableCollection<ContainerAppReplicaContainerInfo>()
                : new ObservableCollection<ContainerAppReplicaContainerInfo>(Replica.Containers);
            SelectedContainer = Containers.FirstOrDefault();
            WindowTitle = string.Format(FormResources.TitleReplicaDetail, Replica?.Name);
            SupprotContainerLogs = _management.SupportContainerLogs;
            SupprotEnterContainer = _management.SupportEnterContainer;
        }
        #endregion

        #region Fields & Properties

        private readonly IAppContainerManagement _management;
        public ContainerAppReplicaInfo Replica { get; }

        public string ContainerAppName { get; }


        public string ResourceGroupName { get; }


        public bool SupprotContainerLogs { get; }
        public bool SupprotEnterContainer { get; }

        public ObservableCollection<ContainerAppReplicaContainerInfo> Containers { get; }

        [ObservableProperty]
        private ContainerAppReplicaContainerInfo _selectedContainer;

        private readonly IDialogService _dialogService;
        #endregion




        [RelayCommand]
        private async Task EnterContainerAsync()
        {
            if (SelectedContainer == null)
            {
                await _dialogService.ShowWarningAsync("未选择容器");
                return;
            }
            ICliStreamSession streamSession = await _management.ExecContainerAsync(ContainerAppName, ResourceGroupName, Replica.RevisionName, Replica.Name, SelectedContainer.Name);
            CliStreamSessionOption option = new()
            {
                Session = streamSession,
                Title = string.Format(FormResources.TitleContainerExec, Replica.Name, SelectedContainer.Name),
                IsInteractive = true
            };
            Show<CliStreamSessionViewModel>(option);
        }

        [RelayCommand]
        private async Task GetLogsAsync()
        {
            if (SelectedContainer == null)
            {
                await _dialogService.ShowWarningAsync("未选择容器");
                return;
            }
            ICliStreamSession streamSession = await _management.GetContainerLogsFollowAsync(ContainerAppName, ResourceGroupName, Replica.RevisionName, Replica.Name);
            CliStreamSessionOption option = new()
            {
                Session = streamSession,
                Title = string.Format(FormResources.TitleContainerLogs, Replica.Name, SelectedContainer.Name),
                IsInteractive = false
            };
            Show<CliStreamSessionViewModel>(option);
        }

        [RelayCommand]
        private async Task OpenFileBrowserAsync()
        {
            if (SelectedContainer == null)
            {
                await _dialogService.ShowWarningAsync("未选择容器");
                return;
            }
            IContainerFileProvider provider = await _management.GetContainerFileBrowserAsync(ContainerAppName, ResourceGroupName, Replica.RevisionName, Replica.Name, SelectedContainer.Name);
            FileBrowserOption option = new()
            {
                FileProvider = provider,
                Title = string.Format(FormResources.TitleFileBrowser, Replica.Name, SelectedContainer.Name)
            };
            Show<FileBrowserViewModel>(option);
        }
    }
}
