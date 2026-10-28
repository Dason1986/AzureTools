using AzureTools.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class NetworksTabViewModel : ContainerRuntimeTabViewModel
    {
        private readonly IContainerRuntimeDialogHost _dialogHost;

        public NetworksTabViewModel(IContainerRuntimeManager runtime, IDialogService dialog, string engine, IContainerRuntimeDialogHost dialogHost)
            : base(runtime, dialog, engine)
        {
            _dialogHost = dialogHost;
        }

        public ObservableCollection<Network> Networks { get; } = new();

        [ObservableProperty]
        private Network _selectedNetwork;

        protected override async Task LoadAsync()
        {
            Networks.Clear();
            foreach (Network network in await Runtime.ListNetworksAsync()) Networks.Add(network);
        }

        [RelayCommand]
        private async Task CreateNetworkAsync()
        {
            string name = await Dialog.ShowPromptDialogAsync(SSHResource.TitleCreateNetwork, SSHResource.PromptNetworkName);
            if (string.IsNullOrWhiteSpace(name)) return;
            try { await Runtime.CreateNetworkAsync(name.Trim()); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task RemoveNetworkAsync()
        {
            if (SelectedNetwork == null) return;
            if (!await Dialog.ShowConfirmAsync(string.Format(SSHResource.MsgConfirmRemoveNetwork, SelectedNetwork.Name))) return;
            try { await Runtime.RemoveNetworkAsync(SelectedNetwork.Id); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task DetailNetworkAsync()
        {
            if (SelectedNetwork == null) return;
            await _dialogHost.ShowNetworkDetailAsync(new NetworkDetailOption
            {
                NetworkId = SelectedNetwork.Id,
                NetworkName = SelectedNetwork.Name,
                Engine = Engine
            });
            await RefreshAsync();
        }

        [RelayCommand]
        private async Task PruneNetworksAsync()
        {
            if (!await Dialog.ShowConfirmAsync(SSHResource.MsgConfirmPruneNetworks)) return;
            try { await Runtime.PruneNetworksAsync(); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }
    }
}
