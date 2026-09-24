using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Azure.ViewModels.Tenants;

namespace Toolkit.Modules.Azure.Views.Tenants
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
