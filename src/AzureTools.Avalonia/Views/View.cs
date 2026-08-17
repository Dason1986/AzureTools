using Avalonia.Controls;
using AzureTools.ViewModels;

namespace AzureTools.Views
{
    public abstract partial class View : Window
    {
        protected View(ViewModel viewModel, ViewModelContext context)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            viewModel.SetContext(context);
            viewModel.Attach(this);
            Context = context;
        }

        public ViewModel ViewModel { get; }

        public ViewModelContext Context { get; }
    }
}
