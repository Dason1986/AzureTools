using AzureTools.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class PodsTabViewModel : ContainerRuntimeTabViewModel
    {
        private readonly IPodManager _pods;
        private readonly IContainerRuntimeDialogHost _dialogHost;

        public PodsTabViewModel(IContainerRuntimeManager runtime, IDialogService dialog, string engine, IContainerRuntimeDialogHost dialogHost)
            : base(runtime, dialog, engine)
        {
            _pods = runtime as IPodManager;
            _dialogHost = dialogHost;
        }

        public ObservableCollection<PodTreeNode> PodNodes { get; } = new();

        [ObservableProperty]
        private PodTreeNode _selectedNode;

        partial void OnSelectedNodeChanged(PodTreeNode value)
        {
            StartPodCommand.NotifyCanExecuteChanged();
            StopPodCommand.NotifyCanExecuteChanged();
            RemovePodCommand.NotifyCanExecuteChanged();
            EditPodCommand.NotifyCanExecuteChanged();
        }

        protected override async Task LoadAsync()
        {
            PodNodes.Clear();
            foreach (Pod pod in await _pods.ListPodsAsync())
            {
                PodNodes.Add(await CreatePodNodeAsync(pod));
            }
        }

        private async Task<PodTreeNode> CreatePodNodeAsync(Pod pod)
        {
            List<PodTreeNode> containers = new();
            try
            {
                foreach (Container container in await _pods.ListPodContainersAsync(pod.Name))
                {
                    containers.Add(new PodTreeNode(container));
                }
            }
            catch
            {
                return new PodTreeNode(pod, containers);
            }
            return new PodTreeNode(pod, containers);
        }

        private bool HasSelectedPod()
        {
            return SelectedNode is { IsPod: true };
        }

        [RelayCommand(CanExecute = nameof(HasSelectedPod))]
        private async Task StartPodAsync()
        {
            try
            {
                await _pods.StartPodAsync(SelectedNode.Pod.Id);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                await Dialog.ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand(CanExecute = nameof(HasSelectedPod))]
        private async Task StopPodAsync()
        {
            try
            {
                await _pods.StopPodAsync(SelectedNode.Pod.Id);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                await Dialog.ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand(CanExecute = nameof(HasSelectedPod))]
        private async Task RemovePodAsync()
        {
            if (!await Dialog.ShowConfirmAsync(string.Format(SSHResource.MsgConfirmRemovePod, SelectedNode.Name))) return;
            try
            {
                await _pods.RemovePodAsync(SelectedNode.Pod.Id, true);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                await Dialog.ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand(CanExecute = nameof(HasSelectedPod))]
        private async Task EditPodAsync()
        {
            await _dialogHost.ShowPodEditAsync(new PodEditOption { Engine = Engine, PodName = SelectedNode.Name });
            await RefreshAsync();
        }

        [RelayCommand]
        private async Task ShowCreatePodAsync()
        {
            await _dialogHost.ShowPodCreateAsync(new PodCreateOption { Engine = Engine });
            await RefreshAsync();
        }

        [RelayCommand]
        private async Task PrunePodsAsync()
        {
            if (!await Dialog.ShowConfirmAsync(SSHResource.MsgConfirmPrunePods)) return;
            try
            {
                await _pods.PrunePodsAsync();
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                await Dialog.ShowErrorAsync(ex.Message);
            }
        }
    }
}
