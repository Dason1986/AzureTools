using AzureTools.Tools.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureTools.Services
{
    public interface IDialogService
    {
        Task ShowInfoAsync(string message, string title = null);
        Task ShowErrorAsync(string message, string title = null);
        Task ShowWarningAsync(string message, string title = null);
        Task<bool> ShowConfirmAsync(string message, string title = null);
        Task<string> ShowPromptDialogAsync(string title, string prompt, PromptDialogOption dialogOption = null);
        Task<IFileEntry> SaveFileAsync(string filter, string defaultFileName);
        Task<IFileEntry> OpenFileAsync(string filter, string defaultFileName);
        Task<IFolderEntry> SelectFolderAsync();
        Task<IList<T>> SelectItemsAsync<T>(IList<T> dataSource, bool isMultiSelect, string title, IDictionary<string, string> propertyMappings = null);
        Task OpenWithExplorerAsync(string path);
        Task<bool> SetClipboardAsync(string text);
        Task OpenWebViewAsync(string url, string title = null);
    }
}
