using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Azure.ViewModels.KeyVaults;

namespace Toolkit.Modules.Azure.Views.KeyVaults
{
    public partial class KeyVaultSecretRecycleView : View
    {
        private readonly KeyVaultSecretRecycleViewModel _viewModel;

        public KeyVaultSecretRecycleView(KeyVaultSecretRecycleViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
            _viewModel = viewModel;
            Loaded += OnLoaded;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshenCommand.ExecuteAsync(null);
        }
    }
}
