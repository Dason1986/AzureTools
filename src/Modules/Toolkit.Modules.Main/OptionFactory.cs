using AzureTools;
using Toolkit.Modules.Models;
using Toolkit.Modules.Properties;

namespace Toolkit.Modules.Main
{
    public class OptionFactory
    {
        public static readonly OptionFactory Instance = new OptionFactory();

        public Option<TenantMode>[] GetTenantModes()
        {
            return
            [
                new Option<TenantMode>(TenantMode.Single, "单租户"),
                new Option<TenantMode>(TenantMode.Multiple, "多租户")
            ];
        }
        public Option<ThemeMode>[] GetThemeModes()
        {
            return
            [
                new Option<ThemeMode>(ThemeMode.System, FormResources.ThemeSystem),
                new Option<ThemeMode>(ThemeMode.Light, FormResources.ThemeLight),
                new Option<ThemeMode>(ThemeMode.Dark, FormResources.ThemeDark)
            ];
        }


    }
}
