using Avalonia.Markup.Xaml;
using AzureTools.Module.Main.ViewModels.Options;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Main.Views.Options
{
    public partial class OptionsView : View
    {
        public OptionsView(OptionsViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
