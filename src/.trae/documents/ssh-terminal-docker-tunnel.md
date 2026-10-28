# SSH 终端 + Docker 隧道模块实现计划

## Context

当前 SSH 模块仅有"文件浏览"功能。用户要求扩展三项能力：
1. **SSH 终端**：通过 SSH 进入远程 shell，复用 Comm 模块已有的终端窗口
2. **Docker 模块**：通过 SSH 端口转发（隧道）暴露远程 Docker daemon 的 HTTP API，本地用 HttpClient 调用，做容器/镜像的列表、启停、日志、创建、删除、清理
3. **SSH 隧道管理服务**：统一管理 ForwardedPortLocal 生命周期，与现有 [ISshConnectionManager](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/Managers/ISshConnectionManager.cs) 关联（复用同一 SshClient，不重复建立连接）

## 设计要点

### 隧道服务
- 新增 `ISshTunnelManager`（接口）+ `DefaultSshTunnelManager`（实现），位于 `Toolkit.Modules.SSH/Managers/`
- 复用 `ISshConnectionManager.GetClient(host)` 拿到 SshClient，在其上创建 `ForwardedPortLocal("127.0.0.1", 0, remoteHost, remotePort)`，启动后取 `LocalPort`
- 同一 hostId+remoteHost+remotePort 复用同一 ForwardedPort，键为 `(hostId, remoteHost, remotePort)` 元组，缓存到 `ConcurrentDictionary`
- `ISshConnectionManager.Disconnect(hostId)` 调用时隧道需一起释放——在 `DefaultSshConnectionManager.Disconnect` 触发 `ISshTunnelManager` 释放该 hostId 所有隧道（依赖注入 `ISshTunnelManager` 到 `DefaultSshConnectionManager`，或反向：让隧道服务订阅连接断开事件）。**采用方案**：`DefaultSshConnectionManager.Disconnect(hostId)` 中遍历调用注入的 `ISshTunnelManager` 的 `CloseAllAsync(hostId)`，避免双向依赖时使用懒加载 `Lazy<ISshTunnelManager>` 或在 `Disconnect` 中显式清理（隧道服务自己用 `ConcurrentDictionary` 缓存，`DefaultSshConnectionManager` 不直接持有隧道服务引用——通过 `IServiceProvider` 解析或注入 `Lazy<ISshTunnelManager>`）。**最终方案**：`DefaultSshConnectionManager` 构造函数注入 `Lazy<ISshTunnelManager>`，避免循环依赖；`Disconnect(hostId)` 时调用 `tunnel.Value.CloseAllAsync(hostId)`。

### Docker HTTP API
- 新增 `IDockerApi`（接口）+ `DefaultDockerApi`（实现），位于 `Toolkit.Modules.SSH/Docker/`
- `DefaultDockerApi` 构造接收 `ISshTunnelManager` + `SshHostProfile`，构造时通过隧道建立到 `127.0.0.1:host.DockerPort` 的本地端口，`HttpClient.BaseAddress = http://localhost:{localPort}`
- API 方法覆盖列表/启停/日志/创建/删除/清理：
  - `ListContainersAsync(all=true)` → GET /containers/json?all=true
  - `ListImagesAsync()` → GET /images/json
  - `StartContainerAsync(id)` → POST /containers/{id}/start
  - `StopContainerAsync(id)` → POST /containers/{id}/stop
  - `GetContainerLogsAsync(id)` → GET /containers/{id}/logs?stdout=1&stderr=1
  - `CreateContainerAsync(config)` → POST /containers/create
  - `RemoveContainerAsync(id, force)` → DELETE /containers/{id}?force={force}
  - `PruneContainersAsync()` → POST /containers/prune
  - `RemoveImageAsync(id, force)` → DELETE /images/{id}?force={force}
  - `PruneImagesAsync()` → POST /images/prune
- 容器/镜像模型类放在 `Toolkit.Modules.SSH/Docker/Models/`，字段与 Docker API JSON 对齐（Id/Names/Image/State/Status/Ports 等）

### SSH 终端会话
- 新增 `SshShellCliSession`，位于 `Toolkit.Modules.SSH/Managers/`，实现 `ICliStreamSession`（[接口在 AzureTools.Abstractions/Cli/ICliStreamSession.cs](file:///d:/guodexuan/source/AzureTools/AzureTools/src/AzureTools.Abstractions/Cli/ICliStreamSession.cs)）
- 构造接收 `SshClient` + `terminalName`，调用 `client.CreateShellStream("xterm", 80, 24, 0, 0, 4096)`
- 启动后台 Task 读 ShellStream，按行触发 `OutputReceived`
- `WriteLineAsync` 写入 ShellStream 并刷新
- `Stop` 关闭 ShellStream 并 Dispose
- **窗口复用**：SSHModule 不新建窗口，[SshHostManagementViewModel.OpenFeatureAsync](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/ViewModels/SshHosts/SshHostManagementViewModel.cs) 中 `Terminal` 分支调用 `ShowKeyed(CommViewKeys.CliStreamSession, new ViewParameters().Set("session", session).Set("title", host.Name).Set("isInteractive", true))`，复用 [Comm 模块已注册的 CliStreamSessionView](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.Comm/CommModule.cs#L24)。SSH 项目需新增对 `Toolkit.Modules.Comm` 的 ProjectReference

### Docker 浏览器窗口
- 新增 `DockerBrowserViewModel` + `DockerBrowserView.axaml`，位于 `Toolkit.Modules.SSH/Views/Docker/` 和 `ViewModels/Docker/`
- 顶部工具栏：刷新、容器/镜像切换 Tab、清理按钮
- 左侧 DataGrid 列出容器或镜像（根据 Tab 切换），右侧选中详情
- 容器行操作：启动/停止/查看日志/删除；镜像行操作：删除
- `IDialogService` 注入用于错误提示
- SSHModule 用 `KeyedMapView<DockerBrowserView, DockerBrowserViewModel>("DockerBrowser", ...)` 注册

### 模型与档案扩展
- [SshHostProfile](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/Models/SshHostProfile.cs) 增加 `int DockerPort = 2375`
- [SshHostFeature](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/Models/SshHostFeature.cs) 的 `All` 数组追加 `Terminal`、`Docker` 两项
- [SshJsonSerializerContext](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/SshJsonSerializerContext.cs) 已覆盖 `SshHostProfile`，新增的 int 字段自动生成元数据，无需改动
- [DefaultSshHostStore.AddOrUpdate](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/Stores/DefaultSshHostStore.cs) 增加 `existing.DockerPort = host.DockerPort` 复制

### 编辑器
- [SshHostEditorViewModel](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/ViewModels/SshHosts/SshHostEditorViewModel.cs) 增加 `DockerPort` 可观察属性；`LoadHost` 读入；`ApplyToHost` 写回
- [SshHostEditorView.axaml](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/Views/SshHosts/SshHostEditorView.axaml) 在功能模块 GroupBox 下方追加 DockPanel：标签 + `TextBox` 绑定 `DockerPort`，`IsVisible` 绑定到 Docker 勾选项的 IsEnabled（通过 ViewModel 暴露 `IsDockerEnabled` 属性，订阅 FeatureOptions 变更）

### 资源字符串
- SSHResource.resx + Designer 增加：`FeatureTerminal`(终端)、`FeatureDocker`(Docker)、`LblSshDockerPort`(Docker 端口)、`TitleSshTerminal`(SSH 终端)、`TitleDockerBrowser`(Docker 容器管理)、`ColContainerId/Name/Image/State/Status`、`ColImageId/RepoTag/Size`、`BtnStart/Stop/Logs/Remove/Prune`、`MsgNoContainers`、`MsgDockerNotEnabled`

### SSHModule 注册
- [SSHModule.RegisterServices](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/SSHModule.cs) 注册 `ISshTunnelManager`/`DefaultSshTunnelManager`(Singleton)、`IDockerApi`/`DefaultDockerApi`(Transient)
- `MapView` 增加 `DockerBrowserView`/`DockerBrowserViewModel`
- 终端窗口复用 Comm 的键，不需要注册

### 管理窗口路由
- [SshHostManagementViewModel.OpenFeatureAsync](file:///d:/guodexuan/source/AzureTools/AzureTools/src/Modules/Toolkit.Modules.SSH/ViewModels/SshHosts/SshHostManagementViewModel.cs) 增加 Terminal、Docker 两个 case
  - Terminal：new SshShellCliSession(connectionManager.GetClient(SelectedItem), "xterm-256color")，ShowKeyed(CommViewKeys.CliStreamSession, ...)
  - Docker：ShowKeyed("DockerBrowser", new ViewParameters().Set("host", SelectedItem))

## 关键文件清单

| 操作 | 文件 |
|---|---|
| 新建 | `Managers/ISshTunnelManager.cs`、`Managers/DefaultSshTunnelManager.cs` |
| 新建 | `Managers/SshShellCliSession.cs` |
| 新建 | `Docker/IDockerApi.cs`、`Docker/DefaultDockerApi.cs` |
| 新建 | `Docker/Models/DockerContainer.cs`、`Docker/Models/DockerImage.cs`、`Docker/Models/DockerContainerCreateConfig.cs` |
| 新建 | `ViewModels/Docker/DockerBrowserViewModel.cs` |
| 新建 | `Views/Docker/DockerBrowserView.axaml(.cs)` |
| 修改 | `Models/SshHostProfile.cs`（+DockerPort） |
| 修改 | `Models/SshHostFeature.cs`（+Terminal/Docker） |
| 修改 | `Managers/ISshConnectionManager.cs`（+Disconnect 触发隧道清理契约注释） |
| 修改 | `Managers/DefaultSshConnectionManager.cs`（注入 Lazy<ISshTunnelManager>，Disconnect 时清理隧道） |
| 修改 | `Stores/DefaultSshHostStore.cs`（复制 DockerPort） |
| 修改 | `ViewModels/SshHosts/SshHostEditorViewModel.cs`（+DockerPort/IsDockerEnabled） |
| 修改 | `Views/SshHosts/SshHostEditorView.axaml`（+Docker 端口输入） |
| 修改 | `ViewModels/SshHosts/SshHostManagementViewModel.cs`（OpenFeatureAsync 增 Terminal/Docker 分支） |
| 修改 | `SSHModule.cs`（注册隧道、Docker 服务 + MapView DockerBrowser） |
| 修改 | `Toolkit.Modules.SSH.csproj`（+ProjectReference Toolkit.Modules.Comm） |
| 修改 | `Properties/SSHResource.resx` + `SSHResource.Designer.cs`（追加资源） |

## 验证

1. `dotnet build Toolkit.Modules.SSH.csproj` 编译通过
2. `dotnet build AzureTools.csproj`（主项目）编译通过
3. 运行时验证：
   - 启动应用 → 菜单 SSH → 主机管理 → 编辑主机，勾选"终端"和"Docker"，Docker 端口输入框出现，保存后右侧菜单出现两项
   - 选一台可连接主机 → 点击"终端" → 弹出终端窗口可交互
   - 选一台 Docker daemon 已开 TCP 2375 的主机 → 点击"Docker" → 容器列表加载；启停容器、查看日志、删除镜像、清理悬空镜像
4. 关闭主机管理窗口或修改主机配置时，隧道自动释放（不残留 ForwardedPort）
