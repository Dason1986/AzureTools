
using AzureTools.ViewModels;
using AzureTools.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AzureTools
{
    public sealed class AvaloniaComponentsOptions
    {
        private readonly ConcurrentDictionary<Type, Type> _mapping = new();

        private readonly ConcurrentDictionary<string, KeyedViewMapping> _keyedMapping = new();
        internal readonly List<ModuleMenuIntent> ModuleIntents = new();
        internal readonly List<MenuItemIntent> ItemIntents = new();
        internal AvaloniaComponentsOptions(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }



        public void MapView<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>()
            where TView : View
            where TViewModel : ViewModel
        {
            _mapping.TryAdd(typeof(TViewModel), typeof(TView));
        }

        public void KeyedMapView<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(
            string key,
            KeyedViewModelFactory viewModelFactory,
            KeyedViewFactory viewFactory)
            where TView : View
            where TViewModel : ViewModel
        {
            ArgumentNullException.ThrowIfNull(viewModelFactory);
            ArgumentNullException.ThrowIfNull(viewFactory);
            if (!_keyedMapping.TryAdd(key, new KeyedViewMapping(typeof(TViewModel), typeof(TView), viewModelFactory, viewFactory)))
                throw new InvalidOperationException($"已存在相同的视图键：{key}");
        }
        public IModuleMenuBuild AddModuleMenu(string code, string name, int order)
        {
            ModuleIntents.Add(new ModuleMenuIntent(code, name, order));
            return new ModuleMenuPlaceholder(code, this);
        }

        public IModuleMenuBuild GetModuleMenu(string code)
            => new ModuleMenuPlaceholder(code, this);
        internal IViewContainerManagement BuildManagement()
            => new DefaultViewContainerManagement(_mapping, _keyedMapping);
        internal IMenuManagement BuildMenus()
        {
            ModuleItem[] modules = ModuleIntents.Select(n => new ModuleItem() { Code = n.Code, Name = n.Name, Order = n.Order }).OrderByDescending(x => x.Order).ToArray();
            MenuItem[] menus = ItemIntents.Select(n => new MenuItem { Code = n.Code, Icon = n.Icon, Name = n.Name, Order = n.Order, ViewModelType = n.ViewModelType, Module = modules.FirstOrDefault(x => x.Code == n.ModuleCode) }).OrderByDescending(x => x.Order).ToArray();
            return new DefaultMenuManagement(menus);
        }
    }

}
