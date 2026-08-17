using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AzureTools.ViewModels
{
    public abstract partial class DialogViewModel : ViewModel
    {
        #region Fields & Properties

        public bool? DialogResult { get; private set; }
        #endregion
        [RelayCommand]
        protected virtual Task ConfirmAsync()
        {
            DialogResult = true;
            GetWindow().Close();
            return Task.CompletedTask;
        }

        [RelayCommand]
        protected virtual Task CancelAsync()
        {
            DialogResult = false;
            GetWindow().Close();
            return Task.CompletedTask;
        }
    }
}
