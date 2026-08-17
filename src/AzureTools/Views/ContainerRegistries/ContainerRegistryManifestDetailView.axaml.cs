using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.ContainerRegistries;

namespace AzureTools.Views.ContainerRegistries
{
    public partial class ContainerRegistryManifestDetailView : View
    {
        public ContainerRegistryManifestDetailView(ContainerRegistryManifestDetailViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
