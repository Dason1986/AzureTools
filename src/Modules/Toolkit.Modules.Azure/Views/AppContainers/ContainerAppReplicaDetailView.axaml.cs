using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Azure.ViewModels.AppContainers;

namespace Toolkit.Modules.Azure.Views.AppContainers
{
    public partial class ContainerAppReplicaDetailView : View
    {
        public ContainerAppReplicaDetailView(ContainerAppReplicaDetailViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
