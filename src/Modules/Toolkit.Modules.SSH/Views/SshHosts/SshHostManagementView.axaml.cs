using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace Toolkit.Modules.SSH.Views.SshHosts
{
    public partial class SshHostManagementView : View
    {
        public SshHostManagementView(ViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent(true);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}