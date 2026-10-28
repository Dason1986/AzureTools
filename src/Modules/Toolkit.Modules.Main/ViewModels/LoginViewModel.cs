using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Toolkit.Modules.Main.Properties;

namespace Toolkit.Modules.Main.ViewModels
{
    public partial class LoginViewModel : ViewModel
    {
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _password = string.Empty;

        public string PasswordHash { get; private set; }

        public LoginViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        [RelayCommand]
        private async Task ConfirmAsync()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                await _dialogService.ShowInfoAsync(MainResource.LoginPasswordRequired);
                return;
            }
            PasswordHash = ComputeHash(Password);
            GetWindow().Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            PasswordHash = null;
            GetWindow().Close();
        }

        private static string ComputeHash(string input)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
