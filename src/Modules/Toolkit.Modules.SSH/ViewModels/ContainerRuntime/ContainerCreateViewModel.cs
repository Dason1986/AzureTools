using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class ContainerCreateViewModel : ViewModel, IDisposable
    {
        private readonly IContainerRuntimeManager _docker;
        private readonly IPodManager _pods;
        private readonly IDialogService _dialog;
        private readonly string _engine;
        private readonly bool _collectOnly;
        private readonly Dictionary<string, string> _imageEnvDefaults = new();
        private bool _disposed;

        /// <summary>CollectOnly 模式下关闭对话框时返回的容器配置。</summary>
        public ContainerCreateConfig CollectedConfig { get; private set; }

        public ContainerCreateViewModel(SshHostProfile host, ContainerCreateOption option, IContainerRuntimeManagerFactory dockerFactory, IDialogService dialog)
        {
            _dialog = dialog;
            _engine = option.Engine;
            _collectOnly = option.CollectOnly;
            _docker = dockerFactory.Create(host, option.Engine);
            _pods = _docker as IPodManager;
            WindowTitle = SSHResource.TitleCreateContainer;
            IsPodman = SshContainerEngine.IsPodman(option.Engine);
            PodName = option.PodName;
            if (!string.IsNullOrEmpty(PodName))
            {
                WindowTitle = string.Format(SSHResource.TitleAddContainerToPod, PodName);
            }
            if (option.InitialConfig != null)
            {
                ApplyConfig(option.InitialConfig);
            }
            else if (!string.IsNullOrEmpty(option.SelectedImage))
            {
                ImageName = option.SelectedImage;
            }
            _ = LoadImagesAsync();
            _ = LoadNetworksAsync();
        }

        public ObservableCollection<string> LocalImages { get; } = new();
        public ObservableCollection<string> LocalNetworks { get; } = new();
        public ObservableCollection<EnvEntry> EnvEntries { get; } = new();
        public ObservableCollection<MountEntry> MountEntries { get; } = new();
        public ObservableCollection<PortEntry> PortEntries { get; } = new();
        public ObservableCollection<string> SuggestedEnvKeys { get; } = new();
        public ObservableCollection<string> SuggestedContainerPorts { get; } = new();
        public ObservableCollection<string> ImageLabels { get; } = new();

        [ObservableProperty]
        private string _imageName;

        [ObservableProperty]
        private string _podName;

        [ObservableProperty]
        private string _containerName;

        [ObservableProperty]
        private string _command;

        [ObservableProperty]
        private bool _autoStart;

        [ObservableProperty]
        private bool _autoRemove;

        [ObservableProperty]
        private bool _privileged;

        [ObservableProperty]
        private string _selectedImage;

        [ObservableProperty]
        private string _selectedNetwork;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isPodman;

        [ObservableProperty]
        private bool _hasImageLabels;

        partial void OnSelectedImageChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                ApplyImageInspect(null);
                return;
            }
            ImageName = value;
            if (LocalImages.Contains(value))
            {
                _ = LoadImageInspectAsync(value);
            }
            else
            {
                ApplyImageInspect(null);
            }
        }

        private async Task LoadImageInspectAsync(string image)
        {
            try
            {
                ImageInspect inspect = await _docker.InspectImageAsync(image);
                if (string.Equals(SelectedImage, image, StringComparison.Ordinal))
                {
                    ApplyImageInspect(inspect?.Config);
                }
            }
            catch
            {
                // 检查信息仅用于输入辅助，失败时不阻断创建流程
            }
        }

        private void ApplyImageInspect(ImageInspectConfig config)
        {
            _imageEnvDefaults.Clear();
            SuggestedEnvKeys.Clear();
            SuggestedContainerPorts.Clear();
            ImageLabels.Clear();
            HasImageLabels = false;
            if (config == null) return;

            if (config.Env != null)
            {
                foreach (string item in config.Env)
                {
                    int separator = item.IndexOf('=');
                    if (separator <= 0) continue;
                    string key = item.Substring(0, separator);
                    _imageEnvDefaults[key] = item.Substring(separator + 1);
                    SuggestedEnvKeys.Add(key);
                }
            }

            if (config.ExposedPorts != null)
            {
                foreach (string key in config.ExposedPorts.Keys)
                {
                    string port = key.Split('/')[0];
                    if (port.Length > 0 && !SuggestedContainerPorts.Contains(port))
                    {
                        SuggestedContainerPorts.Add(port);
                    }
                }
            }

            if (config.Labels != null)
            {
                foreach (KeyValuePair<string, string> pair in config.Labels)
                {
                    ImageLabels.Add($"{pair.Key}={pair.Value}");
                }
                HasImageLabels = ImageLabels.Count > 0;
            }
        }

        [RelayCommand]
        private async Task LoadImagesAsync()
        {
            IsBusy = true;
            try
            {
                IReadOnlyList<Image> images = await _docker.ListImagesAsync();
                LocalImages.Clear();
                foreach (Image img in images.OrderBy(i => i.RepoTag))
                {
                    LocalImages.Add(img.RepoTag);
                }
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(SshContainerEngine.GetLoadFailedMessage(_engine, ex.Message));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadNetworksAsync()
        {
            try
            {
                IReadOnlyList<Network> networks = await _docker.ListNetworksAsync();
                LocalNetworks.Clear();
                foreach (Network network in networks.OrderBy(n => n.Name))
                {
                    LocalNetworks.Add(network.Name);
                }
            }
            catch
            {
                // 网络列表仅用于选择辅助，失败时允许手动输入
            }
        }

        [RelayCommand]
        private void AddEnv()
        {
            EnvEntries.Add(new EnvEntry
            {
                DefaultValueResolver = ResolveEnvDefaultValue,
                ValueConflictResolver = ResolveEnvConflictAsync
            });
        }

        private string ResolveEnvDefaultValue(string key)
        {
            return _imageEnvDefaults.TryGetValue(key, out string value) ? value : null;
        }

        private async Task<bool> ResolveEnvConflictAsync(string key, string currentValue, string defaultValue)
        {
            string message = string.Format(SSHResource.MsgEnvValueConflict, key, currentValue, defaultValue);
            return await _dialog.ShowConfirmAsync(message, SSHResource.TitleCreateContainer);
        }

        [RelayCommand]
        private void RemoveEnv(EnvEntry entry)
        {
            if (entry != null) EnvEntries.Remove(entry);
        }

        [RelayCommand]
        private void AddMount()
        {
            MountEntries.Add(new MountEntry());
        }

        [RelayCommand]
        private void RemoveMount(MountEntry entry)
        {
            if (entry != null) MountEntries.Remove(entry);
        }

        [RelayCommand]
        private void AddPort()
        {
            PortEntries.Add(new PortEntry());
        }

        [RelayCommand]
        private void RemovePort(PortEntry entry)
        {
            if (entry != null) PortEntries.Remove(entry);
        }

        [RelayCommand]
        private async Task CreateAsync()
        {
            if (string.IsNullOrWhiteSpace(ImageName))
            {
                await _dialog.ShowErrorAsync(SSHResource.MsgImageRequired);
                return;
            }

            ContainerCreateConfig config = BuildConfig();
            if (_collectOnly)
            {
                CollectedConfig = config;
                await CloseAsync();
                return;
            }

            IsBusy = true;
            try
            {
                Container container = !string.IsNullOrEmpty(PodName) && _pods != null
                    ? await _pods.CreateContainerInPodAsync(PodName, config, config.Name)
                    : await _docker.CreateContainerAsync(config, config.Name);
                await _dialog.ShowInfoAsync(string.Format(SSHResource.MsgContainerCreated, container.Id));
                await CloseAsync();
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(SshContainerEngine.GetLoadFailedMessage(_engine, ex.Message));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyConfig(ContainerCreateConfig config)
        {
            ImageName = config.Image;
            ContainerName = config.Name;
            Command = config.Cmd == null ? null : string.Join(' ', config.Cmd);
            Privileged = config.Privileged;
            AutoRemove = config.AutoRemove;
            AutoStart = string.Equals(config.RestartPolicy, "always", StringComparison.Ordinal);
            SelectedNetwork = config.Networks?.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
            if (config.Env != null)
            {
                foreach (string item in config.Env)
                {
                    int separator = item.IndexOf('=');
                    if (separator <= 0) continue;
                    EnvEntries.Add(new EnvEntry { Key = item[..separator], Value = item[(separator + 1)..] });
                }
            }
            if (config.Binds != null)
            {
                foreach (string bind in config.Binds)
                {
                    string[] parts = bind.Split(':');
                    if (parts.Length < 2) continue;
                    MountEntries.Add(new MountEntry
                    {
                        HostPath = parts[0],
                        ContainerPath = parts[1],
                        IsReadOnly = parts.Length > 2 && parts[2].Split(',').Contains("ro")
                    });
                }
            }
            if (config.PortBindings != null)
            {
                foreach (string port in config.PortBindings)
                {
                    string[] parts = port.Split(':');
                    if (parts.Length < 2) continue;
                    PortEntries.Add(new PortEntry { HostPort = parts[0], ContainerPort = parts[1] });
                }
            }
        }

        private ContainerCreateConfig BuildConfig()
        {
            ContainerCreateConfig config = new ContainerCreateConfig
            {
                Image = ImageName.Trim(),
                Name = string.IsNullOrWhiteSpace(ContainerName) ? null : ContainerName.Trim(),
                Privileged = Privileged,
                AutoRemove = AutoRemove,
                RestartPolicy = GetRestartPolicy()
            };

            if (!string.IsNullOrWhiteSpace(Command))
            {
                config.Cmd = Command.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (!string.IsNullOrWhiteSpace(SelectedNetwork))
            {
                config.Networks = new List<string> { SelectedNetwork.Trim() };
            }

            if (EnvEntries.Count > 0)
            {
                config.Env = EnvEntries
                    .Where(e => !string.IsNullOrWhiteSpace(e.Key))
                    .Select(e => $"{e.Key.Trim()}={e.Value?.Trim() ?? string.Empty}")
                    .ToList();
            }

            if (MountEntries.Count > 0)
            {
                config.Binds = MountEntries
                    .Where(m => !string.IsNullOrWhiteSpace(m.HostPath) && !string.IsNullOrWhiteSpace(m.ContainerPath))
                    .Select(m => m.IsReadOnly
                        ? $"{m.HostPath.Trim()}:{m.ContainerPath.Trim()}:ro"
                        : $"{m.HostPath.Trim()}:{m.ContainerPath.Trim()}")
                    .ToList();
            }

            if (PortEntries.Count > 0)
            {
                config.PortBindings = PortEntries
                    .Where(p => !string.IsNullOrWhiteSpace(p.HostPort) && !string.IsNullOrWhiteSpace(p.ContainerPort))
                    .Select(p => $"{p.HostPort.Trim()}:{p.ContainerPort.Trim()}")
                    .ToList();
            }

            return config;
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await CloseAsync();
        }

        private string GetRestartPolicy()
        {
            if (AutoRemove) return "no";
            if (AutoStart)
            {
                return IsPodman ? "always" : "always";
            }
            return "no";
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            (_docker as IDisposable)?.Dispose();
        }
    }
}
