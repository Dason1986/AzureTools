using AzureTools.Models;
using CommunityToolkit.Mvvm.Input;

namespace AzureTools.ViewModels
{
    public class MenuItemCard
    {
        public MenuItem Menu { get; set; }

        public string Text => Menu?.Name ?? string.Empty;

        public byte[] Icon => Menu?.Icons;

        public bool IsEnabled => !string.IsNullOrEmpty(Menu?.Form);

        public IRelayCommand ClickCommand { get; set; }
    }
}
