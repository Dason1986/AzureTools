using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;

namespace AzureTools.Views
{
    public partial class AppWizardView : View
    {
        public AppWizardView(AppWizardViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
