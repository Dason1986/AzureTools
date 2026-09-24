using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Toolkit.Modules.Azure.Models;

namespace Toolkit.Modules.Azure.ViewModels.KeyVaults
{
    public partial class KeyVaultEnvEditorViewModel : EditorViewModel<KeyVaultEnv>
    {
        #region Constructors
        public KeyVaultEnvEditorViewModel(KeyVaultEnvEditorOption option) : base(option)
        {

            if (ExistingItem != null)
            {
                Name = ExistingItem.Name;
                KeyVault = ExistingItem.KeyVault;
                KeyVaultUrl = ExistingItem.KeyVaultUrl;
                SubscriptionName = ExistingItem.SubscriptionName;
                SubscriptionId = ExistingItem.SubscriptionId;
                EnableSoftDelete = ExistingItem.EnableSoftDelete ?? false;
                ResourceGroupName = ExistingItem.ResourceGroupName;
            }
        }
        #endregion

        #region Fields & Properties

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _keyVault;

        [ObservableProperty]
        private string _keyVaultUrl;

        [ObservableProperty]
        private string _subscriptionName;
        [ObservableProperty]
        private string _resourceGroupName;
        [ObservableProperty]
        private string _subscriptionId;

        [ObservableProperty]
        private bool _enableSoftDelete;


        #endregion


        public KeyVaultEnv GetItem()
        {
            if (IsEditMode)
            {
                ExistingItem.Name = Name?.Trim();
                ExistingItem.KeyVault = KeyVault?.Trim();
                ExistingItem.KeyVaultUrl = KeyVaultUrl;
                ExistingItem.EnableSoftDelete = EnableSoftDelete;
                ExistingItem.SubscriptionName = SubscriptionName;
                ExistingItem.SubscriptionId = SubscriptionId;
                ExistingItem.ResourceGroupName = ResourceGroupName?.Trim();
                return ExistingItem;
            }

            return new KeyVaultEnv
            {
                Name = Name?.Trim(),
                KeyVault = KeyVault?.Trim(),
                KeyVaultUrl = KeyVaultUrl,
                EnableSoftDelete = EnableSoftDelete,
                SubscriptionName = SubscriptionName,
                SubscriptionId = SubscriptionId,
                ResourceGroupName = ResourceGroupName?.Trim()
            };
        }
    }
}
