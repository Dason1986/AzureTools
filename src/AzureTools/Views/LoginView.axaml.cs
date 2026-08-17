using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;

namespace AzureTools.Views
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
