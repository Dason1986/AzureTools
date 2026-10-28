using AzureTools.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class ContainersTabViewModel : ContainerRuntimeTabViewModel
    {
        private readonly IContainerRuntimeDialogHost _dialogHost;

        public ContainersTabViewModel(IContainerRuntimeManager runtime, IDialogService dialog, string engine, IContainerRuntimeDialogHost dialogHost)
            : base(runtime, dialog, engine)
        {
            _dialogHost = dialogHost;
        }

        public ObservableCollection<ContainerTreeNode> ContainerNodes { get; } = new();

        [ObservableProperty]
        private ContainerTreeNode _selectedNode;

        protected override async Task LoadAsync()
        {
            ContainerNodes.Clear();
            foreach (ContainerTreeNode node in BuildTree(await Runtime.ListContainersAsync()))
            {
                ContainerNodes.Add(node);
            }
        }

        private List<ContainerTreeNode> BuildTree(IReadOnlyList<Container> containers)
        {
            List<ContainerTreeNode> nodes = new();
            foreach (IGrouping<string, Container> group in containers.Where(c => !string.IsNullOrEmpty(c.Pod)).GroupBy(c => c.Pod))
            {
                Container infra = group.FirstOrDefault(c => string.IsNullOrEmpty(c.Image));
                if (infra == null)
                {
                    nodes.AddRange(group.Select(c => new ContainerTreeNode(c)));
                    continue;
                }
                nodes.Add(new ContainerTreeNode(infra, group.Where(c => !string.IsNullOrEmpty(c.Image))));
            }
            nodes.AddRange(containers.Where(c => string.IsNullOrEmpty(c.Pod)).Select(c => new ContainerTreeNode(c)));
            return nodes;
        }

        [RelayCommand]
        private async Task StartAsync()
        {
            if (SelectedNode == null) return;
            try { await Runtime.StartContainerAsync(SelectedNode.Container.Id); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task StopAsync()
        {
            if (SelectedNode == null) return;
            try { await Runtime.StopContainerAsync(SelectedNode.Container.Id); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task LogsAsync()
        {
            if (SelectedNode == null) return;
            try
            {
                string logs = await Runtime.GetContainerLogsAsync(SelectedNode.Container.Id);
                await Dialog.ShowInfoAsync(logs, string.Format(SSHResource.TitleContainerLogs, SelectedNode.Name));
            }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task DetailContainerAsync()
        {
            if (SelectedNode == null) return;
            await _dialogHost.ShowContainerDetailAsync(new ContainerDetailOption
            {
                ContainerId = SelectedNode.Container.Id,
                ContainerName = SelectedNode.Name,
                Engine = Engine
            });
        }

        [RelayCommand]
        private async Task MonitorAsync()
        {
            if (SelectedNode == null) return;
            await _dialogHost.ShowContainerMonitorAsync(new ContainerMonitorOption
            {
                ContainerId = SelectedNode.Container.Id,
                ContainerName = SelectedNode.Name,
                Engine = Engine
            });
        }

        [RelayCommand]
        private async Task RemoveContainerAsync()
        {
            if (SelectedNode == null) return;
            if (!await Dialog.ShowConfirmAsync(string.Format(SSHResource.MsgConfirmRemoveContainer, SelectedNode.Name))) return;
            try { await Runtime.RemoveContainerAsync(SelectedNode.Container.Id, true); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task CreateContainerAsync()
        {
            await _dialogHost.ShowContainerCreateAsync(new ContainerCreateOption { Engine = Engine });
            await RefreshAsync();
        }

        [RelayCommand]
        private async Task PruneContainersAsync()
        {
            if (!await Dialog.ShowConfirmAsync(SSHResource.MsgConfirmPruneContainers)) return;
            try { await Runtime.PruneContainersAsync(); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }
    }
}
