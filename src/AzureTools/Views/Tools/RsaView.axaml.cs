using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;

namespace AzureTools.Views.Tools
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
