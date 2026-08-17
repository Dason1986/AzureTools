using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AzureTools.Properties;
using AzureTools.Services.Platform;
using AzureTools.Tools.IO;
using AzureTools.Views;
using AzureTools.Views.Comms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.Services
{
    public class DefaultDialogService : IDialogService
    {
        private readonly ILogger logger;
        private readonly IMessageFallback messageFallback;

        public DefaultDialogService(ILogger<DefaultDialogService> logger, IMessageFallback messageFallback)
        {
            this.logger = logger;
            this.messageFallback = messageFallback;
        }



        public async Task ShowInfoAsync(string message, string title = null)
        {

            await ShowDialogInternal(message, title ?? DialogResources.TitleInfo, "Information");

        }



        public async Task ShowErrorAsync(string message, string title = null)
        {
            await ShowDialogInternal(message, title ?? DialogResources.TitleError, "Error");

        }

        public async Task ShowWarningAsync(string message, string title = null)
        {
            await ShowDialogInternal(message, title ?? DialogResources.TitleWarning, "Warning");

        }


        public Task<bool> ShowConfirmAsync(string message, string title = null)
        {
            return ShowConfirmDialogAsync(message, title ?? DialogResources.TitleConfirm);
        }

        public Task<string> ShowPromptDialogAsync(string title, string prompt, PromptDialogOption dialogOption = null)
        {
            return Dispatcher.UIThread.InvokeAsync(async () =>
            {
                Window mainWindow = GetMainWindow();
                if (mainWindow == null) return null;
                PromptDialog dialog = new PromptDialog(title, prompt, dialogOption ?? new PromptDialogOption());
                dialog.Topmost = true;
                return await dialog.ShowDialog<string>(mainWindow);
            });
        }



        public Task<IFileEntry> SaveFileAsync(string filter, string defaultFileName)
        {
            return SaveFileDialogAsync(filter, defaultFileName);
        }






        public Task<IList<T>> SelectItemsAsync<T>(IList<T> dataSource, bool isMultiSelect, string title, IDictionary<string, string> propertyMappings = null)
        {
            return SelectItemsDialogAsync(dataSource, isMultiSelect, title, propertyMappings);
        }


        public Task<IFileEntry> OpenFileAsync(string filter, string defaultFileName)
        {
            return OpenFileDialogAsync(filter, defaultFileName);
        }



        public Task OpenWithExplorerAsync(string path)
        {
            return Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            });
        }



        public async Task<bool> SetClipboardAsync(string text)
        {
            try
            {
                if (Dispatcher.UIThread.Thread.ThreadState != System.Threading.ThreadState.Running)
                {
                    return messageFallback.IsSupported && messageFallback.SetClipboard(text);
                }
                return await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    IClipboard clipboard = GetMainWindow()?.Clipboard;
                    if (clipboard == null) return false;
                    await clipboard.SetTextAsync(text);
                    return true;
                });
            }
            catch (Exception)
            {
                logger.LogWarning(DialogResources.ClipboardCopyFailed);
                return false;
            }
        }

        public Task OpenWebViewAsync(string url, string title = null)
        {
            return Dispatcher.UIThread.InvokeAsync(() =>
            {
                Window mainWindow = GetMainWindow();
                if (mainWindow == null) return Task.CompletedTask;
                WebViewWindow webView = new WebViewWindow(url, title);
                return webView.ShowDialog(mainWindow);
            });
        }

        private Task ShowDialogInternal(string message, string title, string iconType)
        {
            if (Dispatcher.UIThread.Thread.ThreadState != System.Threading.ThreadState.Running)
            {
                if (messageFallback.IsSupported)
                {
                    messageFallback.ShowMessage(message, title, iconType);
                }
                return Task.CompletedTask;
            }
            return Dispatcher.UIThread.InvokeAsync(async () =>
            {
                MessageBoxDialog dialog = new MessageBoxDialog(message, title, iconType);
                Window mainWindow = GetMainWindow();

                dialog.Topmost = true;
                await dialog.ShowDialog(mainWindow);
            });
        }

        private Task<bool> ShowConfirmDialogAsync(string message, string title)
        {
            return Dispatcher.UIThread.InvokeAsync(async () =>
            {
                ConfirmDialog dialog = new ConfirmDialog(message, title);
                Window mainWindow = GetMainWindow();
                if (mainWindow == null) return false;
                dialog.Topmost = true;
                bool? result = await dialog.ShowDialog<bool?>(mainWindow);
                return result == true;
            });
        }

        private Task<IFileEntry> SaveFileDialogAsync(string filter, string defaultFileName)
        {
            return Dispatcher.UIThread.InvokeAsync<IFileEntry>(async () =>
            {
                Window mainWindow = GetMainWindow();
                if (mainWindow == null) return null;

                IStorageProvider storageProvider = mainWindow.StorageProvider;
                (string, string) fileType = ParseFilter(filter);
                IStorageFile file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save File",
                    SuggestedFileName = defaultFileName,
                    FileTypeChoices = new[] { new FilePickerFileType(fileType.Item1) { Patterns = new[] { fileType.Item2 } } }
                });

                if (file == null) return null;
                return new AvaloniaFileEntry(file);
            });
        }

        private Task<IFileEntry> OpenFileDialogAsync(string filter, string defaultFileName)
        {
            return Dispatcher.UIThread.InvokeAsync<IFileEntry>(async () =>
            {
                Window mainWindow = GetMainWindow();
                if (mainWindow == null) return null;

                IStorageProvider storageProvider = mainWindow.StorageProvider;
                (string, string) fileType = ParseFilter(filter);
                IReadOnlyList<IStorageFile> files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open File",
                    AllowMultiple = false,
                    SuggestedFileName = defaultFileName,
                    FileTypeFilter = new[] { new FilePickerFileType(fileType.Item1) { Patterns = new[] { fileType.Item2 } } }
                });

                IStorageFile file = files.FirstOrDefault();
                if (file == null) return null;

                return new AvaloniaFileEntry(file);
            });
        }
        public Task<IFolderEntry> SelectFolderAsync()
        {
            return Dispatcher.UIThread.InvokeAsync<IFolderEntry>(async () =>
            {
                Window mainWindow = GetMainWindow();
                if (mainWindow == null) return null;

                IStorageProvider storageProvider = mainWindow.StorageProvider;
                IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Folder",
                    AllowMultiple = false
                });
                if (folders.Count == 0) return null;
                IStorageFolder folder = folders.FirstOrDefault();
                return new AvaloniaFolderEntry(folder);
            });
        }

        private Task<IList<T>> SelectItemsDialogAsync<T>(IList<T> dataSource, bool isMultiSelect, string title, IDictionary<string, string> propertyMappings)
        {
            return Dispatcher.UIThread.InvokeAsync(async () =>
            {
                Window mainWindow = GetMainWindow();
                if (mainWindow == null) return null;

                Type itemType = typeof(T);

                if (isMultiSelect)
                {
                    List<T> list = new List<T>(dataSource);
                    MultiSelectItemsView dialog = new Views.Comms.MultiSelectItemsView(itemType, list, title, propertyMappings);
                    bool? dialogResult = await dialog.ShowDialog<bool?>(mainWindow);
                    if (dialogResult == true)
                    {
                        return dialog.GetSelectedItems<T>();
                    }
                    return new List<T>();
                }
                else
                {
                    List<T> list = new List<T>(dataSource);
                    SelectItemsView dialog = new Views.Comms.SelectItemsView(itemType, list, title, propertyMappings);
                    bool? dialogResult = await dialog.ShowDialog<bool?>(mainWindow);
                    if (dialogResult == true)
                    {
                        T selectedItem = dialog.GetSelectedItem<T>();
                        return selectedItem != null ? new List<T> { selectedItem } : new List<T>();
                    }
                    return new List<T>();
                }
            });
        }

        private Window GetMainWindow()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Window current = desktop.Windows.FirstOrDefault(x => x.IsActive);
                return current ?? desktop.MainWindow;
            }
            return null;
        }

        private static (string, string) ParseFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return ("All Files", ".*");

            string[] parts = filter.Split('|');
            if (parts.Length >= 2)
            {
                return (parts[0], parts[1]);
            }
            return ("All Files", ".*");
        }
    }
}