using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Azure.ViewModels.KeyVaults;

namespace Toolkit.Modules.Azure.Views.KeyVaults
{
    public partial class KeyVaultSecretHistoriesView : View
    {
        public KeyVaultSecretHistoriesView(KeyVaultSecretHistoriesViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
