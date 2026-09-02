using Avalonia.Markup.Xaml;
using AzureTools.Module.Azure.ViewModels.AppContainers;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Azure.Views.AppContainers
{
    public partial class AppContainerEnvManageView : View
    {
        public AppContainerEnvManageView(AppContainerEnvManageViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}
