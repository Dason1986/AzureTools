using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools
{
    public delegate View KeyedViewFactory(ViewModel viewModel, ViewModelContext context);
}
