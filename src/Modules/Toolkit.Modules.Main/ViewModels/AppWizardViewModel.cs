using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Main.Properties;
using Toolkit.Modules.Models;
using Toolkit.Modules.Stores;



namespace Toolkit.Modules.Main.ViewModels
{
    public partial class AppWizardViewModel : ViewModel
    {
        #region Constructors

        public AppWizardViewModel(IToolkitStore store, IDialogService dialogService)
        {
            _store = store;

            _dialogService = dialogService;



            TenantModeOptions = OptionFactory.Instance.GetTenantModes();


            SelectedTenantMode = TenantModeOptions.FirstOrDefault(x => x.Value == TenantMode);
        }

        #endregion

        #region Fields & Properties

        private readonly IToolkitStore _store;

        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _appName = "Azure Tools";

        [ObservableProperty]
        private TenantMode _tenantMode = TenantMode.Single;



        [ObservableProperty]
        private Option<TenantMode> _selectedTenantMode;



        [ObservableProperty]
        private bool _loginUsePassword;

        [ObservableProperty]
        private bool _useDataEncryption;


        [ObservableProperty]
        private string _loginPassword;




        public Option<TenantMode>[] TenantModeOptions { get; }

        public bool Completed { get; private set; }



        partial void OnSelectedTenantModeChanged(Option<TenantMode> value)
        {
            if (value != null)
                TenantMode = value.Value;
        }

        #endregion



        [RelayCommand]
        private async Task ConfirmAsync()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(AppName)) errors.Add(MainResource.AppNameRequired);
            if (LoginUsePassword && string.IsNullOrWhiteSpace(LoginPassword)) errors.Add(MainResource.LoginPasswordRequired);

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


            _store.AddProject(new ProjectInfo { Code = "Project1", Name = "Project1" });
            _store.Save();
            if (this.UseDataEncryption) _store.EnableEncryption();
            await _dialogService.ShowInfoAsync(MainResource.SaveSuccess);
            Completed = true;
            await CloseAsync();
        }


    }
}