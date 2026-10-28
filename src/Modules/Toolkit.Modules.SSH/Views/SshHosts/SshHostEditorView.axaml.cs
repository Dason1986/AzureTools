using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.SSH.ViewModels.SshHosts;

namespace Toolkit.Modules.SSH.Views.SshHosts
{
    public partial class SshHostEditorView : View
    {
        public SshHostEditorView(SshHostEditorViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}