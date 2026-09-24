using AzureTools.SDK.Models;

namespace Toolkit.Modules.Azure
{
    public interface ITenantProvider
    {
        AzureTenant GetTenant(string tenantId);
    }
}
