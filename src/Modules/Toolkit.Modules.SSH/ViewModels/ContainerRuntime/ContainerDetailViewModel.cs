using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class ContainerDetailViewModel : ViewModel, IDisposable
    {
        private readonly IContainerRuntimeManager _docker;
        private readonly IDialogService _dialog;
        private readonly string _engine;
        private bool _disposed;

        public ContainerDetailViewModel(SshHostProfile host, ContainerDetailOption option, IContainerRuntimeManagerFactory dockerFactory, IDialogService dialog)
        {
            _dialog = dialog;
            _engine = option.Engine;
            _docker = dockerFactory.Create(host, option.Engine);
            WindowTitle = string.Format(SSHResource.TitleContainerDetail, option.ContainerName);
            ContainerId = option.ContainerId;
            _ = RefreshAsync();
        }

        public string ContainerId { get; }

        public ObservableCollection<DetailField> BasicInfoFields { get; } = new();
        public ObservableCollection<DetailField> EnvFields { get; } = new();
        public ObservableCollection<DetailField> PortFields { get; } = new();
        public ObservableCollection<DetailField> MountFields { get; } = new();
        public ObservableCollection<DetailField> NetworkFields { get; } = new();

        [ObservableProperty]
        private string _rawJson;

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                string json = await _docker.GetContainerDetailAsync(ContainerId);
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
            EnvFields.Clear();
            PortFields.Clear();
            MountFields.Clear();
            NetworkFields.Clear();

            using JsonDocument doc = JsonDocument.Parse(raw);
            JsonElement root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                root = root[0];

            AddField(BasicInfoFields, "名称", GetString(root, "Name"));
            AddField(BasicInfoFields, "ID", GetString(root, "Id"));
            AddField(BasicInfoFields, "镜像", GetString(root, "Config", "Image"));
            AddField(BasicInfoFields, "创建时间", GetString(root, "Created"));
            AddField(BasicInfoFields, "状态", GetString(root, "State", "Status"));
            AddField(BasicInfoFields, "运行中", GetString(root, "State", "Running"));
            AddField(BasicInfoFields, "退出码", GetString(root, "State", "ExitCode"));
            AddField(BasicInfoFields, "PID", GetString(root, "State", "Pid"));

            if (TryGet(root, out JsonElement envArr, "Config", "Env"))
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

            if (TryGet(root, out JsonElement ports, "NetworkSettings", "Ports"))
            {
                foreach (JsonProperty prop in ports.EnumerateObject())
                {
                    string mapping = string.Empty;
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (JsonElement binding in prop.Value.EnumerateArray())
                        {
                            if (sb.Length > 0) sb.Append("; ");
                            sb.Append(GetString(binding, "HostIp")).Append(':').Append(GetString(binding, "HostPort"));
                        }
                        mapping = sb.ToString();
                    }
                    AddField(PortFields, prop.Name, mapping);
                }
            }

            if (TryGet(root, out JsonElement mounts, "Mounts"))
            {
                int idx = 0;
                foreach (JsonElement m in mounts.EnumerateArray())
                {
                    idx++;
                    string type = GetString(m, "Type");
                    string src = GetString(m, "Source");
                    string dst = GetString(m, "Destination");
                    string rw = GetString(m, "RW");
                    AddField(MountFields, $"[{idx}] {type}", $"{src} → {dst} (RW={rw})");
                }
            }

            if (TryGet(root, out JsonElement networks, "NetworkSettings", "Networks"))
            {
                foreach (JsonProperty prop in networks.EnumerateObject())
                {
                    string ip = GetString(prop.Value, "IPAddress");
                    string gw = GetString(prop.Value, "Gateway");
                    AddField(NetworkFields, prop.Name, $"IP={ip}, Gateway={gw}");
                }
            }
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
