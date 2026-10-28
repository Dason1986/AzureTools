using AzureTools.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public abstract partial class ContainerRuntimeTabViewModel : ObservableObject
    {
        protected ContainerRuntimeTabViewModel(IContainerRuntimeManager runtime, IDialogService dialog, string engine)
        {
            Runtime = runtime;
            Dialog = dialog;
            Engine = engine;
        }

        protected IContainerRuntimeManager Runtime { get; }

        protected IDialogService Dialog { get; }

        protected string Engine { get; }

        [ObservableProperty]
        private bool _isBusy;

        [RelayCommand]
        public async Task RefreshAsync()
        {
            IsBusy = true;
            try
            {
                await LoadAsync();
            }
            catch (Exception ex)
            {
                await Dialog.ShowErrorAsync(SshContainerEngine.GetLoadFailedMessage(Engine, ex.Message));
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected abstract Task LoadAsync();
    }
}
