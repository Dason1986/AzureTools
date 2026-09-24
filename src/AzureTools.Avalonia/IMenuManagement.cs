using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureTools
{
    public interface IMenuManagement
    {
        IMenuItem[] GetMenus();
    }
    class DefaultMenuManagement : IMenuManagement
    {
        private readonly IList<MenuItem> menus;

        public DefaultMenuManagement(IList<MenuItem> menus)
        {
            this.menus = menus;
        }


        public IMenuItem[] GetMenus() => menus.ToArray();
    }
    public interface IModuleMenuBuild
    {
        IMenuBuildItem AddMenu(
            string code,
            string name,
            byte[] icon,
            Type viewModelType,
            int order = 0);
    }

    public interface IMenuItem
    {
        string Code { get; set; }
        string Name { get; set; }
        byte[] Icon { get; set; }
        Type ViewModelType { get; }
        int Order { get; }
        IModuleItem Module { get; }
    }
    public interface IModuleItem
    {
        string Code { get; set; }
        string Name { get; set; }
        int Order { get; }
    }
    public interface IMenuBuildItem
    {
        IMenuBuildItem AddNav();
        IMenuBuildItem AttTools(string shortkey);
        IMenuBuildItem RequirePermission(string permissionCode);
        IMenuBuildItem Configure(Action<IMenuBuildItem> configure);
    }
    record class ModuleMenuIntent
    {
        public ModuleMenuIntent(string code, string name, int order = 0)
        {
            Code = code;
            Name = name;
            Order = order;
        }
        public string Code { get; }
        public string Name { get; }
        public int Order { get; set; }
    }
    record class MenuItemIntent
    {


        public MenuItemIntent(string moduleCode, string code, string name, byte[] icon, Type viewModelType, int order)
        {
            ModuleCode = moduleCode;
            Code = code;
            Name = name;
            Icon = icon;
            ViewModelType = viewModelType;
            Order = order;
        }

        public string Code { get; }
        public string Name { get; }
        public string ModuleCode { get; }
        public byte[] Icon { get; }
        public Type ViewModelType { get; }
        public int Order { get; }
    }
    record class MenuItemPlaceholder : IMenuBuildItem
    {
        public MenuItemPlaceholder(MenuItemIntent menu)
        {
            Menu = menu;
        }

        public MenuItemIntent Menu { get; }

        public IMenuBuildItem AddNav()
        {
            throw new NotImplementedException();
        }

        public IMenuBuildItem AttTools(string shortkey)
        {
            throw new NotImplementedException();
        }

        public IMenuBuildItem Configure(Action<IMenuBuildItem> configure)
        {
            throw new NotImplementedException();
        }

        public IMenuBuildItem RequirePermission(string permissionCode)
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class ModuleMenuPlaceholder : IModuleMenuBuild
    {
        private readonly string _moduleCode;
        private readonly AvaloniaComponentsOptions _collector;

        public ModuleMenuPlaceholder(string moduleCode, AvaloniaComponentsOptions collector)
        {
            _moduleCode = moduleCode;
            _collector = collector;
        }

        public IMenuBuildItem AddMenu(string code, string name, byte[] icon, Type viewModelType, int order = 0)
        {
            MenuItemIntent intent = new MenuItemIntent(_moduleCode, code, name, icon, viewModelType, order);
            _collector.ItemIntents.Add(intent);
            return new MenuItemPlaceholder(intent);
        }
    }
}
