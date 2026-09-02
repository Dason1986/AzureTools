using Avalonia.Markup.Xaml;
using AzureTools.Module.Azure.ViewModels.KeyVaults;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Azure.Views.KeyVaults
{
    public partial class KeyVaultEnvManageView : View
    {
        public KeyVaultEnvManageView(KeyVaultEnvManageViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}
