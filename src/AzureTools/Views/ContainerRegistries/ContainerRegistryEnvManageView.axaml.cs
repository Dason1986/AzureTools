using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.ContainerRegistries;

namespace AzureTools.Views.ContainerRegistries
{
    public partial class ContainerRegistryEnvManageView : View
    {
        public ContainerRegistryEnvManageView(ContainerRegistryEnvManageViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}
