## 项目概览
- 解决方案由多个项目组成：AzureTools（主项目，Avalonia UI）、AzureTools.Abstractions（抽象接口）、AzureTools.Avalonia（Avalonia 基础设施：ViewModel/View 基类、对话框、转换器）、AzureTools.Domain（领域模型）、AzureTools.Tests（测试）
- 技术栈：.NET 10 + Avalonia + Native AOT（IsAotCompatible=true）+ CommunityToolkit.Mvvm
- 主项目 csproj 启用 AvaloniaUseCompiledBindingsByDefault=true（编译时绑定）和 IsAotCompatible=true
- 使用 TrimmerRootDescriptor（TrimmerRoots.xml）和 TrimmerRootAssembly 显式保留 Avalonia 程序集，防止 AOT 裁剪丢失必要类型

## 工程约定
- ViewModel 使用 CommunityToolkit.Mvvm 源生成器（通过 [ObservableProperty] 和 [RelayCommand] 自动生成 INotifyPropertyChanged 和 ICommand）
- 绑定采用 Avalonia 编译时绑定（x:CompileBindings="True" / AvaloniaUseCompiledBindingsByDefault），不在 code-behind 写手动绑定代码
- 使用 Directory.Packages.props 集中管理包版本；.csproj 文件中 PackageReference 不指定 Version 属性
- 抽象接口放在 AzureTools.Abstractions 项目（Services、Stores、Tools/Encryptions、Tools/IO 子目录）；接口实现放在对应项目
- 服务接口和实现分离：IDialogService/IThemeService 在 Abstractions/Services，DefaultDialogService/DefaultThemeService 在 AzureTools.Avalonia/Services
- ViewModel 处理完整业务逻辑（包括文件操作、Azure SDK 调用），View 层简化为纯 UI 绑定
- ViewModel 必须实现操作与保存分离：包含 HasChanges 跟踪、DiscardChangesCommand 和更新 HasChanges 的 Save 方法（子类自行实现，基类不强制）
- 关闭确认通过重写 ViewModel 基类的 CanClose 属性（Func<Task<bool>>）实现，而非 View 的 OnClosing 事件
- 资源文件按职责分离：FormResources.resx（UI 设计时元素：标签、按钮文本、窗体标题、菜单、图标）、MessageResources.resx（运行时消息）、ModelResources.resx（模型相关资源）、DialogResources.resx（对话框资源，位于 AzureTools.Avalonia 项目）
- 通信相关 View（如 SelectItemsView、MultiSelectItemsView、ConfirmDialog、PromptDialog、MessageBoxDialog）放在 AzureTools.Avalonia/Views/Comms 目录
- 可复用 UI 组件（如 WebViewWindow）放在 AzureTools.Avalonia/Views/Comms 目录，通过构造函数接收参数支持多处复用
- 下拉选项使用泛型 Option<T>（位于 AzureTools.Domain/Models/Option.cs），配合 IOption 接口
- View（如 MainView、KeyVaultSecretMainView）必须通过 AddView<TView,TViewModel> 扩展方法注册到 DI 容器（AddScoped），以支持服务解析和作用域管理
- 新模块遵循五层架构：Model（AzureTools.Domain/Models）→ Store（AzureTools/Stores）→ SDK（AzureTools/SDK，接口+实现）→ ViewModel（AzureTools/ViewModels）→ View（AzureTools/Views），参考 ContainerRegistries 模块模式
- 所有 ViewModel 必须继承 ViewModel 基类（位于 AzureTools.Avalonia/ViewModels/ViewModel.cs）；所有 View 必须继承 View 基类（位于 AzureTools.Avalonia/Views/View.cs）
- ViewModel 基类提供 Attach、SetContext、Close、ShowDialog<TView,TViewModel>、Show<TView>、CreateChildContext、GetWindow 方法，用于窗口管理和子窗口打开
- ViewModel 基类的 CanClose 是 virtual 属性（Func<Task<bool>>），子类可重写以实现关闭确认逻辑
- 子窗口通过 IViewModelFactory + ViewModelContext 注入创建；子窗口通过 ViewModel 基类的 ShowDialog<TView,TViewModel> 打开，自动创建子作用域并在关闭时 Dispose
- ViewModelContext 封装 IServiceProvider 和 IServiceScope，通过 CreateChild() 创建子作用域，实现 IDisposable 在窗口关闭时释放资源
- ViewModel 中的简单参数（string、int 等）必须封装为对应的 xxxOption 类（如 TenantEditOption、ProjectEditOption），实现 IViewModelOption 标记接口，放在各自模块的 ViewModels 目录
- View 的 code-behind 构造函数必须接收 ViewModel + ViewModelContext 参数，调用基类构造函数完成 DataContext 绑定和 Attach
- MainViewModel 通过 CreateView(serviceProvider, viewName) 工厂方法按名称解析 View，再调用 Show(view) 打开；View 名通过 nameof 维护以保持编译时检查
- 平台特定服务使用 RuntimeInformation.IsOSPlatform 判断后注册（如 WindowsMessageFallback 仅在 Windows 注册）

## 硬性约束
- FileStoreContext 使用 System.Text.Json 配合源生成的 TypeInfoResolver；JSON 序列化上下文按模块拆分：ToolkitJsonSerializerContext（Toolkit.Modules）、AzureJsonSerializerContext（Toolkit.Modules.Azure）、SshJsonSerializerContext（Toolkit.Modules.SSH）；新增模块序列化类型时在对应模块的 SerializerContext 中登记，不要在主项目集中登记；多态派生类型通过基类上的 [JsonPolymorphic] + [JsonDerivedType] 配置（如 ValidationOptions），而非 Newtonsoft JsonSerializerSettings
- 在 DI 中注册 JsonSerializerOptions 时必须设置 options.TypeInfoResolver = JsonTypeInfoResolver.Combine(ToolkitJsonSerializerContext.Default, AzureJsonSerializerContext.Default, SshJsonSerializerContext.Default)；仅调用 new XxxJsonSerializerContext(options) 不会关联 resolver，导致 AOT 反序列化回退到被裁剪的反射而失败；每个模块的 SerializerContext 必须重复 [JsonSourceGenerationOptions]（DefaultIgnoreCondition/PropertyNameCaseInsensitive 是编译期烘焙进元数据的）
- FileStoreContext 构造函数不接受 'encrypted' 参数；构造函数签名为 (IHostEnvironment environment, IOptions<JsonSerializerOptions> jsonOptions)；加密状态通过 AesEncryptionService.LoadPackage() 检测 EncryptedPackage 是否存在
- FileStoreContext 序列化/反序列化必须使用 _jsonOptions.GetTypeInfo(typeof(List<TEntity>)) 获取 JsonTypeInfo，再传给 JsonSerializer.Deserialize/Serialize，避免 AOT 反射回退
- 对话框结果通过 ViewModel 的 bool? DialogResult 属性传递，窗体关闭由 ViewModel 的 Completed 事件控制；不要使用 Avalonia Window 的 Close 配合按钮 DialogResult 模式
- Avalonia 的 ShowDialog(owner) 要求所有者窗口可见；不要在 ShowDialog() 前对所有者调用 Hide()
- Native AOT 中禁止使用 C# `dynamic` 关键字——Microsoft.CSharp.RuntimeBinder 被裁剪，运行时崩溃（如 ExpressionTreeCallRewriter 中的 ArgumentNullException）。使用具体类型或泛型方法在编译时完成重载决议，而非 `(dynamic)` 运行时分派
- 泛型方法创建 ViewModel/View 实例时，泛型参数必须标注 [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]，保证 AOT 裁剪保留公共构造函数（参考 IViewModelFactory.Create、ViewModel.ShowDialog、AddView 扩展方法）
- 主项目 csproj 必须配置 TrimmerRootAssembly 保留 Avalonia 相关程序集（Avalonia、Avalonia.Base、Avalonia.Controls、Avalonia.Controls.DataGrid、Avalonia.Controls.WebView、Avalonia.Desktop、Avalonia.Markup.Xaml、Avalonia.Skia、Avalonia.Themes.Fluent），否则 AOT 发布后运行时崩溃

## 经验教训
- 操作与保存必须分离；关闭窗口通过重写 ViewModel.CanClose 属性实现确认逻辑，支持通过 DiscardChanges 回滚
- CommunityToolkit.Mvvm 源生成属性在 Native AOT 下无法可靠通过反射访问；改用直接委托访问或 xaml 编译时绑定
- Avalonia Hide() → ShowDialog() → Show() 模式不支持；ShowDialog() 本身是模态的，会保持对话框在所有者之上，无需先隐藏所有者
- 子窗口应通过 ShowDialog<TView,TViewModel> 打开（自动管理子作用域），而非直接 GetService<TView>，以确保 ViewModelContext 作用域正确创建和释放
