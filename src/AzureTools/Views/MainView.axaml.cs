using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;

namespace AzureTools.Views
{
    public partial class MainView : View
    {
        public MainView(MainViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
