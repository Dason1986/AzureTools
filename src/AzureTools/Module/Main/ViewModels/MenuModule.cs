using System.Collections.Generic;

namespace AzureTools.Module.Main.ViewModels
{
    public class MenuModule
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public IReadOnlyList<MenuItemCard> Cards { get; set; }
    }
}
