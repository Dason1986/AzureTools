using AzureTools.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AzureTools.ViewModels
{
    public abstract partial class ManageViewModel<T> : ViewModel
    {
        public ManageViewModel(IDialogService dialogService)
        {
            this.DialogService = dialogService;
     
        }
        #region Fields & Properties


        protected readonly IDialogService DialogService;
        [ObservableProperty]
        private ObservableCollection<T> _items;

        [ObservableProperty]
        private T _selectedItem;



        [ObservableProperty]
        private bool _hasChanges;

        #endregion

        [RelayCommand]
        protected abstract Task SaveAsync();
        [RelayCommand]
        protected abstract Task AddAsync();
        [RelayCommand]
        protected abstract Task EditAsync();
        [RelayCommand]
        protected abstract Task DeleteAsync();
        [RelayCommand]
        protected abstract Task RefreshAsync();

        [RelayCommand]
        protected abstract Task DiscardChangesAsync();
        protected abstract Task<T> EditOptionAsync(T item);
        protected abstract Task<bool> HandleCloseAsync();
    }
}
