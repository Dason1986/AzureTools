using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.AppContainers;

namespace AzureTools.Views.AppContainers
{
    public partial class AppContainerMainView : View
    {
        public AppContainerMainView(AppContainerMainViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
