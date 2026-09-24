using System.Collections.Generic;

namespace Toolkit.Modules.Main.ViewModels
{
    public class MenuModule
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public IReadOnlyList<MenuItemCard> Cards { get; set; }
    }
}
