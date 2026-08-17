using AzureTools.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AzureTools.ViewModels.AppContainers
{
    public partial class AppContainerEnvEditorViewModel : EditorViewModel<AppContainerEnv>
    {
        #region Constructors

        public AppContainerEnvEditorViewModel(AppContainerEnvEditorOption option) : base(option)
        {

            if (ExistingItem != null)
            {

                Name = ExistingItem.Name;
                EnvironmentName = ExistingItem.EnvironmentName;
                ResourceGroupName = ExistingItem.ResourceGroupName;
                SubscriptionName = ExistingItem.SubscriptionName;
                SubscriptionId = ExistingItem.SubscriptionId;
            }
        }
        #endregion

        #region Fields & Properties
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _environmentName;
        [ObservableProperty]
        private string _resourceGroupName;
        [ObservableProperty]
        private string _subscriptionName;
        [ObservableProperty]
        private string _subscriptionId;


        #endregion



        public AppContainerEnv GetItem()
        {
            if (IsEditMode)
            {
                ExistingItem.EnvironmentName = EnvironmentName?.Trim();
                ExistingItem.ResourceGroupName = ResourceGroupName?.Trim();
                ExistingItem.Name = Name?.Trim();
                ExistingItem.SubscriptionName = SubscriptionName?.Trim();
                ExistingItem.SubscriptionId = SubscriptionId?.Trim();
                return ExistingItem;
            }

            return new AppContainerEnv
            {
                Name = Name?.Trim(),
                EnvironmentName = EnvironmentName?.Trim(),
                ResourceGroupName = ResourceGroupName?.Trim(),
                SubscriptionName = SubscriptionName?.Trim(),
                SubscriptionId = SubscriptionId?.Trim(),
            };
        }
    }
}
