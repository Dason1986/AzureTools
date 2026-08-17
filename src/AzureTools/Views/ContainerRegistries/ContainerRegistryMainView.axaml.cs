using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.ContainerRegistries;

namespace AzureTools.Views.ContainerRegistries
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
