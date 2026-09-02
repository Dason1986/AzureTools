using Avalonia.Markup.Xaml;
using AzureTools.Module.Azure.ViewModels.AppContainers;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Azure.Views.AppContainers
{
    public partial class AppContainerEnvEditorView : View
    {
        public AppContainerEnvEditorView(AppContainerEnvEditorViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
