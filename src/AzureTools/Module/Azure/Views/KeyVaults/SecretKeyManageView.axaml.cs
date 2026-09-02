using Avalonia.Markup.Xaml;
using AzureTools.Module.Azure.ViewModels.KeyVaults;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Azure.Views.KeyVaults
{
    public partial class SecretKeyManageView : View
    {
        public SecretKeyManageView(SecretKeyManageViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}
