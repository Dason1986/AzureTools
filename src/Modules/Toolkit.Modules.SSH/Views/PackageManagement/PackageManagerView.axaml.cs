using Avalonia.Controls;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.SSH.ViewModels.PackageManagement;

namespace Toolkit.Modules.SSH.Views.PackageManagement
{
    public partial class PackageManagerView : View
    {
        public PackageManagerView(PackageManagerViewModel viewModel, ViewModelContext context) : base(viewModel, context)
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
