using AzureTools.SDK.Models;

namespace AzureTools.SDK.Abstractions
{
    public interface ITenantProvider
    {
        AzureTenant GetTenant(string tenantId);
    }
}
