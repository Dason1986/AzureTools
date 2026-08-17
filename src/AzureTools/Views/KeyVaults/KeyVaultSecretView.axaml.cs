using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;

namespace AzureTools.Views.KeyVaults
{
    public partial class KeyVaultSecretView : View
    {
        public KeyVaultSecretView(KeyVaultSecretViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
