using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.KeyVaults;

namespace AzureTools.Views.KeyVaults
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
