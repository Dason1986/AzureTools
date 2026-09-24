using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Azure.ViewModels.ContainerRegistries;

namespace Toolkit.Modules.Azure.Views.ContainerRegistries
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
