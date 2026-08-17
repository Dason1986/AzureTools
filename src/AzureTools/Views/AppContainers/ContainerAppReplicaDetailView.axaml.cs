using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.AppContainers;

namespace AzureTools.Views.AppContainers
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
