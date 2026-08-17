using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.AppContainers;

namespace AzureTools.Views.AppContainers
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
