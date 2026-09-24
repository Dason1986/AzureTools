using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Comm.ViewModels.Tools;

namespace Toolkit.Modules.Comm.Views.Tools
{
    public partial class RsaView : View
    {
        public RsaView(RsaViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
