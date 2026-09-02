using Avalonia.Threading;
using AzureTools.Cli;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace AzureTools.Module.Comm.ViewModels.CliStreamSession
{
    public partial class CliStreamSessionViewModel : ViewModel, IDisposable
    {
        private readonly ICliStreamSession _session;
        private bool _disposed;

        public CliStreamSessionViewModel(CliStreamSessionOption option)
        {
            _session = option.Session;
            WindowTitle = option.Title;
            IsInteractive = option.IsInteractive;
            _session.OutputReceived += OnOutput;
            _session.ErrorReceived += OnOutput;
        }

        public bool IsInteractive { get; }

        public ObservableCollection<string> OutputLines { get; } = new();

        [ObservableProperty]
        private string _inputText = string.Empty;

        [ObservableProperty]
        private bool _isRunning = true;

        private void OnOutput(object sender, CliStreamOutputEventArgs e) => AppendLine(e.Line);

        private void AppendLine(string line)
        {
            if (_disposed) return;
            Dispatcher.UIThread.Post(() =>
            {
                if (_disposed) return;
                OutputLines.Add(line);
                if (OutputLines.Count > 5000)
                    OutputLines.RemoveAt(0);
            });
        }

        [RelayCommand]
        private void SendInput()
        {
            if (string.IsNullOrEmpty(InputText) || !_session.IsRunning) return;
            _ = _session.WriteLineAsync(InputText);
            InputText = string.Empty;
        }

        [RelayCommand]
        private void Clear() => OutputLines.Clear();

        [RelayCommand]
        private void Stop()
        {
            _session.Stop();
            IsRunning = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _session.OutputReceived -= OnOutput;
            _session.ErrorReceived -= OnOutput;
            _session.Stop();
        }
    }
}
