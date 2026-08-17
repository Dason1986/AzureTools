using AzureTools.Services;
using AzureTools.Tools.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureTools.Tests.Stubs
{
    public class StubMessageCenter : IDialogService
    {
        public string LastInfoMessage { get; private set; }
        public string LastErrorMessage { get; private set; }
        public string LastWarningMessage { get; private set; }
        public string LastOpenedPath { get; private set; }
        public bool SaveFileCalled { get; private set; }
        public string SaveFileFilter { get; private set; }
        public string SaveFileDefaultName { get; private set; }
        public string LastClipboard { get; private set; }
        public bool ConfirmResult { get; set; } = true;
        public string InputResult { get; set; } = "input";
        public bool OpenFileCalled { get; private set; }
        public string OpenFileFilter { get; private set; }
        public string OpenFileDefaultName { get; private set; }
        public IList SelectedItems { get; set; }





        public Task<IFileEntry> SaveFileAsync(string filter, string defaultFileName)
        {
            SaveFileCalled = true;
            SaveFileFilter = filter;
            SaveFileDefaultName = defaultFileName;
            return Task.FromResult((IFileEntry)new StubFileEntry(defaultFileName, $"C:\\{defaultFileName}"));
        }



        public Task<IFileEntry> OpenFileAsync(string filter, string defaultFileName = null)
        {
            OpenFileCalled = true;
            OpenFileFilter = filter;
            OpenFileDefaultName = defaultFileName;
            string fileName = defaultFileName ?? "test.json";
            return Task.FromResult((IFileEntry)new StubFileEntry(fileName, $"C:\\{fileName}"));
        }


        public Task ShowInfoAsync(string message, string title = null)
        {
            LastInfoMessage = message;
            return Task.CompletedTask;
        }



        public Task ShowErrorAsync(string message, string title = null)
        {
            LastErrorMessage = message;
            return Task.CompletedTask;
        }


        public Task ShowWarningAsync(string message, string title = null)
        {
            LastWarningMessage = message;
            return Task.CompletedTask;
        }



        public Task<bool> ShowConfirmAsync(string message, string title = null)
        {
            return Task.FromResult(ConfirmResult);
        }



        public Task<string> ShowPromptDialogAsync(string title, string prompt, PromptDialogOption dialogOption)
        {
            return Task.FromResult("password");
        }

        public string SelectFolder() => "C:\\";

        public Task<string> SelectFolderAsync() => Task.FromResult("C:\\");

        public IList<T> SelectItems<T>(IList<T> dataSource, bool isMultiSelect, string title, IDictionary<string, string> propertyMappings = null)
        {
            if (SelectedItems is IList<T> typedList)
            {
                return typedList;
            }
            List<T> result = new List<T>();
            foreach (object item in SelectedItems)
            {
                if (item is T typedItem)
                {
                    result.Add(typedItem);
                }
            }
            return result;
        }

        public Task<IList<T>> SelectItemsAsync<T>(IList<T> dataSource, bool isMultiSelect, string title, IDictionary<string, string> propertyMappings = null)
        {
            return Task.FromResult(SelectItems(dataSource, isMultiSelect, title, propertyMappings));
        }

        public void OpenWithExplorer(string path)
        {
            LastOpenedPath = path;
        }

        public Task OpenWithExplorerAsync(string path)
        {
            LastOpenedPath = path;
            return Task.CompletedTask;
        }

        public bool SetClipboard(string text)
        {
            LastClipboard = text;
            return true;
        }

        public Task<bool> SetClipboardAsync(string text)
        {
            LastClipboard = text;
            return Task.FromResult(true);
        }

        public Task OpenWebViewAsync(string url, string title = null)
        {
            LastOpenedPath = url;
            return Task.CompletedTask;
        }

        Task<IFolderEntry> IDialogService.SelectFolderAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
