using Azure.Core;
using AzureTools.Cli;
using AzureTools.Extensions;
using AzureTools.SDK;
using AzureTools.Services;
using AzureTools.Tools.IO;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Models;
using Toolkit.Modules.Properties;
using Toolkit.Modules.Stores;



namespace Toolkit.Modules.Azure.ViewModels.Tenants
{
    public partial class TenantEditorViewModel : DialogViewModel
    {
        #region Constructors

        public TenantEditorViewModel(TenantEditorOption option, IToolkitStore store, IDialogService dialogService, IAzureResourceFactory resourceFactory)
        {
            _store = store;
            _dialogService = dialogService;
            _resourceFactory = resourceFactory;

            CredentialOptions = OptionFactory.Instance.GetCredentials();
            IsEditMode = option.IsEditMode;
            LoadTenant(option.Option);
            TenantMode = _store.GetProfile().TenantMode;
        }

        #endregion

        #region Fields & Properties

        private readonly IToolkitStore _store;
        private readonly IDialogService _dialogService;
        private readonly IAzureResourceFactory _resourceFactory;

        [ObservableProperty]
        private string _tenantCode;

        [ObservableProperty]
        private string _tenantName;

        [ObservableProperty]
        private string _tenantId;

        [ObservableProperty]
        private string _clientId;

        [ObservableProperty]
        private AzureCredential _credential = AzureCredential.DeviceCode;

        [ObservableProperty]
        private Option<AzureCredential> _selectedCredential;


        [ObservableProperty]
        private string _certificateRawData;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isCertificateEnabled;

        [ObservableProperty]
        private bool _isPasswordEnabled;

        public Option<AzureCredential>[] CredentialOptions { get; }
        public bool IsEditMode { get; private set; }
        public TenantMode TenantMode { get; private set; }
        public TenantProfile EditedTenant { get; private set; }
        TenantProfile _tenant;

        public bool? DialogResult { get; private set; }

        partial void OnSelectedCredentialChanged(Option<AzureCredential> value)
        {
            if (value != null)
                Credential = value.Value;
        }

        partial void OnCredentialChanged(AzureCredential value)
        {
            IsCertificateEnabled = value == AzureCredential.Certificate || value == AzureCredential.CertificatePassword;
            IsPasswordEnabled = value == AzureCredential.CertificatePassword;
        }

        #endregion

        void LoadTenant(TenantProfile tenant)
        {

            if (tenant == null) return;
            _tenant = tenant;
            TenantCode = tenant.Code;
            TenantName = tenant.Name;
            TenantId = tenant.TenantId;
            ClientId = tenant.ClientId;
            Password = tenant.Password;
            Credential = Enum.TryParse<AzureCredential>(tenant.Credential, true, out AzureCredential credential) ? credential : AzureCredential.Browser;
            CertificateRawData = tenant.CertificateRawData;
            IsCertificateEnabled = Credential == AzureCredential.Certificate || Credential == AzureCredential.CertificatePassword;
            IsPasswordEnabled = Credential == AzureCredential.CertificatePassword;
            SelectedCredential = CredentialOptions.FirstOrDefault(x => x.Value == Credential);
        }


        [RelayCommand]
        private async Task CerFileAsync()
        {
            await ShowDialogKeyed("Cer");
        }
        [RelayCommand]
        private async Task SelectCertificateAsync()
        {
            IFileEntry fileEntry = await _dialogService.OpenFileAsync(ModelResources.CertificateFileFilter, "certificate");
            if (fileEntry == null) return;
            CertificateRawData = Convert.ToBase64String(fileEntry.GetBuffer());
        }

        [RelayCommand]
        private async Task TestConnectionAsync()
        {
            await TestCertificateAsync(true);
        }
        private async Task<bool> TestCertificateAsync(bool showMessage)
        {


            List<string> errors = await ValidateInput();
            if (errors.Count > 0)
            {
                if (showMessage) await _dialogService.ShowWarningAsync(string.Join("\r\n", errors));
                return false;
            }

            TenantProfile profile = CreateTenantFromForm();
            try
            {
                IDictionary<string, string> token = await _resourceFactory.TestCertificateAsync(profile.ToSdk());
                bool flag = token.TryGetValue(new string[] { "name", "upn", "app_displayname" }, out string username);
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
                if (showMessage) await _dialogService.ShowWarningAsync(string.Format(MessageResources.LoginFailedWithDetail, ex.Message));
            }
            return false;
        }
        protected override async Task ConfirmAsync()
        {
            List<string> errors = await ValidateInput();
            if (TenantMode == TenantMode.Multiple && Credential == AzureCredential.Browser)
            {
                errors.Add(MessageResources.MultipleTenantBrowserNotSupported);
            }
            if (errors.Count > 0)
            {
                await _dialogService.ShowWarningAsync(string.Join("\r\n", errors));
                return;
            }
            if (await TestCertificateAsync(false) == false) return;
            if (IsEditMode)
            {
                _tenant.Code = TenantCode;
                _tenant.Name = TenantName;
                _tenant.TenantId = TenantId;
                _tenant.ClientId = ClientId;
                _tenant.Credential = Credential.ToString();
                _tenant.Password = Password;
                _tenant.CertificateRawData = CertificateRawData;
                EditedTenant = _tenant;
            }
            else
            {
                EditedTenant = CreateTenantFromForm();
            }
            DialogResult = true;
            GetWindow().Close();
        }



        private async Task<List<string>> ValidateInput()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(TenantCode)) errors.Add(FormResources.LblTenantCode);
            if (string.IsNullOrWhiteSpace(TenantName)) errors.Add(FormResources.LblTenantName);
            if (string.IsNullOrWhiteSpace(TenantId)) errors.Add(FormResources.LblTenantId);
            if (!Guid.TryParse(TenantId, out _)) errors.Add(string.Format(MessageResources.GuidFormatInvalid, FormResources.LblTenantId));

            if (_tenant == null && _store.ExistTenant(TenantId))
            {
                errors.Add(FormResources.LblTenantIdExist);
            }
            if (Credential == AzureCredential.Certificate || Credential == AzureCredential.CertificatePassword)
            {
                if (string.IsNullOrWhiteSpace(ClientId)) errors.Add(FormResources.LblClientId);
                if (!Guid.TryParse(ClientId, out _)) errors.Add(string.Format(MessageResources.GuidFormatInvalid, FormResources.LblClientId));

                if (Credential == AzureCredential.CertificatePassword && string.IsNullOrWhiteSpace(Password)) errors.Add(MessageResources.CertificatePasswordRequired);
            }
            if (Credential == AzureCredential.AzureCli)
            {
                bool flag = await CliClientBuilder.ExistAsync("az");
                if (!flag) errors.Add("AzureCli not exist");
            }
            return errors;
        }

        private TenantProfile CreateTenantFromForm()
        {

            return new TenantProfile
            {
                Code = TenantCode,
                Name = TenantName,
                TenantId = TenantId,
                ClientId = ClientId,
                Credential = Credential.ToString(),
                Password = Password,
                CertificateRawData = CertificateRawData,
            };
        }
    }
}
