using Toolkit.Modules.Azure.KeyVaults.Validators;
using Toolkit.Modules.Azure.Models.KeyVaults;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public interface IKeyFactory
    {
        string Type { get; }
        IKeyValidator CreateValidator();
        ValidationOptions CreateOptions(string json);
    }
}
