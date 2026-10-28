# SSH Docker HTTP 工厂 + Feature Tab 插件机制

## Context

当前 SSH 模块的 Docker 浏览器硬编码使用 `SshDockerCliApi`（通过 SSH 执行 `docker` 命令），无法切换为基于 HTTP 的方案。原设计文档 `ssh-terminal-docker-tunnel.md` 本就规划了 HTTP 方案（SSH 隧道 + HttpClient 调用 Docker daemon REST API），但实际落地只实现了 CLI 版。

同时 SSH 主机编辑界面把"功能模块"塞在一个 GroupBox 内（CheckBox 列表 + Docker 端口），无法支撑未来每个功能模块（Docker/Terminal/FileBrowser）独立扩展自己的配置 UI。

本次改造：
1. 补齐 `IDockerApi` 的 HTTP 实现，引入工厂按主机配置选择 Cli/Http。
2. 重构 `SshHostEditorView` 为顶层 TabControl，每个功能模块独立一个 Tab，引入 `SshFeatureTabViewModel` 抽象实现 VS Options 风格的可扩展插件机制。

## 关键决策

- HTTP 方案通过 SSH 隧道转发：复用 `ISshTunnelManager.OpenAsync(host, "127.0.0.1", host.DockerPort)` 获得本地端口，`HttpClient.BaseAddress = http://127.0.0.1:{localPort}/`。
- `SshHostProfile` 新增 `DockerApiMode` 字段（默认 `"Cli"`），随档案持久化。旧档案默认值兜底。
- 工厂 `IDockerApiFactory.Create(host)` 按模式返回 `SshDockerCliApi` 或 `SshDockerHttpApi`，调用方负责 Dispose。
- Tab 插件用抽象类 `SshFeatureTabViewModel`（继承 `ObservableObject`，便于 AOT 源生成 `[ObservableProperty]`），子类通过 DI 注册，`SshHostEditorViewModel` 收集 `IEnumerable<SshFeatureTabViewModel>`。
- 顶层 TabControl：基本 + Docker + 终端 + 文件浏览；"基本" Tab 含原 ScrollViewer 所有非 Features 内容。
- 每个 Tab 一个独立 UserControl View，通过 `ContentControl.DataTemplates` 按运行时类型匹配 DataTemplate（AOT 编译时绑定）。

## 实施步骤

### 1. 模型扩展

**新建 `Models/SshDockerApiMode.cs`**：常量类
```csharp
public static class SshDockerApiMode
{
    public const string Cli = "Cli";
    public const string Http = "Http";
}
```

**修改 `Models/SshHostProfile.cs`**：新增字段
```csharp
public string DockerApiMode { get; set; } = SshDockerApiMode.Cli;
```
（`SshJsonSerializerContext` 已注册 `SshHostProfile`，新字段自动包含，无需改）

**修改 `Stores/DefaultSshHostStore.cs`** `AddOrUpdate` 复制 `existing.DockerApiMode = host.DockerApiMode;`

### 2. HTTP API 实现

**新建 `Docker/Models/Http/DockerHttpContainer.cs` + `DockerHttpImage.cs`**：HTTP 响应专用模型
- `DockerHttpContainer`: `Id` / `Names:List<string>` / `Image` / `State` / `Status` / `Ports`（Docker REST 字段是驼峰小写 + Names 数组）
- `DockerHttpImage`: `Id` / `RepoTags:List<string>` / `Size:long`

**修改 `SshJsonSerializerContext.cs`**：追加
```csharp
[JsonSerializable(typeof(List<DockerHttpContainer>))]
[JsonSerializable(typeof(List<DockerHttpImage>))]
[JsonSerializable(typeof(DockerContainerCreateConfig))]
```

**新建 `Docker/SshDockerHttpApi.cs`**：实现 `IDockerApi` + `IDisposable`
- 构造注入 `(ISshTunnelManager tunnel, SshHostProfile host)`，`new HttpClient { Timeout = 60s }`，**不**在构造里 OpenAsync（避免 async ctor）
- 私有 `EnsureTunnelAsync(ct)`：首次调用时 `_tunnel.OpenAsync(_host, "127.0.0.1", _host.DockerPort, ct)` 得本地端口，设 `BaseAddress`
- 端点映射：
  - `GET v1.24/containers/json?all=true` → 反序列化为 `List<DockerHttpContainer>` → Map 到 `DockerContainer`（`Name = Names.FirstOrDefault()`）
  - `GET v1.24/images/json` → `List<DockerHttpImage>` → Map 到 `DockerImage`（`RepoTag = RepoTags.FirstOrDefault() ?? "<none>:<none>"`）
  - `POST v1.24/containers/{id}/start` / `POST v1.24/containers/{id}/stop` / `POST v1.24/containers/prune` / `POST v1.24/images/prune`
  - `GET v1.24/containers/{id}/logs?stdout=1&stderr=1` → string
  - `POST v1.24/containers/create?name={name}` body=config → 解析 `Id`
  - `DELETE v1.24/containers/{id}?force={force}` / `DELETE v1.24/images/{id}?force={force}`
- `Dispose`: `HttpClient.Dispose()` + `_tunnel.CloseAsync(_host.Id, "127.0.0.1", _host.DockerPort)`

### 3. 工厂

**新建 `Docker/IDockerApiFactory.cs`**：
```csharp
public interface IDockerApiFactory
{
    IDockerApi Create(SshHostProfile host);
}
```

**新建 `Docker/DefaultDockerApiFactory.cs`**：注入 `ISshConnectionManager` + `ISshTunnelManager`
```csharp
public IDockerApi Create(SshHostProfile host) => host.DockerApiMode switch
{
    SshDockerApiMode.Http => new SshDockerHttpApi(_tunnel, host),
    _ => new SshDockerCliApi(_connectionManager, host),
};
```
工厂本身不实现 `IDisposable`，返回的 `IDockerApi` 由调用方 Dispose。

### 4. DockerBrowserViewModel 改造

**修改 `ViewModels/Docker/DockerBrowserViewModel.cs`**：
- 构造签名：`(SshHostProfile host, IDockerApiFactory dockerFactory, IDialogService dialog)`
- 字段 `IDockerApi _docker` 保留，构造改为 `_docker = dockerFactory.Create(host)`
- 实现 `Dispose` 释放 `_docker`（如果实现了 `IDisposable`）：
```csharp
public void Dispose()
{
    if (_disposed) return;
    _disposed = true;
    (_docker as IDisposable)?.Dispose();
}
```

### 5. Feature Tab 插件机制

**新建 `ViewModels/SshHosts/SshFeatureTabViewModel.cs`**：抽象基类
```csharp
public abstract partial class SshFeatureTabViewModel : ObservableObject
{
    public abstract string Code { get; }
    public abstract string DisplayName { get; }
    [ObservableProperty] private bool _isEnabled;
    public abstract Task LoadAsync(SshHostProfile host);
    public abstract void Apply(SshHostProfile host);
}
```

**新建三个子 Tab**（每个 ViewModel 一个文件，遵守"文件名=类型名"约定）：
- `ViewModels/SshHosts/DockerFeatureTabViewModel.cs`：`DockerPort` / `DockerApiMode` + `DockerApiModeOptions:Option<string>[]`，`LoadAsync` 读 `host.DockerPort`/`DockerApiMode`/`EnabledFeatures.Contains(Docker)`，`Apply` 写回
- `ViewModels/SshHosts/TerminalFeatureTabViewModel.cs`：仅 `IsEnabled`
- `ViewModels/SshHosts/FileBrowserFeatureTabViewModel.cs`：仅 `IsEnabled`

**新建三个 Tab View**（UserControl，不继承 `View` 基类因为不是 Window）：
- `Views/SshHosts/DockerFeatureTabView.axaml(.cs)`：空参构造，依赖 DataContext 绑定
- `Views/SshHosts/TerminalFeatureTabView.axaml(.cs)`
- `Views/SshHosts/FileBrowserFeatureTabView.axaml(.cs)`

每个 Tab View 顶部一个 `CheckBox Content="启用此功能" IsChecked="{Binding IsEnabled}"` + 该功能特有配置 UI。

### 6. SshHostEditorViewModel 改造

**修改 `ViewModels/SshHosts/SshHostEditorViewModel.cs`**：
- 构造注入 `IEnumerable<SshFeatureTabViewModel> featureTabs`
- 暴露 `IReadOnlyList<SshFeatureTabViewModel> FeatureTabs`
- **删除** `FeatureOptions` / `IsDockerEnabled` / `DockerPort` / `SshFeatureOption[]` / `BuildFeatureOptions` / `OnFeatureChanged`
- `LoadHost` 末尾：`foreach (var t in FeatureTabs) await t.LoadAsync(host);`
- `ApplyToHost` 末尾：
```csharp
foreach (var t in FeatureTabs) t.Apply(host);
host.EnabledFeatures = FeatureTabs.Where(t => t.IsEnabled).Select(t => t.Code).ToList();
```
（`DockerPort` / `DockerApiMode` 由 DockerFeatureTabViewModel.Apply 直接写到 host）

### 7. SshHostEditorView 改造

**修改 `Views/SshHosts/SshHostEditorView.axaml`** 顶层结构：
```xml
<Window ...>
  <Grid Rows="*,Auto">
    <TabControl Grid.Row="0">
      <TabItem Header="{x:Static resSSH:SSHResource.TabSshHostBasic}">
        <ScrollViewer>
          <StackPanel>
            ...名称/主机/端口/用户名...
            ...认证 GroupBox...
            ...描述...
          </StackPanel>
        </ScrollViewer>
      </TabItem>
      <TabItem Header="{x:Static resSSH:SSHResource.TabDocker}">
        <local:DockerFeatureTabView/>  <!-- 或用 ContentControl + DataTemplate -->
      </TabItem>
      <TabItem Header="{x:Static resSSH:SSHResource.TabTerminal}">
        <local:TerminalFeatureTabView/>
      </TabItem>
      <TabItem Header="{x:Static resSSH:SSHResource.TabFileBrowser}">
        <local:FileBrowserFeatureTabView/>
      </TabItem>
    </TabControl>
    <StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Right">
      <Button Command="{Binding CancelCommand}" .../>
      <Button Command="{Binding ConfirmCommand}" .../>
    </StackPanel>
  </Grid>
</Window>
```

注：因 `FeatureTabs` 是 DI 收集的有序集合，但 View 内直接静态引用三个具体 Tab View 更符合 AOT 编译时绑定且简洁；DataContext 由父 `SshHostEditorViewModel` 通过 `FeatureTabs` 在 ViewModel 内手动暴露具体实例属性（如 `DockerTab` / `TerminalTab` / `FileBrowserTab`）绑定到对应 TabItem.Content。

**最终 View 绑定方式**：
- `SshHostEditorViewModel` 暴露具体属性：`public DockerFeatureTabViewModel DockerTab { get; }` 等（从 `FeatureTabs` 转型或直接由 DI 注入具体实例）
- 各 `TabItem.Content` 绑定 `{Binding DockerTab}` 并设置对应 View 类型作为 ContentControl 的 DataTemplate

或更简单：让 `SshHostEditorView.axaml` 直接实例化三个 Tab View，在 `x:DataType` 编译时绑定 DataContext（DataContext 由 Avalonia 自动从父级继承，但 Tab View 需要显式 DataContext 绑定）。

**推荐做法**（兼顾 AOT 与简洁）：在 `SshHostEditorViewModel` 暴露具体 Tab 属性，View 直接静态写三个 `TabItem`，每个内放 `<local:DockerFeatureTabView DataContext="{Binding DockerTab}"/>`。

### 8. SSHModule 注册

**修改 `SSHModule.cs`**：
```csharp
protected override void RegisterServices(IServiceCollection services)
{
    services.AddSingleton<ISshHostStore, DefaultSshHostStore>();
    services.AddSingleton<ISshConnectionManager, DefaultSshConnectionManager>();
    services.AddSingleton<ISshTunnelManager, DefaultSshTunnelManager>();
    services.AddSingleton(sp => new Lazy<ISshTunnelManager>(sp.GetRequiredService<ISshTunnelManager>));
    services.AddSingleton<IDockerApiFactory, DefaultDockerApiFactory>();
    services.AddTransient<DockerFeatureTabViewModel>();
    services.AddTransient<TerminalFeatureTabViewModel>();
    services.AddTransient<FileBrowserFeatureTabViewModel>();
}
```

**修改 `KeyedMapView<DockerBrowserView, DockerBrowserViewModel>` 工厂委托**：
```csharp
(p, sp) => new DockerBrowserViewModel(
    p.Get<Models.SshHostProfile>(SshViewKeys.DockerBrowserParams.Host),
    sp.GetRequiredService<IDockerApiFactory>(),
    sp.GetRequiredService<AzureTools.Services.IDialogService>()),
```

### 9. 资源字符串

**修改 `Properties/SSHResource.resx` + `SSHResource.Designer.cs`** 追加：
- `TabSshHostBasic` → "基本"
- `TabDocker` → "Docker"
- `TabTerminal` → "终端"
- `TabFileBrowser` → "文件浏览"
- `LblSshDockerApiMode` → "Docker API 模式"
- `DockerApiModeCli` → "命令行 (CLI)"
- `DockerApiModeHttp` → "HTTP 隧道"
- `LblSshFeatureEnable` → "启用此功能"

## 验证

1. `dotnet build Toolkit.Modules.SSH.csproj` 编译通过
2. `dotnet build AzureTools.csproj` 主项目编译通过（含 AOT）
3. 运行时验证：
   - 启动应用 → 菜单 SSH → 主机管理 → 编辑主机 → 顶层出现 4 个 Tab（基本/Docker/终端/文件浏览）
   - Docker Tab：勾选"启用此功能"，输入 DockerPort，选择 API 模式（默认 Cli），保存后重新打开编辑器，配置保留
   - 旧档案（无 DockerApiMode 字段）：默认走 Cli，能正常加载
   - Docker API 模式 = Cli：主机管理列表右键打开 Docker 浏览器，容器列表加载正常（原行为不变）
   - Docker API 模式 = Http：浏览器加载容器列表，验证 SSH 隧道转发 + HttpClient 调用成功
   - 关闭 Docker 浏览器：HttpClient 与隧道正确释放（无残留 ForwardedPort）
   - 关闭主机管理或修改主机：隧道自动清理

## 关键文件清单

| 操作 | 文件 |
|---|---|
| 新建 | `Models/SshDockerApiMode.cs` |
| 新建 | `Docker/Models/Http/DockerHttpContainer.cs`、`DockerHttpImage.cs` |
| 新建 | `Docker/SshDockerHttpApi.cs` |
| 新建 | `Docker/IDockerApiFactory.cs`、`Docker/DefaultDockerApiFactory.cs` |
| 新建 | `ViewModels/SshHosts/SshFeatureTabViewModel.cs` |
| 新建 | `ViewModels/SshHosts/DockerFeatureTabViewModel.cs`、`TerminalFeatureTabViewModel.cs`、`FileBrowserFeatureTabViewModel.cs` |
| 新建 | `Views/SshHosts/DockerFeatureTabView.axaml(.cs)`、`TerminalFeatureTabView.axaml(.cs)`、`FileBrowserFeatureTabView.axaml(.cs)` |
| 修改 | `Models/SshHostProfile.cs`（+DockerApiMode） |
| 修改 | `Stores/DefaultSshHostStore.cs`（复制 DockerApiMode） |
| 修改 | `SshJsonSerializerContext.cs`（+HTTP 模型） |
| 修改 | `Docker/SshDockerCliApi.cs`（无逻辑改，可能微调 public 可见性便于工厂创建） |
| 修改 | `ViewModels/Docker/DockerBrowserViewModel.cs`（注入 IDockerApiFactory + Dispose） |
| 修改 | `ViewModels/SshHosts/SshHostEditorViewModel.cs`（注入 IEnumerable<SshFeatureTabViewModel> + 暴露具体 Tab 属性） |
| 修改 | `Views/SshHosts/SshHostEditorView.axaml(.cs)`（TabControl 重构） |
| 修改 | `SSHModule.cs`（注册工厂 + 三个 Tab） |
| 修改 | `Properties/SSHResource.resx` + `SSHResource.Designer.cs`（追加资源） |

## AOT 注意事项

- `SshFeatureTabViewModel` 用抽象类（非 interface）+ `partial` + `[ObservableProperty]`，子类源生成器正常工作。
- 不用反射创建 Tab 实例，全部走 DI（`IEnumerable<SshFeatureTabViewModel>` 由 DI 容器解析所有注册实现）。
- Tab View 是 `UserControl`，不继承 `View` 基类（`View` 是 `Window`），空参构造 + Avalonia 自动 DataContext 绑定。
- `x:DataType` 在每个 Tab View 内显式标注具体子类类型，保证编译时绑定。
- `SshDockerHttpApi` 用 `SshJsonSerializerContext.Default.ListDockerHttpContainer` 显式 `JsonTypeInfo`，避免 AOT 反射回退。
