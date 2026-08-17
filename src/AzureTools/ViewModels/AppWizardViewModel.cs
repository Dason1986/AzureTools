using Azure.Core;
using AzureTools.Extensions;
using AzureTools.Models;
using AzureTools.Properties;
using AzureTools.SDK;
using AzureTools.SDK.Models;
using AzureTools.Services;
using AzureTools.Stores;
using AzureTools.Tools.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace AzureTools.ViewModels
{
    public partial class AppWizardViewModel : ViewModel
    {
        #region Constructors

        public AppWizardViewModel(IAzureToolsStore store, IAzureResourceFactory azureResourceFactory, IDialogService dialogService)
        {
            _store = store;
            this.azureResourceFactory = azureResourceFactory;
            _dialogService = dialogService;

            CredentialOptions = new Option<AzureCredential>[]
            {
                new Option<AzureCredential>(AzureCredential.Browser, "浏览器"),
                new Option<AzureCredential>(AzureCredential.AzureCli, "Azure Cli"),
                new Option<AzureCredential>(AzureCredential.DeviceCode, "浏览器+授权码"),
                new Option<AzureCredential>(AzureCredential.Certificate, "证书"),
                new Option<AzureCredential>(AzureCredential.CertificatePassword, "证书+密码")
            };

            TenantModeOptions = new Option<TenantMode>[]
            {
                new Option<TenantMode>(TenantMode.Single, "单租户"),
                new Option<TenantMode>(TenantMode.Multiple, "多租户")
            };

            SelectedCredential = CredentialOptions.FirstOrDefault(x => x.Value == Credential);
            SelectedTenantMode = TenantModeOptions.FirstOrDefault(x => x.Value == TenantMode);
        }

        #endregion

        #region Fields & Properties

        private readonly IAzureToolsStore _store;
        private readonly IAzureResourceFactory azureResourceFactory;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _appName = "Azure Tools";

        [ObservableProperty]
        private TenantMode _tenantMode = TenantMode.Single;

        [ObservableProperty]
        private string _tenantName;

        [ObservableProperty]
        private string _tenantCode;

        [ObservableProperty]
        private string _tenantId;

        [ObservableProperty]
        private string _clientId;

        [ObservableProperty]
        private AzureCredential _credential = AzureCredential.Browser;

        [ObservableProperty]
        private Option<AzureCredential> _selectedCredential;

        [ObservableProperty]
        private Option<TenantMode> _selectedTenantMode;


        [ObservableProperty]
        private string _certificateRawData;

        [ObservableProperty]
        private string _certificatePassword;

        [ObservableProperty]
        private bool _loginUsePassword;

        [ObservableProperty]
        private bool _useDataEncryption;


        [ObservableProperty]
        private string _loginPassword;

        [ObservableProperty]
        private bool _isCertificateEnabled;

        [ObservableProperty]
        private bool _isPasswordEnabled;

        public Option<AzureCredential>[] CredentialOptions { get; }
        public Option<TenantMode>[] TenantModeOptions { get; }

        public event EventHandler Completed;

        partial void OnSelectedCredentialChanged(Option<AzureCredential> value)
        {
            if (value != null)
                Credential = value.Value;
        }

        partial void OnSelectedTenantModeChanged(Option<TenantMode> value)
        {
            if (value != null)
                TenantMode = value.Value;
        }

        #endregion

        partial void OnCredentialChanged(AzureCredential value)
        {
            IsCertificateEnabled = value == AzureCredential.Certificate || value == AzureCredential.CertificatePassword;
            IsPasswordEnabled = value == AzureCredential.CertificatePassword;
        }

        [RelayCommand]
        private async Task TestCertificate()
        {

            if (Credential == AzureCredential.Browser || Credential == AzureCredential.DeviceCode)
            {
                await _dialogService.ShowInfoAsync("浏览器模式不能进行测试");
                return;
            }

            await TestCertificate(true);


        }

        private async Task<bool> TestCertificate(bool showMessage)
        {
            if (Credential != AzureCredential.CertificatePassword && Credential != AzureCredential.Certificate) return true;
            if (showMessage)
            {
                List<string> errors = new List<string>();
                if (string.IsNullOrWhiteSpace(TenantId)) errors.Add(MessageResources.TenantIdRequired);
                if (string.IsNullOrWhiteSpace(ClientId)) errors.Add(MessageResources.ClientIdRequired);
                if (string.IsNullOrWhiteSpace(CertificateRawData)) errors.Add(MessageResources.CertificateFileRequired);
                if (Credential == AzureCredential.CertificatePassword && string.IsNullOrWhiteSpace(CertificatePassword)) errors.Add(MessageResources.CertificatePasswordRequired);

                if (errors.Count > 0)
                {
                    await _dialogService.ShowInfoAsync(string.Join("\r\n", errors));
                    return false;
                }
            }
            try
            {
                AzureTenant sdkTenant = new AzureTenant
                {
                    CertificateRawData = CertificateRawData,
                    ClientId = ClientId,
                    Credential = Credential,
                    TenantId = TenantId,
                    Password = CertificatePassword,
                };

                var token = await azureResourceFactory.TestCertificateAsync(sdkTenant);
                var flag = token.TryGetValue(new string[] { "name", "upn", "app_displayname" }, out var username);
                if (showMessage)
                {
                    if (flag)
                    {
                        await _dialogService.ShowWarningAsync(string.Format(MessageResources.LoginSuccessWithAccount, username));
                        return true;
                    }
                    else
                    {
                        await _dialogService.ShowWarningAsync(MessageResources.LoginFailed);
                        return false;
                    }
                }
                else
                {
                    return flag;
                }

            }
            catch (Exception ex)
            {
                await _dialogService.ShowWarningAsync(string.Format(MessageResources.LoginFailedErrorDetail, ex.Message));

            }
            return false;

        }

        [RelayCommand]
        private async Task ConfirmAsync()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(AppName)) errors.Add(MessageResources.AppNameRequired);
            if (LoginUsePassword && string.IsNullOrWhiteSpace(LoginPassword)) errors.Add(MessageResources.LoginPasswordRequired);
            if (string.IsNullOrWhiteSpace(TenantId)) errors.Add(MessageResources.TenantIdRequired);
            if (!Guid.TryParse(TenantId, out Guid _)) errors.Add(string.Format(MessageResources.GuidFormatInvalid, FormResources.LblTenantId));
            if (string.IsNullOrWhiteSpace(TenantName)) errors.Add(MessageResources.TenantNameRequired);
            if (string.IsNullOrWhiteSpace(TenantCode)) errors.Add(MessageResources.TenantCodeRequired);
            if (Credential == AzureCredential.CertificatePassword || Credential == AzureCredential.Certificate)
            {
                if (string.IsNullOrWhiteSpace(ClientId)) errors.Add(MessageResources.ClientIdRequired);
                if (!Guid.TryParse(ClientId, out Guid _)) errors.Add(string.Format(MessageResources.GuidFormatInvalid, FormResources.LblClientId));
                if (string.IsNullOrWhiteSpace(CertificateRawData)) errors.Add(MessageResources.CertificateFileRequired);
                if (Credential == AzureCredential.CertificatePassword && string.IsNullOrWhiteSpace(CertificatePassword)) errors.Add(MessageResources.CertificatePasswordRequired);
            }
            if (TenantMode == TenantMode.Multiple && Credential == AzureCredential.Browser)
            {
                errors.Add(MessageResources.MultipleTenantBrowserNotSupported);
            }
            if (errors.Count > 0)
            {
                await _dialogService.ShowInfoAsync(string.Join("\r\n", errors));
                return;
            }
            if (await TestCertificate(false) == false) return;
            AppProfile profile = new AppProfile()
            {
                Name = AppName,
                TenantMode = TenantMode,
                LoginUsePassword = LoginUsePassword
            };
            if (LoginUsePassword)
            {
                profile.SetPassword(LoginPassword);
            }
            else
            {
                profile.ClearPassword();
            }
            _store.SaveProfile(profile);

            TenantProfile tenant = new TenantProfile
            {
                Name = TenantName ?? string.Empty,
                Code = TenantCode ?? string.Empty,
                Credential = Credential.ToString(),
                TenantId = TenantId ?? string.Empty,
                ClientId = ClientId ?? string.Empty,
                CertificateRawData = CertificateRawData ?? string.Empty,
                Password = CertificatePassword,
            };
            _store.AddOrUpdateTenant(tenant);
            _store.AddProject(new ProjectInfo { Code = "Project1", Name = "Project1" });
            _store.Save();
            if (this.UseDataEncryption) _store.EnableEncryption();
            await _dialogService.ShowInfoAsync(MessageResources.ConfigSaveSuccess);
            Completed?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private async Task SelectCertificateAsync()
        {
            IFileEntry fileEntry = await _dialogService.OpenFileAsync(ModelResources.CertificateFileFilter, "certificate");
            if (fileEntry == null) return;
            try
            {
                byte[] bytes = File.ReadAllBytes(fileEntry.FullPath);
                CertificateRawData = Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(ex.Message, MessageResources.CertificateReadFailed);
            }
        }
    }
}