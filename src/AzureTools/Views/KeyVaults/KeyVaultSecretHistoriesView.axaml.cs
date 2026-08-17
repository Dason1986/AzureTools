using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;

namespace AzureTools.Views.KeyVaults
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
