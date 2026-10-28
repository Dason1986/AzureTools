using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.SSH.ViewModels.ContainerRuntime;

namespace Toolkit.Modules.SSH.Views.ContainerRuntime
{
    public partial class NetworkDetailView : View
    {
        public NetworkDetailView(NetworkDetailViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
