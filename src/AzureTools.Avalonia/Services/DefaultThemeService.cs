using Avalonia;
using Avalonia.Styling;

namespace AzureTools.Services
{
    public class DefaultThemeService : IThemeService
    {
        public void ApplyTheme(ThemeMode mode)
        {
            if (Application.Current == null) return;
            Application.Current.RequestedThemeVariant = mode switch
            {
                ThemeMode.Light => ThemeVariant.Light,
                ThemeMode.Dark => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
        }
    }
}
