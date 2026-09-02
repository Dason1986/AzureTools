using Avalonia.Markup.Xaml;
using AzureTools.Module.Azure.ViewModels.ContainerRegistries;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Azure.Views.ContainerRegistries
{
    public partial class ContainerRegistryMainView : View
    {
        public ContainerRegistryMainView(ContainerRegistryMainViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
