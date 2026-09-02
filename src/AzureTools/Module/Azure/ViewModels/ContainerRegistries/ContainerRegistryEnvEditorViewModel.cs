using AzureTools.Models;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AzureTools.Module.Azure.ViewModels.ContainerRegistries
{
    public partial class ContainerRegistryEnvEditorViewModel : EditorViewModel<ContainerRegistryEnv>
    {
        #region Constructors

        public ContainerRegistryEnvEditorViewModel(ContainerRegistryEnvEditorOption option) : base(option)
        {


            Name = ExistingItem.Name;
            Registry = ExistingItem.Registry;
            RegistryUrl = ExistingItem.RegistryUrl;
            CreatedOn = ExistingItem.CreatedOn;
            IsAdminUserEnabled = ExistingItem.IsAdminUserEnabled ?? false;
            IsAnonymousPullEnabled = ExistingItem.IsAnonymousPullEnabled ?? false;
            SubscriptionName = ExistingItem.SubscriptionName;
            SubscriptionId = ExistingItem.SubscriptionId;
            ResourceGroupName = ExistingItem.ResourceGroupName;
        }
        #endregion

        #region Fields & Properties
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _registry;
        [ObservableProperty]
        private string _registryUrl;

        [ObservableProperty]
        private string _resourceGroupName;
        [ObservableProperty]
        private DateTime? _createdOn;

        [ObservableProperty]
        private bool _isAdminUserEnabled;

        [ObservableProperty]
        private bool _isAnonymousPullEnabled;

        [ObservableProperty]
        private string _subscriptionName;

        [ObservableProperty]
        private string _subscriptionId;


        #endregion



        public ContainerRegistryEnv GetItem()
        {
            if (IsEditMode)
            {
                ExistingItem.Name = Name?.Trim();
                ExistingItem.Registry = Registry?.Trim();
                ExistingItem.RegistryUrl = RegistryUrl?.Trim();
                ExistingItem.CreatedOn = CreatedOn;
                ExistingItem.IsAdminUserEnabled = IsAdminUserEnabled;
                ExistingItem.IsAnonymousPullEnabled = IsAnonymousPullEnabled;
                ExistingItem.SubscriptionName = SubscriptionName;
                ExistingItem.SubscriptionId = SubscriptionId;
                ExistingItem.ResourceGroupName = ResourceGroupName?.Trim();
                return ExistingItem;
            }

            return new ContainerRegistryEnv
            {
                Name = Name?.Trim(),
                Registry = Registry?.Trim(),
                RegistryUrl = RegistryUrl?.Trim(),
                CreatedOn = CreatedOn,
                IsAdminUserEnabled = IsAdminUserEnabled,
                IsAnonymousPullEnabled = IsAnonymousPullEnabled,
                SubscriptionName = SubscriptionName,
                SubscriptionId = SubscriptionId,
                ResourceGroupName = ResourceGroupName?.Trim()
            };
        }
    }
}
