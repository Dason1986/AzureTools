using Avalonia.Threading;
using AzureTools.Cli;
using AzureTools.Properties;
using AzureTools.Services;
using AzureTools.Tools.IO;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.Module.Comm.ViewModels.FileBrowser
{
    public partial class FileBrowserViewModel : ViewModel, IAsyncDisposable
    {
        private readonly IContainerFileProvider _provider;
        private readonly IDialogService _dialogService;
        private bool _disposed;

        public FileBrowserViewModel(FileBrowserOption option, IDialogService dialogService)
        {
            _provider = option.FileProvider;
            _dialogService = dialogService;
            WindowTitle = option.Title;
            CurrentItems = new ObservableCollection<ContainerItemInfo>();
            Tabs = new ObservableCollection<FileTabItem>();
            _ = InitializeAsync();
        }

        public ObservableCollection<ContainerItemInfo> CurrentItems { get; }

        public ObservableCollection<FileTabItem> Tabs { get; }

        [ObservableProperty]
        private string _currentPath;

        [ObservableProperty]
        private ContainerItemInfo _selectedItem;

        [ObservableProperty]
        private FileTabItem _selectedTab;

        [ObservableProperty]
        private bool _isBusy;

        private async Task InitializeAsync()
        {
            try
            {
                string pwd = await _provider.GetWorkingDirectoryAsync();
                await LoadDirectoryAsync(pwd);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand]
        private Task RefreshAsync()
        {
            return LoadDirectoryAsync(CurrentPath);
        }

        [RelayCommand]
        private async Task EnterItemAsync(ContainerItemInfo item)
        {
            if (item == null) return;
            if (item.IsDirectory)
                await LoadDirectoryAsync(item.FullPath);
            else
                await OpenFileAsync(item);
        }

        [RelayCommand]
        private async Task NewFolderAsync()
        {
            string name = await _dialogService.ShowPromptDialogAsync(FormResources.TitleNewFolder, FormResources.LblFolderName);
            if (string.IsNullOrWhiteSpace(name)) return;
            string path = CurrentPath + "/" + name;
            IsBusy = true;
            try
            {
                await _provider.CreateDirectoryAsync(path);
                await RefreshAsync();
            }
            catch (Exception ex) { await _dialogService.ShowErrorAsync(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (SelectedItem == null || IsSpecialEntry(SelectedItem.Name)) return;
            if (!await _dialogService.ShowConfirmAsync(string.Format(FormResources.MsgConfirmDelete, SelectedItem.Name))) return;
            IsBusy = true;
            try
            {
                await _provider.DeleteAsync(SelectedItem.FullPath);
                await RefreshAsync();
            }
            catch (Exception ex) { await _dialogService.ShowErrorAsync(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task UploadAsync()
        {
            IFileEntry entry = await _dialogService.OpenFileAsync("�����ļ�|*.*", null);
            if (entry == null) return;
            string remote = CurrentPath + "/" + entry.Name;
            IsBusy = true;
            try
            {
                await _provider.UploadFileAsync(entry, remote);
                await RefreshAsync();
            }
            catch (Exception ex) { await _dialogService.ShowErrorAsync(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task DownloadAsync()
        {
            if (SelectedItem == null || SelectedItem.IsDirectory || IsSpecialEntry(SelectedItem.Name)) return;
            IFileEntry entry = await _dialogService.SaveFileAsync("*.*", SelectedItem.Name);
            if (entry == null) return;
            IsBusy = true;
            try { await _provider.DownloadFileAsync(SelectedItem.FullPath, entry); }
            catch (Exception ex) { await _dialogService.ShowErrorAsync(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private void CloseTab(FileTabItem tab)
        {
            if (tab != null) Tabs.Remove(tab);
        }

        private async Task LoadDirectoryAsync(string path)
        {
            IsBusy = true;
            try
            {
                string normalized = NormalizePath(path);
                IReadOnlyList<ContainerItemInfo> items = await _provider.GetDirectoryContentsAsync(normalized);
                string parent = GetParentPath(normalized);
                List<ContainerItemInfo> list = items
                    .Where(f => f.Name != "." && f.Name != "..")
                    .OrderByDescending(f => f.IsDirectory)
                    .ThenBy(f => f.Name)
                    .ToList();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    CurrentPath = normalized;
                    CurrentItems.Clear();
                    CurrentItems.Add(new ContainerItemInfo("..", parent, 0, DateTimeOffset.MinValue, true));
                    CurrentItems.Add(new ContainerItemInfo(".", normalized, 0, DateTimeOffset.MinValue, true));
                    foreach (ContainerItemInfo item in list) CurrentItems.Add(item);
                });
            }
            catch (Exception ex) { await _dialogService.ShowErrorAsync(ex.Message); }
            finally { IsBusy = false; }
        }

        private async Task OpenFileAsync(ContainerItemInfo item)
        {
            FileTabItem existing = Tabs.FirstOrDefault(t => t.Path == item.FullPath);
            if (existing != null) { SelectedTab = existing; return; }
            IsBusy = true;
            try
            {
                string content = await _provider.ReadTextFileAsync(item.FullPath);
                FileTabItem tab = new() { Title = item.Name, Path = item.FullPath, Content = content };
                Tabs.Add(tab);
                SelectedTab = tab;
            }
            catch (Exception ex) { await _dialogService.ShowErrorAsync(ex.Message); }
            finally { IsBusy = false; }
        }

        private static bool IsSpecialEntry(string name) => name == "." || name == "..";

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == ".") return "/";
            path = path.Replace("\\", "/");
            if (!path.StartsWith("/")) path = "/" + path;
            if (path.Length > 1) path = path.TrimEnd('/');
            return path.Length == 0 ? "/" : path;
        }

        private static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/") return "/";
            string trimmed = path.TrimEnd('/');
            int idx = trimmed.LastIndexOf('/');
            return idx <= 0 ? "/" : trimmed.Substring(0, idx);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            if (_provider != null) await _provider.DisposeAsync();
        }
    }
}
