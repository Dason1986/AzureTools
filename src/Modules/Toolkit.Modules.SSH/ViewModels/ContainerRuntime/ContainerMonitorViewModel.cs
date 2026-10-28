using Avalonia.Threading;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class ContainerMonitorViewModel : ViewModel, IDisposable
    {
        private const int StatsPollIntervalMs = 2000;
        private const int MaxConsecutiveFailures = 10;
        private readonly IContainerRuntimeManager _runtime;
        private readonly CancellationTokenSource _cts = new();
        private ContainerStats _previousStats;
        private DateTime _previousTimestamp;
        private int _consecutiveFailures;
        private bool _disposed;

        public ContainerMonitorViewModel(SshHostProfile host, ContainerMonitorOption option, IContainerRuntimeManagerFactory runtimeFactory, IDialogService dialog)
        {
            _runtime = runtimeFactory.Create(host, option.Engine);
            WindowTitle = string.Format(SSHResource.TitleContainerMonitor, option.ContainerName);
            InitializeDisplayText();
            _ = Task.Run(() => PollStatsAsync(option.ContainerId, _cts.Token));
        }

        public event Action StatsDataUpdated;

        [ObservableProperty]
        private double _latestCpuPercent;

        [ObservableProperty]
        private double _latestMemoryPercent;

        [ObservableProperty]
        private double _latestDiskReadRate;

        [ObservableProperty]
        private double _latestDiskWriteRate;

        [ObservableProperty]
        private double _latestNetworkRxRate;

        [ObservableProperty]
        private double _latestNetworkTxRate;

        [ObservableProperty]
        private string _cpuDisplayText;

        [ObservableProperty]
        private string _memoryDisplayText;

        [ObservableProperty]
        private string _diskDisplayText;

        [ObservableProperty]
        private string _networkDisplayText;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _cts.Cancel();
            _cts.Dispose();
            (_runtime as IDisposable)?.Dispose();
        }

        private void InitializeDisplayText()
        {
            CpuDisplayText = string.Format(SSHResource.LblMonitorCpu, 0d);
            MemoryDisplayText = string.Format(SSHResource.LblMonitorMemory, 0d);
            DiskDisplayText = string.Format(SSHResource.LblMonitorDisk, FormatRate(0), FormatRate(0));
            NetworkDisplayText = string.Format(SSHResource.LblMonitorNetwork, FormatRate(0), FormatRate(0));
        }

        private async Task PollStatsAsync(string containerId, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    ContainerStats stats = await _runtime.GetContainerStatsAsync(containerId, ct).ConfigureAwait(false);
                    _consecutiveFailures = 0;
                    ApplyStats(stats);
                }
                catch (OperationCanceledException) { break; }
                catch
                {
                    _consecutiveFailures++;
                    if (_consecutiveFailures >= MaxConsecutiveFailures) break;
                }
                try { await Task.Delay(StatsPollIntervalMs, ct).ConfigureAwait(false); } catch { break; }
            }
        }

        private void ApplyStats(ContainerStats stats)
        {
            DateTime now = DateTime.UtcNow;
            double seconds = _previousStats == null ? 0 : (now - _previousTimestamp).TotalSeconds;
            double diskReadRate = ComputeRate(stats.BlockReadBytes, _previousStats?.BlockReadBytes ?? 0, seconds);
            double diskWriteRate = ComputeRate(stats.BlockWriteBytes, _previousStats?.BlockWriteBytes ?? 0, seconds);
            double netRxRate = ComputeRate(stats.NetworkReceivedBytes, _previousStats?.NetworkReceivedBytes ?? 0, seconds);
            double netTxRate = ComputeRate(stats.NetworkTransmittedBytes, _previousStats?.NetworkTransmittedBytes ?? 0, seconds);
            _previousStats = stats;
            _previousTimestamp = now;
            Dispatcher.UIThread.Post(() => UpdateDisplay(stats, diskReadRate, diskWriteRate, netRxRate, netTxRate));
        }

        private void UpdateDisplay(ContainerStats stats, double diskReadRate, double diskWriteRate, double netRxRate, double netTxRate)
        {
            LatestCpuPercent = stats.CpuPercent;
            LatestMemoryPercent = stats.MemoryPercent;
            LatestDiskReadRate = diskReadRate;
            LatestDiskWriteRate = diskWriteRate;
            LatestNetworkRxRate = netRxRate;
            LatestNetworkTxRate = netTxRate;
            CpuDisplayText = string.Format(SSHResource.LblMonitorCpu, stats.CpuPercent);
            MemoryDisplayText = string.Format(SSHResource.LblMonitorMemory, stats.MemoryPercent);
            DiskDisplayText = string.Format(SSHResource.LblMonitorDisk, FormatRate(diskReadRate), FormatRate(diskWriteRate));
            NetworkDisplayText = string.Format(SSHResource.LblMonitorNetwork, FormatRate(netRxRate), FormatRate(netTxRate));
            StatsDataUpdated?.Invoke();
        }

        private static double ComputeRate(long current, long previous, double seconds)
            => seconds > 0 && current >= previous ? (current - previous) / seconds : 0;

        private static string FormatRate(double bytesPerSecond)
        {
            const double unit = 1024;
            if (bytesPerSecond >= unit * unit) return $"{bytesPerSecond / (unit * unit):F2} MB/s";
            if (bytesPerSecond >= unit) return $"{bytesPerSecond / unit:F1} KB/s";
            return $"{bytesPerSecond:F0} B/s";
        }
    }
}
