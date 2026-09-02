using Avalonia.Markup.Xaml;
using AzureTools.Module.Main.ViewModels;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Main.Views
{
    public partial class LoginView : View
    {
        public LoginView(LoginViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
