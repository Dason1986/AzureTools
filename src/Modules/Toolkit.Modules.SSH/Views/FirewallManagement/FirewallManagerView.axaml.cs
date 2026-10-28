using Avalonia.Controls;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.SSH.ViewModels.FirewallManagement;

namespace Toolkit.Modules.SSH.Views.FirewallManagement
{
    public partial class FirewallManagerView : View
    {
        public FirewallManagerView(FirewallManagerViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void OnTerminalTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox terminal)
            {
                terminal.CaretIndex = terminal.Text?.Length ?? 0;
            }
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
    }
}
