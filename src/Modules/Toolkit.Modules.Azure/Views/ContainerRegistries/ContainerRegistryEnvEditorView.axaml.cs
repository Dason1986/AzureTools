using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Azure.ViewModels.ContainerRegistries;

namespace Toolkit.Modules.Azure.Views.ContainerRegistries
{
    public partial class ContainerRegistryEnvEditorView : View
    {
        public ContainerRegistryEnvEditorView(ContainerRegistryEnvEditorViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
