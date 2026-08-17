using System;

namespace AzureTools.SDK.Models
{
    public abstract class AzureEnv
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }

        public string TenantId { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceType { get; set; }
        public string ResourceGroupName { get; set; }
    }
}
