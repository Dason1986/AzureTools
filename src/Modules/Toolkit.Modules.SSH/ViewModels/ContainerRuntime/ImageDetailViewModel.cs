using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class ImageDetailViewModel : ViewModel, IDisposable
    {
        private readonly IContainerRuntimeManager _docker;
        private readonly IDialogService _dialog;
        private readonly string _engine;
        private bool _disposed;

        public ImageDetailViewModel(SshHostProfile host, ImageDetailOption option, IContainerRuntimeManagerFactory dockerFactory, IDialogService dialog)
        {
            _dialog = dialog;
            _engine = option.Engine;
            _docker = dockerFactory.Create(host, option.Engine);
            WindowTitle = string.Format(SSHResource.TitleImageDetail, option.RepoTag);
            ImageId = option.ImageId;
            _ = RefreshAsync();
        }

        public string ImageId { get; }

        public ObservableCollection<DetailField> BasicInfoFields { get; } = new();
        public ObservableCollection<DetailField> ConfigFields { get; } = new();
        public ObservableCollection<DetailField> RepoFields { get; } = new();
        public ObservableCollection<DetailField> EnvFields { get; } = new();
        public ObservableCollection<DetailField> LabelFields { get; } = new();
        public ObservableCollection<DetailField> ExposedPortFields { get; } = new();

        [ObservableProperty]
        private string _rawJson;

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                string json = await _docker.GetImageDetailAsync(ImageId);
                RawJson = PrettyJson(json);
                ParseDetail(json);
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(SshContainerEngine.GetLoadFailedMessage(_engine, ex.Message));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            (_docker as IDisposable)?.Dispose();
        }

        private void ParseDetail(string raw)
        {
            BasicInfoFields.Clear();
            ConfigFields.Clear();
            RepoFields.Clear();
            EnvFields.Clear();
            ExposedPortFields.Clear();
            LabelFields.Clear();

            using JsonDocument doc = JsonDocument.Parse(raw);
            JsonElement root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                root = root[0];

            AddField(BasicInfoFields, "ID", GetString(root, "Id"));
            AddField(BasicInfoFields, "父镜像", GetString(root, "Parent"));
            AddField(BasicInfoFields, "创建时间", GetString(root, "Created"));
            AddField(BasicInfoFields, "大小", FormatSize(GetLong(root, "Size")));
            AddField(BasicInfoFields, "虚拟大小", FormatSize(GetLong(root, "VirtualSize")));
            AddField(BasicInfoFields, "架构", GetString(root, "Architecture"));
            AddField(BasicInfoFields, "操作系统", GetString(root, "Os"));

            if (TryGet(root, out JsonElement repoTags, "RepoTags"))
                foreach (JsonElement t in repoTags.EnumerateArray()) AddField(RepoFields, "标签", t.GetString());
            if (TryGet(root, out JsonElement repoDigests, "RepoDigests"))
                foreach (JsonElement d in repoDigests.EnumerateArray()) AddField(RepoFields, "摘要", d.GetString());

            if (TryGet(root, out JsonElement cfg, "Config"))
            {
                AddField(ConfigFields, "基础镜像", GetString(cfg, "Image"));
                AddField(ConfigFields, "WorkingDir", GetString(cfg, "WorkingDir"));
                AddField(ConfigFields, "Cmd", JoinArray(GetArray(cfg, "Cmd")));
                AddField(ConfigFields, "Entrypoint", JoinArray(GetArray(cfg, "Entrypoint")));

                if (TryGet(cfg, out JsonElement envArr, "Env"))
                {
                    foreach (JsonElement item in envArr.EnumerateArray())
                    {
                        string kv = item.GetString();
                        if (string.IsNullOrEmpty(kv)) continue;
                        int idx = kv.IndexOf('=');
                        if (idx < 0) { AddField(EnvFields, kv, string.Empty); continue; }
                        AddField(EnvFields, kv.Substring(0, idx), kv.Substring(idx + 1));
                    }
                }

                if (TryGet(cfg, out JsonElement labels, "Labels"))
                {
                    foreach (JsonProperty prop in labels.EnumerateObject())
                        AddField(LabelFields, prop.Name, prop.Value.GetString());
                }

                if (TryGet(cfg, out JsonElement exposed, "ExposedPorts"))
                {
                    foreach (JsonProperty prop in exposed.EnumerateObject())
                        AddField(ExposedPortFields, prop.Name, "");
                }
            }
        }

        private static string JoinArray(JsonElement? arr)
        {
            if (!arr.HasValue) return string.Empty;
            return string.Join(", ", arr.Value.EnumerateArray().Select(e => e.GetString()));
        }

        private static JsonElement? GetArray(JsonElement parent, string key)
            => parent.TryGetProperty(key, out JsonElement el) ? el : null;

        private static long GetLong(JsonElement parent, params string[] path)
        {
            if (!TryGet(parent, out JsonElement el, path)) return 0;
            return el.ValueKind == JsonValueKind.Number ? el.GetInt64() : 0;
        }

        private static string FormatSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            int i = 0;
            while (size >= 1024 && i < units.Length - 1) { size /= 1024; i++; }
            return $"{size:0.##} {units[i]}";
        }

        private static void AddField(ObservableCollection<DetailField> list, string name, string value)
            => list.Add(new DetailField { Name = name, Value = value ?? string.Empty });

        private static bool TryGet(JsonElement parent, out JsonElement result, params string[] path)
        {
            result = parent;
            foreach (string key in path)
            {
                if (result.ValueKind != JsonValueKind.Object || !result.TryGetProperty(key, out result)) return false;
            }
            return true;
        }

        private static string GetString(JsonElement parent, params string[] path)
        {
            if (!TryGet(parent, out JsonElement el, path)) return string.Empty;
            switch (el.ValueKind)
            {
                case JsonValueKind.String: return el.GetString() ?? string.Empty;
                case JsonValueKind.Number: return el.GetRawText();
                case JsonValueKind.True: return "true";
                case JsonValueKind.False: return "false";
                case JsonValueKind.Null: return string.Empty;
                default: return el.GetRawText();
            }
        }

        private static string PrettyJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            try
            {
                using JsonDocument doc = JsonDocument.Parse(raw);
                using MemoryStream ms = new MemoryStream();
                using (Utf8JsonWriter writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
                {
                    doc.RootElement.WriteTo(writer);
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch { return raw; }
        }
    }
}
