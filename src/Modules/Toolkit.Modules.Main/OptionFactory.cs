using AzureTools;
using Toolkit.Modules.Main.Properties;
using Toolkit.Modules.Models;

namespace Toolkit.Modules.Main
{
    class OptionFactory
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
                new Option<ThemeMode>(ThemeMode.System, MainResource.ThemeSystem),
                new Option<ThemeMode>(ThemeMode.Light, MainResource.ThemeLight),
                new Option<ThemeMode>(ThemeMode.Dark, MainResource.ThemeDark)
            ];
        }


    }
}
