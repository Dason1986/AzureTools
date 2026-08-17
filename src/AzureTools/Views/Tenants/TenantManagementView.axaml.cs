using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.ViewModels.Tenants;

namespace AzureTools.Views.Tenants
{
    public partial class TenantManagementView : View
    {
        public TenantManagementView(TenantManagementViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
