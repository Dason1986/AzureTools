using System;
using System.ComponentModel.DataAnnotations;

namespace AzureTools.Models
{
    public abstract class AzureEnvOption
    {
        [Key]
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