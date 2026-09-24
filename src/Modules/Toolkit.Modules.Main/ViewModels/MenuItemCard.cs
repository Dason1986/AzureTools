
using AzureTools;
using CommunityToolkit.Mvvm.Input;

namespace Toolkit.Modules.Main.ViewModels
{
    public class MenuItemCard
    {
        public IMenuItem Menu { get; set; }

        public string Text => Menu?.Name ?? string.Empty;

        public byte[] Icon => Menu?.Icon;

        public bool IsEnabled => Menu?.ViewModelType != null;

        public IRelayCommand ClickCommand { get; set; }
    }
}
