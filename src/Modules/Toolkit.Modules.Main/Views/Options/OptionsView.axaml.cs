using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Main.ViewModels.Options;

namespace Toolkit.Modules.Main.Views.Options
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
