using AzureTools.SDK.Models;
using Toolkit.Modules.Models;
using Toolkit.Modules.Stores;

namespace Toolkit.Modules.Azure
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IToolkitStore _store;

        public TenantProvider(IToolkitStore store)
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
