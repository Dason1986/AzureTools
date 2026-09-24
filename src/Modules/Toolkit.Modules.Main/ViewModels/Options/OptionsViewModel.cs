
using AzureTools;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Toolkit.Modules.Models;
using Toolkit.Modules.Properties;
using Toolkit.Modules.Stores;

namespace Toolkit.Modules.Main.ViewModels.Options
{
    public partial class OptionsViewModel : ViewModel
    {
        #region Constructors

        public OptionsViewModel(IToolkitStore store, IDialogService dialogService, IThemeService themeService)
        {
            _store = store;
            _dialogService = dialogService;
            _themeService = themeService;
            CanClose = HandleCloseAsync;

            ThemeModeOptions = OptionFactory.Instance.GetThemeModes();

            LoadFromProfile();
        }

        #endregion

        #region Fields & Properties

        private readonly IToolkitStore _store;
        private readonly IDialogService _dialogService;
        private readonly IThemeService _themeService;




        [ObservableProperty]
        private Option<ThemeMode> _selectedThemeMode;

        [ObservableProperty]
        private bool _loginUsePassword;

        [ObservableProperty]
        private bool _hasChanges;



        public Option<ThemeMode>[] ThemeModeOptions { get; }

        #endregion


        public ThemeMode ThemeMode => SelectedThemeMode?.Value ?? ThemeMode.System;

        partial void OnSelectedThemeModeChanged(Option<ThemeMode> value)
        {
            if (value == null) return;
            _themeService.ApplyTheme(value.Value);
            HasChanges = true;
        }

        // 启用密码：弹出输入窗口，确认后直接保存
        [RelayCommand]
        private async Task EnablePasswordAsync()
        {
            string password = await _dialogService.ShowPromptDialogAsync(
                FormResources.TitleEnablePassword,
                FormResources.PromptEnablePassword,
                PromptDialogOption.PasswordOption);

            if (password == null) return;
            if (string.IsNullOrEmpty(password))
            {
                await _dialogService.ShowWarningAsync(MessageResources.PasswordCannotBeEmpty);
                return;
            }

            AppProfile profile = _store.GetProfile() ?? new AppProfile();
            profile.SetPassword(password);
            _store.SaveProfile(profile);
            _store.Save();

            LoginUsePassword = true;
            await _dialogService.ShowInfoAsync(MessageResources.PasswordEnabledSuccess);
        }

        // 取消密码：弹出窗口验证原密码，通过后清除密码并保存
        [RelayCommand]
        private async Task DisablePasswordAsync()
        {
            string password = await _dialogService.ShowPromptDialogAsync(
                FormResources.TitleDisablePassword,
                FormResources.PromptDisablePassword,
                PromptDialogOption.PasswordOption);

            if (password == null) return;

            AppProfile profile = _store.GetProfile();
            if (profile == null || !profile.IsAuthenticated(password))
            {
                await _dialogService.ShowWarningAsync(MessageResources.PasswordIncorrect);
                return;
            }

            profile.ClearPassword();
            _store.SaveProfile(profile);
            _store.Save();

            LoginUsePassword = false;
            await _dialogService.ShowInfoAsync(MessageResources.PasswordDisabledSuccess);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            await SaveCoreAsync();
            GetWindow().Close();
        }

        private async Task SaveCoreAsync()
        {
            AppProfile profile = _store.GetProfile() ?? new AppProfile();
            profile.ThemeMode = ThemeMode;

            _store.SaveProfile(profile);
            _store.Save();
            HasChanges = false;
            await _dialogService.ShowInfoAsync(FormResources.MsgOptionsSaved);
        }

        [RelayCommand]
        private async Task AboutAsync()
        {
            await _dialogService.OpenWebViewAsync("https://github.com/dason1986/AzureTools", FormResources.TitleAbout);
        }

        [RelayCommand]
        private void DiscardChanges()
        {
            LoadFromProfile();
            HasChanges = false;
        }

        private async Task<bool> HandleCloseAsync()
        {
            if (!HasChanges) return true;
            bool save = await _dialogService.ShowConfirmAsync(MessageResources.ConfirmSaveUnsavedChanges, MessageResources.TitleUnsavedChanges);
            if (save)
            {
                await SaveCoreAsync();
                return true;
            }
            bool discard = await _dialogService.ShowConfirmAsync(MessageResources.ConfirmDiscardChanges, MessageResources.TitleDiscardChanges);
            if (!discard) return false;
            DiscardChanges();
            return true;
        }

        private void LoadFromProfile()
        {
            AppProfile profile = _store.GetProfile();

            ThemeMode themeMode = profile.ThemeMode;
            SelectedThemeMode = System.Array.Find(ThemeModeOptions, x => x.Value == themeMode);
            LoginUsePassword = profile.LoginUsePassword;
            HasChanges = false;
        }
    }
}
