using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.FirewallManagement
{
    public interface IFirewallManager
    {
        FirewallBackendCatalog Catalog { get; }

        Task<FirewallBackendCatalog> DetectAsync(CancellationToken ct = default);

        Task<bool> IsEnabledAsync(CancellationToken ct = default);

        Task<IReadOnlyList<FirewallRule>> ListRulesAsync(CancellationToken ct = default);

        Task<PackageOperationResult> AddRuleAsync(string port, string protocol, bool allow, CancellationToken ct = default);

        Task<PackageOperationResult> RemoveRuleAsync(FirewallRule rule, CancellationToken ct = default);

        Task<PackageOperationResult> SetEnabledAsync(bool enabled, CancellationToken ct = default);
    }
}
