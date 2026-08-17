using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;

namespace AzureTools.Views.Tools
{
    public partial class CerFileView : View
    {
        public CerFileView(CerFileViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
