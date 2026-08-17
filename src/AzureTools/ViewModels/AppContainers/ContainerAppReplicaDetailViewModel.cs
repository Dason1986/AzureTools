using AzureTools.Properties;
using AzureTools.SDK.AppContainers;
using AzureTools.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.ViewModels.AppContainers
{
    public partial class ContainerAppReplicaDetailViewModel : ViewModel
    {
        #region Constructors

        public ContainerAppReplicaDetailViewModel(ContainerAppReplicaDetailOption option, IDialogService dialogService)
        {
            Replica = option.Replica;
            ContainerAppName = option.ContainerAppName;
            ResourceGroupName = option.ResourceGroupName;
            _dialogService = dialogService;
            Containers = Replica?.Containers == null
                ? new ObservableCollection<ContainerAppReplicaContainerInfo>()
                : new ObservableCollection<ContainerAppReplicaContainerInfo>(Replica.Containers);
            SelectedContainer = Containers.FirstOrDefault();
            WindowTitle = string.Format(FormResources.TitleReplicaDetail, Replica?.Name);
        }
        #endregion

        #region Fields & Properties
        public ContainerAppReplicaInfo Replica { get; }

        public string ContainerAppName { get; }

        public string ResourceGroupName { get; }

        public ObservableCollection<ContainerAppReplicaContainerInfo> Containers { get; }

        [ObservableProperty]
        private ContainerAppReplicaContainerInfo _selectedContainer;

        private readonly IDialogService _dialogService;
        #endregion


        string AzureCliCommand => $"az containerapp exec --name {ContainerAppName} --resource-group {ResourceGroupName} --revision {Replica.RevisionName} --replica {Replica.Name} --container {SelectedContainer.Name}";

        [RelayCommand]
        private async Task EnterContainer()
        {
            //  await _dialogService.OpenWithExplorerAsync(AzureCliCommand);
        }
    }
}
