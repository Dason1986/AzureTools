using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AzureTools.Module.Azure.ViewModels.ContainerRegistries;
using AzureTools.SDK.ContainerRegistries;
using AzureTools.ViewModels;
using AzureTools.Views;

namespace AzureTools.Module.Azure.Views.ContainerRegistries
{
    public partial class ContainerRegistryRepositoryListView : View
    {
        public ContainerRegistryRepositoryListView(ContainerRegistryRepositoryListViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (DataContext is ContainerRegistryRepositoryListViewModel vm && vm.IsDeleting)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void OnManifestSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ContainerRegistryRepositoryListViewModel vm && sender is DataGrid grid)
            {
                vm.SelectedManifests.Clear();
                foreach (object item in grid.SelectedItems)
                {
                    if (item is ManifestProperty manifest)
                    {
                        vm.SelectedManifests.Add(manifest);
                    }
                }
            }
        }
    }
}
