using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AzureTools.ViewModels;
using AzureTools.Views;
using System.Collections.Generic;
using System.Linq;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Azure.ViewModels.KeyVaults;

namespace Toolkit.Modules.Azure.Views.KeyVaults
{
    public partial class KeyVaultSecretMainView : View
    {
        public KeyVaultSecretMainView(KeyVaultSecretMainViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnCompareSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox) return;
            if (DataContext is not KeyVaultSecretMainViewModel vm) return;
            List<KeyVaultEnv> selected = listBox.SelectedItems.Cast<KeyVaultEnv>().ToList();
            vm.SelectedCompareOptions = selected;
        }
    }
}
