using Avalonia.Markup.Xaml;
using AzureTools.Module.Main.ViewModels.Tenants;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Main.Views.Tenants
{
    public partial class TenantEditorView : View
    {
        public TenantEditorView(TenantEditorViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
