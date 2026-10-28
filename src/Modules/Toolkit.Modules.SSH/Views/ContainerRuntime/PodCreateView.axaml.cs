using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.SSH.ViewModels.ContainerRuntime;

namespace Toolkit.Modules.SSH.Views.ContainerRuntime
{
    public partial class PodCreateView : View
    {
        public PodCreateView(PodCreateViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void PendingGrid_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (DataContext is PodCreateViewModel viewModel && viewModel.EditPendingContainerCommand.CanExecute(null))
            {
                viewModel.EditPendingContainerCommand.Execute(null);
            }
        }
    }
}
