using Avalonia.Controls;
using AzureTools.Extensions;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Models;
using Toolkit.Modules.SSH.Managers;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Stores;
using Toolkit.Modules.SSH.Views.SshHosts;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public partial class SshHostEditorViewModel : DialogViewModel
    {
        private readonly ISshHostStore _store;
        private readonly IDialogService _dialogService;
        private readonly ISshConnectionTestService _testService;

        public SshHostEditorViewModel(SshHostEditorOption option, ISshHostStore store, IDialogService dialogService, ISshConnectionTestService testService)
        {
            _store = store;
            _dialogService = dialogService;
            _testService = testService;
            AuthModeOptions = new[]
            {
                new Option<SshAuthMode>(SshAuthMode.Password, "密码"),
                new Option<SshAuthMode>(SshAuthMode.PrivateKey, "私钥")
            };
            IsEditMode = option.IsEditMode;
            FeatureTabs = CreateFeatureTabs(option.FeatureTabFactories);
            FeatureTabItems = FeatureTabs.Select(vm => new TabItem
            {
                Header = vm.DisplayName,
                Content = FeatureTabViewFactory.CreateView(vm)
            }).ToArray();
            LoadHost(option.Option);
            _ = LoadFeatureTabsAsync();
        }

        private static SshFeatureTabViewModel[] CreateFeatureTabs(IReadOnlyDictionary<string, Func<SshFeatureTabViewModel>> factories)
        {
            if (factories == null || factories.Count == 0) return Array.Empty<SshFeatureTabViewModel>();
            return factories.Values.Select(f => f()).OrderByByCode().ToArray();
        }

        public Option<SshAuthMode>[] AuthModeOptions { get; }
        public IReadOnlyList<SshFeatureTabViewModel> FeatureTabs { get; }
        public IReadOnlyList<TabItem> FeatureTabItems { get; }
        public bool IsEditMode { get; private set; }
        public SshHostProfile EditedHost { get; private set; }

        [ObservableProperty]
        private string _hostName;

        [ObservableProperty]
        private string _host;

        [ObservableProperty]
        private int _port = 22;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private SshAuthMode _authMode = SshAuthMode.Password;

        [ObservableProperty]
        private Option<SshAuthMode> _selectedAuthMode;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _privateKeyRawData;

        [ObservableProperty]
        private string _passphrase;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private bool _isPasswordEnabled = true;

        [ObservableProperty]
        private bool _isPrivateKeyEnabled;

        [ObservableProperty]
        private bool _isTesting;

        protected override async Task ConfirmAsync()
        {
            SshHostProfile tempHost = CreateTempHost();
            try
            {
                IsTesting = true;
                IDictionary<string, string> profile = await _testService.GetProfileAsync(tempHost);
                tempHost.Os = profile.GetValue<string>("PRETTY_NAME");
                tempHost.OsVersion = profile.GetValue<string>("VERSION_ID");
                tempHost.OsId = profile.GetValue<string>("ID");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"连接测试失败：{ex.Message}", "连接测试");
                return;
            }
            finally
            {
                IsTesting = false;
            }

            ApplyToHost(tempHost);
            await base.ConfirmAsync();
        }

        [RelayCommand]
        async Task SelectPrivateKey()
        {
        }

        partial void OnSelectedAuthModeChanged(Option<SshAuthMode> value)
        {
            if (value != null) AuthMode = value.Value;
        }

        partial void OnAuthModeChanged(SshAuthMode value)
        {
            IsPasswordEnabled = value == SshAuthMode.Password;
            IsPrivateKeyEnabled = value == SshAuthMode.PrivateKey;
        }

        private void LoadHost(SshHostProfile host)
        {
            if (host == null) return;
            EditedHost = host;
            HostName = host.Name;
            Host = host.Host;
            Port = host.Port;
            UserName = host.UserName;
            AuthMode = Enum.TryParse<SshAuthMode>(host.AuthMode, true, out SshAuthMode mode) ? mode : SshAuthMode.Password;
            Password = host.Password;
            PrivateKeyRawData = host.PrivateKeyRawData;
            Passphrase = host.Passphrase;
            Description = host.Description;
            SelectedAuthMode = AuthModeOptions.FirstOrDefault(x => x.Value == AuthMode);
        }

        private async Task LoadFeatureTabsAsync()
        {
            foreach (SshFeatureTabViewModel tab in FeatureTabs)
            {
                await tab.LoadAsync(EditedHost);
            }
        }

        private SshHostProfile CreateTempHost()
        {
            SshHostProfile host = EditedHost ?? new SshHostProfile { Id = Guid.NewGuid() };
            ApplyToHost(host);
            return host;
        }

        private void ApplyToHost(SshHostProfile host)
        {
            host.Name = HostName;
            host.Host = Host;
            host.Port = Port;
            host.UserName = UserName;
            host.AuthMode = AuthMode.ToString();
            host.Password = Password;
            host.PrivateKeyRawData = PrivateKeyRawData;
            host.Passphrase = Passphrase;
            host.Description = Description;
            foreach (SshFeatureTabViewModel tab in FeatureTabs)
            {
                tab.Apply(host);
            }
            EditedHost = host;
        }
    }

    internal static class SshFeatureTabEnumerableExtensions
    {
        public static IOrderedEnumerable<SshFeatureTabViewModel> OrderByByCode(this IEnumerable<SshFeatureTabViewModel> source)
        {
            string[] order = { SshHostFeature.Docker, SshHostFeature.Podman, SshHostFeature.Terminal, SshHostFeature.FileBrowser };
            return source.OrderBy(t =>
            {
                int idx = Array.IndexOf(order, t.Code);
                return idx < 0 ? int.MaxValue : idx;
            });
        }
    }
}
