using Avalonia.Markup.Xaml;
using AzureTools.Module.Main.ViewModels;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Main.Views
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
