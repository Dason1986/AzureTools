using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Models;
using Toolkit.Modules.SSH.FirewallManagement;
using Toolkit.Modules.SSH.Managers;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.FirewallManagement
{
    public partial class FirewallManagerViewModel : ViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly SshFirewallManager _manager;

        public FirewallManagerViewModel(SshHostProfile host, ISshConnectionManager connectionManager, IDialogService dialogService)
        {
            _dialogService = dialogService;
            _manager = new SshFirewallManager(connectionManager, host);
            WindowTitle = string.Format(SSHResource.TitleFirewallManager, host.Name);
            OsSummary = string.IsNullOrWhiteSpace(host.Os) ? SSHResource.MsgOsUnknown : host.Os;
            _ = InitializeAsync();
        }

        public string OsSummary { get; }

        [ObservableProperty]
        private string _firewallSummary;

        [ObservableProperty]
        private string _firewallStatus;

        [ObservableProperty]
        private bool _isEnabled;

        [ObservableProperty]
        private bool _hasToggleSupport;

        [ObservableProperty]
        private string _toggleText;

        [ObservableProperty]
        private string _newPort;

        [ObservableProperty]
        private Option<string> _selectedProtocol;

        [ObservableProperty]
        private Option<string> _selectedAction;

        [ObservableProperty]
        private ObservableCollection<FirewallRule> _rules = new ObservableCollection<FirewallRule>();

        [ObservableProperty]
        private FirewallRule _selectedRule;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusText;

        [ObservableProperty]
        private string _terminalText;

        public Option<string>[] ProtocolOptions { get; } =
        {
            new Option<string>("tcp", "TCP"),
            new Option<string>("udp", "UDP")
        };

        public Option<string>[] ActionOptions { get; } =
        {
            new Option<string>("allow", SSHResource.FirewallActionAllow),
            new Option<string>("deny", SSHResource.FirewallActionDeny)
        };

        private async Task InitializeAsync()
        {
            if (!await EnsureDetectedAsync()) return;
            await RefreshCoreAsync();
        }

        [RelayCommand(CanExecute = nameof(CanOperate))]
        private async Task RefreshAsync()
        {
            if (!await EnsureDetectedAsync()) return;
            IsBusy = true;
            try
            {
                await RefreshCoreAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgFirewallOperateFailed, ex.Message), WindowTitle);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanToggle))]
        private async Task ToggleEnabledAsync()
        {
            bool enable = !IsEnabled;
            string confirmText = enable ? SSHResource.MsgConfirmFirewallEnable : SSHResource.MsgConfirmFirewallDisable;
            if (!await _dialogService.ShowConfirmAsync(confirmText)) return;
            IsBusy = true;
            try
            {
                StatusText = enable ? SSHResource.MsgFirewallEnabling : SSHResource.MsgFirewallDisabling;
                PackageOperationResult result = await _manager.SetEnabledAsync(enable);
                AppendTerminal(result);
                if (!result.Success)
                {
                    await ShowOperateErrorAsync(result);
                    return;
                }
                await RefreshCoreAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgFirewallOperateFailed, ex.Message), WindowTitle);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanOperate))]
        private async Task AddRuleAsync()
        {
            string port = NewPort?.Trim();
            if (!IsValidPort(port))
            {
                await _dialogService.ShowWarningAsync(SSHResource.MsgFirewallPortInvalid, WindowTitle);
                return;
            }
            string protocol = SelectedProtocol?.Value ?? "tcp";
            bool allow = SelectedAction?.Value != "deny";
            IsBusy = true;
            try
            {
                StatusText = string.Format(SSHResource.MsgFirewallAddingRule, port, protocol);
                PackageOperationResult result = await _manager.AddRuleAsync(port, protocol, allow);
                AppendTerminal(result);
                if (!result.Success)
                {
                    await ShowOperateErrorAsync(result);
                    return;
                }
                NewPort = string.Empty;
                StatusText = SSHResource.MsgFirewallRuleAdded;
                await RefreshCoreAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgFirewallOperateFailed, ex.Message), WindowTitle);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanRemove))]
        private async Task RemoveRuleAsync()
        {
            FirewallRule target = SelectedRule;
            if (target == null) return;
            if (!await _dialogService.ShowConfirmAsync(string.Format(SSHResource.MsgConfirmFirewallRemoveRule, DescribeRule(target)))) return;
            IsBusy = true;
            try
            {
                StatusText = string.Format(SSHResource.MsgFirewallRemovingRule, DescribeRule(target));
                PackageOperationResult result = await _manager.RemoveRuleAsync(target);
                AppendTerminal(result);
                if (!result.Success)
                {
                    await ShowOperateErrorAsync(result);
                    return;
                }
                StatusText = SSHResource.MsgFirewallRuleRemoved;
                await RefreshCoreAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgFirewallOperateFailed, ex.Message), WindowTitle);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshCoreAsync()
        {
            bool enabled = await _manager.IsEnabledAsync();
            IReadOnlyList<FirewallRule> rules = await _manager.ListRulesAsync();
            IsEnabled = enabled;
            FirewallStatus = enabled ? SSHResource.MsgFirewallRunning : SSHResource.MsgFirewallStopped;
            ToggleText = enabled ? SSHResource.BtnFirewallDisable : SSHResource.BtnFirewallEnable;
            Rules = new ObservableCollection<FirewallRule>(rules);
            if (SelectedRule != null && !Rules.Contains(SelectedRule)) SelectedRule = null;
            StatusText = string.Format(SSHResource.MsgFirewallRuleCount, rules.Count);
        }

        private async Task<bool> EnsureDetectedAsync()
        {
            if (_manager.Catalog != null) return true;
            IsBusy = true;
            try
            {
                FirewallBackendCatalog catalog = await _manager.DetectAsync();
                if (catalog == null)
                {
                    FirewallSummary = SSHResource.MsgFirewallNotFound;
                    await _dialogService.ShowErrorAsync(SSHResource.MsgFirewallNotFound, WindowTitle);
                    return false;
                }
                FirewallSummary = string.Format(SSHResource.MsgFirewallDetected, catalog.Kind, catalog.Binary);
                HasToggleSupport = catalog.SupportsToggle;
                return true;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgFirewallOperateFailed, ex.Message), WindowTitle);
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShowOperateErrorAsync(PackageOperationResult result)
        {
            string reason = string.IsNullOrEmpty(result.Error) ? "exit " + result.ExitStatus : result.Error;
            await _dialogService.ShowErrorAsync(string.Format(SSHResource.MsgFirewallOperateFailed, reason), WindowTitle);
        }

        private void AppendTerminal(PackageOperationResult result)
        {
            IEnumerable<string> parts = new[] { "$ " + result.Command, result.Output?.TrimEnd(), string.IsNullOrEmpty(result.Error) ? null : result.Error.TrimEnd(), "[exit " + result.ExitStatus + "]" };
            string block = string.Join(Environment.NewLine, parts.Where(p => !string.IsNullOrEmpty(p)));
            TerminalText = string.IsNullOrEmpty(TerminalText) ? block : TerminalText + Environment.NewLine + Environment.NewLine + block;
        }

        private static string DescribeRule(FirewallRule rule)
        {
            string port = rule.Protocol == "service" ? rule.Port : string.Join("/", new[] { rule.Port, rule.Protocol }.Where(p => !string.IsNullOrEmpty(p)));
            return string.Join(" ", new[] { rule.Number != null ? "#" + rule.Number : null, port, rule.Action }.Where(p => !string.IsNullOrEmpty(p)));
        }

        private static bool IsValidPort(string port)
        {
            if (string.IsNullOrEmpty(port)) return false;
            string[] parts = port.Split(':');
            if (parts.Length > 2) return false;
            return parts.All(p => int.TryParse(p, out int value) && value is > 0 and <= 65535);
        }

        private bool CanOperate() => !IsBusy;

        private bool CanToggle() => !IsBusy && HasToggleSupport;

        private bool CanRemove() => !IsBusy && SelectedRule != null;

        partial void OnSelectedRuleChanged(FirewallRule value)
        {
            RemoveRuleCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsBusyChanged(bool value)
        {
            RefreshCommand.NotifyCanExecuteChanged();
            ToggleEnabledCommand.NotifyCanExecuteChanged();
            AddRuleCommand.NotifyCanExecuteChanged();
            RemoveRuleCommand.NotifyCanExecuteChanged();
        }
    }
}
