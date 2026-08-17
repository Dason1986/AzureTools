using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.Tenants;

namespace AzureTools.Views.Tenants
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
