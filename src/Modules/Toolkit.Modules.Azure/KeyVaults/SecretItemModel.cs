using AzureTools.SDK.KeyVaults;
using Toolkit.Modules.Azure.Models.KeyVaults;

namespace Toolkit.Modules.Azure.KeyVaults
{
    public class SecretItemModel : SecretItem
    {
        public string Rule => Validation?.Type;
        public ValidationOptions Validation { get; set; }

        public static SecretItemModel FromModel(SecretItem x)
        {
            return new SecretItemModel
            {
                Name = x.Name,
                Value = x.Value,
                Created = x.Created,
                Deleted = x.Deleted,
                Description = x.Description,
                Enabled = x.Enabled,
                Expires = x.Expires,
                Required = x.Required,
                Tags = x.Tags,
                Updated = x.Updated,
                Version = x.Version,
            };
        }
    }
}
