using AzureTools.Models.KeyVaults;

using AzureTools.Services.KeyVaults.Validators;

namespace AzureTools.Services.KeyVaults.Factories
{
    public interface IKeyFactory
    {
        string Type { get; }
        IKeyValidator CreateValidator();
        ValidationOptions CreateOptions(string json);
    }
}
