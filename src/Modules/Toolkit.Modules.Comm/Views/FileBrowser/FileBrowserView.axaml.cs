using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using Toolkit.Modules.Comm.ViewModels.FileBrowser;

namespace Toolkit.Modules.Comm.Views.FileBrowser
{
    public partial class FileBrowserView : View
    {
        public FileBrowserView(FileBrowserViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnItemDoubleTapped(object sender, TappedEventArgs e)
        {
            if (DataContext is FileBrowserViewModel vm && vm.SelectedItem != null)
                vm.EnterItemCommand.Execute(vm.SelectedItem);
        }

        private void OnCloseTabClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is FileTabItem tab && DataContext is FileBrowserViewModel vm)
                vm.CloseTabCommand.Execute(tab);
        }
    }
}
