using AzureTools.Extensions;
using AzureTools.Models;
using AzureTools.Models.KeyVaults;
using AzureTools.Properties;
using AzureTools.SDK;
using AzureTools.Services;
using AzureTools.Services.KeyVaults;
using AzureTools.Stores;
using AzureTools.Tools.IO;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureTools.Module.Azure.ViewModels.KeyVaults
{
    public partial class SecretKeyManageViewModel : ManageViewModel<SecretKeyOptionItem>
    {
        #region Constructors

        public SecretKeyManageViewModel(ProjectInfo projectInfo, IKeyVaultStore keyVaultStore, IDialogService dialogService, IOptions<JsonSerializerOptions> jsonOption) : base(dialogService)
        {
            this.projectInfo = projectInfo;
            _keyVaultStore = keyVaultStore;
            this.jsonOptions = jsonOption.Value;
            CanClose = HandleCloseAsync;
            LoadItems();
        }

        #endregion

        #region Fields & Properties

        private readonly ProjectInfo projectInfo;
        private readonly IKeyVaultStore _keyVaultStore;
        private readonly JsonSerializerOptions jsonOptions;


        [ObservableProperty]
        private string _filterText;



        #endregion

        private void LoadItems()
        {
            Items = new ObservableCollection<SecretKeyOptionItem>(_keyVaultStore.GetRequiredKeys(projectInfo.Id));
            HasChanges = false;
        }

        protected override Task RefreshAsync()
        {
            LoadItems();
            return Task.CompletedTask;
        }

        protected override async Task AddAsync()
        {
            SecretKeyOptionItem item = await EditOptionAsync(null);
            if (item == null) return;

            if (string.IsNullOrWhiteSpace(item.Key))
            {
                await DialogService.ShowInfoAsync(MessageResources.KeyCannotBeEmpty);
                return;
            }

            if (Items.Any(x => x.Key.Equals(item.Key, StringComparison.OrdinalIgnoreCase)))
            {
                await DialogService.ShowInfoAsync(string.Format(MessageResources.KeyAlreadyExists, item.Key));
                return;
            }
            item.ProjectId = projectInfo.Id;

            _keyVaultStore.AddKey(item);
            OnFilterTextChanged(FilterText);
            HasChanges = true;
        }


        protected override async Task EditAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(MessageResources.SelectItemToEdit);
                return;
            }

            SecretKeyOptionItem item = await EditOptionAsync(SelectedItem);
            if (item == null) return;

            _keyVaultStore.UpdateKey(item);
            OnFilterTextChanged(FilterText);
            HasChanges = true;
        }

        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null)
            {
                await DialogService.ShowInfoAsync(MessageResources.SelectItemToDelete);
                return;
            }

            bool confirm = await DialogService.ShowConfirmAsync(string.Format(MessageResources.ConfirmDeleteKey, SelectedItem.Key));
            if (!confirm) return;

            _keyVaultStore.RemoveKey(SelectedItem);
            OnFilterTextChanged(FilterText);
            HasChanges = true;
        }

        [RelayCommand]
        private async Task ImportAsync()
        {
            IFileEntry fileEntry = await DialogService.OpenFileAsync("JSON 文件|*.json", "EnvKey.json");
            if (fileEntry == null) return;

            try
            {

                string content = fileEntry.GetAllText();
                if (string.IsNullOrEmpty(content)) throw new AzureToolsException("导入文件不是有效的json");
                using JsonDocument doc = JsonDocument.Parse(content);
                List<SecretKeyOptionItem> items = doc.RootElement.EnumerateArray()
                    .Select(x => CreateSecretKeyOptionItem(projectInfo.Id, x))
                    .ToList();
                _keyVaultStore.Import(items);
                await DialogService.ShowInfoAsync(MessageResources.ImportSuccess);
                LoadItems();
                HasChanges = true;
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(ex.Message, MessageResources.ImportFailed);
            }
        }
        protected virtual SecretKeyOptionItem CreateSecretKeyOptionItem(Guid projectId, JsonElement element)
        {
            string name = element.GetString("Name");
            string description = element.GetString("Description");
            string[] tags = element.GetStrings("Tags");
            bool required = element.GetBoolean("Required");
            string type = element.GetString("Type");
            ValidationOptions sdkRule = element.TryGetProperty("Option", out JsonElement optionElement)
                ? KeyValidatorProvider.Default.CreateOptions(type, optionElement)
                : KeyValidatorProvider.Default.CreateOptions(type, new JsonElement());
            return new SecretKeyOptionItem
            {
                Key = name,
                ProjectId = projectId,
                Required = required,
                Type = type,
                Option = sdkRule,
                Description = description,
                Tags = tags,
            };
        }
        [RelayCommand]
        [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
        private async Task ExportAsync()
        {
            if (Items == null || Items.Count == 0)
            {
                await DialogService.ShowInfoAsync(MessageResources.NoDataToExport);
                return;
            }

            IFileEntry fileEntry = await DialogService.SaveFileAsync("JSON 文件|*.json", $"{projectInfo.Name}_Keys.json");
            if (fileEntry == null) return;

            try
            {
                List<SecretKeyOptionItem> exportList = Items.ToList();
                string json = JsonSerializer.Serialize(exportList, jsonOptions);
                fileEntry.WriteAllText(json);
                await DialogService.ShowInfoAsync(MessageResources.ExportSuccess);
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(ex.Message, MessageResources.ExportFailed);
            }
        }


        protected override async Task SaveAsync()
        {
            _keyVaultStore.Save();
            await DialogService.ShowInfoAsync(MessageResources.SaveSuccess);
            HasChanges = false;
        }

        protected override async Task DiscardChangesAsync()
        {
            _keyVaultStore.DiscardChanges();
            LoadItems();
        }

        protected override async Task<SecretKeyOptionItem> EditOptionAsync(SecretKeyOptionItem item)
        {
            SecretKeyEditorViewModel editViewModel = await ShowDialog<SecretKeyEditorViewModel>(new SecretKeyEditorOption { Option = item, IsEditMode = item != null });
            return editViewModel.DialogResult == true ? editViewModel.GetItem() : null;
        }

        protected override async Task<bool> HandleCloseAsync()
        {
            if (!HasChanges) return true;
            bool save = await DialogService.ShowConfirmAsync(MessageResources.ConfirmSaveUnsavedChanges, MessageResources.TitleUnsavedChanges);
            if (save)
            {
                _keyVaultStore.Save();
            }
            else
            {
                _keyVaultStore.DiscardChanges();
            }
            return true;
        }

        partial void OnFilterTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                LoadItems();
                return;
            }

            var filtered = _keyVaultStore.GetRequiredKeys(projectInfo.Id)
                .Where(x => x.Key.Contains(value, StringComparison.OrdinalIgnoreCase)
                    || (x.Description?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            Items = new ObservableCollection<SecretKeyOptionItem>(filtered);
        }
    }
}
