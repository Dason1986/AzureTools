using AzureTools.Extensions;
using AzureTools.Models;
using AzureTools.SDK.Abstractions;
using AzureTools.SDK.Models;
using AzureTools.Stores;

namespace AzureTools.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IAzureToolsStore _store;

        public TenantProvider(IAzureToolsStore store)
        {
            _store = store;
        }

        public AzureTenant GetTenant(string tenantId)
        {
            TenantProfile tenant = _store.GetTenant(tenantId);
            return tenant.ToSdk();
        }
    }
}
