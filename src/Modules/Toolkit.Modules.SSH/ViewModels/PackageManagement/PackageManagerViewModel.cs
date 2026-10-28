using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Managers;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.PackageManagement;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.PackageManagement
{
    public partial class PackageManagerViewModel : ViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly SshPackageManager _manager;
        private HashSet<string> _installedNames = new HashSet<string>();
        private IReadOnlyList<PackageInfo> _allInstalled = Array.Empty<PackageInfo>();

        public PackageManagerViewModel(SshHostProfile host, ISshConnectionManager connectionManager, IDialogService dialogService)
        {
            _dialogService = dialogService;
            _manager = new SshPackageManager(connectionManager, host);
            WindowTitle = string.Format(SSHResource.TitlePackageManager, host.Name);
            OsSummary = string.IsNullOrWhiteSpace(host.Os) ? SSHResource.MsgOsUnknown : host.Os;
            _ = InitializeAsync();
        }

        public string OsSummary { get; }

        [ObservableProperty]
        private string _managerSummary;

        [ObservableProperty]
        private string _keyword;

        [ObservableProperty]
        private string _installedFilter;

        [ObservableProperty]
        private ObservableCollection<PackageInfo> _searchResults = new ObservableCollection<PackageInfo>();

        [ObservableProperty]
        private ObservableCollection<PackageInfo> _installedPackages = new ObservableCollection<PackageInfo>();

        [ObservableProperty]
        private PackageInfo _selectedSearchPackage;

        [ObservableProperty]
        private PackageInfo _selectedInstalledPackage;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusText;

        [ObservableProperty]
        private string _terminalText;

        private async Task InitializeAsync()
        {
            if (!await EnsureDetectedAsync()) return;
            await RefreshAsync();
        }

        [RelayCommand(CanExecute = nameof(CanSearch))]
        private async Task SearchAsync()
        {
            string keyword = Keyword?.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                await _dialogService.ShowWarningAsync(SSHResource.MsgPackageKeywordRequired);
                return;
            }
            if (!await EnsureDetectedAsync()) return;
            IsBusy = true;
            try
            {
                IReadOnlyList<PackageInfo> results = await _manager.SearchAsync(keyword);
                MarkInstalled(results);
                SearchResults = new ObservableCollection<PackageInfo>(results);
                StatusText = string.Format(SSHResource.MsgPackageSearchDone, results.Count);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgPackageSearchFailed, ex.Message), WindowTitle);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanOperate))]
        private async Task RefreshAsync()
        {
            if (!await EnsureDetectedAsync()) return;
            IsBusy = true;
            try
            {
                IReadOnlyList<PackageInfo> installed = await _manager.ListInstalledAsync();
                _installedNames = installed.Select(p => p.Name).ToHashSet();
                _allInstalled = installed;
                ApplyInstalledFilter();
                MarkInstalled(SearchResults);
                StatusText = string.Format(SSHResource.MsgPackageInstalledCount, installed.Count);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgPackageSearchFailed, ex.Message), WindowTitle);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanInstall))]
        private async Task InstallAsync()
        {
            PackageInfo target = SelectedSearchPackage;
            if (target == null) return;
            await RunPackageOperationAsync(
                () => _manager.InstallAsync(target.Name),
                string.Format(SSHResource.MsgConfirmPackageInstall, target.Name),
                string.Format(SSHResource.MsgPackageInstalling, target.Name),
                string.Format(SSHResource.MsgPackageInstallDone, target.Name));
        }

        [RelayCommand(CanExecute = nameof(CanOperateInstalled))]
        private async Task RemoveAsync()
        {
            PackageInfo target = SelectedInstalledPackage;
            if (target == null) return;
            await RunPackageOperationAsync(
                () => _manager.RemoveAsync(target.Name),
                string.Format(SSHResource.MsgConfirmPackageRemove, target.Name),
                string.Format(SSHResource.MsgPackageRemoving, target.Name),
                string.Format(SSHResource.MsgPackageRemoveDone, target.Name));
        }

        [RelayCommand(CanExecute = nameof(CanOperateInstalled))]
        private async Task UpgradeAsync()
        {
            PackageInfo target = SelectedInstalledPackage;
            if (target == null) return;
            await RunPackageOperationAsync(
                () => _manager.UpgradeAsync(target.Name),
                string.Format(SSHResource.MsgConfirmPackageUpgrade, target.Name),
                string.Format(SSHResource.MsgPackageUpgrading, target.Name),
                string.Format(SSHResource.MsgPackageUpgradeDone, target.Name));
        }

        [RelayCommand(CanExecute = nameof(CanOperateInstalled))]
        private async Task DowngradeAsync()
        {
            PackageInfo target = SelectedInstalledPackage;
            if (target == null) return;
            string version = target.Version;
            if (_manager.Catalog?.DowngradeRequiresVersion == true)
            {
                version = await _dialogService.ShowPromptDialogAsync(
                    SSHResource.TitlePackageDowngradeVersion,
                    string.Format(SSHResource.PromptPackageDowngradeVersion, target.Name, target.Version),
                    new PromptDialogOption { DefaultValue = target.Version });
                if (string.IsNullOrWhiteSpace(version)) return;
            }
            await RunPackageOperationAsync(
                () => _manager.DowngradeAsync(target.Name, version.Trim()),
                string.Format(SSHResource.MsgConfirmPackageDowngrade, target.Name, version),
                string.Format(SSHResource.MsgPackageDowngrading, target.Name),
                string.Format(SSHResource.MsgPackageDowngradeDone, target.Name));
        }

        private async Task RunPackageOperationAsync(Func<Task<PackageOperationResult>> execute, string confirmText, string runningText, string doneText)
        {
            if (!await _dialogService.ShowConfirmAsync(confirmText)) return;
            IsBusy = true;
            try
            {
                StatusText = runningText;
                PackageOperationResult result = await execute();
                AppendTerminal(result);
                if (!result.Success)
                {
                    string reason = string.IsNullOrEmpty(result.Error) ? "exit " + result.ExitStatus : result.Error;
                    await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgPackageOperateFailed, reason), WindowTitle);
                    return;
                }
                StatusText = doneText;
                await RefreshInstalledSilentlyAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgPackageOperateFailed, ex.Message), WindowTitle);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AppendTerminal(PackageOperationResult result)
        {
            IEnumerable<string> parts = new[] { "$ " + result.Command, result.Output?.TrimEnd(), string.IsNullOrEmpty(result.Error) ? null : result.Error.TrimEnd(), "[exit " + result.ExitStatus + "]" };
            string block = string.Join(Environment.NewLine, parts.Where(p => !string.IsNullOrEmpty(p)));
            TerminalText = string.IsNullOrEmpty(TerminalText) ? block : TerminalText + Environment.NewLine + Environment.NewLine + block;
        }

        private async Task<bool> EnsureDetectedAsync()
        {
            if (_manager.Catalog != null) return true;
            IsBusy = true;
            try
            {
                PackageManagerCatalog catalog = await _manager.DetectAsync();
                if (catalog == null)
                {
                    ManagerSummary = SSHResource.MsgPackageManagerNotFound;
                    await _dialogService.ShowErrorAsync(SSHResource.MsgPackageManagerNotFound, WindowTitle);
                    return false;
                }
                ManagerSummary = string.Format(SSHResource.MsgPackageManagerDetected, catalog.Kind, catalog.Binary);
                await RefreshInstalledSilentlyAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgPackageSearchFailed, ex.Message), WindowTitle);
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshInstalledSilentlyAsync()
        {
            try
            {
                IReadOnlyList<PackageInfo> installed = await _manager.ListInstalledAsync();
                _installedNames = installed.Select(p => p.Name).ToHashSet();
                _allInstalled = installed;
                ApplyInstalledFilter();
                MarkInstalled(SearchResults);
            }
            catch
            {
            }
        }

        private void ApplyInstalledFilter()
        {
            string filter = InstalledFilter?.Trim();
            IEnumerable<PackageInfo> items = _allInstalled;
            if (!string.IsNullOrEmpty(filter))
            {
                items = items.Where(p => p.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true || p.Description?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true);
            }
            InstalledPackages = new ObservableCollection<PackageInfo>(items);
            if (SelectedInstalledPackage != null && !InstalledPackages.Contains(SelectedInstalledPackage))
            {
                SelectedInstalledPackage = null;
            }
        }

        private void MarkInstalled(IEnumerable<PackageInfo> packages)
        {
            foreach (PackageInfo package in packages)
            {
                package.IsInstalled = _installedNames.Contains(package.Name);
            }
        }

        private bool CanSearch() => !IsBusy;

        private bool CanOperate() => !IsBusy;

        private bool CanInstall() => !IsBusy && SelectedSearchPackage != null;

        private bool CanOperateInstalled() => !IsBusy && SelectedInstalledPackage != null;

        partial void OnInstalledFilterChanged(string value)
        {
            ApplyInstalledFilter();
        }

        partial void OnSelectedSearchPackageChanged(PackageInfo value)
        {
            InstallCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedInstalledPackageChanged(PackageInfo value)
        {
            RemoveCommand.NotifyCanExecuteChanged();
            UpgradeCommand.NotifyCanExecuteChanged();
            DowngradeCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsBusyChanged(bool value)
        {
            SearchCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
            InstallCommand.NotifyCanExecuteChanged();
            RemoveCommand.NotifyCanExecuteChanged();
            UpgradeCommand.NotifyCanExecuteChanged();
            DowngradeCommand.NotifyCanExecuteChanged();
        }
    }
}
