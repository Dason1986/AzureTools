using AzureTools.ViewModels;
using AzureTools.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace AzureTools
{
    public sealed class AvaloniaComponentsOptions
    {
        private readonly ConcurrentDictionary<Type, Type> _mapping = new();

        internal AvaloniaComponentsOptions(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public void AddView<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>()
            where TView : View
            where TViewModel : ViewModel
        {
            Services.AddScoped<TView>();
            Services.AddScoped<TViewModel>();
            _mapping.TryAdd(typeof(TViewModel), typeof(TView));
        }

        public void MapView<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>()
            where TView : View
            where TViewModel : ViewModel
        {
            _mapping.TryAdd(typeof(TViewModel), typeof(TView));
        }

        internal IViewContainerManagement BuildManagement()
            => new DefaultViewContainerManagement(_mapping);
    }
}
