using Avalonia.Markup.Xaml;
using AzureTools.Module.Comm.ViewModels.Tools;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Comm.Views.Tools
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
