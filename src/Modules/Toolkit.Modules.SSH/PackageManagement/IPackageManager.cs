using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.PackageManagement
{
    public interface IPackageManager
    {
        /// <summary>
        /// 当前已识别的包管理器目录项，未识别时为 null
        /// </summary>
        PackageManagerCatalog Catalog { get; }

        /// <summary>
        /// 按发行版信息与远端实际命令探测并锁定包管理器，返回命中的目录项
        /// </summary>
        Task<PackageManagerCatalog> DetectAsync(CancellationToken ct = default);

        /// <summary>
        /// 按关键字搜索远端仓库中的软件包
        /// </summary>
        Task<IReadOnlyList<PackageInfo>> SearchAsync(string keyword, CancellationToken ct = default);

        /// <summary>
        /// 列出远端已安装的软件包
        /// </summary>
        Task<IReadOnlyList<PackageInfo>> ListInstalledAsync(CancellationToken ct = default);

        /// <summary>
        /// 安装指定软件包
        /// </summary>
        Task<PackageOperationResult> InstallAsync(string name, CancellationToken ct = default);

        /// <summary>
        /// 卸载指定软件包
        /// </summary>
        Task<PackageOperationResult> RemoveAsync(string name, CancellationToken ct = default);

        /// <summary>
        /// 升级指定软件包到最新可用版本
        /// </summary>
        Task<PackageOperationResult> UpgradeAsync(string name, CancellationToken ct = default);

        /// <summary>
        /// 降级指定软件包，需要目标版本时由实现方传入
        /// </summary>
        Task<PackageOperationResult> DowngradeAsync(string name, string version, CancellationToken ct = default);
    }
}
